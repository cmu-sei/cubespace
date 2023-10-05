using System;
using System.Collections.Generic;
using Managers;
using Mirror;
using Systems.GameBrain;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
	public class UIHudGalaxyPanelManager : MonoBehaviour
	{
		/// <summary>
		/// All systems displayed in the galaxy panel.
		/// </summary>
		/// public List<NavReaderGalaxySystem> systems;
		[Header("Prefabs")] [SerializeField] private GameObject systemPrefab;

		[SerializeField] private GameObject linePrefab;
		[SerializeField] private GameObject targetPointPrefab;

		/// <summary>
		/// The parent of the system objects.
		/// </summary>
		[SerializeField] private Transform systemParent;

		/// <summary>
		/// The parent of the target objects.
		/// </summary>
		[SerializeField] private Transform targetParent;

		/// <summary>
		/// The parent of the line objects.
		/// </summary>
		public Transform lineParent;

		/// <summary>
		/// The IDs of systems to system components.
		/// </summary>
		public Dictionary<string, NavReaderGalaxySystem> idsToSystems;

        private void Start()
        {
            // These should be disabled by their controllers but if there's no missions their controllers aren't instantiated and the tooltips stay open.
            // These calls will be redundant in other cases but will prevent that problem
            if (DisplayTooltip.Instance != null) DisplayTooltip.Instance.gameObject.SetActive(false);
            if (SolveCountTooltip.Instance != null) SolveCountTooltip.Instance.gameObject.SetActive(false);
            if (PointsTooltip.Instance != null) PointsTooltip.Instance.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            ShipStateManager.OnMissionDatasChange += AddSystemOrSetData;

            // Initialize galaxy map systems
            if (ShipStateManager.Instance && ShipStateManager.Instance.MissionDatas != null)
            {
                AddSystemOrSetData(ShipStateManager.Instance.MissionDatas);
            }
        }

        private void OnDisable()
        {
            ShipStateManager.OnMissionDatasChange -= AddSystemOrSetData;
        }

        /// <summary>
        /// Add systems for each mission to the dictionary and galaxy map and sets them up, or changes their attributes if they already exists.
        /// </summary>
        /// <param name="md">The incoming mission data.</param>
        /// <param name="index">The index of the mission in the mission log corresponding to this system.</param>
        public void AddSystemOrSetData(List<MissionData> mds)
        {
	        //lazy init because Start doesn't get called when object is inactive.
	        if (idsToSystems == null)
	        {
		        idsToSystems = new Dictionary<string, NavReaderGalaxySystem>();
	        }

            // TODO: Need to handle systems associated with missions not in mds (missions that were removed from the json)
            MissionData md;
            for (int index = 0; index < mds.Count; index++)
            {
                md = mds[index];

                // Update system attributes if it already exists
                if (idsToSystems.ContainsKey(md.missionID))
                {
                    idsToSystems[md.missionID].SetSystemMission(md, index);
                }
                // Set up the system if it doesn't
                else
                {
                    // Create a new system on the map from prefabs
                    GameObject systemObj = Instantiate(systemPrefab, systemParent);
                    GameObject lineObj = Instantiate(linePrefab, lineParent);
                    GameObject targetObj = Instantiate(targetPointPrefab, targetParent);

                    // Get the system script
                    NavReaderGalaxySystem system = systemObj.GetComponent<NavReaderGalaxySystem>();

                    // Get the image components
                    Image lineImage = lineObj.transform.GetComponent<Image>();
                    Image targetImage = targetObj.transform.GetChild(0).GetComponent<Image>();

                    // Add the system to the dictionary and set its mission information
                    idsToSystems.Add(md.missionID, system);
                    system.SetSystemMission(md, index, lineImage, targetImage);

                    // Set the position of the system
                    //Allowable x range: [-540, 540]. Allowable y range: [-320, 320]. Give each a circle with diameter 125 t0 avoid overlap
                    systemObj.GetComponent<RectTransform>().localPosition = new Vector2(md.galaxyMapXPos, md.galaxyMapYPos);
                    targetObj.GetComponent<RectTransform>().localPosition = new Vector2(md.galaxyMapTargetXPos, md.galaxyMapTargetYPos);

                    // Get TectTransform references
                    RectTransform lineRect = lineObj.GetComponent<RectTransform>();
                    RectTransform coreDisplayTransform = system.CoreDisplayRect;
                    RectTransform targetRectTransform = targetObj.GetComponent<RectTransform>();

                    // Get the positions of the RectTransforms
                    Vector2 coreDisplayPosition = coreDisplayTransform.position;
                    Vector2 targetPosition = targetRectTransform.position;
                    Vector2 coreDisplayLocalPosition = coreDisplayTransform.parent.localPosition - coreDisplayTransform.localPosition;
                    Vector2 targetLocalPosition = targetRectTransform.localPosition - targetRectTransform.parent.localPosition;

                    // Calculate the distance between the system
                    Vector2 midpoint = (coreDisplayPosition + targetPosition) / 2;
                    float distance = Vector2.Distance(coreDisplayLocalPosition, targetLocalPosition);

                    // Draw the line between the system and its target
                    lineRect.position = midpoint;
                    lineRect.sizeDelta = new Vector2(lineRect.sizeDelta.x, distance);
                    float z = 90 + Mathf.Atan2(targetPosition.y - coreDisplayPosition.y, targetPosition.x - coreDisplayPosition.x) * 180 / Mathf.PI;
                    lineRect.rotation = Quaternion.Euler(0, 0, z);
                }
            }
        }
	}
}
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

		/// <summary>
        /// Adds the system to the dictionary and galaxy map and sets it up, or changes its attributes if it already exists.
        /// </summary>
        /// <param name="md">The incoming mission data.</param>
        /// <param name="index">The index of the mission in the mission log corresponding to this system.</param>
        public void AddSystemOrSetData(MissionData md, int index)
        {
	        //lazy init because Start doesn't get called when object is inactive.
	        if (idsToSystems == null)
	        {
		        idsToSystems = new Dictionary<string, NavReaderGalaxySystem>();
	        }
	        
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
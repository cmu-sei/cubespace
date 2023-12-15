using System.Collections.Generic;
using System.Linq;
using Managers;
using Systems.GameBrain;
using UnityEngine;

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
		private Dictionary<string, NavReaderGalaxySystem> idsToSystems;

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

            // Check to see if any missions have been removed from the data since the last update
            if (idsToSystems.Count > 0)
            {
                List<string> prevSystemIDs = idsToSystems.Keys.ToList();

                foreach (string id in prevSystemIDs)
                {
                    if (!mds.Exists((o) => { return o.missionID == id; }))
                    {
                        idsToSystems[id].Delete();
                        idsToSystems.Remove(id);
                    }
                }
            }
            
            MissionData md = null;
            // Update and create new systems as necessary
            for (int index = 0; index < mds.Count; index++)
            {
                md = mds[index];

                // Update system attributes if it already exists
                if (idsToSystems.ContainsKey(md.missionID))
                {
                    idsToSystems[md.missionID].UpdateSystem(md, index);
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

                    // Add the system to the dictionary and set its mission information
                    idsToSystems.Add(md.missionID, system);
                    system.InitializeSystem(md, index, lineObj, targetObj);
                }
            }
        }
	}
}
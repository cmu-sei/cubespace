/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Managers;
using Mirror;
using Systems.GameBrain;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace UI.HUD
{
	/// <summary>
	/// The container used to display mission pogs in the UI.
	/// </summary>
	public class UIMissionPogContainer : NetworkBehaviour
	{
		/// <summary>
		/// Maps the missions to icons.
		/// </summary>
		[SerializeField]
		private IDToImageMap missionIconLookup;
		/// <summary>
		/// The prefab to use to represent a mission pog.
		/// </summary>
		[SerializeField]
		GameObject MissionPogPrefab;
		
		/// <summary>
		/// The parent object used for mission pogs.
		/// </summary>
		[SerializeField]
		private Transform layoutGroupParent;

		/// <summary>
		/// The maximum amount of icons to display. Actual amount will be count of the missions sent by game brain or this max, whichever is lower
		/// </summary>
		[SerializeField]
		private int maxPogCount = 10;
		/// <summary>
		/// The list of mission pogs.
		/// </summary>
		[SerializeField]
		private UIMissionPog[] pogs;
		/// <summary>
		/// The ID used for empty icon lookup.
		/// </summary>
		[SerializeField]
		private string emptyIconLookupID = "empty";
		/// <summary>
		/// The ID used for the completed icon lookup.
		/// </summary>
		[SerializeField]
		private string completedIconBgLookupID = "completedBg";
		/// <summary>
		/// Whether to display empty icons.
		/// </summary>
		private bool displayEmptyIcons = false;

		private bool initialized = false;
		private int currentPogCount = 0;
		private int previousMissionCount = 0;

		/// <summary>
		/// Starts coroutine that waits until session data comes in to initialize pogs
		/// </summary>
		private void Awake()
		{
			StartCoroutine(WaitToInitializePogs());
		}

		/// <summary>
		/// Subscribes to the mission data change event to update icons.
		/// </summary>
		private void OnEnable()
		{
			ShipStateManager.OnMissionDatasChange += UpdatePogs;
		}

		/// <summary>
		/// Unsubscribes from the mission data change event.
		/// </summary>
		private void OnDisable()
		{
			ShipStateManager.OnMissionDatasChange -= UpdatePogs;
		}

		/// <summary>
		/// Initializes pogs once session data comes in from game brain
		/// </summary>
		private IEnumerator WaitToInitializePogs()
        {
			// TODO: This line doesn't work here but does work elsewhere in the project? The while loop works just as well but it's weird anyways
			//yield return new WaitUntil(() => ShipStateManager.Instance != null && ShipStateManager.Instance.Session != null);

			while (ShipStateManager.Instance == null || ShipStateManager.Instance.Session == null || ShipStateManager.Instance.MissionDatas == null)
			{
				yield return null;
			}

			// If useCodices is false, disable pogs
			if (!ShipStateManager.Instance.Session.useCodices)
			{
				if ((CustomNetworkManager.singleton as CustomNetworkManager).isInDebugMode || (CustomNetworkManager.singleton as CustomNetworkManager).isInDevMode)
				{
                    Debug.Log("useCodices is false, disbaling mission pogs");
                }
				ClearChildren();
				this.enabled = false;
			}
            // else initialize pogs
            else
            {
                displayEmptyIcons = ShipStateManager.Instance.Session.displayIncompleteMissionPogs;
				ClearChildren();
				previousMissionCount = ShipStateManager.Instance.MissionDatas.Count;
                CreatePogs(ShipStateManager.Instance.MissionDatas.Count);
                initialized = true;
                UpdatePogs(ShipStateManager.Instance.MissionDatas);
            }
		}
		
		/// <summary>
		/// Updates the icons displayed for each mission based on the mission data received.
		/// </summary>
		/// <param name="missions">The list of data for missions.</param>
		private void UpdatePogs(List<MissionData> missions)
		{
			if (!initialized) return;
			else if (previousMissionCount != missions.Count)
			{
				previousMissionCount = missions.Count;
				ClearChildren();
				CreatePogs(missions.Count);
			}

			// Fill in pogs for completed missions
			int nextPogIndexToFill = 0;
			for (int i = 0; i < currentPogCount; i++)
			{
				if (missions[i].complete && missions[i].visible && !missions[i].isSpecial)
				{
					pogs[nextPogIndexToFill].gameObject.SetActive(true);
					pogs[nextPogIndexToFill].UpdatePog(missions[i].complete, missions[i].currentScore, missionIconLookup.GetImage(missions[i].missionIcon), missions[i].title, i);
                    nextPogIndexToFill++;
				}
			}

			// Deal with empty pogs
			int n = nextPogIndexToFill;
			while (n < currentPogCount)
            {
				if (displayEmptyIcons)
				{
					pogs[n].SetEmpty();
					pogs[n].gameObject.SetActive(true);
				}
				else
				{
					pogs[n].gameObject.SetActive(false);
				}
				n++;
			}
		}
		
		/// <summary>
		/// Creates icons for the mission pogs.
		/// </summary>
		private void CreatePogs(int pogCount)
		{
            if (pogCount > maxPogCount)
            {
                Debug.LogWarning("More missions sent by GameBrain then can be displayed by pogs but useCodices is true!!! Displaying max of: " + maxPogCount);
				pogCount = maxPogCount;
            }

            pogs = new UIMissionPog[pogCount];
			for (int i = 0; i < pogCount; i++)
			{
				var pig = Instantiate(MissionPogPrefab, layoutGroupParent);
				pogs[i] = pig.GetComponent<UIMissionPog>();
				pogs[i].emptyBgSprite= missionIconLookup.GetImage(emptyIconLookupID);
				pogs[i].completedBgSprite = missionIconLookup.GetImage(completedIconBgLookupID);
				pogs[i].gameObject.SetActive(false);
			}

            currentPogCount = pogCount;
        }

		/// <summary>
		/// Clears out all child objects.
		/// </summary>
		private void ClearChildren()
		{
			foreach (Transform child in layoutGroupParent)
			{
				Destroy(child.gameObject);
			}
		}
	}
}

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
		/// The maximum amount of icons to display.
		/// </summary>
		[SerializeField]
		private int pogCount = 5;
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
		[SerializeField]
		private bool displayEmptyIcons = true;

		/// <summary>
		/// Clears child mission objects and recreates the icons.
		/// </summary>
		private void Awake()
		{
			ClearChildren();
			CreateIcons();

			StartCoroutine(WaitToClearIcons()); // TODO: This doesn't work
		}

		/// <summary>
		/// Subscribes to the mission data change event to update icons.
		/// </summary>
		private void OnEnable()
		{
			ShipStateManager.OnMissionDataChange += UpdateIcons;
		}

		/// <summary>
		/// Unsubscribes from the mission data change event.
		/// </summary>
		private void OnDisable()
		{
			ShipStateManager.OnMissionDataChange -= UpdateIcons;
		}

		/// <summary>
		/// Clears mission pogs if needed.
		/// </summary>
		/// <returns>A yield return while waiting for the ShipStateManager to activate.</returns>
		private IEnumerator WaitToClearIcons()
        {
			// TODO: This doesn't actually work. It never gets past this WaitUntil
			yield return new WaitUntil(() => ShipStateManager.Instance && ShipStateManager.Instance.Session != null);

			// TODO: useCodices is currently not having any effect because this code never runs
			Debug.Log(">" + ShipStateManager.Instance.Session.useCodices);
			if (!ShipStateManager.Instance.Session.useCodices)
			{
				ClearChildren();
			}
		}
		
		/// <summary>
		/// Updates the icons displayed for each mission based on the mission data received.
		/// </summary>
		/// <param name="missions">The list of data for missions.</param>
		private void UpdateIcons(List<MissionData> missions)
		{
			// Fill in pogs for completed missions
			int nextPogIndexToFill = 0;
			for (int i = 0; i < missions.Count; i++)
			{
				if (missions[i].complete && missions[i].visible && !missions[i].isSpecial)
				{
					if (nextPogIndexToFill >= pogCount)
                    {
						Debug.LogError("Server sent more complete missions than there are pog icons to display them! Either change pogCount in the pogContainer or send less missions with complete == true!");
						return;
                    }
					pogs[nextPogIndexToFill].gameObject.SetActive(true);
					pogs[nextPogIndexToFill].UpdatePog(missions[i].complete, missions[i].currentScore, missionIconLookup.GetImage(missions[i].missionIcon), missions[i].title, i);
                    nextPogIndexToFill++;
				}
			}

			// Deal with empty pogs
			int n = nextPogIndexToFill;
			while (n < pogs.Length)
            {
				if (displayEmptyIcons)
				{
					pogs[n].SetEmpty();
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
		private void CreateIcons()
		{
			pogs = new UIMissionPog[pogCount];
			for (int i = 0; i < pogCount; i++)
			{
				var pig = Instantiate(MissionPogPrefab, layoutGroupParent);
				pogs[i] = pig.GetComponent<UIMissionPog>();
				pogs[i].emptyBgSprite= missionIconLookup.GetImage(emptyIconLookupID);
				pogs[i].completedBgSprite = missionIconLookup.GetImage(completedIconBgLookupID);
				pogs[i].gameObject.SetActive(false);
			}
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

/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using Systems.GameBrain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
	/// <summary>
	/// Mission details panel of the UI HUD.
	/// </summary>
	public class UIHudMissionDetailsPanel : MonoBehaviour
	{
		/// <summary>
		/// The title of the mission with these details.
		/// </summary>
		[Header("Config")]
		[SerializeField]
		private TMP_Text missionTitle;
		/// <summary>
		/// The background of this object.
		/// </summary>
		[SerializeField]
		private Image missionTitleBG;
		/// <summary>
		/// The description of the mission.
		/// </summary>
		[SerializeField]
		private TMP_Text missionDescription;
		/// <summary>
		/// The list of roles useful for this mission.
		/// </summary>
		[SerializeField]
		private UIHudMissionRoleList roleList;
		/// <summary>
		/// The list of tasks the players need to accomplish to complete this mission.
		/// </summary>
		[SerializeField]
		private UIHudTaskList taskList;
		
		/// <summary>
		/// Sets data for a mission.
		/// </summary>
		/// <param name="data">The data used to contruct a mission and how it appears.</param>
		public void SetMissionData(MissionData data)
		{
			if (data == null)
			{
				return;
			}
			
			missionTitle.text = data.title;
			missionDescription.text = data.summaryLong;

			if (data.isSpecial)
            {
				missionTitleBG.color = ColorPalettes.ColorPalette.GetColor(ColorPalettes.PaletteColor.UISpecialMissionSelected);
            }
            else
            {
				missionTitleBG.color = ColorPalettes.ColorPalette.GetColor(ColorPalettes.PaletteColor.UIAccentColorOne);
            }
		
			// This is called twice to force the layout group to update from layout element
			// These are preferred positions, which change at the "same" time (in wrong order, sometimes) so it doesn't update properly the first.
			roleList.UpdateRoleList(data.roleList);
			roleList.UpdateRoleList(data.roleList);

			taskList.SetTaskList(data.taskList);
		}
	}
}

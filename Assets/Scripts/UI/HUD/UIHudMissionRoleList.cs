/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
	/// <summary>
	/// The list of roles displayed in the UI. The roles themselbes are populated via UpdateRoleList.
	/// </summary>
	public class UIHudMissionRoleList : MonoBehaviour
	{
		/// <summary>
		/// The layout group where the roles are populated.
		/// </summary>
		[SerializeField]
		private LayoutGroup _roleLayoutGroup;
		/// <summary>
		/// The tags used for each role.
		/// </summary>
		public UIHudRoleTagItem[] RoleTags; // Should have 4 of these set in the editor

		//      "Cyber Defense Forensics Analyst"
		//      "Cyber Defense Incident Responder"
		//      "Exploitation Analyst"
		//      "Network Operations Specialist"
		//      "Threat/Warning Analyst"
		//      "Vulnerability Assessment Analyst"
		
		/// <summary>
		/// Updates the list of roles.
		/// </summary>
		/// <param name="roles">The list of roles.</param>
		public void UpdateRoleList(string[] roles)
		{
			if (roles == null)
			{
				// Hide all on null data
				roles = Array.Empty<string>();
			}

			for (int i = 0; i < RoleTags.Length; i++)
			{
				if (roles.Length > i && !String.IsNullOrWhiteSpace(roles[i]))
				{
					RoleTags[i].gameObject.SetActive(true);
					RoleTags[i].SetRole(roles[i]);
				}
				else
				{
					RoleTags[i].gameObject.SetActive(false);
				}
			}
			
			// Force a re-draw
			Canvas.ForceUpdateCanvases();
			_roleLayoutGroup.enabled = false;
			_roleLayoutGroup.enabled = true;
		}
	}
}

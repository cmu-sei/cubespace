/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
{
	/// <summary>
	/// The UI piece displayed for a tag.
	/// </summary>
	public class UIHudRoleTagItem : MonoBehaviour
	{
		/// <summary>
		/// The text used to display a tag.
		/// </summary>
		[SerializeField]
		private TMP_Text _text;
		/// <summary>
		/// The layout element used for this role tag.
		/// </summary>
		[SerializeField]
		private LayoutElement _layoutElement;
		/// <summary>
		/// The padding used between role tags.
		/// </summary>
		[SerializeField]
		private float padding = 15;

		/// <summary>
		/// Sets this role tag to use the text provided.
		/// </summary>
		/// <param name="text">Sets the role text to be the text provided.</param>
		public void SetRole(string text)
		{
			_text.text = text;
			_layoutElement.preferredWidth = _text.renderedWidth+padding;
		}

		/// <summary>
		/// Shows this role tag.
		/// </summary>
		public void Show()
		{
			gameObject.SetActive(true);
		}

		/// <summary>
		/// Hides this role tag.
		/// </summary>
		public void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}

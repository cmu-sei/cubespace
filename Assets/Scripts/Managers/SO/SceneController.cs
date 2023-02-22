/*
Cyber Defenders Video Game
Copyright 2023 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
This Software includes and/or makes use of Third-Party Software each subject to its own license.
DM23-0100
*/

using UnityEngine;

namespace Managers
{
	/// <summary>
	/// A ScriptableObject that holds a reference to all scenes that should be additively loaded on the client. This script is needed because
	/// we are unable to easily access the scenes specified in Unity's build settings when building the game.
	/// Scriptable Objects make for useful "managers", since they can be referenced anywhere. 
	/// A scene manager is a particularly good use-case for 'SO as Managers'. It doesn't care about anything in the scene, game events, order, etc.
	/// </summary>
	[CreateAssetMenu(fileName = "Scene Manager", menuName = "Game Data/Scene Manager", order = 0)]
	public class SceneController : ScriptableObject
	{
		// The different scenes available
		/*
		 * Adding, removing, or renaming one of these fields will require that the script SceneControllerEditor.cs
		 * be refactored with its fields appropriately modified.
		 * Note that these get assigned to the full path of the scene, not just its name.
		 * That prevents conflict, but if the scene is moved, its reference will likely need to be reassigned.
		 * */
		public string GameplayScene;
		public string WorkstationScene;
		public string HUDScene;

		/// <summary>
		/// Returns an array of all additional scenes used after entering the online scene.
		/// </summary>
		/// <returns>A list of all scenes that should be additively loaded on the server and client after the online scene has been loaded.</returns>
		public string[] GetAdditionalScenes()
		{
			return new string[] { GameplayScene, WorkstationScene, HUDScene };
		}
	}
}

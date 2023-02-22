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
using Mirror;

namespace Managers
{
	/// <summary>
	/// A general definition of an object following the Singleton pattern. This class is used by creating another script,
	/// making it be of type Singleton<MyScript>, and then just referencing Instance elsewhere.
	/// <para>
	/// The Singleton will create itself via a lazy load on first get, if it doesn't exist in the scene, or otherwise 
	/// self-instantiating as an empty object in a DontDestroyOnLoad scene.
	/// </para>
	/// Note that this singleton is inappropriate for scene-specific setup.
	/// 
	/// This specific implementation is from Hands-On Game Development Patterns with Unity 2019 by David Baron.
	/// </summary>
	public class NetworkedSingleton<T> : NetworkBehaviour where T : Component
	{
		// The privately stored reference to the single instance of this script
		private static T _instance;

		// The public reference to the single instance of this object
		public static T Instance
		{
			get
			{
				// If there's no instance stored yet, try to find a possible instance
				if (_instance == null)
				{
					_instance = FindObjectOfType<T>();
				}
				return _instance;
			}
		}

		/// <summary>
		/// Sets the singleton instance to be this script if there is none; destroys this GameObject if there is.
		/// </summary>
		public virtual void Awake()
		{
			if (_instance == null || _instance == this)
			{
				_instance = this as T;
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}


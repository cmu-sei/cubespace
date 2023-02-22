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
using Systems;

namespace Managers
{
	/// <summary>
	/// A class creating a central authority for objects of a specific type. The singleton instance creates itself automatically.
	/// The usage in implementation follows the syntax "public class GameManagerItem : Singleton<GameManagerItem>". Scripts referencing the singleton object should reference it via Instance.
	/// <para>
	/// Any new scripts using the singleton pattern should carefully consider whether it should be a singleton, because it could add race conditions.
	/// </para>
	/// This specific implementation is from Hands-On Game Development Patterns with Unity 2019 by David Baron.
	/// </summary>
	public class Singleton<T> : MonoBehaviour where T : Component
	{
		// The private instance of the singleton of this type
		private static T _instance;

		/// <summary>
		/// The public instance of the singleton of this type. Searches for an object of a type.
		/// </summary>
		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					// Check for other instances of this type
					_instance = FindObjectOfType<T>();
				}
				return _instance;
			}
		}

		/// <summary>
		/// Unity event function that marks this instance of the component as the singleton instance if possible, and destroys it otherwise.
		/// </summary>
		public virtual void Awake()
		{
			// If there is no existing singleton instance of this object, mark this as the singleton instance
			if (_instance == null)
			{
				_instance = this as T;
			}
			// Otherwise, destroy this component's GameObject
			else
			{
				Destroy(gameObject);
			}
		}

		/// <summary>
		/// Unity event function that marks this object as needing to not be destroyed when a new scene loads.
		/// <para>
		/// DontDestroyOnLoad is placed here because in Mirror, having a DontDestroyOnLoad in Awake causes issues in editor for objects that have network identites on them or their children.
		/// Full details can be found here: https://github.com/vis2k/Mirror/issues/1748
		/// </para>
		/// </summary>
		public virtual void Start()
		{
			if (_instance == this as T)
			{
				DontDestroyOnLoad(gameObject);
			}
		}
	}

	/// <summary>
	/// Singletons used in the main scene that are managed by the Mirror Network Manager. This includes items such as the CutsceneSystem and the HUDController.
	/// <para>
	/// Note that singletons used when not connected, such as the Audio Manager, are not ConnectedSingletons.
	/// </para>
	/// </summary>
	/// <typeparam name="T">The type of the ConnectedSingleton instance.</typeparam>
	public class ConnectedSingleton<T> : Singleton<T> where T : Component 
	{
		/// <summary>
		/// Unity event function that registers this object as one to be destroyed when the client disconnects.
		/// </summary>
		public override void Awake()
		{
			base.Awake();
			if (Instance == this as T)
			{
				Mirror.NetworkManager.singleton.GetComponent<CustomNetworkManager>().RegisterObjectToDestroyOnDisconnect(gameObject);
			}
		}
	}
}


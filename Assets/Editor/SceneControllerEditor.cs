using Managers;
using UnityEditor;

/// <summary>
/// Allows Scene assets to be used in specific Inspector fields within the SceneController ScriptableObject.
/// This code is copied from the Unity Manual: https://docs.unity3d.com/ScriptReference/SceneAsset.html
/// </summary>
[CustomEditor(typeof(SceneController))]
public class SceneControllerEditor : Editor
{
	/// <summary>
	/// Makes specified fields on SceneController ScriptableObjects use Scene objects in the Inspector 
	/// upon drawing the Inspector window.
	/// </summary>
	public override void OnInspectorGUI()
	{
		// The target of this Editor script is an object of type SceneController
		var picker = target as SceneController;
		// Get each scene as specified in the SceneController
		var oldWorkstationScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(picker.WorkstationScene);
		var oldGameplayScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(picker.GameplayScene);
		var oldHUDScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(picker.HUDScene);

		// Update the object
		serializedObject.Update();
		EditorGUI.BeginChangeCheck();

		// Modify the Inspector window rendered for the ScriptableObject
		var workstationScene = EditorGUILayout.ObjectField("Ship Workstations", oldWorkstationScene, typeof(SceneAsset), false) as SceneAsset;
		var gameplayScene = EditorGUILayout.ObjectField("Gameplay", oldGameplayScene, typeof(SceneAsset), false) as SceneAsset;
		var hudScene = EditorGUILayout.ObjectField("HUD", oldHUDScene, typeof(SceneAsset), false) as SceneAsset;

		// If the GUI state changed, render the scenes
		if (EditorGUI.EndChangeCheck())
		{
			// Workstation scene
			var newWorkstationPath = AssetDatabase.GetAssetPath(workstationScene);
			var scenePathProperty = serializedObject.FindProperty("WorkstationScene");
			scenePathProperty.stringValue = newWorkstationPath;

			// Main scene
			var newGameplayPath = AssetDatabase.GetAssetPath(gameplayScene);
			var gameplayPathProperty = serializedObject.FindProperty("GameplayScene");
			gameplayPathProperty.stringValue = newGameplayPath;

			// HUD scene
			var newHUDPath = AssetDatabase.GetAssetPath(hudScene);
			var hudPathProperty = serializedObject.FindProperty("HUDScene");
			hudPathProperty.stringValue = newHUDPath;
		}

		// Apply the properties
		serializedObject.ApplyModifiedProperties();
	}
}

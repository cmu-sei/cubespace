using UnityEditor;
using UnityEngine;

namespace Mirror
{
    /// <summary>
    /// Allows scenes to be linked together within different scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneAttribute))]
    public class SceneDrawer : PropertyDrawer
    {
        /// <summary>
        /// Method that draws a custom GUI for a given property - in this case a SceneAttribute, to convert a string representing a Scene into an actual Scene property.
        /// </summary>
        /// <param name="position">The rectangle used for the GUI drawing. Required, but unneeded for this method.</param>
        /// <param name="property">The specific property this GUI is being made for.</param>
        /// <param name="label">The label provided for this property. Required, but unneeded for this method.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // If the property is a string, we are able to convert it to a Scene
            if (property.propertyType == SerializedPropertyType.String)
            {
                // Load the Scene asset with the name provided
                SceneAsset sceneObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);
                if (sceneObject == null && !string.IsNullOrWhiteSpace(property.stringValue))
                {
                    // Try to load the scene from the build settings for legacy compatibility
                    sceneObject = GetBuildSettingsSceneObject(property.stringValue);
                }

                // If we couldn't find a Scene with the name given, return an error
                if (sceneObject == null && !string.IsNullOrWhiteSpace(property.stringValue))
                {
                    Debug.LogError($"Could not find scene {property.stringValue} in {property.propertyPath}; assign the proper scenes in your NetworkManager.");
                }

                // Create a GUI field accepting the Scene we found
                SceneAsset scene = (SceneAsset)EditorGUI.ObjectField(position, label, sceneObject, typeof(SceneAsset), true);
                property.stringValue = AssetDatabase.GetAssetPath(scene);
            }
            // If the given property isn't a string, we can't do anything, so warn the user they've made an error
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [Scene] with strings.");
            }
        }

        /// <summary>
        /// Locates the Scene with the name given among those within the Build Settings.
        /// </summary>
        /// <param name="sceneName">The name of the Scene we want to find.</param>
        /// <returns>The Scene with the given name, or null if no matching Scene can be found.</returns>
        protected SceneAsset GetBuildSettingsSceneObject(string sceneName)
        {
            // Loop through all scenes in the build settings and return the one matching
            foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);
                if (sceneAsset != null && sceneAsset.name == sceneName)
                {
                    return sceneAsset;
                }
            }
            return null;
        }
    }
}

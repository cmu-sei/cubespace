using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Allows fast scene switching in the editor. Accessible by selecting the three dots at the top right of the Scene view, then selecting Overlays > Scene Switcher Creator Overlay.
/// </summary>
public static class EditorSceneSwitcher
{
    // Boolean the developer can toggle if they want to load play mode immediately after opening a scene
    public static bool AutoEnterPlaymode = false;
    // A series of Scenes within the Assets folder
    public static readonly List<string> ScenePaths = new();

    /// <summary>
    /// Opens a scene from the Editor.
    /// </summary>
    /// <param name="scenePath">The path of the Scene within the Assets folder that we want to open.</param>
    public static void OpenScene(string scenePath)
    {
        // Prompt the developer to save the current scene and then switch to the new one
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        // Enter play mode if the developer has selected so
        if (AutoEnterPlaymode) EditorApplication.EnterPlaymode();
    }

    /// <summary>
    /// Loads all scenes within the Assets folder.
    /// </summary>
    public static void LoadScenes()
    {
        // Clear scenes 
        ScenePaths.Clear();

        // Find all scenes in the Assets folder
        var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] {"Assets"});

        // Pre-emptively load all possible scenes we could switch to
        foreach (var sceneGuid in sceneGuids)
        {
            var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
            var sceneAsset = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset));
            ScenePaths.Add(scenePath);
        }
    }
}

/// <summary>
/// The actual overlay in the toolbar for switching scenes.
/// </summary>
[Icon("d_SceneAsset Icon")]
[Overlay(typeof(SceneView), OverlayID, "Scene Switcher Creator Overlay")]
public class SceneSwitcherToolbarOverlay : ToolbarOverlay
{
    // The identifier for this overlay
    public const string OverlayID = "scene-switcher-overlay";

    /// <summary>
    /// The constructor for this overlay; no logic is needed for it.
    /// </summary>
    private SceneSwitcherToolbarOverlay() : base(
        SceneDropdown.ID,
        // The reload button is largely unnecessary, so it's commented out here, but uncommenting this line will reimplement the reload button
        // ReloadButton.ID,
        AutoEnterPlayModeToggle.ID
    )
    {
    }

    /// <summary>
    /// The method invoked when this overlay is first created.
    /// </summary>
    public override void OnCreated()
    {
        // Load scenes and subscribe to a callback that automatically fires when the project has been modified
        base.OnCreated();
        EditorSceneSwitcher.LoadScenes();
        EditorApplication.projectChanged += OnProjectChanged;
    }

    /// <summary>
    /// The method invoked when the project has been modified.
    /// </summary>
    private void OnProjectChanged()
    {
        // Reload the scenes whenever the project has changed
        EditorSceneSwitcher.LoadScenes();
    }
}

/// <summary>
/// A simple dropdown on the overlay that presents a list of scenes that can be switched to.
/// </summary>
[EditorToolbarElement(ID, typeof(SceneView))]
public class SceneDropdown : EditorToolbarDropdown
{
    // The identifier of this overlay addition
    public const string ID = SceneSwitcherToolbarOverlay.OverlayID + "/scene-dropdown";
    // The tooltip displayed on hover
    private const string Tooltip = "Switch scene.";

    /// <summary>
    /// The constructor for drawing the Scene selection dropdown menu.
    /// </summary>
    public SceneDropdown()
    {
        // Draw the dropdown with a scene icon
        var content = EditorGUIUtility.TrTextContentWithIcon(SceneManager.GetActiveScene().name, Tooltip, "d_SceneAsset Icon");

        // Set basic styling properties
        text = content.text;
        tooltip = content.tooltip;
        icon = content.image as Texture2D;
        ElementAt(1).style.paddingLeft = 5;
        ElementAt(1).style.paddingRight = 5;

        // Add a callback to display or hide the dropdown when it is clicked
        clicked += ToggleDropdown;

        // Keep track of Play mode state changes
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
        // Keep track of when the project has been modified
        EditorApplication.projectChanged += OnProjectChanged;
    }

    /// <summary>
    /// Callback method to fire when the Unity project is modified.
    /// </summary>
    private void OnProjectChanged()
    {
        // Update the dropdown label in case that the scene was renamed
        text = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// A method called when the project switches from Edit mode to Play mode or vice versa.
    /// </summary>
    /// <param name="stateChange">An enum representing if the Editor's Play mode was entered or exited.</param>
    private void PlayModeStateChanged(PlayModeStateChange stateChange)
    {
        switch (stateChange)
        {
            case PlayModeStateChange.EnteredEditMode:
                // Allow switchign scenes while in Edit mode
                SetEnabled(true);
                break;
            case PlayModeStateChange.EnteredPlayMode:
                // Don't allow switching scenes while in Play mode
                SetEnabled(false);
                break;
        }
    }

    /// <summary>
    /// Method that displays or hides a dropdown menu for selecting a Scene.
    /// </summary>
    private void ToggleDropdown()
    {
        // Create a general menu
        var menu = new GenericMenu();
        // Loop through the existing list of scenes in the Assets folder
        foreach (var scenePath in EditorSceneSwitcher.ScenePaths)
        {
            // Get the scene name from the provided path
            var sceneName = Path.GetFileNameWithoutExtension(scenePath);
            // Display the scene name on the menu, and add a function to call when the scene name is selected
            menu.AddItem(new GUIContent(sceneName), text == sceneName,
                () => OnDropdownItemSelected(sceneName, scenePath));
        }

        // Activate a dropdown
        menu.DropDown(worldBound);
    }

    /// <summary>
    /// Method called when a Scene within the dropdown is selected.
    /// </summary>
    /// <param name="sceneName">The scene name selected from the dropdown.</param>
    /// <param name="scenePath">The path of the Scene.</param>
    private void OnDropdownItemSelected(string sceneName, string scenePath)
    {
        // Set the selected Scene and open it
        text = sceneName;
        EditorSceneSwitcher.OpenScene(scenePath);
    }
}

/// <summary>
/// A simple button that reloads the scnees displayed within the dropdown menu. Not used presently.
/// </summary>
[EditorToolbarElement(ID, typeof(SceneView))]
public class ReloadButton : EditorToolbarButton
{
    // The identifier for this button
    public const string ID = SceneSwitcherToolbarOverlay.OverlayID + "/reload-button";
    // The tooltip displayed on hover
    private const string Tooltip = "Reload scenes.";

    /// <summary>
    /// Basic button that reloads all scenes displayed in the dropdown.
    /// </summary>
    public ReloadButton()
    {
        // Create a new button with a refresh icon
        var content = EditorGUIUtility.TrTextContentWithIcon("", Tooltip, "d_Refresh");

        // Basic styling
        text = content.text;
        tooltip = content.tooltip;
        icon = content.image as Texture2D;

        // Add a callback to reload the dropdown menu display when this button is selected
        clicked += OnClicked;
    }

    /// <summary>
    /// Reloads the scenes in the dropdown menu when selected.
    /// </summary>
    void OnClicked()
    {
        EditorSceneSwitcher.LoadScenes();
    }
}

/// <summary>
/// A simple button that toggles automatically starting Play mode on a scene change.
/// </summary>
[EditorToolbarElement(ID, typeof(SceneView))]
public class AutoEnterPlayModeToggle : EditorToolbarToggle
{
    // The identifier for this button
    public const string ID = SceneSwitcherToolbarOverlay.OverlayID + "/auto-enter-playmode-toggle";
    // The tooltip displayed on hover
    private const string Tooltip = "Auto enter playmode.";

    /// <summary>
    /// Basic button that toggles whether to immediately start playing a Scene after selecting it from the dropdown.
    /// </summary>
    public AutoEnterPlayModeToggle()
    {
        // Create a simple button with a Play icon
        var content = EditorGUIUtility.TrTextContentWithIcon("", Tooltip, "d_PlayButton On");

        // Basic styling
        text = content.text;
        tooltip = content.tooltip;
        icon = content.image as Texture2D;

        // The existing value is whether we're already set to automatically enter Play mode or not
        value = EditorSceneSwitcher.AutoEnterPlaymode;

        // When the value above changes, toggle the Play mode selection
        this.RegisterValueChangedCallback(Toggle);
    }

    /// <summary>
    /// Switches whether to automatically enter Play mode when changing from one Scene to another.
    /// </summary>
    /// <param name="evt">An event with a boolean representing whether to enter Play mode when switching Scenes or not.</param>
    void Toggle(ChangeEvent<bool> evt)
    {
        EditorSceneSwitcher.AutoEnterPlaymode = evt.newValue;
    }
}
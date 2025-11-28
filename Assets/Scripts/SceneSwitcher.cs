using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button clothSceneButton;
    [SerializeField] private Button jellySceneButton;

    [Header("Scene Names")]
    [SerializeField] private string clothSceneName = "ClothScene";
    [SerializeField] private string jellySceneName = "JellyScene";

    private void Start()
    {
        Debug.Log($"SceneSwitcher: Starting in scene '{SceneManager.GetActiveScene().name}'");

        // Set up button listeners
        if (clothSceneButton != null)
        {
            clothSceneButton.onClick.AddListener(() => LoadScene(clothSceneName));
            Debug.Log("SceneSwitcher: Cloth button listener added");
        }
        else
        {
            Debug.LogWarning("SceneSwitcher: Cloth Scene Button is not assigned!");
        }

        if (jellySceneButton != null)
        {
            jellySceneButton.onClick.AddListener(() => LoadScene(jellySceneName));
            Debug.Log("SceneSwitcher: Jelly button listener added");
        }
        else
        {
            Debug.LogWarning("SceneSwitcher: Jelly Scene Button is not assigned!");
        }

        // Disable the button for the current scene
        UpdateButtonStates();
    }

    private void LoadScene(string sceneName)
    {
        Debug.Log($"SceneSwitcher: Attempting to load scene '{sceneName}'");

        // Check if scene exists in build settings
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"SceneSwitcher: Scene '{sceneName}' not found in Build Settings! Please add it via File > Build Settings");
        }
    }

    private void UpdateButtonStates()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log($"SceneSwitcher: Current scene is '{currentScene}'");

        if (clothSceneButton != null)
        {
            bool isInteractable = (currentScene != clothSceneName);
            clothSceneButton.interactable = isInteractable;
            Debug.Log($"SceneSwitcher: Cloth button interactable = {isInteractable}");
        }

        if (jellySceneButton != null)
        {
            bool isInteractable = (currentScene != jellySceneName);
            jellySceneButton.interactable = isInteractable;
            Debug.Log($"SceneSwitcher: Jelly button interactable = {isInteractable}");
        }
    }
}

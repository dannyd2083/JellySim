using UnityEngine;

public class InputManager : MonoBehaviour
{
    public enum ControlMode
    {
        Camera,      
        WindSource   
    }

    [Header("References")]
    public OrbitCamera orbitCamera;
    public WindSource windSource;
    public LayerMask windSourceLayer;  

    [Header("Current State")]
    public ControlMode currentMode = ControlMode.Camera;

    [Header("Visual Feedback")]
    public Color selectedColor = Color.yellow;
    public Color deselectedColor = Color.white;
    private Renderer windSourceRenderer;

    void Start()
    {
        // Get WindSource visual renderer
        if (windSource != null)
        {
            windSourceRenderer = windSource.GetComponentInChildren<Renderer>();
        }

        SetMode(ControlMode.Camera);  // Start with camera control
    }

    void Update()
    {
        HandleModeSwitch();
    }

    void HandleModeSwitch()
    {
        // Left mouse click to select
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if clicked on WindSource
            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.transform.IsChildOf(windSource.transform) || hit.transform == windSource.transform)
                {
                    SetMode(ControlMode.WindSource);
                    Debug.Log("Mode: WindSource Control");
                    return;
                }
            }

            // Clicked on nothing or ground -> Camera control
            SetMode(ControlMode.Camera);
            Debug.Log("Mode: Camera Control");
        }
    }

    void SetMode(ControlMode mode)
    {
        currentMode = mode;

        if (orbitCamera != null)
        {
            orbitCamera.allowMovement = (mode == ControlMode.Camera);
        }

        if (windSource != null)
        {
            windSource.allowControl = (mode == ControlMode.WindSource);
        }
        UpdateVisualFeedback();
    }

    void UpdateVisualFeedback()
    {
        if (windSourceRenderer != null)
        {
            if (currentMode == ControlMode.WindSource)
            {
                windSourceRenderer.material.color = selectedColor;
            }
            else
            {
                windSourceRenderer.material.color = deselectedColor;
            }
        }
    }
}
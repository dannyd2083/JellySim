using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = Vector3.zero;

    [Header("Orbit Settings")]
    public float distance = 10f;
    public float minDistance = 2f;
    public float maxDistance = 20f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;

    [Header("Pan Settings")]
    public float panSpeed = 0.5f;
    public float keyboardPanSpeed = 5f; 

    [Header("Smoothing")]
    public float smoothSpeed = 10f;

    [Header("Control")]
    public bool allowMovement = true;  

    // Internal
    private float currentX = 0f;
    private float currentY = 20f;
    private Vector3 currentTargetPosition;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("OrbitCamera: No target assigned!");
            return;
        }

        currentTargetPosition = target.position + targetOffset;

        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        if (!allowMovement) return;  

        // Right mouse button - Rotate camera
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            currentX += mouseX * rotationSpeed;
            currentY -= mouseY * rotationSpeed;

            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Middle mouse button - Pan camera
        if (Input.GetMouseButton(2))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 right = transform.right * -mouseX * panSpeed;
            Vector3 up = transform.up * -mouseY * panSpeed;

            targetOffset += right + up;
        }

        // ← WASD keyboard pan 
        float horizontal = Input.GetAxis("Horizontal");  // A/D
        float vertical = Input.GetAxis("Vertical");      // W/S

        if (Mathf.Abs(horizontal) > 0.01f || Mathf.Abs(vertical) > 0.01f)
        {
            Vector3 right = transform.right * horizontal * keyboardPanSpeed * Time.deltaTime;
            Vector3 forward = transform.forward * vertical * keyboardPanSpeed * Time.deltaTime;
            forward.y = 0;  // Keep horizontal

            targetOffset += right + forward;
        }

        // Q/E - vertical pan 
        if (Input.GetKey(KeyCode.Q))
        {
            targetOffset += Vector3.down * keyboardPanSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            targetOffset += Vector3.up * keyboardPanSpeed * Time.deltaTime;
        }

        // Mouse scroll Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void UpdateCameraPosition()
    {
        Vector3 targetPos = target.position + targetOffset;
        currentTargetPosition = Vector3.Lerp(currentTargetPosition, targetPos, Time.deltaTime * smoothSpeed);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = rotation * Vector3.back;
        Vector3 desiredPosition = currentTargetPosition + direction * distance;

        transform.position = desiredPosition;
        transform.LookAt(currentTargetPosition);
    }

    public void ResetCamera()
    {
        currentX = 0f;
        currentY = 20f;
        distance = 10f;
        targetOffset = Vector3.zero;
    }
}
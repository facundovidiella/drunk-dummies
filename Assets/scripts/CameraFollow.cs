using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    public Transform target;             // CameraTarget
    public Transform player;             // Player transform for rotation offset

    [Header("Settings")]
    public float distance = 5f;          // Distance from player
    public float height = 2f;            // Height offset
    public float sensitivity = 2f;       // Mouse/Stick sensitivity
    public float rotationSmoothSpeed = 10f; 
    public float followSmoothTime = 0.05f;

    [Header("Clamp Pitch")]
    public float minPitch = -30f;
    public float maxPitch = 60f;

    private float yaw;
    private float pitch;
    private Vector3 currentVelocity;

    // Input System
    private PlayerInput playerInput;
    private InputAction lookAction;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Find PlayerInput
        playerInput = FindObjectOfType<PlayerInput>();
        if (!playerInput)
        {
            Debug.LogError("PlayerInput not found.");
            return;
        }

        lookAction = playerInput.actions["Look"];
        if (lookAction == null)
            Debug.LogError("Look action not found.");
    }

    void LateUpdate()
    {
        if (!target || lookAction == null) return;

        // ----- Mouse / Stick input -----
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        yaw += lookInput.x * sensitivity;
        pitch -= lookInput.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // ----- Camera rotation -----
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        // ----- Desired position -----
        Vector3 desiredPosition = target.position - rotation * Vector3.forward * distance + Vector3.up * height;

        // ----- Smooth follow using SmoothDamp -----
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSmoothTime);

        // ----- Look at player target -----
        transform.LookAt(target.position + Vector3.up * 0.5f);

    }
}

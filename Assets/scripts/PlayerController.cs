using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float rotationSpeed = 10f;

    [Header("Jumping")]
    public float jumpForce = 6f;
    public LayerMask groundMask; // suelo

    [Header("Animator")]
    public Animator animator;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isWalking;
    private bool isSprinting;
    private bool isJumping;
    private bool isGrounded;
    private Camera mainCam;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;

        var playerInput = GetComponent<PlayerInput>();
        var sprintAction = playerInput.actions["Sprint"];

        sprintAction.started += ctx => { isSprinting = true;};
        sprintAction.canceled += ctx => { isSprinting = false;};

    }

    // ---------------- INPUT SYSTEM ----------------
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        isWalking = moveInput.magnitude > 0.1f;
    }

    public void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            isJumping = true;
            isSprinting = false;
        }
    }

    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            animator.SetTrigger("attack");
        }
    }

    void Update()
    {
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isJumping", !isGrounded);
        animator.SetBool("isSprinting", isSprinting);
    }

    // ---------------- PHYSICS ----------------
    void FixedUpdate()
    {
        GroundCheck();

        // Direccion relativa a camara
        Vector3 forward = mainCam.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = mainCam.transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;

        // Velocidad actual
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;

        // Movimiento con Rigidbody
        Vector3 targetPosition = rb.position + moveDir * targetSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);

        // Rotación suave hacia la dirección de movimiento
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }

        // Salto
        if (isJumping)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = false;
        }
    }

    void GroundCheck()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        isGrounded = Physics.Raycast(ray, 0.3f, groundMask);
    }
}

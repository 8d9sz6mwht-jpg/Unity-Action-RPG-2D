using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float sprintDuration = 5f;
    [SerializeField] private float sprintCooldown = 3f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airDrag = 2f;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Vector2 moveInput = Vector2.zero;
    private Vector2 currentVelocity = Vector2.zero;
    private Vector2 lastDirection = Vector2.right;

    private bool isGrounded = false;
    private bool isMoving = false;
    private bool isSprinting = false;
    private bool canSprint = true;
    private bool canDash = true;

    private float sprintTimer = 0f;
    private float sprintCooldownTimer = 0f;
    private float dashCooldownTimer = 0f;
    private float dashTimer = 0f;
    private bool isDashing = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (rb == null) Debug.LogError("Rigidbody2D not found!");
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer not found!");
    }

    private void Update()
    {
        GetInput();
        HandleFlip();
        HandleSprint();
        HandleDash();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ApplyMovement();
        ApplyDrag();
    }

    private void GetInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        moveInput = new Vector2(horizontal, vertical).normalized;
        isMoving = moveInput.magnitude > 0;

        if (Input.GetKeyDown(KeyCode.LeftShift) && canSprint)
        {
            isSprinting = true;
            sprintTimer = sprintDuration;
            canSprint = false;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isSprinting = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && canDash && isGrounded)
        {
            StartDash();
        }
    }

    private void HandleFlip()
    {
        if (moveInput.x > 0)
        {
            lastDirection = Vector2.right;
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < 0)
        {
            lastDirection = Vector2.left;
            spriteRenderer.flipX = true;
        }
    }

    private void HandleSprint()
    {
        if (isSprinting && isMoving)
        {
            sprintTimer -= Time.deltaTime;
            if (sprintTimer <= 0)
            {
                isSprinting = false;
                sprintCooldownTimer = sprintCooldown;
            }
        }

        if (!canSprint)
        {
            sprintCooldownTimer -= Time.deltaTime;
            if (sprintCooldownTimer <= 0)
            {
                canSprint = true;
                sprintCooldownTimer = 0;
            }
        }
    }

    private void HandleDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
        }

        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0)
            {
                canDash = true;
            }
        }
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        canDash = false;
        dashCooldownTimer = dashCooldown;
        
        if (animator != null)
            animator.SetTrigger("Dash");
    }

    private void ApplyMovement()
    {
        if (isDashing)
        {
            currentVelocity = lastDirection * dashSpeed;
        }
        else if (isMoving)
        {
            float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;
            currentVelocity = Vector2.Lerp(currentVelocity, moveInput * targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        rb.velocity = currentVelocity;
    }

    private void ApplyDrag()
    {
        rb.gravityScale = isGrounded ? 0 : 1f;
    }

    private void CheckGrounded()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, groundCheckRadius, groundLayer);
        isGrounded = colliders.Length > 0;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", currentVelocity.magnitude);
        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsSprinting", isSprinting && isMoving);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsDashing", isDashing);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
    }

    public bool IsGrounded => isGrounded;
    public bool IsMoving => isMoving;
    public bool IsSprinting => isSprinting;
    public Vector2 GetCurrentVelocity => currentVelocity;
    public Vector2 GetLastDirection => lastDirection;
}

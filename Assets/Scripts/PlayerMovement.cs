using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Basic")]
    [SerializeField] private float speed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpPower = 10f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private bool isJumping;
    private bool jumpCooldown;

    [Header("Dash")] 
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;

    //var gang
    private Vector2 moveInput;

    //comps gang
    private Rigidbody2D rb;
    private BoxCollider2D feetCol;
    private TrailRenderer tr;
    private Animator animator;
    private SpriteRenderer sr;
    
    //bools gang
    private bool isGrounded;
    private bool canDash = true;
    private bool isDashing;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        feetCol = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (isDashing) { return; }
        Run();
        Flip();
        Grounded();
        PlayerAnimation();
        JumpSetup();
    }

    #region Basic

    public void OnMove(InputAction.CallbackContext value)
    {
        moveInput = value.ReadValue<Vector2>();
    }
    private void Run()
    {
        Vector2 playerVelocity = new Vector2(moveInput.x * speed, rb.velocity.y);
        rb.velocity = playerVelocity;
    }

    #endregion

    #region Jump
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isJumping = true;
            jumpBufferCounter = jumpBufferTime;
        }

        if (context.canceled)
        {
            isJumping = false;

            if (rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
                coyoteTimeCounter = 0f;
            }
        }
    }
    public void JumpSetup()
    {
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f && !jumpCooldown)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            jumpBufferCounter = 0f;

            StartCoroutine(JumpCooldown());

            if (!isJumping)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    private IEnumerator JumpCooldown()
    {
        jumpCooldown = true;
        yield return new WaitForSeconds(0.4f);
        jumpCooldown = false;
    }

    #endregion

    #region Dash

    public void OnDash()
    {
        if(canDash)
        {
            StartCoroutine(Dash());
        }
    }
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        //tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        //tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    #endregion

    #region Collider

    private void Grounded()
    {
        if (feetCol.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    #endregion

    #region Animation

    private void PlayerAnimation()
    {
        bool runAnimation = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon && isGrounded;
        animator.SetBool("isRunning", runAnimation);
        animator.SetBool("isJumping", !isGrounded);
        animator.SetFloat("yVelocity", rb.velocity.y);
    }

    #endregion

    #region Extra

    private void Flip()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
        }
    }

    #endregion
}


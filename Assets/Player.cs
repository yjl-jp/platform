using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    private bool canDoubleJump;

    [Header("Buffer & coyate jump")]
    [SerializeField] private float bufferJumpWindow = .25f;
    private float bufferJumpActived=-1;
    [SerializeField] private float coyoteJumpWindow = .5f;
    private float coyoteJumpActivated = -1;


    [Header("Wall interactions")]
    [SerializeField] private float wallJumpDuration = .6f;
    [SerializeField] private Vector2 wallJumpForce;
    private bool isWallJumping;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 1;
    [SerializeField] private Vector2 knockbackPower;
    private bool isKnocked;
   


    [Header("Collision")]
    [SerializeField] private float groundCheckDistnace;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;
    private bool isAirborne;
    private bool isWallDetected;

    private float xInput;
    private float yInput;
    private bool facingRight = true;
    private int facingDir = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
       
        UpdateAirbornStatus();
        if (isKnocked)
        {
            return;
        }
        HandleCollision();
        HandleInput();
        HandleWallSlide();
       
        HandleMovement();
        HandleFlip();

        HandleAnimations();
       

    }
    public void Knockback()
    {
        if (isKnocked)
        {
            return;
        }
        StartCoroutine(KnockbackRoutine());
        anim.SetTrigger("knockback");
        rb.linearVelocity = new Vector2(knockbackPower.x * -facingDir, knockbackPower.y);
    }
    private IEnumerator KnockbackRoutine()
    {

        isKnocked = true;
        yield return new WaitForSeconds(knockbackDuration);

        isKnocked = false;
    }



    private void UpdateAirbornStatus()
    {
        if (isGrounded && isAirborne)
        {
            HandleLanding();
        }
        if (!isGrounded && !isAirborne)
        {
            BecomeAirborne();
        }
    }

    private void BecomeAirborne()
    {
        isAirborne = true;
        if (rb.linearVelocityY<0)
        {
            ActivateCoyoteJump();
        }
       
    }

    private void HandleLanding()
    {
        isAirborne = false;
        canDoubleJump = true;
        AttemptBufferJump();
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpButton();
            RequestBufferJump();
        }
    }
    #region Buffer & Coyote Jump
    private void RequestBufferJump()
    {
        if (isAirborne)
        {
            bufferJumpActived = Time.time;
        }
    }
    private void AttemptBufferJump()
    {
        if (Time.time < bufferJumpActived + bufferJumpWindow)
        {
            bufferJumpActived = Time.time - 1;
            Jump();
        }
    }
    private void ActivateCoyoteJump()=> coyoteJumpActivated = Time.time;
    private void CancelCoyoteJump() => coyoteJumpActivated = Time.time - 1;
    #endregion
    private void JumpButton()
    {
        bool coyoteJumpAvalible=Time.time < coyoteJumpActivated + coyoteJumpWindow;
        if (isGrounded || coyoteJumpAvalible)
        {
            if (coyoteJumpAvalible)
            {
                Debug.Log("i ve used coyote");
            }
            Jump();
        }else if (isWallDetected && !isGrounded)
        {
            WallJump();
        }
        else if (canDoubleJump)
        {
            DoubleJump();
        }
        CancelCoyoteJump();
    }

    private void Jump() => rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);

    private void DoubleJump()
    {
        isWallJumping = false;
        canDoubleJump = false;
        rb.linearVelocity = new Vector2(rb.linearVelocityX, doubleJumpForce);
    }
    private void WallJump()
    {
        canDoubleJump = true;
        rb.linearVelocity = new Vector2(wallJumpForce.x*-facingDir,wallJumpForce.y);
        Flip();
        StopAllCoroutines();
        StartCoroutine(WallJumpRoutine());
    }
    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(wallJumpDuration);
        isWallJumping = false;
    }
    private void HandleWallSlide()
    {
        bool canWallSlide = isWallDetected && rb.linearVelocityY < 0;
        float yModifer = yInput < 0 ? 1f : .05f;
        if (canWallSlide == false)
        {
            return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * yModifer);

    }

   
    private void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistnace, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    private void HandleAnimations()
    {

        anim.SetFloat("xVelocity", rb.linearVelocityX);
        anim.SetFloat("yVelocity", rb.linearVelocityY);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallDetected", isWallDetected);

    }

    private void HandleMovement()
    {
        if (isWallDetected)
        {
            return;
        }
        if (isWallJumping)
        {
            return;
        }
        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
    }
    private void HandleFlip()
    {
        if (xInput < 0 && facingRight || xInput > 0 && !facingRight)
        {
            Flip();

        }
    }
    private void Flip()
    {
        facingDir = facingDir * -1;
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistnace));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDir), transform.position.y));
    }
}
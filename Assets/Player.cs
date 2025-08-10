using System;
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

    [Header("Collision info")]
    [SerializeField] private float groundCheckDistnace;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;
    private bool isAirborne; 

    private float xInput;
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

        HandleCollision();
        HandleInput();
        HandleMovement();
        HandleAnimations();
        HandleFlip();

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
    }

    private void HandleLanding()
    {
        isAirborne = false;
        canDoubleJump = true;
    }

    private void HandleInput()
    {
        xInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpButton();
        }
    }
    private void JumpButton()
    {
        if (isGrounded)
        {
            Jump();
        }else if (canDoubleJump)
        {
            DoubleJump();
        }
    }

    private void Jump() => rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);

    private void DoubleJump()
    {
        canDoubleJump = false;
        rb.linearVelocity = new Vector2(rb.linearVelocityX, doubleJumpForce);
    }
    private void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistnace, whatIsGround);
    }

    private void HandleAnimations()
    {

        anim.SetFloat("xVelocity", rb.linearVelocityX);
        anim.SetFloat("yVelocity", rb.linearVelocityY);
        anim.SetBool("isGrounded", isGrounded);
    }

    private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
    }
    private void HandleFlip()
    {
        if (rb.linearVelocityX < 0 && facingRight || rb.linearVelocityX >0 && !facingRight)
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
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x,transform.position.y - groundCheckDistnace));
    }
}

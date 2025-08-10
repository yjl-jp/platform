using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;

    [Header("Collision info")]
    [SerializeField] private float groundCheckDistnace;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;
    
    private float xInput;
   
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }
    private void Update()
    {

        HandleCollision();
        HandleInput();
        HandleMovement();
        HandleAnimations();
       
    }

    private void HandleInput()
    {
        xInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
    }

   
    private void Jump() => rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);

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
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x,transform.position.y - groundCheckDistnace));
    }
}

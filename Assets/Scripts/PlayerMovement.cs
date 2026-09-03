using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMovement : MonoBehaviour
{//player movement speed
  public float moveSpeed=5f;
  //force behind the jump
  public float jumpForce =5f;
  public Transform groundCheck;
  public float groundDistance;
  public LayerMask groundMask;
  private Rigidbody Rb;
  private Vector2 moveInput;
  private bool isGrounded;
  private PlayerInput playerInput;

    void Start()
    {
        Rb= GetComponent<Rigidbody>();
        playerInput = new PlayerInput();


    }

  
    void Update()
    {
        CheckGround();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
    void OnJump()
    {  
         if(isGrounded)
        {
           Rb.AddForce(new Vector3(0,jumpForce,0),ForceMode.Impulse ); 
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position,groundDistance,groundMask);
    }

    void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void MovePlayer()
    {
        Vector3 direction= transform.right * moveInput.x + transform.forward *moveInput.y;
        direction.Normalize();
        Rb.linearVelocity = new Vector3(direction.x * moveSpeed,Rb.linearVelocity.y , direction.z*moveSpeed);
    }
}

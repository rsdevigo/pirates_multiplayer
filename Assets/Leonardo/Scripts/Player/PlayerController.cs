using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;

public class PlayerController : NetworkBehaviour
{
  private Input_System m_Input;
  private Rigidbody2D rb;
  public float moveSpeed;
  private Vector2 direction;
  public float jumpForce;
  private bool jump;
  private bool isground;
  private Vector3 facingRigth;
  private Vector3 facingLeft;
  public Transform detectsGround;
  public LayerMask ground;
  public Animator animator;

  private void Awake()
  {
    facingLeft = transform.localScale;
    facingRigth = transform.localScale;
    facingLeft.x = facingLeft.x * -1;

  }

  public override void OnStartServer()
  {
    base.OnStartServer();
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>();
 
  }
  public override void OnStartClient()
  {
    base.OnStartClient();
    rb = GetComponent<Rigidbody2D>();
    if (base.IsClientOnly)
      rb.isKinematic = true;
    if (base.IsOwner) {
      m_Input = new Input_System();
    }
      
  }

  private void OnEnable()
  {
    m_Input?.Enable();
  }
  private void OnDisable()
  {
    m_Input?.Disable();
  }
  public void Jump(InputAction.CallbackContext context)
  {
    if (base.IsOwner)
      jump = context.performed;
  }
  public void SetMoviment(InputAction.CallbackContext context)
  {
    if (base.IsOwner)
      direction = context.ReadValue<Vector2>();
  }
  private void FixedUpdate()
  {
    if (!base.IsOwner)
      return;
    RpcMove(direction, jump);
  }

  [ServerRpc]
  private void RpcMove(Vector2 dir, bool j)
  {
    rb.velocity = new Vector2(dir.x * moveSpeed, rb.velocity.y);
    if (dir.x > 0)
    {
      transform.localScale = facingRigth;
    }
    if (dir.x < 0)
    {
      transform.localScale = facingLeft;
    }
    isground = Physics2D.OverlapCircle(detectsGround.position, 0.2f, ground);
    if (j == true && isground == true)
    {
      rb.velocity = Vector2.zero;
      rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
      animator.SetBool("isJump", true);

    }
    if (isground && rb.velocity.y == 0)
    {
      animator.SetBool("isJump", false);
    }
    if (dir.x != 0)
    {
      animator.SetBool("isRun", true);
    }
    else
    {
      animator.SetBool("isRun", false);
    }
  }


}

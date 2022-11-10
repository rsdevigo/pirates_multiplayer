using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;

public class PlayerInputManager : NetworkBehaviour
{
  public bool dash;
  public bool jump;
  public Vector2 direction;
  public void OnDash(InputAction.CallbackContext ctx)
  {
    if (base.IsOwner)
      dash = ctx.performed;
  }
  public void OnJump(InputAction.CallbackContext ctx)
  {
    if (base.IsOwner)
      jump = ctx.performed;
  }

  public void OnMovement(InputAction.CallbackContext ctx)
  {
    if (base.IsOwner)
      direction = ctx.ReadValue<Vector2>();
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object.Prediction;
using FishNet.Object;

public class PlayerCheckManager : NetworkBehaviour
{
  [SerializeField]
  private Transform groundCheckTransform;
  [SerializeField]
  private LayerMask groundLayerMask;
  [SerializeField]
  private float groundRadius = 0.2f;
  public bool isGrounded;

  public override void OnStartNetwork()
  {
    base.OnStartNetwork();
    base.TimeManager.OnTick += TimeManager_OnUpdate;
  }

  public override void OnStopNetwork()
  {
    base.OnStopNetwork();
    if (base.TimeManager != null)
    {
      base.TimeManager.OnTick -= TimeManager_OnUpdate;
    }
  }

  public void TimeManager_OnUpdate()
  {
    isGrounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundRadius, groundLayerMask);
  }

  public void OnDrawGizmos()
  {
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(groundCheckTransform.position, groundRadius);
  }
}

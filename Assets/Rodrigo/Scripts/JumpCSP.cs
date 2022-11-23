using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Prediction;

public class JumpCSP : NetworkBehaviour
{
  private Rigidbody2D rb;
  private PlayerInputManager inputManager;
  private PlayerCheckManager playerCheckManager;
  [SerializeField]
  private float jumpForce = 15f;
  public override void OnStartNetwork()
  {
    base.OnStartNetwork();
    base.TimeManager.OnTick += TimeManager_OnTick;
    base.TimeManager.OnPostTick += TimeManager_OnPostTick;
  }

  public override void OnStopNetwork()
  {
    base.OnStopNetwork();
    if (base.TimeManager != null)
    {
      base.TimeManager.OnTick -= TimeManager_OnTick;
      base.TimeManager.OnPostTick -= TimeManager_OnPostTick;
    }
  }

  public override void OnStartServer()
  {
    base.OnStartServer();
    rb = GetComponent<Rigidbody2D>();
    playerCheckManager = GetComponent<PlayerCheckManager>();
  }

  public override void OnStartClient()
  {
    base.OnStartClient();
    rb = GetComponent<Rigidbody2D>();
    playerCheckManager = GetComponent<PlayerCheckManager>();
    if (base.IsOwner)
      inputManager = GetComponent<PlayerInputManager>();
  }

  public void TimeManager_OnTick()
  {
    if (base.IsOwner)
    {
      Reconciliation(default, false);
      JumpCSPData md = default;
      md.jump = inputManager.jump;
      inputManager.jump = false;
      ExecuteAction(md, false);
    }
    if (base.IsServer)
    {
      ExecuteAction(default, true);
    }
  }

  public void TimeManager_OnPostTick()
  {
    if (base.IsServer)
    {
      JumpCSPRecoil rd = new JumpCSPRecoil(rb.velocity, rb.angularVelocity, transform.position);
      Reconciliation(rd, true);
    }
  }

  [Replicate]
  private void ExecuteAction(JumpCSPData data, bool asServer, bool replaying = false)
  {
    if (data.jump && playerCheckManager.isGrounded)
    {
      rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
    }
  }

  [Reconcile]
  private void Reconciliation(JumpCSPRecoil data, bool asServer)
  {
    rb.velocity = data.velocity;
    transform.position = data.position;
    rb.angularVelocity = data.angularVelocity;
  }
}

public struct JumpCSPData
{
  public bool jump;
}

public struct JumpCSPRecoil
{
  public Vector2 velocity;
  public float angularVelocity;
  public Vector3 position;
  public JumpCSPRecoil(Vector2 vel, float aVel, Vector3 pos)
  {
    velocity = vel;
    angularVelocity = aVel;
    position = pos;
  }
}

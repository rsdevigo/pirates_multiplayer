using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Prediction;

public class MovementCSP : NetworkBehaviour
{
  private Rigidbody2D rb;
  private PlayerInputManager inputManager;
  [SerializeField]
  private float moveSpeed = 15f;
  private Vector2 m_Velocity;
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
      base.TimeManager.OnPostTick += TimeManager_OnPostTick;
    }
  }

  public override void OnStartServer()
  {
    base.OnStartServer();
    rb = GetComponent<Rigidbody2D>();
  }

  public override void OnStartClient()
  {
    base.OnStartClient();
    rb = GetComponent<Rigidbody2D>();
    if (base.IsOwner)
      inputManager = GetComponent<PlayerInputManager>();
  }

  public void TimeManager_OnTick()
  {
    if (base.IsOwner)
    {
      Reconciliation(default, false);
      MovementCSPData md = default;
      md.direction = inputManager.direction;
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
      MovementCSPRecoil rd = new MovementCSPRecoil(rb.velocity, rb.angularVelocity, transform.position);
      Reconciliation(rd, true);
    }
  }

  [Replicate]
  private void ExecuteAction(MovementCSPData data, bool asServer, bool replaying = false)
  {
    //Vector2 targetVelocity = new Vector2(data.direction.x * moveSpeed, rb.velocity.y);
    rb.AddForce(new Vector2(data.direction.x * moveSpeed, Physics.gravity.y));
    //rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref m_Velocity, 0.05f);
  }

  [Reconcile]
  private void Reconciliation(MovementCSPRecoil data, bool asServer)
  {
    rb.velocity = data.velocity;
    transform.position = data.position;
    rb.angularVelocity = data.angularVelocity;
  }
}

public struct MovementCSPData
{
  public Vector2 direction;
}

public struct MovementCSPRecoil
{
  public Vector2 velocity;
  public float angularVelocity;
  public Vector3 position;
  public MovementCSPRecoil (Vector2 vel, float aVel, Vector3 pos)
  {
    velocity = vel;
    angularVelocity = aVel;
    position = pos;
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
public class DashCSP : NetworkBehaviour
{
  private Rigidbody2D rb;
  private bool isDashing;
  private PlayerInputManager inputManager;
  private TrailRenderer tr;
  [SyncObject]
  private readonly SyncTimer _dashTimer = new SyncTimer();
  [SyncObject]
  private readonly SyncTimer _dashCooldownTimer = new SyncTimer();
  [SerializeField]
  private float dashDuration = 0.2f;
  [SerializeField]
  private float dashCooldown = 1f;
  [SerializeField]
  private float dashPower = 10f;
  private float gravityScale;
  // Start is called before the first frame update
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
    _dashCooldownTimer.Update(Time.deltaTime);
    _dashTimer.Update(Time.deltaTime);
  }

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
    tr = GetComponent<TrailRenderer>();
    _dashCooldownTimer.StartTimer(0f);
    _dashTimer.StartTimer(0f);
    isDashing = false;
  }

  public override void OnStartClient()
  {
    base.OnStartClient();
    rb = GetComponent<Rigidbody2D>();
    tr = GetComponent<TrailRenderer>();
    if (base.IsOwner)
      inputManager = GetComponent<PlayerInputManager>();
  }

  public void TimeManager_OnTick()
  {
    if (base.IsOwner)
    {
      Reconciliation(default, false);
      DashCSPData md = default;
      md.dash = inputManager.dash;
      md.direction = inputManager.direction;
      inputManager.dash = false;
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
      DashCSPRecoil rd = new DashCSPRecoil(rb.velocity, rb.angularVelocity, rb.gravityScale);
      Reconciliation(rd, true);
    }
  }

  [Replicate]
  private void ExecuteAction(DashCSPData data, bool asServer, bool replaying = false)
  {
    if (_dashTimer.Remaining > 0f)
      return;
    if (_dashTimer.Remaining <= 0f && isDashing)
    {
      StopDash(asServer);
      _dashCooldownTimer.StartTimer(dashCooldown);
    }
    if (data.dash && _dashCooldownTimer.Remaining <= 0f && !replaying)
    {
      isDashing = true;
      if (!asServer)
        tr.emitting = true;
      StartDash(data);
      _dashTimer.StartTimer(dashDuration);
    }
  }

  private void StartDash(DashCSPData data)
  {
    gravityScale = rb.gravityScale;
    rb.gravityScale = 0f;
    rb.velocity = new Vector2(data.direction.x * dashPower, 0f);
  }

  private void StopDash(bool asServer)
  {
    if (!asServer)
      tr.emitting = false;
    isDashing = false;
    rb.gravityScale = gravityScale;
    rb.velocity = Vector2.zero;
  }

  [Reconcile]
  private void Reconciliation(DashCSPRecoil data, bool asServer)
  {
    rb.velocity = data.velocity;
    rb.angularVelocity = data.angularVelocity;
    rb.gravityScale = data.gravityScale;
  }
}

public struct DashCSPData
{
  public bool dash;
  public Vector2 direction;
}

public struct DashCSPRecoil
{
  public Vector2 velocity;
  public float angularVelocity;
  public float gravityScale;

  public DashCSPRecoil(Vector2 vel, float aVel, float gScale)
  {
    velocity = vel;
    angularVelocity = aVel;
    gravityScale = gScale;
  }
}
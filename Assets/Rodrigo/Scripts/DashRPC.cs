using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Connection;
public class DashRPC : NetworkBehaviour
{
  private Rigidbody2D rb;
  [SyncVar]
  public bool isDashing;
  private PlayerInputManager inputManager;
  [SerializeField]
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
    if (base.IsOwner)
    {
      DashCSPData md = new DashCSPData(inputManager.dash, inputManager.direction);
      inputManager.dash = false;
      if (_dashTimer.Remaining > 0f)
        return;
      if (_dashTimer.Remaining <= 0f && isDashing)
      {
        StopDash();
      }
      if (md.dash && _dashCooldownTimer.Remaining <= 0f)
      {
        StartDash(md);
      }
    }
  }

  public override void OnStartNetwork()
  {
    base.OnStartNetwork();
    base.TimeManager.OnTick += TimeManager_OnTick;
    //base.TimeManager.OnPostTick += TimeManager_OnPostTick;
  }

  public override void OnStopNetwork()
  {
    base.OnStopNetwork();
    if (base.TimeManager != null)
    {
      base.TimeManager.OnTick -= TimeManager_OnTick;
      //base.TimeManager.OnPostTick -= TimeManager_OnPostTick;
    }
  }

  public override void OnStartServer()
  {
    base.OnStartServer();
    rb = GetComponent<Rigidbody2D>();
    _dashCooldownTimer.StartTimer(0f);
    _dashTimer.StartTimer(0f);
    isDashing = false;
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

  }

  [ServerRpc]
  private void CmdDash(DashCSPData data)
  {
    if (_dashTimer.Remaining > 0f)
      return;
    if (_dashTimer.Remaining <= 0f && isDashing)
    {
      StopDash();

    }
    if (data.dash && _dashCooldownTimer.Remaining <= 0f)
    {
      isDashing = true;
      ToggleTrail(base.Owner);
      StartDash(data);

    }
  }

  [TargetRpc]
  private void ToggleTrail(NetworkConnection conn)
  {
    tr.emitting = !tr.emitting;
  }
  [ServerRpc]
  private void StartDash(DashCSPData data)
  {
    Debug.Log("Iniciando Dash no Servidor");
    isDashing = true;
    gravityScale = rb.gravityScale;
    rb.gravityScale = 0f;
    rb.AddForce(new Vector2(dashPower * data.direction.x, 0f), ForceMode2D.Impulse);
    _dashTimer.StartTimer(dashDuration);
  }

  [ServerRpc]
  private void StopDash()
  {
    Debug.Log("Finalizando Dash no Servidor");
    ToggleTrail(base.Owner);
    isDashing = false;
    rb.gravityScale = gravityScale;
    rb.velocity = Vector2.zero;
    _dashCooldownTimer.StartTimer(dashCooldown);
  }
}
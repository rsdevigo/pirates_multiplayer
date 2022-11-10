using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using UnityEngine.InputSystem;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
public class CSPTestScript : NetworkBehaviour
{
    private Rigidbody2D rb;
    private bool dashPressed;
    private bool isDashing;
    private TrailRenderer tr;
    [SyncObject]
    private readonly SyncTimer _dashTimer = new SyncTimer();
    [SyncObject]
    private readonly SyncTimer _dashCooldownTimer = new SyncTimer();
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
        dashPressed = false;
        isDashing = false;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();
    }

    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (base.IsOwner)
        {
            dashPressed = ctx.performed;
        }
    }

    public void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            CSPDataTest md = default;
            md.dash = dashPressed;
            dashPressed = false;
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
            CSPRecoilTest rd = new CSPRecoilTest(rb.velocity, rb.angularVelocity);
            Reconciliation(rd, true);
        }
    }

    [Replicate]
    private void ExecuteAction(CSPDataTest data, bool asServer, bool replaying = false)
    {
        if (_dashTimer.Remaining > 0f)
            return;
        if (_dashTimer.Remaining <= 0f && isDashing)
        {
            StopDash(asServer);
            _dashCooldownTimer.StartTimer(4f);
        }
        if (data.dash && _dashCooldownTimer.Remaining <= 0f && !replaying)
        {
            isDashing = true;
            if (!asServer)
                tr.emitting = true;
            StartDash();
            _dashTimer.StartTimer(10f);
        }
    }

    private void StartDash()
    {
        rb.velocity = new Vector2(-1f * 10f, 0f);
    }

    private void StopDash(bool asServer)
    {
        if (!asServer)
            tr.emitting = false;
        isDashing = false;
    }

    [Reconcile]
    private void Reconciliation(CSPRecoilTest data, bool asServer)
    {
        rb.velocity = data.velocity;
        rb.angularVelocity = data.angularVelocity;
    }
}

public struct CSPDataTest
{
    public bool dash;
}

public struct CSPRecoilTest
{
    public Vector2 velocity;
    public float angularVelocity;

    public CSPRecoilTest(Vector2 vel, float aVel)
    {
        velocity = vel;
        angularVelocity = aVel;
    }
}
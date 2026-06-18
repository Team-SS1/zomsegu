using UnityEngine;
using VehicleEnum;

[RequireComponent(typeof(Rigidbody2D))]
public class VehicleController2D : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private VehicleDriveStats stats;

    [Header("Runtime")]
    [SerializeField] private bool engineOn = true;
    [SerializeField] private float fuel = 100f;

    public float CurrentSpeedKmh => currentSpeedKmh;
    public float CurrentSteerAngle => steerAngle;
    public bool EngineOn => engineOn;
    public float Fuel => fuel;

    private Rigidbody2D rb;
    private VehicleDamage damage;

    private float currentSpeedKmh;
    private float steerAngle;
    private float steerLimitBlend = 1f;

    private float zeroHoldTimer;
    private int lastMoveDir; // 1 forward, -1 reverse, 0 none

    private float engineFuelTimer;
    private float driveFuelTimer;
    private float soundTimer;

    private float lowSpeedCurveT;
    private int curveMoveDir;

    private const float KmhToMps = 1000f / 3600f;

    public VehicleSpriteState SpriteState { get; private set; } = VehicleSpriteState.Drive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        damage = GetComponent<VehicleDamage>();

        rb.gravityScale = 0f;
        rb.freezeRotation = false;

        if (stats != null)
            fuel = Mathf.Clamp(fuel, 0f, stats.maxFuel);
    }

    private void Update()
    {
        if (stats == null)
            return;

        bool broken = damage != null && damage.IsBroken;
        bool canDrive = engineOn && fuel > 0f && !broken;

        float vertical = GetVerticalInput();
        float horizontal = GetHorizontalInput();

        UpdateSteering(horizontal);
        UpdateSpeed(vertical, canDrive);
        UpdateFuel(canDrive);
        UpdateSoundEvent(canDrive);
    }

    private void FixedUpdate()
    {
        Debug.Log($"Speed:{currentSpeedKmh} " + $"Velocity:{rb.velocity} " + $"MoveDir:{transform.up}");

        if (stats == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (Mathf.Abs(currentSpeedKmh) <= 0.001f)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float appliedSteer = steerAngle * steerLimitBlend;

        if (Mathf.Abs(appliedSteer) > 0.01f)
        {
            float moveSign = Mathf.Sign(currentSpeedKmh);

            float rotationDelta =
                -appliedSteer *
                stats.rotationSpeedMultiplier *
                moveSign *
                Time.fixedDeltaTime;

            rb.MoveRotation(rb.rotation + rotationDelta);
        }

        float mps = Mathf.Abs(currentSpeedKmh) * KmhToMps;
        float sign = Mathf.Sign(currentSpeedKmh);

        Vector2 moveDir = transform.up;
        rb.velocity = moveDir.normalized * mps * sign;
    }

    private float GetVerticalInput()
    {
        bool w = Input.GetKey(KeyCode.W);
        bool s = Input.GetKey(KeyCode.S);

        if (w && s)
            return 0f;

        if (w) return 1f;
        if (s) return -1f;
        return 0f;
    }

    private float GetHorizontalInput()
    {
        bool a = Input.GetKey(KeyCode.A);
        bool d = Input.GetKey(KeyCode.D);

        if (a && d)
            return 0f;

        if (a) return -1f;
        if (d) return 1f;
        return 0f;
    }

    private void UpdateSpeed(float inputDir, bool canDrive)
    {
        bool w = Input.GetKey(KeyCode.W);
        bool s = Input.GetKey(KeyCode.S);

        bool brakeInput =
            Input.GetKey(KeyCode.Space) ||
            (inputDir > 0f && currentSpeedKmh < -0.01f) ||
            (inputDir < 0f && currentSpeedKmh > 0.01f) ||
            (w && s);

        UpdateSpriteState(inputDir, brakeInput);

        if (!canDrive)
        {
            DecelerateByNatural();
            return;
        }

        if (brakeInput)
        {
            Brake();
            return;
        }

        if (Mathf.Abs(currentSpeedKmh) <= 0.01f)
        {
            currentSpeedKmh = 0f;

            if (lastMoveDir != 0)
            {
                zeroHoldTimer += Time.deltaTime;

                if (zeroHoldTimer < stats.zeroHoldBeforeReverse)
                    return;
            }
        }
        else
        {
            zeroHoldTimer = 0f;
            lastMoveDir = currentSpeedKmh > 0f ? 1 : -1;
        }

        if (Mathf.Abs(inputDir) > 0.01f)
        {
            int wantedDir = inputDir > 0f ? 1 : -1;

            if (lastMoveDir != 0 && wantedDir != lastMoveDir && zeroHoldTimer < stats.zeroHoldBeforeReverse)
                return;

            Accelerate(wantedDir);
        }
        else
        {
            DecelerateByNatural();
        }
    }

    private void Accelerate(int dir)
    {
        float speedAbs = Mathf.Abs(currentSpeedKmh);

        if (curveMoveDir != dir)
        {
            curveMoveDir = dir;
            lowSpeedCurveT = GetCurveTFromSpeed01(speedAbs / stats.lowSpeedLimit);
        }

        if (speedAbs < stats.lowSpeedLimit)
        {
            lowSpeedCurveT += Time.deltaTime / Mathf.Max(0.01f, stats.zeroToLowSpeedTime);
            lowSpeedCurveT = Mathf.Clamp01(lowSpeedCurveT);

            float curveValue = Mathf.Clamp01(stats.startAccelCurve.Evaluate(lowSpeedCurveT));
            speedAbs = curveValue * stats.lowSpeedLimit;

            // 커브 초반이 너무 완만해서 0 근처에 묶이는 것 방지
            float minLaunchSpeed = 1f;
            if (speedAbs > 0f && speedAbs < minLaunchSpeed)
                speedAbs = minLaunchSpeed;
        }
        else
        {
            lowSpeedCurveT = 1f;

            float accelPerSec = (stats.maxSpeed - stats.lowSpeedLimit) / Mathf.Max(0.01f, stats.lowToMaxSpeedTime);
            speedAbs += accelPerSec * Time.deltaTime;
        }

        speedAbs = Mathf.Clamp(speedAbs, 0f, stats.maxSpeed);
        currentSpeedKmh = speedAbs * dir;
        lastMoveDir = dir;
    }

    private void DecelerateByNatural()
    {
        float speedAbs = Mathf.Abs(currentSpeedKmh);

        if (speedAbs <= 0.01f)
        {
            currentSpeedKmh = 0f;
            return;
        }

        float decelPerSec =
            stats.maxSpeed / stats.naturalDecelTime;

        float ratio =
            Mathf.Clamp01(speedAbs / stats.rollingThreshold);

        float rollingFactor =
            Mathf.Lerp(
                stats.rollingDecelMultiplier,
                1f,
                ratio
            );

        decelPerSec *= rollingFactor;

        speedAbs -= decelPerSec * Time.deltaTime;

        if (speedAbs <= 0.05f)
            speedAbs = 0f;

        currentSpeedKmh = speedAbs * Mathf.Sign(currentSpeedKmh);
    }

    private void Brake()
    {
        float speedAbs = Mathf.Abs(currentSpeedKmh);
        float brakePerSec = stats.maxSpeed / stats.brakeToZeroTime;

        speedAbs -= brakePerSec * Time.deltaTime;

        if (speedAbs <= 0.05f)
        {
            speedAbs = 0f;
            zeroHoldTimer = 0f;
            lowSpeedCurveT = 0f;
            curveMoveDir = 0;
        }

        currentSpeedKmh = speedAbs * Mathf.Sign(currentSpeedKmh);
    }

    private void UpdateSteering(float horizontal)
    {
        // 입력 없거나 A/D 동시 입력이면 즉시 직진
        if (Mathf.Abs(horizontal) < 0.01f)
        {
            steerAngle = 0f;
        }
        else
        {
            // 입력 방향으로 즉시 조향각 설정
            steerAngle = horizontal * stats.maxSteerAngle;
        }

        float targetBlend = Mathf.Abs(currentSpeedKmh) >= stats.highSpeedSteerThreshold ? stats.highSpeedSteerMultiplier : 1f;

        steerLimitBlend = Mathf.MoveTowards(
            steerLimitBlend,
            targetBlend,
            stats.steerLimitSmooth * Time.deltaTime
        );
    }

    private float GetCurveTFromSpeed01(float speed01)
    {
        speed01 = Mathf.Clamp01(speed01);

        float bestT = 0f;
        float bestDiff = float.MaxValue;

        const int samples = 32;

        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            float v = Mathf.Clamp01(stats.startAccelCurve.Evaluate(t));
            float diff = Mathf.Abs(v - speed01);

            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestT = t;
            }
        }

        return bestT;
    }

    private void UpdateFuel(bool canDrive)
    {
        if (!engineOn || fuel <= 0f || stats == null)
            return;

        engineFuelTimer += Time.deltaTime;
        if (engineFuelTimer >= 10f)
        {
            engineFuelTimer = 0f;
            fuel -= stats.engineFuelPer10Sec;
        }

        if (canDrive && Mathf.Abs(currentSpeedKmh) > 0.1f)
        {
            driveFuelTimer += Time.deltaTime;
            if (driveFuelTimer >= 3f)
            {
                driveFuelTimer = 0f;
                fuel -= stats.driveFuelPer3Sec;
            }
        }

        fuel = Mathf.Clamp(fuel, 0f, stats.maxFuel);
    }

    private void UpdateSoundEvent(bool canDrive)
    {
        if (!canDrive || WorldEventManager.Instance == null)
            return;

        soundTimer -= Time.deltaTime;
        if (soundTimer > 0f)
            return;

        soundTimer = 1f;

        float radius = stats.engineStartSoundRadius;

        if (Mathf.Abs(currentSpeedKmh) > 0.1f)
            radius = Mathf.Abs(currentSpeedKmh) <= 50f ? stats.driveSoundRadiusLow : stats.driveSoundRadiusHigh;

        WorldEventManager.Instance.RaiseSoundEvent(
            new SoundEvent(transform.position, radius, gameObject)
        );
    }

    public void ForceStop()
    {
        currentSpeedKmh = 0f;
        rb.velocity = Vector2.zero;
        zeroHoldTimer = 0f;
        lowSpeedCurveT = 0f;
        curveMoveDir = 0;
    }

    public void ReduceSpeed(float amountKmh)
    {
        float sign = Mathf.Sign(currentSpeedKmh);
        float speedAbs = Mathf.Max(0f, Mathf.Abs(currentSpeedKmh) - amountKmh);
        currentSpeedKmh = speedAbs * sign;
    }

    public void SetEngine(bool value)
    {
        engineOn = value;
    }

    private void UpdateSpriteState(float inputDir, bool brakeInput)
    {
        if (brakeInput)
        {
            SpriteState = VehicleSpriteState.Brake;
            return;
        }

        // 실제 후진 중이면 Reverse
        if (currentSpeedKmh < -0.01f)
        {
            SpriteState = VehicleSpriteState.Reverse;
            return;
        }

        SpriteState = VehicleSpriteState.Drive;
    }
}
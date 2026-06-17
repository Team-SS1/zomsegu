using UnityEngine;

[CreateAssetMenu(menuName = "SO/Drive Stats")]
public class VehicleDriveStats : ScriptableObject
{
    [Header("Speed km/h")]
    public float lowSpeedLimit = 50f;
    public float maxSpeed = 150f;

    [Header("Rotation")]
    public float rotationSpeedMultiplier = 1f;

    [Header("Acceleration")]
    public float zeroToLowSpeedTime = 3f;
    public AnimationCurve startAccelCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float lowToMaxSpeedTime = 5f;

    [Header("Deceleration")]
    public float brakeToZeroTime = 5f;

    [Header("Direction Switch")]
    public float zeroHoldBeforeReverse = 1f;

    [Header("Steering")]
    public float maxSteerAngle = 90f;
    public float steerFullTime = 1.5f;
    public float highSpeedSteerThreshold = 80f;
    public float highSpeedSteerMultiplier = 0.5f;
    public float steerLimitSmooth = 6f;

    [Header("Fuel")]
    public float maxFuel = 100f;
    public float engineFuelPer10Sec = 0.1f;
    public float driveFuelPer3Sec = 0.1f;

    [Header("Sound Radius")]
    public float engineStartSoundRadius = 5f;
    public float driveSoundRadiusLow = 7f;
    public float driveSoundRadiusHigh = 10f;
}
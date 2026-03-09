using UnityEngine;

public class MonsterFacing8Dir : MonoBehaviour
{
    private Monster monster;

    private Vector2 snappedFacingDir = Vector2.down;

    [SerializeField] private bool useAngleHysteresis8Dir = true;
    private int hysteresis8DirIndex = 6;

    public bool UseAngleHysteresis8Dir => useAngleHysteresis8Dir;

    private static readonly float[] HysMinDeg = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
    private static readonly float[] HysMaxDeg = { 50f, 95f, 140f, 185f, 230f, 275f, 320f, 365f };

    private void Awake()
    {
        monster = GetComponent<Monster>();
    }

    public void ResetFacing()
    {
        Vector2 fd = monster.FacingDirection;
        if (fd.sqrMagnitude < 0.0001f)
            fd = Vector2.down;

        float ang = Mathf.Atan2(fd.y, fd.x) * Mathf.Rad2Deg;
        if (ang < 0f) ang += 360f;

        hysteresis8DirIndex = Mathf.RoundToInt(ang / 45f) % 8;
        snappedFacingDir = IndexTo8DirVector(hysteresis8DirIndex);
    }

    public void TickUpdateFacingFromMove()
    {
        if (monster.MoveDirection.sqrMagnitude > 0.001f)
            monster.FacingDirection = monster.MoveDirection.normalized;
    }

    public Vector2 SnapTo8Dir(Vector2 v)
    {
        if (v.sqrMagnitude < 0.0001f)
            return Vector2.zero;

        const float dead = 0.05f;
        if (Mathf.Abs(v.x) < dead) v.x = 0f;
        if (Mathf.Abs(v.y) < dead) v.y = 0f;

        if (v.sqrMagnitude < 0.0001f)
            return Vector2.zero;

        v.Normalize();

        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;

        int idx = Mathf.RoundToInt(angle / 45f) % 8;
        return IndexTo8DirVector(idx);
    }

    public Vector2 SnapTo8DirStable(Vector2 targetDir, float changeThresholdDeg = 25f)
    {
        if (targetDir.sqrMagnitude < 0.0001f)
            return snappedFacingDir;

        Vector2 desired = SnapTo8Dir(targetDir);
        float angle = Vector2.Angle(snappedFacingDir, desired);

        if (angle >= changeThresholdDeg)
            snappedFacingDir = desired;

        return snappedFacingDir;
    }

    public Vector2 SnapTo8DirHysteresisByAngleTable(Vector2 targetDir)
    {
        if (targetDir.sqrMagnitude < 0.0001f)
            return IndexTo8DirVector(hysteresis8DirIndex);

        float ang = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
        if (ang < 0f) ang += 360f;

        int matchA = -1;
        int matchB = -1;

        for (int i = 0; i < 8; i++)
        {
            if (AngleInRange(ang, HysMinDeg[i], HysMaxDeg[i]))
            {
                if (matchA == -1) matchA = i;
                else
                {
                    matchB = i;
                    break;
                }
            }
        }

        int nextIdx;

        if (matchA == -1)
        {
            nextIdx = Mathf.RoundToInt(ang / 45f) % 8;
        }
        else if (matchB == -1)
        {
            nextIdx = matchA;
        }
        else
        {
            if (hysteresis8DirIndex == matchA || hysteresis8DirIndex == matchB)
                nextIdx = hysteresis8DirIndex;
            else
                nextIdx = matchA;
        }

        hysteresis8DirIndex = nextIdx;
        return IndexTo8DirVector(hysteresis8DirIndex);
    }

    private bool AngleInRange(float ang, float min, float max)
    {
        if (max <= 360f)
            return ang >= min && ang < max;

        float wrappedMax = max - 360f;
        return (ang >= min && ang < 360f) || (ang >= 0f && ang < wrappedMax);
    }

    private Vector2 IndexTo8DirVector(int idx)
    {
        switch (idx)
        {
            case 0: return Vector2.right;
            case 1: return (Vector2.right + Vector2.up).normalized;
            case 2: return Vector2.up;
            case 3: return (Vector2.left + Vector2.up).normalized;
            case 4: return Vector2.left;
            case 5: return (Vector2.left + Vector2.down).normalized;
            case 6: return Vector2.down;
            case 7: return (Vector2.right + Vector2.down).normalized;
            default: return Vector2.down;
        }
    }
}
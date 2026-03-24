using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Zombie))]
public class ZombiePathAgent : MonoBehaviour
{
    [Header("Stuck 판정")]
    [Tooltip("n초 동안 0.2유닛 이하 이동이면 Stuck")]
    public float stuckDistanceThreshold = 0.2f;

    [Header("Path Mode")]
    public float pathModeDuration = 3f;
    public float repathInterval = 0.25f;
    public float repathMoveThreshold = 0.4f;

    [Header("Dependencies")]
    public MonoBehaviour gridProviderBehaviour; // IGridProvider
    private IGridProvider grid;

    private Zombie z;

    // stuck tracking
    private Vector2 stuckCheckStartPos;
    private float stuckCheckTimer;

    // mode
    private bool pathMode;
    private float pathModeTimer;
    private float repathTimer;

    // target tracking
    private Vector2 lastPlannedGoal;
    private Vector2 lastKnownTargetPos;

    // path buffer
    private readonly Vector2[] pathPoints = new Vector2[128];
    private int pathCount;
    private int pathIndex;

    private SimpleAStar aStar;

    private bool forcePathReplanPending;

    private void Awake()
    {
        z = GetComponent<Zombie>();

        grid = gridProviderBehaviour as IGridProvider;
        if (grid == null && gridProviderBehaviour != null)
        {
#if UNITY_EDITOR
            Debug.LogError($"[ZombiePathAgent] gridProviderBehaviour는 IGridProvider를 구현해야 함: {gridProviderBehaviour.name}");
#endif
        }

        aStar = new SimpleAStar();
        stuckCheckStartPos = z.GetNavigationOrigin();
    }

    public void ResetAll()
    {
        stuckCheckStartPos = z.GetNavigationOrigin();
        stuckCheckTimer = 0f;

        pathMode = false;
        pathModeTimer = 0f;
        repathTimer = 0f;

        pathCount = 0;
        pathIndex = 0;

        lastPlannedGoal = Vector2.zero;
        lastKnownTargetPos = Vector2.zero;
    }

    private float GetStuckDecisionTime()
    {
        switch (z.zombieType)
        {
            case MonsterEnum.ZombieType.Athlete:
            case MonsterEnum.ZombieType.Police:
            case MonsterEnum.ZombieType.Soldier:
            case MonsterEnum.ZombieType.Firefighter:
                return 0.5f;
        }

        float ms = z.stat != null ? z.stat.MoveSpeed : 0f;
        if (ms >= 120f) return 1f;
        if (ms >= 80f) return 2f;
        return 3f;
    }

    public bool TickPathAndStuck_Aggro()
    {
        if (grid == null) return false;
        if (!z.HasValidTarget()) return false;

        if (z.IsAttacking || z.IsTakingDamage || (z.Knockback != null && z.Knockback.IsKnockbacking))
        {
            // 넉백/피격 중에는 path를 "진행"하진 않되, 이미 강제로 켠 pathMode는 유지
            return false;
        }

        // 넉백/피격이 끝난 직후 강제 path 재계산
        if (forcePathReplanPending && pathMode && z.HasValidTarget())
        {
            forcePathReplanPending = false;
            PlanPathForAggro();
        }

        if (forcePathReplanPending && pathMode && z.HasValidTarget())
        {
            forcePathReplanPending = false;

#if UNITY_EDITOR
            Debug.Log("[ZombiePathAgent] Replan after hit");
#endif

            PlanPathForAggro();
        }

        if (!pathMode)
        {
            CheckStuck();

            if (pathMode)
            {
                PlanPathForAggro();
                return pathMode;
            }

            return false;
        }

        pathModeTimer -= Time.deltaTime;
        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;

            Vector2 goalNow = GetAggroGoal();
            if ((goalNow - lastPlannedGoal).sqrMagnitude >= repathMoveThreshold * repathMoveThreshold)
                PlanPathForAggro();
        }

        FollowPathStep();

        if (pathModeTimer <= 0f)
        {
            pathMode = false;
            pathCount = 0;
            pathIndex = 0;
            return false;
        }

        return pathMode;
    }

    public bool TickPathAndStuck_Investigate(Vector2 investigatePos)
    {
        if (grid == null) return false;

        if (z.IsAttacking || z.IsTakingDamage || (z.Knockback != null && z.Knockback.IsKnockbacking))
        {
            // 넉백/피격 중에는 path를 "진행"하진 않되, 이미 강제로 켠 pathMode는 유지
            return false;
        }

        if (!pathMode)
        {
            CheckStuck();

            if (pathMode)
            {
                PlanPath(investigatePos);
                return pathMode;
            }

            return false;
        }

        pathModeTimer -= Time.deltaTime;
        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;

            if ((investigatePos - lastPlannedGoal).sqrMagnitude >= repathMoveThreshold * repathMoveThreshold)
                PlanPath(investigatePos);
        }

        FollowPathStep();

        if (pathModeTimer <= 0f)
        {
            pathMode = false;
            pathCount = 0;
            pathIndex = 0;
            return false;
        }

        return pathMode;
    }

    private void CheckStuck()
    {
        if (z.MoveDirection.sqrMagnitude < 0.0001f)
        {
            stuckCheckStartPos = z.GetNavigationOrigin();
            stuckCheckTimer = 0f;
            return;
        }

        stuckCheckTimer += Time.deltaTime;

        float decisionTime = GetStuckDecisionTime();
        if (stuckCheckTimer < decisionTime)
            return;

        float moved = Vector2.Distance(z.GetNavigationOrigin(), stuckCheckStartPos);

#if UNITY_EDITOR
        Debug.Log($"[ZombiePathAgent] StuckCheck - elapsed:{stuckCheckTimer:F2}, moved:{moved:F3}, threshold:{stuckDistanceThreshold:F2}");
#endif

        if (moved <= stuckDistanceThreshold)
        {
            pathMode = true;
            pathModeTimer = pathModeDuration;
            repathTimer = 0f;

#if UNITY_EDITOR
            Debug.Log("[ZombiePathAgent] Enter PathMode (stuck detected)");
#endif
        }

        stuckCheckStartPos = z.GetNavigationOrigin();
        stuckCheckTimer = 0f;
    }

    private Vector2 GetAggroGoal()
    {
        if (z.HasValidTarget())
        {
            lastKnownTargetPos = z.GetTargetBodyOrigin();
            return lastKnownTargetPos;
        }

        return lastKnownTargetPos;
    }

    private void PlanPathForAggro()
    {
        Vector2 goal = GetAggroGoal();
        PlanPath(goal);
    }

    private void PlanPath(Vector2 worldGoal)
    {
        Vector2 startWorld = z.GetNavigationOrigin();
        Vector2 goalWorld = worldGoal;

        bool foundStart = TryFindNearestWalkable(startWorld, 2, out startWorld);
        bool foundGoal = TryFindNearestWalkable(goalWorld, 8, out goalWorld);

#if UNITY_EDITOR
        Debug.Log($"[ZombiePathAgent] PlanPath adjusted start:{startWorld} foundStart:{foundStart}, goal:{goalWorld} foundGoal:{foundGoal}");
#endif

        if (!foundStart || !foundGoal)
        {
            pathMode = false;
            pathCount = 0;
            pathIndex = 0;
            return;
        }

        lastPlannedGoal = goalWorld;

        int count = aStar.FindPath(grid, startWorld, goalWorld, pathPoints);
        pathCount = count;
        pathIndex = 0;

#if UNITY_EDITOR
        Debug.Log($"[ZombiePathAgent] PlanPath result count:{count}");
#endif

        if (pathCount <= 1)
        {
            pathMode = false;
            pathCount = 0;
            pathIndex = 0;
        }
    }

    private bool TryFindNearestWalkable(Vector2 worldPos, int searchRadius, out Vector2 resultWorld)
    {
        resultWorld = worldPos;

        grid.WorldToGrid(worldPos, out int cx, out int cy);

        if (grid.IsWalkable(cx, cy))
        {
            resultWorld = grid.GridToWorld(cx, cy);
            return true;
        }

        for (int r = 1; r <= searchRadius; r++)
        {
            // 상/하 라인
            for (int x = -r; x <= r; x++)
            {
                int topY = cy + r;
                int bottomY = cy - r;

                int gx1 = cx + x;
                if (grid.IsWalkable(gx1, topY))
                {
                    resultWorld = grid.GridToWorld(gx1, topY);
                    return true;
                }

                if (grid.IsWalkable(gx1, bottomY))
                {
                    resultWorld = grid.GridToWorld(gx1, bottomY);
                    return true;
                }
            }

            // 좌/우 라인 (모서리 중복 제외)
            for (int y = -r + 1; y <= r - 1; y++)
            {
                int leftX = cx - r;
                int rightX = cx + r;

                int gy1 = cy + y;
                if (grid.IsWalkable(leftX, gy1))
                {
                    resultWorld = grid.GridToWorld(leftX, gy1);
                    return true;
                }

                if (grid.IsWalkable(rightX, gy1))
                {
                    resultWorld = grid.GridToWorld(rightX, gy1);
                    return true;
                }
            }
        }

        return false;
    }

    private void FollowPathStep()
    {
        if (pathCount <= 0)
        {
            pathMode = false;
            return;
        }

        Vector2 pos = z.GetNavigationOrigin();

        while (pathIndex < pathCount)
        {
            Vector2 currentPoint = pathPoints[pathIndex];
            if ((currentPoint - pos).sqrMagnitude < 0.09f)
                pathIndex++;
            else
                break;
        }

        if (pathIndex >= pathCount)
        {
            pathMode = false;
            pathCount = 0;
            pathIndex = 0;
            return;
        }

        Vector2 nextPoint = pathPoints[pathIndex];
        Vector2 to = nextPoint - pos;

        if (to.sqrMagnitude > 0.0001f)
            z.MoveDirection = to.normalized;
    }

    public void ForcePathModeAfterHit()
    {
        if (grid == null)
            return;

        pathMode = true;
        pathModeTimer = pathModeDuration;   // 지속시간 초기화
        repathTimer = 0f;                   // 즉시 재탐색 가능하게
        pathCount = 0;
        pathIndex = 0;

        stuckCheckStartPos = z.GetNavigationOrigin();
        stuckCheckTimer = 0f;

        forcePathReplanPending = true;

#if UNITY_EDITOR
        Debug.Log("[ZombiePathAgent] ForcePathModeAfterHit()");
#endif
    }

    public void ForcePathModeAfterAttack()
    {
        if (grid == null)
            return;

        pathMode = true;
        pathModeTimer = pathModeDuration;   // 지속시간 초기화
        repathTimer = 0f;                   // 즉시 재탐색
        pathCount = 0;
        pathIndex = 0;

        stuckCheckStartPos = z.GetNavigationOrigin();
        stuckCheckTimer = 0f;

        forcePathReplanPending = true; // 이름은 hit지만 재사용 가능
    }

    // -------------------------------------------------
    // Simple A* (그리드 기반 / GC 최소화)
    // -------------------------------------------------
    private sealed class SimpleAStar
    {
        private const int MAX_NODES = 4096;

        private readonly int[] open = new int[MAX_NODES];
        private readonly bool[] inOpen = new bool[MAX_NODES];
        private readonly bool[] inClosed = new bool[MAX_NODES];

        private readonly int[] cameFrom = new int[MAX_NODES];
        private readonly int[] gCost = new int[MAX_NODES];
        private readonly int[] fCost = new int[MAX_NODES];

        private readonly int[] nodeX = new int[MAX_NODES];
        private readonly int[] nodeY = new int[MAX_NODES];

        private int nodeCount;

        public int FindPath(IGridProvider grid, Vector2 startW, Vector2 goalW, Vector2[] outPath)
        {
            nodeCount = 0;

            grid.WorldToGrid(startW, out int sx, out int sy);
            grid.WorldToGrid(goalW, out int gx, out int gy);

            if (!InBounds(grid, sx, sy) || !InBounds(grid, gx, gy))
                return 0;

            if (!grid.IsWalkable(sx, sy) || !grid.IsWalkable(gx, gy))
                return 0;

            int start = GetOrAddNode(sx, sy);

            for (int i = 0; i < MAX_NODES; i++)
            {
                inOpen[i] = false;
                inClosed[i] = false;
                cameFrom[i] = -1;
                gCost[i] = int.MaxValue / 4;
                fCost[i] = int.MaxValue / 4;
            }

            int openCount = 0;

            gCost[start] = 0;
            fCost[start] = Heuristic(sx, sy, gx, gy);

            open[openCount++] = start;
            inOpen[start] = true;

            while (openCount > 0)
            {
                int current = PopLowestF(open, ref openCount);
                inOpen[current] = false;
                inClosed[current] = true;

                int cx = nodeX[current];
                int cy = nodeY[current];

                if (cx == gx && cy == gy)
                    return Reconstruct(grid, current, outPath);

                // 8방향
                TryNeighbor(grid, cx + 1, cy, current, cx, cy, gx, gy, 10, open, ref openCount);
                TryNeighbor(grid, cx - 1, cy, current, cx, cy, gx, gy, 10, open, ref openCount);
                TryNeighbor(grid, cx, cy + 1, current, cx, cy, gx, gy, 10, open, ref openCount);
                TryNeighbor(grid, cx, cy - 1, current, cx, cy, gx, gy, 10, open, ref openCount);

                TryNeighbor(grid, cx + 1, cy + 1, current, cx, cy, gx, gy, 14, open, ref openCount);
                TryNeighbor(grid, cx - 1, cy + 1, current, cx, cy, gx, gy, 14, open, ref openCount);
                TryNeighbor(grid, cx + 1, cy - 1, current, cx, cy, gx, gy, 14, open, ref openCount);
                TryNeighbor(grid, cx - 1, cy - 1, current, cx, cy, gx, gy, 14, open, ref openCount);
            }

            return 0;
        }

        private void TryNeighbor(
    IGridProvider grid,
    int nx, int ny,
    int current,
    int cx, int cy,
    int gx, int gy,
    int moveCost,
    int[] open,
    ref int openCount)
        {
            if (!InBounds(grid, nx, ny))
                return;

            if (!grid.IsWalkable(nx, ny))
                return;

            // 대각선 이동이면 코너 끼워지기 금지
            int dx = nx - cx;
            int dy = ny - cy;
            bool isDiagonal = dx != 0 && dy != 0;

            if (isDiagonal)
            {
                if (!grid.IsWalkable(cx + dx, cy) || !grid.IsWalkable(cx, cy + dy))
                    return;
            }

            int n = GetOrAddNode(nx, ny);
            if (n < 0)
                return;

            if (inClosed[n])
                return;

            int tentativeG = gCost[current] + moveCost;

            if (!inOpen[n])
            {
                open[openCount++] = n;
                inOpen[n] = true;
            }
            else if (tentativeG >= gCost[n])
            {
                return;
            }

            cameFrom[n] = current;
            gCost[n] = tentativeG;
            fCost[n] = tentativeG + Heuristic(nx, ny, gx, gy);
        }

        private int Reconstruct(IGridProvider grid, int goalNode, Vector2[] outPath)
        {
            int count = 0;
            int cur = goalNode;

            while (cur >= 0 && count < outPath.Length)
            {
                outPath[count++] = grid.GridToWorld(nodeX[cur], nodeY[cur]);
                cur = cameFrom[cur];
            }

            int i = 0;
            int j = count - 1;
            while (i < j)
            {
                Vector2 tmp = outPath[i];
                outPath[i] = outPath[j];
                outPath[j] = tmp;
                i++;
                j--;
            }

            return count;
        }

        private int PopLowestF(int[] open, ref int openCount)
        {
            int bestIndex = 0;
            int bestNode = open[0];
            int bestF = fCost[bestNode];

            for (int i = 1; i < openCount; i++)
            {
                int node = open[i];
                if (fCost[node] < bestF)
                {
                    bestF = fCost[node];
                    bestNode = node;
                    bestIndex = i;
                }
            }

            openCount--;
            open[bestIndex] = open[openCount];
            return bestNode;
        }

        private int GetOrAddNode(int x, int y)
        {
            for (int i = 0; i < nodeCount; i++)
            {
                if (nodeX[i] == x && nodeY[i] == y)
                    return i;
            }

            if (nodeCount >= MAX_NODES)
                return -1;

            int idx = nodeCount++;
            nodeX[idx] = x;
            nodeY[idx] = y;
            return idx;
        }

        private int Heuristic(int x, int y, int gx, int gy)
        {
            int dx = Mathf.Abs(x - gx);
            int dy = Mathf.Abs(y - gy);
            return (dx + dy) * 10;
        }

        private bool InBounds(IGridProvider grid, int x, int y)
        {
            return (uint)x < (uint)grid.Width && (uint)y < (uint)grid.Height;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (pathCount <= 0)
            return;

        Gizmos.color = Color.green;

        for (int i = pathIndex; i < pathCount; i++)
        {
            Gizmos.DrawSphere(pathPoints[i], 0.06f);

            if (i < pathCount - 1)
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
        }
    }
#endif
}
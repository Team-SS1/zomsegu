using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(WildDog))]
public class WildDogPathAgent : MonoBehaviour
{
    [Header("Path")]
    public float repathInterval = 0.2f;
    public float repathMoveThreshold = 0.5f;
    public float stuckDecisionTime = 1f;

    [Header("Dependencies")]
    public MonoBehaviour gridProviderBehaviour;
    private IGridProvider grid;

    private WildDog dog;

    private Vector2 lastPlannedGoal;
    private float repathTimer;

    private Vector2 stuckCheckStartPos;
    private float stuckTimer;

    private readonly Vector2[] pathPoints = new Vector2[128];
    private int pathCount;
    private int pathIndex;

    private bool forceReplanPending;

    public int DebugPathCount => pathCount;
    public int DebugPathIndex => pathIndex;

    private SimpleAStar aStar;

    private void Awake()
    {
        dog = GetComponent<WildDog>();
        grid = gridProviderBehaviour as IGridProvider;
        aStar = new SimpleAStar();
        stuckCheckStartPos = dog.GetNavigationOrigin();
    }

    public void ResetAll()
    {
        repathTimer = 0f;
        stuckTimer = 0f;
        stuckCheckStartPos = dog.GetNavigationOrigin();
        lastPlannedGoal = Vector2.zero;
        pathCount = 0;
        pathIndex = 0;
        forceReplanPending = false;
    }

    public void ForcePathModeAfterHit()
    {
        forceReplanPending = true;
        repathTimer = 0f;
        stuckTimer = 0f;
        pathCount = 0;
        pathIndex = 0;
        stuckCheckStartPos = dog.GetNavigationOrigin();
    }

    public void ForcePathModeAfterAttack()
    {
        forceReplanPending = true;
        repathTimer = 0f;
        pathCount = 0;
        pathIndex = 0;
    }

    public bool TickPathToGoal(Vector2 goalWorld)
    {
        if (grid == null)
            return false;

        if (dog.IsAttacking || dog.IsTakingDamage || dog.IsStunned)
            return pathCount > 0;

        if (dog.Knockback != null && dog.Knockback.IsKnockbacking)
            return pathCount > 0;

        // 1. 강제 재탐색 요청은 즉시 처리
        if (forceReplanPending)
        {
            forceReplanPending = false;
            PlanPath(goalWorld);
        }

        // 2. 현재 경로가 없으면 타이머 기다리지 말고 즉시 재탐색
        if (pathCount <= 0)
        {
            PlanPath(goalWorld);
        }

        // 3. 주기적 재탐색
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;

            bool needRepath =
                pathCount <= 0 ||
                (goalWorld - lastPlannedGoal).sqrMagnitude >= repathMoveThreshold * repathMoveThreshold;

            if (needRepath)
                PlanPath(goalWorld);
        }

        CheckStuck(goalWorld);
        FollowPathStep();

        // 4. FollowPathStep 이후 경로가 끝났으면 바로 다음 프레임 재탐색되도록 타이머 초기화
        if (pathCount <= 0)
            repathTimer = 0f;

        return pathCount > 0;
    }

    private void CheckStuck(Vector2 goalWorld)
    {
        if (dog.MoveDirection.sqrMagnitude < 0.0001f)
        {
            stuckCheckStartPos = dog.GetNavigationOrigin();
            stuckTimer = 0f;
            return;
        }

        stuckTimer += Time.deltaTime;
        if (stuckTimer < stuckDecisionTime)
            return;

        float moved = Vector2.Distance(dog.GetNavigationOrigin(), stuckCheckStartPos);
        if (moved <= 0.2f)
            PlanPath(goalWorld);

        stuckCheckStartPos = dog.GetNavigationOrigin();
        stuckTimer = 0f;
    }

    private void PlanPath(Vector2 worldGoal)
    {
        Vector2 startWorld = dog.GetNavigationOrigin();
        Vector2 goal = worldGoal;

        bool foundStart = TryFindNearestWalkable(startWorld, 2, out startWorld);
        bool foundGoal = TryFindNearestWalkable(goal, 8, out goal);

        if (!foundStart || !foundGoal)
        {
            pathCount = 0;
            pathIndex = 0;
            return;
        }

        lastPlannedGoal = goal;
        pathCount = aStar.FindPath(grid, startWorld, goal, pathPoints);
        pathIndex = 0;
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
            return;

        Vector2 pos = dog.GetNavigationOrigin();

        while (pathIndex < pathCount)
        {
            Vector2 p = pathPoints[pathIndex];
            if ((p - pos).sqrMagnitude < 0.16f)
                pathIndex++;
            else
                break;
        }

        if (pathIndex >= pathCount)
        {
            pathCount = 0;
            pathIndex = 0;
            return;
        }

        Vector2 next = pathPoints[pathIndex];
        Vector2 to = next - pos;
        if (to.sqrMagnitude > 0.0001f)
            dog.MoveDirection = to.normalized;
    }

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

            int dx = nx - cx;
            int dy = ny - cy;
            bool diagonal = dx != 0 && dy != 0;

            if (diagonal)
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
}
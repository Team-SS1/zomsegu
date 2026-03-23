using UnityEngine;

[DisallowMultipleComponent]
public class PhysicsGridProvider : MonoBehaviour, IGridProvider
{
    [Header("Grid Bounds (World Space)")]
    [SerializeField] private Vector2 worldMin = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 worldMax = new Vector2(50f, 50f);

    [Header("Cell Size")]
    [SerializeField] private float cellSize = 0.2f;

    [Header("Obstacle Check")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Agent Clearance")]
    [Tooltip("길찾기에서 사용할 좀비 몸 크기(BoxCollider2D 기준)")]
    [SerializeField] private Vector2 agentSize = new Vector2(0.6f, 1.0f);

    [Tooltip("좀비 몸 크기에 추가로 더할 여유 거리")]
    [SerializeField] private float extraClearance = 0.05f;

    private readonly Collider2D[] overlapBuffer = new Collider2D[16];

    private int width;
    private int height;

    public int Width => width;
    public int Height => height;

    private void Awake()
    {
        Rebuild();
    }

    private void OnValidate()
    {
        if (cellSize < 0.1f) cellSize = 0.1f;
        Rebuild();
    }

    public void Rebuild()
    {
        width = Mathf.Max(1, Mathf.CeilToInt((worldMax.x - worldMin.x) / cellSize));
        height = Mathf.Max(1, Mathf.CeilToInt((worldMax.y - worldMin.y) / cellSize));
    }

    public void WorldToGrid(Vector2 world, out int gx, out int gy)
    {
        gx = Mathf.FloorToInt((world.x - worldMin.x) / cellSize);
        gy = Mathf.FloorToInt((world.y - worldMin.y) / cellSize);
    }

    public Vector2 GridToWorld(int gx, int gy)
    {
        float x = worldMin.x + (gx + 0.5f) * cellSize;
        float y = worldMin.y + (gy + 0.5f) * cellSize;
        return new Vector2(x, y);
    }

    public bool IsWalkable(int gx, int gy)
    {
        if ((uint)gx >= (uint)width || (uint)gy >= (uint)height)
            return false;

        Vector2 center = GridToWorld(gx, gy);

        // 좀비 몸 크기 + 여유 거리만큼 장애물과 겹치면 막힘 처리
        Vector2 checkSize = agentSize + Vector2.one * (extraClearance * 2f);

        int count = Physics2D.OverlapBoxNonAlloc(center, checkSize, 0f, overlapBuffer, obstacleMask);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = overlapBuffer[i];
            if (col == null)
                continue;

            if (col.isTrigger)
                continue;

            return false;
        }

        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = (worldMin + worldMax) * 0.5f;
        Vector3 size = worldMax - worldMin;
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
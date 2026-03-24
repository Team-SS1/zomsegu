using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class TilemapGridProvider : MonoBehaviour, IGridProvider
{
    [Header("Obstacle Tilemap")]
    [Tooltip("충돌/장애물 타일이 깔려있는 Tilemap을 넣어줘. 이 타일이 있으면 막힘 처리.")]
    [SerializeField] private Tilemap obstacleTilemap;

    [Header("Optional")]
    [Tooltip("장애물 타일맵 bounds가 너무 넓다면, 여기서 bounds를 수동으로 제한할 수도 있어(선택).")]
    [SerializeField] private bool useCustomBounds = false;

    [SerializeField] private Vector3Int customMinCell;
    [SerializeField] private Vector3Int customMaxCell; // max는 포함 X (Unity BoundsInt 규칙)

    private BoundsInt bounds;
    private Vector3Int minCell;
    private int width;
    private int height;

    public int Width => width;
    public int Height => height;

    private void Awake()
    {
        RebuildBounds();
    }

    private void OnValidate()
    {
        // 에디터에서도 안전하게
        if (obstacleTilemap != null)
            RebuildBounds();
    }

    public void RebuildBounds()
    {
        if (obstacleTilemap == null)
        {
            width = height = 0;
            bounds = new BoundsInt();
            minCell = Vector3Int.zero;
            return;
        }

        if (useCustomBounds)
        {
            // customMaxCell은 포함X로 맞춰줘야 함
            bounds = new BoundsInt(customMinCell, customMaxCell - customMinCell);
        }
        else
        {
            // obstacleTilemap.cellBounds는 "전체 가능한 영역"이라 넓을 수 있음.
            // 그래도 간단/빠르게 가려면 이걸 쓰고,
            // 더 최적화하려면 실제 사용 영역 bounds를 별도로 관리하는 걸 추천.
            bounds = obstacleTilemap.cellBounds;
        }

        minCell = bounds.min;
        width = bounds.size.x;
        height = bounds.size.y;
    }

    public void WorldToGrid(Vector2 world, out int gx, out int gy)
    {
        if (obstacleTilemap == null)
        {
            gx = gy = 0;
            return;
        }

        Vector3Int cell = obstacleTilemap.WorldToCell(world);

        gx = cell.x - minCell.x;
        gy = cell.y - minCell.y;
    }

    public Vector2 GridToWorld(int gx, int gy)
    {
        if (obstacleTilemap == null)
            return Vector2.zero;

        Vector3Int cell = new Vector3Int(gx + minCell.x, gy + minCell.y, 0);

        // 셀 중심 반환
        Vector3 w = obstacleTilemap.GetCellCenterWorld(cell);
        return (Vector2)w;
    }

    public bool IsWalkable(int gx, int gy)
    {
        if (obstacleTilemap == null) return false;

        // bounds 밖은 막힘 처리
        if ((uint)gx >= (uint)width || (uint)gy >= (uint)height)
            return false;

        Vector3Int cell = new Vector3Int(gx + minCell.x, gy + minCell.y, 0);

        // 장애물 타일이 있으면 막힘
        return !obstacleTilemap.HasTile(cell);
    }
}
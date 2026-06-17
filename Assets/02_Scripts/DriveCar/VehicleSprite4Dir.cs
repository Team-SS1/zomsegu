using UnityEngine;
using VehicleEnum;

public class VehicleSprite4Dir : MonoBehaviour
{
    [System.Serializable]
    public class SpriteSet4Dir
    {
        public Sprite up;
        public Sprite right;
        public Sprite down;
        public Sprite left;
    }

    [Header("References")]
    [SerializeField] private VehicleController2D controller;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Drive Sprites")]
    [SerializeField] private SpriteSet4Dir driveSprites;

    [Header("Brake Sprites")]
    [SerializeField] private SpriteSet4Dir brakeSprites;

    [Header("Reverse Sprites")]
    [SerializeField] private SpriteSet4Dir reverseSprites;

    private int currentDirIndex = -1;
    private VehicleSpriteState currentState;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponentInParent<VehicleController2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        UpdateSprite();
        UpdateVisualRotation();
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null || controller == null)
            return;

        float z = NormalizeAngle(controller.transform.eulerAngles.z);
        int dirIndex = Get4DirIndex(z);
        VehicleSpriteState state = controller.SpriteState;

        if (dirIndex == currentDirIndex && state == currentState)
            return;

        currentDirIndex = dirIndex;
        currentState = state;

        SpriteSet4Dir set = GetSpriteSet(state);
        Sprite sprite = GetSpriteFromSet(set, dirIndex);

        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }

    private void UpdateVisualRotation()
    {
        if (spriteRenderer == null || controller == null)
            return;

        float z = NormalizeAngle(controller.transform.eulerAngles.z);
        float baseAngle = GetBaseAngleByDir(currentDirIndex);
        float visualAngle = Mathf.DeltaAngle(baseAngle, z);

        spriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, visualAngle);
    }

    private SpriteSet4Dir GetSpriteSet(VehicleSpriteState state)
    {
        switch (state)
        {
            case VehicleSpriteState.Brake:
                return brakeSprites;

            case VehicleSpriteState.Reverse:
                return reverseSprites;

            default:
                return driveSprites;
        }
    }

    private Sprite GetSpriteFromSet(SpriteSet4Dir set, int dirIndex)
    {
        if (set == null)
            return null;

        switch (dirIndex)
        {
            case 0: return set.up;
            case 1: return set.right;
            case 2: return set.down;
            case 3: return set.left;
            default: return set.up;
        }
    }

    private int Get4DirIndex(float z)
    {
        z = NormalizeAngle(z);

        if (z >= 315f || z < 45f)
            return 0; // Up

        if (z >= 45f && z < 135f)
            return 3; // Left

        if (z >= 135f && z < 225f)
            return 2; // Down

        return 1; // Right
    }

    private float GetBaseAngleByDir(int dirIndex)
    {
        switch (dirIndex)
        {
            case 0: return 0f;    // Up
            case 1: return 270f;  // Right
            case 2: return 180f;  // Down
            case 3: return 90f;   // Left
            default: return 0f;
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f)
            angle += 360f;
        return angle;
    }
}
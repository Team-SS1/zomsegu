using UnityEngine;

public class VehicleSprite4Dir : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("4 Direction Sprites")]
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite rightSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite leftSprite;

    [Header("Options")]
    [SerializeField] private bool flipRightFromLeft = false;

    private int currentDirIndex = -1;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        UpdateSpriteByRotation();

        if (spriteRenderer != null)
        {
            float z = NormalizeAngle(transform.eulerAngles.z);
            float baseAngle = GetBaseAngleByDir(currentDirIndex);
            float visualAngle = Mathf.DeltaAngle(baseAngle, z);

            spriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, visualAngle);
        }
    }

    private void UpdateSpriteByRotation()
    {
        float z = transform.eulerAngles.z;

        int dirIndex = Get4DirIndex(z);

        if (dirIndex == currentDirIndex)
            return;

        currentDirIndex = dirIndex;

        switch (dirIndex)
        {
            case 0: // Up
                SetSprite(upSprite, false);
                break;

            case 1: // Right
                if (flipRightFromLeft)
                    SetSprite(leftSprite, true);
                else
                    SetSprite(rightSprite, false);
                break;

            case 2: // Down
                SetSprite(downSprite, false);
                break;

            case 3: // Left
                SetSprite(leftSprite, false);
                break;
        }
    }

    private int Get4DirIndex(float z)
    {
        z = NormalizeAngle(z);

        // 45도 기준으로 방향 전환
        // 315~45 = Up
        // Unity 2D에서 transform.up 기준:
        // z = 0   -> Up
        // z = 90  -> Left
        // z = 180 -> Down
        // z = 270 -> Right

        if (z >= 315f || z < 45f)
            return 0; // Up

        if (z >= 45f && z < 135f)
            return 3; // Left

        if (z >= 135f && z < 225f)
            return 2; // Down

        return 1; // Right
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f)
            angle += 360f;
        return angle;
    }

    private void SetSprite(Sprite sprite, bool flipX)
    {
        if (spriteRenderer == null || sprite == null)
            return;

        spriteRenderer.sprite = sprite;
        spriteRenderer.flipX = flipX;
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
}
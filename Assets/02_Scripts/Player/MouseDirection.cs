using UnityEngine;
using UnityEngine.InputSystem;

public class MouseDirection
{
    private Camera cam;

    public MouseDirection(Camera cam)
    {
        this.cam = cam;
    }

    public Vector2 GetLocalMouseDirection(Vector2 origin)
    {
        Vector3 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        Vector2 dir = (Vector2)mouseWorld - origin;

        if (dir.sqrMagnitude < 0.001f)
            return Vector2.zero;

        return dir.normalized;
    }
}
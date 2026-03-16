using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [SerializeField] private Camera cam;
    public MouseDirection Mousedir { get; private set; }  

    private void Start()
    {
        Debug.Log($"[PlayerAim Awake] cam field = {cam}");
        Debug.Log($"[PlayerAim Awake] Camera.main = {Camera.main}");
        if (cam == null)
        {
            cam = Camera.main;
        }
        Mousedir = new MouseDirection(cam);
    }
}
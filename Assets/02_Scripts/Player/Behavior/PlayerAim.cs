using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    public MouseDirection mouseDir { get; private set; }  

    private void Awake()
    {
        mouseDir = new MouseDirection(Camera.main);
    }
}
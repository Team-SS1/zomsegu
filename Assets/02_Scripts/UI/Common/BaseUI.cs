using UnityEngine;

public abstract class BaseUI : MonoBehaviour
{
    // ===== Unity API ===== 
    private void Awake()
    {
        AwakeInternal();
    }

    private void OnDestroy()
    {
        DestroyInternal();
    }

    private void OnEnable()
    {
        EnableInternal();
    }

    private void OnDisable()
    {
        DisableInternal();
    }

    protected virtual void AwakeInternal() { }

    protected virtual void DestroyInternal() { }

    protected virtual void EnableInternal() { }

    protected virtual void DisableInternal() { }
}

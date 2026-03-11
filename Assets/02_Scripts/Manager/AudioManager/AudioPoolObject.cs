using AudioEnum;
using System;
using UnityEngine;

public class AudioPoolObject : MonoBehaviour
{
    private IAudioInstance instance;
    private Transform target;
    private AudioPriority priority;
    private bool useSpatial;

    public IAudioInstance Instance => instance;
    public AudioPriority Priority => priority;

    public event Action<AudioPoolObject> OnEnd;

    #region Unity API
    private void OnEnable()
    {
        EnableInteral();
    }

    private void Update()
    {
        if (instance == null)
        {
            Destroy(gameObject);
            return;
        }

        // 플레이 완료
        if (!instance.IsPaused && !instance.IsPlaying)
        {
            gameObject.SetActive(false);
            return;
        }

        // 거리 기반
        if (useSpatial)
        {
            if (target == null)
            {
                gameObject.SetActive(false);
                return;
            }

            instance.SetPosition(target.position);
        }
    }

    private void OnDisable()
    {
        DisableInteral();
    }

    private void OnDestroy()
    {
        OnEnd = null;
    }
    #endregion

    public void Create(IAudioInstance instance)
    {
        this.instance = instance;
    }

    public void Init(AudioPriority priority, Transform target = null)
    {
        useSpatial = target != null;
        this.priority = priority;
        this.target = target;
    }

    private void EnableInteral()
    {
    }

    private void DisableInteral()
    {
        instance?.Stop();
        OnEnd?.Invoke(this);
    }
}

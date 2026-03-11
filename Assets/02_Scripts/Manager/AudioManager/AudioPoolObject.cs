using AudioEnum;
using UnityEngine;

public class AudioPoolObject : MonoBehaviour
{
    private IAudioInstance instance;
    private Transform target;
    private AudioPriority priority;
    private bool useSpatial;

    public IAudioInstance Instance => instance;
    public AudioPriority Priority => priority;

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
            instance.SetPosition(target.position);

            if (target == null)
            {
                gameObject.SetActive(false);
                return;
            }
        }
    }

    private void OnDisable()
    {
        DisableInteral();
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
    }
}

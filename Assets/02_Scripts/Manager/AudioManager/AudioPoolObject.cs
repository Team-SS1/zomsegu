using AudioEnum;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPoolObject : MonoBehaviour
{
    private AudioSource source;
    private Transform poolRoot;
    private Transform followTarget;

    private AudioPriority priority;
    private int playId;
    private bool isPaused;

    public AudioSource Source => source;
    public AudioPriority Priority => priority;
    /// <summary>
    /// Pool이 꽉 차면 priority가 낮은 object를 반환하고 다시 쓸 수 있다.
    /// 이때 이전 WaitAsync가 새로 재생 중인 sound를 반환하지 않도록 재생마다 값이 바뀐다.
    /// </summary>
    public int PlayId => playId;
    public bool IsPaused => isPaused;

    private void OnDisable()
    {
        ResetForPool();
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;

        transform.position = followTarget.position;
    }

    public void Create(AudioSource source)
    {
        this.source = source;
        poolRoot = transform.parent;
    }

    public void ResetForPool()
    {
        if (source == null) return;

        isPaused = false;
        followTarget = null;
        source.Stop();
        source.clip = null;
        transform.SetParent(poolRoot, false);
        transform.localPosition = Vector3.zero;
    }

    public void Play(AudioPriority priority)
    {
        StartPlay(priority);
    }

    /// <summary>
    /// Loop SFX가 지정 Transform의 world position을 따라가게 한다.
    /// </summary>
    public void PlayLoop(AudioPriority priority, Transform parent)
    {
        if (parent == null) return;

        followTarget = parent;
        transform.position = parent.position;
        StartPlay(priority);
    }

    private void StartPlay(AudioPriority priority)
    {
        this.priority = priority;
        playId++;
        isPaused = false;
        source.Play();
    }

    public void Pause()
    {
        if (isPaused) return;

        isPaused = true;
        if (source.isPlaying)
        {
            source.Pause();
        }
    }

    public void Resume()
    {
        if (!isPaused) return;

        isPaused = false;
        source.UnPause();
    }

    public float GetDuration()
    {
        float clipLength = source.clip != null ? source.clip.length : 0f;
        return clipLength / Mathf.Max(Mathf.Abs(source.pitch), 0.0001f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        AudioManager audioManager = FindAnyObjectByType<AudioManager>();
        if (audioManager == null || !audioManager.AlwaysDrawSpatialGizmo) return;

        DrawSpatialGizmo(audioManager);
    }

    private void OnDrawGizmosSelected()
    {
        DrawSpatialGizmo(FindAnyObjectByType<AudioManager>());
    }

    private void DrawSpatialGizmo(AudioManager audioManager)
    {
        if (audioManager == null) return;

        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null || audioSource.spatialBlend <= 0f) return;

        Gizmos.color = audioManager.SpatialMinDistanceGizmoColor;
        Gizmos.DrawWireSphere(transform.position, audioSource.minDistance);
        Gizmos.color = audioManager.SpatialMaxDistanceGizmoColor;
        Gizmos.DrawWireSphere(transform.position, audioSource.maxDistance);
    }
#endif
}

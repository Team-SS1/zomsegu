using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Monster))]
public class MonsterPerception : MonoBehaviour
{
    private Monster monster;

    [Header("Target Tracking")]
    private float timeSinceSeen;

    [Header("Sound Tracking")]
    public Vector2 lastHeardSoundPos;
    public GameObject lastSoundSource;
    public bool hasSoundTarget;

    private void Awake()
    {
        monster = GetComponent<Monster>();
    }

    private void OnEnable()
    {
        if (WorldEventManager.Instance != null)
            WorldEventManager.Instance.OnSoundEvent += OnSoundEventReceived;
    }

    private void OnDisable()
    {
        if (WorldEventManager.Instance != null)
            WorldEventManager.Instance.OnSoundEvent -= OnSoundEventReceived;
    }

    public void ResetPerception()
    {
        timeSinceSeen = 0f;
        lastHeardSoundPos = Vector2.zero;
        lastSoundSource = null;
        hasSoundTarget = false;
    }

    public void OnTargetSeen(Transform seenTarget)
    {
        if (monster.IsDead)
            return;

        monster.SetTarget(seenTarget);
        timeSinceSeen = 0f;

        monster.MarkEverAggroed();

        if (monster.Initial != null && monster.Initial.IsFakeDieActiveAndCanWakeUp())
        {
            monster.Initial.StartWakeUp();
            return;
        }

        if (monster.currentState != MonsterStateType.Aggro)
            monster.AI?.OnTargetDetected();
    }

    public void OnTargetNotSeen()
    {
        if (monster.IsDead)
            return;

        if (monster.currentState != MonsterStateType.Aggro)
            return;

        timeSinceSeen += Time.deltaTime;

        if (monster.target != null && monster.stat != null)
        {
            float dist = Vector2.Distance(transform.position, monster.target.position);
            if (dist > monster.stat.LoseAggroDistance)
            {
                monster.AI?.OnTargetLost();
                return;
            }
        }

        if (monster.stat != null && timeSinceSeen >= monster.stat.LoseAggroTimeSec)
            monster.AI?.OnTargetLost();
    }

    public void ClearTargetAndReturnIdle()
    {
        monster.SetTarget(null);
        timeSinceSeen = 0f;
        monster.StopMove();
    }

    private void OnSoundEventReceived(SoundEvent soundEvent)
    {
        if (monster.IsDead)
            return;

        if (monster.IsEating)
            return;

        if (soundEvent.source == null || soundEvent.source == gameObject)
            return;

        if (soundEvent.radius <= 0f)
            return;

        if (monster.currentState == MonsterStateType.Aggro)
            return;

        if (monster.IsWakeUp || monster.IsFakeDie)
            return;

        float dist = Vector2.Distance(transform.position, soundEvent.position);
        if (dist > soundEvent.radius)
            return;

        lastHeardSoundPos = soundEvent.position;
        lastSoundSource = soundEvent.source;
        hasSoundTarget = true;

        monster.AI?.OnSoundDetected();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 월드에 발생한 '행동 결과 이벤트'
/// 실제 소리가 아니라, 몬스터가 감지 가능한 행동 정보
/// </summary>
public struct SoundEvent
{
    public Vector2 position;   // 이벤트가 발생한 위치
    public float radius;       // 몬스터가 감지 가능한 범위
    public GameObject source;  // 이벤트를 발생시킨 주체 (Player, Projectile 등)

    public SoundEvent(Vector2 position, float radius, GameObject source)
    {
        this.position = position;
        this.radius = radius;
        this.source = source;
    }
}

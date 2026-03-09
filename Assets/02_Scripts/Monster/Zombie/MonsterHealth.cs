using UnityEngine;

public class MonsterHealth : MonoBehaviour
{
    private Monster monster;
    private float currentHP;

    public float CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0f;

    private void Awake()
    {
        monster = GetComponent<Monster>();
    }

    public void Init(float maxHP)
    {
        currentHP = maxHP;
    }

    public void ApplyDamage(float amount)
    {
        if (IsDead)
            return;

        currentHP -= amount;

        if (currentHP <= 0f)
        {
#if UNITY_EDITOR
            Debug.Log($"[Health] {monster.name} HP zero");
#endif
            currentHP = 0f;
            monster.Die();
        }
    }
}
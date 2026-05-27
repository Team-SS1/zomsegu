using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(WildDog))]
public class WildDogHealth : MonoBehaviour
{
    private WildDog dog;
    private float currentHP;

    public float CurrentHP => currentHP;
    public bool IsDead => currentHP <= 0f;

    private void Awake()
    {
        dog = GetComponent<WildDog>();
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
            dog.Die();
    }
}
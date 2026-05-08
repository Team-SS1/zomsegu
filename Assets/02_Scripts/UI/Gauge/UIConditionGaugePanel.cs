using UnityEngine;
using EventEnum;
using PlayerEnum;

public class UIConditionGaugePanel : MonoBehaviour
{
    [Header("Context")] 
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext; // 지금 구분 못함

    [Header("Gauge")]
    [SerializeField] private UICircleConditionGauge hungerGauge;
    [SerializeField] private UICircleConditionGauge thirstGauge;
    [SerializeField] private UICircleConditionGauge tiredGauge;

    [Header("Condition Reference")]
    [SerializeField] private PlayerCondition playerCondition; // 임시로 넣어둔 참조, 나중에 PlayerManager 또는 PlayerDataManager에서 가져오는 식으로 바꿔야 할듯

    private void OnEnable()
    {
        EventManager.Subscribe<PlayerCondition>(EventKey.OnHungerChanged, OnHungerChanged);
        EventManager.Subscribe<PlayerCondition>(EventKey.OnThirstChanged, OnThirstChanged);
        EventManager.Subscribe<PlayerCondition>(EventKey.OnTiredChanged, OnTiredChanged);

        RefreshAll();
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<PlayerCondition>(EventKey.OnHungerChanged, OnHungerChanged);
        EventManager.UnSubscribe<PlayerCondition>(EventKey.OnThirstChanged, OnThirstChanged);
        EventManager.UnSubscribe<PlayerCondition>(EventKey.OnTiredChanged, OnTiredChanged);
    }
    private void OnHungerChanged(PlayerCondition condition)
    {
        if (condition != playerCondition)
            return;

        RefreshGauge(AbnormalType.Hunger, hungerGauge);
    }
    private void OnThirstChanged(PlayerCondition condition)
    {
        if (condition != playerCondition)
            return;

        RefreshGauge(AbnormalType.Thirst, thirstGauge);
    }
    private void OnTiredChanged(PlayerCondition condition)
    {
        if (condition != playerCondition)
            return;

        RefreshGauge(AbnormalType.Tired, tiredGauge);
    }
    private void RefreshAll()
    {
        RefreshGauge(AbnormalType.Hunger, hungerGauge);
        RefreshGauge(AbnormalType.Thirst, thirstGauge);
        RefreshGauge(AbnormalType.Tired, tiredGauge);
    }
    private void RefreshGauge(AbnormalType abnormalType, UICircleConditionGauge gauge)
    {
        if (gauge == null) return;

        if (playerCondition == null)
        {
            gauge.SetValue(0, 0);
            return;
        }
        int currentValue = Mathf.RoundToInt(playerCondition.GetValue(abnormalType));
        int maxValue = Mathf.RoundToInt(playerCondition.GetMaxValue(abnormalType));

        gauge.SetValue(currentValue, maxValue);
    }
}

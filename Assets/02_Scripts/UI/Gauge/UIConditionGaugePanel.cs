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
        if (!IsCurrentInspectPlayer(condition))
            return;

        RefreshGauge(AbnormalType.Hunger, hungerGauge);
    }
    private void OnThirstChanged(PlayerCondition condition)
    {
        if (!IsCurrentInspectPlayer(condition))
            return;

        RefreshGauge(AbnormalType.Thirst, thirstGauge);
    }
    private void OnTiredChanged(PlayerCondition condition)
    {
        if (!IsCurrentInspectPlayer(condition))
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

        PlayerCondition playerCondition = GetCurrentInspectCondition();

        if (playerCondition == null)
        {
            gauge.SetValue(0, 0);
            return;
        }
        int currentValue = Mathf.RoundToInt(playerCondition.GetValue(abnormalType));
        int maxValue = Mathf.RoundToInt(playerCondition.GetMaxValue(abnormalType));

        gauge.SetValue(currentValue, maxValue);
    }
    private PlayerCondition GetCurrentInspectCondition()
    {
        if(PlayerManager.Instance == null) return null;

        PlayerType playerType = GetCurrentInspectPlayer();
        return PlayerManager.Instance.GetCurrentPlayerCondition(playerType);
    }
    private PlayerType GetCurrentInspectPlayer()
    {
        if(selectedCharacterContext != null)
            return selectedCharacterContext.CurrentInspectPlayer;

        if(PlayerManager.Instance != null)
            return PlayerManager.Instance.CurrentActivePlayer;

        return PlayerType.Player_SHIN; // 기본값
    }
    private bool IsCurrentInspectPlayer(PlayerCondition playerCondition)
    {
        if(playerCondition == null) return false;

        PlayerType playerType = GetCurrentInspectPlayer();
        return PlayerManager.Instance.GetCurrentPlayerCondition(playerType);
    }
}

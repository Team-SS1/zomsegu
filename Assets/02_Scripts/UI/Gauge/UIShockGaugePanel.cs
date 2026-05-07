using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventEnum;
using PlayerEnum;

public class UIShockGaugePanel : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerCondition playerCondition;
    [SerializeField] private UIShockGauge shockGauge;

    private void OnEnable()
    {
        EventManager.Subscribe(EventKey.OnShockChanged, OnShockChanged);
        RefreshShock();
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe(EventKey.OnShockChanged, OnShockChanged);
    }
    private void OnShockChanged()
    {
        RefreshShock();
    }
    private void RefreshShock()
    {
        if (shockGauge == null) return;
        if(playerCondition == null)
        {
            shockGauge.SetValue(0, 0);
            return;
        }

        int currentValue = Mathf.RoundToInt(playerCondition.GetValue(AbnormalType.Shock));
        int maxValue = Mathf.RoundToInt(playerCondition.GetMaxValue(AbnormalType.Shock));

        shockGauge.SetValue(currentValue, maxValue);
    }
}

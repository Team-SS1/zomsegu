using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using EventEnum;
using ItemEnum;
using UnityEngine.Diagnostics;
using UIEnum;

public class UIConditionStatusPanel : MonoBehaviour
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;

    [Header("Tag Views")]
    [SerializeField] private UIStatusTagView hungerTagView;
    [SerializeField] private UIStatusTagView thirstTagView;
    [SerializeField] private UIStatusTagView tiredTagView;
    [SerializeField] private UIStatusTagView shockTagView;
    [SerializeField] private UIStatusTagView injuryTagView;

    private void OnEnable()
    {
        EventManager.Subscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectCharacterChanged);

        EventManager.Subscribe<PlayerCondition>(EventKey.OnHungerChanged, OnConditionChanged);
        EventManager.Subscribe<PlayerCondition>(EventKey.OnThirstChanged, OnConditionChanged);
        EventManager.Subscribe<PlayerCondition>(EventKey.OnTiredChanged, OnConditionChanged);
        EventManager.Subscribe<PlayerCondition>(EventKey.OnShockChanged, OnConditionChanged);
        EventManager.Subscribe<PlayerCondition>(EventKey.OnInjuryChanged, OnConditionChanged);

        Refresh();
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectCharacterChanged);

        EventManager.UnSubscribe<PlayerCondition>(EventKey.OnHungerChanged, OnConditionChanged);
        EventManager.UnSubscribe<PlayerCondition>(EventKey.OnThirstChanged, OnConditionChanged);
        EventManager.UnSubscribe<PlayerCondition>(EventKey.OnTiredChanged, OnConditionChanged);
        EventManager.UnSubscribe<PlayerCondition>(EventKey.OnShockChanged, OnConditionChanged);
        EventManager.UnSubscribe<PlayerCondition>(EventKey.OnInjuryChanged, OnConditionChanged);
    }
    private void OnInspectCharacterChanged(PlayerType playerType)
    {
        Refresh();
    }
    private void OnConditionChanged(PlayerCondition condition)
    {
        PlayerCondition currentCondition = GetCurrentInspectCondition();

        if (currentCondition != condition)
            return;

        Refresh();
    }
    private void Refresh()
    { 
        if(PlayerManager.Instance == null) return;
        Player player = PlayerManager.Instance.GetPlayer(selectedCharacterContext.CurrentInspectPlayer);
        PlayerCondition condition = GetCurrentInspectCondition();

        if (player == null || condition == null)
        {
            ClearAll();
            return;
        }

        RefreshHunger(condition);
        RefreshThirst(condition);
        RefreshTired(condition);
        RefreshShock(condition);
        RefreshInjury(player, condition);
    }
    private void RefreshHunger(PlayerCondition condition)
    {
        float hunger = condition.GetValue(AbnormalType.Hunger);

        UIStatusTagState state;

        if (hunger <= 0)
            state = UIStatusTagState.Danger; // 기아
        else if (hunger <= 3)
            state = UIStatusTagState.Active; // 허기짐
        else
            state = UIStatusTagState.Inactive; // 어두운 허기짐

        if(hungerTagView != null)
            hungerTagView.SetState(state);
    }
    private void RefreshThirst(PlayerCondition condition)
    {
        float thirst = condition.GetValue(AbnormalType.Thirst);

        UIStatusTagState state;

        if(thirst <= 0)
            state = UIStatusTagState.Danger; // 탈수
        else if(thirst <= 3)
            state = UIStatusTagState.Active; // 갈증
        else
            state = UIStatusTagState.Inactive; // 어두운 갈증

        if(thirstTagView != null)
            thirstTagView.SetState(state);
    }
    private void RefreshTired(PlayerCondition condition)
    {
        float tired = condition.GetValue(AbnormalType.Tired);

        UIStatusTagState state;

        if(tired <= 3)
            state = UIStatusTagState.Active; // 피곤함
        else
            state = UIStatusTagState.Inactive; // 어두운 피곤함

        if(tiredTagView != null)  
            tiredTagView.SetState(state);
    }
    private void RefreshShock(PlayerCondition condition)
    {
        float shock = condition.GetValue(AbnormalType.Shock);

        UIStatusTagState state;

        if (shock >= 3)
            state = UIStatusTagState.Active; // 쇼크
        else
            state = UIStatusTagState.Inactive; // 어두운 쇼크

        if(shockTagView != null)
            shockTagView.SetState(state);
    }
    private void RefreshInjury(Player player, PlayerCondition condition)
    {
        float injury = condition.GetValue(AbnormalType.Injury);

        UIStatusTagState state;

        //이따가 행동 불능 집어 넣으세요

        if (player.IsDead)
            state = UIStatusTagState.Dead; // 사망
        else if (injury >= 2)
            state = UIStatusTagState.Severe; // 심한 부상
        else if (injury >= 1)
            state = UIStatusTagState.Active; // 부상
        else
            state = UIStatusTagState.Inactive; // 어두운 부상

        if(injuryTagView != null)
            injuryTagView.SetState(state);
    }
            private PlayerCondition GetCurrentInspectCondition()
            {
                PlayerType playerType = selectedCharacterContext.CurrentInspectPlayer;

                if (PlayerManager.Instance == null)
                    return null;

                return PlayerManager.Instance.GetCurrentPlayerCondition(playerType);
            }
    private void ClearAll()
    {
        if (hungerTagView != null)
            hungerTagView.Clear();
        if(thirstTagView != null)
            thirstTagView.Clear();
        if(tiredTagView != null) 
            tiredTagView.Clear();
        if (shockTagView != null)
            shockTagView.Clear();
        if(injuryTagView != null)
            injuryTagView.Clear();
    }
}

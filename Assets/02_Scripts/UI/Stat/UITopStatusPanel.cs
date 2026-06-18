using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using EventEnum;
using UIEnum;
using ItemEnum;
public class UITopStatusPanel : MonoBehaviour
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;

    [Header("UIStatView")]
    [SerializeField] private UIStatView attackStatView;
    [SerializeField] private UIStatView attackSpeedStatView;
    [SerializeField] private UIStatView moveSpeedStatView;
    [SerializeField] private UIStatView staminaStatView;

    private void OnEnable()
    {
        EventManager.Subscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectCharacterChanged);
        EventManager.Subscribe<PlayerType>(EventKey.StatChanged, OnStatChanged);
        EventManager.Subscribe(EventKey.OnStaminaChanged, OnStaminaChanged); // 지금 스태미너 쪽에 매개변수 없이 이벤트 쏘고 있음(나중에 고치셈)

        Refresh();
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectCharacterChanged);
        EventManager.UnSubscribe<PlayerType>(EventKey.StatChanged, OnStatChanged);
        EventManager.UnSubscribe(EventKey.OnStaminaChanged, OnStaminaChanged);
    }
    private void OnInspectCharacterChanged(PlayerType player)
    {
        Refresh();
    }
    private void OnStatChanged(PlayerType player)
    {
        if (player != selectedCharacterContext.CurrentInspectPlayer) return;
        
        Refresh();
    }
    private void OnStaminaChanged()
    {
        Refresh();
    }
    private void Refresh()
    {
        Player player = PlayerManager.Instance.GetPlayer(selectedCharacterContext.CurrentInspectPlayer);

        if(player == null || player.data == null)
        {
            ClearAll();
            return;
        }

        RefreshAttack(player);
        RefreshAttackSpeed(player);
        RefreshMoveSpeed(player);
        RefreshStamina(player);
    }
    private void RefreshAttack(Player player)
    {
        float weaponAttack = 0;

        if(TryGetEquippedWeaponStat(out WeaponStat weaponStat))
            weaponAttack = weaponStat.Attack;

        float baseAttack = player.data.Stat.BaseAttack + weaponAttack;
        float currentAttack = player.CurrentAttack;

        UIStatViewState state = GetViewState(currentAttack, baseAttack, true);
        string text = currentAttack.ToString("0.#");

        if(attackStatView != null)
            attackStatView.SetValue(text, state);
    }
    private void RefreshAttackSpeed(Player player)
    {
        float currentAttackSpeed = player.CurrentAttackSpeed;

        float baseAttackSpeed = 0;
        
        if(TryGetEquippedWeaponStat(out WeaponStat weaponStat))
            baseAttackSpeed = weaponStat.AttackSpeed;

        UIStatViewState state = GetViewState(currentAttackSpeed, baseAttackSpeed, false);
        string text = currentAttackSpeed.ToString("0.#");

        if(attackSpeedStatView != null)
            attackSpeedStatView.SetValue(text, state);
    }
    private void RefreshMoveSpeed(Player player)
    {
        float currentMoveSpeed = player.CurrentMovement;
        float baseMoveSpeed = 100f; // 기본 이동 속도 수치, 필요에 따라 조정

        UIStatViewState state = GetViewState(currentMoveSpeed, baseMoveSpeed, true);
        string text = currentMoveSpeed.ToString("0.#");

        if(moveSpeedStatView != null)
            moveSpeedStatView.SetValue(text, state);
    }
    private void RefreshStamina(Player player)
    {
        float currentStamina = player.CurrentStamina;
        float maxStamina = player.MaxStamina;
        float baseMax = player.data.Stat.BaseMaxStamina; // 100f;

        UIStatViewState state = GetViewState(maxStamina, baseMax, true);
        string text = $"{currentStamina:0.#} / {maxStamina:0.#}";

        if(staminaStatView != null)
            staminaStatView.SetValue(text, state);
    }
    private UIStatViewState GetViewState(float current, float baseValue, bool isHigherBetter)
    {
        if(Mathf.Approximately(current, baseValue))
            return UIStatViewState.Normal;

        if (current > baseValue)
            return isHigherBetter ? UIStatViewState.Up : UIStatViewState.Down;


        return isHigherBetter ? UIStatViewState.Down : UIStatViewState.Up;
    }
    private void ClearAll()
    {
        if(attackStatView != null)
            attackStatView.Clear();
        if(attackSpeedStatView != null)
            attackSpeedStatView.Clear();
        if(moveSpeedStatView != null)
            moveSpeedStatView.Clear();
        if(staminaStatView != null)
            staminaStatView.Clear();
    }
    private bool TryGetEquippedWeaponStat(out WeaponStat weaponStat)
    {
        weaponStat = null;
        
        PlayerType playerType = selectedCharacterContext.CurrentInspectPlayer;

        ItemStack equippedWeapon = EquipmentQueryService.GetEquippedInstance(playerType, EquipSlotType.Weapon);

        if (equippedWeapon == null)
            return false;
        
        int itemId  = equippedWeapon.itemId;

        return ItemDB.TryGetWeaponStat(itemId, out weaponStat);
    }
}

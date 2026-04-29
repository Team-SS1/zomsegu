using TMPro;
using UnityEngine;
using PlayerEnum;
using EventEnum;

public class UICalculateWeightVolume : MonoBehaviour
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private TextMeshProUGUI weightText;

    [Header("Color")]
    [SerializeField] private Color normalColor = new Color(0x11, 0x11, 0x11, 0xFF);
    [SerializeField] private Color overloadColor = new Color(0xF4, 0x43, 0x36, 0xFF);

    private void OnEnable()
    {
        EventManager.Subscribe<PlayerType>(EventKey.InventoryChanged, OnInventoryChanged);
        EventManager.Subscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectorChanged);

        Refresh(selectedCharacterContext.CurrentInspectPlayer);
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<PlayerType>(EventKey.InventoryChanged, OnInventoryChanged);
        EventManager.UnSubscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectorChanged);
    }
    private void OnInventoryChanged(PlayerType playerType)
    {
        if (playerType != selectedCharacterContext.CurrentInspectPlayer) return;
        Refresh(playerType);
    }
    private void OnInspectorChanged(PlayerType playerType)
    {
        Refresh(playerType);
    }
    private void Refresh(PlayerType playerType)
    {
        Inventory inventory = GetInventory(playerType);
        if (inventory == null)
        {
            if (volumeText != null) volumeText.text = "";
            if (weightText != null) weightText.text = "";
            return;
        }

        if(volumeText != null)
        {
            volumeText.text = $"{inventory.CurrentVolume:0.#}/{inventory.MaxVolume:0.#}";
            volumeText.color = normalColor;
        }
        if(weightText != null)
        {
            weightText.text = $"{inventory.CurrentWeight:0.#}/{inventory.MaxWeight:0.#}kg";
            weightText.color = inventory.CurrentWeight > inventory.MaxWeight ? overloadColor : normalColor;
        }
    }
    private Inventory GetInventory(PlayerType playerType)
    {
        PlayerData playerData = PlayerDataManager.Instance.GetPlayerData(playerType);
        return playerData !=null ? playerData.Inventory : null;
    }
}

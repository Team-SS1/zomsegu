using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ItemEnum;
using PlayerEnum;

public class UIItemAmountPopup : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject rootPanel;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI currentAmountText;

    [Header("Input")]
    [SerializeField] private TMP_InputField amountInputField;

    [Header("Button")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private ItemAmountPopupMode currentMode = ItemAmountPopupMode.None;
    private SlotRef currentSlot;
    private PlayerType currentPlayerType;
    private int maxAmount;

    private void Awake()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnClickConfirm);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Close);

        if (amountInputField != null)
            amountInputField.onValueChanged.AddListener(OnInputValueChanged);

        Close();
    }

    public void OpenForGive(SlotRef from, PlayerType playerType, int currentAmount)
    {
        if (currentAmount <= 0) return;

        currentMode = ItemAmountPopupMode.GiveToOtherPlayer;
        currentSlot = from;
        currentPlayerType = playerType;
        maxAmount = currentAmount;

        if (titleText != null)
            titleText.text = "아이템을 전달하시겠습니까?";

        if (currentAmountText != null)
            currentAmountText.text = $"현재 보유개수 : {currentAmount}개";

        if(amountInputField != null)
        {
            amountInputField.text = currentAmount.ToString();
            amountInputField.ActivateInputField();
            amountInputField.Select();
        }

        if(rootPanel != null)
            rootPanel.SetActive(true);
    }

    public void OpenForDrop(SlotRef from, int currentAmount)
    {
        if(currentAmount <= 0) return;

        currentMode = ItemAmountPopupMode.DropToWorld;
        currentSlot = from;
        currentPlayerType = default;
        maxAmount = currentAmount;

        if (titleText != null)
            titleText.text = "아이템을 버리겠습니까?";

        if (currentAmountText != null)
            currentAmountText.text = $"현재 보유개수 : {currentAmount}개";

        if(amountInputField != null)
        {
            amountInputField.text = currentAmount.ToString();
            amountInputField.ActivateInputField();
            amountInputField.Select();
        }

        if(rootPanel != null)
            rootPanel.SetActive(true);
    }
    public void Close()
    {
        currentMode = ItemAmountPopupMode.None;
        maxAmount = 0;

        if(rootPanel != null)
            rootPanel.SetActive(false);
    }

    private void OnInputValueChanged(string value)
    {
        if (amountInputField == null) return;
        if(string.IsNullOrWhiteSpace(value)) return;

        if(!int.TryParse(value, out int parsed))
        {
            amountInputField.text = "1";
            return;
        }

        parsed = Mathf.Clamp(parsed, 1, maxAmount);

        if(amountInputField.text != parsed.ToString())
            amountInputField.text = parsed.ToString();
    }

    private void OnClickConfirm()
    {
        int amount = GetValidAmount();
        if (amount <= 0) return;

        switch (currentMode)
        {
            case ItemAmountPopupMode.GiveToOtherPlayer:
                ItemTransferService.TryGiveToOtherPlayer(currentSlot, currentPlayerType, amount);
                break;

            case ItemAmountPopupMode.DropToWorld:
                ItemTransferService.TryDropOutside(currentSlot,amount);
                break;
        }
        Close();
    }
    private int GetValidAmount()
    {
        if(amountInputField == null) return 0;
        if(!int.TryParse(amountInputField.text, out int amount)) return 0;

        return Mathf.Clamp(amount, 1, maxAmount);
    }
}

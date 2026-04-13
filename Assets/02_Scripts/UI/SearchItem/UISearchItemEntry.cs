using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Rendering.Universal;

public class UISearchItemEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler //아이템 한칸 UI
{
    [Header("UI")]
    [SerializeField] private Button rootButton;
    [SerializeField] private Button pickupButton;

    [SerializeField] private Image bgImage;
    [SerializeField] private Image iconImage;

    [SerializeField] private TextMeshProUGUI nameTXT; //수량은 차피 nameTXT로 들어감
    [SerializeField] private TextMeshProUGUI typeTXT;

    [Header("BG - Normal")]
    [SerializeField] private Sprite normalBG;
    [SerializeField] private Sprite rareBG;
    [SerializeField] private Sprite epicBG;
    [SerializeField] private Sprite uniqueBG;

    [Header("BG - Selected")]
    [SerializeField] private Sprite normalSelectedBG;
    [SerializeField] private Sprite rareSelectedBG;
    [SerializeField] private Sprite epicSelectedBG;
    [SerializeField] private Sprite uniqueSelectedBG;

    private UISearchWindow owner;
    private SearchDisplayEntry entryData;

    private float lastClickTime = -10f;
    private const float DOUBLE_CLICK_DELAY = 0.25f;

    private bool isSelected;

    public SearchDisplayEntry EntryData => entryData;
    public RectTransform Rect => transform as RectTransform;

    private void Awake()
    {
        if (rootButton != null)
            rootButton.onClick.AddListener(OnClickRoot);

        if (pickupButton != null)
            pickupButton.onClick.AddListener(OnClickPickupButton);
    }

    public void SetEntry(UISearchWindow owner,  SearchDisplayEntry entryData, bool selected)
    {
        this.owner = owner;
        this.entryData = entryData;

        RefreshUI();
        SetSelected(selected);
    }
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        RefreshBackground();
    }
    private void RefreshUI()
    {
        if (entryData == null)
            return;

        if(nameTXT != null)
            nameTXT.text = SearchDisplayTextUtil.GetDisplayName(entryData);

        if (typeTXT != null)
            typeTXT.text = SearchDisplayTextUtil.GetTypeText(entryData.itemId);

        if(iconImage != null)
        {
            string iconPath = ItemDB.GetIconPath(entryData.itemId);
            Sprite iconSprite = null;

            if(!string.IsNullOrEmpty(iconPath))
                iconSprite = Resources.Load<Sprite>(iconPath);

            iconImage.sprite = iconSprite;
            iconImage.enabled = iconImage != null;
        }
        RefreshBackground();
    }
    private void RefreshBackground()
    {
        if(bgImage == null || entryData == null) return;

        bgImage.sprite = GetBackgroundSprite(entryData.itemId, isSelected);
    }
    private Sprite GetBackgroundSprite(int itemId, bool selected)
    {
        int rarity = ItemDB.GetRarity(itemId);

        return rarity switch
        {
            1 => selected ? rareSelectedBG : rareBG,
            2 => selected ? epicSelectedBG : epicBG,
            3 => selected ? uniqueSelectedBG : uniqueBG,
            _ => selected ? normalSelectedBG : normalBG
        };
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}

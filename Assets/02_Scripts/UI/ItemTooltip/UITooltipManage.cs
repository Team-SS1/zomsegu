using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemEnum;

public class UITooltipManage : MonoBehaviour
{
    [Header("Tooltip UI")]
    [SerializeField] private UIItemTooltip mainTooltip;
    [SerializeField] private UIItemTooltip compareTooltip;

    [Header("Canvas")]
    [SerializeField] private Canvas rootCanvas;

    [Header("Offset")]
    [SerializeField] private Vector2 mainOffset = new Vector2(30f, 0f);
    [SerializeField] private Vector2 compareOffset = new Vector2(20f, 0f);

    private RectTransform canvasRect;
    private Camera uiCamera;

    private void Awake()
    {
        if (rootCanvas != null)
        {
            canvasRect = rootCanvas.GetComponent<RectTransform>();

            if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                uiCamera = null;
            else
                uiCamera = rootCanvas.worldCamera;
        }
        HideAll();
    }
    public void ShowInventoryTooltip(RectTransform target, int itemId, ItemStack instance, int compareItemId = 0, ItemStack compareInstance = null)
    {
        if (target == null || itemId == 0) return;

        ItemTooltipData mainData = ItemTooltipBuilder.Build(itemId, instance, false, compareItemId);
        if(mainData == null) return;

        mainTooltip.Show(mainData);

        bool showCompare = ShouldShowCompare(itemId, compareItemId);

        if (showCompare)
        {
            ItemTooltipData compareData = ItemTooltipBuilder.Build(compareItemId, compareInstance, true, 0);

            if(compareData != null)
            {
                compareTooltip.Show(compareData);
                PlaceTooltips(target, true);
                return;
            }
        }

        compareTooltip.Hide();
        PlaceTooltips(target, false);
    }
    public void ShowEquipmentTooltip(RectTransform target, int itemId, ItemStack instance, bool isEquipped = true)
    {
        if (target == null || itemId == 0) return;
        
        ItemTooltipData data = ItemTooltipBuilder.Build(itemId, instance, isEquipped, 0);
        if(data == null) return;

        mainTooltip.Show(data);
        compareTooltip.Hide();

        PlaceTooltips(target, false);
    }

    public void HideAll()
    {
        if(mainTooltip != null)
            mainTooltip.Hide();
        if(compareTooltip != null)
            compareTooltip.Hide();
    }
    private bool ShouldShowCompare(int itemId, int compareItemId)
    {
        if (compareItemId == 0) return false;

        CommonItemData common = ItemDB.GetCommon(itemId);
        if (common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;
        return itemType == ItemType.Shoes || itemType == ItemType.Weapon || itemType == ItemType.Bag;
    }
    private void PlaceTooltips(RectTransform target, bool hasCompare)
    {
        if(canvasRect == null || mainTooltip == null) return;

        RectTransform mainRect = mainTooltip.GetComponent<RectTransform>();
        RectTransform compareRect = compareTooltip != null ? compareTooltip.GetComponent<RectTransform>() : null;

        Vector3 worldCenter = target.TransformPoint(target.rect.center);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCenter);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCamera, out Vector2 localPoint);

        bool isRightSide = screenPoint.x >= Screen.width*0.5f;

        float canvasHalfWidth = canvasRect.rect.width * 0.5f;
        float fixedX = isRightSide ? -canvasHalfWidth*0.45f : canvasHalfWidth*0.45f;

        mainRect.pivot = isRightSide ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);

        mainRect.anchoredPosition = new Vector2(fixedX, localPoint.y + mainOffset.y);
        ClampToCanvas(mainRect);

        if(hasCompare && compareRect != null && compareTooltip.gameObject.activeSelf)
        {
            compareRect.pivot = mainRect.pivot;

            float dir = isRightSide ? 1f : -1f;
            float compareX = mainRect.anchoredPosition.x + dir * (mainRect.rect.width + compareOffset.x);

            compareRect.anchoredPosition = new Vector2(compareX, mainRect.anchoredPosition.y);
            ClampToCanvas(compareRect);
        }
    }
    private void ClampToCanvas(RectTransform rect)
    {
        if(rect == null || canvasRect == null) return;

        Vector2 pos = rect.anchoredPosition;
        Vector2 size = rect.rect.size;
        Vector2 canvasSize = canvasRect.rect.size;

        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;

        pos.x = Mathf.Clamp(pos.x, -canvasSize.x * 0.5f + halfW, canvasSize.x * 0.5f - halfW);
        pos.y = Mathf.Clamp(pos.y, -canvasSize.y * 0.5f + halfH, canvasSize.y * 0.5f - halfH);

        rect.anchoredPosition = pos;
    }
}

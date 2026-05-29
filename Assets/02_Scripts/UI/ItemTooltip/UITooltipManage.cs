using UnityEngine;
using ItemEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class UITooltipManage : BaseUI
{
    [Header("Tooltip UI")]
    [SerializeField] private UIItemTooltip mainTooltip;
    [SerializeField] private UIItemTooltip compareTooltip;

    [Header("Canvas")]
    [SerializeField] private Canvas rootCanvas;

    [Header("Offset")]
    [SerializeField] private Vector2 mainOffset = new Vector2(0f, 0f);

    [Header("Layout")]
    [SerializeField] private float tooltipWidth = 280f;
    [SerializeField] private float singleCenterRatio = 0.42f;
    [SerializeField] private float groupCenterRatio = 0.42f;

    [SerializeField] private float spacing = 0f;

    [Header("Fine Tune")]
    [SerializeField] private float singleLeftNudge = 130f;   // 커서가 오른쪽 -> 툴팁은 왼쪽
    [SerializeField] private float singleRightNudge = 50f;  // 커서가 왼쪽 -> 툴팁은 오른쪽
    [SerializeField] private float groupLeftNudge = 100f;
    [SerializeField] private float groupRightNudge = 120f;

    [Header("Delay")]
    [SerializeField] private float showDelay = 0.2f;

    private Coroutine showDelayCoroutine;
    private int tooltipRequestItemId;

    private RectTransform canvasRect;
    private Camera uiCamera;

    protected override void AwakeInternal()
    {
        base.AwakeInternal();
        if(rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

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
    public void RequestInventoryTooltip(RectTransform target, int itemId, ItemStack instance, int compareItemId = 0, ItemStack compareInstance = null)
    {
        CancelPendingTooltip();

        int requestId = tooltipRequestItemId;
        showDelayCoroutine = StartCoroutine(ShowInventoryTooltipAfterDelay(requestId,target, itemId, instance, compareItemId, compareInstance));
    }
    private IEnumerator ShowInventoryTooltipAfterDelay(int requestId, RectTransform target, int itemId, ItemStack instance, int compareItemId, ItemStack compareInstance)
    {
        yield return new WaitForSeconds(showDelay);

        if (requestId != tooltipRequestItemId) yield break;

        ShowInventoryTooltip(target, itemId, instance, compareItemId, compareInstance);
        showDelayCoroutine = null;
    }
    private void ShowInventoryTooltip(RectTransform target, int itemId, ItemStack instance, int compareItemId = 0, ItemStack compareInstance = null)
    {
        if (target == null || itemId == 0) return;

        ItemTooltipData mainData = ItemTooltipBuilder.Build(itemId, instance, false, compareItemId);
        if (mainData == null) return;

        mainTooltip.Show(mainData);

        bool showCompare = ShouldShowCompare(itemId, compareItemId, instance, compareInstance);

        if (showCompare)
        {
            ItemTooltipData compareData = ItemTooltipBuilder.Build(compareItemId, compareInstance, true, 0);

            if (compareData != null)
            {
                compareTooltip.Show(compareData);

                mainTooltip.RefreshLayout();
                compareTooltip.RefreshLayout();

                PlaceTooltips(target, true);
                return;
            }
        }

        compareTooltip.Hide();
        mainTooltip.RefreshLayout();

        PlaceTooltips(target, false);
    }
    public  void RequestEquipmentTooltipDelayed(RectTransform target, int itemId, ItemStack instance, bool isEquipped = true)
    {
        CancelPendingTooltip();

        int requestId = tooltipRequestItemId;

        showDelayCoroutine = StartCoroutine(ShowEquipmentTooltipAfterDelay(requestId,target, itemId, instance, isEquipped));
    }
    private IEnumerator ShowEquipmentTooltipAfterDelay(int requestId, RectTransform target, int itemId, ItemStack instance, bool isEquipped)
    {
        yield return new WaitForSeconds(showDelay);

        if (requestId != tooltipRequestItemId) yield break;

        ShowEquipmentTooltip(target, itemId, instance, isEquipped);
        showDelayCoroutine = null;
    }
    private void CancelPendingTooltip()
    {
        tooltipRequestItemId++;
        if (showDelayCoroutine != null)
        {
            StopCoroutine(showDelayCoroutine);
            showDelayCoroutine = null;
        }
    }
    private void ShowEquipmentTooltip(RectTransform target, int itemId, ItemStack instance, bool isEquipped = true)
    {
        if (target == null || itemId == 0) return;

        ItemTooltipData data = ItemTooltipBuilder.Build(itemId, instance, isEquipped, 0);
        if (data == null) return;

        mainTooltip.Show(data);
        compareTooltip.Hide();

        PlaceTooltips(target, false);
    }

    public void HideAll()
    {
        CancelPendingTooltip();

        if (mainTooltip != null)
            mainTooltip.Hide();

        if (compareTooltip != null)
            compareTooltip.Hide();
    }

    private void PlaceTooltips(RectTransform target, bool hasCompare)
    {
        if (canvasRect == null || mainTooltip == null || target == null) return;

        RectTransform mainRect = mainTooltip.GetComponent<RectTransform>();
        RectTransform compareRect = compareTooltip != null ? compareTooltip.GetComponent<RectTransform>() : null;

        Vector3 worldCenter = target.TransformPoint(target.rect.center);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCenter);

        bool isRightSide = screenPoint.x >= Screen.width * 0.5f;

        float y = mainOffset.y-30f;
        float canvasHalfWidth = canvasRect.rect.width * 0.5f;
        float canvasHalfHeight = canvasRect.rect.height * 0.5f;

        mainRect.pivot = new Vector2(0f, 0.5f);
        if (compareRect != null)
            compareRect.pivot = new Vector2(0f, 0.5f);

        bool showCompare = hasCompare && compareRect != null && compareRect.gameObject.activeSelf;

        if (!showCompare)
        {
            float centerX = isRightSide
                ? -canvasHalfWidth * singleCenterRatio
                : canvasHalfWidth * singleCenterRatio;

            centerX += isRightSide ? singleLeftNudge : singleRightNudge;

            float mainX = centerX - (tooltipWidth * 0.5f);

            mainX = Mathf.Clamp(mainX, -canvasHalfWidth, canvasHalfWidth - tooltipWidth);

            float halfH = mainRect.rect.height * 0.5f;
            y = Mathf.Clamp(y, -canvasHalfHeight + halfH, canvasHalfHeight - halfH);

            mainRect.anchoredPosition = new Vector2(mainX, y);
            return;
        }

        float groupWidth = tooltipWidth * 2f + spacing;

        float groupCenterX = isRightSide
            ? -canvasHalfWidth * groupCenterRatio
            : canvasHalfWidth * groupCenterRatio;

        groupCenterX += isRightSide ? groupLeftNudge : groupRightNudge;

        float groupX = groupCenterX - (groupWidth * 0.5f);

        groupX = Mathf.Clamp(groupX, -canvasHalfWidth, canvasHalfWidth - groupWidth);

        float groupHeight = Mathf.Max(mainRect.rect.height, compareRect.rect.height);
        float halfGroupH = groupHeight * 0.5f;
        y = Mathf.Clamp(y, -canvasHalfHeight + halfGroupH, canvasHalfHeight - halfGroupH);

        mainRect.anchoredPosition = new Vector2(groupX, y);
        compareRect.anchoredPosition = new Vector2(groupX + tooltipWidth + spacing, y);
    }
    private bool ShouldShowCompare(int itemId, int compareItemId, ItemStack instance, ItemStack compareInstance)
    {
        if (compareItemId == 0) return false;

        CommonItemData common = ItemDB.GetCommon(itemId);
        if (common == null) return false;

        ItemType itemType = (ItemType)common.ItemType;

        bool canCompareType = itemType == ItemType.Shoes || itemType == ItemType.Weapon || itemType == ItemType.Bag;

        if(!canCompareType) return false;

        if (itemId == compareItemId)
        {
            if (instance != null && compareInstance != null)
            {
                if (instance.guid == compareInstance.guid)
                    return false;
            }
            else
            {
                return false;
            }
        }return true;
    }
}
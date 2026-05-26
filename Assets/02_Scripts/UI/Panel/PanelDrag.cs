using UnityEngine;
using UnityEngine.EventSystems;

public class PanelDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform targetPanel; //움직일 패널
    [SerializeField] private RectTransform standardRect; //닫기버튼 기준으로 화면 나가기 금지

    private Vector2 offset;
    private void Awake()
    {
        if (targetPanel == null)
            targetPanel = transform.parent as RectTransform;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (targetPanel == null) return;

        targetPanel.SetAsLastSibling();

        RectTransform parentRect = targetPanel.parent as RectTransform;
        if (parentRect == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle( //eventData 좌표를 로컬 좌표로 변환
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out var localMousePos);
        offset = targetPanel.anchoredPosition - localMousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(targetPanel == null) return;

        RectTransform parentTransform = targetPanel.parent as RectTransform;
        if (parentTransform == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentTransform,
            eventData.position,
            eventData.pressEventCamera,
            out var localMousePos);
        targetPanel.anchoredPosition = localMousePos + offset;

        KeepCloseButtonOnScreen();
    }
    private void KeepCloseButtonOnScreen()
    {
        if(standardRect == null) return;

        Vector3[] corners = new Vector3[4];
        standardRect.GetWorldCorners(corners);

        float minX = 0;
        float maxX = Screen.width;
        float minY = 0;
        float maxY = Screen.height;

        Vector3 fix = Vector3.zero;

        if (corners[0].x < minX) fix.x = minX - corners[0].x; //left
        if (corners[2].x > maxX) fix.x = maxX - corners[2].x; //right
        if (corners[0].y < minY) fix.y = minY - corners[0].y; //bottom
        if (corners[2].y > maxY) fix.y = maxY - corners[2].y; //top
        targetPanel.position += fix;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetPanel == null) return;

        targetPanel.SetAsLastSibling();
    }
}

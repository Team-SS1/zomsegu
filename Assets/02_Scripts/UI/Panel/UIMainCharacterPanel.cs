using UnityEngine;
using PlayerEnum;
using UnityEngine.UI;
using EventEnum;

public class UIMainCharacterPanel : MonoBehaviour
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;

    [Header("Buttons")]
    [SerializeField] private Button shinButton;
    [SerializeField] private Button hanButton;

    [Header("Outlines")]
    [SerializeField] private Outline shinOutline;
    [SerializeField] private Outline hanOutline;

    [Header("Panel BG")]
    [SerializeField] private Image leftPanelBG;
    [SerializeField] private Image rightPanelBG;
    [SerializeField] private Image giveItemPanelBG;

    [Header("Images")]
    [SerializeField] private Image characterImg;
    [SerializeField] private Sprite shinSprite;
    [SerializeField] private Sprite hanSprite;

    [Header("Lock")]
    [SerializeField] private GameObject shinLock;
    [SerializeField] private GameObject hanLock;
    [SerializeField] private GameObject itemGiveLock;

    [Header("Colors")]
    [SerializeField] private Color shinColor = new Color(166f / 255f, 166f / 255f, 166f / 255f);
    [SerializeField] private Color hanColor = new Color(143f / 255f, 170f / 255f, 220f / 255f);


    [SerializeField] private ScrollRect inventoryScrollRect;

    private void Awake()
    {
        shinButton.onClick.AddListener(OnClickShin);
        hanButton.onClick.AddListener(OnClickHan);
    }
    private void OnDestroy()
    {
        shinButton.onClick.RemoveListener(OnClickShin);
        hanButton.onClick.RemoveListener(OnClickHan);
    }
    private void OnEnable()
    {
        EventManager.Subscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectCharacterChanged);
        EventManager.Subscribe<GamePlayType>(EventKey.GamePlayTypeChanged, OnGamePlayTypeChanged);
        Refresh();
        
    }
    private void OnDisable()
    {
        EventManager.UnSubscribe<PlayerType>(EventKey.InspectCharacterChanged, OnInspectCharacterChanged);
        EventManager.UnSubscribe<GamePlayType>(EventKey.GamePlayTypeChanged, OnGamePlayTypeChanged);
    }
    private void OnInspectCharacterChanged(PlayerType playerType)
    {
        Refresh();
    }
    private void OnGamePlayTypeChanged(GamePlayType playType)
    {
        Refresh();
    }
    private void Refresh()
    {
        GamePlayType currentPlayType = PlayerManager.Instance.CurrentPlayType;

        if (currentPlayType == GamePlayType.PlayBOTH)
        {
            if (shinLock != null)
                shinLock.SetActive(false);
            if (hanLock != null)
                hanLock.SetActive(false);
            if (itemGiveLock != null)
                itemGiveLock.SetActive(false);
        }
        else if (currentPlayType == GamePlayType.PlaySHIN)
        {
            if (shinLock != null)
                shinLock.SetActive(false);
            if (hanLock != null)
                hanLock.SetActive(true);
            if (itemGiveLock != null)
                itemGiveLock.SetActive(true);
            if (selectedCharacterContext.CurrentInspectPlayer == PlayerType.Player_HAN)
            {
                selectedCharacterContext.SetInspectPlayer(PlayerType.Player_SHIN);
                return;
            }
        }
        else if (currentPlayType == GamePlayType.PlayHAN)
        {
            if(shinLock != null)
                shinLock.SetActive(true);
            if(hanLock != null)
                hanLock.SetActive(false);
            if(itemGiveLock != null)
                itemGiveLock.SetActive(true);
            if (selectedCharacterContext.CurrentInspectPlayer == PlayerType.Player_SHIN)
            {
                selectedCharacterContext.SetInspectPlayer(PlayerType.Player_HAN);
                return;
            }   
        }
        SetInteractable(currentPlayType);

        PlayerType currentPlayer = selectedCharacterContext.CurrentInspectPlayer;
        bool isShinSelected = currentPlayer == PlayerType.Player_SHIN;
        bool isHanSelected = currentPlayer == PlayerType.Player_HAN;

        if (shinOutline != null) shinOutline.enabled = !isShinSelected;
        if (hanOutline != null) hanOutline.enabled = !isHanSelected;

        Color colors = isShinSelected ? shinColor : hanColor;
        Color giveItemColor = isShinSelected ? hanColor : shinColor;

        if (leftPanelBG != null) leftPanelBG.color = colors;
        if (rightPanelBG != null) rightPanelBG.color = colors;
        if (giveItemPanelBG != null) giveItemPanelBG.color = giveItemColor;

        if (characterImg != null)
        {
            characterImg.sprite = isShinSelected ? shinSprite : hanSprite;
        }  
    }
    public void OnClickShin()
    {
        selectedCharacterContext.SetInspectPlayer(PlayerType.Player_SHIN);
        ResetScroll();
    }
    public void OnClickHan()
    {
        selectedCharacterContext.SetInspectPlayer(PlayerType.Player_HAN);
        ResetScroll();
    }
    private void ResetScroll()
    {
        if (inventoryScrollRect == null) return;

        Canvas.ForceUpdateCanvases();
        inventoryScrollRect.verticalNormalizedPosition = 1f;
    }
    private void SetInteractable(GamePlayType playType)
    {
        switch(playType)
        {
            case GamePlayType.PlayBOTH:
                if(shinButton != null)
                    shinButton.interactable = true;
                if(hanButton != null)
                    hanButton.interactable = true;
                break;
            case GamePlayType.PlaySHIN:
                if(shinButton != null) 
                    shinButton.interactable = true;
                if(hanButton != null)
                    hanButton.interactable = false;
                break;
            case GamePlayType.PlayHAN:
                if(shinButton != null)
                    shinButton.interactable = false;
                if(hanButton != null)
                    hanButton.interactable = true;
                break;
        }
    }
}

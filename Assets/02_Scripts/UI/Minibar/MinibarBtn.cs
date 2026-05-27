using UnityEngine;
using UnityEngine.UI;
using UIEnum;
public class MiniBarBtn : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button phoneBtn;
    [SerializeField] private Button mainPanelBtn;
    [SerializeField] private Button diaryBtn;
    [SerializeField] private Button settingBtn;

    private UIMainPanelFlowController flowController;

    private void Awake()
    {
        phoneBtn.onClick.AddListener(OnClickPhone);
        mainPanelBtn.onClick.AddListener(OnClickMainPanel);
        diaryBtn.onClick.AddListener(OnClickDiary);
        settingBtn.onClick.AddListener(OnClickSetting);
    }
    private void OnDestroy()
    {
        if(phoneBtn != null) phoneBtn.onClick.RemoveListener(OnClickPhone);
        if(mainPanelBtn != null) mainPanelBtn.onClick.RemoveListener(OnClickMainPanel);
        if(diaryBtn != null) diaryBtn.onClick.RemoveListener(OnClickDiary);
        if (settingBtn != null) settingBtn.onClick.RemoveListener(OnClickSetting);
    }
    public void BindFlowController(UIMainPanelFlowController flowController)
    {
        this.flowController = flowController;
    }
    private void OnClickMainPanel()
    {
        if(flowController == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("MinibarBtn : FlowController가 바인딩되지 않았습니다. UIBootstrap에서 MinibarBtn에 FlowController를 바인딩해주세요.");
#endif
            return;
        }
        flowController.OnMiniBarMainPanelButtonClicked();
    }

    private void OnClickPhone()
    {
        // 나중에 폰 만들면 그때 폰 UI 열도록 수정
    }
    private void OnClickDiary()
    {
        // 나중에 다이어리 만들면 그때 다이어리 UI 열도록 수정
    }
    private void OnClickSetting()
    {
        // 나중에 설정 만들면 그때 설정 UI 열도록 수정
    }
}

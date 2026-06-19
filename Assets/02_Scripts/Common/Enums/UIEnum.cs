namespace UIEnum
{
    public enum UIOrder
    {
        Bottom_Panel = 0,
        Middle_Panel = 10,
        Middle_Popup = 20,
        Top_Panel = 30,
        Top_Popup = 40
    }
    public enum BlinkMode
    {
        None,
        FillAlert,
        BgAlert
    }
    public enum UIMainPanelFlowState
    {
        None,
        MainPanelOepn,
        DurabilityPanelOepn
    }
    public enum UIStatViewState
    {
        Normal,
        Up,
        Down
    }
    public enum UIStatusTagState
    {
        Inactive, // 어두운 이미지
        Active, // 일반 이미지
        Danger, // 기아, 탈수 같은 위험한 상태
        Severe, // 심한 부상
        Dead // 사망
    }
}
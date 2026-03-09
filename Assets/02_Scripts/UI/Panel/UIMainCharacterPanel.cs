using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;
using UnityEngine.UI;

public class UIMainCharacterPanel : MonoBehaviour
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;

    [SerializeField] private Button shinButton;
    [SerializeField] private Button hanButton;

    private void Awake()
    {
        shinButton.onClick.AddListener(OnClickShin);
        hanButton.onClick.AddListener(OnClickHan);
    }

    public void OnClickShin()
    {
        selectedCharacterContext.SetInspectPlayer(PlayerType.Player_SHIN);
    }
    public void OnClickHan()
    {
        selectedCharacterContext.SetInspectPlayer(PlayerType.Player_HAN);
    }
}

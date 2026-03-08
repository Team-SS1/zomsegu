using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerEnum;

public class UIMainCharacterPanel : MonoBehaviour
{
    [SerializeField] private UISelectedCharacterContext selectedCharacterContext;
    [SerializeField] private UIInventory uiInventory;

    public void OnClickShin()
    {
        selectedCharacterContext.SetInspectPlayer(PlayerType.Player_SHIN);
        RefreshAll();
    }
    public void OnClickHan()
    {
        selectedCharacterContext.SetInspectPlayer(PlayerType.Player_HAN);
        RefreshAll();
    }
    private void RefreshAll()
    {
        
    }
}

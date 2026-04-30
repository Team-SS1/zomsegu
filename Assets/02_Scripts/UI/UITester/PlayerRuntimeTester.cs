using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRuntimeTester : MonoBehaviour // 잠시 테스트용으로 붙인 후 삭제할 컴포넌트, 게이지 확인용
{
    [Header("Reference")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerCondition playerCondition;

    private void Start()
    {
        if(player == null || playerCondition == null || PlayerDataManager.Instance == null)
        {
            Debug.LogError("PlayerRuntimeTester: Missing references. Please assign Player, PlayerCondition, and ensure PlayerDataManager is initialized.");
            return;
        }
        player.Init();
        playerCondition.Init();
        Debug.Log("PlayerRuntimeTester: Initialization complete. Player and PlayerCondition are set up.");
    }
}

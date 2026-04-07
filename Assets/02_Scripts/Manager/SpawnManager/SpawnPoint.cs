using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private SpawnData spawnData;
    public SpawnData SpawnData => spawnData;

    private void Awake()
    {
        if (spawnData == null)
        {
            Debug.Log("Spawn Data is Null");
            return;
        }
    }
}

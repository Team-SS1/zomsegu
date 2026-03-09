using System.Collections;
using System.Collections.Generic;
using PlayerEnum;
using UnityEngine;

public class DataManager : GlobalSingleton<DataManager>
{
    protected override void Awake()
    {
        base.Awake();
        DataTableLoader.LoadTables();
    }
}

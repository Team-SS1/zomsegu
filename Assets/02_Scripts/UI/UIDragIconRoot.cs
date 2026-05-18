using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDragIconRoot : MonoBehaviour
{
    public static Transform Root {  get; private set; }
    private void Awake()
    {
        Root = transform;
    }
    private void OnDestroy()
    {
        if(Root == transform)
            Root = null;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemGuid
{
    public static string NewGuid() => Guid.NewGuid().ToString("N"); //32자리마다 하이픈(-)이 없는 32자리 문자열로 변환
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTooltipData
{
    public Sprite IconSprite;
    public string Name;
    public Color NameColor = Color.white;
    public string Description;
    public List<ItemTooltipLine> Lines = new();
}

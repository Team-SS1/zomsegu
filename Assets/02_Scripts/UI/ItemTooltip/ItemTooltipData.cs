using System.Collections.Generic;
using UnityEngine;

public class ItemTooltipData
{
    public Sprite IconSprite;
    public string Name;
    public Color NameColor = Color.white;
    public string Description;
    public Color DescriptionColor = Color.white;
    public List<ItemTooltipLine> Lines = new();
}

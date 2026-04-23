using UnityEngine;

[System.Serializable]
public class ItemTooltipLine
{
    public string Label;
    public string Value;
    public Color ValueColor = Color.white;
    public Color LabelColor = Color.white;
    public string Suffix;
    public bool IsDescription;
}

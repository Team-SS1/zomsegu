using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIItemTooltipLine : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txt;

    public void SetLine(ItemTooltipLine line)
    {
        if(txt == null || line == null) return;

        if (line.IsDescription)
        {
            txt.text = line.Value;
            return;
        }

        string valueHex = ColorUtility.ToHtmlStringRGB(line.ValueColor);

        string label = string.IsNullOrEmpty(line.Label) ? "" : $"{line.Label} : ";
        string value = string.IsNullOrEmpty(line.Value) ? "" : $"<color=#{valueHex}>{line.Value}</color>";
        string suffix = string.IsNullOrEmpty(line.Suffix) ? "" : line.Suffix;

        txt.text = $"{label}{value}{suffix}";
    }
    public void Clear()
    {
        if (txt != null)
            txt.text = "";
    }
}

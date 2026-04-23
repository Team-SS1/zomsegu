using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemTooltip : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameTXT;

    [Header("Body")]
    [SerializeField] private Transform lineContainer;
    [SerializeField] private TextMeshProUGUI descriptionTXT;

    [Header("Prefab")]
    [SerializeField] private UIItemTooltipLine linePrefab;

    [SerializeField] private RectTransform bgRect;

    private readonly List<UIItemTooltipLine> spawnedLines = new();

    public void SetTooltip(ItemTooltipData data)
    {
        Clear();

        if (data == null) return;

        if(icon != null)
        {
            icon.sprite = data.IconSprite;
            icon.enabled = data.IconSprite != null;
        }

        if(nameTXT != null)
        {
            nameTXT.text = data.Name;
            nameTXT.color = data.NameColor;
        }

        if(lineContainer != null && linePrefab != null && data.Lines != null)
        {
            for(int i = 0; i< data.Lines.Count; i++)
            {
                ItemTooltipLine line = data.Lines[i];
                if(line == null) continue;

                UIItemTooltipLine lineUI = Instantiate(linePrefab, lineContainer);
                lineUI.SetLine(line);
                spawnedLines.Add(lineUI);
            }
        }

        if(descriptionTXT != null)
        {
            string description = ConvertDescription(data.Description);

            descriptionTXT.text = description;
            descriptionTXT.color = data.DescriptionColor;
            descriptionTXT.gameObject.SetActive(!string.IsNullOrEmpty(data.Description));
        }

        RefreshLayout();
    }
    public void Clear()
    {
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        if (nameTXT != null)
            nameTXT.text = "";

        for (int i = 0; i < spawnedLines.Count; i++)
        {
            if (spawnedLines[i] != null)
                spawnedLines[i].gameObject.SetActive(false);
        }

        spawnedLines.Clear();

        if (descriptionTXT != null)
        {
            descriptionTXT.text = "";
            descriptionTXT.gameObject.SetActive(false);
        }
    }

    public void Show(ItemTooltipData data)
    {
        gameObject.SetActive(true);
        SetTooltip(data);
    }
    public void Hide()
    {
        Clear();
        gameObject.SetActive(false);
    }
    public void RefreshLayout()
    {
        Canvas.ForceUpdateCanvases();

        if(lineContainer != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(lineContainer as RectTransform);

        if(descriptionTXT != null)
        {
            descriptionTXT.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(descriptionTXT.transform as RectTransform);
        }

        if(bgRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(bgRect);

        Canvas.ForceUpdateCanvases();

        if(bgRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(bgRect);
    }
    private string ConvertDescription(string description)
    {
        if (string.IsNullOrEmpty(description)) return "";
        return description.Replace("\\n","\n");
    }
}

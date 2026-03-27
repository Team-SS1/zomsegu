using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ItemEnum;
using PlayerEnum;
public static class ItemTooltipBuilder
{
    private static readonly Color COLOR_NORMAL = Hex("#B0B0B0");
    private static readonly Color COLOR_RARE = Hex("#5EC8FF");
    private static readonly Color COLOR_EPIC = Hex("B06CFF");
    private static readonly Color COLOR_UNIQUE = Hex("#FFD44A");

    private static readonly Color COLOR_WHITE = Hex("#FFFFFF");
    private static readonly Color COLOR_GREEN = Hex("#3DDC84");
    private static readonly Color COLOR_RED = Hex("#F44336");

    private static readonly Color COLOR_ATK_1 = Hex("#E74C3C");
    private static readonly Color COLOR_ATK_2 = Hex("#F39C3D");
    private static readonly Color COLOR_ATK_3 = Hex("#E6C85C");
    private static readonly Color COLOR_ATK_4 = Hex("#A8D672");
    private static readonly Color COLOR_ATK_5 = Hex("#9E9E9E");

    public static ItemTooltipData Build(
        int itemId,
        ItemStack instance = null,
        bool isEquipped = false,
        int compareItemId = 0)
    {
        CommonItemData common = ItemDB.GetCommon(itemId);
        if (common == null) return null;

        ItemTooltipData data = new ItemTooltipData
        {
            IconSprite = LoadIcon(ItemDB.GetIconPath(itemId)),
            Name = GetDisplayName(itemId, isEquipped),
            NameColor = GetRarityColor(itemId),
            Description = ItemDB.GetDescription(itemId)
        };

        List<ItemTooltipLine> lines = data.Lines;

        

    }
    private static string GetDisplayName(int itemId, bool isEquipped)
    {
        string itemName = ItemDB.GetItemName(itemId);
        if (isEquipped)
            itemName += "(장착 중)";
        return itemName;
    }
    private static string GetRarityText(int rarity)
    {
        switch (rarity)
        {
            case 0: return "일반";
            case 1: return "레어";
            case 2: return "에픽";
            case 3: return "유니크";
            case 4: return "일반";
            default: return "일반";
        }
    }
    private static Color GetRarityColor(int rarity)
    {
        switch (rarity)
        {
            case 0: return COLOR_NORMAL;
            case 1: return COLOR_RARE;
            case 2: return COLOR_EPIC;
            case 3: return COLOR_UNIQUE;
            default: return COLOR_NORMAL;
        }
    }
    private static string GetItemTypeText(int itemId)
    {
        CommonItemData common = ItemDB.GetCommon(itemId);
        if (common == null) return "알 수 없음";

        ItemType itemType = (ItemType)common.ItemType;


        switch (itemType)
        {
            case ItemType.Head: return "머리 방어구";
            case ItemType.Body: return "몸통 방어구";
            case ItemType.Leg: return "다리 방어구";
            case ItemType.Shoes: return "신발";
            case ItemType.Bag: return "가방";
            case ItemType.Accessory: return "엑세서리";
            case ItemType.Consumable: return "소모품";
            case ItemType.Misc: return "잡동사니";
            case ItemType.Weapon:
                return GetWeaponTypeText(itemId);
            default:
                return "알 수 없음";
        }
    }
    private static string GetWeaponTypeText(int itemId)
    {
        if (!ItemDB.TryGetWeaponStat(itemId, out var stat))
            return "무기";

        switch (stat.WeaponDamageType)
        {
            case 0: return "맨손";
            case 1: return "원거리무기/투척";
            case 2: return "원거리무기/사물";
            case 3: return "근접무기/칼";
            case 4: return "근접무기/둔기";
            case 5: return "근접무기/도끼";
            case 6: return "근접무기/사물";
            default: return "무기";
        }
    }
    private static Color GetAttackColor(float attack)
    {
        if (attack >= 111f)
            return COLOR_ATK_1;
        else if(attack >= 91f)
            return COLOR_ATK_2;
        else if(attack >= 71f)
            return COLOR_ATK_3;
        else if(attack >= 51f)
            return COLOR_ATK_4;

        return COLOR_ATK_5;
    }
    private static string GetAttackSpeedText(float attackSpeed)
    {
        if (attackSpeed <= 0.8f) return $"빠름({attackSpeed:0.#}초)";
        else if (attackSpeed <= 1.2f) return $"보통({attackSpeed:0.#}초)";
        return $"느림({attackSpeed:0.#}초)";
    }
    private static string GetAttackRangeText(float attackRange, bool isRanged)
    {
        if (isRanged)
        {
            if (attackRange <= 4f) return "짧음";
            if (Mathf.Approximately(attackRange, 5f)) return "보통";
            return "긺";
        }
        if (attackRange < 0.8f) return "짧음";
        else if (attackRange < 1.2f) return "보통";
        return "긺";
    }
    private static string GetAttackAngleText(int attackAngle)
    {
        if (attackAngle >= 121) return "넓음";
        else if (attackAngle >= 91) return "보통";
        return "좁음";
    }
    private static Sprite LoadIcon(string path)
    {
        if(string.IsNullOrEmpty(path)) return null;
        return Resources.Load<Sprite>(path);
    }
    private static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var color);
        return color;
    }
}

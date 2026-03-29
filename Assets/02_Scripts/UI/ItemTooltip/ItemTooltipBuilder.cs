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

        AddBasicLines(lines, itemId);

        ItemType itemType = (ItemType)common.ItemType;

        switch (itemType)
        {
            case ItemType.Weapon:
                BuildWeaponLines(lines, itemId, instance, compareItemId);
                break;

            case ItemType.Head:
            case ItemType.Body:
            case ItemType.Leg:
                BuildArmorLines(lines, itemId, instance);
                break;

            case ItemType.Shoes:
                BuildShoesLines(lines, itemId, compareItemId);
                break;

            case ItemType.Bag:
                BuildBagLines(lines, itemId, compareItemId);
                break;

            case ItemType.Accessory:
                BuildAccessoryLines(lines, itemId);
                break;

            case ItemType.Consumable:
                BuildConsumableLines(lines, itemId);
                break;

            case ItemType.Misc:
                BuildMiscLines(lines, itemId);
                break;
        }

        AddWeightVolumeLine(lines, itemId);

        return data;
    }
    private static void AddBasicLines(List<ItemTooltipLine> lines, int itemId)
    {
        lines.Add(new ItemTooltipLine
        {
            Label = "유형",
            Value = GetItemTypeText(itemId),
            ValueColor = COLOR_WHITE
        });
        int rarity = ItemDB.GetRarity(itemId);
        lines.Add(new ItemTooltipLine
        {
            Label = "등급",
            Value = GetRarityText(itemId),
            ValueColor = COLOR_WHITE
        });
    }
    private static void BuildWeaponLines(List<ItemTooltipLine> lines, int itemId, ItemStack instance, int compareItemId)
    {
        if (!ItemDB.TryGetWeaponStat(itemId, out var weaponStat)) return;

        bool isRanged = ItemDB.IsRangedWeapon(itemId);

        string attackText = Mathf.FloorToInt(weaponStat.Attack).ToString(); //소수점 버리기
        Color attackColor = GetAttackColor(weaponStat.Attack);

        if (compareItemId != 0 && ItemDB.TryGetWeaponStat(compareItemId, out var compareStat))
        {
            int diff = Mathf.FloorToInt(weaponStat.Attack - compareStat.Attack);
            if (diff > 0)
                attackText += $"(+{diff})";
            else if (diff < 0)
                attackText += $"(-{Mathf.Abs(diff)})";
        }

        lines.Add(new ItemTooltipLine
        {
            Label = "공격력",
            Value = attackText,
            ValueColor = attackColor
        });

        lines.Add(new ItemTooltipLine         //////여기서부터 보면 비교했을때 컬러 변화가 안들어 가있음 나중에 수정
        {
            Label = "공격속도",
            Value = GetAttackSpeedText(weaponStat.AttackSpeed),
            ValueColor = COLOR_WHITE
        });

        lines.Add(new ItemTooltipLine
        {
            Label = "공격사거리",
            Value = GetAttackRangeText(weaponStat.AttackRange, isRanged),
            ValueColor = COLOR_WHITE
        });

        if (!isRanged)
        {
            lines.Add(new ItemTooltipLine
            {
                Label = "공격범위",
                Value = GetAttackAngleText(weaponStat.AttackAngle),
                ValueColor = COLOR_WHITE
            });
        }
        lines.Add(new ItemTooltipLine
        {
            Label = "치명타확률",
            Value = $"{weaponStat.CritChance}%",
            ValueColor = COLOR_WHITE
        });

        if (!isRanged)
        {
            int maxDurability = weaponStat.Durability;
            int currentDurability = instance != null ? instance.durability : maxDurability;

            lines.Add(new ItemTooltipLine
            {
                Label = "내구도",
                Value = $"{currentDurability}/{maxDurability}",
                ValueColor = COLOR_WHITE
            });
        }
    }
    private static void BuildArmorLines(List<ItemTooltipLine> lines, int itemId, ItemStack instance)
    {
        if (!ItemDB.TryGetArmorStat(itemId, out var stat)) return;

        int maxDurability = stat.Durability;
        int currentDurability = instance != null ? instance.durability : maxDurability;

        lines.Add(new ItemTooltipLine
        {
            Label = "내구도",
            Value = $"{currentDurability}/{maxDurability}",
            ValueColor = COLOR_WHITE
        });
    }
    private static void BuildShoesLines(List<ItemTooltipLine> lines, int itemId, int comparItmeId)
    {
        if(!ItemDB.TryGetAccessoryStat(itemId, out var stat)) return;

        string moveSpeedText = Mathf.FloorToInt(stat.SpdBuffAdd).ToString();

        if(comparItmeId != 0 && ItemDB.TryGetAccessoryStat(comparItmeId, out var compareStat))
        {
            int diff = Mathf.FloorToInt(stat.SpdBuffAdd - compareStat.SpdBuffAdd);
            if (diff > 0)
                moveSpeedText += $"(+{diff})";
            else if (diff < 0)
                moveSpeedText += $"(-{Mathf.Abs(diff)})";
        }
        lines.Add(new ItemTooltipLine
        {
            Label = "이동속도",
            Value = moveSpeedText,
            ValueColor = COLOR_WHITE
        } );
    }
    private static void BuildBagLines(List<ItemTooltipLine> lines, int itemId, int compareItemId)
    {
        if(!ItemDB.TryGetAccessoryStat(itemId, out var stat)) return;

        string bagCapacityText = stat.BagCapacity.ToString("0.#");
        string bagWeightLimitText = stat.BagWeightLimit.ToString("0.#");
        string penaltyFreeWeightText = stat.PenaltyFreeWeight.ToString();

        if(compareItemId != 0 && ItemDB.TryGetAccessoryStat(compareItemId, out var compareStat))
        {
            float capacityDiff = stat.BagCapacity - compareStat.BagCapacity;
            float weightLimitDiff = stat.BagWeightLimit - compareStat.BagWeightLimit;
            int penaltyFreeWeightDiff = stat.PenaltyFreeWeight - compareStat.PenaltyFreeWeight;

            if (capacityDiff > 0f)
                bagCapacityText += $"(+{capacityDiff:0.#})";
            else if(capacityDiff < 0f)
                bagCapacityText += $"(-{Mathf.Abs(capacityDiff):0.#})";

            if(weightLimitDiff > 0f)
                bagWeightLimitText += $"(+{weightLimitDiff:0.#})";
            else if(weightLimitDiff < 0f)
                bagWeightLimitText += $"(-{Mathf.Abs(weightLimitDiff):0.#})";

            if(penaltyFreeWeightDiff > 0)
                penaltyFreeWeightText += $"(+{penaltyFreeWeightDiff})";
            else if(penaltyFreeWeightDiff < 0)
                penaltyFreeWeightText += $"(-{Mathf.Abs(penaltyFreeWeightDiff)})";
        }

        lines.Add(new ItemTooltipLine
        {
            Label = "수납용량",
            Value = bagCapacityText,
            ValueColor = COLOR_WHITE
        });
        lines.Add(new ItemTooltipLine
        {
            Label = "수납무게",
            Value = bagWeightLimitText,
            Suffix = "kg",
            ValueColor = COLOR_WHITE
        });
        lines.Add(new ItemTooltipLine
        {
            Label = "무게 면제",
            Value = penaltyFreeWeightText,
            Suffix = "kg",
            ValueColor = COLOR_WHITE
        });
    }
    private static void BuildAccessoryLines(List<ItemTooltipLine> lines, int itemId)
    {
        if (!ItemDB.TryGetAccessoryStat(itemId, out var stat)) return;
        
        AddIfNotZero(lines, "공격력 증가", Mathf.FloorToInt(stat.AtkBuffAdd).ToString(), stat.AtkBuffAdd);
        AddIfNotZero(lines, "공격속도 감소",$"{stat.AtkSpdBuffAdd:0.0}초" , stat.AtkSpdBuffAdd,true);
        AddIfNotZero(lines, "이동속도 증가", Mathf.FloorToInt(stat.SpdBuffAdd).ToString(), stat.SpdBuffAdd);
        AddIfNotZero(lines, "최대 스태미너 증가", Mathf.FloorToInt(stat.MaxStaminaBuffAdd).ToString(), stat.MaxStaminaBuffAdd);
    }
    private static void BuildMiscLines(List<ItemTooltipLine> lines, int itemId)
    {
        // 잡동사니는 현재로서는 특별한 스탯이 없지만, 나중에 추가될 수 있으므로 이 메서드를 만들어두었습니다.
    }

    private static void BuildConsumableLines(List<ItemTooltipLine> lines, int itemId)
    {
        if (!ItemDB.TryGetConsumableStat(itemId, out var stat)) return;
        
        AddIfNotZero(lines, "배고픔 회복", $"{stat.HungerRecover}칸", stat.HungerRecover);
        AddIfNotZero(lines, "목마름 회복", $"{stat.ThirstRecover}칸", stat.ThirstRecover);

        AddIfNotZero(lines, "공격력 증가", Mathf.FloorToInt(stat.AtkBuffAdd).ToString(), stat.AtkBuffAdd);
        AddIfNotZero(lines, "공격속도 감소", $"{stat.AtkSpdBuffAdd:0.0}초", stat.AtkSpdBuffAdd);
        AddIfNotZero(lines, "이동속도 증가", Mathf.FloorToInt(stat.SpdBuffAdd).ToString(), stat.SpdBuffAdd);
        AddIfNotZero(lines, "최대 스태미너 증가", Mathf.FloorToInt(stat.MaxStaminaBuffAdd).ToString(), stat.MaxStaminaBuffAdd);

        if(stat.Duration != 0)
        {
            lines.Add(new ItemTooltipLine
            {
                Label = "지속시간",
                Value = $"{stat.Duration}초",
                ValueColor = COLOR_WHITE
            });
        }
    }
    private static void AddWeightVolumeLine(List<ItemTooltipLine> lines, int itemId)
    {
        float volume = ItemDB.GetVolume(itemId);
        float weight = ItemDB.GetWeight(itemId);

        lines.Add(new ItemTooltipLine
        {
            Label = "용량/무게",
            Value = $"{volume:0.#}/{weight:0.#}",
            Suffix = "kg",
            ValueColor = COLOR_WHITE
        });
    }
    private static void AddIfNotZero(List<ItemTooltipLine> lines, string label, string valueText, float rawValue, bool minusPrefix = false)
    {
        if(Mathf.Approximately(rawValue, 0f)) return;

        string prefix = minusPrefix ? "-" : "+";

        lines.Add(new ItemTooltipLine
        {
            Label = label,
            Value = $"{prefix}{valueText}",
            ValueColor = COLOR_GREEN
        });
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

using UnityEngine;

public class WorldLootObject : MonoBehaviour // 실제 월드에 존재하는 아이템 오브젝트
{
    [Header("Loot Source")]
    [SerializeField] private LootSource lootSource;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer iconRenderer;

    [Header("Option")]
    [SerializeField] private bool destroyWhenEmpty = true; // 아이템이 모두 사라졌을 때 오브젝트도 함께 제거할지 여부

    public LootSource LootSource => lootSource;

    private void Reset()
    {
        if (iconRenderer == null)
            iconRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    public void Init(LootSource source, Sprite icon) // 초기화 메서드 (아이콘 포함)
    {
        lootSource = source;
        SetIcon(icon);
    }
    public LootSource GetLootSource()
    {
        return lootSource;
    }
    public bool HasLootItems() // 아이템이 존재하는지 여부
    {
        return lootSource != null 
            && lootSource.lootItems != null
            && lootSource.lootItems.Count > 0;
    }
    public void RefreshAfterLootChanged() // 아이템이 변경된 후에 호출되는 메서드
    {
        if(!destroyWhenEmpty) return;

        if (!HasLootItems())
        {
            Destroy(gameObject);
        }
    }
    public void SetIcon(Sprite icon) // 아이콘 설정 메서드
    {
        if (iconRenderer == null) return;

        iconRenderer.sprite = icon;
        iconRenderer.enabled = icon != null;
    }
}

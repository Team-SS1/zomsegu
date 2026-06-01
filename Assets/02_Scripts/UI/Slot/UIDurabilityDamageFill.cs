using UnityEngine;
using UnityEngine.UI;

public class UIDurabilityDamageFill : MonoBehaviour
{
    [SerializeField] private Image durabilityDamageFillImage;

    public void SetDurability(ItemStack instance)
    {
        if(durabilityDamageFillImage == null) return;

        if(instance == null || !instance.HasDurability || instance.maxDurability <= 0)
        {
            Hide();
            return;
        }
        
        float currentRatio = Mathf.Clamp01((float)instance.durability / instance.maxDurability);
        float damageRatio = 1f - currentRatio;

        durabilityDamageFillImage.gameObject.SetActive(damageRatio > 0f);
        durabilityDamageFillImage.fillAmount = damageRatio;
    }
    public void Hide()
    {
        if(durabilityDamageFillImage == null) return;

        durabilityDamageFillImage.fillAmount = 0f;
        durabilityDamageFillImage.gameObject.SetActive(false);
    }
}

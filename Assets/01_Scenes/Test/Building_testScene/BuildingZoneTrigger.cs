using UnityEngine;

public class BuildingZoneTrigger : MonoBehaviour
{
    [SerializeField] private BuildingSection section;
    [SerializeField] private BuildingVisibilityManager manager;

    private void Awake()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<BuildingVisibilityManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (manager == null)
            return;

        manager.ChangeSection(section);
    }
}

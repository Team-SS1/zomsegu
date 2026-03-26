using UnityEngine;

public class BuildingVisibilityManager : MonoBehaviour
{
    [Header("Floor 1 ~ 2")]
    [SerializeField] private GameObject floor01;
    [SerializeField] private GameObject lowerStair12;
    [SerializeField] private GameObject landing12;
    [SerializeField] private GameObject upperStair12;
    [SerializeField] private GameObject floor02;

    [Header("Floor 2 ~ 3")]
    [SerializeField] private GameObject lowerStair23;
    [SerializeField] private GameObject landing23;
    [SerializeField] private GameObject upperStair23;
    [SerializeField] private GameObject floor03;

    public BuildingSection CurrentSection { get; private set; } = BuildingSection.Floor1Side;

    private void Awake()
    {
        ChangeSection(BuildingSection.Floor1Side, true);
    }

    public bool ChangeSection(BuildingSection targetSection)
    {
        return ChangeSection(targetSection, false);
    }

    private bool ChangeSection(BuildingSection targetSection, bool force)
    {
        if (!force && !CanTransitionTo(targetSection))
        {
            return false;
        }

        CurrentSection = targetSection;

        switch (targetSection)
        {
            case BuildingSection.Floor1Side:
                ShowFloor1Side();
                break;

            case BuildingSection.Landing12:
                ShowLanding12();
                break;

            case BuildingSection.Floor2Side:
                ShowFloor2Side();
                break;

            case BuildingSection.Landing23:
                ShowLanding23();
                break;

            case BuildingSection.Floor3Side:
                ShowFloor3Side();
                break;
        }

        return true;
    }

    private bool CanTransitionTo(BuildingSection targetSection)
    {
        if (targetSection == CurrentSection)
            return false;

        return (CurrentSection, targetSection) switch
        {
            (BuildingSection.Floor1Side, BuildingSection.Landing12) => true,
            (BuildingSection.Landing12, BuildingSection.Floor1Side) => true,
            (BuildingSection.Landing12, BuildingSection.Floor2Side) => true,
            (BuildingSection.Floor2Side, BuildingSection.Landing12) => true,

            (BuildingSection.Floor2Side, BuildingSection.Landing23) => true,
            (BuildingSection.Landing23, BuildingSection.Floor2Side) => true,
            (BuildingSection.Landing23, BuildingSection.Floor3Side) => true,
            (BuildingSection.Floor3Side, BuildingSection.Landing23) => true,

            _ => false,
        };
    }

    private void ShowFloor1Side()
    {
        SetActiveSafe(floor01, true);
        SetActiveSafe(lowerStair12, true);
        SetActiveSafe(landing12, false);
        SetActiveSafe(upperStair12, false);
        SetActiveSafe(floor02, false);

        SetActiveSafe(lowerStair23, false);
        SetActiveSafe(landing23, false);
        SetActiveSafe(upperStair23, false);
        SetActiveSafe(floor03, false);
    }

    private void ShowLanding12()
    {
        SetActiveSafe(floor01, false);
        SetActiveSafe(lowerStair12, true);
        SetActiveSafe(landing12, true);
        SetActiveSafe(upperStair12, true);
        SetActiveSafe(floor02, false);

        SetActiveSafe(lowerStair23, false);
        SetActiveSafe(landing23, false);
        SetActiveSafe(upperStair23, false);
        SetActiveSafe(floor03, false);
    }

    private void ShowFloor2Side()
    {
        SetActiveSafe(floor01, false);
        SetActiveSafe(lowerStair12, false);
        SetActiveSafe(landing12, true);
        SetActiveSafe(upperStair12, true);
        SetActiveSafe(floor02, true);

        SetActiveSafe(lowerStair23, true);
        SetActiveSafe(landing23, false);
        SetActiveSafe(upperStair23, false);
        SetActiveSafe(floor03, false);
    }

    private void ShowLanding23()
    {
        SetActiveSafe(floor01, false);
        SetActiveSafe(lowerStair12, false);
        SetActiveSafe(landing12, false);
        SetActiveSafe(upperStair12, false);
        SetActiveSafe(floor02, false);

        SetActiveSafe(lowerStair23, true);
        SetActiveSafe(landing23, true);
        SetActiveSafe(upperStair23, true);
        SetActiveSafe(floor03, false);
    }

    private void ShowFloor3Side()
    {
        SetActiveSafe(floor01, false);
        SetActiveSafe(lowerStair12, false);
        SetActiveSafe(landing12, false);
        SetActiveSafe(upperStair12, false);
        SetActiveSafe(floor02, false);

        SetActiveSafe(lowerStair23, false);
        SetActiveSafe(landing23, true);
        SetActiveSafe(upperStair23, true);
        SetActiveSafe(floor03, true);
    }

    private void SetActiveSafe(GameObject target, bool isActive)
    {
        if (target != null)
        {
            target.SetActive(isActive);
        }
    }
}
/// <summary>
/// Unity 전용 Random
/// </summary>
public sealed class UnityRandom : IRandom
{
    public int Range(int min, int max)
    {
        return UnityEngine.Random.Range(min, max);
    }
}

/// <summary>
/// 테스트용 Random
/// </summary>
public sealed class FixedRandom : IRandom
{
    private readonly int value;
    public FixedRandom(int value) => this.value = value;
    public int Range(int min, int max) => value;
}
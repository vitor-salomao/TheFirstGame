using UnityEngine;

[CreateAssetMenu(menuName = "Packers/Difficulty Curve")]
public class DifficultyCurve : ScriptableObject
{
    [Header("Quota")]
    [Tooltip("Average quota on day 1.")]
    [Min(1)] public int BaseQuota = 60;
    [Tooltip("Quota multiplier per day. 1.3 = +30% per day.")]
    [Min(1f)] public float GrowthPerDay = 1.3f;
    [Tooltip("How far an easy/hard day moves the quota from average. 0.15 = ±15%.")]
    [Range(0f, 0.5f)] public float QuotaVariance = 0.15f;

    [Header("Surplus (total value spawned ÷ quota)")]
    [Min(1f)] public float EasySurplus = 1.8f;
    [Min(1f)] public float HardSurplus = 1.15f;

    [Header("Limits")]
    [Min(1)] public int MaxItemsPerDay = 15;
    [Tooltip("No single item can be worth more than this share of the quota.")]
    [Range(0.1f, 1f)] public float MaxItemShareOfQuota = 0.5f;

    /// difficulty: 0 = easiest possible day, 1 = hardest.
    public int QuotaFor(int day, float difficulty)
    {
        float average = BaseQuota * Mathf.Pow(GrowthPerDay, day - 1);
        float swing = 1f + QuotaVariance * (difficulty * 2f - 1f);
        return Mathf.Max(1, Mathf.RoundToInt(average * swing));
    }

    public float SurplusFor(float difficulty) => Mathf.Lerp(EasySurplus, HardSurplus, difficulty);
}
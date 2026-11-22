using System;
using UnityEngine;

public class Durability
{
    private bool indestructible;

    public float Max { get; private set; } = 100f;
    public float Current { get; private set; } = 100f;
    public bool IsBroken => !indestructible && Current <= 0f;

    public event Action Broken;

    public void Reset(float max, bool isIndestructible)
    {
        Max = max;
        Current = max;
        indestructible = isIndestructible;
    }

    public void Damage(float amount)
    {
        if (indestructible || IsBroken || amount <= 0f) return;

        Current = Mathf.Max(0f, Current - amount);
        if (IsBroken) Broken?.Invoke();
    }
}

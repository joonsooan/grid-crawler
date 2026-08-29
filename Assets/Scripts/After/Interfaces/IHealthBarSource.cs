using System;
using UnityEngine;

public interface IHealthBarSource
{
    Transform HealthBarAnchor { get; }
    int CurrentHp { get; }
    int MaxHp { get; }
    event Action<int, int> OnHealthChanged;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Job", fileName = "AIJob")]
public class AIJob : ScriptableObject
{
    [field: SerializeField] public string DisplayName { get; protected set; }
    [field: SerializeField] public Structure AssociatedStructure { get; protected set; }
    [field: SerializeField] public bool IsPlayerSetTask { get; protected set; } = false;
    [field: SerializeField, Range(0f, 1f)] public float Cooldown { get; protected set; } = 0.5f;
    [field: SerializeField, Range(0f, 1f)] public float JobPriority { get; protected set; } = 0.5f;
    [field: SerializeField, Range(0f, 1f)] public float DecayRate { get; protected set; } = 0.005f;
}

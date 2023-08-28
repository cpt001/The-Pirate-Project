using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public enum EInteractionType
{
    Instant = 0,
    OverTime = 1,
}



[System.Serializable]
public class InteractionStatChange
{
    public NeedFulfillmentType targetStat;
    public float value;
}

public abstract class BaseInteractions : MonoBehaviour
{
    public bool isInventoryItem;
    public bool isConsumedOnUse;

    [SerializeField] protected string _DisplayName;
    [SerializeField] protected EInteractionType _InteractionType = EInteractionType.Instant;
    [SerializeField] protected float _Duration = 0f;
    [SerializeField, FormerlySerializedAs("statChanges")] protected InteractionStatChange[] _statChanges;

    public string DisplayName => _DisplayName;
    public EInteractionType InteractionType => _InteractionType;
    public float Duration => _Duration;
    public InteractionStatChange[] statChanges => _statChanges;

    public abstract bool Usable();
    public abstract void LockInteraction();
    public abstract void Perform(PawnBaseClass performer, UnityAction<BaseInteractions> onCompleted);
    public abstract void UnlockInteraction();

    public void ApplyStatChanges(PawnBaseClass performer, float proportion)
    {
        foreach (var statChange in statChanges)
        {
            performer.UpdateIndividualStat(statChange.targetStat, statChange.value * proportion);
        }
    }
}

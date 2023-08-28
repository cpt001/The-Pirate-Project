using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

///Interactions are what the need fulfillment devices, and are effectively the individual commands on a smart object.

//This class is responsible for handling logic required for anything to interact with a smart object
public class SimpleInteraction : BaseInteractions
{
    protected class PerformerInfo
    {
        public PawnBaseClass PerformingAI;
        public float ElapsedTime;
        public UnityAction<BaseInteractions> taskCompleted;
    }
    [SerializeField] protected int MaxSimultaneousUsers = 1;

    protected int numCurrentUsers = 0;
    protected List<PerformerInfo> currentPerformers = new List<PerformerInfo>();
    public override bool Usable()
    {
        return numCurrentUsers < MaxSimultaneousUsers;
    }

    public override void LockInteraction()
    {
        ++numCurrentUsers;
        if (numCurrentUsers > MaxSimultaneousUsers)
        {
            Debug.LogError($"Too many users have locked this interaction {_DisplayName}");
        }

    }

    public override void Perform(PawnBaseClass performer, UnityAction<BaseInteractions> onCompleted)
    {
        if (numCurrentUsers <= 0)
        {
            Debug.LogError($"Trying to perform an action with no users {_DisplayName}");
            return;
        }
        //Check interaction type
        if (InteractionType == EInteractionType.Instant)
        {
            if (_statChanges.Length > 0)
                ApplyStatChanges(performer, 1f);
                onCompleted.Invoke(this);
        }
        if (InteractionType == EInteractionType.OverTime)
        {
            currentPerformers.Add(new PerformerInfo() { PerformingAI = performer, ElapsedTime = 0, taskCompleted = onCompleted });
        }
    }

    public override void UnlockInteraction()
    {
        if (numCurrentUsers <= 0)
        {
            Debug.LogError($"Trying to unlock an already unlocked interaction {_DisplayName}");
        }
        --numCurrentUsers;
    }

    protected virtual void Update()
    {
        for (int index = currentPerformers.Count - 1; index >= 0; index--)
        {
            PerformerInfo performer = currentPerformers[index];
            float previousElapsedTime = performer.ElapsedTime;
            performer.ElapsedTime = Mathf.Min(performer.ElapsedTime + Time.deltaTime, _Duration);

            if (_statChanges.Length > 0)
                ApplyStatChanges(performer.PerformingAI, (performer.ElapsedTime - previousElapsedTime) - _Duration);

            if (performer.ElapsedTime >= _Duration)
            {
                performer.taskCompleted.Invoke(this);
                currentPerformers.RemoveAt(index);
            }
        }
    }
}

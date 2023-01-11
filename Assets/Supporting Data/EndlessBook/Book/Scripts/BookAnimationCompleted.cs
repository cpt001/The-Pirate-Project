namespace echo17.EndlessBook
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Triggered at the end of the book animation cycle
    /// </summary>
    public class BookAnimationCompleted : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.GetComponentInParent<EndlessBook>().BookAnimationCompleted();
        }
    }
}
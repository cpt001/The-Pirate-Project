namespace echo17.EndlessBook
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Triggered at the end of the page animation cycle
    /// </summary>
    public class PageAnimationCompleted : StateMachineBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.GetComponentInParent<Page>().PageAnimationCompleted();
        }
    }
}
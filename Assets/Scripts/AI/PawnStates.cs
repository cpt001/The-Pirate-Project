using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnStates : MonoBehaviour
{
    //Okay. This is an exploratory implementation of overall states. 
    //Not implemented yet
    /// <summary>
    /// It should help to a degree with setting up idle states, and giving the pawns more ability to explore beyond home/work cycle
    /// </summary>
    public enum NavigationState
    {
        STOPPED_SLEEPING,
        STOPPED_IDLE,
        STOPPED_WORKING,

        MOVING_IDLE,
        MOVING_PURPOSE,

        FLEEING,

        OFF_GRID,
    }
}

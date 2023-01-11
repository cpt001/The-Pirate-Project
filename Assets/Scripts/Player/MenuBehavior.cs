using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuBehavior : MonoBehaviour
{
    [Header("Menu Interaction Keys")]
    [SerializeField]
    private KeyCode openLastTabKey = KeyCode.Tab;
    [SerializeField]
    private KeyCode inventoryHotkey = KeyCode.I;
    [SerializeField]
    private KeyCode mapHotkey = KeyCode.M;
    [SerializeField]
    private KeyCode skillsHotkey = KeyCode.K;
    [SerializeField]
    private KeyCode journalHotkey = KeyCode.J;
    [SerializeField]
    private KeyCode pauseMenu = KeyCode.Escape;
    [SerializeField]
    private KeyCode interact = KeyCode.I;


    public enum MenuState
    {

    }
    [Tooltip("This keeps track of what was opened last")]
    public MenuState menuState;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskbarIconUI : MonoBehaviour
{
    // scene references
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;

    // variables for focus
    [SerializeField] private Color normalColor;
    [SerializeField] private Color inFocusColor;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite inFocusSprite;

    // reference to program this belongs to
    private Program program;

    /// <summary>
    /// Initializes this taskbar icon.
    /// </summary>
    /// <param name="program">Program this taskbar icon belongs to</param>
    internal void Initialize(Program program)
    {
        // make gameobjects name recognizable
        gameObject.name += program.programName;

        // set title and sprite to match program's
        title.text = program.programName;
        icon.sprite = program.iconSprite;

        // keep track of program 
        this.program = program;
    }

    /// <summary>
    /// Called when programs focus changes.
    /// </summary>
    /// <param name="inFocus">Whether the program is in focus</param>
    internal void ProgramInFocus(bool inFocus)
    {
        // change color and sprite based on focus state
        background.color = inFocus ? inFocusColor : normalColor;
        background.sprite = inFocus ? inFocusSprite : normalSprite;
    }

    /// <summary>
    /// Called on click
    /// </summary>
    public void OnClick()
    {
        // if the program is currently in focus
        if (program.inFocus)
            program.Minimize(true);
        else
            program.Focus(true);
    }
}

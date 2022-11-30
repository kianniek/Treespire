using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShortcutUI : MonoBehaviour, IPointerClickHandler
{
    // scene references
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI title;

    // reference to the program this shortcut belongs to
    internal Program program { get; private set; }

    // type of shortcuts
    private Type type;
    internal enum Type { 
        None = 0, 
        Desktop, 
        StartMenu 
    }

    /// <summary>
    /// Initializes the shortcut.
    /// </summary>
    /// <param name="program">Program this shortcut belongs to</param>
    /// <param name="type">Type of shortcut</param>
    internal void Initialize(Program program, Type type)
    {
        // make gameobjects name recognizable
        gameObject.name += program.programName;

        // set title and sprite to match program's
        title.text = program.programName;
        icon.sprite = program.iconSprite;

        // keep track of program and type
        this.program = program;
        this.type = type;
    }

    /// <summary>
    /// Opens the program via this shortcut.
    /// </summary>
    private void OpenProgram()
    {
        // start the program
        program.StartUp();

        // close the start menu if this is a startmenu shortcut
        // since it's open in that case
        if (type == Type.StartMenu)
            GameManager.instance.startMenu.Close();
    }

    /// <summary>
    /// Called when this shortcut is clicked.
    /// </summary>
    /// <param name="eventData">The eventdata on click</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // behave based on type
        switch (type)
        {
            case Type.Desktop:
                // open program after a double click
                if (eventData.clickCount == 2) OpenProgram();
                break;
            case Type.StartMenu:
                // open program after one click
                OpenProgram();
                break;
        }
    }
}

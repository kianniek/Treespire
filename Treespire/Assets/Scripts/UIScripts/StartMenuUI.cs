using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuUI : MonoBehaviour
{
    // scene references 
    [SerializeField] private Image buttonBackground;
    [SerializeField] private GameObject startMenu;

    // variables for menu open
    [SerializeField] private Color normalColor;
    [SerializeField] private Color menuOpenColor;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite menuOpenSprite;

    // whether the start menu is open
    internal bool IsOpen { get { return startMenu.activeInHierarchy; } }

    /// <summary>
    /// Called when start menu is opened.
    /// </summary>
    internal void Open()
    {
        // activate the menu
        startMenu.SetActive(true);

        // set button background color and sprite
        buttonBackground.color = menuOpenColor;
        buttonBackground.sprite = menuOpenSprite;

        // set the click detector so that clicking anywhere besides the menu, 
        // it closes the menu again
        GameManager.instance.BringClickDetectorToFront(Close, startMenu.transform);
    }

    /// <summary>
    /// Called when start menu is closed.
    /// </summary>
    internal void Close()
    {
        // set button background color and sprite
        buttonBackground.color = normalColor;
        buttonBackground.sprite = normalSprite;

        // deactivate the menu
        startMenu.SetActive(false);

        // reset the click detector
        GameManager.instance.BringClickDetectorToBack(true);
    }

    /// <summary>
    /// Called when start button is clicked.
    /// </summary>
    public void StartButtonClicked()
    {
        // open or close the start menu
        // depending on current state
        if (startMenu.activeInHierarchy)
            Close();
        else
            Open();
    }

    /// <summary>
    /// Called when shut down is clicked.
    /// </summary>
    public void ShutDownClicked()
    {
        // close this menu
        Close();

        // call shutdown on the gamemanager
        GameManager.instance.PrepareShutDown();
    }
}

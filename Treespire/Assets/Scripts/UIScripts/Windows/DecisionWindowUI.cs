using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DecisionWindowUI : WindowUI
{
    // Scene references to buttons
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button denyButton;
    [SerializeField] protected TextMeshProUGUI message;

    internal override void Open()
    {
        // call base
        base.Open();

        // set the click detector so that clicking anywhere besides the menu, doesn't do anything
        GameManager.instance.BringClickDetectorToFront(null, transform, GameManager.instance.windowParent, true);
    }

    internal override void Close()
    {
        // call base
        base.Close();

        // reset the click detector
        GameManager.instance.BringClickDetectorToBack();
    }

    /// <summary>
    /// Sets the text of this window.
    /// </summary>
    /// <param name="title">Title of the window</param>
    /// <param name="message">Message of the window</param>
    internal void SetText(string title, string message)
    {        
        // set texts
        this.title.text = title;
        this.message.text = message;
    }

    /// <summary>
    /// Sets the delegates for callbacks.
    /// </summary>
    /// <param name="acceptDelegate">Delegate on accept clicked</param>
    /// <param name="denyDelegate">Delegate on deny clicked</param>
    internal void SetButtonDelegates(ButtonDelegate acceptDelegate, ButtonDelegate denyDelegate)
    {
        // remove all listeners
        acceptButton.onClick.RemoveAllListeners();
        denyButton.onClick.RemoveAllListeners();

        // and set new listeners
        acceptButton.onClick.AddListener(Close);
        denyButton.onClick.AddListener(Close);
        if (acceptDelegate != null)
            acceptButton.onClick.AddListener(delegate { acceptDelegate(); });
        if (denyDelegate != null)
            denyButton.onClick.AddListener(delegate { denyDelegate(); });
    }
}

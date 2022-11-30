using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ErrorWindowUI : WindowUI
{
    // Scene reference to button
    [SerializeField] private Button okButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI message;

    // Delegate to call back on clicks
    private ButtonDelegate okDelegate;

    internal override void Open()
    {
        // call base
        base.Open();

        // set the click detector so that clicking anywhere besides the menu, doesn't do anything
        GameManager.instance.BringClickDetectorToFront(null, transform, GameManager.instance.windowParent, true);

        // play error sound
        GameManager.instance.audioManager.PlaySound(AudioManager.Sound.Type.Error);
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
    /// <param name="error">The error to display</param>
    internal void SetText(ErrorDefinition error)
    {
        // set texts
        title.text = "ERROR: " + error.code;
        message.text = error.message + " (" + error.code + ") ";
    }

    /// <summary>
    /// Sets the delegate for callbacks.
    /// </summary>
    /// <param name="okDelegate">Delegate on ok clicked</param>
    internal void SetButtonDelegates(ButtonDelegate okDelegate)
    {
        // remember delegate
        this.okDelegate = okDelegate;

        // make the ok and close button close the window and execute the okdelegate
        okButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        okButton.onClick.AddListener(delegate { ClickClose(); });
        closeButton.onClick.AddListener(delegate { ClickClose(); });
        if (okDelegate != null)
        {
            okButton.onClick.AddListener(delegate { okDelegate(); });
            closeButton.onClick.AddListener(delegate { okDelegate(); });
        }
    }
}

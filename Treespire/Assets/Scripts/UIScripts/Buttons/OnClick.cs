using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OnClick : MonoBehaviour, IPointerClickHandler
{
    // whether the sound should wait for a double click
    [SerializeField] private bool soundOnDoubleClick;

    private Button button;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        // get components
        button = GetComponent<Button>();
    }

    /// <summary>
    /// Called on click.
    /// </summary>
    /// <param name="eventData">Eventdata of click event</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // no sounds if the button isn't active!
        if (button != null && !button.interactable)
            return;

        // make a sound on 1 or 2 clicks respectively
        if (soundOnDoubleClick)
        {
            if (eventData.clickCount == 2)
                GameManager.instance.audioManager.PlaySound(AudioManager.Sound.Type.ButtonClick);
        }
        else
            GameManager.instance.audioManager.PlaySound(AudioManager.Sound.Type.ButtonClick);
    }
}

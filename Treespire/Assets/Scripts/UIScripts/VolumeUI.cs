using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeUI : MonoBehaviour
{
    // scene references 
    [SerializeField] private GameObject volumeMenu;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Image buttonBackground;
    [SerializeField] private GameObject volumeOffObject;

    // references for menu open
    [SerializeField] private Color normalColor;
    [SerializeField] private Color menuOpenColor;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite menuOpenSprite;

    // whether the volume menu is open
    internal bool IsOpen { get { return volumeMenu.activeInHierarchy; } }

    /// <summary>
    /// Initializes the volume UI.
    /// </summary>
    internal void Initialize()
    {
        // call slider change to make sure it's up to date
        VolumeSliderChanged();
    }

    /// <summary>
    /// Opens slider menu
    /// </summary>
    internal void Open()
    {
        // activate the slider menu
        volumeMenu.SetActive(true);

        // set color and sprite
        buttonBackground.color = menuOpenColor;
        buttonBackground.sprite = menuOpenSprite;

        // set the click detector to close on click anywhere outside the slider menu
        GameManager.instance.BringClickDetectorToFront(Close, volumeMenu.transform, null);
    }

    /// <summary>
    /// Closes slider menu
    /// </summary>
    internal void Close()
    {
        // deactive the slider menu
        volumeMenu.SetActive(false);

        // set color and sprite
        buttonBackground.color = normalColor;
        buttonBackground.sprite = normalSprite;

        // reset the click detector 
        GameManager.instance.BringClickDetectorToBack(true);
    }

    /// <summary>
    /// Called when volume button is clicked to open / close the volume menu.
    /// </summary>
    public void VolumeButtonClicked()
    {
        // toggle open / close based on whether
        // the menu is active already
        if (volumeMenu.activeInHierarchy)
            Close();
        else
            Open();
    }

    /// <summary>
    /// Called on slider changed.
    /// </summary>
    public void VolumeSliderChanged()
    {
        // update the current volume based on slider value
        GameManager.instance.audioManager.OnVolumeChanged(volumeSlider.value);

        // set the volume off object on if slider is 0
        volumeOffObject.SetActive(volumeSlider.value == 0);
    }
}

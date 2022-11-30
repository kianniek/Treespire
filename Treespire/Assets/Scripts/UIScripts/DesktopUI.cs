using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DesktopUI : MonoBehaviour
{
    // reference to the background
    [SerializeField] private Image backgroundImage;

    // keep track of all the shortcuts
    private List<ShortcutUI> shortcuts;

    /// <summary>
    /// Call to initialize the desktop.
    /// </summary>
    internal void Initialize()
    {
        // load bytes for background image 
        byte[] pngBytes = System.IO.File.ReadAllBytes(GameManager.instance.GetStreamingAssetsPath + GameManager.SLASH + GameManager.instance.GetImagesPath + "/background.png");

        // create a texture to load the image in
        Texture2D tex = new Texture2D(2,2);
        tex.LoadImage(pngBytes);

        // create a new sprite from the texture
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

        // set the sprite as background
        backgroundImage.sprite = sprite;

        // initialize list for shortcuts
        shortcuts = new List<ShortcutUI>();
    }

    /// <summary>
    /// Call to add and keep track of a new shortcut
    /// </summary>
    /// <param name="shortcut"></param>
    internal void AddShortcut(ShortcutUI shortcut)
    {
        // add it to the list of shortcuts
        shortcuts.Add(shortcut);
    }

    /// <summary>
    /// Called once per frame
    /// </summary>
    private void Update()
    {
        // check for each short cut if the shortcut file exists
        bool fileExists = false;
        bool shortcutIsActive = false;

        for(int i = 0; i < shortcuts.Count; i++)
        {
            // get file and gameobject status
            fileExists = shortcuts[i].program.DesktopShortcutExists();
            shortcutIsActive = shortcuts[i].gameObject.activeInHierarchy;

            // if the file doesn't exists and the gameobject is enabled, disable it
            // and vise versa
            if (!fileExists && shortcutIsActive)
                shortcuts[i].gameObject.SetActive(false);
            else if (fileExists && !shortcutIsActive)
                shortcuts[i].gameObject.SetActive(true);
        }
    }
}

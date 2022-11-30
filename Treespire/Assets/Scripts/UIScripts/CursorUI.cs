using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorUI : MonoBehaviour
{
    // scene references 
    [SerializeField] private Image image;
    [SerializeField] private RectTransform rect;

    // sprites
    [SerializeField] private Sprite cursorSprite;
    [SerializeField] private Sprite cursorOverSprite;

    // particles
    [SerializeField] private GameObject trailEffect;
    [SerializeField] private Transform trailParent;
    [SerializeField] private float totalTimeBetweenTrailSpawn;
    private float timeBetweenTrailSpawn;

    // types of cursor
    private enum Type { Normal, Over }

    // vectors to calculate mouse position
    private Vector2 viewPortPos, mousePos;

    /// <summary>
    /// Called when script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        // get component
        rect = GetComponent<RectTransform>();

        // make normal cursor invisible
        Cursor.visible = false;

        // set cursor type to normal for starters
        SetCursorType(Type.Normal);
    }

    /// <summary>
    /// Called every frame.
    /// </summary>
    private void Update()
    {
        // disable normal cursor, 
        // since it can get back on focus etc.
        Cursor.visible = false;

        // update the position by converting positions
        // from screen (original mouse) to canvas (our mouse)
        viewPortPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        mousePos = new Vector2((viewPortPos.x * GameManager.instance.canvasRect.sizeDelta.x) - (GameManager.instance.canvasRect.sizeDelta.x * 0.5f),
                                        (viewPortPos.y * GameManager.instance.canvasRect.sizeDelta.y) - (GameManager.instance.canvasRect.sizeDelta.y * 0.5f));
        rect.anchoredPosition = mousePos;

        // spawn trail effect on current position every x secs
        if (timeBetweenTrailSpawn <= 0)
        {
            Instantiate(trailEffect, transform.position, Quaternion.identity, trailParent);
            timeBetweenTrailSpawn = totalTimeBetweenTrailSpawn;
        }
        else
            timeBetweenTrailSpawn -= Time.unscaledDeltaTime;
    }

    /// <summary>
    /// Called when clickable object is entered.
    /// </summary>
    internal void PointerEntered()
    {
        SetCursorType(Type.Over);
    }

    /// <summary>
    /// Called when clickable object is exited.
    /// </summary>
    internal void PointerExited()
    {
        SetCursorType(Type.Normal);
    }

    /// <summary>
    /// Changes the cursor sprite according to given type.
    /// </summary>
    /// <param name="type">Type to change to</param>
    private void SetCursorType(Type type)
    {
        switch (type)
        {
            case Type.Normal:
                image.sprite = cursorSprite;
                break;
            case Type.Over:
                image.sprite = cursorOverSprite;
                break;
        }
    }
}

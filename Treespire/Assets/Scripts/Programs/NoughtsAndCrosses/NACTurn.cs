using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NACTurn : MonoBehaviour
{
    // component references
    [SerializeField] private Image image;

    // sprites used in game
    [SerializeField] private Sprite noughtSprite;
    [SerializeField] private Sprite crossSprite;

    // quick color refs
    private readonly Color onColor = new Color(1, 1, 1, 1);
    private readonly Color offColor = new Color(1, 1, 1, 0);

    // keep track of corresponding coordinates
    internal int x { get; private set; }
    internal int y { get; private set; }

    // reference to the NAC program
    private NoughtsAndCrosses noughtsAndCrossesRef;

    /// <summary>
    /// Call to initialize the turn object.
    /// </summary>
    /// <param name="noughtsAndCrossesRef">A reference to the program</param>
    /// <param name="x">This x position on the board</param>
    /// <param name="y">This y position on the board</param>
    internal void Initialize(NoughtsAndCrosses noughtsAndCrossesRef, int x, int y)
    {
        // store coords
        this.x = x;
        this.y = y;

        // keep references
        this.noughtsAndCrossesRef = noughtsAndCrossesRef;
    }

    /// <summary>
    /// Called when this object is clicked.
    /// </summary>
    public void OnClick()
    {
        // call the program that this turn was clicked
        noughtsAndCrossesRef.ClickedTurn(x, y);
    }

    /// <summary>
    /// Set the objects color and image according to given type.
    /// </summary>
    /// <param name="turnType">Defines the color and image to set</param>
    internal void Set(TurnType turnType)
    {
        // handle each type differently
        switch (turnType)
        {
            // hide 
            case TurnType.None:
                image.sprite = null;
                image.color = offColor;
                break;

            // set to nought
            case TurnType.Nought:
                image.sprite = noughtSprite;
                image.color = onColor;
                break;

            // set to cross 
            case TurnType.Cross:
                image.sprite = crossSprite;
                image.color = onColor;
                break;
        }
    }
}

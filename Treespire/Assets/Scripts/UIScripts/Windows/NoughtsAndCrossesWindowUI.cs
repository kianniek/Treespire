using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NoughtsAndCrossesWindowUI : WindowUI
{
    // reference to the program
    private NoughtsAndCrosses gameRef;

    // references to objects for choosing state
    [SerializeField] private GameObject choosingStateParent;
    [SerializeField] private TextMeshProUGUI choosingTitle;

    // references to objects for ingame state
    [SerializeField] private GameObject inGameStateParent;
    [SerializeField] private GameObject turnPrefab;
    [SerializeField] private Transform turnParent;
    internal NACTurn[,] turns;

    // strings used
    private const string PROMPT_CHOOSE = "Play as nought? Or are you cross?";
    private const string WIN = "You win!\n";
    private const string LOSE = "You lose (haha)!\n";
    private const string DRAW = "It's a draw.\n";

    internal override void Initialize(Program program = null)
    {
        // call base
        base.Initialize(program);

        // keep a ref to the casted program
        gameRef = (NoughtsAndCrosses)program;

        // add turn objects to the board
        turns = new NACTurn[NoughtsAndCrosses.BOARD_SIZE, NoughtsAndCrosses.BOARD_SIZE];
        for(int y = 0; y < NoughtsAndCrosses.BOARD_SIZE; y++)
        {
            for (int x = 0; x < NoughtsAndCrosses.BOARD_SIZE; x++)
            {
                // instantiate the object under the parent
                turns[x, y] = Instantiate(turnPrefab, turnParent).GetComponent<NACTurn>();

                // initialize it 
                turns[x, y].Initialize(gameRef, x, y);

                // give it a recognizable name
                turns[x, y].name = string.Format("Turn[{0},{1}]", x, y);
            }
        }
    }

    internal override void Open()
    {
        // call base
        base.Open();

        // set title
        title.text = program.programName;
    }

    /// <summary>
    /// Called when program goes to choosing state.
    /// </summary>
    internal void ShowChoosing()
    {
        // enable correct gameobjects
        choosingStateParent.SetActive(true);
        inGameStateParent.SetActive(false);

        // set the title 
        choosingTitle.text = PROMPT_CHOOSE;
    }

    /// <summary>
    /// Called when program goes to ingame state.
    /// </summary>
    internal void ShowIngame()
    {
        // enable correct gameobjects
        choosingStateParent.SetActive(false);
        inGameStateParent.SetActive(true);

        // make sure all turns are reset
        for (int y = 0; y < NoughtsAndCrosses.BOARD_SIZE; y++)
            for (int x = 0; x < NoughtsAndCrosses.BOARD_SIZE; x++)
                turns[x,y].Set(TurnType.None);
    }

    /// <summary>
    /// Called when program goes to gameover state.
    /// </summary>
    internal void ShowGameOver(TurnType won, TurnType player)
    {
        // enable correct gameobjects, 
        // note: we go back to the visuals for the choosing state
        // but we add a win / lose / draw text 
        choosingStateParent.SetActive(true);
        inGameStateParent.SetActive(false);

        // set the text according to who won 
        switch (won)
        {
            case TurnType.Cross:
                choosingTitle.text = won == player ? WIN : LOSE;
                break;
            case TurnType.Nought:
                choosingTitle.text = won == player ? WIN : LOSE;
                break;
            case TurnType.None:
                choosingTitle.text = DRAW;
                break;
        }

        // add the 'choose' text
        choosingTitle.text += PROMPT_CHOOSE;
    }

    /// <summary>
    /// Called after start and after every turn 
    /// to show the visual board updated.
    /// </summary>
    /// <param name="currentBoard">The data of the current board</param>
    internal void UpdateBoard(NACBoard currentBoard)
    {
        // set each turn object as to the corresponding data
        for (int y = 0; y < NoughtsAndCrosses.BOARD_SIZE; y++)
            for (int x = 0; x < NoughtsAndCrosses.BOARD_SIZE; x++)
                turns[x, y].Set(currentBoard.coordinates[x, y].playedType);
    }

    /// <summary>
    /// Called when the user clicks either a nought
    /// or a cross in the choosing screen.
    /// </summary>
    /// <param name="cross">Whether cross was clicked or not</param>
    public void ClickChoose(bool cross)
    {
        // pass to the program which choice the player made
        gameRef.ClickedChoose(cross ? TurnType.Cross : TurnType.Nought);
    }
}


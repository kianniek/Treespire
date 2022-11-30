using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal enum TurnType { None, Nought, Cross }

[CreateAssetMenu(menuName = "Programs/Noughts and Crosses")]
public class NoughtsAndCrosses : Program
{
    // reference to the game window prefab
    [SerializeField] private GameObject gameWindowPrefab;

    // reference to the instantiated game window object
    private NoughtsAndCrossesWindowUI gameWindow;

    // game states to keep track of
    // what texts to show and
    // what input to expect
    private GameState gameState = 0;
    private enum GameState
    {
        Opening,
        InGame,
        GameOver
    }

    // data of the board
    private NACBoard board;
    internal const int BOARD_SIZE = 3;

    // as what does the player play?
    internal TurnType playersTurn = TurnType.Cross;

    // keep track of current turn
    private TurnType currentTurn;
    private int turnCount;

    // reference to routine for AI turn
    // so that it can be quit if the program shuts down
    private Coroutine aiRoutine;

    // keep track of the current best move for the AI
    private NACCoord bestMove;

    // who won?
    private TurnType winner;

    internal override void Initialize()
    {
        // call base
        base.Initialize();

        // instantiate the prefab window
        gameWindow = Instantiate(gameWindowPrefab, GameManager.instance.windowParent).GetComponent<NoughtsAndCrossesWindowUI>();

        // setup the window
        gameWindow.Initialize(this);
        gameWindow.Close();

        // programs window is same as game window
        // but a different type
        window = gameWindow;
    }

    internal override bool StartUp()
    {
        // call base
        if (!base.StartUp())
            return false;

        // start with no turns yet
        turnCount = 0;

        // start in the choosing state (play with x or o?)
        GotoState(GameState.Opening);

        // succesfully started 
        return true;

        // wait for input
    }

    internal override void ShutDown()
    {
        // make sure the AI routine is stopped
        if (aiRoutine != null)
            GameManager.instance.StopCoroutine(aiRoutine);

        // call base
        base.ShutDown();
    }

    /// <summary>
    /// Goes to the given state from the current one.
    /// </summary>
    /// <param name="gameState">The state to go to</param>
    private void GotoState(GameState gameState)
    {
        // keep track of the current state
        this.gameState = gameState;

        // start the state 
        switch (gameState)
        {
            case GameState.Opening:
                // show the choosing view
                gameWindow.ShowChoosing();
                break;

            case GameState.GameOver:
                // show the game over view with a small delay
                GameManager.instance.StartCoroutine(GotoGameOverDelayed());
                break;

            case GameState.InGame:
                // start with a new data board 
                board = new NACBoard(this);

                // show the in game view
                gameWindow.ShowIngame();

                // play AI turn or wait?
                if (playersTurn != currentTurn)
                    aiRoutine = GameManager.instance.StartCoroutine(PlayAIRoutine());
                break;
        }
    }

    /// <summary>
    /// Called after the player chose what he plays, 
    /// either nought or cross.
    /// </summary>
    /// <param name="turnType">Type chosen by player</param>
    internal void ClickedChoose(TurnType turnType)
    {
        // error if none was somehow passed!
        if(turnType == TurnType.None)
        {
            Debug.LogError("Have to play with either nought or cross! None was chosen");
            return;
        }

        // remember what the player choose
        playersTurn = turnType;

        // cross always starts
        currentTurn = TurnType.Cross;

        // and progress to ingame state
        GotoState(GameState.InGame);
    }

    /// <summary>
    /// Called by the turn objects when they are clicked.
    /// </summary>
    /// <param name="x">The x coordinate of the turn</param>
    /// <param name="y">The y coordinate of the turn</param>
    internal void ClickedTurn(int x, int y)
    {
        // can only click ingame
        if (gameState != GameState.InGame)
            return;

        // can only click if it's the players turn
        if (playersTurn != currentTurn)
            return;

        // play the turn
        PlayTurn(x, y);
    }

    /// <summary>
    /// Resolves a played turn if it is valid.
    /// </summary>
    /// <param name="x">The x coordinate of the turn</param>
    /// <param name="y">The y coordinate of the turn</param>
    private void PlayTurn(int x, int y)
    {
        // check whether this is a valid play
        if (board.coordinates[x, y].played)
            return;

        // set the turn to played
        board.PlayCoordinate(x, y, currentTurn);

        // increase turn count
        turnCount++;

        // visually update the board
        gameWindow.UpdateBoard(board);

        // check for ending the game by
        // either a win, lose or draw
        if (board.CheckWin())
        {
            // the winner is the one that
            // played the current turn
            winner = currentTurn;

            // go to the game over state
            GotoState(GameState.GameOver);

            // done
            return;
        }
        else if (board.CheckDraw())
        {
            // no winners here
            winner = TurnType.None;

            // goto game over state
            GotoState(GameState.GameOver);

            // done
            return;
        }

        // it's the others turn now
        if (currentTurn == TurnType.Cross)
            currentTurn = TurnType.Nought;
        else
            currentTurn = TurnType.Cross;

        // play AI turn or wait?
        if (playersTurn != currentTurn)
        {
            // play AI turn
            aiRoutine = GameManager.instance.StartCoroutine(PlayAIRoutine());
        }
    }

    /// <summary>
    /// Determines and plays one AI turn.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PlayAIRoutine()
    {
        // brief pause...
        yield return new WaitForSeconds(0.5f);

        // build the game tree for x layers
        Tree<NACBoard> gameTree = BuildCurrentGameTree(10);

        // execute minimax on the game tree 
        // to decide the best move for the AI
        Minimax(gameTree.rootNode, true);

        // play the best move for the AI
        PlayTurn(bestMove.x, bestMove.y);
    }

    /// <summary>
    /// Builds a game tree with a given amount of levels 
    /// starting at the current state of the board.
    /// </summary>
    /// <param name="levels">The amount of levels to build</param>
    /// <returns>A game tree with the possible next states of the board</returns>
    private Tree<NACBoard> BuildCurrentGameTree(int levels)
    {
        // define the new tree
        Tree<NACBoard> gameTree = new Tree<NACBoard>();

        // the root is a copy of the current board
        gameTree.rootNode = new Node<NACBoard>(new NACBoard(board));

        // call expand on the root node, 
        // which will call it on all its nodes etc.
        // until end state or max level is reached
        ExpandCurrentNode(gameTree.rootNode, currentTurn, 2, levels);

        // shuffle the nodes so that it picks different options
        // when the evaluated scores are the same
        ShuffleChildNodes(gameTree.rootNode);

        // return resulting tree
        return gameTree;
    }

    /// <summary>
    /// Called by BuildCurrentGameTree to recursively build out the given node.
    /// </summary>
    /// <param name="currentNode">The node to build out</param>
    /// <param name="currentTurn">Whether it's the turn for noughts or crosses</param>
    /// <param name="currentLevel">The current level of the tree</param>
    /// <param name="maxLevel">The max level to build out to</param>
    /// <param name="gameTree">Reference to the whole game tree</param>
    private void ExpandCurrentNode(Node<NACBoard> currentNode, TurnType currentTurn, int currentLevel, int maxLevel)
    {
        // if this is an end state, no need to build further
        if (currentNode.content.CheckEnd())
            return;

        // if this is the max level, no need to build further
        if (currentLevel >= maxLevel)
            return;

        // the current node has as many children as
        // there are free coordinates on the currents board
        currentNode.childNodes = new Node<NACBoard>[currentNode.content.freeCoordinates];

        // determine the next turn
        TurnType nextTurn = currentTurn == TurnType.Cross ? TurnType.Nought : TurnType.Cross;

        // keep track of the current child node index
        // differs from i since i checks every spot instead of only free spots
        int childNodeIndex = 0;

        // evaluate and expand the tree for each free coordinate
        for (int y = 0; y < BOARD_SIZE; y++)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                // continue if this coordinate is played
                if (currentNode.content.coordinates[x, y].played)
                    continue;

                // set the child nodes board equal to a copy of the current
                currentNode.childNodes[childNodeIndex] = new Node<NACBoard>(new NACBoard(currentNode.content));

                // play the turn on the child nodes board
                currentNode.childNodes[childNodeIndex].content.PlayCoordinate(x, y, currentTurn);

                // and expand recursively on this child node
                ExpandCurrentNode(currentNode.childNodes[childNodeIndex], nextTurn, ++currentLevel, maxLevel);

                // increase child counter
                // to evaluate the next one
                childNodeIndex++;
            }
        }
    }

    /// <summary>
    /// Shuffles all child nodes of the given node and all their children.
    /// </summary>
    /// <param name="node">The node to shuffle</param>
    private void ShuffleChildNodes(Node<NACBoard> node)
    {
        // nothing to shuffle if this is a leaf
        if (node.IsLeafNode())
            return;

        // shuffle it's children
        node.childNodes.Shuffle();

        // and call this method recursively on all it's children
        for (int i = 0; i < node.childNodes.Length; i++)
            ShuffleChildNodes(node.childNodes[i]);
    }

    /// <summary>
    /// Determines the best move to play 
    /// based on the minimax algorithm.
    /// </summary>
    /// <param name="node">The node to evaluate</param>
    /// <param name="max">Whether to minimize or maximize</param>
    /// <returns>The best score</returns>
    private int Minimax(Node<NACBoard> node, bool max)
    {
        // TODO: implement this function so that
        // 1. it returns the best score  
        // 2. and it saves the corresponding move to play in bestMove

        // use node.content.Evaluate() to get the evaluated score of that node
        // use GetMaxIndex() and GetMinIndex() to get the index of the highest or lowest score
        // call Minimax recursively for all child nodes, until it's a leaf node
        // alternate between max and min levels 

        // TEMP code to pick the first possible move
        bestMove = node.childNodes[0].content.lastSetCoordinate;
        return 0;
    }

    /// <summary>
    /// Get the index of the highest int in an array.
    /// </summary>
    /// <param name="results">The array to pick the highest int from</param>
    /// <returns>The index of the highest int</returns>
    private int GetMaxIndex(int[] results)
    {
        // init best score and index 
        int bestScore = Int32.MinValue;
        int bestIndex = -1;

        // go over the array 
        for (int i = 0; i < results.Length; i++)
        {
            // if the score is higher than the one we already got, 
            // update the best score and index
            if(results[i] > bestScore)
            {
                bestScore = results[i];
                bestIndex = i;
            }
        }

        // return the best index
        return bestIndex;
    }

    /// <summary>
    /// Get the index of the lowest int in an array.
    /// </summary>
    /// <param name="results">The array to pick the lowest int from</param>
    /// <returns>The index of the lowest int</returns>
    private int GetMinIndex(int[] results) 
    {
        // init best score and index 
        int bestScore = Int32.MaxValue;
        int bestIndex = -1;

        // go over the array 
        for (int i = 0; i < results.Length; i++)
        {
            // if the score is lower than the one we already got, 
            // update the best score and index
            if (results[i] < bestScore)
            {
                bestScore = results[i];
                bestIndex = i;
            }
        }

        // return the best index
        return bestIndex;
    }

    /// <summary>
    /// Go to game over with a short delay.
    /// </summary>
    /// <returns></returns>
    private IEnumerator GotoGameOverDelayed()
    {
        // short wait... 
        yield return new WaitForSeconds(1f);

        // show the game over view
        gameWindow.ShowGameOver(winner, playersTurn);
    }
}

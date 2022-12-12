using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NACCoord
{
    // x and y coordinates 
    // defines where this coord is
    internal int x;
    internal int y;

    // whether this coord is played
    // and what type as been played (is none if not played)
    internal bool played;
    internal TurnType playedType;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="x">X position of this coord</param>
    /// <param name="y">Y position of this coord</param>
    internal NACCoord(int x, int y)
    {
        // store the x and y
        this.x = x;
        this.y = y;

        // start not played
        played = false;
        playedType = TurnType.None;
    }
}

public class NACBoard 
{
    // all coordinates and their data in a 2D array,
    // represents the board
    internal NACCoord[,] coordinates = new NACCoord[NoughtsAndCrosses.BOARD_SIZE, NoughtsAndCrosses.BOARD_SIZE];

    // the last coordinate set on this board
    internal NACCoord lastSetCoordinate;

    // keeps track of the amount of free coordinates left on the board
    internal int freeCoordinates;

    // reference to the noughts and crosses game
    private NoughtsAndCrosses gameRef;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="gameRef">Reference to the game this board belongs to</param>
    internal NACBoard(NoughtsAndCrosses gameRef)
    {
        // keep a ref to the game
        this.gameRef = gameRef;

        // as many free coordinates as there are places on the board
        freeCoordinates = NoughtsAndCrosses.BOARD_SIZE * NoughtsAndCrosses.BOARD_SIZE;
        
        // define the board as a 2D array of coordinates
        coordinates = new NACCoord[NoughtsAndCrosses.BOARD_SIZE, NoughtsAndCrosses.BOARD_SIZE];

        // create coordinates with the correct position 
        for (int y = 0; y < NoughtsAndCrosses.BOARD_SIZE; y++)
            for (int x = 0; x < NoughtsAndCrosses.BOARD_SIZE; x++)
                coordinates[x, y] = new NACCoord(x, y);
    }

    /// <summary>
    /// Copy-constructor.
    /// </summary>
    /// <param name="board">NACBoard to copy</param>
    internal NACBoard(NACBoard board)
    {
        // copy variables
        freeCoordinates = board.freeCoordinates;
        lastSetCoordinate = board.lastSetCoordinate;
        gameRef = board.gameRef;

        // re-create this board as the given one
        coordinates = new NACCoord[NoughtsAndCrosses.BOARD_SIZE, NoughtsAndCrosses.BOARD_SIZE];
        for (int y = 0; y < NoughtsAndCrosses.BOARD_SIZE; y++)
        {
            for (int x = 0; x < NoughtsAndCrosses.BOARD_SIZE; x++)
            {
                coordinates[x, y] = board.coordinates[x, y];
            }
        }
    }

    /// <summary>
    /// Call to play a coordinate on the board.
    /// </summary>
    /// <param name="x">The x position of the coord</param>
    /// <param name="y">The y position of the coord</param>
    /// <param name="type">The type to play (x or o)</param>
    /// <returns>True if the move was valid</returns>
    internal bool PlayCoordinate(int x, int y, TurnType type)
    {
        // cannot play a coordinate that already has been played
        if (coordinates[x, y].played)
            return false;

        // set the coordinate to played w/ correct type
        coordinates[x, y].played = true;
        coordinates[x, y].playedType = type;

        // this is now the last played coordinate
        lastSetCoordinate = coordinates[x, y];

        // one less free coordinate
        freeCoordinates--;

        // succesful play
        return true;
    }

    /// <summary>
    /// Call to play a coordinate on the board.
    /// </summary>
    /// <param name="coordinate">The coordinate to play</param>
    /// <returns>True if the move was valid</returns>
    internal bool PlayCoordinate(NACCoord coordinate)
    {
        return PlayCoordinate(coordinate.x, coordinate.y, coordinate.playedType);
    }

    internal void ClearBoard()
    {
        freeCoordinates = NoughtsAndCrosses.BOARD_SIZE * NoughtsAndCrosses.BOARD_SIZE;

        for (int y = 0; y < NoughtsAndCrosses.BOARD_SIZE; y++)
        {
            for (int x = 0; x < NoughtsAndCrosses.BOARD_SIZE; x++)
            {
                coordinates[x, y].played = false;
                coordinates[x, y].playedType = TurnType.None;
            }
        }
    }

    /// <summary>
    /// Checks whether this board is in an end state.
    /// </summary>
    /// <returns>True if the board is in an end state</returns>
    internal bool CheckEnd()
    {
        // in an end state means it's a draw or a win
        return CheckDraw() || CheckWin();
    }

    /// <summary>
    /// Checks whether this board is in a draw state.
    /// </summary>
    /// <returns>True if the board is in a draw state</returns>
    internal bool CheckDraw()
    {
        // it's a draw if
        // there are no free spaces anymore
        // and it isn't a win
        return freeCoordinates == 0 && !CheckWin();
    }

    /// <summary>
    /// Checks whether this board is in a win state.
    /// </summary>
    /// <returns>True if the board is in a win state</returns>
    internal bool CheckWin()
    {
        // no need to check further
        // if there were less than 3 turns
        if (freeCoordinates >= coordinates.Length - NoughtsAndCrosses.BOARD_SIZE)
            return false;

        // check horizontals
        if (CompareCoordinates(coordinates[0, 0], coordinates[1, 0], coordinates[2, 0]) != TurnType.None ||
            CompareCoordinates(coordinates[0, 1], coordinates[1, 1], coordinates[2, 1]) != TurnType.None ||
            CompareCoordinates(coordinates[0, 2], coordinates[1, 2], coordinates[2, 2]) != TurnType.None)
            return true;

        // check verticals
        if (CompareCoordinates(coordinates[0, 0], coordinates[0, 1], coordinates[0, 2]) != TurnType.None ||
            CompareCoordinates(coordinates[1, 0], coordinates[1, 1], coordinates[1, 2]) != TurnType.None ||
            CompareCoordinates(coordinates[2, 0], coordinates[2, 1], coordinates[2, 2]) != TurnType.None)
            return true;

        // check diagonals
        if (CompareCoordinates(coordinates[0, 0], coordinates[1, 1], coordinates[2, 2]) != TurnType.None ||
            CompareCoordinates(coordinates[0, 2], coordinates[1, 1], coordinates[2, 0]) != TurnType.None)
            return true;

        return false;
    }

    /// <summary>
    /// Compares the types of the given coordinates.
    /// </summary>
    /// <param name="coord1">The first coordinate</param>
    /// <param name="coord2">The second coordinate</param>
    /// <param name="coord3">The third coordinate</param>
    /// <returns>The type all compared coordinates share or 
    /// none if they don't all share the same type</returns>
    private TurnType CompareCoordinates(NACCoord coord1, NACCoord coord2, NACCoord coord3)
    {
        // no match if any isn't played yet
        if (!coord1.played || !coord2.played || !coord3.played)
            return TurnType.None;

        // match if all played types are equal
        if ((coord1.playedType == coord2.playedType) && (coord2.playedType == coord3.playedType))
            return coord1.playedType;

        // return none if they didn't match
        return TurnType.None;
    }

    /// <summary>
    /// Evaluates and scores the current state of the board.
    /// </summary>
    /// <returns>The evaluated score</returns>
    //internal int Evaluate()
    //{
    //     //compare horizontals
    //    TurnType h1 = CompareCoordinates(coordinates[0, 0], coordinates[1, 0], coordinates[2, 0]);
    //    TurnType h2 = CompareCoordinates(coordinates[0, 1], coordinates[1, 1], coordinates[2, 1]);
    //    TurnType h3 = CompareCoordinates(coordinates[0, 2], coordinates[1, 2], coordinates[2, 2]);

    //     //compare vertical
    //    TurnType v1 = CompareCoordinates(coordinates[0, 0], coordinates[0, 1], coordinates[0, 2]);
    //    TurnType v2 = CompareCoordinates(coordinates[1, 0], coordinates[1, 1], coordinates[1, 2]);
    //    TurnType v3 = CompareCoordinates(coordinates[2, 0], coordinates[2, 1], coordinates[2, 2]);

    //    // compare diagonals
    //    TurnType d1 = CompareCoordinates(coordinates[0, 0], coordinates[1, 1], coordinates[2, 2]);
    //    TurnType d2 = CompareCoordinates(coordinates[0, 2], coordinates[1, 1], coordinates[2, 0]);

    //     //is there a win for cross? 
    //     //return -1 if the player plays cross, else 1
    //    if (h1 == TurnType.Cross || h2 == TurnType.Cross || h3 == TurnType.Cross ||
    //       v1 == TurnType.Cross || v2 == TurnType.Cross || v3 == TurnType.Cross ||
    //       d1 == TurnType.Cross || d2 == TurnType.Cross)
    //        return gameRef.playersTurn == TurnType.Cross ? -1 : 1;
    //     //is there a win for nought?
    //     //return -1 if the player plays nought, else 1
    //    else if (h1 == TurnType.Nought || h2 == TurnType.Nought || h3 == TurnType.Nought ||
    //       v1 == TurnType.Nought || v2 == TurnType.Nought || v3 == TurnType.Nought ||
    //       d1 == TurnType.Nought || d2 == TurnType.Nought)
    //        return gameRef.playersTurn == TurnType.Nought ? -1 : 1;
    //     //if there is no win (yet), return 0
    //    else
    //        return 0;
    //}

    internal int Evaluate()
    {
        TurnType turnType1 = this.CompareCoordinates(this.coordinates[0, 0], this.coordinates[1, 0], this.coordinates[2, 0]);
        TurnType turnType2 = this.CompareCoordinates(this.coordinates[0, 1], this.coordinates[1, 1], this.coordinates[2, 1]);
        TurnType turnType3 = this.CompareCoordinates(this.coordinates[0, 2], this.coordinates[1, 2], this.coordinates[2, 2]);
        TurnType turnType4 = this.CompareCoordinates(this.coordinates[0, 0], this.coordinates[0, 1], this.coordinates[0, 2]);
        TurnType turnType5 = this.CompareCoordinates(this.coordinates[1, 0], this.coordinates[1, 1], this.coordinates[1, 2]);
        TurnType turnType6 = this.CompareCoordinates(this.coordinates[2, 0], this.coordinates[2, 1], this.coordinates[2, 2]);
        TurnType turnType7 = this.CompareCoordinates(this.coordinates[0, 0], this.coordinates[1, 1], this.coordinates[2, 2]);
        TurnType turnType8 = this.CompareCoordinates(this.coordinates[0, 2], this.coordinates[1, 1], this.coordinates[2, 0]);

        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {

            }
        }
        if (turnType1 == TurnType.Cross || turnType2 == TurnType.Cross || turnType3 == TurnType.Cross || turnType4 == TurnType.Cross || turnType5 == TurnType.Cross || turnType6 == TurnType.Cross || turnType7 == TurnType.Cross || turnType8 == TurnType.Cross)
        {
            return this.gameRef.playersTurn != TurnType.Cross ? 1 : -1;
        }

        if (turnType1 != TurnType.Nought && turnType2 != TurnType.Nought && turnType3 != TurnType.Nought && turnType4 != TurnType.Nought && turnType5 != TurnType.Nought && turnType6 != TurnType.Nought && turnType7 != TurnType.Nought && turnType8 != TurnType.Nought)
        {
            return 0;
        }
        return this.gameRef.playersTurn != TurnType.Nought ? 1 : -1;
    }

    //#region evluate Test
    //internal int Evaluate()
    //{
    //    int score = 0;
    //    // Evaluate score for each of the 8 lines (3 rows, 3 columns, 2 diagonals)
    //    score += EvaluateLine(0, 0, 0, 1, 0, 2);  // row 0
    //    score += EvaluateLine(1, 0, 1, 1, 1, 2);  // row 1
    //    score += EvaluateLine(2, 0, 2, 1, 2, 2);  // row 2
    //    score += EvaluateLine(0, 0, 1, 0, 2, 0);  // col 0
    //    score += EvaluateLine(0, 1, 1, 1, 2, 1);  // col 1
    //    score += EvaluateLine(0, 2, 1, 2, 2, 2);  // col 2
    //    score += EvaluateLine(0, 0, 1, 1, 2, 2);  // diagonal
    //    score += EvaluateLine(0, 2, 1, 1, 2, 0);  // alternate diagonal
    //    return score;
    //}

    ///** The heuristic evaluation function for the given line of 3 cells
    //    @Return +100, +10, +1 for 3-, 2-, 1-in-a-line for computer.
    //            -100, -10, -1 for 3-, 2-, 1-in-a-line for opponent.
    //            0 otherwise */
    //private int EvaluateLine(int row1, int col1, int row2, int col2, int row3, int col3)
    //{
    //    int score = 0;

    //    // First cell
    //    if (this.coordinates[row1, col1].playedType == TurnType.Cross)
    //    {
    //        score = 1;
    //    }
    //    else if (this.coordinates[row1,col1].playedType == TurnType.Nought)
    //    {
    //        score = -1;
    //    }

    //    // Second cell
    //    if (this.coordinates[row2,col2].playedType == TurnType.Cross)
    //    {
    //        if (score == 1)
    //        {   // cell1 is mySeed
    //            score = 10;
    //        }
    //        else if (score == -1)
    //        {  // cell1 is oppSeed
    //            return 0;
    //        }
    //        else
    //        {  // cell1 is empty
    //            score = 1;
    //        }
    //    }
    //    else if (this.coordinates[row2,col2].playedType == TurnType.Nought)
    //    {
    //        if (score == -1)
    //        { // cell1 is oppSeed
    //            score = -10;
    //        }
    //        else if (score == 1)
    //        { // cell1 is mySeed
    //            return 0;
    //        }
    //        else
    //        {  // cell1 is empty
    //            score = -1;
    //        }
    //    }

    //    // Third cell
    //    if (this.coordinates[row3, col3].playedType == TurnType.Cross)
    //    {
    //        if (score > 0)
    //        {  // cell1 and/or cell2 is mySeed
    //            score *= 10;
    //        }
    //        else if (score < 0)
    //        {  // cell1 and/or cell2 is oppSeed
    //            return 0;
    //        }
    //        else
    //        {  // cell1 and cell2 are empty
    //            score = 1;
    //        }
    //    }
    //    else if (this.coordinates[row3, col3].playedType == TurnType.Nought)
    //    {
    //        if (score < 0)
    //        {  // cell1 and/or cell2 is oppSeed
    //            score *= 10;
    //        }
    //        else if (score > 1)
    //        {  // cell1 and/or cell2 is mySeed
    //            return 0;
    //        }
    //        else
    //        {  // cell1 and cell2 are empty
    //            score = -1;
    //        }
    //    }
    //    return score;
    //}
    //#endregion
}
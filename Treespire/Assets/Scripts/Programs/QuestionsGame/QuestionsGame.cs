using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "Programs/20 Questions Game")]
public class QuestionsGame : Program
{
    // reference to the game window prefab
    [SerializeField] private GameObject gameWindowPrefab;

    // reference to the instantiated game window object
    private QuestionsGameWindowUI gameWindow;

    // game states to keep track of
    // what texts to show and
    // what input to expect
    private GameStates gameState = 0;
    private enum GameStates
    {
        Opening,

        NewGame,
        PromptNewOrExisting, 

        InGame,
        PromptCorrectAnswer,
        PromptAnother,

        PromptNewQuestion,
        PromptNewYesAnswer,
        PromptNewNoAnswer,

        PromptAnswer,
        PromptQuestion,
        PromptYNAnswer,

        Ending
    }

    // texts for dialog
    #region TEXTS FOR DIALOG
    private const string OPENING_MESSAGE = "Welcome to 20 Questions - The Game!";

    private const string NEW_GAME = "It seems like there isn't any stored knowledge. I'll initialize a new game.";
    private const string KNOWLEDGE_DELETED = "My knowledge ... is ... fading.";
    private const string KNOWLEDGE_FOUND = "I have stored knowledge!";
    private const string PROMPT_NEW_OR_EXISTING_GAME = "Play with existing knowledge? Type 'yes'. Or start fresh? Type 'no'.";
    private const string PROMPT_REINPUT_INCORRECT_INPUT = "I don't understand? Please enter again?";
    private const string PROMPT_CORRECT_ANSWER = "Are you thinking of";

    private const string PROMPT_NEW_TREE_QUESTION = "Enter a question to start with:";
    private const string PROMPT_NEW_TREE_YES = "Interesting! Now enter a possible guess if the response would be yes.";
    private const string PROMPT_NEW_TREE_NO = "And enter a possible guess for no.";
    private const string NEW_KNOWLEDGE = "I've created new stored knowledge.";

    private const string PROMPT_TREE_QUESTION_1 = "WHAT?! How could I have distinguished that from ";
    private const string PROMPT_TREE_QUESTION_2 = "? Please enter a question.";
    private const string PROMPT_TREE_ANSWER_1 = "If you were thinking of";
    private const string PROMPT_TREE_ANSWER_2 = ", what would the answer to the question be? Yes or no?";
    private const string UPDATED_KNOWLEDGE = "Ha! One more step towards knowing EVERTYTHING!";

    private const string NOTE_ON_ENTERING_ANSWERS = "FYI: If you start the word with a capital case, I'll assume it's a name. " +
        "Also, don't use an article before your answer.";

    private const string WIN = "Wow! You win! \nBut... what were you thinking of??";
    private const string WIN_LAST_QUESTION = "So many questions, I give up! You win!";
    private const string LOSE = "Knew it! Ha!";

    private const string PROMPT_PLAY_AFTER_NEW = "Now I can play! Do you wanna?";
    private const string PROMPT_PLAY_AGAIN = "So much fun! Play again with me?";
    private const string BYE = "Bye! Thanks for playing ~ !";

    private const string A_ARTICLE = "a";
    private const string AN_ARTICLE = "an";
    private readonly List<char> vowels = new List<char> { 'A', 'E', 'I', 'O', 'U' };

    private string[] yesAnswers = new string[] { "yes", "y", "yea", "yeah", "uhu", "yash" };
    private string[] noAnswers = new string[] { "no", "n", "na", "nah", "nope", "nein" };
    #endregion

    // path to knowledge file
    internal string GetQGKnowledgeFilePath { get { return GameManager.instance.GetStreamingAssetsPath + GameManager.SLASH + GameManager.instance.GetDocumentsPath + GameManager.SLASH + "20Questions" + GameManager.SLASH + "knowledge.xml"; } }

    // a reference to the tree
    // since the tree knows it's nodes
    private BinaryTree tree = null;

    // variables for when updating the tree
    private bool updatingTree = false;
    private bool updateIsNew = false;
    private string newQuestion = string.Empty;
    private string newYesAnswer = string.Empty;
    private string newNoAnswer = string.Empty;
    private string newTempAnswer = string.Empty;

    internal override void Initialize()
    {
        // call base
        base.Initialize();

        // instantiate the prefab window
        gameWindow = GameObject.Instantiate(gameWindowPrefab, GameManager.instance.windowParent).GetComponent<QuestionsGameWindowUI>();

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

        // set opening message
        gameWindow.AppendToMessage(OPENING_MESSAGE, false, false);

        // start in opening state
        gameState = GameStates.Opening;

        // start the game!
        StartGame();

        // succesfully started 
        return true;
        
        // wait for input, ResolveInput() will be called when done waiting.
    }

    internal override void Focus(bool inFocus)
    {
        base.Focus(inFocus);

        if (inFocus)
            gameWindow.SelectInputField();
    }

    /// <summary>
    /// Starts a new game, 
    /// checks and prompts whether to do that with new or existing knowledge.
    /// </summary>
    private void StartGame()
    {
        // check if we have knowledge stored
        bool knowledgeLoaded = false;

        // the file should exist
        if (File.Exists(GetQGKnowledgeFilePath))
        {
            // and the file should contain a valid tree
            tree = XMLSerializer.Deserialize<BinaryTree>(GetQGKnowledgeFilePath);
            if (tree != null && tree.rootNode != null && tree.rootNode.leftNode != null && tree.rootNode.rightNode != null)
                knowledgeLoaded = true;
        }

        // if there is knowledge stored and loaded, new or existing game can be played
        // else new game is started
        if (knowledgeLoaded)
        {
            // choose between new game and existing
            gameWindow.AppendToMessage(KNOWLEDGE_FOUND, true, false);
            gameWindow.AppendToMessage(PROMPT_NEW_OR_EXISTING_GAME, true, false);

            // and go to appropriate game state
            gameState = GameStates.PromptNewOrExisting;
        }
        else
        {
            // show new game message
            gameWindow.AppendToMessage(NEW_GAME, true, false);

            // and go to appropriate game state
            gameState = GameStates.NewGame;

            // start the new game
            StartNewGame();
        }
    }

    /// <summary>
    /// Starting the game with existing knowledge.
    /// </summary>
    private void StartExistingGame()
    {
        // goto ingame state
        gameState = GameStates.InGame;

        // start the query, queries first node
        tree.StartIterating();

        // started first iteration, resolve it
        // printing the nodes question and setting up for input
        ResolveIteration();

        // wait for finishing prompt y/n for left/right nodes
        // ResolveInput is called again with new input
    }

    /// <summary>
    /// Deletes current tree and currently stored knowledge.
    /// </summary>
    private void DeleteExistingKnowledge()
    {
        // delete tree if we had any
        if (tree != null)
            tree = null;

        // delete tree file
        File.Delete(GetQGKnowledgeFilePath);
    }

    /// <summary>
    /// Starting the game without knowledge. 
    /// Prompts for starting knowledge.
    /// </summary>
    private void StartNewGame()
    {
        // setup for updating tree
        updatingTree = true;
        updateIsNew = true;
        newQuestion = string.Empty;
        newYesAnswer = string.Empty;
        newNoAnswer = string.Empty;
        newTempAnswer = string.Empty;
        tree = null;

        // prompts for question
        gameWindow.AppendToMessage(PROMPT_NEW_TREE_QUESTION, true, false);

        // wait for input
        gameState = GameStates.PromptNewQuestion;
    }

    /// <summary>
    /// Iterates to the next node and resolves it.
    /// </summary>
    private void IterateNode(bool left)
    {
        // iterate to next node
        tree.IterateNext(left);

        // and resolve it
        ResolveIteration();
    }

    /// <summary>
    /// Resolves iteration with new current node
    /// </summary>
    private void ResolveIteration()
    {
        // if the current node is a leaf node,
        // it's an answer
        if (tree.currentNode.IsLeafNode())
        {
            // prompt for correct answer 
            string article = GetArticle(tree.currentNode.text);
            gameWindow.AppendToMessage(PROMPT_CORRECT_ANSWER + 
                (article == string.Empty ? " " : " " + article + " ") + 
                tree.currentNode.text + "?", true, false);
            gameState = GameStates.PromptCorrectAnswer;
            return;
        }

        // if the question limit is reached
        if (tree.currentQuery > 20)
        {
            // print the winning text and end the game
            gameWindow.AppendToMessage(WIN_LAST_QUESTION, true, false);
            EndOfGame(true, true);
            return;
        }

        // append the currents node to the message
        gameWindow.AppendToMessage(tree.currentNode.text, true, false);
    }

    /// <summary>
    /// Ends this game and resolves the win.
    /// </summary>
    /// <param name="playerWin">Whether the player won</param>
    /// <param name="lastQuestionReached">Whether the last question was reached</param>
    internal void EndOfGame(bool playerWin, bool lastQuestionReached = false)
    {
        if (playerWin && !lastQuestionReached)
        {
            // new info to be learned
            // setup for updating the tree
            updatingTree = true;
            updateIsNew = false;
            newQuestion = string.Empty;
            newYesAnswer = string.Empty;
            newNoAnswer = string.Empty;
            newTempAnswer = string.Empty;

            // prompts for correct answer
            gameState = GameStates.PromptAnswer;
        }
        else
        {
            // prompt for new game
            gameWindow.AppendToMessage(PROMPT_PLAY_AGAIN, true, false);
            gameState = GameStates.PromptAnother;
        }
    }

    /// <summary>
    /// Called when updating tree is finished.
    /// Updates the tree on file and prompts for more play.
    /// </summary>
    private void FinishUpdatingTree()
    {
        // return if we aren't updating rn
        if (!updatingTree)
        {
            Debug.LogError("FinishUpdateTree called before updating started!");
            return;
        }

        // make sure the question starts with a captital case
        // and ends with a questionmark
        if (!char.IsUpper(newQuestion[0]))
            newQuestion = char.ToUpper(newQuestion[0]) + newQuestion.Substring(1);
        if (newQuestion[newQuestion.Length - 1] != '?')
            newQuestion += "?";

        // add the question differently when it's a whole new tree 
        // vs when it's added to current node
        if (updateIsNew)
        {
            // create a new tree with the data
            tree = new BinaryTree(newQuestion, newYesAnswer, newNoAnswer);

            // append texts, prompt for play game
            gameWindow.AppendToMessage(NEW_KNOWLEDGE, true, false);
            gameWindow.AppendToMessage(PROMPT_PLAY_AFTER_NEW, true, false);
        }
        else
        {
            // append the current node with the data
            tree.currentNode.text = newQuestion;
            tree.currentNode.leftNode = new BinaryNode(newYesAnswer);
            tree.currentNode.rightNode = new BinaryNode(newNoAnswer);

            // append texts, prompt for play game
            gameWindow.AppendToMessage(UPDATED_KNOWLEDGE, true, false);
            gameWindow.AppendToMessage(PROMPT_PLAY_AGAIN, true, false);
        }

        // save the updated tree
        XMLSerializer.Serialize(tree, GetQGKnowledgeFilePath);

        // wait in this game state for input
        gameState = GameStates.PromptAnother;
    }

    /// <summary>
    /// Called when input is given. 
    /// Resolves the given input.
    /// </summary>
    /// <param name="input">Given user input</param>
    internal void ResolveInput(string input)
    {
        bool resolved = true;

        // input is resolved based on gamestate so switch on the current one
        switch (gameState)
        {
            case GameStates.Opening:
            case GameStates.NewGame:
            case GameStates.Ending:
                // no input expected,
                // state change is handled before input is resolved
                break;

            case GameStates.PromptNewOrExisting:
                if (ValidateInput(input, yesAnswers))
                {
                    // start with loading existing knowledge
                    // and then play a game
                    StartExistingGame();
                }
                else if (ValidateInput(input, noAnswers))
                {
                    // remove existing tree
                    gameWindow.AppendToMessage(KNOWLEDGE_DELETED, true, false);
                    DeleteExistingKnowledge();

                    // start game without knowledge
                    StartNewGame();
                }
                else
                {
                    // unexpected input, prompt again and resolve
                    ResolveIncorrectInput();
                    resolved = false;
                }
                break;

            case GameStates.InGame:
                if (ValidateInput(input, yesAnswers))
                {
                    // iterate to the right node
                    IterateNode(false);
                }
                else if (ValidateInput(input, noAnswers))
                {
                    // iterate to the left node
                    IterateNode(true);
                }
                else
                {
                    // unexpected input, prompt again and resolve
                    ResolveIncorrectInput();
                    resolved = false;
                }
                break;

            case GameStates.PromptCorrectAnswer:
                if (ValidateInput(input, yesAnswers))
                {
                    // correct guess, player loses and end game
                    gameWindow.AppendToMessage(LOSE, true, false);
                    EndOfGame(false);
                }
                else if (ValidateInput(input, noAnswers))
                {
                    // wrong guess, player wins and end game
                    gameWindow.AppendToMessage(WIN + " " + NOTE_ON_ENTERING_ANSWERS, true, false);
                    EndOfGame(true);
                }
                else
                {
                    // unexpected input, prompt again and resolve
                    ResolveIncorrectInput();
                    resolved = false;
                }
                break;

            case GameStates.PromptAnother:
                if (ValidateInput(input, yesAnswers))
                {
                    // on another game, end current iteration on tree
                    tree.EndIterating();

                    // prompt message
                    gameWindow.AppendToMessage(PROMPT_NEW_OR_EXISTING_GAME, true, false);

                    // and go to appropriate game state to wait for input
                    gameState = GameStates.PromptNewOrExisting;
                }
                else if (ValidateInput(input, noAnswers))
                {
                    // no new game, say bye and shut down with short delay
                    gameWindow.AppendToMessage(BYE, true, false);
                    GameManager.instance.StartCoroutine(ShutDownDelayed(1f));
                }
                else
                {
                    // unexpected input, prompt again and resolve
                    ResolveIncorrectInput();
                    resolved = false;
                }
                break;

            case GameStates.PromptNewQuestion:
                // save input as question
                newQuestion = input;

                // prompt and wait for yes answer input
                gameWindow.AppendToMessage(PROMPT_NEW_TREE_YES + " " + NOTE_ON_ENTERING_ANSWERS, true, false);
                gameState = GameStates.PromptNewYesAnswer;
                break;

            case GameStates.PromptNewYesAnswer:
                // save input as yes answer
                newYesAnswer = input;

                // prompt and wait for no answer input
                gameWindow.AppendToMessage(PROMPT_NEW_TREE_NO, true, false);
                gameState = GameStates.PromptNewNoAnswer;
                break;

            case GameStates.PromptNewNoAnswer:
                // save input as no answer
                newNoAnswer = input;
                
                // gathered all data, so finish updating
                FinishUpdatingTree();
                break;

            case GameStates.PromptAnswer:
                // save input as answer,
                // but dunno yet if this is the yes or no answer
                newTempAnswer = input;

                // prompt and wait for question
                gameWindow.AppendToMessage(PROMPT_TREE_QUESTION_1 + tree.currentNode.text + PROMPT_TREE_QUESTION_2, true, false);
                gameState = GameStates.PromptQuestion;
                break;

            case GameStates.PromptQuestion:
                // save input as question
                newQuestion = input;

                // prompt and wait for yes answer input
                string article = GetArticle(newTempAnswer);
                gameWindow.AppendToMessage(PROMPT_TREE_ANSWER_1 +
                    (article == string.Empty ? " " : " " + article + " ") + newTempAnswer + 
                    PROMPT_TREE_ANSWER_2, true, false);
                gameState = GameStates.PromptYNAnswer;
                break;

            case GameStates.PromptYNAnswer:
                if (ValidateInput(input, yesAnswers))
                {
                    newNoAnswer = newTempAnswer;
                    newYesAnswer = tree.currentNode.text;
                }
                else if (ValidateInput(input, noAnswers))
                {
                    newNoAnswer = tree.currentNode.text;
                    newYesAnswer = newTempAnswer; 
                }
                else
                {
                    // unexpected input, prompt again and resolve
                    ResolveIncorrectInput();
                    resolved = false;
                }

                // if the input was succesfully resolved, 
                // append text and goto next state
                if (resolved) 
                {
                    // gathered all data, so finish updating
                    FinishUpdatingTree();
                }
                break;
        }
    }

    /// <summary>
    /// Get the correct article for a word.
    /// </summary>
    /// <param name="word">The word to get the article for</param>
    /// <returns>No article if the word starts with a capital letter, 
    /// else either a or an, depending on the word.</returns>
    private string GetArticle(string word)
    {
        // if it starts with a captial letter, 
        // assume it's a name and doesn't need an article
        if (char.IsUpper(word[0]))
            return string.Empty;

        // if the first letter is a vowel, return 'an'
        if (vowels.Contains(char.ToUpper(word[0])))
            return AN_ARTICLE;

        // else return 'a'
        return A_ARTICLE;
    }

    #region VALIDATION
    /// <summary>
    /// Prompts again for input, called when incorrect input is provided
    /// </summary>
    private void ResolveIncorrectInput()
    {
        // no valid input 
        gameWindow.AppendToMessage(PROMPT_REINPUT_INCORRECT_INPUT, true, false);

        // wait for new input and call to ResolveInput
    }

    /// <summary>
    /// Checks given input against expected input.
    /// </summary>
    /// <param name="input">The given input, to be checked.</param>
    /// <param name="expectedInput">The expected input, to be checked against.</param>
    /// <returns></returns>
    private bool ValidateInput(string input, string expectedInput)
    {
        // return whether they are the same
        return input == expectedInput;
    }

    /// <summary>
    /// Checks given input against a collection of expected inputs.
    /// </summary>
    /// <param name="input">The given input, to be checked.</param>
    /// <param name="expectedInputs">The expected inputs, to be checked against.</param>
    /// <returns></returns>
    private bool ValidateInput(string input, string[] expectedInputs)
    {
        // return valid if we have a match in collection of expected inputs
        for (int i = 0; i < expectedInputs.Length; i++)
            if (expectedInputs[i] == input.ToLower())
                return true;

        // if we still haven't found a match at this point
        // the input wasn't valid
        return false;
    }
    #endregion
}

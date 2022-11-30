using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class QuestionsGameWindowUI : WindowUI
{
    // variable for text msg
    [SerializeField] private TextMeshProUGUI message;

    // Variables for rects for sizing and parenting
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private ScrollRect scrollRect;
    
    // Variables for input field
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject inputFieldIndicator;
    private bool inputfieldSelected = false;
    private float inputFieldIndicatorTimer = 0;
    private float inputFieldIndicatorBlinkRate = 0.85f;

    // reference to the program
    private QuestionsGame questionsGameRef;

    internal override void Initialize(Program program = null)
    {
        base.Initialize(program);
        questionsGameRef = (QuestionsGame)program;
    }

    internal override void Open()
    {
        // call base
        base.Open();

        // set title
        title.text = "20 Questions - The Game";

        // clear message until set
        message.text = string.Empty;

        // clear input field
        // and select it so player can start typing right away
        inputField.text = string.Empty;
        SelectInputField();
    }

    /// <summary>
    /// Called every frame by Unity.
    /// </summary>
    private void Update()
    {
        // if the input field is selected, 
        if (inputfieldSelected)
        {
            // blink the indicator
            inputFieldIndicatorTimer -= Time.unscaledDeltaTime;
            if(inputFieldIndicatorTimer <= 0)
            {
                inputFieldIndicator.SetActive(!inputFieldIndicator.activeSelf);
                inputFieldIndicatorTimer = inputFieldIndicatorBlinkRate;
            }

            // if we catch 'ENTER' input,
            // resolve the text in the field
            if (Input.GetKeyUp(KeyCode.Return))
                ResolveInputField();
        }
    }

    /// <summary>
    /// Call to select the input field w/ Unity's event system.
    /// </summary>
    internal void SelectInputField()
    {
        // select the input field
        // if the event system is not already selecting
        if (!EventSystem.current.alreadySelecting)
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
    }

    /// <summary>
    /// Called when inputfield is selected.
    /// </summary>
    public void OnInputFieldSelected()
    {
        OnClick();

        // set variables for selection
        inputFieldIndicatorTimer = 0;
        inputfieldSelected = true;
    }

    /// <summary>
    /// Called when inputfield is deselected.
    /// </summary>
    public void OnInputFieldDeselected()
    {
        // set variables for deselection
        inputFieldIndicatorTimer = 0;
        inputfieldSelected = false;
        inputFieldIndicator.SetActive(true);
    }

    /// <summary>
    /// Called when done editing inputfield.
    /// Resolves the current input.
    /// </summary>
    private void ResolveInputField()
    {
        // get the resulting command 
        string result = inputField.text;

        // reset and deselect the input field
        inputField.text = string.Empty;
        EventSystem.current.SetSelectedGameObject(null);

        // add the input to the message as user input
        AppendToMessage(result, true, true);

        // give the program the resulting input 
        // program will resolve the input and progress the game
        questionsGameRef.ResolveInput(result);

        // forces size fitter updates, waits until those are done
        // and updates scroll view position 
        StartCoroutine(SetScrollviewDelayed());
    }

    /// <summary>
    /// Waits for UI update and then sets the scroll view position.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SetScrollviewDelayed()
    {
        // force a rebuild on the layouts so the content in scrollview is resized with the newly added texts
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);

        // reselect the input field for next input
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);

        // then wait for an Unity update until everything is up-to-date and resized
        // before we set the scrolled value
        yield return new WaitForEndOfFrame();

        // set scroll rect position so that it's scrolled to the bottom
        scrollRect.verticalNormalizedPosition = 0;
    }

    /// <summary>
    /// Adds new text to the current message.
    /// </summary>
    /// <param name="textToAppend">The new text</param>
    /// <param name="newLine">Whether to start on a new line</param>
    /// <param name="userInput">Whether the new text is put in by the user</param>
    internal void AppendToMessage(string textToAppend, bool newLine, bool userInput)
    {
        string totalTextToAppend = string.Empty;

        // add a new line if there isn't one at the end already
        if (newLine && message.text != string.Empty && !message.text.EndsWith("\n"))
            totalTextToAppend += "\n";

        // add > if it's user input
        if (userInput)
            totalTextToAppend += "> ";

        // add the text and a new line to end this appended text
        totalTextToAppend += textToAppend + "\n";

        // and finally add it to the message text
        message.text += totalTextToAppend;
    }
}

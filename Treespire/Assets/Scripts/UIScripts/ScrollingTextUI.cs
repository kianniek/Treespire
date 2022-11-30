using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollingTextUI : MonoBehaviour
{
    // scene references
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private RectTransform parentTransform;
    [SerializeField] private RectTransform textTransform;
    [SerializeField] private float scrollSpreed;

    // clone of the text so that we can loop it
    private TextMeshProUGUI clonedText;

    // variables to keep track of scroll 
    private bool scrolling;
    private float scrollPosition;

    // margin for text
    private Vector4 margin = new Vector4(0, 0, 40, 0);

    /// <summary>
    /// Called on start
    /// </summary>
    private void Start()
    {
        // make a clone of the original text and parent it to the text
        clonedText = Instantiate(text);
        RectTransform clonedTransform = clonedText.GetComponent<RectTransform>();
        clonedTransform.SetParent(textTransform);
        clonedTransform.anchorMin = GameManager.instance.topLeftAnchor;
        clonedTransform.anchorMax = GameManager.instance.bottomLeftAnchor;
        clonedTransform.pivot = GameManager.instance.bottomLeftAnchor;
        clonedTransform.anchoredPosition = Vector2.zero;

        // call one update already
        Update();
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    private void Update()
    {
        // call text changed when change is noted
        if (text.havePropertiesChanged)
            OnTextChanged();

        // do scroll behavior if text is long enough
        if (scrolling)
        {
            scrollPosition += scrollSpreed * Time.deltaTime;
            textTransform.anchoredPosition = new Vector2(-scrollPosition % textTransform.sizeDelta.x, 0);
        }
    }

    /// <summary>
    /// Called when text is changed, updates scrolling behavior
    /// </summary>
    internal void OnTextChanged()
    {
        // forces updates and waits until those are done
        // than changes text transform values according to new sizes
        StartCoroutine(OnTextChangedDelayed());
    }

    /// <summary>
    /// Resolve text changes with delay for rebuilds.
    /// </summary>
    /// <returns></returns>
    private IEnumerator OnTextChangedDelayed()
    {
        // remove margin as to not influence to size fitter
        text.margin = clonedText.margin = Vector4.zero;

        // force a layout update to make sure content size fitter is up to date
        LayoutRebuilder.ForceRebuildLayoutImmediate(textTransform);

        // wait for rebuild
        yield return new WaitForEndOfFrame();

        // reset scroll position
        scrollPosition = 0;

        // change cloned text
        clonedText.text = text.text;

        // check if the text is larger than the parent
        if (textTransform.sizeDelta.x > parentTransform.sizeDelta.x)
        {
            // if larger, position all the way to the left
            textTransform.anchorMin = GameManager.instance.bottomLeftAnchor;
            textTransform.anchorMax = GameManager.instance.bottomLeftAnchor;
            textTransform.pivot = GameManager.instance.bottomLeftAnchor;
            textTransform.anchoredPosition = Vector2.zero;

            // set margins to space out the 2 texts
            text.margin = clonedText.margin = margin;

            // enable cloned 
            clonedText.gameObject.SetActive(true);

            // scroll to show whole text
            scrolling = true;
        }
        else
        {
            // if smaller, center
            textTransform.anchorMin = GameManager.instance.bottomCenterAnchor;
            textTransform.anchorMax = GameManager.instance.bottomCenterAnchor;
            textTransform.pivot = GameManager.instance.bottomCenterAnchor; 
            textTransform.anchoredPosition = Vector2.zero;

            // disable cloned 
            clonedText.gameObject.SetActive(false);

            // no need to scroll a small text
            scrolling = false;
        }

        // no need to update again for now
        text.havePropertiesChanged = false;
    }
}

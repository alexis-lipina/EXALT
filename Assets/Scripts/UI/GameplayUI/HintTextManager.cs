using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintTextManager : MonoBehaviour
{
    [SerializeField] private List<Image> TopRow;
    [SerializeField] private List<Image> BottomRow;
    [SerializeField] private Image TopRowBackground;
    [SerializeField] private Image BottomRowBackground;

    [SerializeField] private string FontSpritesheet_ResourceName;

    private static Dictionary<string, Sprite> _fontMapping;
    private bool IsFadingOut = false;
    private bool IsFadingIn = false;
    private bool TopIsActive = false;
    private bool BottomIsActive = false;


    //higher rate = shorter duration
    private const float FadeOutRate = 1.0f;
    private const float FadeInRate = 1.0f;


    void Awake()
    {
        _instanceOf = this;
    }

    void Start()
    {
        //hide stuff thats shown in editor so I know it exists
        if (_fontMapping == null)
        {
            _fontMapping = new Dictionary<string, Sprite>();
            Sprite[] sprites = Resources.LoadAll<Sprite>(FontSpritesheet_ResourceName);
            foreach (Sprite sprite in sprites)
            {
                _fontMapping.Add(sprite.name, sprite);
                Debug.Log("Sprite added : " + sprite.name);
            }
        }
        foreach (Image image in TopRow)
        {
            image.color = new Color(1, 1, 1, 0);
        }
        foreach (Image image in BottomRow)
        {
            image.color = new Color(1, 1, 1, 0);
        }
        TopRowBackground.color = new Color(0, 0, 0, 0);
        BottomRowBackground.color = new Color(0, 0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
    }


    /// <summary>
    /// Show hint text
    /// </summary>
    /// <param name="duration">fi duration is 0, will be held forever</param>
    /// <param name=""></param>
    public void ShowHintText(string textLineOne, string textLineTwo)
    {
        StartCoroutine(FadeIn(textLineOne, textLineTwo));
    }

    /// <summary>
    /// Fade-out the hint text if its still displayed
    /// </summary>
    public void HideHintText()
    {
        StartCoroutine(FadeOut());
    }

    /// <summary>
    /// Changes text displayed, should be done while everything is fully hidden
    /// </summary>
    private void ChangeHintText(string textLineOne, string textLineTwo)
    {
        char[] charArrayOne = textLineOne.ToUpper().ToCharArray();
        char[] charArrayTwo = textLineTwo.ToUpper().ToCharArray();

        if (charArrayOne.Length > TopRow.Count)
        {
            Debug.LogError("Text Line One is too big : " + charArrayOne.Length + " > " + TopRow.Count);
            return;
        }
        if (charArrayTwo.Length > TopRow.Count)
        {
            Debug.LogError("Text Line Two is too big : " + textLineTwo.Length);
        }



        for (int i = 0; i < charArrayOne.Length; i++)
        {
            SetCharacter(TopRow[i], charArrayOne[i]);
        }
        for (int i = charArrayOne.Length; i < TopRow.Count; i++)
        {
            TopRow[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < charArrayTwo.Length; i++)
        {
            SetCharacter(BottomRow[i], charArrayTwo[i]);
        }
        for (int i = charArrayTwo.Length; i < BottomRow.Count; i++)
        {
            BottomRow[i].gameObject.SetActive(false);
        }
    }

    private void SetCharacter(Image image, char character)
    {
        Sprite characterSprite;

        image.gameObject.SetActive(true);
        if (char.IsLetterOrDigit(character))
        {
             characterSprite = _fontMapping[character.ToString()];
        }
        else if (character == '?')
        {
            characterSprite = _fontMapping["QUESTION"];
        }
        else if (character == '!')
        {
            characterSprite = _fontMapping["EXCLAMATION"];
        }
        else if (character == ' ')
        {
            characterSprite = _fontMapping["SPACE"];
        }
        else
        {
            Debug.LogError("WARNING! CHARACTER \"" + character + "\" not located in font spritesheet!");
            characterSprite = _fontMapping["QUESTION"];
        }


        image.sprite = characterSprite;
    }

    //=============| Coroutines for hiding + showing

    private IEnumerator FadeOut()
    {
        if (IsFadingOut) yield break; // "early return" - only one fadeout at a time

        while (IsFadingIn)
        {
            yield return new WaitForSeconds(0.5f); //wait for fading in to stop
        }
        IsFadingOut = true;

        float opacity = 1.0f;

        while (opacity > 0.0f)
        {
            opacity -= Time.deltaTime * FadeOutRate;
            foreach (Image image in TopRow)
            {
                image.color = new Color(1.0f, 1.0f, 1.0f, opacity);
            }
            foreach (Image image in BottomRow)
            {
                image.color = new Color(1.0f, 1.0f, 1.0f, opacity);
            }
            if (TopIsActive) TopRowBackground.color = new Color(0, 0, 0, opacity * 0.5f);
            if (BottomIsActive) BottomRowBackground.color = new Color(0, 0, 0, opacity * 0.5f);
            yield return new WaitForEndOfFrame();
        }
        TopIsActive = false;
        BottomIsActive = false;
        IsFadingOut = false;
    }

    private IEnumerator FadeIn(string textLineOne, string textLineTwo)
    {
        if (IsFadingIn) yield break; // "early return" - only one fadeout at a time

        while (IsFadingOut)
        {
            yield return new WaitForSeconds(0.5f); //wait for fading out to stop
        }
        IsFadingIn = true;

        ChangeHintText(textLineOne, textLineTwo);

        bool isFadingInTop = textLineOne.Length > 0;
        bool isFadingInBottom = textLineTwo.Length > 0;



        float opacity = 0.0f;

        while (opacity < 1.0f)
        {
            opacity += Time.deltaTime * FadeInRate;
            foreach (Image image in TopRow)
            {
                image.color = new Color(1.0f, 1.0f, 1.0f, opacity);
            }
            foreach (Image image in BottomRow)
            {
                image.color = new Color(1.0f, 1.0f, 1.0f, opacity);
            }
            if (isFadingInTop) TopRowBackground.color = new Color(0, 0, 0, opacity * 0.5f);
            if (isFadingInBottom) BottomRowBackground.color = new Color(0, 0, 0, opacity * 0.5f);
            yield return new WaitForEndOfFrame();
        }
        IsFadingIn = false;
        TopIsActive = isFadingInTop;
        BottomIsActive = isFadingInBottom;
    }

    //==========================================| SINGLETON STUFF
    public static HintTextManager GetInstanceOf()
    {
        if (_instanceOf) return _instanceOf;
        else
        {
            Debug.LogError("Attempted to retrieve instance of HintTextManager but no such instance exists!!!");
            return null;
        }
    }
    private static HintTextManager _instanceOf;
}

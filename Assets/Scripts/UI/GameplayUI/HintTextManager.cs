using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintTextManager : MonoBehaviour
{
    [SerializeField] private ControlMenuManager controlMenuManager;
    [SerializeField] private List<Image> TopRow;
    [SerializeField] private List<Image> BottomRow;
    [SerializeField] private Image TopRowBackground;
    [SerializeField] private Image BottomRowBackground;

    [SerializeField] private string FontSpritesheet_ResourceName;

    [SerializeField] private string InputSpritesheet_ResourceName;

    private static Dictionary<string, Sprite> _fontMapping; // maps a character to a sprite matching that character
    private static Dictionary<char, Sprite> _inputMapping; // matches an encoded input verb character to the corresponding sprite for its input
    private bool IsFadingOut = false;
    private bool IsFadingIn = false;
    private bool TopIsActive = false;
    private bool BottomIsActive = false;

    private Dictionary<string, char> _inputEncoder; // used to map an input action to a character that is otherwise unused in text

    //higher rate = shorter duration
    private const float FadeOutRate = 2.0f;
    private const float FadeInRate = 1.0f;

    private Vector2 FONTRECT_NORMAL;
    private Vector2 FONTRECT_SQUARE;
    private Color CurrentFontColor; // COLOR IS SET LIKE THIS - "The word |red|APPLE|white| is red"

    void Awake()
    {
        _inputEncoder = new Dictionary<string, char>();
        _inputEncoder.Add("Heal", '#');
        _inputEncoder.Add("ChangeStyle_Fire", '@');
        _inputEncoder.Add("ChangeStyle_Zap", '%');
        _inputEncoder.Add("ChangeStyle_Void", '&');
        _inputEncoder.Add("ChangeStyle_Ichor", '+');
        _inputEncoder.Add("Rest", '*');
        _inputEncoder.Add("Jump", '$');
        _inputEncoder.Add("Blink", '}');
        _inputEncoder.Add("MoveUp", ';');
        _inputEncoder.Add("MoveDown", ':');
        _inputEncoder.Add("MoveLeft", '^');
        _inputEncoder.Add("MoveRight", '=');
        _inputEncoder.Add("Melee", ')');
        _inputEncoder.Add("RangedAttack", '(');
        _instanceOf = this;

        FONTRECT_NORMAL = new Vector2(36, 72);
        FONTRECT_SQUARE = new Vector2(72, 72);
        CurrentFontColor = Color.white; 
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
                //Debug.Log("Sprite added : " + sprite.name);
            }
        }
        if (_inputMapping == null)
        {
            _inputMapping = new Dictionary<char, Sprite>();
            Sprite[] sprites = Resources.LoadAll<Sprite>(InputSpritesheet_ResourceName);
            foreach (Sprite sprite in sprites)
            {
                //_inputMapping.Add(sprite.name, sprite);
                if (_inputEncoder.ContainsKey(sprite.name))
                {
                    //Debug.Log("Input sprite added : " + sprite.name);
                    _inputMapping.Add(_inputEncoder[sprite.name], sprite);
                }
                else
                {
                    //Debug.LogWarning("Sprite " + sprite.name + " does not have corresponding entry in _inputEncoder!");
                    //TODO : Replace inputEncoder with reference to control menu manager
                }
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


        //ShowHintText("Press <TRIGGER_R> or |red|<MOUSE_RIGHT>", "to <STICK_R> the [space]");
        //ShowHintText("The |void|apple |white|is purple", "");
        //ShowHintText("These are", "|void|three |zap|different |fire|elements");
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

    public void HardSetHintText(string textLineOne, string textLineTwo)
    {
        ChangeHintText(textLineOne, textLineTwo);
    }

    public void TrickleInHintTextWords(string textLineOne, string textLineTwo, float[] delays)
    {
        StartCoroutine(StampInHintText(textLineOne, textLineTwo, delays));
    }

    private IEnumerator StampInHintText(string textLineOne, string textLineTwo, float[] delays)
    {
        string[] rowOne = textLineOne.Split(' ');
        string[] rowTwo = textLineTwo.Split(' ');

        string builtLineOne = "";
        string builtLineTwo = "";

        for (int i = 0; i < rowOne.Length; i++)
        {
            builtLineOne += rowOne[i];
            ChangeHintText(FillWithSpaces(builtLineOne, textLineOne), "");
            if (i != rowOne.Length - 1) builtLineOne += " ";
            yield return new WaitForSeconds(delays[i]);
        }

        for (int i = 0; i < rowTwo.Length; i++)
        {
            builtLineTwo += rowTwo[i];
            ChangeHintText(builtLineOne, FillWithSpaces(builtLineTwo, textLineTwo));
            if (i != rowTwo.Length - 1) builtLineTwo += " ";
            yield return new WaitForSeconds(delays[rowOne.Length + i]);
        }
        //HideHintText();
    }

    private string FillWithSpaces(string substring, string originalstring)
    {
        int sizeReductionOriginal = 0;
        int sizeReductionSubstring = 0;
        for (int i = 0; i < originalstring.Length; i++)
        {
            //color code check
            if (originalstring[i] == '|')
            {
                int tempoffset = DecodeColor(originalstring.Substring(i, originalstring.Length - i), false);
                sizeReductionOriginal += tempoffset;
                i += tempoffset - 1;
            }

        }
        for (int i = 0; i < substring.Length; i++)
        {
            //color code check
            if (originalstring[i] == '|')
            {
                int tempoffset = DecodeColor(substring.Substring(i, substring.Length - i), false);
                sizeReductionSubstring += tempoffset;
                i += tempoffset - 1;
            }

        }

        int numSpaces = (originalstring.Length - sizeReductionOriginal) - (substring.Length - sizeReductionSubstring);
        for (int i = 0; i < numSpaces; i++)
        {
            substring += "_";
        }
        return substring;
    }

    /// <summary>
    /// Changes text displayed, should be done while everything is fully hidden
    /// </summary>
    private void ChangeHintText(string textLineOne, string textLineTwo)
    {
        textLineOne = EncodeString(textLineOne);
        textLineTwo = EncodeString(textLineTwo);
        char[] charArrayOne = textLineOne.ToUpper().ToCharArray();
        char[] charArrayTwo = textLineTwo.ToUpper().ToCharArray();
        int sizeDeductionOne = 0;
        int sizeDeductionTwo = 0;


        for (int i = 0; i < charArrayOne.Length; i++)
        {
            //color code check
            if (charArrayOne[i] == '|')
            {
                int tempoffset = DecodeColor(textLineOne.Substring(i, textLineOne.Length - i));
                sizeDeductionOne += tempoffset;
                i += tempoffset - 1;
            }
            else
            {
                SetCharacter(TopRow[i - sizeDeductionOne], charArrayOne[i]);
            }
        }
        if (charArrayOne.Length-sizeDeductionOne > TopRow.Count)
        {
            Debug.LogError("Text Line One is too big : " + charArrayOne.Length + " > " + TopRow.Count);
            return;
        }
        for (int i = charArrayOne.Length - sizeDeductionOne; i < TopRow.Count; i++)
        {
            TopRow[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < charArrayTwo.Length; i++)
        {
            //color code check
            if (charArrayTwo[i] == '|')
            {
                int tempoffset = DecodeColor(textLineTwo.Substring(i, textLineTwo.Length - i));
                sizeDeductionTwo += tempoffset;
                i += tempoffset - 1;
            }
            else
            {
                SetCharacter(BottomRow[i - sizeDeductionTwo], charArrayTwo[i]);
            }
        }
        if (charArrayTwo.Length - sizeDeductionTwo > TopRow.Count)
        {
            Debug.LogError("Text Line Two is too big : " + textLineTwo.Length);
        }
        for (int i = charArrayTwo.Length - sizeDeductionTwo; i < BottomRow.Count; i++)
        {
            BottomRow[i].gameObject.SetActive(false);
        }
    }

    private void SetCharacter(Image image, char character)
    {
        Sprite characterSprite = null;

        image.gameObject.SetActive(true);
        image.rectTransform.sizeDelta = FONTRECT_NORMAL;
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
        else if (character == ',')
        {
            characterSprite = _fontMapping[","];
        }
        else if (character == '.')
        {
            characterSprite = _fontMapping["."];
        }
        else if (character == ' ')
        {
            characterSprite = _fontMapping["SPACE"];
        }
        else if (character == '[')
        {
            characterSprite = _fontMapping["["];
        }
        else if (character == ']')
        {
            characterSprite = _fontMapping["]"];
        }
        else if (character == '_')
        {
            characterSprite = _fontMapping["SPACE"];
        }
        else if (_inputEncoder.ContainsValue(character))
        {
            foreach (KeyValuePair<string, char> entry in _inputEncoder)
            {
                if (entry.Value == character)
                {
                    characterSprite = controlMenuManager.GetSpriteForAction(entry.Key);
                }
            }
            if (characterSprite == null)
            {
                characterSprite = _inputMapping[character];
            }
            image.rectTransform.sizeDelta = FONTRECT_SQUARE;
        }
        else
        {
            Debug.LogWarning("WARNING! CHARACTER \"" + character + "\" not located in font spritesheet!");
            characterSprite = _fontMapping["QUESTION"];
        }

        image.color = CurrentFontColor;
        image.sprite = characterSprite;
    }

    private string EncodeString(string rawString)
    {
        //Debug.Log("INPUT STRING : " + rawString);
        if (!rawString.Contains("<")) { return rawString; }// early return if nothing to encode


        string encodedString = "";
        for (int i = 0; i < rawString.Length; i++)
        {
            //find opening bracket
            if (rawString[i] == '<')
            {
                //find ending bracket
                //Debug.Log("i = " + i + " and rawString.Length - i = " + (rawString.Length - i));
                string code = rawString.Substring(i+1, rawString.Length - i - 1);
                code = code.Substring(0, code.IndexOf(">"));
                if (_inputEncoder.ContainsKey(code))
                {
                    //Debug.Log("<color=green>Code " + code + " successfully encoded!</color>");
                    encodedString += _inputEncoder[code];
                    i += code.Length + 1;
                }
                else
                {
                    Debug.LogError("No such _fontMapping entry for code " + code + "");
                }
            }
            else
            {
                encodedString += rawString[i];
            }
        }
        //Debug.Log("OUTPUT STRING : " + encodedString);
        return encodedString;
    }

    /// <summary>
    /// Changes the color and returns info for iteration consistency
    /// </summary>
    /// <param name="substring">substring from i onwards including the first pipe (|)</param>
    /// <returns>Number to add to i to skip color tag</returns>
    private int DecodeColor(string substring, bool bSetColor = true)
    {
        Debug.Log("Color tailing substring : " + substring);
        int iterationJumpSize = 0;
        for (int i = 1; i < substring.Length; i++)
        {
            //find opening bracket
            if (substring[i] == '|')
            {
                iterationJumpSize = i+1;
                string decodedColor = substring.Substring(1, i - 1);
                Debug.LogWarning("DECODED COLOR : " + decodedColor);
                decodedColor = decodedColor.ToUpper();
                switch (decodedColor)
                {
                    case "ZAP":
                        if (bSetColor) CurrentFontColor = PlayerHandler.GetElementColor(ElementType.ZAP);
                        break;
                    case "FIRE":
                        if (bSetColor) CurrentFontColor = PlayerHandler.GetElementColor(ElementType.FIRE);
                        break;
                    case "VOID":
                        if (bSetColor) CurrentFontColor = PlayerHandler.GetElementColor(ElementType.VOID);
                        //CurrentFontColor = new Color(0.7f, 0.3f, 1.0f, 1.0f);
                        break;
                    case "ICHOR":
                        if (bSetColor) CurrentFontColor = PlayerHandler.GetElementColor(ElementType.ICHOR);
                        break;
                    case "WHITE":
                        if (bSetColor) CurrentFontColor = Color.white;
                        break;
                    default:
                        if (bSetColor) CurrentFontColor = Color.magenta;
                        Debug.LogWarning("Alert! Font color \"" + decodedColor + "\" does not exist!");
                        break;
                }

                return iterationJumpSize;
            }
        }
        Debug.LogError("No second pipe (|) found for encoded color!");
        return 0;
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
                Color tempColor = image.color;
                tempColor.a = opacity;
                image.color = tempColor;
            }
            foreach (Image image in BottomRow)
            {
                Color tempColor = image.color;
                tempColor.a = opacity;
                image.color = tempColor;
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
                Color tempColor = image.color;
                tempColor.a = opacity;
                image.color = tempColor;
            }
            foreach (Image image in BottomRow)
            {
                Color tempColor = image.color;
                tempColor.a = opacity;
                image.color = tempColor;
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

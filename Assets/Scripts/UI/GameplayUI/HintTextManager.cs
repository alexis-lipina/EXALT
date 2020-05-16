using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintTextManager : MonoBehaviour
{
    [SerializeField] private List<Image> TopRow;
    [SerializeField] private List<Image> BottomRow;

    [SerializeField] private string FontSpritesheet_ResourceName;

    private static Dictionary<string, Sprite> _fontMapping;

    void Awake()
    {
        _instanceOf = this;
    }

    void Start()
    {
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
        ShowHintText("Top text!", "Bottom Text?");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Show hint text
    /// </summary>
    /// <param name="duration"></param>
    /// <param name=""></param>
    public void ShowHintText(string textLineOne, string textLineTwo, float duration = 0f)
    {
        char[] charArrayOne = textLineOne.ToUpper().ToCharArray();
        char[] charArrayTwo = textLineTwo.ToUpper().ToCharArray();

        if (charArrayOne.Length > TopRow.Count)
        {
            Debug.LogError("Text Line One is too big : " + textLineOne.Length);
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

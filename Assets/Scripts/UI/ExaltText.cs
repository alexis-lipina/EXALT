using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Exalt-specific implementation of UI text.
/// </summary>
[RequireComponent(typeof(HorizontalLayoutGroup))]
[ExecuteInEditMode]
public class ExaltText : MonoBehaviour
{
    private static Dictionary<string, char> InputEncoder;

    [SerializeField] private string TextString;
    [SerializeField] private Vector2 CharacterSize;
    private static RectTransform characterPrefab;
    private static Dictionary<string, Sprite> fontSpritesheet;

    void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnValidate()
    {
        SetText(TextString);
    }

    public void SetText(string NewText)
    {
        //Initialization if necessary
        if (fontSpritesheet == null || fontSpritesheet.Count == 0)
        {
            fontSpritesheet = new Dictionary<string, Sprite>();
            Sprite[] sprites = Resources.LoadAll<Sprite>("Font_Spritesheet");
            foreach (Sprite sprite in sprites)
            {
                fontSpritesheet.Add(sprite.name, sprite);
            }
        }

        if (!characterPrefab)
        {
            characterPrefab = Resources.Load<RectTransform>("Prefabs/UI/FontChar");
        }

        TextString = NewText;
        //create children for text
        List<GameObject> objectsToDestroy = new List<GameObject>();
        RectTransform newObj;
        for (int i = 0; i < TextString.Length; i++)
        {
            if (i >= transform.childCount)
            {
                newObj = Instantiate(characterPrefab, transform);
            }
            else
            {
                newObj = transform.GetChild(i).GetComponent<RectTransform>();
            }
            if (fontSpritesheet.ContainsKey(TextString.Substring(i, 1).ToUpper()))
            {
                newObj.GetComponent<Image>().sprite = fontSpritesheet[TextString.Substring(i, 1).ToUpper()];
            }
            else if (TextString.Substring(i, 1) == " ")
            {
                newObj.GetComponent<Image>().sprite = fontSpritesheet["SPACE"];
            }
            else
            {
                newObj.GetComponent<Image>().sprite = fontSpritesheet["QUESTION"];
            }
            newObj.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, CharacterSize.x);
            newObj.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, CharacterSize.y);
        }

        if (TextString.Length < transform.childCount)
        {
            for (int i = TextString.Length; i < transform.childCount; i++)
            {
                if (!Application.isPlaying)
                {
                    StartCoroutine(DestroyWhenever(transform.GetChild(TextString.Length).gameObject));
                }
                else
                {
                    GameObject.Destroy(transform.GetChild(i).gameObject);
                }
            }
        }
    }

    IEnumerator DestroyWhenever(GameObject obj)
    {
        yield return new WaitForEndOfFrame();
        GameObject.DestroyImmediate(obj);
    }
}

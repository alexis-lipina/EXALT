using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Exalt-specific implementation of UI text.
/// </summary>

public class ExaltText : MonoBehaviour
{
    private static Dictionary<string, char> InputEncoder;

    [SerializeField] private List<Image> CharacterImages;
    [SerializeField] private Image FontSpritesheet;

    void Awake()
    {
        if (InputEncoder.Count == 0)
        {
            InputEncoder = new Dictionary<string, char>();
            InputEncoder.Add("DPAD_DOWN", '#');
            InputEncoder.Add("DPAD_LEFT", '@');
            InputEncoder.Add("DPAD_RIGHT", '%');
            InputEncoder.Add("DPAD_UP", '&');
            InputEncoder.Add("FACEBUTTON_UP", '*');
            InputEncoder.Add("FACEBUTTON_DOWN", '$');
            InputEncoder.Add("FACEBUTTON_RIGHT", '}');
            InputEncoder.Add("FACEBUTTON_LEFT", '{');
            InputEncoder.Add("KEY_W", ';');
            InputEncoder.Add("KEY_A", ':');
            InputEncoder.Add("KEY_S", '^');
            InputEncoder.Add("KEY_D", '=');
            InputEncoder.Add("KEY_DOWN", '_');
            InputEncoder.Add("KEY_UP", '+');
            InputEncoder.Add("KEY_RIGHT", '/');
            InputEncoder.Add("KEY_LEFT", '\\');
            InputEncoder.Add("MOUSE_LEFT", ')');
            InputEncoder.Add("MOUSE_RIGHT", '(');
            InputEncoder.Add("STICK_L", '\'');
            InputEncoder.Add("STICK_R", '\"');
            InputEncoder.Add("TRIGGER_L", ',');
            InputEncoder.Add("TRIGGER_R", '-');
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetString(string newString)
    {
        if (newString.Length > CharacterImages.Count)
        {
            Debug.LogError("ExaltText : String length exceeds amount of available character images!");
            return;
        }

        for (int i = 0; i < newString.Length; i++)
        {

        }
    }
}

using Rewired;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomizationMenu : MonoBehaviour
{
    static CustomizationOptionsDataArray PaletteOptions;
    public bool OverwriteFile = true;
    [SerializeField] private Slider _optionsSlider;
    [SerializeField] private ExaltText _paletteNameText;
    public GameObject SourceUIMenu;

    private static void LoadOptions(bool forceOverwrite)
    {
        // if can load file, load it - otherwise, create default and save it to disk
        if (System.IO.File.Exists(Application.persistentDataPath + "ExaltPalettes") && !forceOverwrite)
        {
            Debug.Log("Reading from existing accessibility options data");
            StreamReader reader = new StreamReader(Application.persistentDataPath + "ExaltPalettes");
            PaletteOptions = JsonUtility.FromJson<CustomizationOptionsDataArray>(reader.ReadToEnd());
            reader.Close();
        }
        else
        {
            GenerateFile();
        }
    }

    private static void GenerateFile()
    {
        Debug.Log("No current customization options file - writing new one and setting up default");
        PaletteOptions = new CustomizationOptionsDataArray();

        List<CustomizationOptionsDataElement> elementList = new List<CustomizationOptionsDataElement>();
        
        elementList.Add(new CustomizationOptionsDataElement() 
        {
            PaletteNickname = "Original",
            ColorA = new float[] { 0 / 255f, 0 / 255f, 25 / 255f, 1 },
            ColorB = new float[] { 0 / 255f, 21 / 255f, 39 / 255f, 1 },
            ColorC = new float[] { 3 / 255f, 40 / 255f, 56 / 255f, 1 },
            ColorD = new float[] { 53 / 255f, 21 / 255f, 39 / 255f, 1 },
            ColorE = new float[] { 36 / 255f, 15 / 255f, 27 / 255f, 1 },
            ColorF = new float[] { 32 / 255f, 32 / 255f, 43 / 255f, 1 },
            ColorG = new float[] { 19 / 255f, 20 / 255f, 32 / 255f, 1 }
        });
        elementList.Add(new CustomizationOptionsDataElement()
        {
            PaletteNickname = "Trans rights",
            ColorA = new float[] { 170 / 255f, 47 / 255f, 137 / 255f, 1 },
            ColorB = new float[] { 255 / 255f, 108 / 255f, 216 / 255f, 1 },
            ColorC = new float[] { 255 / 255f, 202 / 255f, 241 / 255f, 1 },
            ColorD = new float[] { 53 / 255f, 194 / 255f, 220 / 255f, 1 },
            ColorE = new float[] { 15 / 255f, 158 / 255f, 185 / 255f, 1 },
            ColorF = new float[] { 208 / 255f, 208 / 255f, 208 / 255f, 1 },
            ColorG = new float[] { 176 / 255f, 176 / 255f, 176 / 255f, 1 }
        });

        elementList.Add(new CustomizationOptionsDataElement()
        {
            PaletteNickname = "Bi pride",
            ColorA = new float[] { 82 / 255f, 0 / 255f, 41 / 255f, 1 },
            ColorB = new float[] { 147 / 255f, 0 / 255f, 74 / 255f, 1 },
            ColorC = new float[] { 194 / 255f, 29 / 255f, 112 / 255f, 1 },
            ColorD = new float[] { 0 / 255f, 42 / 255f, 126 / 255f, 1 },
            ColorE = new float[] { 0 / 255f, 24 / 255f, 73 / 255f, 1 },
            ColorF = new float[] { 155 / 255f, 79 / 255f, 151 / 255f, 1 },
            ColorG = new float[] { 123 / 255f, 44 / 255f, 119 / 255f, 1 }
        });

        elementList.Add(new CustomizationOptionsDataElement()
        {
            PaletteNickname = "Nonbinary",
            ColorA = new float[] { 141 / 255f, 120 / 255f, 0 / 255f, 1 },
            ColorB = new float[] { 211 / 255f, 181 / 255f, 0 / 255f, 1 },
            ColorC = new float[] { 214 / 255f, 205 / 255f, 0 / 255f, 1 },
            ColorD = new float[] { 70 / 255f, 0 / 255f, 120 / 255f, 1 },
            ColorE = new float[] { 48 / 255f, 0 / 255f, 88 / 255f, 1 },
            ColorF = new float[] { 23 / 255f, 23 / 255f, 23 / 255f, 1 },
            ColorG = new float[] { 8 / 255f, 8 / 255f, 8 / 255f, 1 }
        });

        elementList.Add(new CustomizationOptionsDataElement()
        { 
            PaletteNickname = "Peach",
            ColorA = new float[] { 138 / 255f, 41 / 255f, 52 / 255f, 1 },
            ColorB = new float[] { 188 / 255f, 73 / 255f, 64 / 255f, 1 },
            ColorC = new float[] { 214 / 255f, 131 / 255f, 110 / 255f, 1 },
            ColorD = new float[] { 208 / 255f, 191 / 255f, 122 / 255f, 1 },
            ColorE = new float[] { 167 / 255f, 137 / 255f, 59 / 255f, 1 },
            ColorF = new float[] { 194 / 255f, 70 / 255f, 55 / 255f, 1 },
            ColorG = new float[] { 103 / 255f, 11 / 255f, 21 / 255f, 1 }
        });

        elementList.Add(new CustomizationOptionsDataElement()
        {
            PaletteNickname = "Drifter",
            ColorA = new float[] { 64 / 255f, 0 / 255f, 17 / 255f, 1 },
            ColorB = new float[] { 111 / 255f, 11 / 255f, 31 / 255f, 1 },
            ColorC = new float[] { 150 / 255f, 19 / 255f, 19 / 255f, 1 },
            ColorD = new float[] { 70 / 255f, 70 / 255f, 70 / 255f, 1 },
            ColorE = new float[] { 59 / 255f, 59 / 255f, 59 / 255f, 1 },
            ColorF = new float[] { 23 / 255f, 23 / 255f, 23 / 255f, 1 },
            ColorG = new float[] { 5 / 255f, 5 / 255f, 5 / 255f, 1 }
        });

        elementList.Add(new CustomizationOptionsDataElement()
        {
            PaletteNickname = "Noir",
            ColorA = new float[] { 0 / 255f, 0 / 255f, 0 / 255f, 1 },
            ColorB = new float[] { 25 / 255f, 25 / 255f, 25 / 255f, 1 },
            ColorC = new float[] { 50 / 255f, 50 / 255f, 50 / 255f, 1 },
            ColorD = new float[] { 160 / 255f, 160 / 255f, 160 / 255f, 1 },
            ColorE = new float[] { 111 / 255f, 111 / 255f, 59 / 255f, 1 },
            ColorF = new float[] { 23 / 255f, 23 / 255f, 23 / 255f, 1 },
            ColorG = new float[] { 0 / 255f, 0 / 255f, 0 / 255f, 1 }
        });

        PaletteOptions.array = elementList.ToArray();

        System.IO.File.WriteAllText(Application.persistentDataPath + "ExaltPalettes", (JsonUtility.ToJson(PaletteOptions)));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(_optionsSlider.gameObject);
        if (PaletteOptions == null)
        {
            LoadOptions(true);
        }
        _optionsSlider.wholeNumbers = true;
        _optionsSlider.maxValue = PaletteOptions.array.Length - 1;
        _optionsSlider.minValue = 0;
        _optionsSlider.value = AccessibilityOptionsSingleton.GetInstance().CurrentPaletteIndex;
    }

    public void OnOptionsSliderChanged()
    {
        AccessibilityOptionsSingleton.GetInstance().CurrentPaletteIndex = (int)_optionsSlider.value;
        ApplyPlayerColorPalette();
        _paletteNameText.SetText(GetCurrentColorPalette().PaletteNickname);
    }

    public void OnBackToMenuClicked()
    {
        gameObject.SetActive(false);
        SourceUIMenu.SetActive(true);
    }

    public static void ApplyPlayerColorPalette()
    {
        if (PaletteOptions == null)
        {
            LoadOptions(true);
        }
        if (PaletteOptions.array.Length <= AccessibilityOptionsSingleton.GetInstance().CurrentPaletteIndex)
        {
            AccessibilityOptionsSingleton.GetInstance().CurrentPaletteIndex = 0;
        }

        CustomizationOptionsDataElement palette = GetCurrentColorPalette();
        Shader.SetGlobalColor("_PaletteSwapA", new Color(palette.ColorA[0], palette.ColorA[1], palette.ColorA[2], 1));
        Shader.SetGlobalColor("_PaletteSwapB", new Color(palette.ColorB[0], palette.ColorB[1], palette.ColorB[2], 1));
        Shader.SetGlobalColor("_PaletteSwapC", new Color(palette.ColorC[0], palette.ColorC[1], palette.ColorC[2], 1));
        Shader.SetGlobalColor("_PaletteSwapD", new Color(palette.ColorD[0], palette.ColorD[1], palette.ColorD[2], 1));
        Shader.SetGlobalColor("_PaletteSwapE", new Color(palette.ColorE[0], palette.ColorE[1], palette.ColorE[2], 1));
        Shader.SetGlobalColor("_PaletteSwapF", new Color(palette.ColorF[0], palette.ColorF[1], palette.ColorF[2], 1));
        Shader.SetGlobalColor("_PaletteSwapG", new Color(palette.ColorG[0], palette.ColorG[1], palette.ColorG[2], 1));
        Debug.Log(palette.PaletteNickname);
        Debug.Log(AccessibilityOptionsSingleton.GetInstance().CurrentPaletteIndex);
        AccessibilityOptionsSingleton.GetInstance().SaveCurrentOptions();
    }

    public static CustomizationOptionsDataElement GetCurrentColorPalette()
    {
        return PaletteOptions.array[AccessibilityOptionsSingleton.GetInstance().CurrentPaletteIndex];
    }
}

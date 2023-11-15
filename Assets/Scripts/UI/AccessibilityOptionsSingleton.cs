
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// This class interfaces with and exposes the serialized AccessibilityOptionsData class
/// </summary>
public class AccessibilityOptionsSingleton
{
    private const string DATA_PATH = "AccessibilityOptionsData.txt";
    //instance stuff
    private AccessibilityOptionsData _data;

    //setup instance
    public AccessibilityOptionsSingleton()
    {
        // if can load file, load it - otherwise, create default and save it to disk
        if (System.IO.File.Exists(Application.persistentDataPath + "/" + DATA_PATH))
        {
            //Debug.Log("Reading from existing accessibility options data");
            StreamReader reader = new StreamReader(Application.persistentDataPath + "/" + DATA_PATH);
            _data = JsonUtility.FromJson<AccessibilityOptionsData>(reader.ReadToEnd());
            reader.Close();
        }
        else
        {
            //Debug.Log("No current accessibility options file - writing new one and setting up default");
            _data = new AccessibilityOptionsData();
            System.IO.File.WriteAllText(Application.persistentDataPath + "/" + DATA_PATH, (JsonUtility.ToJson(_data)));
        }
    }

    public void SaveCurrentOptions()
    {
        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + DATA_PATH);
        writer.Write(JsonUtility.ToJson(_data));
        writer.Close();
    }

    public bool IsFlashingEnabled
    {
        get
        {
            return _data.IsFlashingEnabled;
        }
        set
        {
            _data.IsFlashingEnabled = value;
        }
    }

    public bool LowHPVignette
    {
        get
        {
            return _data.LowHPVignette;
        }
        set
        {
            _data.LowHPVignette = value;
        }
    }

    public float ScreenshakeAmount
    {
        get
        {
            return _data.ScreenshakeAmount;
        }
        set
        {
            _data.ScreenshakeAmount = value;
        }
    }
    public float UIScale
    {
        get
        {
            return _data.UIScale;
        }
        set
        {
            _data.ScreenshakeAmount = UIScale;
        }
    }

    public int CurrentPaletteIndex
    {
        get
        {
            return _data.CustomizationOptionIndex;
        }
        set
        {
            _data.CustomizationOptionIndex = value;
        }
    }
    public bool IsBlinkInDirectionOfMotion
    {
        get
        {
            return _data.IsBlinkInDirectionOfMotion;
        }
        set
        {
            _data.IsBlinkInDirectionOfMotion = value;
        }
    }



    // singleton stuff
    private static AccessibilityOptionsSingleton _instance;
    public static AccessibilityOptionsSingleton GetInstance()
    {
        if (_instance == null)
        {
            _instance = new AccessibilityOptionsSingleton();
        }
        return _instance;
    }
}

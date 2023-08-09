using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CampaignSaveSingleton/* : MonoBehaviour*/
{
    private const string DATA_PATH = "CampaignSaveData.txt";
    //instance stuff
    public CampaignSaveData _data;

    //setup instance
    public CampaignSaveSingleton()
    {
        // if can load file, load it - otherwise, create default and save it to disk
        if (System.IO.File.Exists(Application.persistentDataPath + "/" + DATA_PATH))
        {
            Debug.Log("Reading from existing campaign save data at " + Application.persistentDataPath + "/" + DATA_PATH);
            StreamReader reader = new StreamReader(Application.persistentDataPath + DATA_PATH);
            _data = JsonUtility.FromJson<CampaignSaveData>(reader.ReadToEnd());
            reader.Close();
        }
        else
        {
            Debug.Log("No current campaign save file - writing new one and setting up default");
            _data = new CampaignSaveData();
            System.IO.File.WriteAllText(Application.persistentDataPath + "/" + DATA_PATH, (JsonUtility.ToJson(_data)));
        }
    }

    public void SaveCurrentState()
    {
        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/" + DATA_PATH);
        writer.Write(JsonUtility.ToJson(_data));
        writer.Close();
    }

    // singleton stuff
    private static CampaignSaveSingleton _instance;
    public static CampaignSaveSingleton GetInstance()
    {
        if (_instance == null)
        {
            _instance = new CampaignSaveSingleton(); // this appears to not be working...?
        }
        return _instance;
    }
}

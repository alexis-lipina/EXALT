using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MonolithPersistent : MonoBehaviour
{
    public float SuperweaponTimer = 0.0f;
    public float SuperweaponLoopDuration = 8.0f;

    private AudioSource _laserAudioSource;
    public UnityEvent OnLaserFire;
    bool isPaused = false;
    private const float TIMESTEP = 0.5f;

    private float RealTimeAtSceneUnload = 0.0f;


    public static MonolithPersistent GetInstance()
    {
        return instance;
    }
    private static MonolithPersistent instance;

    private void Awake()
    {
        if (instance)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        instance = this;
        _laserAudioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject.DontDestroyOnLoad(this);
        StartCoroutine(SuperweaponCycle());
        SceneManager.sceneLoaded += OnSceneLoad;
        SceneManager.sceneUnloaded += OnSceneUnload;
    }

    void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        // the hitching doesnt actually happen between these events I think.  so I'm just gonna guess a value for how much time we need to offset
        SuperweaponTimer += 0.5f;
        // SuperweaponTimer += Time.realtimeSinceStartup - RealTimeAtSceneUnload;
    }

    void OnSceneUnload(Scene scene)
    {
        //RealTimeAtSceneUnload = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        if (!isPaused) SuperweaponTimer += Time.deltaTime;
        //Debug.Log(SuperweaponTimer);

        if (ReInput.players.GetPlayer(0).GetButtonDown("Pause"))
        {
            if (_laserAudioSource.isPlaying)
            {
                _laserAudioSource.Pause();
                isPaused = true;
            }
            else
            {
                _laserAudioSource.UnPause();
                isPaused = false;
            }
        }
    }

    IEnumerator SuperweaponCycle()
    {
        yield return new WaitForSeconds(1.0f);
        while (true)
        {
            SuperweaponTimer = 0.0f;
            FiringChamberManager manager = GameObject.FindObjectOfType<FiringChamberManager>();
            if (manager)
            {
                manager.FireCannon();
            }
            _laserAudioSource.Play();
            OnLaserFire.Invoke();
            while (SuperweaponTimer < 8.0f)
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(300, 300, 0, 300), SuperweaponTimer.ToString());
    }

    private void OnApplicationPause(bool pause)
    {

        if (pause)
        {
            _laserAudioSource.Pause();
        }
        else
        {
            _laserAudioSource.UnPause();
        }
    }
}

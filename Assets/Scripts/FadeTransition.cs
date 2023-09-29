using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeTransition : MonoBehaviour
{
    public static FadeTransition Singleton;
    private bool _hasBeganToExit = false;
    
    private void Awake()
    {
        Singleton = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine( FadeInTransition() );
    }

    public void FadeToScene(string levelName, string doorName)
    {
        if (!_hasBeganToExit)
        {
            StartCoroutine(FadeOutTransition(levelName, doorName));
            _hasBeganToExit = true;
        }
    }

    // allows fadetoscene to be called by a UnityEvent, which only exposes one arg
    public void FadeToScene_OneArg(string LevelNameDotDoorName)
    {
        FadeToScene(
            LevelNameDotDoorName.Split('.')[0],
            LevelNameDotDoorName.Split('.')[1]);
    }


    private IEnumerator FadeInTransition(float rate = 2f)
    {
        Time.timeScale = 0.0f; // prevents things from happening during loading, like player falling
        _hasBeganToExit = false;
        GetComponent<Image>().color = new Color(0, 0, 0, 1);

        // wait for a moment to ensure scene load
        PlayerHandler player = FindObjectOfType<PlayerHandler>();
        

        if (player) //wait for scene to load for a few milliseconds, and hang player in position. Solves problems where player falls through objects as level loads in.
        {
            yield return new WaitForSecondsRealtime(0.01f); // let the EntranceVolume initialize the players position before doing anything else

            float StartPlayerHeight = player.GetEntityPhysics().GetObjectElevation();

            float WaitTimer = 0.2f;
            while (WaitTimer > 0.0f)
            {
                player.GetEntityPhysics().SetObjectElevation(StartPlayerHeight);
                player.GetEntityPhysics().ZVelocity = 0.0f;
                yield return new WaitForSecondsRealtime(0.01f);
                WaitTimer -= 0.01f;
            }

            Time.timeScale = 1.0f;
            WaitTimer = 0.2f;
            while (WaitTimer > 0.0f)
            {
                player.GetEntityPhysics().SetObjectElevation(StartPlayerHeight);
                player.GetEntityPhysics().ZVelocity = 0.0f;
                yield return new WaitForSeconds(0.01f);
                WaitTimer -= 0.01f;
            }
        }
        else
        {
            // menus
            yield return new WaitForSecondsRealtime(0.5f);

            Time.timeScale = 1.0f;
        }



        float opacity = 1f;
        while (opacity > 0)
        {
            yield return new WaitForSeconds(0.01f);
            GetComponent<Image>().color = new Color(0, 0, 0, opacity);
            opacity -= 0.02f * rate;
        }
        GetComponent<Image>().color = new Color(0, 0, 0, 0);
    }

    private IEnumerator FadeOutTransition(string sceneName, string doorName, float rate = 2f)
    {
        GetComponent<Image>().color = new Color(0, 0, 0, 0);
        float opacity = 0f;
        while (opacity < 1)
        {
            yield return new WaitForSeconds(0.01f);
            GetComponent<Image>().color = new Color(0, 0, 0, opacity);
            opacity += 0.02f * rate;
        }
        GetComponent<Image>().color = new Color(0, 0, 0, 1);
        PlayerHandler.PREVIOUS_SCENE = SceneManager.GetActiveScene().name;
        PlayerHandler.PREVIOUS_SCENE_DOOR = doorName;
        SceneManager.LoadScene(sceneName);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the background animation in the "doomsday" scenario on the CollapsingBalconyLedge scene
/// </summary>
public class DoomsdayAnimationController : MonoBehaviour
{
    [SerializeField] private Animator DeathmarchAnimation;
    [SerializeField] private SpriteRenderer LightPulseSprite;
    [SerializeField] private SpriteRenderer MonolithGlowSprite;
    [SerializeField] private TriggerVolume StartAnimationTriggerVolume;
    [SerializeField] private SpriteRenderer ZoopSprite;
    [SerializeField] private Vector3 ZoopTop;
    [SerializeField] private Vector3 ZoopBottom;
    [SerializeField] private Animator FlashAnimation;
    [SerializeField] private Animator CrossFlashAnimation;
    [SerializeField] private EarthShatterController ShatterController;
    [SerializeField] private SpriteRenderer ShatterPatternSprite;
    [SerializeField] private FloorCollapseManager FloorCollapse;

    private ScreenFlash ScreenFlashInstance;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(OpeningLoop());
        FlashAnimation.GetComponent<SpriteRenderer>().enabled = false;
        CrossFlashAnimation.GetComponent<SpriteRenderer>().enabled = false;
        ScreenFlashInstance = ScreenFlash.InstanceOfScreenFlash;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // Helpers


    /// <summary>
    /// Flash absorption beam
    /// </summary>
    /// <returns></returns>
    private IEnumerator LightPulse()
    {
        float attenuationScale = 2.0f; //affects speed the pulse fades out
        Debug.Log("Pew!");

        //set beam opacity
        float beamOpacity = 1.0f;
        float glowOpacity = 1.0f;
        LightPulseSprite.material.SetFloat("_Opacity", beamOpacity);
        MonolithGlowSprite.material.SetFloat("_Opacity", glowOpacity);
        yield return new WaitForSeconds(0.1f);

        while (beamOpacity > 0)
        {
            LightPulseSprite.material.SetFloat("_Opacity", beamOpacity);
            MonolithGlowSprite.material.SetFloat("_Opacity", glowOpacity);
            beamOpacity -= Time.deltaTime * attenuationScale;
            glowOpacity -= Time.deltaTime * attenuationScale * 0.3f;

            ZoopSprite.transform.localPosition = Vector3.Lerp(ZoopTop, ZoopBottom, beamOpacity * beamOpacity * beamOpacity);
            ZoopSprite.material.SetFloat("_Opacity", beamOpacity * 1.2f - 0.3f);

            yield return new WaitForEndOfFrame();
        }
    }


    //=================================| STAGES |===================================
     
    /// <summary>
    /// Looping, default animation 
    /// </summary>
    /// <returns></returns>
    private IEnumerator OpeningLoop()
    {
        while (true)
        {
            DeathmarchAnimation.Play("deathmarch", -1, 0f);
            StartCoroutine(LightPulse());
            yield return new WaitForSeconds(2.0f);
            if (StartAnimationTriggerVolume.IsTriggered) break;
        }
        StartCoroutine(StageOne());
    }

    /// <summary>
    /// Row of people are getting zooped up by the monolith
    /// </summary>
    /// <returns></returns>
    private IEnumerator StageOne()
    {
        StartCoroutine(LightPulse());
        yield return new WaitForSeconds(2.0f);
        StartCoroutine(LightPulse());
        yield return new WaitForSeconds(2.0f);
        StartCoroutine(LightPulse());
        yield return new WaitForSeconds(2.0f);
        StartCoroutine(LightPulse());
        yield return new WaitForSeconds(1.0f);
        DeathmarchAnimation.enabled = false;

        StartCoroutine(StageTwo());
    }

    /// <summary>
    /// Monolith charges up, fires laser, shatter sprite appears
    /// </summary>
    /// <returns></returns>
    private IEnumerator StageTwo()
    {
        // pregnant pause...
        yield return new WaitForSeconds(2.0f);

        // flash windup flare
        FlashAnimation.GetComponent<SpriteRenderer>().enabled = true;
        FlashAnimation.Play("BigFlare_Slow", -1, 0.0f);
        yield return new WaitForSeconds(0.7f);

        FlashAnimation.GetComponent<SpriteRenderer>().enabled = false;
        CrossFlashAnimation.GetComponent<SpriteRenderer>().enabled = true;
        CrossFlashAnimation.Play("CrossFlash", -1, 0.0f);
        yield return new WaitForSeconds(0.3f);

        CrossFlashAnimation.GetComponent<SpriteRenderer>().enabled = false;
        ScreenFlashInstance.PlayFlash(1.0f, 0.01f);
        ShatterPatternSprite.enabled = false;
        StartCoroutine(StageThree());
    }

    /// <summary>
    /// Monolith charges up again, shatters earth, bigger deal this time
    /// </summary>
    /// <returns></returns>
    private IEnumerator StageThree()
    {
        // pregnant pause
        yield return new WaitForSeconds(2.0f);

        // windup flare
        FlashAnimation.GetComponent<SpriteRenderer>().enabled = true;
        FlashAnimation.Play("BigFlare_Slow", -1, 0.0f);
        yield return new WaitForSeconds(0.7f);

        // cross flash
        FlashAnimation.GetComponent<SpriteRenderer>().enabled = false;
        CrossFlashAnimation.GetComponent<SpriteRenderer>().enabled = true;
        CrossFlashAnimation.Play("CrossFlash", -1, 0.0f);
        yield return new WaitForSeconds(0.3f);

        //blammo
        CrossFlashAnimation.GetComponent<SpriteRenderer>().enabled = false;
        ScreenFlashInstance.PlayFlash(1.0f, 0.005f);
        ShatterController.StartShatter();
        FloorCollapse.StartCollapse();

    }



}

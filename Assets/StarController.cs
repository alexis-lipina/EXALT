using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Controls the sky part of the skybox */
public class StarController : MonoBehaviour
{
    // stars that should be visible during each phase
    [SerializeField] List<SpriteRenderer> Stars1;
    [SerializeField] List<SpriteRenderer> Stars2;
    [SerializeField] List<SpriteRenderer> Stars3;
    [SerializeField] List<SpriteRenderer> Stars4;

    Dictionary<SpriteRenderer, Vector2> ActiveStars; // vector2(sinmultiplier, somethingelse)

    [SerializeField] AnimationCurve StarFlashCurve;
    [SerializeField] AnimationCurve StarFloorBrightness; // flashcurve is added to this
    [SerializeField] AnimationCurve StarCurveScalar;
    [SerializeField] AnimationCurve StarYScale;

    [SerializeField] SpriteRenderer GreenNebula;
    [SerializeField] SpriteRenderer BlueNebula;
    [SerializeField] AnimationCurve NebulaOpacity;
    [SerializeField] float NebulaOscillationPeriod = 4.0f;
    [SerializeField] float NebulaOscillationMagnitude = 0.2f;

    public float PlatformCharge = 0.0f; // 0...1 for charge, 1...2 for lightning bolt warmup


    public int Phase = 0; //1, 2, 3, 4, reveals corresponding star



    // Start is called before the first frame update
    void Start()
    {
        foreach (var asdf in Stars1)
        {
            asdf.gameObject.SetActive(false);
        }
        foreach (var asdf in Stars2)
        {
            asdf.gameObject.SetActive(false);
        }
        foreach (var asdf in Stars3)
        {
            asdf.gameObject.SetActive(false);
        }
        foreach (var asdf in Stars4)
        {
            asdf.gameObject.SetActive(false);
        }

        ActiveStars = new Dictionary<SpriteRenderer, Vector2>();
        AdvancePhase();
    }

    // Update is called once per frame
    void Update()
    {
        // twinkle stars
        foreach (var entry in ActiveStars)
        {
            entry.Key.material.SetFloat("_LowerBound", StarFlashCurve.Evaluate(Time.time * (entry.Value.x * 1  + 1) % 1.0f) * 5.0f - 5.0f);
            entry.Key.transform.GetChild(0).GetComponent<SpriteRenderer>().material.SetFloat("_Opacity", StarFlashCurve.Evaluate(Time.time * (entry.Value.x * 1 + 1) % 1.0f) * StarCurveScalar.Evaluate(PlatformCharge) + StarFloorBrightness.Evaluate(PlatformCharge));
            
        }

        // nebula
        BlueNebula.material.SetFloat("_Opacity", NebulaOpacity.Evaluate(PlatformCharge) + Mathf.Sin(Time.time / NebulaOscillationPeriod) * NebulaOscillationMagnitude);
        GreenNebula.material.SetFloat("_Opacity", NebulaOpacity.Evaluate(PlatformCharge) - Mathf.Sin(Time.time / NebulaOscillationPeriod) * NebulaOscillationMagnitude);
        //BlueNebula.material.SetFloat("_Opacity", 0);
        //GreenNebula.material.SetFloat("_Opacity", 0);
    }

    public void AdvancePhase()
    {
        Phase++;
        switch (Phase)
        {
            case 1:
                foreach (var asdf in Stars1)
                {
                    asdf.gameObject.SetActive(true);
                    float randomscale = Random.Range(0.75f, 1.25f);
                    asdf.gameObject.transform.localScale = new Vector3(randomscale, randomscale, 1);
                    ActiveStars.Add(asdf, new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
                }
                break;
            case 2:
                foreach (var asdf in Stars2)
                {
                    asdf.gameObject.SetActive(true);
                    ActiveStars.Add(asdf, new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
                }
                break;
            case 3:
                foreach (var asdf in Stars3)
                {
                    asdf.gameObject.SetActive(true);
                    ActiveStars.Add(asdf, new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
                }
                break;
            case 4:
                foreach (var asdf in Stars4)
                {
                    asdf.gameObject.SetActive(true);
                    ActiveStars.Add(asdf, new Vector2(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
                }
                break;
        }
    }
}

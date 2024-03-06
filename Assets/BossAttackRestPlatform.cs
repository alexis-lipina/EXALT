using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackRestPlatform : MonoBehaviour
{
    
    [SerializeField] ZapFXController TopLightningBolt;
    [SerializeField] ZapFXController BottomLightningBolt;
    [SerializeField] List<ZapFXController> LightningBolts_Near; // these are all background
    [SerializeField] List<ZapFXController> LightningBolts_Mid;
    [SerializeField] List<ZapFXController> LightningBolts_Far;
    [SerializeField] AnimationCurve BoltThicknessModifierOverCharge;
    [SerializeField] AnimationCurve BoltFrequencyOverCharge;
    [SerializeField] AnimationCurve FlashScaleOverCharge;
    [SerializeField] AnimationCurve AmbientGlowOverCharge;
    [SerializeField] private AnimationCurve BoltGlowOverTime;
    [SerializeField] private AnimationCurve BoltDistanceOverCharge;
    private RestPlatform restPlatform;

    float timer = 0.0f;
    float charge = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        restPlatform = GetComponent<RestPlatform>();
        // init lightning
        foreach (var asdf in LightningBolts_Near)
        {
            asdf.GetComponentInChildren<SpriteRenderer>().enabled = false;
        }
        // init lightning
        foreach (var asdf in LightningBolts_Mid)
        {
            asdf.GetComponentInChildren<SpriteRenderer>().enabled = false;
        }
        // init lightning
        foreach (var asdf in LightningBolts_Far)
        {
            asdf.GetComponentInChildren<SpriteRenderer>().enabled = false;
        }
        StartCoroutine(RunVFX());
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        charge = restPlatform.CurrentChargeAmount;
        /*
        if (timer > 1.0f)
        {
            timer = 0.0f;
            TopLightningBolt.SetupLine(Vector3.zero, new Vector3(0, 36, 0));
            TopLightningBolt.Play(0.5f);
            BottomLightningBolt.SetupLine(Vector3.zero, new Vector3(0, 36, 0));
            BottomLightningBolt.Play(0.5f);
            Debug.LogError("Lightning!");
        }*/
    }


    IEnumerator RunVFX()
    {
        while (true)
        {
            //float charge = ControllingRestPlatform.CurrentChargeAmount;

            ZapFXController bolt = null;
            float boltOffset = 0f;
            switch (Random.Range(0, 3))
            {
                case 0:
                    bolt = LightningBolts_Near[Random.Range(0, LightningBolts_Near.Count)];
                    boltOffset = 25.0f;
                    break;
                case 1:
                    bolt = LightningBolts_Mid[Random.Range(0, LightningBolts_Mid.Count)];
                    boltOffset = 15.0f;
                    break;
                case 2:
                    bolt = LightningBolts_Far[Random.Range(0, LightningBolts_Far.Count)];
                    boltOffset = 7.5f;
                    break;
            }
                    
            bolt.SetupLine(Vector3.zero, new Vector3(0, -boltOffset * BoltDistanceOverCharge.Evaluate(charge), 0)); // change distance of bolt over charge
            bolt.SetThickness(BoltThicknessModifierOverCharge.Evaluate(charge), BoltThicknessModifierOverCharge.Evaluate(charge) + 0.125f);
            bolt.Play(0.2f);
            StartCoroutine(PulseLightningGlow(bolt.GetComponentInChildren<SpriteRenderer>(), 0.5f));

            //bolt.SetThickness(BoltThicknessModifierOverCharge.Evaluate(charge), BoltSizeOverCharge.Evaluate(currentChargeAmount) * 2.0f);
            //bolt.SetupLine(StartPosition, Vector3.Lerp(StartPosition, EndPosition, BoltDistanceOverCharge.Evaluate(currentChargeAmount)));
            //bolt.Play(BoltDurationOverCharge.Evaluate(currentChargeAmount));
            //yield return new WaitForSeconds(Random.Range(0.8f, 1.2f) * BoltDelayOverCharge.Evaluate(currentChargeAmount));
            yield return new WaitForSeconds(Random.Range(0.0f, 0.6f) / BoltFrequencyOverCharge.Evaluate(charge));
        }
    }

    IEnumerator PulseLightningGlow(SpriteRenderer sprite, float duration)
    {
        sprite.enabled = true;

        float scaleModifier = FlashScaleOverCharge.Evaluate(charge);

        sprite.transform.localScale *= scaleModifier;

        float timer = 0.0f;
        while (timer < duration)
        {
            sprite.material.SetFloat("_Opacity", BoltGlowOverTime.Evaluate(timer / duration));
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        sprite.transform.localScale /= scaleModifier;
        sprite.enabled = false;
    }
}

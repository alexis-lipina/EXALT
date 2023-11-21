using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreFinalBossLightningBlast : MonoBehaviour
{

    // use restplatform to drive this in the same way the lightning bolts in the final boss encounter are driven by it
    [SerializeField] RestPlatform ControllingRestPlatform;
    [SerializeField] List<ZapFXController> LightningBolts_Near;
    [SerializeField] List<ZapFXController> LightningBolts_Mid;
    [SerializeField] List<ZapFXController> LightningBolts_Far;
    [SerializeField] AnimationCurve BoltThicknessModifierOverCharge;
    [SerializeField] AnimationCurve BoltFrequencyOverCharge;
    [SerializeField] private AnimationCurve BoltDistanceOverCharge;
    [SerializeField] private AnimationCurve BoltGlowOverTime;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunVFX());
        foreach (var sadf in GetComponentsInChildren<SpriteRenderer>())
        {
            sadf.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RunVFX()
    {

        while (true)
        {
            float charge = ControllingRestPlatform.CurrentChargeAmount;

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
            bolt.SetupLine(Vector3.zero, new Vector3(0, -boltOffset, 0));
            bolt.Play(0.2f);
            StartCoroutine(PulseLightningGlow(bolt.GetComponentInChildren<SpriteRenderer>(), 0.5f));
            /*
            Bolt.SetThickness(BoltThicknessModifierOverCharge.Evaluate(charge), BoltSizeOverCharge.Evaluate(currentChargeAmount) * 2.0f);
            Bolt.SetupLine(StartPosition, Vector3.Lerp(StartPosition, EndPosition, BoltDistanceOverCharge.Evaluate(currentChargeAmount)));
            Bolt.Play(BoltDurationOverCharge.Evaluate(currentChargeAmount));*/
            //yield return new WaitForSeconds(Random.Range(0.8f, 1.2f) * BoltDelayOverCharge.Evaluate(currentChargeAmount));
            yield return new WaitForSeconds(Random.Range(0.0f, 0.6f));
        }
    }

    IEnumerator PulseLightningGlow(SpriteRenderer sprite, float duration)
    {
        sprite.enabled = true;
        float timer = 0.0f;
        while (timer < duration)
        {
            sprite.material.SetFloat("_Opacity", BoltGlowOverTime.Evaluate(timer / duration));
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        sprite.enabled = false;
    }
}

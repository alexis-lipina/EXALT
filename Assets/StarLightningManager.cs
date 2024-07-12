using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarLightningManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> Stars;
    [SerializeField] private List<ZapFXController> Zaps;
    [SerializeField] private AnimationCurve StarFlashNormalizedXScaleCurve;
    [SerializeField] private AnimationCurve StarFlashNormalizedYScaleCurve;
    [SerializeField] private Vector3 LightningTargetEnd1;
    [SerializeField] private Vector3 LightningTargetEnd2;

    [SerializeField] private GameObject BigFlashyBeam;
    private float BeamInitialScale;
    [SerializeField] private float BeamAddedScaleOnFlash = 1.0f;

    public float Frequency = 4.0f;
    public AnimationCurve FlashStrengthOverTime;
    public float FlashDuration = 0.1f;    

    // Start is called before the first frame update
    void Start()
    {
        BeamInitialScale = BigFlashyBeam.transform.localScale.x;
        List<GameObject> starsToRemove = new List<GameObject>();
        foreach (GameObject star in Stars)
        {
            if (star.transform.localPosition.y < 20)
            {
                starsToRemove.Add(star);
            }
        }
        foreach (GameObject starRemove in starsToRemove)
        {
            Stars.Remove(starRemove);
        }
    }

    // Update is called once per frame
    void Update()
    {
        BigFlashyBeam.transform.localScale = new Vector3(Mathf.Lerp(BigFlashyBeam.transform.localScale.x, BeamInitialScale, 0.5f), BigFlashyBeam.transform.localScale.y, 1.0f);
    }

    public void StartStarLightning()
    {
        foreach (var zap in Zaps)
        {
            StartCoroutine(RunLightningBolt(zap));
        }
    }

    IEnumerator RunLightningBolt(ZapFXController controller)
    {
        float time = 0;
        while (true)
        {
            int starIndex = Random.Range(0, Stars.Count);
            float delay = Mathf.Max(FlashDuration, 1.0f / (Frequency * Random.Range(0.5f, 1.5f) / Zaps.Count));
            time += delay;
            yield return new WaitForSeconds(delay);
            controller.SetThickness(FlashStrengthOverTime.Evaluate(time) - 0.5f, FlashStrengthOverTime.Evaluate(time));
            controller.SetupLine(Stars[starIndex].transform.position, Vector3.Lerp(LightningTargetEnd1, LightningTargetEnd2, Random.Range(0.0f, 1.0f)));
            //controller.SetupLine(Stars[starIndex].transform.position, new Vector3(LightningTargetEnd1.x, Stars[starIndex].transform.position.y - Random.Range(50, 200)));
            controller.Play(FlashDuration);
            controller.GetComponentInChildren<SpriteRenderer>().gameObject.transform.position = Stars[starIndex].transform.position;
            StartCoroutine(FlashStarFlare(controller.GetComponentInChildren<SpriteRenderer>().gameObject, FlashDuration, time));
            
        }
    }

    IEnumerator FlashStarFlare(GameObject Star, float FlareFlashDuration, float time)
    {
        float timer = 0.0f;
        while (timer < FlashDuration)
        {
            Star.transform.localScale = new Vector3(
                StarFlashNormalizedXScaleCurve.Evaluate(timer / FlareFlashDuration) * FlashStrengthOverTime.Evaluate(time), 
                StarFlashNormalizedYScaleCurve.Evaluate(timer / FlareFlashDuration) * FlashStrengthOverTime.Evaluate(time),
                1.0f);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}

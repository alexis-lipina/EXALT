using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnvironmentPhysics))]
public class CollapsingPlatform : MonoBehaviour
{
    [SerializeField] private bool CanCollapseByTrigger = false;
    [SerializeField] private TriggerVolume Trigger;
    [SerializeField] private float RumbleDuration = 1.0f;
    [SerializeField] private CollapsingPlatform[] DependentCollapsingPlatforms; //nodes which depend on this one for support


    private float RumbleTimer;
    [SerializeField] private float PropagateDelayMin = 0.25f;
    [SerializeField] private float PropagateDelayMax = 0.75f;
    private bool IsCollapsing = false;

    private EnvironmentPhysics environmentPhysics;
    

    // Start is called before the first frame update
    void Start()
    {
        environmentPhysics = GetComponent<EnvironmentPhysics>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CanCollapseByTrigger && Trigger.IsTriggered)
        {
            StartCollapse();
        }
    }

    public void StartCollapse()
    {
        if (!IsCollapsing)
        {
            StartCoroutine(BeginCollapseCoroutine());
            IsCollapsing = true;
            Debug.Log("RUMBLE RUMBLE");
        }
    }

    private IEnumerator BeginCollapseCoroutine()
    {
        Vector3 originalPosition = transform.position;
        float originalTopHeight = environmentPhysics.TopHeight;
        float originalBottomHeight = environmentPhysics.BottomHeight;

        //propagate collapse
        for (int i = 0; i < DependentCollapsingPlatforms.Length; i++)
        {
            StartCoroutine(PropagateTo(DependentCollapsingPlatforms[i]));
        }


        //rumble
        RumbleTimer = RumbleDuration;
        while (RumbleTimer > 0)
        {
            yield return new WaitForEndOfFrame();
            transform.position = originalPosition + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
            RumbleTimer -= Time.deltaTime;
        }

        //collapse
        float heightOffset = 0.0f;
        float opacity = 1.0f;
        environmentPhysics._isCollapsed = true;
        while (heightOffset > -100)
        {
            environmentPhysics.TopHeight += heightOffset;
            environmentPhysics.BottomHeight += heightOffset;
            heightOffset -= Time.deltaTime * 2f;

            opacity -= Time.deltaTime * 2f;
            environmentPhysics.TopSprite.material.SetFloat("_Opacity", opacity);
            environmentPhysics.FrontSprite.material.SetFloat("_Opacity", opacity);

            environmentPhysics.TopSprite.gameObject.transform.position += new Vector3(0.0f, heightOffset, 0.0f);
            environmentPhysics.FrontSprite.gameObject.transform.position += new Vector3(0.0f, heightOffset, 0.0f);
            yield return new WaitForEndOfFrame();
        }

    }

    private IEnumerator PropagateTo(CollapsingPlatform other)
    {
        yield return new WaitForSeconds(Random.Range(PropagateDelayMin, PropagateDelayMax));
        other.StartCollapse();
    }
}

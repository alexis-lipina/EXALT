using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCollapseManager : MonoBehaviour
{
    [SerializeField] private CollapsingPlatform[] PlatformsToCollapse; //these will collapse in order
    [SerializeField] private float TimeBetweenCollapses = 1.0f;
    [SerializeField] private float TimeUntilStart = 5.0f;
    private bool HasStarted;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        TimeUntilStart -= Time.deltaTime;
        if (TimeUntilStart < 0 && !HasStarted)
        {
            HasStarted = true;
            StartCoroutine(CollapseAllInSequence());
        }*/
    }

    public void StartCollapse()
    {
        StartCoroutine(CollapseAllInSequence());
    }
    private IEnumerator CollapseAllInSequence()
    {
        for (int i = 0; i < PlatformsToCollapse.Length; i++)
        {
            PlatformsToCollapse[i].StartCollapse();
            yield return new WaitForSeconds(TimeBetweenCollapses);
        }
    }

}

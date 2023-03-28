using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RippleRow : MonoBehaviour
{
    enum CardinalDirection { North, South, East, West};

    [SerializeField] AnimationCurve NormalizedWavePattern;
    [SerializeField] List<MovingEnvironment> UnsortedRippleEnvironment;
    [SerializeField] CardinalDirection RippleDirection;
    List<MovingEnvironment> SortedRippleEnvironment;

    const float PropagationRate = 20; // "speed" of wavefront in steps hit per second
    const float CycleDuration = 3.0f; // duration of each steps cycle

    void Start()
    {
        SortedRippleEnvironment = new List<MovingEnvironment>();
        SortRipples();
    }

    // Update is called once per frame
    public void TriggerRipple()
    {
        StartCoroutine(Wave());
    }

    IEnumerator Wave()
    {
        foreach (MovingEnvironment environment in SortedRippleEnvironment)
        {
            environment.PlayAnim();
            yield return new WaitForSeconds(1.0f / PropagationRate);
        }
    }

    void SortRipples()
    {
        while (UnsortedRippleEnvironment.Count > 0)
        {
            MovingEnvironment envt = null;
            float bestdistance = RippleDirection == CardinalDirection.North || RippleDirection == CardinalDirection.East ? 100000 : -100000;
            for (int i = 0; i < UnsortedRippleEnvironment.Count; i++) // find the next "nearest" in the direction of ripple
            {
                float currentdistance;
                switch (RippleDirection)
                {
                    case CardinalDirection.East:
                        currentdistance = UnsortedRippleEnvironment[i].transform.position.x;
                        if (currentdistance < bestdistance)
                        {
                            envt = UnsortedRippleEnvironment[i];
                            bestdistance = currentdistance;
                        }
                        break;
                    case CardinalDirection.West:
                        currentdistance = UnsortedRippleEnvironment[i].transform.position.x;
                        if (currentdistance > bestdistance)
                        {
                            envt = UnsortedRippleEnvironment[i];
                            bestdistance = currentdistance;
                        }
                        break;
                    case CardinalDirection.North:
                        currentdistance = UnsortedRippleEnvironment[i].transform.position.y;
                        if (currentdistance < bestdistance)
                        {
                            envt = UnsortedRippleEnvironment[i];
                            bestdistance = currentdistance;
                        }
                        break;
                    case CardinalDirection.South:
                        currentdistance = UnsortedRippleEnvironment[i].transform.position.y;
                        if (currentdistance > bestdistance)
                        {
                            envt = UnsortedRippleEnvironment[i];
                            bestdistance = currentdistance;
                        }
                        break;
                }
            }
            SortedRippleEnvironment.Add(envt);
            UnsortedRippleEnvironment.Remove(envt);
        }
    }
}

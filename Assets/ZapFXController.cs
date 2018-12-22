using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZapFXController : MonoBehaviour
{
    [SerializeField] private LineRenderer _coreBolt;
    [SerializeField] private LineRenderer _outlineBolt;
    [SerializeField] private float _segmentDensity; //how many vertices per unit length of bolt
    [SerializeField] private float _zigZagEccentricity; //scalar for point randomization

    private float _timer;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else
        {
            _timer = 0f;
            _coreBolt.enabled = false;
            _outlineBolt.enabled = false;
        }
	}

    /// <summary>
    /// Sets up the zap line with start and end points, as well as the length
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void SetupLine(Vector3 start, Vector3 end)
    {
        float length = Vector3.Distance(start, end);
        int numVertices = (int)(_segmentDensity * length); //would use Mathf.FloorToInt but this seems like the faster way, since casting truncates anyway
        //Debug.Log(numVertices);
        _coreBolt.positionCount = numVertices;
        _outlineBolt.positionCount = numVertices;

        Vector3 outlineOffset = new Vector3(0f, 0f, 0.01f);
        Vector3 tempZeroer = new Vector3(1, 1, 0);
        Vector3 currentPoint;
        Vector2 randomVector;
        /*
        for (int i = 0; i < numVertices; i++)
        {
            //get next point along line (subdivide with lerp?)
            //randomize position somehow, pass to _coreBolt

            currentPoint = Vector3.Lerp(start, end, i/((float)numVertices));
            randomVector = Random.insideUnitCircle * _zigZagEccentricity;
            currentPoint += new Vector3(randomVector.x, randomVector.y, 0f);
            _coreBolt.SetPosition(i, currentPoint);
            _outlineBolt.SetPosition(i, currentPoint + outlineOffset);
        }
        
        //hardwire first and last points, rather than changing for-loop boundaries to avoid invalid ranges at short ranges
        _coreBolt.SetPosition(0, start);
        _coreBolt.SetPosition(numVertices-1, end);
        _outlineBolt.SetPosition(0, start + outlineOffset);
        _outlineBolt.SetPosition(numVertices - 1,end + outlineOffset);
        */



        //New right-angle algorithm

        //determine direction of first line
        bool isHorizontalFirst = (start - end).x > (start - end).y;
        Vector3 firstPoint;
        Vector3 secondPoint;
        //for each node, from first to penultimate
        for (int i = 1; i < numVertices - 1; i++)
        {
            //find point of i and i+1
            firstPoint = Vector3.Lerp(start, end, i / ((float)numVertices));
            secondPoint = Vector3.Lerp(start, end, (i + 1) / ((float)numVertices));
            //calculate current point based off those

        }
    }

    /// <summary>
    /// Start playing the animation
    /// </summary>
    /// <param name="duration"></param>
    public void Play(float duration)
    {
        _timer = duration;
        _coreBolt.enabled = true;
        _outlineBolt.enabled = true;
    }
}

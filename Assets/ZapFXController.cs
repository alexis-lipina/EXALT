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

        //Old, basic, random-offset zigzag
         // /*
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

        // */



        //New right-angle algorithm
        /*  So the way this works is a bit funky:
         *        
         *                             X----- (...)
         *                             |
         *           X--------X        |
         *           |        |        o
         *           |        o        |
         *           o        |        |
         *           |        X--------X
         *           |
         *  (...) ---X
         *  
         *  KEY:
         *      "o"     : POINTS on a line from the starting point (player) to the end point (enemy/wall/whatever)
         *  "-" and "|" : lines to be drawn
         *      "x"     : VERTICES generated to draw lines between
         *      
         *  This algorithm takes points "o" along a line (based on a "segment density" field visible in the inspector) and generates a 
         *  zigzaggy pattern  of vertices "X", which will be passed to a LineRenderer. In order for the zigzags to fit the right-angle 
         *  style of the game, each vertex will have either the X or Y position in common with its previous / next vertex.
         *  
         *  For any given point (P) on the line, vertices (P).v1 and (P).v2 will be generated :
         *  
         *  =======| v1 |
         *  
         *  (P).v1.x = (P).x;
         *  if ( (P-1) == null ) (P).v1.y = (P).y
         *  else (P).v1.y = (P-1).v2.y
         *  
         *  (P).v1 will inherit the x-position of (P) and the y-position of (P-1).v2, where (P-1) is the point immediately preceeding (P). 
         *  If (P-1) is null, (P) can be assumed to be the starting point and the y-position of (P).v1 is set to the y-position of (P).
         *  Otherwise, the y-position of (P).v1 is set to the y-position of (P-1).v2
         *  
         *  =======| v2 |
         *  
         *  (P).v2.x = (P).x
         *  if ( (P+1) == null ) (P).v2.y = (P).y
         *  else 
         *  { float C = Random.Range(0, 1) * sign((P).y - (P).v1.y)
         *    (P).v2.y = (P).y + C
         *  }
         *  
         *  (P).v2 will inherit the x-position of (P) and the y-position of (P) + C, where C is a random float offset generated, whose sign
         *  is the sign of the operation (P).y - (P).v1.y. This ensures the generated line zigzags around the original line but doesnt 
         *  deviate too far. If (P+1) is null, (P) can be assumed to be the ending point and the y-position of (P).v2 is set to the 
         *  y-position of (P).
         *  
         *  
         *  ========| ADDENDUM
         *  Will need to implement ability to toggle between horizontally and vertically oriented vertices.
         * */

        

        //determine direction of first line
        //bool isHorizontal = (start - end).x > (start - end).y;
        int currentIndex = 0; //keeps track of index of list of vertices in linerenderer
        float randomOffset = Random.Range(0.1f, 1f);
        Vector3 v1, v2;
        Vector3 previousPoint;

        _coreBolt.positionCount = numVertices * 2;
        _outlineBolt.positionCount = numVertices * 2;

        //setup start
        _coreBolt.SetPosition(0, start);
        _outlineBolt.SetPosition(0, start + outlineOffset);
        _coreBolt.SetPosition(1, start + new Vector3(0, randomOffset, 0));
        _outlineBolt.SetPosition(1, start + new Vector3(0, randomOffset, 0) + outlineOffset);
        currentIndex = 2;

        previousPoint = start;
        //for each node, from first to penultimate
        for (int i = 1; i < numVertices - 1; i++)
        {
            currentPoint = Vector3.Lerp(start, end, i / ((float)numVertices)); // Get (P)
            randomVector = Random.insideUnitCircle * _zigZagEccentricity;
            currentPoint += new Vector3(randomVector.x, randomVector.y, 0f);
            v1 = currentPoint; //v1.x = P.x
            v1.y = previousPoint.y + randomOffset;

            if (randomOffset > 0)
            {
                randomOffset = Random.Range(0.1f, 1f) * -1;
            }
            else
            {
                randomOffset = Random.Range(0.1f, 1.0f);
            }


            v2 = currentPoint;
            v2.y += randomOffset;

            _coreBolt.SetPosition(currentIndex, v1);
            _outlineBolt.SetPosition(currentIndex, v1 + outlineOffset);
            ++currentIndex;
            _coreBolt.SetPosition(currentIndex, v2);
            _outlineBolt.SetPosition(currentIndex, v2 + outlineOffset);
            ++currentIndex;

            previousPoint = currentPoint;
        }

        //add end nodes
        currentPoint = end;
        v1 = currentPoint;
        v1.y = previousPoint.y + randomOffset;

        v2 = currentPoint;

        _coreBolt.SetPosition(currentIndex, v1);
        _outlineBolt.SetPosition(currentIndex, v1 + outlineOffset);
        ++currentIndex;
        _coreBolt.SetPosition(currentIndex, v2);
        _outlineBolt.SetPosition(currentIndex, v2 + outlineOffset);
        ++currentIndex;

        

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

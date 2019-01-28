using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZapFXController : MonoBehaviour
{
    //[SerializeField] private LineRenderer _coreBolt;
    [SerializeField] private OrthoLine _outlineBolt;
    [SerializeField] private OrthoLine _coreBolt;
    [SerializeField] private float _segmentDensity; //how many vertices per unit length of bolt
    [SerializeField] private AnimationCurve _amplitude;
    [SerializeField] private AnimationCurve _randomization;
    [SerializeField] private AnimationCurve _wavelength;
    [SerializeField] private AudioSource _soundEffect;

    private float _timer = 0f;

	
	// Update is called once per frame
	void Update ()
    {
        /*
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else
        {
            _timer = 0f;
            
            _coreBolt.GetComponent<MeshRenderer>().enabled = false;
            _outlineBolt.GetComponent<MeshRenderer>().enabled = false;
        }
        */
	}

    /// <summary>
    /// Sets up the zap line with start and end points, as well as the length
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void SetupLine(Vector3 start, Vector3 end)
    {

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

        float length = Vector3.Distance(start, end);
        int numVertices = (int)(_segmentDensity * length); //would use Mathf.FloorToInt but this seems like the faster way, since casting truncates anyway

        float sumOfIncrements = 0f;
        float runningTotalIncrements = 0f;
        for (int i = 0; i < numVertices; i++)
        {
            sumOfIncrements += _wavelength.Evaluate(i / ((float)numVertices));
        }

        Vector3 outlineOffset = new Vector3(0f, 0f, 0.1f);
        Vector3 tempZeroer = new Vector3(1, 1, 0);
        Vector3 currentPoint;
        Vector2 randomVector;


        //determine direction of first line
        bool isHorizontal = Mathf.Abs((start - end).x) > Mathf.Abs((start - end).y);
        float randomOffset = Random.Range(0.1f, 1f);
        Vector3 v1, v2;
        Vector3 previousPoint;
        List<Vector3> corePoints = new List<Vector3>(numVertices * 2);
        List<Vector3> outlinePoints = new List<Vector3>(numVertices * 2);

        //setup start
        corePoints.Add(start);
        outlinePoints.Add(start + outlineOffset);
        if (isHorizontal)
        {
            corePoints.Add(start + new Vector3(0, randomOffset, 0));
            outlinePoints.Add(start + new Vector3(0, randomOffset, 0) + outlineOffset);
        }
        else
        {
            corePoints.Add(start + new Vector3(randomOffset, 0, 0));
            outlinePoints.Add(start + new Vector3(randomOffset, 0, 0) + outlineOffset);
        }
        runningTotalIncrements += _wavelength.Evaluate(0f);
        previousPoint = start;
        //for each node, from first to penultimate
        for (int i = 1; i < numVertices - 1; i++)
        {
            runningTotalIncrements += _wavelength.Evaluate((i)/((float)numVertices));
            //currentPoint = Vector3.Lerp(start, end, i / ((float)numVertices)); // Get (P)
            currentPoint = Vector3.Lerp(start, end, runningTotalIncrements / sumOfIncrements);
            randomVector = Random.insideUnitCircle * _randomization.Evaluate(i/((float)numVertices));
            currentPoint += new Vector3(randomVector.x, randomVector.y, 0f);
            v1 = currentPoint; //v1.x = P.x
            if (isHorizontal)
            {
                v1.y = previousPoint.y + randomOffset;
            }
            else
            {
                v1.x = previousPoint.x + randomOffset;
            }

            if (randomOffset > 0)
            {
                randomOffset = Random.Range(0.1f, 1f) * -1 * _amplitude.Evaluate(i/ ((float)numVertices));
            }
            else
            {
                randomOffset = Random.Range(0.1f, 1.0f) * _amplitude.Evaluate(i / ((float)numVertices));
            }


            v2 = currentPoint;
            if (isHorizontal)
            {
                v2.y += randomOffset;
            }
            else
            {
                v2.x += randomOffset;
            }

            corePoints.Add(v1);
            outlinePoints.Add(v1 + outlineOffset);
            corePoints.Add(v2);
            outlinePoints.Add(v2 + outlineOffset);


            previousPoint = currentPoint;
        }
        runningTotalIncrements += _wavelength.Evaluate(1);

        //add end nodes
        currentPoint = Vector3.Lerp(start, end, 0.95f); //prevents overpenetration, since mesh draws a bit deeper than the actual end node
        v1 = currentPoint;
        if (isHorizontal)
        {
            corePoints[corePoints.Count - 1] += new Vector3(0, -corePoints[corePoints.Count - 1].y + v1.y, 0);
            outlinePoints[corePoints.Count - 1] += new Vector3(0, -outlinePoints[corePoints.Count - 1].y + v1.y, 0);
            v1.x = previousPoint.x;// + randomOffset;
        }
        else
        {
            corePoints[corePoints.Count - 1] += new Vector3(-corePoints[corePoints.Count - 1].x + v1.x, 0, 0);
            outlinePoints[corePoints.Count - 1] += new Vector3(-outlinePoints[corePoints.Count - 1].x + v1.x, 0, 0);
            v1.y = previousPoint.y;// + randomOffset;
        }
        v2 = currentPoint;

        corePoints.Add(v1);
        outlinePoints.Add(v1 + outlineOffset);
        corePoints.Add(v2);
        outlinePoints.Add(v2 + outlineOffset);
        //Debug.Log("Running total : " + runningTotalIncrements);
        //Debug.Log("Actual Total : " + sumOfIncrements);


        //pass to vfx renderers
        _coreBolt.SetPoints(corePoints);
        _outlineBolt.SetPoints(outlinePoints);
        //_coreBolt.UpdateLineMesh();
        //_outlineBolt.UpdateLineMesh();
    }

    /// <summary>
    /// Start playing the animation
    /// </summary>
    /// <param name="duration"></param>
    public void Play(float duration)
    {
        StopCoroutine(PlayLightningBolt(duration));
        StartCoroutine(PlayLightningBolt(duration));
        
        /*
        _coreBolt.GetComponent<MeshRenderer>().enabled = true;
        _outlineBolt.GetComponent<MeshRenderer>().enabled = true;
        */
    }

    IEnumerator PlayLightningBolt(float duration)
    {
        //the commented out block below would be better for the bigger attack
        /*
        _coreBolt.GetComponent<MeshRenderer>().enabled = true;
        _outlineBolt.GetComponent<MeshRenderer>().enabled = true;
        yield return new WaitForSeconds(duration * 0.25f);
        _coreBolt.GetComponent<MeshRenderer>().enabled = false;
        _outlineBolt.GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(duration * 0.25f);
        _coreBolt.GetComponent<MeshRenderer>().enabled = true;
        _outlineBolt.GetComponent<MeshRenderer>().enabled = true;
        yield return new WaitForSeconds(duration * 0.5f);
        _coreBolt.GetComponent<MeshRenderer>().enabled = false;
        _outlineBolt.GetComponent<MeshRenderer>().enabled = false;
        */

        ScreenFlash.InstanceOfScreenFlash.PlayFlash(0.3f, 0.1f);
        _soundEffect.Play();

        _coreBolt.GetComponent<MeshRenderer>().enabled = true;
        _outlineBolt.GetComponent<MeshRenderer>().enabled = true;

        _coreBolt.LineThickness = _coreBolt.LineThickness * 8f;
        _outlineBolt.LineThickness = _outlineBolt.LineThickness * 4f;
        _coreBolt.UpdateLineMesh();
        _outlineBolt.UpdateLineMesh();
        yield return new WaitForSeconds(duration * 0.25f);

        _coreBolt.LineThickness = _coreBolt.LineThickness * .25f;
        _outlineBolt.LineThickness = _outlineBolt.LineThickness * .5f;
        _coreBolt.UpdateLineMesh();
        _outlineBolt.UpdateLineMesh();
        yield return new WaitForSeconds(duration * 0.25f);

        _coreBolt.LineThickness = _coreBolt.LineThickness * 0.5f;
        _outlineBolt.LineThickness = _outlineBolt.LineThickness * 0.5f;
        _coreBolt.UpdateLineMesh();
        _outlineBolt.UpdateLineMesh();

        yield return new WaitForSeconds(duration * 0.5f);

        _coreBolt.GetComponent<MeshRenderer>().enabled = false;
        _outlineBolt.GetComponent<MeshRenderer>().enabled = false;
    }
}

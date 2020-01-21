using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthShatterController : MonoBehaviour
{
    [SerializeField] private Transform BlocksParent;
    [SerializeField] private float timeToWait = 5;
    [SerializeField] private float shatterHeightScalar = 1;
    private bool hasLaunched = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timeToWait -= Time.deltaTime;
        if (timeToWait < 0 && hasLaunched == false)
        {
            hasLaunched = true;
            StartShatter();
        }
    }


    private void StartShatter()
    {
        ShatteredEarth[] blocks = BlocksParent.GetComponentsInChildren<ShatteredEarth>();

        for (int i = 0; i < blocks.Length; i++)
        {
            Vector3 newPosition = blocks[i].GetBlockCenter();
            Vector2 perspectiveModifier = new Vector2(1.0f, 2.0f);
            Vector2 newPosition2D = newPosition;
            float newHeight = 1.0f / ( (newPosition2D * perspectiveModifier).magnitude * 0.5f );
            //newHeight = Mathf.Sqrt(newHeight);
            newHeight *= shatterHeightScalar;
            newHeight *= Random.Range(0.5f, 1f);
            //newHeight /= newPosition.magnitude;

            newPosition += new Vector3(0, newHeight, 0);


            blocks[i].newPosition = newPosition;
            blocks[i].IsShattered = true;
            Debug.Log("START" + blocks[i].GetBlockCenter());
            Debug.Log("END" + newPosition);

        }

    }
}

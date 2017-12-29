using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Params
{
    public Vector3[] angles = new Vector3[8]
    {
        Vector3.zero,
        new Vector3(90,0,0),
        new Vector3(0,90,0),
        new Vector3(0,0,90),
        new Vector3(90,90,0),
        new Vector3(90,0,90),
        new Vector3(0,90,90),
        new Vector3(90,90,90),
    };
}

public class Rotator : MonoBehaviour {

    [SerializeField]
    float interval;

    Transform myTransform;

    float timer = 0.0f;    

    Vector3 startAngle,destAngle;

    Params param = new Params();
    // Use this for initialization
    void Start () {
        this.myTransform = this.GetComponent<Transform>();
        myTransform.localEulerAngles = param.angles[0];
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (timer < interval)
        {
            timer += Time.fixedDeltaTime;
            myTransform.localEulerAngles = Vector3.Lerp(startAngle,destAngle,timer / interval);
        }
        else
        {
            myTransform.localEulerAngles = destAngle;

            destAngle = this.param.angles[Random.Range(1,param.angles.Length)];
            startAngle = myTransform.localEulerAngles;
            timer = 0;
        }
        
    }
}

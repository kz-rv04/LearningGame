using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Learner : MonoBehaviour {

    [SerializeField]
    Transform[] joints;

    [SerializeField]
    int stateNum; // 関節の状態の数

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RotateJoints(Param param)
    {
        for (int i = 0; i < joints.Length; i++)
        {
            this.joints[i].localEulerAngles = param.angles[i];
        }
    }

    public class Param
    {
        public Vector3[] angles;

        public Param() { }

        /*
        public Param(Vector3 lf, Vector3 rf, Vector3 lb, Vector3 rb)
        {
            this.angleLF = lf;
            this.angleRF = rf;
            this.angleLB = lb;
            this.angleRB = rb;
        }
        public static Param CreateRandom()
        {
            var param = new Param();
            param.angleLF = GetRandomAngle();
            param.angleRF = GetRandomAngle();
            param.angleLB = GetRandomAngle();
            param.angleRB = GetRandomAngle();
            return param;
        }
        */

        public Param(Vector3[] a)
        {
            this.angles = a;
        }
        public static Param CreateRandom()
        {
            var param = new Param();
            for (int i = 0;i < param.angles.Length;i++)
            {
                param.angles[i] = GetRandomAngle();
            }
            return param;
        }


        public static Vector3 GetRandomAngle()
        {
            return new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        }
    }
}

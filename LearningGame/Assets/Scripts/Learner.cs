using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    ACTIVE,FINISHED
}
public class Learner : MonoBehaviour {

    [SerializeField]
    Transform[] joints;

    [SerializeField]
    float interval;

    [SerializeField]
    private State state = State.FINISHED;
    

    public IEnumerator RotateJoints(List<Param> paramList)
    {
        yield return new WaitForSeconds(0.5f);

        LearnerState = State.ACTIVE;
        for (;LearnerState == State.ACTIVE;)
        {
            foreach (Param param in paramList)
            {
                if (LearnerState != State.ACTIVE)
                {
                    yield break;
                }
                for (int i = 0; i < joints.Length; i++)
                {
                    StartCoroutine(this.Rotate(joints[i], param.angles[i]));
                }
                yield return new WaitForSeconds(interval + 0.2f);
            }
        }
    }

    IEnumerator Rotate(Transform target,Vector3 destAngle)
    {
        Vector3 startAngle = target.localEulerAngles;
        float startTime = Time.time;
        for (;Time.time - startTime < this.interval;)
        {
            if (LearnerState == State.FINISHED)
                yield break;

            target.localEulerAngles =
                Vector3.Lerp(startAngle, destAngle, (Time.time - startTime) / this.interval);
            yield return null;
        }
        target.localEulerAngles = destAngle;
    }

    public void StopLearner()
    {
        this.state = State.FINISHED;
    }
    public State LearnerState
    {
        get { return this.state; }
        set { this.state = value; }
    }

    public int GetJointNum
    {
        get { return this.joints.Length; }
    }

    [System.Serializable]
    public class Param
    {
        public Vector3[] angles;

        public Param() { }
        
        public static Param CreateRandom(int jointNum)
        {
            var param = new Param();
            param.angles = new Vector3[jointNum];
            for (int i = 0;i < param.angles.Length;i++)
            {
                if (i > 3)
                    param.angles[i] = GetRandomAngle();
                else
                    param.angles[i] = GetRandomAngle(true,false,true);
            }
            return param;
        }

        public static Param CreateParam(Vector3[] angle)
        {
            var param = new Param();
            param.angles = angle;
            return param;
        }


        public static Vector3 GetRandomAngle(bool X = true,bool Y = true,bool Z = true)
        {
            Vector3 angle = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            if (!X) angle.x = 0; if (!Y) angle.y = 0; if (!Z) angle.z = 0;
            return angle;
        }

        public string GetString()
        {
            string str = string.Empty;
            foreach(Vector3 angle in angles)
            {
                str += string.Format("{0},{1},{2}\n", angle.x, angle.y, angle.z);
            }
            return str;
        }
    }
}

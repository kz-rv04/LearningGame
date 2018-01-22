using System.Collections;
using System;
using KZ.IOLib;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

// 遺伝子データを読み込んで動かすためだけのクラス
public class LearnerController : MonoBehaviour
{
    [SerializeField]
    Transform[] joints;

    [SerializeField]
    float interval;

    [SerializeField]
    int stateNum; // 関節の状態の数

    List<GAController.Gene> geneList = new List<GAController.Gene>();

    [SerializeField]
    private State state = State.FINISHED;

    // 読み込むファイルと無視する文字列
    [SerializeField]
    string dataPath;
    [SerializeField]
    List<string> ignoreItems;

    // Use this for initialization
    void Start () {
        this.geneList = new List<GAController.Gene>();
	}
	
	// Update is called once per frame
	void Update () {
        if (state == State.FINISHED && Input.GetKeyDown(KeyCode.Q))
        {
            LoadBestGenes(this.dataPath, ref geneList, 1);
            StartCoroutine(this.RotateJoints(this.geneList[0].paramList));
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            this.StopLearner();
            StopCoroutine("RotateJoints");
        }
    }

    public IEnumerator RotateJoints(List<Learner.Param> paramList)
    {
        yield return new WaitForSeconds(0.5f);

        LearnerState = State.ACTIVE;
        for (; LearnerState == State.ACTIVE;)
        {
            foreach (Learner.Param param in paramList)
            {
                if (LearnerState != State.ACTIVE)
                {
                    yield break;
                }
                for (int i = 0; i < joints.Length; i++)
                {
                    StartCoroutine(this.Rotate(joints[i], param.angles[i]));
                }
                yield return new WaitForSeconds(interval + 0.05f);
            }
        }
    }

    IEnumerator Rotate(Transform target, Vector3 destAngle)
    {
        Vector3 startAngle = target.localEulerAngles;
        float startTime = Time.time;

        Quaternion start = target.localRotation;
        Quaternion dest = Quaternion.Euler(destAngle);
        for (; Time.time - startTime < this.interval;)
        {
            if (LearnerState == State.FINISHED)
                yield break;
            /*
            target.localEulerAngles =
                Vector3.Lerp(startAngle, destAngle, (Time.time - startTime) / this.interval);
            */
            target.localRotation = Quaternion.Lerp(start, dest, (Time.time - startTime) / this.interval);
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

    /// <summary>
    /// BestGeneを読み込む
    /// </summary>
    /// <param name="path"></param>
    /// <param name="bestGenes">読み込み先のリスト</param>
    /// <param name="num">読み込む遺伝子データの数</param>
    private void LoadBestGenes(string path, ref List<GAController.Gene> bestGenes, int num)
    {
        using (StreamReader sr = new StreamReader(path))
        {
            string str = sr.ReadLine();
            string[] g = str.Split(new char[2] { ' ', ',' });
            int generation = Int32.Parse(g[1]);

            List<List<float>> data = new List<List<float>>();

            CSVIO.LoadMap(ref data, sr.ReadToEnd(), this.ignoreItems);

            int index = 0;
            for (int i = 0; i < num; i++)
            {
                List<Learner.Param> paramList = new List<Learner.Param>();
                for (int j = 0; j < stateNum; j++)
                {
                    paramList.Add(MakeParam(data.GetRange(index, GetJointNum)));
                    index += GetJointNum;
                }
                geneList.Add(new GAController.Gene(paramList));
            }
            sr.Close();
        }

    }

    // JointNum個のベクトルデータからParamを作成
    private Learner.Param MakeParam(List<List<float>> lines)
    {
        Vector3[] vec = new Vector3[this.GetJointNum];
        for (int i = 0; i < this.GetJointNum; i++)
        {
            vec[i] = FloatToVec(lines[i]);
        }
        return Learner.Param.CreateParam(vec);
    }

    private Vector3 FloatToVec(List<float> i)
    {
        Vector3 vec = new Vector3(i[0], i[1], i[2]);
        //print(vec);
        return vec;
    }
}

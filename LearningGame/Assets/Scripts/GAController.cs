using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;
using KZ.IOLib;


public class GAController : MonoBehaviour {

    // 生成するゲームオブジェクト
    [SerializeField]
    GameObject learnerObj;

    private List<Learner> controller;

    [SerializeField]
    List<Gene> geneList = new List<Gene>();

    [SerializeField]
    List<Gene> bestGenes = new List<Gene>(5);
    public List<Gene> GetBestGene
    {
        get { return this.bestGenes; }
    }

    // 読み込むファイルと無視する文字列
    [SerializeField]
    string dataPath;
    [SerializeField]
    List<string> ignoreItems;

    [SerializeField]
    int stateNum; // 関節の状態の数
    int jointNum; // 関節の数

    [SerializeField]
    float waveTime;// 1世代あたりの時間
    public float timer; // 現在の時間


    int childNum = 50;
    int keepNum = 5;
    int extinctionNum = 15;
    float mutateRate = 0.02f;
    int generation = 0; // 現在の世代
    public int GetGeneration { get { return this.generation; } }

    Spawner spawner;

	// Use this for initialization
	void Start () {
        spawner = new Spawner(this.learnerObj,new Vector3(50,0,0));
        jointNum = learnerObj.GetComponent<Learner>().GetJointNum;
        foreach (var i in Enumerable.Range(0, childNum)) {
            Gene g = new Gene(this.stateNum,this.jointNum);
            geneList.Add(g);
		}
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(StartGA());
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            print("LoadData : " + dataPath);
            this.LoadData(dataPath, ref this.geneList);
        }

	}
	IEnumerator StartGA() {
		string logDirPath = string.Format ("Log/{0}", DateTime.Now.ToString ("yyyyMMdd_hhmmss"));

		while (true) {

            this.controller = spawner.SpawnObjects(this.childNum, Vector3.zero);

            print("Generation : " + generation);
            // 各個体を動作させる
            for (int i = 0; i < childNum; i++)
            {
                if (this.controller[i].LearnerState != State.ACTIVE)
                {
                    this.geneList[i].generation = generation;
                    StartCoroutine(this.controller[i].RotateJoints(geneList[i].paramList));
                }
            }
            // 動作終了時間まで待機
            for (this.timer = 0.0f;this.timer < waveTime;)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            
            // 各個体の動作を止め,値を評価
            for (int i = 0; i < childNum; i++)
            {
                controller[i].StopLearner();
                geneList[i].point =
                    controller[i].gameObject.transform.position.z;
                Destroy(controller[i].gameObject);
            }
            
            geneList.Sort ((a, b) => {
				if (a.point > b.point) return -1;
				else if (a.point < b.point) return 1;
				else return 0;
			});


            float totalPoint = 0.0f;
			float minPoint = geneList[geneList.Count - extinctionNum - 1].point;
			foreach (var i in Enumerable.Range(0, geneList.Count -extinctionNum)) {
				totalPoint += (geneList [i].point - minPoint);
				if (minPoint > geneList [i].point) {
					minPoint = geneList [i].point;
				}
			}
            
			foreach (var gene in geneList) {
				gene.rate = Mathf.Max((gene.point - minPoint) / totalPoint, 0);
            }
            // bestgeneを更新
            this.UpdateBestGene(geneList[0]);
            /*
			foreach (var gene in geneList) {
				Debug.Log (string.Format ("{0}/{1}", gene.point, gene.rate));
			}
            */
            Debug.Log (string.Format ("=== Gen {0} : Top {1} m ===", generation, geneList [0].point));

			generation++;

            if (generation % 20 == 0)
            {
                print("WriteLog");
                this.writeLog(logDirPath, generation, geneList);
            }

			var newGeneList = new List<Gene> ();
            
            var elite = SelectElite(this.geneList);
			foreach (var i in Enumerable.Range(0, childNum)) {
				if (i < keepNum) {
					newGeneList.Add (geneList[i]);
				} else {
                    /*
					Gene dad = SelectParent (geneList);
					Gene mom = SelectParent (geneList);
                    */
                    // eliteの中から重複無しで2つ子を選択
                    var parents = SelectRandomParents(elite);
                    Gene dad = parents[0];
                    Gene mom = parents[1];
                    //Gene child = OnePointCrossover (dad, mom);
                    Gene child = TwoPointCrossover(dad,mom);
					Mutate (ref child);
					newGeneList.Add (child);
				}
			}

			geneList = newGeneList;
		}
	}

    private void writeLog(string logDirPath, int generation, List<Gene> geneList)
    {
        if (!Directory.Exists(logDirPath)) Directory.CreateDirectory(logDirPath);

        string logPath = string.Format("{0}/{1}.csv", logDirPath, generation);
        using (StreamWriter sw = new StreamWriter(logPath, false))
        {
            sw.WriteLine(string.Format("Generation {0}", generation));
            foreach (var gene in geneList)
            {
                string str = string.Format("Point {0},Rate {1},ParamCnt {2}\n", gene.point, gene.rate, gene.paramList.Count);
                foreach (var param in gene.paramList)
                {
                    str += param.GetString();
                    str += "\n";
                }
                sw.WriteLine(str);
            }
            sw.Flush();
            sw.Close();
        }
    }

    /// <summary>
    /// csvファイルからデータを読み込む
    /// </summary>
    /// <param name="path">読み込むファイルのパス</param>
    /// <param name="geneList">遺伝子のリスト</param>
    private void LoadData(string path,ref List<Gene> geneList)
    {
        using (StreamReader sr = new StreamReader(path))
        {
            string str = sr.ReadLine();
            string[] g = str.Split(new char[2]{ ' ',',' });
            this.generation = Int32.Parse(g[1]);

            List<List<int>> data = new List<List<int>>();

            CSVIO.LoadMap(ref data, sr.ReadToEnd(),this.ignoreItems);

            geneList = new List<Gene>(childNum);

            int index = 0;
            for (int i = 0; i < childNum; i++)
            {
                List<Learner.Param> paramList = new List<Learner.Param>();
                for (int j = 0; j < stateNum; j++)
                {
                    paramList.Add(MakeParam(data.GetRange(index,jointNum)));
                    index += jointNum;
                }
                geneList.Add(new Gene(paramList));
            }
            sr.Close();
        }
    }

    // JointNum個のベクトルデータからParamを作成
    private Learner.Param MakeParam(List<List<int>> lines)
    {
        Vector3[] vec = new Vector3[this.jointNum];
        for (int i = 0; i < this.jointNum; i++)
        {
            vec[i] = IntToVec(lines[i]);
        }
        return Learner.Param.CreateParam(vec);
    }

    private Vector3 IntToVec(List<int> i)
    {
        Vector3 vec = new Vector3(i[0], i[1], i[2]);
        //print(vec);
        return vec;
    }
    private List<Gene> SelectElite(List<Gene> geneList)
    {
        var elite = new List<Gene>();
        foreach(var i in Enumerable.Range(0, this.keepNum))
        {
            elite.Add(geneList[i]);
        }
        return elite;
    }

    // Rateを用いてルーレット選択する
    private Gene SelectParent(List<Gene> geneList) {
		float val = UnityEngine.Random.value;
		float totalRate = 0.0f;
		foreach (var gene in geneList) {
			totalRate += gene.rate;
			if (totalRate >= val) { 
				return gene;
			}
		}
		return geneList [geneList.Count - 1];
    }
    private List<Gene> SelectRandomParents(List<Gene> geneList)
    {
        var parents = new List<Gene>();
        int val1 = UnityEngine.Random.Range(0,geneList.Count);
        int val2 = UnityEngine.Random.Range(0, geneList.Count);
        val2 = (val2 != val1) ? val2 : UnityEngine.Random.Range(0, geneList.Count);
        parents.Add(geneList[val1]);parents.Add(geneList[val2]);
        return parents;
    }

    // 一点交叉によって子の遺伝子を作成
    private Gene OnePointCrossover(Gene dad, Gene mom) {
        Gene child = new Gene();
		child.paramList.Clear ();

		int dadLimit = UnityEngine.Random.Range (1, dad.paramList.Count);

        child.paramList = new List<Learner.Param> (
			dad.paramList.Skip (0).Take (dadLimit));
		child.paramList.AddRange (
			mom.paramList.Skip (dadLimit).Take (mom.paramList.Count - dadLimit));
		return child;
	}

    private Gene TwoPointCrossover(Gene dad, Gene mom)
    {
        Gene child = new Gene();
        child.paramList.Clear();

        int dadLimit = UnityEngine.Random.Range(1, dad.paramList.Count - 1);
        int momLimit = UnityEngine.Random.Range (1, mom.paramList.Count - dadLimit);

        child.paramList = new List<Learner.Param>(
            dad.paramList.Skip(0).Take(dadLimit));
        child.paramList.AddRange(
            mom.paramList.Skip(dadLimit).Take(momLimit));
        child.paramList.AddRange(
            dad.paramList.Skip(dadLimit + momLimit).Take(dad.paramList.Count - (dadLimit + momLimit)));

        //print(string.Format("dad {0} mom {1} childcnt {2}", dadLimit, momLimit, child.paramList.Count));
        return child;
    }

    private void Mutate(ref Gene gene) {
		foreach (var i in Enumerable.Range(0, gene.paramList.Count)) {
			if (UnityEngine.Random.value < mutateRate) {
				gene.paramList[i] = Learner.Param.CreateRandom(jointNum);
			}
		}
	}

    // 現世代の一番スコアが良い遺伝子をbestGenesに追加
    private void UpdateBestGene(Gene best)
    {
        Gene g = Gene.Clone(best);
        if (bestGenes.Count == 0)
        {
            bestGenes.Add(g);
            return;
        }
        foreach (int i in Enumerable.Range(0, this.bestGenes.Count))
        {
            if (this.bestGenes[i].point < g.point)
            {
                if (this.bestGenes.Count > 4) this.bestGenes.RemoveAt(4);
                this.bestGenes.Insert(i,g);
                return;
            }
        }
    }

    [System.Serializable]
	public class Gene {
		public List<Learner.Param> paramList;
		public float rate;
		public float point; // 評価値
        public int generation;

        public Gene()
        {
            paramList = new List<Learner.Param>();
        }

		public Gene(int paramNum,int joints) {
			paramList = new List<Learner.Param>();
			for (int j = 0; j < paramNum ; j++) {
				paramList.Add(Learner.Param.CreateRandom(joints));
			}
		}
        public Gene(List<Learner.Param> param)
        {
            paramList = param;
        }
        public string GetString()
        {
            string nl = Environment.NewLine;
            string str = string.Format("Generation : {0}Rate : {1}Point : {2}",
                this.generation + nl,this.rate + nl,this.point + nl);
            return str;
        }
        public static Gene Clone(Gene gene)
        {
            Gene g = new Gene(gene.paramList);
            g.point = gene.point;
            g.rate = gene.rate;
            g.generation = gene.generation;
            return g;
        }

	}

    public class Spawner
    {
        GameObject obj;

        Vector3 offset;// GameObjectを生成する間隔

        public Spawner(GameObject targetObj,Vector3 offset)
        {
            this.obj = targetObj;
            //this.objSize = obj.GetComponent<Renderer>().bounds.size;
            this.offset = offset;
        }

        public List<Learner> SpawnObjects(int num, Vector3 initPos)
        {
            Vector3 pos = Vector3.zero;
            List<Learner> li = new List<Learner>(num);
            for (int i = 0; i < num; i++)
            {
                pos = initPos;
                pos.x += offset.x * i;
                GameObject obj = Instantiate(this.obj, pos, Quaternion.identity);
                li.Add(obj.GetComponent<Learner>());
            }
            return li;
        }
        // 縦横の個数を指定して生成
        public List<Learner> SpawnObjects(int row,int col, Vector3 initPos)
        {
            Vector3 pos = Vector3.zero;
            List<Learner> li = new List<Learner>(row*col);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    pos = initPos;
                    pos.x += offset.x * j;
                    pos.z += offset.z * i;
                    GameObject obj = Instantiate(this.obj, pos, Quaternion.identity);
                    li.Add(obj.GetComponent<Learner>());
                }
            }
            return li;
        }
    }
}
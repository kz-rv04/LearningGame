using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using System;


public class GAController : MonoBehaviour {

    // 生成するゲームオブジェクト
    [SerializeField]
    GameObject learnerObj;

    private List<Learner> controller;

    
	List<Gene> geneList = new List<Gene>();
	bool isStart = false;

	IEnumerator<bool> player;

    // 学習に使用する個体の数
    int childNum = 20;
	int keepNum = 5;
	int extinctionNum = 5;
	float mutateRate = 0.01f;

    Spawner spawner;

	// Use this for initialization
	void Start () {
        spawner = new Spawner(this.learnerObj,new Vector3(30,0,30));
        this.controller = spawner.SpawnObjects(this.childNum, Vector3.zero);
        foreach (var i in Enumerable.Range(0, childNum)) {
			geneList.Add(new Gene());
		}
	}
	
	// Update is called once per frame
	void Update () {

	}
    /*
	IEnumerator<bool> GetPlayer(List<Gene> geneList) {
		int generation = 0;
		string logDirPath = string.Format ("Log/{0}", DateTime.Now.ToString ("yyyyMMdd_hhmmss"));

		while (true) {
			foreach (var gene in geneList) {
				controller.Play (gene.paramList);
				while (!controller.IsFinished) {
					yield return false;
				}

				gene.point = Mathf.Max(controller.getPoint (), 0.0f);
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

			foreach (var gene in geneList) {
				Debug.Log (string.Format ("{0}/{1}", gene.point, gene.rate));
			}
			Debug.Log (string.Format ("=== Gen {0} : Top {1} m ===", generation, geneList [0].point));
			//controller.setResult (generation, geneList);

			generation++;

			var newGeneList = new List<Gene> ();
			foreach (var i in Enumerable.Range(0, childNum)) {
				if (i < keepNum) {
					newGeneList.Add (geneList [i]);
				} else {
					var dad = SelectParent (geneList);
					var mom = SelectParent (geneList);
					var child = Mate (dad, mom);
					Mutate (ref child);
					newGeneList.Add (child);
				}
			}

			geneList = newGeneList;
		}
	}
    */

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

	private Gene Mate(Gene dad, Gene mom) {
		Gene child = new Gene ();
		child.paramList.Clear ();

		int dadLimit = UnityEngine.Random.Range (1, dad.paramList.Count);
		int momLimit = UnityEngine.Random.Range (0, mom.paramList.Count);
        /*
		child.paramList = new List<CarController.Param> (
			dad.paramList.Skip (0).Take (dadLimit));
		child.paramList.AddRange (
			mom.paramList.Skip (momLimit).Take (mom.paramList.Count - momLimit));
        */
		return child;
	}

	private void Mutate(ref Gene gene) {
		foreach (var i in Enumerable.Range(0, gene.paramList.Count)) {
			if (UnityEngine.Random.value < mutateRate) {
				//gene.paramList[i] = CarController.Param.CreateRandom();
			}
		}
	}

	public class Gene {
		public List<Learner.Param> paramList;
		public float rate;
		public float point;

		public Gene() {
			paramList = new List<Learner.Param>();
			for (int j = 0; j < UnityEngine.Random.Range(1, 11); j++) {
				paramList.Add(Learner.Param.CreateRandom());
			}
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIScript : MonoBehaviour
{
    GAController gc;

    [SerializeField]
    Color textColor = Color.red;
    void Start()
    {
        gc = GameObject.Find("GameController").GetComponent<GAController>();
    }

    void OnGUI()
    {
        GUI.contentColor = Color.black;

        GUI.Label(new Rect(50, 10, 500, 50), string.Format("Generation : {0}", gc.GetGeneration));
        GUI.Label(new Rect(100, 30, 500, 50), string.Format("Time : {0}", gc.timer.ToString()));

        GUI.contentColor = textColor;
        DisplayBestGenes(new Rect(Screen.width - 120, 0, 500, 500));
    }

    private void DisplayBestGenes (Rect initPos)
    {
        for(int i = 0;i < gc.GetBestGene.Count;i++)
        {
            var gene = gc.GetBestGene[i];
            GUI.Label(initPos,(i + 1) + " : \n" + gene.GetString());
            initPos.y += 60;
        }
    }
}

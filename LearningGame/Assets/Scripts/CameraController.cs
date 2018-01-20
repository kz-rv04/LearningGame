using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    Transform cameraTransform;

    Transform focused;

    [SerializeField]
    GameObject[] learners;

    [SerializeField]
    Vector3 offset;
    // Use this for initialization
    void Start() {
        cameraTransform = this.transform;
        StartCoroutine(this.TrackLearner());
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.P))
        {
            GetLearners();
        }
    }

    private void GetLearners()
    {
        this.learners = GameObject.FindGameObjectsWithTag("Player");
        if (learners.Length == 0) return;
        GameObject topLearner = learners[0];
        int top = 0;
        for (int i = 0; i < learners.Length; i++)
        {
            if (learners[i].transform.position.z > learners[top].transform.position.z)
                top = i;
        }
        print(string.Format("Focused Learner {0} Score {1}", top,learners[top].transform.position.z));
        //FocusTopLearner(topLearner.transform);

        this.focused = learners[top].transform;
    }

    IEnumerator TrackLearner()
    {
        for (;;)
        {
            if (this.focused != null)
            {
                FocusTopLearner(this.focused);
            }
            yield return null;
        }
    }

    private void FocusTopLearner(Transform pos)
    {
        Vector3 dest = pos.position + offset;
        cameraTransform.position = dest;
    }
}

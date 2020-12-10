using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    private Vector3 diff;
    // Start is called before the first frame update
    void Start()
    {
        diff = this.transform.position-target.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        this.transform.position = target.position+diff;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CubeFollower : MonoBehaviour
{
    public GameObject target;
    public UnityEngine.AI.NavMeshAgent nma;
    private Rigidbody rb;

    public float updatewait   =1.0f;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        nma.updatePosition=false;
        nma.updateRotation=false;
        updatewait+=Random.RandomRange(-0.1f,0.1f);
    }

public float forceamount=5.0f;
    private float t;
    public Vector3 dir=Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        t+=Time.deltaTime;
        if (t>updatewait){
            t=0;
            nma.Warp(this.transform.position);
            nma.SetDestination(target.transform.position);
        }

        
        dir = nma.steeringTarget-this.transform.position;
        dir.y=0;
        if (dir.magnitude<0.5){

        } else {
            dir.Normalize();
            rb.MovePosition(transform.position+dir*Time.deltaTime*forceamount);
        }
        
    }
}

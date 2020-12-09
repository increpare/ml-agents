using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallenderWurfel : MonoBehaviour
{
    private Vector3 spawnpos;
    public WalkerAgent agent;
    public float forcestrength=10;
    void OnCollisionEnter(Collision collision)
    {
        // if (collision.collider.tag =="ground"){
        //     Respawn();
        // }
    }

    void RandoForce(){
        var points = new Transform[]{agent.handL,agent.handR,agent.footL,agent.footR,agent.head,agent.armL,agent.armR,agent.thighL,agent.thighR,agent.shinL,agent.shinR,agent.forearmL,agent.forearmR,agent.chest};
        var target = points[Random.Range(0,points.Length)];
        var rb = target.GetComponent<Rigidbody>();
        var f = forcestrength*Random.onUnitSphere;
        rb.AddForce(f,ForceMode.Impulse);

    }


    private float time=0.0f;
    private float period=1.0f;
    // Update is called once per frame
    void Update()
    {
        time+=Time.deltaTime;
        if (time>period){
            time-=period;
            RandoForce();
            period = Random.Range(1.0f,3.0f);
        }
    }


    void Start()
    {
        RandoForce();
    }


}

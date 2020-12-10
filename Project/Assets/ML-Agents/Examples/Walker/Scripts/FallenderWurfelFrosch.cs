using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallenderWurfelFrosch : MonoBehaviour
{
    private Vector3 spawnpos;
    public FroschAgent agent;
    public float forcestrength=10;
    void OnCollisionEnter(Collision collision)
    {
        // if (collision.collider.tag =="ground"){
        //     Respawn();
        // }
    }

    void RandoForce(){
         var target = agent.rigidbodies[Random.Range(0,agent.rigidbodies.Count)];
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

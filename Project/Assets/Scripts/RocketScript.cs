using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketScript : MonoBehaviour
{
    public Transform spawnpoint;
    public Rigidbody rb;
    public Rigidbody fps;
    public float delayTilGravity=0.5f;

    public float initmovvel=10.0f;

    private float timer=0;
        void Start()
    {
        rb.isKinematic=true;
    }

    public float spawnvel=30;

    void Update()
    {
        timer+=Time.deltaTime;

        
        if (initmovvel*timer>6.5f)
        {
            // rb.useGravity=true;
            // rb.velocity=fps.velocity/2;
            rb.isKinematic=false;
            if ( this.transform.parent!=null){
                rb.AddForce(spawnvel*rb.transform.right,ForceMode.VelocityChange);
            }
            this.transform.parent=null;
        } else {
                    transform.localPosition = new Vector3(initmovvel*timer,0.0f,0.0f);
        }
    }

    public GameObject explosion;
    void OnCollisionEnter(Collision collision){
        Debug.Log("COLLISION "+collision.collider.name);
        var exp = Instantiate(explosion,collision.contacts[0].point,Quaternion.identity);
        Destroy(exp,2.0f);
        Destroy(this.gameObject);
    }
}

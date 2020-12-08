using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooty : MonoBehaviour
{
    public GameObject rocketPrefab;
    public Transform rocketSpawnPoint;
    public Renderer rocketspawnrenderer;
    public float cooldown=2;
    float cooldowntimer=2;
    // Start is called before the first frame update
    void Start()
    {
        rocketspawnrenderer.enabled=true;
    }

    // Update is called once per frame
    void Update()
    {
        if (cooldowntimer<cooldown){
            cooldowntimer+=Time.deltaTime;
            if (cooldowntimer>=cooldown){
                rocketspawnrenderer.enabled=true;
            }
        }
        if (Input.GetButtonDown("Fire1") && rocketspawnrenderer.isVisible){
            cooldowntimer=0;
            rocketspawnrenderer.enabled=false;
            cooldowntimer=0;
            var spawned = Instantiate(rocketPrefab,rocketSpawnPoint.position,rocketSpawnPoint.rotation);
            var rs = spawned.GetComponent<RocketScript>();
            rs.spawnpoint=rocketSpawnPoint;
            rs.fps = this.GetComponent<Rigidbody>();
            rs.transform.parent=rocketSpawnPoint;
            spawned.transform.localScale = rocketSpawnPoint.transform.localScale;

            spawned.GetComponent<Rigidbody>().velocity=GetComponent<Rigidbody>().velocity;
        }
    }
}

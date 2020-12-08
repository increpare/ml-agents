using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimLightOverTime : MonoBehaviour
{
    public Light glight;
    public float duration=4.0f;
    float initlight;
    // Start is called before the first frame update
    void Start()
    {
        initlight=glight.intensity;
    }

float t=0;
    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
        glight.intensity=Mathf.Max(0,initlight*(duration-t));
    }
}

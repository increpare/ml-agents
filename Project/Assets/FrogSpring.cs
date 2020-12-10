using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogSpring : MonoBehaviour
{
    public HingeJoint[] leggies;
    public HingeJoint[] feetsies;
    public HingeJoint[] toesicles;
    
    public HingeJoint[] faceLeggies;
    public HingeJoint[] faceToesicles;

    // Start is called before the first frame update
    void Start()
    {
        var rb = this.GetComponent<Rigidbody>();
        rb.centerOfMass = rb.centerOfMass + center_of_mass_offset;

        faceLeggies = faceLeggies[0].GetComponents<HingeJoint>();

        System.Array.Sort<HingeJoint>(faceLeggies, new System.Comparison<HingeJoint>( 
                  (hj1, hj2) => hj1.connectedBody.gameObject.name.CompareTo(hj2.connectedBody.gameObject.name))); 
    }

    public float leggie_jump=-66;
    public float footsie_jump=66;

    public float faceLeggie_jump=66;

    public float toesicle_jump=66;
    public float faceToesicle_jump=66;

    public float leggie_sit=0;
    public float footsie_sit=0;
    public float faceLeggie_sit=0;

    public float toesicle_sit=0;
    public float faceToesicle_sit=0;

public float forceStrength=100.0f;
public float forceDelay=0.3f;

public Vector3 center_of_mass_offset = new Vector3(0.5f,0.0f,0.0f);

    public void JumpForce(){
        var forcedir = (-transform.right+Vector3.up);
        GetComponent<Rigidbody>().AddForce(forceStrength*(forcedir),ForceMode.Impulse);
        Debug.DrawRay(transform.position,forcedir,new Color(0,1.0f,1.0f),10.0f);
    }

    float t = 0;
    public float walkspeed=1.0f;

    public float walk_forestep_up = 26.0f;
    public float walk_forestep_down = -26.0f;

    public float walk_backstep_thigh_smol = 0.0f;
    public float walk_backstep_thigh_big = 30.0f;

    public float walk_backstep_foreleg_smol = 0.0f;
    public float walk_backstep_foreleg_big = 30.0f;

    public float walk_backstep_toes_smol = 0.0f;
    public float walk_backstep_toes_big = 30.0f;


    private (float r0, float r1, float r2) legPhase(float phase){
        
        int phase_int = Mathf.FloorToInt(phase);
        float phase_progress = phase-(float)phase_int;
        float r0=0;
        float r1=0;
        float r2=0;

        switch(phase_int){
            case 0:
                r0=phase_progress;
                r1=1;
                r2=Mathf.Sqrt(1-phase_progress);
                break;
            case 1:
                r0=1;
                r1=1-phase_progress;
                r2=0;
                break;
            case 2:
                r0=1-phase_progress;
                r1=0;
                r2=0;
                break;
            case 3:
                r0=0;
                r1=phase_progress;
                r2=0;
                break;
            case 4:
                r0=0;
                r1=1;
                r2=phase_progress;
                break;
        }

        Debug.Log((phase_int,r0,r1,r2));
        
        r0 = Mathf.Lerp(walk_backstep_thigh_smol,walk_backstep_thigh_big,r0);
        r1 = Mathf.Lerp(walk_backstep_foreleg_smol,walk_backstep_foreleg_big,r1);
        r2 = Mathf.Lerp(walk_backstep_toes_smol,walk_backstep_toes_big,r2);

        return (r0,r1,r2);
    }

    // Update is called once per frame
    void Update()
    {
        t +=walkspeed*Time.deltaTime;

        if (t>=Mathf.PI*4){
            t=0;
        }        
        if (Input.GetButtonDown("Fire1")){
            Invoke("JumpForce",forceDelay);
            foreach (var leggie in leggies){
                JointSpring spring = leggie.spring;
                spring.targetPosition=leggie_jump;
                leggie.spring=spring;
            }
            foreach (var faceLeggie in faceLeggies){
                JointSpring spring = faceLeggie.spring;
                spring.targetPosition=faceLeggie_jump;
                faceLeggie.spring=spring;
            }
            foreach (var footsie in feetsies){
                JointSpring spring = footsie.spring;
                spring.targetPosition=footsie_jump;
                footsie.spring=spring;
            }
            foreach (var toesicle in toesicles){
                JointSpring spring = toesicle.spring;
                spring.targetPosition=toesicle_jump;
                toesicle.spring=spring;
            }
            foreach (var faceToesicle in faceToesicles){
                JointSpring spring = faceToesicle.spring;
                spring.targetPosition=faceToesicle_jump;
                faceToesicle.spring=spring;
            }
        }
        if (Input.GetButtonUp("Fire1")){
            foreach (var leggie in leggies){
                JointSpring spring = leggie.spring;
                spring.targetPosition=leggie_sit;
                leggie.spring=spring;
            }
            foreach (var faceLeggie in faceLeggies){
                JointSpring spring = faceLeggie.spring;
                spring.targetPosition=faceLeggie_sit;
                faceLeggie.spring=spring;
            }
            foreach (var footsie in feetsies){
                JointSpring spring = footsie.spring;
                spring.targetPosition=footsie_sit;
                footsie.spring=spring;
            }
            foreach (var toesicle in toesicles){
                JointSpring spring = toesicle.spring;
                spring.targetPosition=toesicle_sit;
                toesicle.spring=spring;
            }
            foreach (var faceToesicle in faceToesicles){
                JointSpring spring = faceToesicle.spring;
                spring.targetPosition=faceToesicle_sit;
                faceToesicle.spring=spring;
            }
        }

        if (!Input.GetButton("Fire1")){
            var s = (t+Mathf.PI)%(Mathf.PI*4);
            bool cycle1 = s<Mathf.PI*2;
            bool cycle2 =!cycle1;
            var v = Input.GetAxis("Vertical");
            
            JointSpring spring = faceLeggies[0].spring;
            spring.targetPosition= cycle1?walk_forestep_down:walk_forestep_up;
            faceLeggies[0].spring=spring;

            
            spring = faceLeggies[1].spring;
            spring.targetPosition= cycle2?walk_forestep_down:walk_forestep_up;
            faceLeggies[1].spring=spring;

            //5 phase step
            float phase = 5*t/(Mathf.PI*4);
            var (r0, r1, r2) = legPhase(phase);
        
            spring = leggies[0].spring;
            spring.targetPosition=r0;
            leggies[0].spring=spring;

            spring = feetsies[0].spring;
            spring.targetPosition=r1;
            feetsies[0].spring=spring;

            spring = toesicles[0].spring;
            spring.targetPosition=r2;
            toesicles[0].spring=spring;
            
            phase = (phase+2.5f)%5;
            (r0, r1, r2) = legPhase(phase);
        
            spring = leggies[1].spring;
            spring.targetPosition=r0;
            leggies[1].spring=spring;

            spring = feetsies[1].spring;
            spring.targetPosition=r1;
            feetsies[1].spring=spring;

            spring = toesicles[1].spring;
            spring.targetPosition=r2;
            toesicles[1].spring=spring;
        }
        
    }
}

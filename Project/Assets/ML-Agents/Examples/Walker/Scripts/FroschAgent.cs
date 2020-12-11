using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using BodyPart = Unity.MLAgentsExamples.BodyPart;
using Random = UnityEngine.Random;

public class FroschAgent : Agent
{

    [Header("Walk Speed")]
    [Range(0.1f, 30)]
    [SerializeField]
    //The walking speed to try and achieve
    private float m_TargetWalkingSpeed = 20;

    public float MTargetWalkingSpeed // property
    {
        get { return m_TargetWalkingSpeed; }
        set { m_TargetWalkingSpeed = Mathf.Clamp(value, .1f, m_maxWalkingSpeed); }
    }

    const float m_maxWalkingSpeed = 30; //The max walking speed

    //Should the agent sample a new goal velocity each episode?
    //If true, walkSpeed will be randomly set between zero and m_maxWalkingSpeed in OnEpisodeBegin() 
    //If false, the goal velocity will be walkingSpeed
    public bool randomizeWalkSpeedEachEpisode;

    //The direction an agent will walk during training.
    private Vector3 m_WorldDirToWalk = Vector3.right;

    [Header("Target To Walk Towards")] public Transform target; //Target the agent will walk towards during training.

    [Header("Body Parts")] public Transform hips;

    public Transform head;
    public Transform armL;
    public Transform armR;
    public Transform handL;
    public Transform fingerL1;
    public Transform fingerL2;
    public Transform fingerL3;
    public Transform handR;
    public Transform fingerR1;
    public Transform fingerR2;
    public Transform fingerR3;
    public Transform thighL;
    public Transform thighR;
    public Transform shinL;
    public Transform shinR;
    public Transform footL;
    public Transform toeL1;
    public Transform toeL2;
    public Transform toeL3;
    public Transform footR;
    public Transform toeR1;
    public Transform toeR2;
    public Transform toeR3;


    private List<Transform> bodyparts = new List<Transform>();
    public List<Rigidbody> rigidbodies = new List<Rigidbody>();
    private List<HingeJoint> hingeJoints = new List<HingeJoint>();

    private List<Vector3> orig_part_positions = new List<Vector3>();
    private List<Quaternion> orig_part_rotations = new List<Quaternion>();
    private List<GroundContact> groundContacts = new List<GroundContact>();

    public float jointForce=200;
    
    void RegisterBodyPart(Transform transform){
        bodyparts.Add(transform);
        rigidbodies.Add(transform.GetComponent<Rigidbody>());
        orig_part_positions.Add(transform.localPosition);
        orig_part_rotations.Add(transform.localRotation);

        var joints = transform.GetComponents<HingeJoint>();
        foreach(var joint in joints){
            var m = joint.motor;
            m.force=jointForce;
            m.targetVelocity=0;
            joint.motor=m;
        }
        hingeJoints.AddRange(joints);

        var gc = transform.GetComponent<GroundContact>();
        if (gc==null){
            gc = transform.gameObject.AddComponent<GroundContact>();
        }
        groundContacts.Add(gc);
    }


        public void Reset(int i)
        {
            var t = bodyparts[i];
            var orig_position=orig_part_positions[i];
            var orig_rotation=orig_part_rotations[i];

            t.localPosition = orig_part_positions[i];
            t.localRotation = orig_part_rotations[i];

            var rb = rigidbodies[i];
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            groundContacts[i].touchingGround = false;
            
        }

    //This will be used as a stabilized model space reference point for observations
    //Because ragdolls can move erratically during training, using a stabilized reference transform improves learning
    OrientationCubeController m_OrientationCube;

    //The indicator graphic gameobject that points towards the target
    DirectionIndicator m_DirectionIndicator;
    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        m_OrientationCube = GetComponentInChildren<OrientationCubeController>();
        m_DirectionIndicator = GetComponentInChildren<DirectionIndicator>();
        if (target==null){
            target = GetComponentInChildren<TargetController>().transform;
        }

        //Setup each body part
        RegisterBodyPart(hips);
        RegisterBodyPart(head);
        RegisterBodyPart(armL);
        RegisterBodyPart(armR);
        RegisterBodyPart(handL);
        RegisterBodyPart(fingerL1);
        RegisterBodyPart(fingerL2);
        RegisterBodyPart(fingerL3);
        RegisterBodyPart(handR);
        RegisterBodyPart(fingerR1);
        RegisterBodyPart(fingerR2);
        RegisterBodyPart(fingerR3);
        RegisterBodyPart(thighL);
        RegisterBodyPart(thighR);
        RegisterBodyPart(shinL);
        RegisterBodyPart(shinR);
        RegisterBodyPart(footL);
        RegisterBodyPart(toeL1);
        RegisterBodyPart(toeL2);
        RegisterBodyPart(toeL3);
        RegisterBodyPart(footR);
        RegisterBodyPart(toeR1);
        RegisterBodyPart(toeR2);
        RegisterBodyPart(toeR3);

        m_ResetParams = Academy.Instance.EnvironmentParameters;
    }

    /// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        //Reset all of the body parts
        for (var i=0;i<bodyparts.Count;i++){
            Reset(i);
        }
    
        transform.RotateAround(hips.position,Vector3.up,Random.Range(0.0f, 360.0f));
        
        UpdateOrientationObjects();

        //Set our goal walking speed
        MTargetWalkingSpeed =
            randomizeWalkSpeedEachEpisode ? Random.Range(0.1f, m_maxWalkingSpeed) : MTargetWalkingSpeed;


    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(int i, VectorSensor sensor)
    {
        var t = bodyparts[i];
        var rb = rigidbodies[i];
        
        //GROUND CHECK
        sensor.AddObservation(groundContacts[i].touchingGround); // Is this bp touching the ground
        
        //Get velocities in the context of our orientation cube's space
        //Note: You can get these velocities in world space as well but it may not train as well.
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(rb.velocity));
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(rb.angularVelocity));

        //Get position relative to hips in the context of our orientation cube's space
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(rb.position - hips.position));

        sensor.AddObservation(t.localRotation);
    }

    /// <summary>
    /// Loop over body parts to add them to observation.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        var cubeForward = m_OrientationCube.transform.forward;

        //velocity we want to match
        var velGoal = cubeForward * MTargetWalkingSpeed;
        //ragdoll's avg vel
        var avgVel = GetAvgVelocity();

        //current ragdoll velocity. normalized 
        sensor.AddObservation(Vector3.Distance(velGoal, avgVel));
        //avg body vel relative to cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(avgVel));
        //vel goal relative to cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformDirection(velGoal));

        //rotation deltas
        sensor.AddObservation(Quaternion.FromToRotation(hips.forward, cubeForward));
        sensor.AddObservation(Quaternion.FromToRotation(head.forward, cubeForward));

        //Position of target position relative to cube
        sensor.AddObservation(m_OrientationCube.transform.InverseTransformPoint(target.transform.position));
    
        sensor.AddObservation(head.position.y-hips.position.y);
        
        sensor.AddObservation((footL.position.y+footR.position.y)/2-hips.position.y);
        
        for (var i=0;i<bodyparts.Count;i++){
            CollectObservationBodyPart(i,sensor);
        }
        foreach (var hinge in hingeJoints){
            if (hinge.useLimits){
                sensor.AddObservation(hinge.motor.targetVelocity);
            }
        }
        
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;
        int j=0;
        for (var i=0;i<hingeJoints.Count;i++){
            var hj = hingeJoints[i];
            if (hj.useLimits){
                var clamped = Mathf.Clamp(continuousActions[j],-1,1);
                var m = hj.motor;
                m.targetVelocity =1000*clamped;
                hj.motor=m;
                j++;
            }
        }
    }

    //Update OrientationCube and DirectionIndicator
    void UpdateOrientationObjects()
    {
        m_WorldDirToWalk = target.position - hips.position;
        m_OrientationCube.UpdateOrientation(hips,target);
        if (m_DirectionIndicator)
        {
            m_DirectionIndicator.MatchOrientation(m_OrientationCube.transform);
        }
    }

    void FixedUpdate()
    {
        UpdateOrientationObjects();

    

        var cubeForward = m_OrientationCube.transform.forward;
        var matchSpeedReward = GetMatchingVelocityReward(cubeForward * MTargetWalkingSpeed, GetAvgVelocity());
        Debug.DrawRay(hips.transform.position,cubeForward,Color.yellow);
        Debug.DrawRay(hips.transform.position,-head.right,Color.cyan);
        var lookAtTargetReward = (Vector3.Dot(cubeForward, -head.right) + 1) * .5F;//-head.right is actually forward
        var reward = matchSpeedReward * lookAtTargetReward;

        AddReward(reward/3.0f);
    }

    //Returns the average velocity of all of the body parts
    //Using the velocity of the hips only has shown to result in more erratic movement from the limbs, so...
    //...using the average helps prevent this erratic movement
    Vector3 GetAvgVelocity()
    {
        Vector3 velSum = Vector3.zero;
        Vector3 avgVel = Vector3.zero;

        //ALL RBS
        int numOfRB = 0;
        foreach (var rb in rigidbodies)
        {
            numOfRB++;
            velSum += rb.velocity;
        }

        avgVel = velSum / numOfRB;
        return avgVel;
    }

    //normalized value of the difference in avg speed vs goal walking speed.
    public float GetMatchingVelocityReward(Vector3 velocityGoal, Vector3 actualVelocity)
    {
        //distance between our actual velocity and goal velocity
        var velDeltaMagnitude = Mathf.Clamp(Vector3.Distance(actualVelocity, velocityGoal), 0, MTargetWalkingSpeed);
        //return the value on a declining sigmoid shaped curve that decays from 1 to 0
        //This reward will approach 1 if it matches perfectly and approach zero as it deviates
        return Mathf.Pow(1 - Mathf.Pow(velDeltaMagnitude / MTargetWalkingSpeed, 2), 2);
    }

    /// <summary>
    /// Agent touched the target
    /// </summary>
    public void TouchedTarget()
    {
       // AddReward(1f);
    }

}

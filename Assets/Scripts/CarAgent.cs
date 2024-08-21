using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(DecisionRequester))]
public class CarAgent : Agent
{
    private Rigidbody rb = null;
    
    private Vector3 recall_position;
    private Quaternion recall_rotation;
    [SerializeField] private Transform spawnPosition;
    public CheckpointManager checkpointManager;
    
    public WheelCollider[] wheelsC;
    public Transform[] wheelsT;

    public float targetSteerAngle = 0;
    public float SteerSpeed = 100f;

    public float maxSteerAngle = 40;
    public float Angle;
    public float BrakeTorque = 1000000;
    //Brake bias
    public float BrakeDist = 0.3f;
    public Vector3 centerOfMass;
    public float KPH;
    public float WheelTorque;
    
    public float countdown;
    public bool timerIsOn = false;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 30; // FPS
    }
    public override void Initialize()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
        recall_position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        recall_rotation = new Quaternion(this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z, this.transform.rotation.w);
    }
    public override void OnEpisodeBegin()
    {
        this.transform.position = recall_position;
        this.transform.rotation = recall_rotation;
        
        // Respawn at the same place with zero moment
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = false;
        rb.detectCollisions = true;
                
        countdown = 30;
        timerIsOn = true;

        //Reset index of checkpoint
        checkpointManager.ResetCheckpoints();
    }
    void Update()
    {
        //Reset the episode after given time
        if (timerIsOn)
        {
            if (countdown > 0)
            {
                countdown -= Time.deltaTime;
            }
            else
            {
                timerIsOn = false;
                checkpointManager.ResetCheckpoints();
                EndEpisode();
            }
        }
    }
    public override void Heuristic(float[] actionsOut)
    {
        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");
        
        if (move < 0)
            actionsOut[0] = 0;
        else if (move == 0)
            actionsOut[0] = 1;
        else if (move > 0)
            actionsOut[0] = 2;
        
        if (turn < 0)
            actionsOut[1] = 0;
        else if (turn == 0)
            actionsOut[1] = 1;
        else if (turn > 0)
            actionsOut[1] = 2;
    }
    public override void OnActionReceived(float[] vectorAction)
    {   
        float lin = 0f;
        float rot = 0f;
        switch (vectorAction[0])
        {
            case 0: //back
                lin = -1f;
                break;
            case 1: //no action
                lin = 0f;
                break;
            case 2: //forward
                lin = +1f;
                break;
        }
        switch (vectorAction[1])
        {
            case 0: //left
                rot = -1f;
                break;
            case 1: //no action
                rot = 0f;
                break;
            case 2: //right
                rot = +1f;
                break;
        }
        GetInput(lin, rot);
    }
    public void OnCollisionEnter(Collision other)
    {
       if (other.gameObject.CompareTag("Wall"))
       {
           Debug.Log("Wall hit");
           AddReward(-2f);
           Debug.Log(GetCumulativeReward());

           rb.isKinematic = true;
           rb.detectCollisions = false;
           rb.velocity = Vector3.zero;
       }
    }
    private void UpdateWheelPose(WheelCollider _collider, Transform _transform)
    {
            Vector3 _pos;
            Quaternion _quat;
            _collider.GetWorldPose(out _pos, out _quat);
            _transform.position = _pos;
            _transform.rotation = _quat;
    }
    public void GetInput(float lin, float rot)
    {        
        for (int i=0; i < wheelsC.Length; i++) 
        {
            // Wheels animation
            UpdateWheelPose(wheelsC[i], wheelsT[i]); 

            // Rear Wheels
            if(i>=2)
            {   
                //Accelerating
                if (lin >0)
                {
                    wheelsC[i].motorTorque=WheelTorque*Time.deltaTime*30;
                    wheelsC[i].brakeTorque=0;
                }
                //Braking
                else if (lin<0)
                {
                    wheelsC[i].motorTorque=0;
                    wheelsC[i].brakeTorque=BrakeTorque * (1-BrakeDist)*Time.deltaTime*100;
                }
                //Coasting
                else
                {
                    wheelsC[i].motorTorque=0;
                    wheelsC[i].brakeTorque=0;
                }
            }
            // Front Wheels
            else
            {
                //Accelerating
                if (lin >0)
                {
                    wheelsC[i].brakeTorque=0;
                    Angle= maxSteerAngle;
                }
                //Braking
                else if (lin<0)
                {
                    wheelsC[i].brakeTorque=BrakeTorque * BrakeDist*Time.deltaTime*100;
                    Angle=20;
                }
                //Coasting
                else
                {
                    wheelsC[i].brakeTorque=0;
                    Angle=maxSteerAngle;
                }
                targetSteerAngle=Angle * rot;
                wheelsC[i].steerAngle=Mathf.Lerp(wheelsC[i].steerAngle, targetSteerAngle, 10 * Time.deltaTime);
            }
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {      
        KPH = rb.velocity.magnitude * 3.6f;
        //Limits top speed
        if (KPH<180)
        {
            WheelTorque = 12000; 
        }
        else
        {
            WheelTorque = 0;
        }
    }
}
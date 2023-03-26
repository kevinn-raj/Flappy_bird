using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class Jumper : Agent
{
    [SerializeField] private float jumpForce;
    [SerializeField] private string jumpKey;
    
    // Jump is available or not
    // private bool jumpIsReady = true;
    [SerializeField]
    private float JUMP_TIME_WINDOW = .01f; // in seconds // Cannot jump twice within that time
    private float lastJump; // Time of the last jump

    public float gravity_multiplier = 1f;

    private float XMAX = 6;
    private float YMAX = 6;
    private float ZMAX = 6;

    private Rigidbody rBody;
    private Vector3 startingPosition;
    [SerializeField]
    private int score = 0;
    [SerializeField]
    private  int maxScore = 0;

    public GameObject ground;
    public GameObject roof;
    public GameObject Pipe_spawner;       // Fetch the Pipe_Spawner object
    // public List<GameObject> pipes;

    // To store the next target  (the score triggers), the element will be replaced by the next score trigger,
    // whenever the player hits its trigger
    [SerializeField]
    // private GameObject target=null; 
    private GameObject[] targets = new GameObject[2];
    

    public bool useObs = true;



    public event Action OnReset;

    EnvironmentParameters m_ResetParams;

    [Header("Debugs")]
    [SerializeField]
    private bool isDebug = true;
    

    public override void Initialize(){
        rBody = GetComponent<Rigidbody>();
        startingPosition = transform.position;
        lastJump = Time.time;

        m_ResetParams = Academy.Instance.EnvironmentParameters;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if(useObs){
            // fetch the max Y of the ground
            float groundMaxY = ground.GetComponent<Collider>().bounds.max.y;
            // Set the vertical distance from the ground as observation
            sensor.AddObservation(Mathf.Abs(groundMaxY - transform.position.y));

            // fetch the min Y of the roof
            float roofMinY = roof.GetComponent<Collider>().bounds.min.y;
            //Set the vertical distance from the roof as observation
            sensor.AddObservation(Mathf.Abs(roofMinY - transform.position.y));

        // Add each target as observation    
        foreach(GameObject target in targets){    
        // Get the scoring Triggers
            //// If the target is not null
            if(target){
                /* Observations */
                var scoreColl = target.GetComponent<BoxCollider>();
                // x coordinate of The Left 
                sensor.AddObservation(scoreColl.bounds.min.x - transform.position.x);
                // y coordinate of the Bottom
                sensor.AddObservation(scoreColl.bounds.min.y - transform.position.y);
                // y coordinate of the Top
                sensor.AddObservation(scoreColl.bounds.max.y - transform.position.y);

            }
            else{
            // If no pipe is spawned yet, then suppose the pipe is far away
            // The number of observation must always be the same
                // x coordinate of The Left      
                sensor.AddObservation(2);
                // y coordinate of the Bottom
                sensor.AddObservation(-2);
                // y coordinate of the Top
                sensor.AddObservation(2);
                 }
         }
             
        // velocity Y
        sensor.AddObservation(rBody.velocity.y);
        }
    }

    // Get the gameobject of the scoring collider
    public GameObject GetScoringZone(GameObject parentPipe){
        return parentPipe.transform.Find("Scoring Zone").gameObject;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers){
        var act = actionBuffers.DiscreteActions;

        // Action 0 = 0 : Nothing, 1 : Jump
        if(act[0] == 1){
            Jump();
        }


    }
 
    //  Controlled by the user
    public override void Heuristic(in ActionBuffers actionsOut){

        // Get the discrete action
        var act = actionsOut.DiscreteActions;

        // Action = 0 : Nothing, 1 : Jump
        act[0] = Convert.ToInt16(Input.GetButton("Jump"));

    }

    private void Jump()
    {
        if (Time.time > JUMP_TIME_WINDOW + lastJump)
        {
            // velocity to zero before jump
            rBody.velocity = Vector3.zero;
            rBody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);

            lastJump = Time.time;
        }
    }

    public override void OnEpisodeBegin(){
        // Reset the Pipe spawner GameObject
        Pipe_spawner.SetActive(true);
        // Reset
        Reset();

    }

    public void Update(){
        // Constraints
        float x = Mathf.Clamp(transform.localPosition.x, -XMAX, XMAX);
        float y = Mathf.Clamp(transform.localPosition.y, -YMAX, YMAX);
        float z = Mathf.Clamp(transform.localPosition.z, -ZMAX, ZMAX);

        transform.localPosition = new Vector3(x, y, z);


    }

    public void FixedUpdate(){
        
        // Add custom gravity force 
        rBody.AddForce(Physics.gravity * gravity_multiplier);

    }
    public void LateUpdate(){
         /* Update the target */
        List<GameObject> pipes = Pipe_spawner.GetComponent<Pipe_Spawner>().pipes_spawned;        
        if(pipes.Count != 0) // The pipes is not empty
        {    
            for(int i=0; i<pipes.Count; i++){
                // Distance from the player
                float pos = pipes[i].transform.position.x;

                // The first pipe in front of the player
                if(pos - transform.position.x >= 0 ){
                    // target = GetScoringZone(pipes[i]);
                    targets[0] = GetScoringZone(pipes[i]); // 1st target
                    // 2nd target
                    if(i+1 < pipes.Count) targets[1] = GetScoringZone(pipes[i+1]);
                    else targets[1] = null;
                    break;  // Forget the remaining elements after finding the right one
                 }     
            }
        }
        else{ // If it is empty
            // target = null; 
            for(int i=0; i<targets.Length; i++) 
                targets[i] = null;
        }

       /* Debugs */
        if(isDebug){
            // For the targets
            foreach(GameObject target in targets){
                if(target != null){ // Rays from player to the score target
                    Debug.DrawLine(transform.position, target.transform.position, Color.red);
                    var bounds = target.GetComponent<BoxCollider>().bounds;
                    Debug.DrawLine(transform.position,
                                    bounds.min,
                                      Color.red);
                    Debug.DrawLine(transform.position,
                                    new Vector3(bounds.min.x, bounds.max.y, 0),
                                      Color.red);
                    }   
            }

        // For the roof and the ground    
        // fetch the max Y of the ground
        float groundMaxY = ground.GetComponent<Collider>().bounds.max.y;
       Debug.DrawLine(transform.position,
                 new Vector3(transform.position.x, groundMaxY, transform.position.z));

        // fetch the min Y of the roof
        float roofMinY = roof.GetComponent<Collider>().bounds.min.y;
         Debug.DrawLine(transform.position,
                 new Vector3(transform.position.x, roofMinY, transform.position.z), Color.blue);
        }   
    }

    private void Reset()
    {
        score = 0;
        
        //Reset Movement and Position
        transform.position = startingPosition;
        rBody.velocity = Vector3.zero;
        
        OnReset?.Invoke();
    }

    private void OnCollisionEnter(Collision collidedObj)
    {  
    }

    private void OnTriggerEnter(Collider collidedObj)
    {      
        // Loooooose
        if (collidedObj.gameObject.CompareTag("Obstacle"))
            {
                AddReward(-5);
                // Debug.Log(GetCumulativeReward());
                // Reset the spawner, delete all the instances
                Pipe_spawner.GetComponent<Pipe_Spawner>().Reset();
                Pipe_spawner.SetActive(false); // Disable the Pipe spawner GameObject
                EndEpisode();
            }
        // SCOREEEEEEEEEEEEE!
        else if (collidedObj.gameObject.CompareTag("Scoring"))
        {
            score++;
            maxScore = Mathf.Max(score, maxScore);
            // ScoreCollector.Instance.AddScore(score);
            AddReward(1);

            if(score == 3) {
                AddReward(1);
            }
            else if(score == 5) {
                AddReward(2);
            }
            // Goal reached
            else if(score == 10) {
                AddReward(10);
                EndEpisode();
            }
        }
    }
}

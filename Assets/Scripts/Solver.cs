using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class Solver : Agent
{
    public bool isTraining = true;
    [SerializeField] private float jumpForce=1.5f;
    [SerializeField] private string jumpKey;
    
    // Jump is available or not
    // private bool jumpIsReady = true;
    [SerializeField]
    private float JUMP_TIME_WINDOW = .1f; // in seconds // Cannot jump twice within that time
    private float lastJump; // Time of the last jump

    public float gravity_multiplier = .6f;

    private float XMAX = 6;
    private float YMAX = 6;
    private float ZMAX = 6;

    public Rigidbody rBody;
    private Vector3 startingPosition;
    [SerializeField]
    public int score = 0;
    [SerializeField]
    public  int maxScore = 0;

    public GameObject ground;
    public GameObject roof;
    public GameObject Generator;       // Fetch the Generator object

    // To store the next target  (the score triggers), the element will be replaced by the next score trigger,
    // whenever the player hits its trigger
    [SerializeField]
    [Range(1,2)]
    private int target_numbers = 2; // Number of target the agent can see
    [SerializeField]
    // private GameObject target=null; 
    private GameObject[] targets = new GameObject[2]; // this target array has always 2 elements
    

    public bool useObs = true;


    public event Action OnReset;

    EnvironmentParameters m_ResetParams;

    [Header("Debugs")]
    [SerializeField]
    private bool isDebug = true;
    

    public override void Initialize(){
        rBody = GetComponent<Rigidbody>();
        startingPosition = transform.localPosition;
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
            // rBody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.VelocityChange);
            rBody.velocity = new Vector3(0, jumpForce, 0);

            lastJump = Time.time;
        }
    }

    public override void OnEpisodeBegin(){
        // Reset
        Reset();
        // Random
    }

    public void Update(){
        // Constraints on local coordinate
        float x = Mathf.Clamp(transform.localPosition.x, -XMAX, XMAX);
        float y = Mathf.Clamp(transform.localPosition.y, -YMAX, YMAX);
        float z = Mathf.Clamp(transform.localPosition.z, -ZMAX, ZMAX);

        transform.localPosition = new Vector3(x, y, z);
    }

    public void FixedUpdate(){
        
        // Add custom gravity force 
        rBody.AddForce(Physics.gravity * gravity_multiplier);

        AddReward(0.0001f); // To motivate to fly

    }
    public void LateUpdate(){
        
         /* Update the target */
        List<GameObject> pipes = Generator.GetComponent<Generator>().Obstacles_lst;        
 

       /* Debugs */
        if(isDebug){

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
        transform.localPosition = new Vector3(startingPosition.x, Random.Range(-4.8f, 4.26f) , startingPosition.z);
        rBody.velocity = Vector3.zero;
        
        OnReset?.Invoke();
    }
    private void RewardTheGen(float solverReward){
        // Reward the generator
            if(Generator){
            float aux = Generator.GetComponent<Generator>().aux_input;
            float weight = 1.0f;
            float generator_reward = weight * solverReward * aux;
            // End the episode of the Generator and the solver
            Generator.GetComponent<Generator>().AddReward(generator_reward);
            }
    }

    private void OnCollisionEnter(Collision collidedObj)
    {  
    }

    private void OnTriggerEnter(Collider collidedObj)
    {      
        // Loooooose
        if (collidedObj.gameObject.CompareTag("Obstacle"))
            {
                float CollideReward = -1f;
                AddReward(CollideReward);
                RewardTheGen(CollideReward);
                EndEpisode();
                Generator.GetComponent<Generator>().EndEpisode();
            }
        // else if (collidedObj.gameObject.CompareTag("Limit"))
        // {
        //     AddReward(-0.001f); // To motivate not to hit the roof

        // }
        // SCOREEEEEEEEEEEEE!
        else if (collidedObj.gameObject.CompareTag("Scoring"))
        {
            score++;
            maxScore = Mathf.Max(score, maxScore);
            // log the scores into TensorBoard
            var statsRecorder = Academy.Instance.StatsRecorder;
            statsRecorder.Add("Score", score);
            // Reward the score
            float reward = 0.1f;
            AddReward(reward);
            RewardTheGen(reward);

            if(Generator){
            Generator.GetComponent<Generator>().latestAchieved = true; // let the next obstacle to be created
            Generator.GetComponent<Generator>().CreateWithAgent();
                    }
            // Goal reached
            if(score >= Generator.GetComponent<Generator>().n_obstacles){
                // only end the episode on training
                if(isTraining){
                    EndEpisode();
                Generator.GetComponent<Generator>().EndEpisode();
                }
            }
        }
    }
}

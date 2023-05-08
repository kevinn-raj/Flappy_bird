using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class Solver_PCG : Agent
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
    private const float minVelocity=-50f;
    private const float maxVelocity=50f;
    private Vector3 startingPosition;
    [SerializeField]
    public int score = 0;
    [SerializeField]
    public  int maxScore = 0;

    public GameObject ground;
    public GameObject roof;
    public RuledPCG gen;       // Fetch the Generator object

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
    
    public float normalize01(float actual, float minimum, float maximum){
        return (actual - minimum)/(maximum - minimum);}
        // -1 to 1
    public float normalize(float actual, float minimum, float maximum){
        return (2*actual - minimum - maximum)/(maximum - minimum);

    }
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
        // fetch the min Y of the roof
        float roofMinY = roof.GetComponent<Collider>().bounds.min.y;

        float obs = normalize(groundMaxY - transform.position.y, groundMaxY, roofMinY); //normalized for stability
        // Set the vertical distance from the ground as observation
        sensor.AddObservation(obs);

        obs = normalize(roofMinY - transform.position.y, groundMaxY, roofMinY);  //normalized for stability
        //Set the vertical distance from the roof as observation
        sensor.AddObservation(obs);

        // velocity Y
        obs = normalize(rBody.velocity.y, minVelocity, maxVelocity);
        sensor.AddObservation(obs);
        }
    }


    public override void OnActionReceived(ActionBuffers actionBuffers){
        var act = actionBuffers.DiscreteActions;
        // Action 0 = 0 : Nothing, 1 : Jump
        if(act[0] == 1){
            Jump();
            //Vector2.SmoothDamp();
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
        // Reset the Generator as well
        gen.Reset();
        // Request decision if without Decision requester
        gen.CreateRandom();
    }
    public void Start()
    {
        try{
        gen = GameObject.Find("RuledPCG").GetComponent<RuledPCG>();
        }catch(Exception e) { }
    }
    public void Update(){

    }

    public void FixedUpdate(){
        
        // Add custom gravity force 
        rBody.AddForce(Physics.gravity * gravity_multiplier);

        float reward = 0.01f;
        AddReward(reward); // To motivate to stay alive

    }

    private void Reset()
    {
        score = 0;
        float groundMaxY = ground.GetComponent<Collider>().bounds.max.y;
        float roofMinY = roof.GetComponent<Collider>().bounds.min.y;
        //Reset Movement and Position
        transform.localPosition = new Vector3(startingPosition.x, Random.Range(groundMaxY, roofMinY) , startingPosition.z);
        rBody.velocity = Vector3.zero;
        
        OnReset?.Invoke();
    }


    private void OnCollisionEnter(Collision collidedObj)
    {  
    }

    private void OnTriggerEnter(Collider collidedObj)
    {      
        // Loooooose 
        if (collidedObj.gameObject.CompareTag("Ground") || 
            collidedObj.gameObject.CompareTag("Obstacle_top") ||
            collidedObj.gameObject.CompareTag("Obstacle_bottom"))
            {
                float CollideReward = -1f;
                AddReward(CollideReward);
            EndEpisode();
            try{
                gen.Reset();
            }
            catch (Exception e) { }
        } 
        // SCOREEEEEEEEEEEEE!
        else if (collidedObj.gameObject.CompareTag("Scoring"))
        {
            score++;
            // log the scores into TensorBoard
            var statsRecorder = Academy.Instance.StatsRecorder;
            statsRecorder.Add("Score", score);
            maxScore = Mathf.Max(score, maxScore);
            try
            {
                // Goal reached
                if (score >= gen.n_obstacles)
                {
                    // only end the episode on training
                    if (isTraining)
                    { // For reaching the goal
                        float reward_goal = 1f;
                        AddReward(reward_goal);
                        EndEpisode();
                        gen.Reset();
                    }
                }
            }
            catch (Exception e) { }
        }
        else if (collidedObj.gameObject.CompareTag("Limit"))
        {
        float punishment = -.5f;
            AddReward(punishment);  // Prevent to stay on the roof
        }
    }

    private void OnTriggerStay(Collider collidedObj)
    {
        if (collidedObj.gameObject.CompareTag("Limit"))
        {
            float punishment = -.5f;
            AddReward(punishment);  // Prevent to stay on the roof
        }
    }
}

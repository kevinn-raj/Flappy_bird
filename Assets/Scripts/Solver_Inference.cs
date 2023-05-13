using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.IO;
using System.Globalization; // For float string conversion


public class Solver_Inference : Agent
{
    public bool isTraining = true;
    [SerializeField] private float jumpForce=5.5f;
    [SerializeField] private string jumpKey;
    
    // Jump is available or not
    // private bool jumpIsReady = true;
    [SerializeField]
    private float JUMP_TIME_WINDOW = .001f; // in seconds // Cannot jump twice within that time
    private float lastJump; // Time of the last jump

    public float gravity_multiplier = .7f;

    public Rigidbody rBody;
    private const float minVelocity=-50f;
    private const float maxVelocity=50f;
    private Vector3 startingPosition;
    [SerializeField]
    public int score = 0;
    [SerializeField]
    public  int maxScore = 0;
    public string csv_path; // Where the csv should be save
    static StreamWriter writer; // for the csv export
    public List<int> scores;

    public GameObject ground;
    public GameObject roof;
    public SpawnLevels gen;       // Fetch the Generator object
    

    public bool useObs = true;


    public UnityEvent OnReset;

    EnvironmentParameters m_ResetParams;
       
    
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
    }

    public void Awake()
    {
        
        scores = new List<int>();
    }


    public void Start()
    {
        if(gen == null)
            Debug.LogError("Spawner not assigned");

        // #########################################################################
        // IMPORTANT to properly convert float to string with "." as decimal not ","
        System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        // ######################################################################### 
    }
    public void Update(){

    }

    public void FixedUpdate(){
        
        // Add custom gravity force 
        rBody.AddForce(Physics.gravity * gravity_multiplier);

        float reward = 0.01f;
        AddReward(reward); // To motivate to stay alive
    }
    void OnApplicationQuit()
    {
        string filename_prefix = csv_path + "/" + GetAuxInput().ToString() + "-";
        int suffix = 0; //pick a number for suffix
        while (File.Exists(filename_prefix + suffix.ToString() + ".csv"))
            suffix++;
        writer = new StreamWriter(filename_prefix + suffix.ToString() + ".csv", true); // Append in case it still exists regardless of the previous condition, hence the keyword true
        foreach (int i in scores)
        {
            writer.WriteLine(i);
        }
        writer.Close();
    }

    private void Reset()
    {
        // Register the score then reset it
        RegisterScore();
        score = 0;
        float groundMaxY = ground.GetComponent<Collider>().bounds.max.y;
        float roofMinY = roof.GetComponent<Collider>().bounds.min.y;
        //Reset Movement and Position
        transform.localPosition = new Vector3(startingPosition.x, Random.Range(groundMaxY, roofMinY) , startingPosition.z);
        rBody.velocity = Vector3.zero;
        
        OnReset.Invoke();
    }
    public void StopAcademy(){
        Academy.Instance.Dispose(); // Stop the academy
    }
    public float GetAuxInput(){
        float aux_input = Academy.Instance.EnvironmentParameters.GetWithDefault("aux_input", 1f);
        return aux_input;
    }
    public int GetEpisodeCount()
    {
        return Academy.Instance.EpisodeCount;
    }
 
    private void RegisterScore()
    {
        // log the scores into TensorBoard
        var statsRecorder = Academy.Instance.StatsRecorder;
        // Record the most recent Score
        statsRecorder.Add("Score", score, StatAggregationMethod.Average);
        scores.Add(score);
        // Record the episode count
        statsRecorder.Add("Episode", Academy.Instance.EpisodeCount, StatAggregationMethod.MostRecent);
    }

    private void OnTriggerEnter(Collider collidedObj)
    {      
        // Loooooose 
        if (collidedObj.gameObject.CompareTag("Ground") || 
            collidedObj.gameObject.CompareTag("Obstacle_top") ||
            collidedObj.gameObject.CompareTag("Obstacle_bottom")){
            EndEpisode();
        } 
        // SCOREEEEEEEEEEEEE!
        else if (collidedObj.gameObject.CompareTag("Scoring"))
        {
            score++;
            maxScore = Mathf.Max(score, maxScore);
            try{// Goal reached
                if (score >= gen.n_obstacles){  // only end the episode on training
                    if (isTraining)
                        EndEpisode();
                }
            }catch (Exception e) { }
            
        }
    }

}

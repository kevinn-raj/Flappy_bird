using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class Generator : Agent
{
    public Solver solver;
    [Header("Auxiliary Input")]
    public bool randomizeAuxInput = true;
    [Range(-1f,1f)]    public float aux_input=1f;
    private const int nAuxInputs = 5;
    private float[] auxInputs = new float[nAuxInputs]{-1f, -.5f, 0f, .5f, 1f};

    private bool isRandom=false; // randomize on one episode or not
    [Header("Generator actions")]
    // Hole height
    //  minimum 1.5, Always positive
    private float height_m; //mean
    private float height_std; //standard deviation
    private float height_min=2f;
    private float height_max=6f;
    private float height_std_scale = 4.5f; // std = [0, 10]

    // Horizontal distance between consecutive holes
    // minimum 1.12, Always Positive
    private float h_distance_m; //mean; 
    private float h_distance_std; //std
    private const float h_distance_min=1.12f;
    private float h_distance_max=4f;
    private float h_distance_std_scale = 5f; // std = [0, 10]

    // Vertical difference between consecutive holes
    // The next minus previous
    // Can be negative
    private float v_difference_m; //mean
    private float v_difference_std; //std
    private float v_difference_min;
    private float v_difference_max;
    private float v_difference_std_scale = 4f; // std = [0, 10]

    // Actions variable
    [SerializeField]    private float nextTopY;
    [SerializeField]    private float nextBottomY;
    [SerializeField]    private float nextHDistance=h_distance_min;
    [SerializeField]    private float nextHeight;

    // obstacle speed, constant for an episode an same for every obstacle
    // Positive
    private float obst_speed = 3f;
    private float obst_speed_min = .1f;
    private float obst_speed_max = 6f;

    /* Observations */
    // Previous obstacle's position
    private List<float> previous = new List<float>();
    private GameObject prevPipe;
    private GameObject pipe;


    [Header("Parameters")]
    [Min(0)]    public int n_obstacles = 10;
    public GameObject prefab;

    /* Limits and Constraints */
    float top_maxy = 4.8f;
    float bottom_maxy;
    float bottom_miny = -4.5f;
    float top_miny;

    public bool latestAchieved=true;
    [HideInInspector] public bool isHeuristic;
    [Header("Heuristic Parameters")]
    [Range(1.12f,4f)] public float heur_hdist_min=1.12f;
    [Range(1.12f,4f)] public float heur_hdist_max=4f;
    [Range(2f,5f)] public float heur_height_min=2f;
    [Range(2f,5f)] public float heur_height_max=5f;


    [HideInInspector]
    public List<GameObject> Obstacles_lst = new List<GameObject>();

    private int counter=0; // counter for the obstacle

    private float GetRandom(float mean, float std){
        // mean = (max + min)/2
        // std = (max - min)/2
        float max_n = mean + std;
        float min_n = mean - std;
        return Random.Range(min_n, max_n);
    }

    // Return randomly -1 or +1
    private float GetRandomSign(){
        return Mathf.Sign(Random.Range(-1, 1));
    }

    void Awake(){
        // Initialize variables
        bottom_maxy = top_maxy - height_min;
        top_miny = bottom_miny + height_min;

        v_difference_min = -height_max;
        v_difference_max = height_max;

        // initialize it
        previous = new List<float>(){transform.position.y - top_maxy, transform.position.y - bottom_miny, 0};
        prevPipe = gameObject;

        latestAchieved = true; // otherwise no obstacle will be generated

        // initialize the solver variable
        solver = transform.parent.GetComponentInChildren<Solver>();

        isHeuristic = gameObject.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BehaviorType == Unity.MLAgents.Policies.BehaviorType.HeuristicOnly; /*Check if Heuristic or not*/
    }

    public override void OnEpisodeBegin(){
        //  Delete previous created objects before creating new ones
        foreach(GameObject o in Obstacles_lst){
           Destroy(o.gameObject);
           // Debug.Log("Destroying");
        }
        Obstacles_lst = new List<GameObject>();

        counter = 0;

        // RequestDecision();

        // To store the position of the previous obstacle, relative to the generator transform
        previous = new List<float>(){transform.position.y - top_maxy, transform.position.y - bottom_miny, 0};
        prevPipe = gameObject;
        pipe = gameObject;
        
        // randomize the aux input, choose one of the values
        /*if(randomizeAuxInput) aux_input = auxInputs[Random.Range(0, nAuxInputs)];*/

        // aux_input as environment parameters // Curriculum
        aux_input = Academy.Instance.EnvironmentParameters.GetWithDefault("aux_input", 0.0f);
        
        // Set the first hdistance
        // nextHDistance=Random.Range(h_distance_min, h_distance_max);
        RequestDecision();
        latestAchieved = true;

        CreateWithAgent();
        
    }
    public override void CollectObservations(VectorSensor sensor){
        sensor.AddObservation(previous[0]); // top y
        sensor.AddObservation(previous[1]); // bottom y
        sensor.AddObservation(previous[2]); // top or bottom or parent x
        sensor.AddObservation(aux_input); // auxiliary input
    }

    public override void Heuristic(in ActionBuffers actionsOut){
        Transform top, bottom;
        Vector3 initPos = transform.position;

        for(int j=0; j<n_obstacles && counter<n_obstacles; j++){
        float randHeight = Random.Range(heur_height_min, heur_height_max);
        float yposTop = Random.Range(top_maxy, top_miny+randHeight);//local
        // float randHDistance = Random.Range(h_distance_min, h_distance_max);
        float randHDistance = Random.Range(heur_hdist_min, heur_hdist_max);
        
        GameObject p = Instantiate(prefab, initPos, Quaternion.identity);
        p.GetComponent<Obstacles>().speed = obst_speed;
        p.transform.position += new Vector3(randHDistance, 0, 0);

        top = p.transform.Find("Top Pipe");
        bottom = p.transform.Find("Bottom Pipe");    

        // Position the top and bottom pipes, absolute positions
        top.position += new Vector3(0, yposTop, 0);
        bottom.position = new Vector3(bottom.position.x, top.position.y-randHeight , bottom.position.z);

        // Add the created obstacle to the list of all generated obstacles
        Obstacles_lst.Add(p);
        initPos = p.transform.position;
        counter++;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers){
        var act = actionBuffers.ContinuousActions;
        float randness = .1f; // Add some randomness
        /* Actions - And internal rewards*/
        // y position of the next top pipe, relative position
        nextTopY = ScaleAction(act[0], top_miny, top_maxy) - Random.Range(0f, randness);

        nextHeight = ScaleAction(act[1], height_min, height_max) + Random.Range(0f, randness);
        // y position of the next bottom pipe, relative position
        nextBottomY = nextTopY - nextHeight;

        // Vertical difference between consecutive holes, relative position
        nextHDistance = ScaleAction(act[2], h_distance_min, h_distance_max) + Random.Range(0f, randness);
    }

    void FixedUpdate(){
    }


    // Generate with the Generator Agent
    public void CreateWithAgent(){
        if(!isHeuristic){ /*Execute only in Inference mode*/
        float valid_reward = .1f;
        float punishment = -.05f;

        /*Spawn only if it is less than a certain number and the solver has achieved the previous one*/
        if(counter <= n_obstacles && latestAchieved){
            Transform top, bottom;
            Vector3 initPos;

            if(counter ==0){ // place randomly the first one
                float randHeight = Random.Range(height_min, height_max);
                float yposTop = Random.Range(top_maxy, top_miny+randHeight);
                float yposBot = yposTop-randHeight;
                initPos = transform.position;

                pipe = Instantiate(prefab, initPos, Quaternion.identity);
                pipe.GetComponent<Obstacles>().speed = obst_speed;
                top = pipe.transform.Find("Top Pipe");
                bottom = pipe.transform.Find("Bottom Pipe");    

                // Position the top and bottom pipes, absolute positions
                top.position = new Vector3(top.position.x, yposTop + transform.position.y, top.position.z);
                bottom.position = new Vector3(bottom.position.x, yposBot + transform.position.y, bottom.position.z);
            }
            else{
            initPos = new Vector3(prevPipe.transform.position.x + nextHDistance, transform.position.y, transform.position.z);
                    // Instantiate the obstacle in the same position as previous
                    pipe = Instantiate(prefab, initPos, Quaternion.identity);
                //Debug.Log(pipe);
        
                    //  Debug
                    // Debug.DrawLine(transform.position, previous);
        
                    // Obstacle speed
                    pipe.GetComponent<Obstacles>().speed = obst_speed;
        
                    /* Constraints - Rewarding the Generator*/
                    // Internal Reward
                    top = pipe.transform.Find("Top Pipe");
                    bottom = pipe.transform.Find("Bottom Pipe");
        
                    // If it is not the first one
                    // if(counter != 0)
        
                    // Position the top and bottom pipes, absolute positions
                    top.position = new Vector3(top.position.x, nextTopY + transform.position.y, top.position.z);
                    bottom.position = new Vector3(bottom.position.x, nextBottomY + transform.position.y, bottom.position.z);
            }
            // Request decision after the changes
                RequestDecision(); 
                    
             // Add the created obstacle to the list of all generated obstacles
            Obstacles_lst.Add(pipe);
    
            // REWARD- The bottom one must be above the lower limit
            if(transform.position.y - bottom.position.y  >= bottom_miny){
                // + reward
                AddReward(valid_reward);
                // Debug.Log("Valid");
            }else{  
                AddReward(punishment); // - reward
                // Reset the episode after mistakes or not
                // Comment the following line if not
               // EndEpisode();
            }
                                   
        // Reassign the variable to the actual obstacle
            // Relative position to the generator transform
            previous[0] = transform.position.y - top.position.y;
            previous[1] = transform.position.y - bottom.position.y;
            previous[2] =  transform.position.x - top.transform.position.x;
            
            counter++; // increment the counter
            //Debug.Log(counter);
            latestAchieved = false;
            prevPipe = pipe;
        
            }
        }
    }

}





    


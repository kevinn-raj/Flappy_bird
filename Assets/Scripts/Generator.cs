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
    public bool isDrivenByCurriculum = true;
    public bool randomizeAuxInput = true;
    [Range(-3f,3f)]    public float aux_input=1f;
    private const int nAuxInputs = 5;
    private float[] auxInputs = new float[nAuxInputs]{-1f, -.5f, 0f, .5f, 1f};

    [Header("Generator actions")]
    // Hole height
    //  minimum 1.5, Always positive
    private   float height_m; //mean
    private   float height_std; //standard deviation
    private const float height_min=2f;
    private const float height_max=6f;
    private   float height_std_scale = 4.5f; // std = [0, 10]

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
    [SerializeField]    private float nextHeight=height_min;
    public bool endEpisodeOnWrong = true; // end episode
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
    private Vector3 nextPipePos;
    [SerializeField] private float theta_t = 0f; //actual angle relative to previous obstacle
    private float theta_max = 77f * Mathf.Deg2Rad;
    [SerializeField] private float theta_next = 0f;

    [Header("Parameters")]
    [Min(0)]    public int n_obstacles = 10;
    public GameObject prefab;

    /* Limits and Constraints */
    [HideInInspector] public float top_maxy = 4.8f;
    [HideInInspector] public float bottom_maxy;
    [HideInInspector] public float bottom_miny = -4.5f;
    [HideInInspector] public float top_miny;

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

        prevPipe = gameObject;

        // initialize the solver variable
        solver = transform.parent.GetComponentInChildren<Solver>();

        isHeuristic = gameObject.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BehaviorType == Unity.MLAgents.Policies.BehaviorType.HeuristicOnly; /*Check if Heuristic or not*/
    }
    
    public void FixedUpdate(){
        // In case no obstacle in the scene
        if(Obstacles_lst != null){
            if(Obstacles_lst.Count == 0){ 
                transform.parent.GetComponentInChildren<Solver>().EndEpisode();
            }
        }
    }

    public override void OnEpisodeBegin(){
        //  Delete previous created objects before creating new ones
        foreach(GameObject o in Obstacles_lst){
           Destroy(o.gameObject);
           // Debug.Log("Destroying");
        }
        Obstacles_lst = new List<GameObject>();
        counter = 0;

        // To store the position of the previous obstacle, relative to the generator transform
        prevPipe = gameObject;
        pipe = gameObject;
        
        // aux_input as environment parameters // Curriculum
        if(isDrivenByCurriculum)
        aux_input = Academy.Instance.EnvironmentParameters.GetWithDefault("aux_input", 0.0f);
        // randomize the aux input, choose one of the values
        else if(randomizeAuxInput) aux_input = auxInputs[Random.Range(0, nAuxInputs)];
    }
    public override void CollectObservations(VectorSensor sensor){
        sensor.AddObservation(aux_input); // auxiliary input
        if(counter != 0){
        sensor.AddObservation(prevPipe.transform.position.x);
        sensor.AddObservation(prevPipe.transform.position.y);
        sensor.AddObservation(theta_t); //actual angle relative to previous obstacle
        }else{ // For the first spawn
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.y + Random.Range(top_miny, bottom_maxy)); // suupose a random starting position
        sensor.AddObservation(0f); //suppose angle relative to previous obstacle is 0
        // Debug.Log("First obs");
        }  
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
        /* Actions - And internal rewards*/
        // Turn angle in radian
        theta_next = ScaleAction(act[0], -theta_max, theta_max);
        // Debug.Log(theta_next);

        nextHeight = ScaleAction(act[1], height_min, height_max);

        // Vertical difference between consecutive holes, relative position
        nextHDistance = ScaleAction(act[2], h_distance_min, h_distance_max);


        float Dy = nextHDistance * Mathf.Tan(theta_next);
        nextPipePos =  new Vector3(nextHDistance, Dy, 0);
        // y position of the next top pipe, relative position
        nextTopY =  nextHeight/2; // local coord
        // y position of the next bottom pipe, relative position
        nextBottomY = -nextHeight/2;
        CreateWithAgent();

    }

    // Generate with the Generator Agent
    public void CreateWithAgent(){
        if(!isHeuristic){ /*Execute only in Inference mode*/


        /*Spawn only if it is less than a certain number and the solver has achieved the previous one*/
        if(counter <= n_obstacles){
            Transform top, bottom;
            Vector3 initPos;

            initPos = prevPipe.transform.position + nextPipePos;
            // Instantiate the obstacle in the same position as previous
            pipe = Instantiate(prefab, initPos, Quaternion.identity);
                //Debug.Log(pipe);

            // Obstacle speed
            pipe.GetComponent<Obstacles>().speed = obst_speed;
            pipe.GetComponent<Obstacles>().generator = gameObject;

            /* Constraints - Rewarding the Generator*/
            // Internal Reward
            top = pipe.transform.Find("Top Pipe");
            bottom = pipe.transform.Find("Bottom Pipe");

            // Position the top and bottom pipes, local positions
            top.localPosition = new Vector3(0, nextTopY,0);
            bottom.localPosition = new Vector3(0, nextBottomY, 0);
                    
             // Add the created obstacle to the list of all generated obstacles
            Obstacles_lst.Add(pipe);
    


            counter++; // increment the counter
            //Debug.Log(counter);
            latestAchieved = false;
            prevPipe = pipe;
            theta_t = theta_next;
            }
        }
    }

}





    


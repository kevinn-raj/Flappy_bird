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
    [Header("Auxiliary Input")]
    public bool randomizeAuxInput = true;
    [Range(-1f,1f)]
    public float aux_input=1f;

    private bool isRandom=false; // randomize on one episode or not
    [Header("Generator actions")]
    // Hole height
    //  minimum 1.5, Always positive
    private float height_m; //mean
    private float height_std; //standard deviation
    private float height_min=2f;
    private float height_max=4f;
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
    [SerializeField]
    private float nextTopY;
    [SerializeField]
    private float nextBottomY;
    [SerializeField]
    private float nextHDistance=h_distance_min;

    // obstacle speed, constant for an episode an same for every obstacle
    // Positive
    private float obst_speed = 3f;
    private float obst_speed_min = .1f;
    private float obst_speed_max = 6f;

    /* Observations */
    // Previous obstacle's position
    private List<float> previous = new List<float>();


    [Header("Parameters")]
    [Min(0)]
    public int n_obstacles = 10;
    public GameObject prefab;

    /* Limits and Constraints */
    float top_maxy = 4.8f;
    float bottom_maxy;
    float bottom_miny = -4.5f;
    float top_miny;

    private bool fails = false; // Check if the generator has failed the constraints

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
        
        
        if(randomizeAuxInput) aux_input = Random.Range(-1f, 1f);
        
        // Set the first hdistance
        // nextHDistance=Random.Range(h_distance_min, h_distance_max);
        RequestDecision();

        
    }
    public override void CollectObservations(VectorSensor sensor){
        sensor.AddObservation(previous[0]); // top y
        sensor.AddObservation(previous[1]); // bottom y
        sensor.AddObservation(previous[2]); // top or bottom or parent x
        sensor.AddObservation(aux_input); // auxiliary input
    }
    public override void Heuristic(in ActionBuffers actionsOut){
        // CreateRandomly();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers){
        var act = actionBuffers.ContinuousActions;
        float randness = .1f; // Add some randomness
        /* Actions - And internal rewards*/
        // y position of the next top pipe, relative position
        nextTopY = ScaleAction(act[0], top_miny, top_maxy) - Random.Range(0f, randness);

        // y position of the next bottom pipe, relative position
        nextBottomY = ScaleAction(act[1], bottom_miny, bottom_maxy) + Random.Range(0f, randness);

        // Vertical difference between consecutive holes, relative position
        nextHDistance = ScaleAction(act[2], h_distance_min, h_distance_max) + Random.Range(0f, randness);

    }

    // Generate Randomly Proceduraly
    /*public void CreateRandomly(){
        // To store the position of the previous obstacle
         previous = new List<float>(){top_maxy, bottom_miny}; // 

        for(int i=0; i<n_obstacles; i++){
            // Instantiate the obstacle in the same position as previous
            GameObject pipe = Instantiate(prefab, previous, Quaternion.identity);

            // Set height of the hole
            pipe.GetComponent<Obstacles>().Distance = GetRandom(height_m, height_std);

            // Vertical position difference, + or -
            pipe.transform.position += Vector3.up * GetRandom(v_difference_m, v_difference_std); 

            // Horizontal position, only to the right
            pipe.transform.position += Vector3.right * GetRandom(h_distance_m, h_distance_std);

            // Reassign the variable to the actual obstacle
            previous = pipe.transform.position;

            // Obstacle speed
            pipe.GetComponent<Obstacles>().speed = obst_speed;

        }
    }*/

    // Generate with the Generator Agent
    public void CreateWithAgent(){

        float valid_reward = .05f;
        float punishment = -.01f;

        if(counter <= n_obstacles){
            Vector3 initPos = new Vector3(transform.position.x - previous[2] + nextHDistance, transform.position.y, transform.position.z);
            // Instantiate the obstacle in the same position as previous
            GameObject pipe = Instantiate(prefab, initPos, Quaternion.identity);
	    //Debug.Log(pipe);

            //  Debug
            // Debug.DrawLine(transform.position, previous);

            // Obstacle speed
            pipe.GetComponent<Obstacles>().speed = obst_speed;

            /* Constraints - Rewarding the Generator*/
            // Internal Reward
            Transform top = pipe.transform.Find("Top Pipe");
            Transform bottom = pipe.transform.Find("Bottom Pipe");

            // If it is not the first one
            // if(counter != 0)
            {
            // Position the top and bottom pipes, absolute positions
            top.position = new Vector3(top.position.x, nextTopY + transform.position.y, top.position.z);
            bottom.position = new Vector3(bottom.position.x, nextBottomY + transform.position.y, bottom.position.z);
                    }
            // else{ // Randomly place if it the first one
            // top.position = new Vector3(top.position.x,
            //                      Random.Range(transform.position.y + height_min/2, top_maxy/4),
            //                       top.position.z);
            // bottom.position = new Vector3(bottom.position.x,
            //                  Random.Range(transform.position.y - height_min/2, bottom_miny/4),
            //                   bottom.position.z);
            // // Next hdistance
            // // nextHDistance=Random.Range(h_distance_min, h_distance_max);
            // Debug.Log("First one");
            // }
            // Request decision after the changes
            if(counter !=0) // Only if it is not the first
                RequestDecision(); 
                
             // Add the created obstacle to the list of all generated obstacles
            Obstacles_lst.Add(pipe);



            // The difference must be at least height_min and at most height_max
            if(top.position.y - bottom.position.y >= height_min && top.position.y - bottom.position.y 
                <= height_max){
                // + reward
                AddReward(valid_reward);
            }else{
            // Penalize proportionnally to the mistake
            // AddReward(1f * (top_maxy - top.position.y));
                AddReward(punishment); // - reward
                fails = true;
                // Reset the variable previous
                //previous = new List<float>(){transform.position.y - top_maxy, transform.position.y - bottom_miny, 0};
                // Reset the episode after mistakes or not
                // Comment the following line if not
               EndEpisode();
            }

           
	    // Reassign the variable to the actual obstacle
            // Relative position to the generator transform
            previous[0] = transform.position.y - top.position.y;
            previous[1] = transform.position.y - bottom.position.y;
            previous[2] =  transform.position.x - top.transform.position.x;
            
            counter++; // increment the counter
            //Debug.Log(counter);
        }

    }
    
    void FixedUpdate(){
        // After Creating the obstacles
        // End the generator episode if there was a fail
        // Do not continue the training, even for the solver if the generator has failed
        // if(fails) {
        //     fails = false;
        //     EndEpisode();
        // }
            CreateWithAgent();

            /*For generator training only*/
            // if(counter == n_obstacles-1) EndEpisode();
    }



    

}

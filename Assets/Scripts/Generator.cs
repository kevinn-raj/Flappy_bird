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
    [Range(-1f,1f)]
    public float aux_input=1f;

    public bool isRandom=false; // randomize on one episode or not
    [Header("Generator actions")]
    // Hole height
    //  minimum 1.5, Always positive
    public float height_m; //mean
    public float height_std; //standard deviation
    float height_min=1.5f;
    float height_max=9.5f;
    float height_std_scale = 4.5f; // std = [0, 10]

    // Horizontal distance between consecutive holes
    // minimum 1.12, Always Positive
    public float h_distance_m; //mean; 
    public float h_distance_std; //std
    float h_distance_min=1.12f;
    float h_distance_max=7f;
    float h_distance_std_scale = 5f; // std = [0, 10]

    // Vertical difference between consecutive holes
    // The next minus previous
    // Can be negative
    public float v_difference_m; //mean
    public float v_difference_std; //std
    float v_difference_min;
    float v_difference_max;
    float v_difference_std_scale = 4f; // std = [0, 10]

    // obstacle speed, constant for an episode an same for every obstacle
    // Positive
    public float obst_speed;
    float obst_speed_min = .1f;
    float obst_speed_max = 6f;

    /* Observations */
    // Previous obstacle's position
    Vector3 previous;


    [Header("Parameters")]
    [Min(0)]
    public int n_obstacles = 10;
    public GameObject prefab;

    /* Limits and Constraints */
    float top_maxy = 4.8f;
    float bottom_maxy;
    float bottom_miny = -4.5f;
    float top_miny;

    bool fails = false; // Check if the generator has failed the constraints
    bool isGenerating=false; // is the generator generating at the moment

    [HideInInspector]
    public List<GameObject> Obstacles_lst = new List<GameObject>();

    float GetRandom(float mean, float std){
        // mean = (max + min)/2
        // std = (max - min)/2
        float max_n = mean + std;
        float min_n = mean - std;
        return Random.Range(min_n, max_n);
    }

    // Return randomly -1 or +1
    float GetRandomSign(){
        return Mathf.Sign(Random.Range(-1, 1));
    }

    void Awake(){
        // Initialize variables
        bottom_maxy = top_maxy - height_min;
        top_miny = bottom_miny + height_min;

        v_difference_min = -height_max;
        v_difference_max = height_max;
    }

    public override void CollectObservations(VectorSensor sensor){
        sensor.AddObservation(previous);
    }

    // Generate Randomly Proceduraly
    public void CreateRandomly(){
        // To store the position of the previous obstacle
        Vector3 previous = transform.position; // 

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
    }

    // Generate with the Generator Agent
    public void CreateWithAgent(){
        isGenerating=true;

        // To store the position of the previous obstacle
        previous = transform.position; // 

        float valid_reward = 1f;

        for(int i=0; i<n_obstacles; i++){

            RequestDecision(); 

            // Instantiate the obstacle in the same position as previous
            GameObject pipe = Instantiate(prefab, previous, Quaternion.identity);

            /* Request the actions */
            if(isRandom){ // Randomize the actions for an Episode
            // Set height of the hole
            pipe.GetComponent<Obstacles>().Distance = GetRandom(height_m, height_std);

            // Vertical position difference, + or -
            pipe.transform.position += Vector3.up * GetRandom(v_difference_m, v_difference_std); 

            // Horizontal position, only to the right
            pipe.transform.position += Vector3.right * GetRandom(h_distance_m, h_distance_std);
            }else{
            // Set height of the hole   
            pipe.GetComponent<Obstacles>().Distance = height_m;

            // Vertical position difference, + or -
            pipe.transform.position += Vector3.up * GetRandom(v_difference_m, v_difference_std); 

            // Horizontal position, only to the right
            pipe.transform.position += Vector3.right * h_distance_m;
            }

            // Reassign the variable to the actual obstacle
            previous = pipe.transform.position;

            // Obstacle speed
            pipe.GetComponent<Obstacles>().speed = obst_speed;

            /* Constraints - Rewarding the Generator*/
            // Internal Reward
            Transform top = pipe.transform.Find("Top Pipe");
            if(top.position.y > top_maxy){
                // Penalize proportionnally to the mistake
                // AddReward(1f * (top_maxy - top.position.y));
                AddReward(-1f);
                fails = true;
            // EndEpisode();
            }else if(top.position.y < top_miny){
                // AddReward(1f * (top.position.y - top_miny));
                AddReward(-1f);
                fails = true;
            // EndEpisode();
            }
            else{ // + Reward for valid
                AddReward(valid_reward);
            }

            Transform bottom = pipe.transform.Find("Bottom Pipe");
            if(bottom.position.y > bottom_maxy){
                // Penalize proportionnally to the mistake
                // AddReward(1f * (bottom_maxy - bottom.position.y));
                AddReward(-1f);
                fails = true;
            // EndEpisode();
            }else if(bottom.position.y < bottom_miny){
                // AddReward(1f * (bottom.position.y - bottom_miny));
                AddReward(-1f);
                fails = true;
            // EndEpisode();
            }
            else{ // + Reward for valid
                AddReward(valid_reward);
            }

            // Add the created obstacle to the list of all generated obstacles
            Obstacles_lst.Add(pipe);
        }

    }
    void FixedUpdate(){
        // After Creating the obstacles
        // End the generator episode if there was a fail
        // Do not continue the training, even for the solver if the generator has failed
        if(fails) {
            fails = false;
            EndEpisode();
        }
        if(!isGenerating){
            CreateWithAgent();
        }
    }

    public override void OnEpisodeBegin(){
        //  Delete previous created objects before creating new ones
        foreach(GameObject o in Obstacles_lst){
           Destroy(o.gameObject);
           // Debug.Log("Destroying");
        }
        Obstacles_lst = new List<GameObject>();

        isGenerating = false;
    }

    public override void Heuristic(in ActionBuffers actionsOut){
        // CreateRandomly();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers){
        var act = actionBuffers.ContinuousActions;

        /* Actions - And internal rewards*/
        // Hole height , Always positive
        height_m = ScaleAction(act[0], height_min, height_max); // at least height_min
        AddReward(1f * Mathf.Min(act[0] - height_min, 0)); // Penalizing when outputing smaller than allowed
        height_std = ScaleAction(act[1], 0, height_std_scale); // [0, height_std_scale]

        // Horizontal distance between consecutive holes
        h_distance_m = ScaleAction(act[2], h_distance_min, h_distance_max); // at least h_distance_min
        AddReward(1f * Mathf.Min(act[2] - h_distance_min, 0)); // Penalizing when outputing smaller than allowed
        h_distance_std = ScaleAction(act[3], 0, h_distance_std_scale);

        // Vertical difference between consecutive holes
        v_difference_m = ScaleAction(act[4], v_difference_min, v_difference_max);
        v_difference_std = ScaleAction(act[4], 0, v_difference_std_scale);

        // obstacle speed, Always positive
        obst_speed = ScaleAction(act[6], obst_speed_min, obst_speed_max); //at least obst_speed_min
        AddReward(1f * Mathf.Min(act[6] - obst_speed_min, 0)); // Penalizing when outputing negative

    }

}

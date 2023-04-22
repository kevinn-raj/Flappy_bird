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

    [Header("Generator actions")]
    // Hole height
    //  minimum 1.5, Always positive
    public float height_m; //mean
    public float height_std; //standard deviation
    const float height_min=1.5f;

    // Horizontal distance between consecutive holes
    // minimum 1.12, Always Positive
    public float h_distance_m; //mean; 
    public float h_distance_std; //std
    const float h_distance_min=1.12f;

    // Vertical difference between consecutive holes
    // The next minus previous
    // Can be negative
    public float v_difference_m; //mean
    public float v_difference_std; //std

    // obstacle speed, constant for an episode an same for every obstacle
    // Positive
    public float obst_speed;

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
        bottom_maxy = top_maxy - height_min;
        top_miny = bottom_miny + height_min;
    }

    public override void CollectObservations(VectorSensor sensor){
        sensor.AddObservation(aux_input);
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
        // To store the position of the previous obstacle
        Vector3 previous = transform.position; // 

        for(int i=0; i<n_obstacles; i++){
            RequestDecision(); 

            // Instantiate the obstacle in the same position as previous
            GameObject pipe = Instantiate(prefab, previous, Quaternion.identity);

            /* Request the actions */

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

            /* Constraints - Rewarding the Generator*/
            // Internal Reward
            Transform top = pipe.transform.Find("Top Pipe");
            if(top.position.y > top_maxy){
                // Penalize proportionnally to the mistake
                AddReward(1f * (top_maxy - top.position.y));
                fails = true;
            }else if(top.position.y < top_miny){
                AddReward(1f * (top.position.y - top_miny));
                fails = true;
            }

            Transform bottom = pipe.transform.Find("Bottom Pipe");
            if(bottom.position.y > bottom_maxy){
                // Penalize proportionnally to the mistake
                AddReward(1f * (bottom_maxy - bottom.position.y));
                fails = true;
            }else if(bottom.position.y < bottom_miny){
                AddReward(1f * (bottom.position.y - bottom_miny));
                fails = true;
            }
        }
        // After Creating the obstacles
        // End the generator episode if there was a fail
        // Do not continue the training, even for the solver if the generator has failed
        // if(fails) EndEpisode();
    }

    public override void OnEpisodeBegin(){
        CreateWithAgent();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers){
        var act = actionBuffers.ContinuousActions;

        /* Actions - And internal rewards*/
        // Hole height , Always positive
        height_m = Mathf.Abs(act[0]);
        AddReward(-1f * Mathf.Min(act[0], 0));
        height_std = act[1];

        // Horizontal distance between consecutive holes
        h_distance_m = Mathf.Abs(act[2]);
        AddReward(-1f * Mathf.Min(act[2], 0)); // Penalizing when outputing negative
        h_distance_std = act[3];

        // Vertical difference between consecutive holes
        v_difference_m = act[4];
        v_difference_std = act[5];

        // obstacle speed, Always positive
        obst_speed = Mathf.Abs(act[6]); 
        AddReward(-1f * Mathf.Min(act[6], 1)); // Penalizing when outputing negative
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using UnityEngine;
using System;

public class RuledPCG : MonoBehaviour
{
    public GameObject prefab;

    [Header("Parameters")]
    [Min(0)] public int n_obstacles = 10;
    public int counter = 0;

    public float obst_speed = 3f;

    public bool isControledAuto = false;
    [Tooltip("Set to bigger than 0 if you want the random to be fixed")]
    public int seed = 0;
    [Range(1.12f, 4f)] public float heur_hdist_min = 1.12f;
    [Range(1.12f, 4f)] public float heur_hdist_max = 4f;
    [Range(2f, 5f)] public float heur_height_min = 2f;
    [Range(2f, 5f)] public float heur_height_max = 5f;

    /* Limits and Constraints */
    public GameObject ground;
    public GameObject roof;
    [HideInInspector] public float top_maxy = 4.8f;
    [HideInInspector] public float bottom_maxy;
    [HideInInspector] public float bottom_miny = -4.5f;
    [HideInInspector] public float top_miny;
    private Vector3 resetPos;
    
    [HideInInspector]
    public List<GameObject> Obstacles_lst = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        //// fetch the max Y of the ground
        bottom_miny = ground.GetComponent<Collider>().bounds.max.y;
        //// fetch the min Y of the roof
        top_maxy = roof.GetComponent<Collider>().bounds.min.y;

        resetPos = transform.position;
        
        if(seed != 0)
        { // Set the seed
            Random.InitState(seed);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Reset()
    {
        //  Delete previous created objects before creating new ones
        try{
            foreach(GameObject o in Obstacles_lst){
            Destroy(o.gameObject);
            // Debug.Log("Destroying");
            }
        }catch(Exception e){}
        counter = 0;
        Obstacles_lst = new List<GameObject>();
    }
    public void CreateRandom()
    {
        Transform top, bottom;
        Vector3 initPos = resetPos;

        // Values controlled by environment parameters
        if(isControledAuto)
        {
            heur_hdist_min = Academy.Instance.EnvironmentParameters.GetWithDefault("heur_hdist_min", 1.12f);
            heur_hdist_max = Academy.Instance.EnvironmentParameters.GetWithDefault("heur_hdist_max", 4f);
            heur_height_min = Academy.Instance.EnvironmentParameters.GetWithDefault("heur_height_min", 2f);
            heur_height_max = Academy.Instance.EnvironmentParameters.GetWithDefault("heur_height_max", 5f);
        }
        else
        {
            if (seed != 0)
            { // Set the seed
                Random.InitState(seed);
            }
        }

        // Request decision if without Decision requester
        for (int j = 0; j < n_obstacles && counter < n_obstacles; j++)
        {
            float randHeight = Random.Range(heur_height_min, heur_height_max);
            float yposTop = Random.Range(bottom_miny + randHeight, top_maxy);

            // float randHDistance = Random.Range(h_distance_min, h_distance_max);
            float randHDistance = Random.Range(heur_hdist_min, heur_hdist_max);

            GameObject p = Instantiate(prefab, initPos, Quaternion.identity);
            p.GetComponent<Obstacles>().speed = obst_speed;
            p.transform.position += new Vector3(randHDistance, 0, 0);
            Obstacles_lst.Add(p);

            top = p.transform.Find("Top Pipe");
            bottom = p.transform.Find("Bottom Pipe");

            // Position the top and bottom pipes, absolute positions
            top.position = new Vector3(top.position.x, yposTop, top.position.z);
            bottom.position = new Vector3(bottom.position.x, top.position.y - randHeight, bottom.position.z);

            // Add the created obstacle to the list of all generated obstacles
            //Obstacles_lst.Add(p);
            initPos = p.transform.position;
            counter++;
        }
    }

}

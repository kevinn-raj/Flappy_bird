using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuledPCG : MonoBehaviour
{
    public GameObject prefab;

    [Header("Parameters")]
    [Min(0)] public int n_obstacles = 10;
    public int counter = 0;

    public float obst_speed = 3f;

    [Range(1.12f, 4f)] public float heur_hdist_min = 1.12f;
    [Range(1.12f, 4f)] public float heur_hdist_max = 4f;
    [Range(2f, 5f)] public float heur_height_min = 2f;
    [Range(2f, 5f)] public float heur_height_max = 5f;

    /* Limits and Constraints */
    [HideInInspector] public float top_maxy = 4.8f;
    [HideInInspector] public float bottom_maxy;
    [HideInInspector] public float bottom_miny = -4.5f;
    [HideInInspector] public float top_miny;

    // Start is called before the first frame update
    void Start()
    {
        CreateRandom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Reset()
    {
        counter = 0;
    }
    public void CreateRandom()
    {
        Transform top, bottom;
        Vector3 initPos = transform.position;

        for (int j = 0; j < n_obstacles && counter < n_obstacles; j++)
        {
            float randHeight = Random.Range(heur_height_min, heur_height_max);
            float yposTop = Random.Range(top_maxy, top_miny + randHeight);//local
                                                                          // float randHDistance = Random.Range(h_distance_min, h_distance_max);
            float randHDistance = Random.Range(heur_hdist_min, heur_hdist_max);

            GameObject p = Instantiate(prefab, initPos, Quaternion.identity);
            p.GetComponent<Obstacles>().speed = obst_speed;
            p.transform.position += new Vector3(randHDistance, 0, 0);

            top = p.transform.Find("Top Pipe");
            bottom = p.transform.Find("Bottom Pipe");

            // Position the top and bottom pipes, absolute positions
            top.position += new Vector3(0, yposTop, 0);
            bottom.position = new Vector3(bottom.position.x, top.position.y - randHeight, bottom.position.z);

            // Add the created obstacle to the list of all generated obstacles
            //Obstacles_lst.Add(p);
            initPos = p.transform.position;
            counter++;
        }
    }
}

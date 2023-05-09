using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Pipe_Spawner : MonoBehaviour
{
    [Header("Specific to the pipes")]
    public GameObject prefab;
    public float pipe_Hole_Height = 2.5f; // The same as Distance in Pipes
    [Tooltip("Distance between two consecutive pair of pipes")]
    public float nextPipeDistance = 2f;
    [HideInInspector]
    public float spawnRate; // time bewteen spawn
    public float pipeSpeed=3; // 3 matches with the speed of the ground texture animation

    // min and max values for the random Y coordinate
    [Range(-3.25f, 3.25f)]
    public float minY;
    [Range(-3.25f, 3.25f)]
    public float maxY;

    // limits for the y coordinate of the pipe parent object
    private float[] limitsY = new float[2];

     //Height in unit that is seen by the camera, actually the camera can see from
    // y = 5 to y = -5 : bg_height/2 to -bg_height/2, so is centered at y=0
    private float bg_height = 10;

    // List of the Spawned pipes
    public int numOfSpawnedPipes;
    [HideInInspector]
    public List<GameObject> pipes_spawned = new List<GameObject>(); 

    public void Awake(){
        // Initialize the spawnRate
        spawnRate = nextPipeDistance / pipeSpeed;
        numOfSpawnedPipes = pipes_spawned.Count; // Updates the # of spawned Pipes


        // Initialize the y-coordinate limits
      // with some padding 0.5 to avoid the Pipe to disappear completely from the screen
        limitsY[0] = -bg_height/2 + .5f + pipe_Hole_Height/2; // lower limit
        limitsY[1] = bg_height/2 - .5f - pipe_Hole_Height/2; // upper limit

        // Clamp the minY and maxY to not exceed the limits
        minY = Mathf.Clamp(minY, limitsY[0], limitsY[1]);
        maxY = Mathf.Clamp(maxY, limitsY[0], limitsY[1]);

        // Random.InitState(15); // fix random
    }
    private void OnEnable()
    {
        
        InvokeRepeating(nameof(Spawn), 0, spawnRate);

    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Spawn));
    }

    private void Spawn()
    {


        GameObject pipe = Instantiate(prefab, transform.position, Quaternion.identity);
        pipe.GetComponent<Pipes>().Distance = pipe_Hole_Height;
        pipe.transform.position += Vector3.up * Random.Range(minY, maxY);
        pipe.GetComponent<Pipes>().speed = pipeSpeed;

        // Append the element by reference
        pipes_spawned.Add(pipe);
        numOfSpawnedPipes = pipes_spawned.Count; // Updates the # of spawned Pipes

        pipe.GetComponent<Pipes>().spawner = this;
    }

    public void Reset(){

        foreach (GameObject p in pipes_spawned) {
            Destroy(p.gameObject);
        }
        pipes_spawned.Clear();
        numOfSpawnedPipes = pipes_spawned.Count; // Updates the # of spawned Pipes
        // Random.InitState(15); // fix random
    }


}

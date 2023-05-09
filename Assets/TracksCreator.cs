using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;


public class TracksCreator : MonoBehaviour
{
    Generator gen;
    
    public Generator.Track tracks;

    public float timescale = 0f;
    

    // Start is called before the first frame update
    void Awake()
    {
        Time.timeScale = timescale;
        gen = GetComponent<Generator>();
        tracks =  new Generator.Track();
        gen.CreateTracks(10, 1, out tracks);
    }

    // Update is called once per frame
    void Update()
    {
        gen.RequestDecision();
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnLevels : MonoBehaviour
{
    [Tooltip("Relative to Resources")]
    public string TracksRootFolder = "Tracks/"; // Under resources
    public List<string> aux_list = new List<string>{"-1", "-0,5", "0", "0,5", "1"};
    public float aux_input = 1f;
    public bool aux_from_env = true; // If load the aux_input from the env parameter
    
    public int track_counter = 0;
    public int trackNumber = 75;
    public int n_obstacles=10;
    public int maxTrials = 100;
    private int trialCounter = 0;

    public Solver_Inference solver;
    GameObject actualPrefab; // The prefab
    GameObject actualTrackObj; //The instanciated object

    public float timeScale = 1f;
    // Start is called before the first frame update
    void Start()
    {
        // #########################################################################
        // IMPORTANT to properly convert float to string with "." as decimal not ","
        System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";
        System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        // ######################################################################### 


        track_counter = 0;

        if(solver != null ){
            solver.OnReset.AddListener(Reset); // Listening to the event OnReset
            if(aux_from_env)        // Get the aux input from the solver if it exists
                aux_input = solver.GetAuxInput();
        }else{
            Debug.LogError("Solver not assigned");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
    }


    public void NextTrack(){
        // Debug.Log("Next!!");
        // without extension
        actualPrefab = Resources.Load<GameObject>(TracksRootFolder + "/" + Convert.ToString(aux_input) + "/" + Convert.ToString(track_counter));
        actualTrackObj = Instantiate(actualPrefab, transform.position, Quaternion.identity);
        track_counter++; // Increment
        if(track_counter == trackNumber){ // 
            trialCounter++;
        }

        track_counter %= trackNumber; //0 to track number - 1
    }
    public void Reset(){
        Destroy(actualTrackObj);
        if(trialCounter == maxTrials){ // If the number of max trials is reached stop the Academy or the inference
            if(solver != null){
            solver.StopAcademy();
            }
            Debug.Log("Trial finished!");
        }else{ // Spawn only if the number of max trials is not reached
            NextTrack();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class TracksCreator : MonoBehaviour
{
    Generator gen; 
    public bool isCreating = true;
    public Generator.Track tracks;
    [HideInInspector] public int n_aux;
    // [HideInInspector]
    public float[] auxs;
    // public float[] auxs = new float[n_aux]{-2, -1, -.5f, 0, .5f, 1, 2}; // Aux input list that will be used
    public float timescale = 0f;
    public int n_per_track = 10;
    int track_counter = 0;
    public int track_per_aux = 50;
    public string root = "Assets/Prefabs/Tracks";
    int aux_counter = 0;

    public string delimiter = ";";


    // Start is called before the first frame update
    void Awake()
    {
        Time.timeScale = timescale;
        gen = GetComponent<Generator>();
        tracks =  new Generator.Track();
        // WriteString("Hello", "test.txt");
        n_aux = auxs.Length;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
#if UNITY_EDITOR
        if(isCreating && aux_counter < n_aux)
        {
            // Export for 1 track
            if(gen.counter >= n_per_track){ //After the gen has created every obstacle of the current track
                if(track_counter < track_per_aux){
                   // replace the aux input by the next one
                    if(aux_counter < n_aux){
                        string aux_string = Convert.ToString(auxs[aux_counter]);
                        gen.CreateTracks(n_per_track, auxs[aux_counter], out tracks); 
                        // create folders if not existing
                        if (!Directory.Exists(root + "/" + aux_string))
                            AssetDatabase.CreateFolder(root, aux_string);
                            string track_string = Convert.ToString(track_counter);
                            string localPath = root + "/" + aux_string + "/" + track_string + ".prefab";
                            GameObject parent = tracks.pieces[0].transform.parent.gameObject; //Get the parent
                            PrefabUtility.SaveAsPrefabAsset(parent, localPath); // save the prefab
                            // writing the specifications into a file
                            List<float> ydiffs = tracks.ydiffs;
                            List<float> heights = tracks.heights;
                            List<float> distances = tracks.distances;
                            List<float> angles = tracks.angles;
                            
                            string content = ToCSV(ydiffs, heights, distances, angles);
                            string csv_path = root + "/" + aux_string + "/" + track_string + ".csv";
                            WriteString(content, csv_path);
                            
                        track_counter++; // new track
                        gen.EndEpisode(); // Reset the Gen counter and destroy the GameObjects
                    }
                }
                else{ // Reset it
                    track_counter = 0;
                    aux_counter++;
                }
            }
        }
#endif

    }

    public static void WriteString(string text, string path)
       {
            // Debug.Log(path);
           //Write some text to the test.txt file

            using(var writer = new StreamWriter(path, false))
            {
                writer.Write(text);
                writer.Close();
            }

        //    StreamReader reader = new StreamReader(path);
        //    //Print the text from the file
        //    Debug.Log(reader.ReadToEnd());
        //    reader.Close();

        }

        public string ToCSV(List<float> ydiffs_,List<float> heights_,List<float> distances_,List<float> angles_)
        {
            var sb = new System.Text.StringBuilder("ydiffs,heights,distances,angles");
            for(int i = 1; i < n_per_track; i++) { // the first element is not relevant
            string y = ydiffs_[i].ToString();
            string h = heights_[i].ToString();
            string d = distances_[i].ToString();
            string a = angles_[i].ToString();
                sb.Append('\n').Append(y).Append(delimiter).Append(h).Append(delimiter).Append(d).Append(delimiter).Append(a);
            }
            return sb.ToString();
        }


}

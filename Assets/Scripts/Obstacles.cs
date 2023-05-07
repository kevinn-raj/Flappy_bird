﻿using UnityEngine;

public class Obstacles : MonoBehaviour
{
    public Transform top;
    public Transform bottom;
    public BoxCollider scoringZone;
    public GameObject generator;
    private Generator gen;
    Transform genTrans;
    public GameObject origin_obj;

    [SerializeField]
    private float distance; // distance between the top and the bottom pipes
    public float Distance // Get and Set methods
    {
        get {return distance;}
        set {
            distance = value;
            // Update the contents
            // Set the distance between the pipes
            top.localPosition = new Vector3(top.localPosition.x, distance/2, top.localPosition.z);
            bottom.localPosition = new Vector3(bottom.localPosition.x, -distance/2, bottom.localPosition.z);

            // Set the size of the scoring zone accordingly
            scoringZone.size = new Vector3(scoringZone.size.x,
                                            distance,
                                            scoringZone.size.z);
        }
    }

    public float speed;
    private float leftEdge;

    [HideInInspector]
    public Pipe_Spawner spawner; // Who spawned you


    void SetScoringBox(){
        Vector3 diff = top.localPosition - bottom.localPosition;
        Vector3 mid = (top.localPosition + bottom.localPosition)/2;
        scoringZone.size = new Vector3(scoringZone.size.x, diff.y, 2);
        scoringZone.center = new Vector3(scoringZone.center.x, mid.y, scoringZone.center.z); // at the center
    }
    private void Start()
    {
        if(generator)
            gen = generator.GetComponent<Generator>();  
 
    }

    private void Update()
    {
        // Move the Pipe to the left
        transform.position += Vector3.left * speed * Time.deltaTime;

        SetScoringBox();

    }

    private void OnTriggerStay(Collider o){
         if (o.gameObject.CompareTag("Destroyer")){
            // Destroy this gameObject and its children
            gameObject.SetActive(false);
            Destroy(gameObject, 10f); // Destroy after 10s
         }
    }

}

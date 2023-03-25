using UnityEngine;

public class Pipes : MonoBehaviour
{
    public Transform top;
    public Transform bottom;
    public BoxCollider scoringZone;

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

    private void Start()
    {
        // Set the distance between the pipes
        top.localPosition = new Vector3(top.localPosition.x, distance/2, top.localPosition.z);
        bottom.localPosition = new Vector3(bottom.localPosition.x, -distance/2, bottom.localPosition.z);   

        // Set the size of the scoring zone accordingly
        scoringZone.size = new Vector3(scoringZone.size.x,
                                        distance,
                                        scoringZone.size.z);

        leftEdge = spawner.GetComponent<Transform>().position.x - 12f;
    }

    private void Update()
    {
        // Move the Pipe to the left
        transform.position += Vector3.left * speed * Time.deltaTime;

        // Destroy the Pipe
        if (transform.position.x < leftEdge) {
            // Remove from the array before destroying it, to avoid errors
            spawner.pipes_spawned.Remove(this.gameObject);
            Destroy(gameObject);
        }
    }

}

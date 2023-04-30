using UnityEngine;

public class Obstacles : MonoBehaviour
{
    public Transform top;
    public Transform bottom;
    public BoxCollider scoringZone;
    public GameObject generator;
    private Generator gen;
    Transform genTrans;

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
        scoringZone.size = new Vector3(0.8f, diff.y, 2);
        scoringZone.center = new Vector3(0, mid.y, 0); // at the center
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
        if(generator){
            genTrans = gen.GetComponent<Transform>();
    
            float valid_reward = .01f;
            float punishment = -.1f;
                    // REWARD- The bottom and the top  must be inside the limits
            if(genTrans.position.y + bottom.position.y  >= gen.bottom_maxy - gen. ||
                genTrans.position.y + top.position.y  <= gen.top_miny ||
                genTrans.position.y + top.position.y  >= gen.top_maxy){
                gen.AddReward(punishment); // - reward
                // Reset the episode after mistakes or not
                // Comment the following line if not
                if(gen.endEpisodeOnWrong)
                    gen.EndEpisode(); 
            }else{  
                // + reward
                gen.AddReward(valid_reward);
                // Debug.Log("Valid");
            }
        }
    }

    private void OnTriggerStay(Collider o){
         if (o.gameObject.CompareTag("Destroyer")){

            // Destroy the children then the parent
            // foreach(Transform child in transform){
            //     Destroy(child.gameObject);
            // }

            // Destroy this gameObject and its children
            gameObject.SetActive(false);
            Destroy(gameObject, 10f); // Destroy after a minute
         }
    }

}

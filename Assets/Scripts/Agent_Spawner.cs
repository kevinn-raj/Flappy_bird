using UnityEngine;

// [ExecuteInEditMode] // Execute in Edit Mode
public class Agent_Spawner : MonoBehaviour
{
    public GameObject prefab;
    public int spawnCount = 10;
    public Vector3 minPos;
    public Vector3 maxPos;

    private void Awake()
    {
        Spawn();
    }

    private void Spawn()
    {
        for(int i=0; i<spawnCount; i++){
            // Random position
            Vector3 randpos = new Vector3(Random.Range(minPos.x, maxPos.x),
                                                     Random.Range(minPos.y, maxPos.y),
                                                     Random.Range(minPos.z, maxPos.z));
            // Instantiate the prefab object
            GameObject player_inst = Instantiate(prefab,
                                                 transform.position + randpos,
                                                 prefab.transform.rotation);

            // Copy the scale of the prefab
            player_inst.transform.localScale = prefab.transform.localScale;
        }
    }

}

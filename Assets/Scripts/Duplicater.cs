using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// [ExecuteInEditMode]
public class Duplicater : MonoBehaviour
{
    public GameObject item;
    public int number;
    public float dx; 

    // Start is called before the first frame update
    void Start()
    {
        // for(int i=0; i<number; i++){
        //     Vector3 coord = transform.position + new Vector3(dx*(i+1),0f,0f);
        //     GameObject o = PrefabUtility.InstantiatePrefab(item) as GameObject;
        //     o.transform.position = coord;
        // }
    }

}

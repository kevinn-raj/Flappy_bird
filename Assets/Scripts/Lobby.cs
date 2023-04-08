using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // With agent
    public void LoadTest(){
        SceneManager.LoadScene("Test_scene", LoadSceneMode.Single);
    }

    // Human play
    public void LoadPlay(){
        SceneManager.LoadScene("Play_scene", LoadSceneMode.Single);
    }

    // Load lobby
    public void LoadLobby(){
        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }
}

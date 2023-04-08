using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Score_display : MonoBehaviour
{
    // Text in using UnityEngine.UI;
    public Text score;
    public Text maxScore;
    public Jumper player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //  Convert in System
        score.text = Convert.ToString(player.score);
        maxScore.text = Convert.ToString(player.maxScore);
    }
}

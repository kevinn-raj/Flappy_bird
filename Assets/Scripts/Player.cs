using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Player : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    private int spriteIndex;

    private Rigidbody2D rb;
    public float m_Thrust = 10.0f;

    public float strength = 5f;
    public float gravity = -9.81f;
    public float tilt = 5f;

    private Vector3 direction;

    private string Action = ""; // Or "jump"
    private bool jumpReady = true;
    private const float JUMP_TIME_DELAY = .01f; // Cannot jump twice in this window of time

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); 
           //Fetch the Rigidbody from the GameObject with this script attached
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(AnimateSprite), 0.15f, 0.15f);

    }

    // Set the player up
    private void setup(){
        Vector3 position = transform.position;
        position.y = 0f;
        transform.position = position;
        direction = Vector3.zero;   // Reset the direction
        rb.velocity = Vector2.zero; // Reset the velocity
    }

    public void OnEnable()
    {
        setup();
    }


    private void FixedUpdate()
    {
        // Jump
        if (Input.GetButtonDown("Jump")) {
            Vector2 force = new Vector2(0, m_Thrust);
            rb.AddForce(force, ForceMode2D.Force);
        }

        // Apply gravity and update the position
        direction.y += gravity * Time.deltaTime;
        // transform.position += direction * Time.deltaTime;

        // Tilt the bird based on the direction
        Vector3 rotation = transform.eulerAngles;
        rotation.z = direction.y * tilt;
        transform.eulerAngles = rotation;
    }

    private void AnimateSprite()
    {
        spriteIndex++;

        if (spriteIndex >= sprites.Length) {
            spriteIndex = 0;
        }

        if (spriteIndex < sprites.Length && spriteIndex >= 0) {
            spriteRenderer.sprite = sprites[spriteIndex];
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Single instance
        if (other.gameObject.CompareTag("Obstacle")) {
            FindObjectOfType<GameManager>().GameOver();
            setup();
        } else if (other.gameObject.CompareTag("Scoring")) {
            FindObjectOfType<GameManager>().IncreaseScore();
        }

        // Many instances
        // if (other.gameObject.CompareTag("Obstacle")) {
        //     FindObjectOfType<GameManager>().GameOver();
        // } else if (other.gameObject.CompareTag("Scoring")) {
        //     FindObjectOfType<GameManager>().IncreaseScore();
        // }
    }

}

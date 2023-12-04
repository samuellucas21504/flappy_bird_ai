using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public GameManager gameManager;
    public NeuralNetwork nn;

    private SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    private int currentSprite;

    private Vector3 direction;
    public float gravity = -9.8f;
    public float strength = 5;

    public float score = 0;
    public float publicScore = 0;
    public float shouldFlap = 0;
    public bool alive = true;

    public void Start()
    {
        nn = GetComponent<NeuralNetwork>();
        gameManager = GetComponent<GameManager>();
        InvokeRepeating(nameof(AnimateSprite), 0.15f, 0.15f);
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if (alive)
        {
            score += Time.deltaTime;
        }
    }

    void Update()
    {
        if(alive)
        {
            direction.y += gravity * Time.deltaTime;
            transform.position += direction * Time.deltaTime;
        }
        else
        {
            transform.position = new Vector3(0, -10000, 0);
        }
    }

    public void Flap()
    {
        direction = Vector3.up * strength;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            alive = false;
            
        } else if(collision.gameObject.CompareTag("Scoring") && alive)
        {
            score++;
            publicScore++;
        }
    }

    private async void AnimateSprite() 
    {
        currentSprite++;

        if(currentSprite >= sprites.Length)
        {
            currentSprite = 0;
        }

        spriteRenderer.sprite = sprites[currentSprite];
    }

    public void Reset()
    {
        transform.SetPositionAndRotation(new Vector3(-1, 0, 0), Quaternion.identity);
        score = 0;
        publicScore = 0;
        alive = true;
    }
}

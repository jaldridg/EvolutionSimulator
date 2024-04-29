using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines the creature's needs and how they affect the creature
public class Biology : MonoBehaviour
{
    [HideInInspector]
    public float maxFood;
    public float food;

    private WorldManager world;

    // Start is called before the first frame update
    void Start()
    {
        world = FindObjectOfType<WorldManager>();

        maxFood = 100.0f;
        food = maxFood;   
    }

    // Update is called once per frame
    void Update()
    {
        food -= Time.deltaTime * WorldManager.HUNGER_DECAY;
        if (food < 0) {
            Destroy(gameObject);
        }
    }

    public void Eat() {
        food = Math.Min(food + 80.0f, maxFood);
    }
}

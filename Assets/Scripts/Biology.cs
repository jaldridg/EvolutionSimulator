using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Defines the creature's needs and how they affect the creature
public class Biology : MonoBehaviour
{
    [HideInInspector] public float stomachCapacity;
    public float food;

    public float maxSpeed;
    public float maxTurnSpeed;

    [HideInInspector] public float maxHealth;
    public float health;

    // Comes from food, High energy is beneficial and low energy is detremental
    public float energyLevel;

    public float mass;

    // In seconds - the point that creature's start having negative health effects due to age
    [HideInInspector] public float oldAgePoint;
    public float age;

    /* To implement
        energy
        regenerationRate
        age
        acceleration
        maxSize
        size
    */

    // The amount of energy a creature spends per second no matter what it's doing or how big it is
    public const float BASE_ENERGY_EXPENSE = 1.0f;

    // Higher numbers are faster
    public const float REGENERATION_CONSTANT = 0.1f;

    // The energy cost of movement
    public const float MOVEMENT_CONSTANT = 0.2f;

    private NavMeshAgent agent;
    private WorldManager world;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        world = FindObjectOfType<WorldManager>();

        // Speed related
        maxSpeed = 2.5f;
        agent.speed = maxSpeed;
        maxTurnSpeed = maxSpeed * 50.0f;
        agent.angularSpeed = maxTurnSpeed;

        // Health related
        maxHealth = 100.0f;
        health = maxHealth;

        // Food related
        stomachCapacity = 100.0f;
        food = 80.0f;

        // Body related
        mass = 1.0f;

        // Age related
        age = 0.0f;
        oldAgePoint = 180.0f;

        StartCoroutine(IncreaseAge());
    }

    // Update is called once per frame
    void Update()
    {
        // Use environmental and internal factors to affect creature's state

        // Set energy level
        // Well fed buff
        if (food > stomachCapacity * 0.8f) {
            energyLevel = food / (stomachCapacity * 0.8f);

        // Starvation penalty
        } else if (food < stomachCapacity * 0.3f) {
            energyLevel = 0.5f + 0.5f * (food / (stomachCapacity * 0.3f));

        // Normal enery levels
        } else {
            energyLevel = 1.0f;
        }

        float movementmultiplier = Mathf.Min(energyLevel, 1); 
        agent.speed = maxSpeed * movementmultiplier;
        agent.angularSpeed = maxTurnSpeed * movementmultiplier;

        // Model off kinetic energy formula
        float energyDemand = mass * agent.speed * agent.speed * MOVEMENT_CONSTANT + BASE_ENERGY_EXPENSE ;

        food = Mathf.Max(food - energyDemand * Time.deltaTime, 0);

        // use energy level to determine regeneration
        // use energy level to test 

        // Enact the consequences of creature's state
        if (energyLevel > 1.0f) {
            float regenerationRate = REGENERATION_CONSTANT * (energyLevel - 1) * maxHealth * Time.deltaTime;
            health = Mathf.Min(health + regenerationRate, maxHealth);
        }
        if (food == 0) {
            health -= energyDemand * Time.deltaTime;
        }

        if (health < 0) {
            Destroy(gameObject);
        }
    }

    IEnumerator IncreaseAge() {
        while (true) {
            age++;
            if (age > oldAgePoint) {
                maxSpeed *= 0.99f;
                maxTurnSpeed *= 0.995f;
                health -= (age - oldAgePoint) / oldAgePoint;
            }
            yield return new WaitForSeconds(1);
        }
    }

    public void Eat() {
        food = Mathf.Min(food + WorldManager.FOOD_NUTRITION, stomachCapacity);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Defines the creature's needs and how they affect the creature
public class Biology : MonoBehaviour
{
    [SerializeField] private int generation;

    [HideInInspector] public float stomachCapacity;
    public float food;

    // The percent fullness of the stomach where creatures are well fed
    public static float WELL_FED_CONSTANT = 0.8f;
    // The percent fullness of the stomach where creatures will begin looking for food
    public static float HUNGER_CONSTANT = 0.5f;
    // The percent fullness of the stomach where creatures will have negative effects from hunger
    public static float STARVATION_CONSTANT = 0.25f;

    public float maxSpeed;
    public float maxTurnSpeed;

    [HideInInspector] public float maxHealth;
    public float health;

    // Comes from food, High energy is beneficial and low energy is detremental
    public float energyLevel;

    public float mass;

    // In seconds - the point that creature's start having negative health effects due to age
    [HideInInspector] public float oldAgePoint;
    [HideInInspector] public float maturityPoint;
    public float age;

    // The total energy the creature needs to spend to reproduce
    public float offspringEnergyCost;

    // The amount of energy expended to produce the next offspring
    public float offspringEnergySpent;


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
        oldAgePoint = 180.0f;
        maturityPoint = oldAgePoint / 3.0f;
        age = 0.0f;

        // Reproduction related
        offspringEnergyCost = 25.0f;
        offspringEnergySpent = 0.0f;

        gameObject.name = "Creature (Gen: " + generation + ")";

        StartCoroutine(IncreaseAge());
    }

    // Update is called once per frame
    void Update()
    {
        /*********** Use environmental and internal factors to affect creature's state **************/

        // Set energy level
        // Well fed buff
        if (food > stomachCapacity * WELL_FED_CONSTANT) {
            energyLevel = food / (stomachCapacity * WELL_FED_CONSTANT);

        // Starvation penalty
        } else if (food < stomachCapacity * STARVATION_CONSTANT) {
            energyLevel = 0.5f + 0.5f * (food / (stomachCapacity * STARVATION_CONSTANT));

        // Normal enery levels
        } else {
            energyLevel = 1.0f;
        }

        float movementMultiplier = Mathf.Min(energyLevel, 1); 
        agent.speed = maxSpeed * movementMultiplier;
        agent.angularSpeed = maxTurnSpeed * movementMultiplier;

        // See how much energy should be spent on reproduction
        float offspringEnergy = 0.0f;
        if (age > maturityPoint) {
            offspringEnergy = food > stomachCapacity * HUNGER_CONSTANT ? energyLevel : 0.0f;
        }
        offspringEnergySpent += offspringEnergy * Time.deltaTime;

        // Modeled off kinetic energy formula
        float movementEnergy = mass * agent.speed * agent.speed * MOVEMENT_CONSTANT;

        float energyDemand = BASE_ENERGY_EXPENSE + movementEnergy + offspringEnergy;
        food = Mathf.Max(food - energyDemand * Time.deltaTime, 0);

        // Reproduce if enough energy has been put into it
        if (offspringEnergySpent >= offspringEnergyCost) {
            GameObject offspring = Instantiate(world.creature, transform.position, Quaternion.identity);
            offspring.GetComponent<Biology>().increaseGeneration(generation);
            offspringEnergySpent = 0.0f;
        }

        /*********** Enact the consequences of creature's state **************/
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

    public void increaseGeneration(int gen) {
        generation = gen + 1;
    }

    public void Eat() {
        food = Mathf.Min(food + WorldManager.FOOD_NUTRITION, stomachCapacity);
    }
}

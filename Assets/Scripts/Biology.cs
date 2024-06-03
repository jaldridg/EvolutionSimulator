using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    [HideInInspector] public float maxHealth;
    public float health;

    // Comes from food, High energy is beneficial and low energy is detremental
    public float energyLevel;

    [HideInInspector] public float normalEnergyLevel;

    public float mass;

    public float size;

    // The amount of energy a creature spends per second no matter what it's doing based on it's size
    public float baseEnergyExpense;

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
        acceleration
        maxSize
        size
    */

    // The ratio of base energy that can be spent to stay alive - anything under this ratio and the creature dies
    public const float BASE_ENERGY_DEATH_RATIO = 0.3f;

    // Higher numbers are faster
    public const float REGENERATION_CONSTANT = 0.1f;

    // The energy cost of movement
    public const float MOVEMENT_CONSTANT = 0.2f;

    // The energy ratio amount which will be maintained by sacrificing health
    [HideInInspector] public float healthToEnergyDeficitPoint;

    private NavMeshAgent agent;
    private WorldManager world;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        world = FindObjectOfType<WorldManager>();

        // Body related
        size = 1.0f;
        mass = size * size * size;
        baseEnergyExpense = size;

        // Health related
        maxHealth = mass * 100.0f;
        health = maxHealth;

        // Food related
        stomachCapacity = mass * 100.0f;
        food = stomachCapacity * WELL_FED_CONSTANT;

        // Energy related
        normalEnergyLevel = size;
        healthToEnergyDeficitPoint = normalEnergyLevel * 0.5f;

        // Age related
        oldAgePoint = 180.0f;
        maturityPoint = oldAgePoint / 3.0f;
        age = 0.0f;

        // Reproduction related
        offspringEnergyCost = 25.0f;
        offspringEnergySpent = 0.0f;

        gameObject.name = "Creature (Gen " + generation + ")";

        StartCoroutine(IncreaseAge());
    }

    // Update is called once per frame
    void Update()
    {
        if (health < 0) { Die(); }

        // Assume worst for creature
        agent.speed = 0.0f;
        agent.angularSpeed = 0.0f;

        /*********** Budget energy based on the creature's necessities **************/
        energyLevel = calculateEnergyLevel();
        food -= energyLevel * Time.deltaTime;

        // If needed, maintain base energy levels by sacrificing the body (health) for energy
        if (energyLevel < healthToEnergyDeficitPoint) {
            health -= (healthToEnergyDeficitPoint - energyLevel) * Time.deltaTime;
        }
        float currentRemainingEnergy = energyLevel;

        // Expend minimum required energy and use health if there's not enough
        float minimumEnergyRequirement = baseEnergyExpense * BASE_ENERGY_DEATH_RATIO;
        if (currentRemainingEnergy < minimumEnergyRequirement) {
            health -= (minimumEnergyRequirement - currentRemainingEnergy) * Time.deltaTime;
        }
        currentRemainingEnergy -= minimumEnergyRequirement;

        // Expend remaining energy on movement and growth
        bool concious = currentRemainingEnergy > 0;
        if (concious) {
            // Find a ratio between movement and growth based on creature's health
            // Priority is on creature's movement when injured and on growth when healthy
            float movementVersusGrowthRatio = currentRemainingEnergy * WELL_FED_CONSTANT / normalEnergyLevel;
            float growthEnergyBudget = currentRemainingEnergy - movementVersusGrowthRatio;
            float movementEnergyBudget = currentRemainingEnergy - growthEnergyBudget;

            expendGrowthEnergy(growthEnergyBudget);
            expendMovementEnergy(movementEnergyBudget);
        }
    }

    IEnumerator IncreaseAge() {
        while (true) {
            age++;
            if (age > oldAgePoint) {
                normalEnergyLevel *= 0.995f;
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

    private void Die() {
        Destroy(gameObject);
    }

    private float calculateEnergyLevel() {
        // Set energy level
        // Well fed buff
        float eLevel;
        if (food > stomachCapacity * WELL_FED_CONSTANT) {
            // Lerp between full energy and a little bonus energy when full
            eLevel = normalEnergyLevel * (food / (stomachCapacity * WELL_FED_CONSTANT));

        // Starvation penalty
        } else if (food < stomachCapacity * STARVATION_CONSTANT) {
            // Lerps between full and no energy when starving
            // TODO: Go back to 50% minimum energy
            eLevel = normalEnergyLevel * (food / (stomachCapacity * STARVATION_CONSTANT));

        // Normal enery levels
        } else {
            eLevel = normalEnergyLevel;
        }

        // Health also affects energy
        eLevel *= health / maxHealth;
        return eLevel;
    }

    // Regenerates, matures, or reproduces given an amoutn of energy
    private void expendGrowthEnergy(float energyBudget) {
        // For now split energy equally
        if (health < maxHealth) {
            float regenerationBudget = energyBudget / 2;
            float developmentBudget = energyBudget / 2;

            health += developmentBudget * Time.deltaTime;
            expendDevelopmentEnergy(regenerationBudget);
        } else {
            expendDevelopmentEnergy(energyBudget);
        }
    }

    // Put energy towards maturing or reproducing
    private void expendDevelopmentEnergy(float energyBudget) {
        // Reproduce if enough energy has been put into it
        offspringEnergySpent += energyBudget * Time.deltaTime;
        if (offspringEnergySpent >= offspringEnergyCost) {
            GameObject offspring = Instantiate(world.creature, transform.position, Quaternion.identity);
            offspring.GetComponent<Biology>().increaseGeneration(generation);
            offspringEnergySpent = 0.0f;
        }
    }

    // Calculates how fast the creature can move to exactly expend the energy budget
    private void expendMovementEnergy(float energyBudget) {
        // Solve for speed given formula: movementEnergy = mass * speed * speed * MOVEMENT_CONSTANT;
        agent.speed = (float) Math.Sqrt(energyBudget / (mass * MOVEMENT_CONSTANT));
        agent.angularSpeed = agent.speed * 50.0f;
    }
}

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

    public bool hungry;
    public bool starving;
    public bool concious;
    public bool healthy;
    public bool mature;

    // The percent fullness of the stomach where creatures are well fed
    public static float WELL_FED_CONSTANT = 0.8f;
    // The percent fullness of the stomach where creatures will begin looking for food
    public static float HUNGER_CONSTANT = 0.5f;

    // The percent fullness of the stomach where creatures will have negative effects from hunger
    public static float STARVATION_CONSTANT = 0.25f;


    [HideInInspector] public float maxHealth;
    public float health;

    [HideInInspector] public float normalEnergyLevel;

    public float currentEnergyLevel;

    public float mass;

    public float size;

    public float maxSize;

    // The amount of energy a creature spends per second no matter what it's doing based on it's size
    [HideInInspector] public float baseEnergyExpense;

    public float age;

    // The total energy the creature needs to spend to mature or reproduce
    public float growthEnergyCost;

    // The amount of energy expended towards maturity or the next offspring
    public float growthEnergySpent;

    // The energy expenditure in which an offspring will be born
    // Less energy to reproduce will result in premature offspring which may have difficulty surviving
    public float offspringEnergyCutoff;

    public int offspringCount;


    /* To implement
        acceleration?
        maxSize
    */

    // The ratio of base energy that can be spent to stay alive - anything under this ratio and the creature dies
    public const float BASE_ENERGY_DEATH_RATIO = 0.3f;

    // Higher numbers are faster
    public const float REGENERATION_CONSTANT = 0.1f;

    // The energy cost of movement
    public const float MOVEMENT_CONSTANT = 0.2f;

    // The angular rotation speed multiplier compared to speed
    public const float ROTATION_CONSTANT = 100.0f;

    // The speed constant of food digestion
    public const float DIGESTION_CONSTANT = 0.01f;

    // The minimum energy which will be maintained by sacrificing health
    [HideInInspector] public float energyDeficiencyPoint;

    private NavMeshAgent agent;
    private WorldManager world;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        world = FindObjectOfType<WorldManager>();

        // Bodily constants
        maxSize = 1.0f;
        growthEnergyCost = maxSize * maxSize * maxSize * 30.0f;
        offspringEnergyCutoff = growthEnergyCost / 2;
        growthEnergySpent = offspringEnergyCutoff;

        // Set starting size based on bodily constants
        updateSize();

        // Starting stats
        health = maxHealth;
        food = stomachCapacity * WELL_FED_CONSTANT;
        age = 0.0f;
        mature = false;
        offspringCount = 0;

        gameObject.name = "Creature (Gen " + generation + ")";

        StartCoroutine(IncreaseAge());
    }

    // Update is called once per frame
    void Update()
    {
        if (health < 0) { Die(); }

        /*********** Determine body states **************/
        hungry = (food / stomachCapacity) < HUNGER_CONSTANT;
        healthy = health == maxHealth;

        // Assume worst for creature
        agent.speed = 0.0f;
        agent.angularSpeed = 0.0f;

        /*********** Budget energy based on the creature's necessities **************/
        currentEnergyLevel = generateEnergy();
        float currentRemainingEnergy = currentEnergyLevel;

        // Expend minimum required energy
        float minimumEnergyRequirement = baseEnergyExpense * BASE_ENERGY_DEATH_RATIO;
        currentRemainingEnergy -= minimumEnergyRequirement;

        // Expend remaining energy on movement and growth
        concious = currentRemainingEnergy > 0;
        if (concious) {
            // Priority is on creature's movement when injured and on growth when healthy
            if (starving) {
                expendMovementEnergy(currentRemainingEnergy);
            } else {
                // Find a ratio between movement and growth based on creature's health
                float movementVersusGrowthRatio = currentRemainingEnergy * WELL_FED_CONSTANT / normalEnergyLevel;
                float growthEnergyBudget = currentRemainingEnergy * movementVersusGrowthRatio;
                float movementEnergyBudget = currentRemainingEnergy - growthEnergyBudget;

                expendGrowthEnergy(growthEnergyBudget);
                expendMovementEnergy(movementEnergyBudget);
            }
        }
    }

    // A less frequently ran update method
    IEnumerator IncreaseAge() {
        while (true) {
            // Update body limits based on new size
            if (!mature) {
                updateSize();
            }
            age++;
            yield return new WaitForSeconds(1);
        }
    }

    public void increaseGeneration(int gen) {
        generation = gen + 1;
    }

    public void setGrowthEnergySpent(float energy) {
        growthEnergySpent = energy;
    }

    private void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag.Equals("Food")) {
            Destroy(collider.gameObject);
            Eat();
        }
    }

    public void Eat() {
        food = Mathf.Min(food + WorldManager.FOOD_NUTRITION, stomachCapacity);
    }

    private void Die() {
        Destroy(gameObject);
    }

    private void updateSize() {
        size = growthEnergySpent / growthEnergyCost * maxSize;
        mass = size * size * size;

        normalEnergyLevel = size;   
        baseEnergyExpense = size / 3;
        energyDeficiencyPoint = normalEnergyLevel * 0.5f;             
        maxHealth = mass * 20.0f;
        stomachCapacity = mass * 50.0f;

        transform.localScale = new Vector3(size, size, size);
    }

    // Uses food or health to generate creature energy
    private float generateEnergy() {
        // Well fed buff
        food = Math.Max(food - stomachCapacity * DIGESTION_CONSTANT * Time.deltaTime, 0);
        float energyLevel;
        if (food > stomachCapacity * WELL_FED_CONSTANT) {
            // Lerp between full energy and a little bonus energy when full
            energyLevel = normalEnergyLevel * (food / (stomachCapacity * WELL_FED_CONSTANT));

        // Starvation penalty
        } else if (food < stomachCapacity * STARVATION_CONSTANT) {
            // Lerps between full and no energy when starving
            // TODO: Go back to 50% minimum energy
            energyLevel = normalEnergyLevel * (food / (stomachCapacity * STARVATION_CONSTANT));

        // Normal enery levels
        } else {
            energyLevel = normalEnergyLevel;
        }

        starving = energyLevel < energyDeficiencyPoint;
        // If needed, maintain base energy levels by sacrificing the body (health) for energy
        if (starving) {
            float healthEnergy = energyDeficiencyPoint - energyLevel;
            health -= healthEnergy * Time.deltaTime;
            energyLevel += healthEnergy;
        }

        return energyLevel;
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
        growthEnergySpent += energyBudget * Time.deltaTime;
        if (mature) {
            if (growthEnergySpent >= offspringEnergyCutoff) {
                GameObject offspring = Instantiate(world.creature, transform.position, Quaternion.identity);
                Biology offspringBio = offspring.GetComponent<Biology>();
                offspringBio.increaseGeneration(generation);
                offspringBio.setGrowthEnergySpent(offspringEnergyCutoff);
                //growthEnergySpent = 0.0f;
                offspringCount++;
            }
        } else {
            if (growthEnergySpent > growthEnergyCost) {
                mature = true;
                growthEnergySpent = 0.0f;
            }
        }
    }

    // Calculates how fast the creature can move to exactly expend the energy budget
    private void expendMovementEnergy(float energyBudget) {
        // Solve for speed given formula: movementEnergy = mass * speed * speed * MOVEMENT_CONSTANT;
        float baseSpeed = (float) Math.Sqrt(energyBudget / (mass * MOVEMENT_CONSTANT));
        float injuryMultiplier = health / maxHealth;
        agent.speed = baseSpeed * injuryMultiplier;
        agent.angularSpeed = baseSpeed * ROTATION_CONSTANT * injuryMultiplier;
    }
}

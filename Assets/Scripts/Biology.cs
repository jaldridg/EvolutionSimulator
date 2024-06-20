using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

// Defines the creature's needs and how they affect the creature
public class Biology : MonoBehaviour
{
    /*** Creature states ***/
    public bool hungry;
    public bool starving;
    public bool concious;
    public bool healthy;
    public bool mature;


    /*** Biological constants used for balancing ***/
    // The speed constant of food digestion
    public static float DIGESTION_CONSTANT = 0.01f;
    // The percent fullness of the stomach where creatures are well fed
    public static float WELL_FED_CONSTANT = 0.8f;
    // The percent fullness of the stomach where creatures will begin looking for food
    public static float HUNGER_CONSTANT = 0.5f;

    // The percent fullness of the stomach where creatures will have negative effects from hunger
    public static float STARVATION_CONSTANT = 0.25f;

    // The ratio of base energy that can be spent to stay alive - anything under this ratio and the creature dies
    public static float BASE_ENERGY_DEATH_RATIO = 0.5f;

    // The energy cost of movement which is multiplied with speed
    public static float MOVEMENT_CONSTANT = 0.15f;
    // A flat energy cost of movement based on distance travelled
    // Prevents small and super fast creatures from evolving
    public static float FRICTION_CONSTANT = 0.025f;
    // The angular rotation speed multiplier compared to speed
    public static float ROTATION_CONSTANT = 100.0f;

    public static Color MATURE_BODY_COLOR = new Color (0.02f, 0.71f, 0.86f);


    /*** Creature variables ***/
    [HideInInspector] public float maxHealth;
    public float health;

    [HideInInspector] public float stomachCapacity;
    public float food;

    [HideInInspector] public float normalEnergyLevel;
    public float currentEnergyLevel;

    public float mass;
    public float size;


    // The total energy the creature needs to spend to mature or reproduce
    public float growthEnergyCost;
    // The amount of energy expended towards maturity or the next offspring
    public float growthEnergySpent;

    // The size of the offspring in the creature's body
    public float offspringMass;
    public int offspringCount;

    public int generation;
    public float age;
    // The time spent since maturing
    public float timeMature;


    /*** Mutable traits which change each offspring to evolve ***/
    // The minimum energy which will be maintained by sacrificing health
    [HideInInspector] public float energyDeficiencyPoint;
    // The ratio of energy a creature must spend per second no matter what it's doing based on its size
    // This determines how much energy is put into sensing the environment and intelligent behavior
    [HideInInspector] public float baseEnergyRatio;

    // The mass / maxMass ratio at which a creature will be born
    // Less mass will result in premature offspring which may have difficulty surviving
    public float offspringMassRatio;

    // The energy cost of growing a cubic unit of creature
    public float bodySizeEnergyCost;

    public float maxSize;

    public float maxMass;


    /*** UI variables ***/
    // Health change per second
    [HideInInspector] public float healthDelta;
    [HideInInspector] public float foodDelta;


    /*** Unity components ***/
    private NavMeshAgent agent;
    private WorldManager world;

    private Material bodyMaterial;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        world = FindObjectOfType<WorldManager>();
        bodyMaterial = transform.GetChild(0).GetComponent<Renderer>().material;

        // Bodily constants
        maxSize = Evolution.STARTING_CREATURE_MAX_SIZE;
        bodySizeEnergyCost = Evolution.STARTING_CREATURE_BODY_SIZE_ENERGY_COST;
        offspringMassRatio = Evolution.STARTING_CREATURE_OFFSPRING_MASS_RATIO;
        maxMass = maxSize * maxSize * maxSize;
        growthEnergyCost = maxMass * bodySizeEnergyCost;
        growthEnergySpent = offspringMassRatio * growthEnergyCost;

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

        healthDelta = 0;
        foodDelta = 0;

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
        float minimumEnergyRequirement = normalEnergyLevel * baseEnergyRatio;
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
                bodyMaterial.color = Color.Lerp(Color.white, MATURE_BODY_COLOR, growthEnergySpent / growthEnergyCost);
            } else {
                timeMature += 0.1f;
                float colorDarkening = timeMature / 1000.0f;
                bodyMaterial.color = MATURE_BODY_COLOR - new Color(0.0f, colorDarkening, colorDarkening);
            }
            age += 0.1f;
            yield return new WaitForSeconds(0.1f);
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
        mass = growthEnergySpent / growthEnergyCost * maxSize;
        size = (float) Math.Pow(mass, 1.0f/3.0f);

        normalEnergyLevel = size;   
        energyDeficiencyPoint = normalEnergyLevel * 0.5f;
        float originalMaxHealth = maxHealth;
        maxHealth = mass * 20.0f;
        // Keep health percent constant
        health *= maxHealth / originalMaxHealth;
        stomachCapacity = mass * 75.0f;

        transform.localScale = new Vector3(size, size, size);
    }

    // Uses food or health to generate creature energy
    private float generateEnergy() {
        // Well fed buff
        food = Math.Max(food - stomachCapacity * DIGESTION_CONSTANT * Time.deltaTime, 0);
        foodDelta -= stomachCapacity * DIGESTION_CONSTANT;
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
            healthDelta -= healthEnergy;
            energyLevel += healthEnergy;
        }

        return energyLevel;
    }

    // Regenerates, matures, or reproduces given an amoutn of energy
    private void expendGrowthEnergy(float energyBudget) {
        // For now split energy equally
        if (health < maxHealth) {
            float developmentBudget = energyBudget / 2;
            float regenerationBudget = energyBudget / 2;

            health += developmentBudget * Time.deltaTime;
            healthDelta += developmentBudget;
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
            // Determine the mass of the offspring given the energy spent on it
            float offspringMass = growthEnergySpent / bodySizeEnergyCost;
            if (offspringMass >= maxMass * offspringMassRatio) {
                GameObject offspring = Instantiate(world.creature, transform.position, Quaternion.identity);
                Biology offspringBio = offspring.GetComponent<Biology>();
                offspringBio.increaseGeneration(generation);
                offspringBio.setGrowthEnergySpent(offspringMassRatio * offspringMassRatio * offspringMassRatio);
                growthEnergySpent = 0.0f;
                offspringCount++;
            }
        } else {
            if (growthEnergySpent > growthEnergyCost) {
                // Make sure max variables are set - they can get off from rounding errors
                size = maxSize;
                mass = size * size * size;
                mature = true;
                growthEnergySpent = 0.0f;
            }
        }
    }

    // Calculates how fast the creature can move to exactly expend the energy budget
    private void expendMovementEnergy(float energyBudget) {
        // Solve for speed given formula: movementEnergy = mass * speed * speed * MOVEMENT_CONSTANT + speed * FRICTION_CONSTANT;
        // Using quadratic formula
        float determinant = (float) Math.Sqrt(FRICTION_CONSTANT * FRICTION_CONSTANT + 4 * mass * MOVEMENT_CONSTANT * energyBudget);
        float baseSpeed = (-FRICTION_CONSTANT + determinant) / (2 * mass * MOVEMENT_CONSTANT);
        float injuryMultiplier = health / maxHealth;
        agent.speed = baseSpeed * injuryMultiplier;
        agent.angularSpeed = baseSpeed * ROTATION_CONSTANT * injuryMultiplier;
    }
}

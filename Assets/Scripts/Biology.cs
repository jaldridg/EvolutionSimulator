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

    // The multiplier for vision distance given brain size
    public static float VISION_CONSTANT = 0.5f;

    // The energy cost of movement which is multiplied with speed
    public static float MOVEMENT_CONSTANT = 0.15f;

    // The angular rotation speed multiplier compared to speed
    public static float ROTATION_CONSTANT = 100.0f;

    // Determines the sum of health, stomach, and brain
    // In other words BSPB = maxHealth + stomachCapacity + brainSize;
    public static float BODY_SPACE_PACKING_BUDGET = 100.0f;

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

    public float brainSize;


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
    // The minimum energy ratio which is maintained by sacrificing health
    [HideInInspector] public float energyDeficiencyRatio;
    // The ratio of energy a creature must spend per second no matter what it's doing based on its size
    // This determines how much energy is put into sensing the environment and intelligent behavior
    [HideInInspector] public float baseEnergyRatio;

    // The mass / maxMass ratio at which a creature will be born
    // Less mass will result in premature offspring which may have difficulty surviving
    public float offspringMassRatio;

    // The weight of spending energy on reproducing as opposed to regeneration
    // 1.0 fully prioritizes reproducing while 0.0 prioritizes regeneration
    public float offspringToRegenerationWeight;

    // The energy cost of growing a cubic unit of creature
    public float bodySizeEnergyCost;

    // These three variables determine the ratio of the body used for certain functions
    // Higher weight in a category means the creature can store more food, see better, or has more health
    // Since these are normalized, strength in one trait means weakness in the others
    public float bodySpaceStomachWeight;
    public float bodySpaceBrainWeight;
    public float bodySpaceHealthWeight;

    public float maxSize;

    public float maxMass;


    /*** UI variables ***/
    // Health change per second
    [HideInInspector] public float healthDelta;
    // Food level change per second
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
        maxSize = Evolution.STARTING_MAX_SIZE;
        bodySizeEnergyCost = Evolution.STARTING_BODY_SIZE_ENERGY_COST;
        energyDeficiencyRatio = Evolution.STARTING_ENERGY_DEFICIENCY_RATIO;
        offspringMassRatio = Evolution.STARTING_OFFSPRING_MASS_RATIO;
        offspringToRegenerationWeight = Evolution.STARTING_OFFSPRING_TO_REGENERATION_WEIGHT;
        bodySpaceStomachWeight = Evolution.STARTING_BODY_SPACE_STOMACH_WEIGHT;
        bodySpaceBrainWeight = Evolution.STARTING_BODY_SPACE_BRAIN_WEIGHT;
        bodySpaceHealthWeight = Evolution.STARTING_BODY_SPACE_HEALTH_WEIGHT;
        baseEnergyRatio = bodySpaceBrainWeight / BODY_SPACE_PACKING_BUDGET;
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
        float originalMaxHealth = maxHealth;
        maxHealth = mass * bodySpaceHealthWeight;
        // Keep health percent constant
        health *= maxHealth / originalMaxHealth;
        stomachCapacity = mass * bodySpaceStomachWeight;

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

        starving = energyLevel / normalEnergyLevel < energyDeficiencyRatio;
        // If needed, maintain base energy levels by sacrificing the body (health) for energy
        if (starving) {
            float energyDeficit = energyDeficiencyRatio * normalEnergyLevel - energyLevel;
            health -= energyDeficit * Time.deltaTime;
            healthDelta -= energyDeficit;
            energyLevel += energyDeficit;
        }

        return energyLevel;
    }

    // Regenerates, matures, or reproduces given an amoutn of energy
    private void expendGrowthEnergy(float energyBudget) {
        // For now split energy equally
        if (health < maxHealth) {
            float offspringBudget = energyBudget  * offspringToRegenerationWeight;
            float regenerationBudget = energyBudget * (1 - offspringToRegenerationWeight);

            health += regenerationBudget * Time.deltaTime;
            healthDelta += regenerationBudget;
            expendDevelopmentEnergy(offspringBudget);
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
                mass = maxMass;
                mature = true;
                growthEnergySpent = 0.0f;
            }
        }
    }

    // Calculates how fast the creature can move to exactly expend the energy budget
    private void expendMovementEnergy(float energyBudget) {
        // Solve for speed given formula: movementEnergy = mass * speed * speed * MOVEMENT_CONSTANT + speed * FRICTION_CONSTANT;
        // Using quadratic formula
        float fc = WorldManager.FRICTION_CONSTANT;
        float determinant = (float) Math.Sqrt(fc * fc + 4 * mass * MOVEMENT_CONSTANT * energyBudget);
        float baseSpeed = (-fc + determinant) / (2 * mass * MOVEMENT_CONSTANT);
        float injuryMultiplier = health / maxHealth;
        agent.speed = baseSpeed * injuryMultiplier;
        agent.angularSpeed = baseSpeed * ROTATION_CONSTANT * injuryMultiplier;
    }
}

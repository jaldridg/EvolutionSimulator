using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Defines the creature's needs and how they affect the creature
[System.Serializable]
public class Biology : MonoBehaviour
{

    /*** Biological constants used for balancing ***/
    // The ratio of the food in the stomach which gets used to generate energy per second
    public static float DIGESTION_CONSTANT = 0.01f;
    // The percent fullness of the stomach where creatures are well fed
    public static float WELL_FED_CONSTANT = 0.8f;
    // The percent fullness of the stomach where creatures will begin looking for food
    public static float HUNGER_CONSTANT = 0.5f;

    // The percent fullness of the stomach where creatures will have negative effects from hunger
    public static float STARVATION_CONSTANT = 0.25f;

    // The ratio of base energy that can be spent to stay alive - anything under this ratio and the creature dies
    public static float BASE_ENERGY_DEATH_RATIO = 50.0f;

    // The multiplier for vision distance given brain size
    public static float VISION_CONSTANT = 20.0f;

    // The higher the number, the less energy required to move
    public static float MOVEMENT_CONSTANT = 0.15f;

    // The angular rotation speed multiplier compared to speed
    public static float ROTATION_CONSTANT = 100.0f;

    // Determines the sum of health, stomach, and brain
    // In other words BSPB = maxHealth + stomachCapacity + brainSize;
    public static float BODY_SPACE_PACKING_BUDGET = 100.0f;

    // The energy cost of growing a cubic unit of the following body parts

    public static float BRAIN_ENERGY_COST = 300.0f;
    public static float BODY_ENERGY_COST = 100.0f;
    public static float STOMACH_ENERGY_COST = 50.0f;

    public static Color MATURE_BODY_COLOR = new Color (0.02f, 0.71f, 0.86f);
    

    /*** Creature states ***/
    [Header("General Stats")]
    public bool concious;
    public bool healthy;
    public bool hungry;
    public bool starving;
    public bool mature;


    /*** Creature variables ***/
    [Space(10)]
    [Header("Detailed Stats")]
    public float health;
    [HideInInspector] public float maxHealth;

    public float food;
    // Measured in kilograms
    [HideInInspector] public float stomachCapacity;

    // The average nutrition in the creatue's stomach
    public float foodNutrition;

    [HideInInspector] public float normalEnergyLevel;
    public float currentEnergyLevel;

    public float mass;
    public float size;

    // The total energy the creature needs to spend to mature or reproduce
    public float growthEnergyCost;
    // The amount of energy expended towards maturity or the next offspring (not including food for new creature)
    public float growthEnergySpent;

    public float totalOffspringEnergyCost;

    // The size of the offspring in the creature's body
    public int offspringCount;

    public int generation;
    public float age;
    // The time spent since maturing
    private float timeMature;


    /*** Mutable traits which change each offspring to evolve ***/
    // The minimum energy ratio which is maintained by sacrificing health
    [Space(10)]
    [Header("Genes")]
    // The mass / maxMass ratio at which a creature will be born
    // Less mass will result in premature offspring which may have difficulty surviving
    public float offspringMassRatio;

    // The ratio of food which offspring will have when born
    public float offspringFoodRatio;

    // The weight of spending energy on reproducing as opposed to regeneration
    // 1.0 fully prioritizes reproducing while 0.0 prioritizes regeneration
    public float offspringToRegenerationWeight;
    // The energy ratio which will be maintained by sacrificing health
    public float energyDeficiencyRatio;

    // These three variables determine the ratio of the body used for certain functions
    // Higher weight in a category means the creature can store more food, see better, or has more health
    // Since these are normalized, strength in one trait means weakness in the others
    public float bodySpaceStomachRatio;
    public float bodySpaceBrainRatio;
    public float bodySpaceHealthRatio;

    public float maxSize;

    public float maxMass;


    /*** UI variables ***/
    // Health change per second
    [HideInInspector] public float healthDelta;
    // Food level change per second
    [HideInInspector] public float foodDelta;

    [HideInInspector] public float speed;

    [HideInInspector] public float currentBaseEnergyExpenditure;
    [HideInInspector] public float currentMovementEnergyExpenditure;
    [HideInInspector] public float currentReproductionEnergyExpenditure;
    [HideInInspector] public float currentRegenerationEnergyExpenditure;


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
        if (generation == 1) {
            maxSize = Evolution.STARTING_MAX_SIZE;
            energyDeficiencyRatio = Evolution.STARTING_ENERGY_DEFICIENCY_RATIO;
            offspringMassRatio = Evolution.STARTING_OFFSPRING_MASS_RATIO;
            offspringFoodRatio = Evolution.STARTING_OFFSPRING_FOOD_RATIO;
            offspringToRegenerationWeight = Evolution.STARTING_OFFSPRING_TO_REGENERATION_WEIGHT;
            bodySpaceStomachRatio = Evolution.STARTING_BODY_SPACE_STOMACH_RATIO;
            bodySpaceBrainRatio = Evolution.STARTING_BODY_SPACE_BRAIN_RATIO;
            bodySpaceHealthRatio = Evolution.STARTING_BODY_SPACE_HEALTH_RATIO;
        }

        maxMass = maxSize * maxSize * maxSize;
        growthEnergyCost = calculateGrowthEnergyCost(maxMass);
        growthEnergySpent = offspringMassRatio * growthEnergyCost;
        totalOffspringEnergyCost = calculateTotalOffspringEnergy();

        // Set starting size based on bodily constants
        updateSize();

        // Starting stats
        health = maxHealth;
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
        currentBaseEnergyExpenditure = normalEnergyLevel * bodySpaceBrainRatio;
        currentRemainingEnergy -= currentBaseEnergyExpenditure;

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
                currentMovementEnergyExpenditure = currentRemainingEnergy - growthEnergyBudget;

                expendGrowthEnergy(growthEnergyBudget);
                expendMovementEnergy(currentMovementEnergyExpenditure);
            }
        }
    }

    // A less frequently ran update method
    IEnumerator IncreaseAge() {
        float timeDelta = 0.1f;
        while (true) {
            // Update body limits based on new size
            if (!mature) {
                updateSize();
                bodyMaterial.color = Color.Lerp(Color.white, MATURE_BODY_COLOR, growthEnergySpent / growthEnergyCost);
            } else {
                timeMature += timeDelta;
                float tempTerm = 1000 * Mathf.Pow((float) Math.E, -0.007f * age);
                float colorDarkening = 1 / (2 + tempTerm);
                bodyMaterial.color = MATURE_BODY_COLOR - new Color(0.0f, colorDarkening, colorDarkening);
            }
            age += timeDelta;
            yield return new WaitForSeconds(timeDelta);
        }
    }

    public void increaseGeneration(int gen) {
        generation = gen + 1;
    }

    public void setGrowthEnergySpent(float energy) {
        growthEnergySpent = energy;
    }

    public void setStomach(float foodLevel, float nutrition) {
        food = foodLevel;
        foodNutrition = nutrition;
    }

    public void mutateGenes(Biology offspringBio) {
        offspringBio.maxSize = Evolution.mutatePositiveValue(maxSize);
        offspringBio.offspringMassRatio = Evolution.mutateRatio(offspringMassRatio);
        offspringBio.offspringFoodRatio = Evolution.mutateRatio(offspringFoodRatio);
        offspringBio.energyDeficiencyRatio = Evolution.mutateRatio(energyDeficiencyRatio);
        offspringBio.offspringToRegenerationWeight = Evolution.mutateRatio(offspringToRegenerationWeight);

        float[] newBodySpaceRatios = Evolution.mutateRatioThree(new float[] {bodySpaceBrainRatio, bodySpaceHealthRatio, bodySpaceStomachRatio});
        offspringBio.bodySpaceBrainRatio = newBodySpaceRatios[0];
        offspringBio.bodySpaceHealthRatio = newBodySpaceRatios[1];
        offspringBio.bodySpaceStomachRatio = newBodySpaceRatios[2];
    }

    public float calculateGrowthEnergyCost(float maximumMass) {
        float brainCost = bodySpaceBrainRatio * BRAIN_ENERGY_COST;
        float healthCost = bodySpaceHealthRatio * BODY_ENERGY_COST;
        float stomachCost = bodySpaceStomachRatio * STOMACH_ENERGY_COST;
        return (brainCost + healthCost + stomachCost) * maximumMass;
    }

    public float calculateOffspringStartingFood() {
        float desiredOffspringMass = maxMass * offspringMassRatio;
        return desiredOffspringMass * bodySpaceStomachRatio * BODY_SPACE_PACKING_BUDGET * offspringFoodRatio * 10 /* 10x to convert to kg */;
    }

    public float calculateTotalOffspringEnergy() {
        float desiredOffspringMass = maxMass * offspringMassRatio;
        return calculateGrowthEnergyCost(desiredOffspringMass) + calculateOffspringStartingFood();
    }

    private void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag.Equals("Food")) {
            Destroy(collider.gameObject);
            Eat(collider.gameObject);
        }
    }

    public void Eat(GameObject foodObject) { 
        Fruit fruit = foodObject.GetComponent<Fruit>();
        float foodEaten = Mathf.Min(fruit.mass, stomachCapacity - food);
        float newFood = food + foodEaten;
        foodNutrition = (food * foodNutrition + foodEaten * fruit.nutrition) / newFood;
        food += foodEaten;
    }

    private void Die() {
        Destroy(gameObject);
        WorldManager.creatureCount--;
    }

    private void updateSize() {
        mass = growthEnergySpent / growthEnergyCost * maxMass;
        size = (float) Math.Pow(mass, 1.0f/3.0f);

        normalEnergyLevel = size * size;   
        float originalMaxHealth = maxHealth;
        maxHealth = mass * bodySpaceHealthRatio * BODY_SPACE_PACKING_BUDGET;
        // Keep health percent constant
        health *= maxHealth / originalMaxHealth;
        stomachCapacity = mass * bodySpaceStomachRatio * BODY_SPACE_PACKING_BUDGET * 10 /* convert to kg by doing x10 */;

        transform.localScale = new Vector3(size, size, size);
    }

    // Uses food or health to generate creature energy
    private float generateEnergy() {
        // Digest food, and reward larger, energy efficient creatures
        float sizeEfficiency = (float) Math.Sqrt(maxSize);
        float digestionRate = stomachCapacity * DIGESTION_CONSTANT / sizeEfficiency;
        food = Math.Max(food - digestionRate * Time.deltaTime, 0);
        foodDelta = food == 0 ? 0 : -digestionRate;

        // Well fed buff
        float energyLevel;
        if (food > stomachCapacity * WELL_FED_CONSTANT) {
            // Lerp between full energy and a little bonus energy when full
            energyLevel = normalEnergyLevel * (food / (stomachCapacity * WELL_FED_CONSTANT));

        // Starvation penalty
        } else if (food < stomachCapacity * STARVATION_CONSTANT) {
            // Lerps between full and no energy when starving
            energyLevel = normalEnergyLevel * (food / (stomachCapacity * STARVATION_CONSTANT));

        // Normal enery levels
        } else {
            energyLevel = normalEnergyLevel;
        }

        // Food nutrition multiplier
        energyLevel *= foodNutrition;

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

    // Regenerates, matures, or reproduces given an amount of energy
    private void expendGrowthEnergy(float energyBudget) {
        if (health < maxHealth) {
            currentReproductionEnergyExpenditure = energyBudget  * offspringToRegenerationWeight;
            currentRegenerationEnergyExpenditure = energyBudget * (1 - offspringToRegenerationWeight);

            health += currentRegenerationEnergyExpenditure * Time.deltaTime;
            healthDelta += currentRegenerationEnergyExpenditure;
            expendDevelopmentEnergy(currentReproductionEnergyExpenditure);
        } else {
            currentReproductionEnergyExpenditure = energyBudget;
            currentRegenerationEnergyExpenditure = 0.0f;
            expendDevelopmentEnergy(energyBudget);
        }
    }

    // Put energy towards maturing or reproducing
    private void expendDevelopmentEnergy(float energyBudget) {
        // Reproduce if enough energy has been put into it
        growthEnergySpent += energyBudget * Time.deltaTime;
        if (mature) {
            if (growthEnergySpent >= totalOffspringEnergyCost) {
                GameObject offspring = Instantiate(world.creature, transform.position, Quaternion.identity);
                Biology offspringBio = offspring.GetComponent<Biology>();
                mutateGenes(offspringBio);
                offspringBio.increaseGeneration(generation);
                offspringBio.setGrowthEnergySpent(offspringMassRatio * offspringMassRatio * offspringMassRatio);
                offspringBio.setStomach(calculateOffspringStartingFood(), 1.0f);
                growthEnergySpent = 0.0f;
                offspringCount++;
                WorldManager.creatureCount++;
            }
        } else {
            if (growthEnergySpent > growthEnergyCost) {
                // Make sure max variables are set - they can get off from rounding errors
                size = maxSize;
                mass = maxMass;
                maxHealth = mass * bodySpaceHealthRatio * BODY_SPACE_PACKING_BUDGET;
                stomachCapacity = mass * bodySpaceStomachRatio * BODY_SPACE_PACKING_BUDGET * 10 /* convert to kg by doing x10 */;
                mature = true;
                growthEnergySpent -= growthEnergyCost;
            }
        }
    }

    // Calculates how fast the creature can move to exactly expend the energy budget
    private void expendMovementEnergy(float energyBudget) {
        // Solve for speed given formula: movementEnergy = mass * speed * speed * MOVEMENT_CONSTANT + speed * FRICTION_CONSTANT;
        // Using quadratic formul
        float fc = WorldManager.FRICTION_CONSTANT;
        float determinant = (float) Math.Sqrt(fc * fc + 4 * mass * MOVEMENT_CONSTANT * energyBudget);
        float baseSpeed = (-fc + determinant) / (2 * mass * MOVEMENT_CONSTANT);
        float injuryMultiplier = health / maxHealth;
        agent.speed = baseSpeed * injuryMultiplier;
        agent.angularSpeed = baseSpeed * ROTATION_CONSTANT * injuryMultiplier;
        speed = agent.speed;
    }
}

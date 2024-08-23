using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class WorldManager : MonoBehaviour
{
    public GameObject creature;
    public GameObject food;

    // The number of world units given to each starting creature
    public static float SPAWN_SPARCITY = 100.0f;

    // Food spawns per second in the world
    public static float FOOD_RATE = 1.0f;

    // How much energy can be extracted from on cubic meter of food
    public static float FOOD_ENERGY_CONSTANT = 200.0f;

    // A flat energy cost of movement based on distance travelled
    // Prevents small and super fast creatures from evolving
    public static float FRICTION_CONSTANT = 0.0025f;
    
    // Note: planeScale is not tied to ingame size
    private static float planeScale;

    [SerializeField] public static float creatureCount;

    [SerializeField] public static float simulationTime;

    // Start is called before the first frame update
    void Start()
    {
        SpawnCreatures();
        StartCoroutine(SpawnFood());
        StartCoroutine(CheckForExtinction());
    }

    // Update is called once per frame
    void Update() { 
        simulationTime += Time.deltaTime;
    }

    // Assumes the world is a square plane
    public static Vector3 GetRandomWorldPosition() {
        float edge = planeScale * 5 - 1;
        float x = Random.Range(-edge, edge);
        float z = Random.Range(-edge, edge);
        return new Vector3(x, 0.0f, z);
    }

    public static Vector3 GetNearbyWorldPosition(Vector3 currentPosition) {
        float radius = 10.0f;
        Vector2 randomDirection = Random.insideUnitCircle * radius;
        Vector3 newPosition = currentPosition + new Vector3(randomDirection.x, 0.0f, randomDirection.y);
        NavMeshHit hit;
        NavMesh.SamplePosition(newPosition, out hit, radius, 1);
        return hit.position;
    }

    public void SpawnCreatures() {
        planeScale = gameObject.transform.localScale.x;
        float worldArea = (planeScale * 5) * (planeScale * 5);

        float startingSize = Evolution.STARTING_MAX_SIZE;
        float startingMass = startingSize * startingSize * startingSize * Evolution.STARTING_OFFSPRING_MASS_RATIO;
        float foodPerMass = Evolution.STARTING_BODY_SPACE_STOMACH_RATIO * Biology.BODY_SPACE_PACKING_BUDGET * 10 /* 10x to convert to kg */;
        float startingFood = startingMass * foodPerMass * Evolution.STARTING_OFFSPRING_FOOD_RATIO;

        //for (int i = 0; i < worldArea / SPAWN_SPARCITY; i++) {
            GameObject startingCreature = Instantiate(creature, GetRandomWorldPosition(), Quaternion.identity);
            Biology bio = startingCreature.GetComponent<Biology>();
            bio.increaseGeneration(0);
            bio.setStomach(startingFood, 1.0f);
            startingCreature.name = "Creature (Gen " + bio.generation + ")";
            creatureCount++;
        //}
    }

    IEnumerator CheckForExtinction() {
        while (true) {
        if (creatureCount == 0) {
            SpawnCreatures();
            simulationTime = 0;
        }
        yield return new WaitForSeconds(10);
        }
    }

    IEnumerator SpawnFood() {
        while (true) {
            float radius = (0.6f + Random.Range(-0.1f, 0.1f)) / 2;
            Vector3 foodPos = GetRandomWorldPosition() + Vector3.up * radius;
            GameObject fruitObject = Instantiate(food, foodPos, Quaternion.identity);
            food.transform.GetChild(0).transform.localScale = new Vector3(radius, radius, radius);

            Fruit fruit = fruitObject.GetComponent<Fruit>();
            float nutrition = Random.Range(0.8f, 1.2f);
            fruit.setStats(radius, nutrition);
            
            yield return new WaitForSeconds(1 / FOOD_RATE);
        }
    }
}

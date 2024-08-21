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

    // The rate that food spawns in the world (food spawns per second)
    public static float FOOD_RATE = 1.0f;

    // How much food satisfies a creature's need to eat
    public static float FOOD_NUTRITION = 30.0f;

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
        for (int i = 0; i < worldArea / SPAWN_SPARCITY; i++) {
            GameObject startingCreature = Instantiate(creature, GetRandomWorldPosition(), Quaternion.identity);
            Biology bio = startingCreature.GetComponent<Biology>();
            bio.increaseGeneration(0);
            startingCreature.name = "Creature (Gen " + bio.generation + ")";
            creatureCount++;
        }
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
            float foodSize = food.transform.GetChild(0).transform.localScale.y;
            Vector3 foodPos = GetRandomWorldPosition() + Vector3.up * (foodSize / 2);
            Instantiate(food, foodPos, Quaternion.identity);
            yield return new WaitForSeconds(1 / FOOD_RATE);
        }
    }
}

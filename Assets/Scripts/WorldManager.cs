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

    // The rate that hunger goes down per second
    public const float HUNGER_DECAY = 3.0f;

    // The number of world units given to each starting creature
    public const float SPAWN_SPARCITY = 50.0f;

    // The rate that food spawns in the world (food spawns per second)
    public const float FOOD_RATE = 0.5f;
    
    // Note: planeScale is not tied to ingame size
    private static float planeScale;

    // Start is called before the first frame update
    void Start()
    {
        planeScale = gameObject.transform.localScale.x;
        float worldArea = (planeScale * 5) * (planeScale * 5);

        for (int i = 0; i < worldArea / SPAWN_SPARCITY; i++) {
            Instantiate(creature, GetRandomWorldPosition(), Quaternion.identity);
        }
        StartCoroutine(SpawnFood());
    }

    // Update is called once per frame
    void Update()
    {
        
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

    IEnumerator SpawnFood() {
        while (true) {
            float foodSize = food.transform.GetChild(0).transform.localScale.y;
            Vector3 foodPos = GetRandomWorldPosition() + Vector3.up * (foodSize / 2);
            Instantiate(food, foodPos, Quaternion.identity);
            yield return new WaitForSeconds(1 / FOOD_RATE);
        }
    }
}

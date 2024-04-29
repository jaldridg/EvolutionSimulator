using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public GameObject creature;
    public GameObject food;

    // The rate that hunger goes down per second
    public const float HUNGER_DECAY = 4.0f;
    
    // Note: planeScale is not tied to ingame size
    private const float PLANE_SCALE = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 10; i++) {
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
        float edge = PLANE_SCALE * 5 - 1;
        float x = Random.Range(-edge, edge);
        float z = Random.Range(-edge, edge);
        return new Vector3(x, 0.0f, z);
    }

    IEnumerator SpawnFood() {
        while (true) {
            float foodSize = food.transform.GetChild(0).transform.localScale.y;
            Vector3 foodPos = GetRandomWorldPosition() + Vector3.up * (foodSize / 2);
            Instantiate(food, foodPos, Quaternion.identity);
            yield return new WaitForSeconds(3);
        }
    }
}

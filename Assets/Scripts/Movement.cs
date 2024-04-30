using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
    public NavMeshSurface navMesh;

    private Biology body;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        body = GetComponent<Biology>();
        agent.destination = WorldManager.GetNearbyWorldPosition(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        // Move to new locations over and over
        if (agent.remainingDistance < agent.stoppingDistance + 0.5f) {
            Vector3 waypoint;
            // Look for food if hungry
            if (body.food < body.maxFood / 2) {
                GameObject closestFood = FindClosestFood();
                if (closestFood == null) {
                    waypoint = WorldManager.GetNearbyWorldPosition(transform.position);
                } else {
                    waypoint = closestFood.transform.position;
                }

            // Wander
            } else {
                waypoint = WorldManager.GetNearbyWorldPosition(transform.position);
            }
            agent.destination = waypoint;
        }
    }

    private GameObject FindClosestFood() {
        GameObject[] food = GameObject.FindGameObjectsWithTag("Food");
        if (food.Length == 0) {
            return null;
        }

        float bestDistance = float.PositiveInfinity;
        GameObject closestFood = null;
        for (int i = 0; i < food.Length - 1; i++) {
            float testDistance = Vector3.Distance(food[i].transform.position, transform.position);
            if (testDistance < bestDistance) {
                bestDistance = testDistance;
                closestFood = food[i];
            }
        }
        return closestFood;
    }

    private void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag.Equals("Food")) {
            Destroy(collider.gameObject);
            body.Eat();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

// The brain of the creature
public class Movement : MonoBehaviour
{
    public NavMeshSurface navMesh;

    private Biology bio;
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        bio = GetComponent<Biology>();
        StartCoroutine(SetWaypoint());
    }

    // Update is called once per frame
    void Update() { }

    // Sets the creature destination based on its needs
    IEnumerator SetWaypoint() {
        while (true) {
            // Flag waypoint as invalid so it can be set
            Vector3 waypoint = Vector3.zero;
            // Look for food if hungry or injured
            if (bio.hungry || !bio.healthy) {
                GameObject closestFood = FindClosestFood();
                waypoint = closestFood == null ? Vector3.zero : closestFood.transform.position;
            }

            // Wander if else
            if (waypoint == Vector3.zero) {
                // Get new waypoint if previous one has been reached
                if (agent.remainingDistance < agent.stoppingDistance + 0.5f) {
                    waypoint = WorldManager.GetNearbyWorldPosition(transform.position);
                } else {
                    waypoint = agent.destination;
                }
            }
            agent.destination = waypoint;
            yield return new WaitForSeconds(1);
        }
    }

    // Finds the closest visible food or null if none exists
    private GameObject FindClosestFood() {
        GameObject[] food = GameObject.FindGameObjectsWithTag("Food");

        float bestDistance = float.PositiveInfinity;
        GameObject closestFood = null;
        float sightDistance = bio.bodySpaceBrainRatio * Biology.VISION_CONSTANT;
        for (int i = 0; i < food.Length - 1; i++) {
            float testDistance = Vector3.Distance(food[i].transform.position, transform.position);
            if (testDistance < bestDistance) {
                if (testDistance < sightDistance) {
                    bestDistance = testDistance;
                    closestFood = food[i];
                }
            }
        }
        return closestFood;
    }
}

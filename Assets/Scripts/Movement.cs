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
            bool hungry = body.food < body.stomachCapacity / 2;
            bool healthy = body.health == body.maxHealth;
            if (hungry || !healthy) {
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

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
        agent.destination = WorldManager.GetRandomWorldPosition();
    }

    // Update is called once per frame
    void Update()
    {
        // Move to new locations over and over
        if (agent.remainingDistance < agent.stoppingDistance + 0.5f) {
            Vector3 waypoint;
            // Look for food if hungry
            if (body.food < body.maxFood / 2) {
                waypoint = GetRandomFood();
                if (waypoint == Vector3.positiveInfinity) {
                    waypoint = WorldManager.GetRandomWorldPosition();
                    Debug.Log("No food");
                }

            // Wander
            } else {
                waypoint = WorldManager.GetRandomWorldPosition();
            }
            agent.destination = waypoint;
        }
    }

    private Vector3 GetRandomFood() {
        GameObject[] food = GameObject.FindGameObjectsWithTag("Food");
        if (food.Length == 0) {
            return Vector3.positiveInfinity;
        }
        return food[Random.Range(0, food.Length - 1)].transform.position;
    }

    private void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag.Equals("Food")) {
            Destroy(collider.gameObject);
            body.Eat();
        }
    }
}

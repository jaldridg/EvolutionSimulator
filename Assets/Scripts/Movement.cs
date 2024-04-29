using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
    public NavMeshSurface navMesh;

    // Start is called before the first frame update
    void Start()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.destination = new Vector3(5.0f, 0.0f, 5.0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

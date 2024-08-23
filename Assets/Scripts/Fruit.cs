using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public float radius;
    // In kilograms
    public float mass;
    // The energy multipier of the fruit
    public float nutrition;

    // Start is called before the first frame update
    void Start() { }
     // Update is called once per frame
    void Update() { }

    public void setStats(float r, float n) {
        radius = r;
        nutrition = n;
        // Multiply by 1000 to convert cubic meters to kilograms
        mass = (4.0f / 3) * Mathf.PI * Mathf.Pow(r, 3) * 1000;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.mass = mass;
    }
}

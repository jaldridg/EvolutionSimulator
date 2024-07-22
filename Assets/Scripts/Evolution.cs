using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evolution : MonoBehaviour
{
    /* The genes of the first generation of creatures spawned at the start of simulation */
    public static float STARTING_MAX_SIZE = 2.0f;
    public static float STARTING_OFFSPRING_MASS_RATIO = 0.2f;

    public static float STARTING_BASE_ENERGY_RATIO = 0.15f;
    public static float STARTING_ENERGY_DEFICIENCY_RATIO = 0.5f;
    public static float STARTING_OFFSPRING_TO_REGENERATION_WEIGHT = 0.5f;

    // These should add up to Biology.BODY_SPACE_PACKING_BUDGET
    public static float STARTING_BODY_SPACE_STOMACH_WEIGHT = 70.0f;
    public static float STARTING_BODY_SPACE_HEALTH_WEIGHT = 20.0f;
    public static float STARTING_BODY_SPACE_BRAIN_WEIGHT = 10.0f;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    // Variable mutation methods
    // 0 to x scale with one variable
    // 0 to 1 scale with two variables
    // 0 to 1 scale with three variables (might just need to use normalization - mutate then normalize)
    // etc
}

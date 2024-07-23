using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Evolution : MonoBehaviour
{
    /* The genes of the first generation of creatures spawned at the start of simulation */
    public static float STARTING_MAX_SIZE = 1.0f;
    public static float STARTING_OFFSPRING_MASS_RATIO = 0.2f;

    public static float STARTING_ENERGY_DEFICIENCY_RATIO = 0.5f;
    public static float STARTING_OFFSPRING_TO_REGENERATION_WEIGHT = 0.5f;

    // Trifold ratio - these three values should add to 1
    public static float STARTING_BODY_SPACE_STOMACH_RATIO = 0.5f;
    public static float STARTING_BODY_SPACE_HEALTH_RATIO = 0.5f;
    public static float STARTING_BODY_SPACE_BRAIN_RATIO = 0.0f;

    // The maximum ratio of a gene which is added to or sutracted from the parent's gene
    public static float MUTATION_CONSTANT = 0.05f;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }

    // Mutates a value while keeping it positive
    public static float mutatePositiveValue(float originalValue) {
        float mutationMultiplier = Random.Range(1 - MUTATION_CONSTANT, 1 + MUTATION_CONSTANT);
        return originalValue * mutationMultiplier;
    }

    // Mutates a ratio between 0 and 1
    public static float mutateRatio(float originalValue) {
        // Determine if ratio increases or decreses
        float mutationAmount = 0.0f;
        if (Random.Range(0.0f, 1.0f) > 0.5f) {
            // Decrease the ratio
            mutationAmount = Random.Range(-originalValue, 0);
        } else {
            // Increase the ratio
            mutationAmount = Random.Range(0, 1 - originalValue);
        }
        return originalValue + mutationAmount * MUTATION_CONSTANT;
    }

    // Mutates three values and normalizes them so that their sum is 1
    public static float[] mutateRatioThree(float[] originalValues) {
        float[] newValues = {mutateRatio(originalValues[0]), mutateRatio(originalValues[1]), mutateRatio(originalValues[2])};
        float sum = newValues.Sum();
        return new float[] {newValues[0] / sum, newValues[1] / sum, newValues[2] / sum};
    }
}

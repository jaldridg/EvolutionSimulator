using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CreatureDisplay : MonoBehaviour
{
    private GameObject selectedCreature;

    [SerializeField] private GameObject wholeUI;

    // Health
    [Header("Health Display")]
    [SerializeField] private TextMeshProUGUI healthStateText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthRateText;
    [SerializeField] private TextMeshProUGUI healthLevelText;

    // Food
    [Space(10)]
    [Header("Food Display")]
    [SerializeField] private TextMeshProUGUI foodStateText;
    [SerializeField] private Slider foodSlider;
    [SerializeField] private TextMeshProUGUI foodRateText;
    [SerializeField] private TextMeshProUGUI foodLevelText;

    // Growth
    [Space(10)]
    [Header("Growth Display")]
    [SerializeField] private TextMeshProUGUI growthStateText;
    [SerializeField] private Slider growthSlider;

    [SerializeField] private TextMeshProUGUI childrenText;
    [SerializeField] private TextMeshProUGUI generationText;
    [SerializeField] private TextMeshProUGUI ageText;
    [SerializeField] private TextMeshProUGUI massText;
    [SerializeField] private TextMeshProUGUI sizeText;

    [Space(10)]
    [Header("Energy Display")]
    [SerializeField] private TextMeshProUGUI energyStateText;
    [SerializeField] private Slider baseEnergySlider;
    [SerializeField] private Slider movementEnergySlider;
    [SerializeField] private Slider growthEnergySlider;
    [SerializeField] private TextMeshProUGUI energyLevelText;
    [SerializeField] private TextMeshProUGUI energySourceText;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI baseEnergyPercentText;
    [SerializeField] private TextMeshProUGUI movementEnergyPercentText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI growthPercentText;
    [SerializeField] private TextMeshProUGUI reproductionEnergyPercentText;
    [SerializeField] private TextMeshProUGUI regenerationEnergyPercentText;



    // Colors
    private Color customRed = new Color(1.0f, 0.21f, 0.21f);
    private Color customOrange = new Color(1.0f, 0.69f, 0.21f);
    private Color customYellow = new Color(1.0f, 0.86f, 0.21f);

    [SerializeField] private Camera camera;

    private CameraUI camUI;

    public GameObject visionCircle;

    // Start is called before the first frame update
    void Start()
    {
        selectedCreature = null;
        camUI = camera.GetComponent<CameraUI>();
    }

    // Update is called once per frame
    void Update()
    {
        selectedCreature = camUI.selectedCreature;
        if (selectedCreature) {
            wholeUI.SetActive(true);
            Biology bio = selectedCreature.GetComponent<Biology>();

            float creatureX = selectedCreature.transform.position.x;
            float creatureZ = selectedCreature.transform.position.z;
            visionCircle.transform.position = new Vector3(creatureX, 0.0f, creatureZ);
            float visionDistance = bio.bodySpaceBrainWeight * Biology.VISION_CONSTANT * 2;
            visionCircle.transform.localScale = new Vector3 (visionDistance, 1.0f, visionDistance);

            setHealthUI(bio);
            setFoodUI(bio);
            setGrowthUI(bio);
            setEnergyUI(bio);

        } else {
            visionCircle.transform.position = new Vector3(0.0f, -1.0f, 0.0f);
            wholeUI.SetActive(false);
        }
    }

    private void setVisionCircle(Biology bio) {
        visionCircle.transform.position = transform.position;
        float visionDistance = bio.bodySpaceBrainWeight * Biology.VISION_CONSTANT;
        visionCircle.transform.localScale = new Vector3 (visionDistance, 1.0f, visionDistance);
    }

    private void setHealthUI(Biology bio) {
        healthRateText.text = bio.healthDelta == 0 ? "" : String.Format("{0:+#0.0;-#0.0} / s", bio.healthDelta);
        healthLevelText.text = ((int) bio.health) + " / " + ((int) bio.maxHealth);
        float healthPercent = bio.health / bio.maxHealth;
        healthSlider.value = healthPercent;
        if (healthPercent > 0.75) {
            healthStateText.text = "HEALTHY";
            healthStateText.color = Color.white;
        } else if (healthPercent > 0.3) {
            healthStateText.text = "INJURED";
            healthStateText.color = customYellow;
        } else {
            healthStateText.text = "DYING";
            healthStateText.color = customRed;
        }
    }

    private void setFoodUI(Biology bio) {
        foodRateText.text = bio.foodDelta == 0 ? "" : String.Format("{0:+#0.0;-#0.0} / s", bio.foodDelta);
        foodLevelText.text = ((int) bio.food) + " / " + ((int) bio.stomachCapacity);
        float foodPercent = bio.food / bio.stomachCapacity;
        foodSlider.value = foodPercent;
        if (foodPercent > Biology.WELL_FED_CONSTANT) {
            foodStateText.text = "WELL FED";
            foodStateText.color = Color.green;
        } else if (foodPercent > Biology.HUNGER_CONSTANT) {
            foodStateText.text = "NOURISHED";
            foodStateText.color = Color.white;
        } else if (foodPercent > Biology.STARVATION_CONSTANT) {
            foodStateText.text = "HUNGRY";
            foodStateText.color = customOrange;
        } else {
            foodStateText.text = "STARVING";
            foodStateText.color = customRed;
        }
    }

    private void setGrowthUI(Biology bio) {
        // Show maturation or reproduction progress
        if (bio.mature) {
            growthStateText.text = "REPRODUCING...";
            float offspringMass = bio.growthEnergySpent / Biology.BODY_SIZE_ENERGY_COST;
            growthSlider.value = offspringMass / (bio.maxMass * bio.offspringMassRatio);
        } else {
            growthStateText.text = "MATURING...";
            growthSlider.value = bio.growthEnergySpent / bio.growthEnergyCost;
        }

        childrenText.text = "Children: " + bio.offspringCount;
        generationText.text = "Generation: " + bio.generation;
        ageText.text = "Age: " + (int) bio.age;
        massText.text = "Mass: " + String.Format("{0:0.00}", bio.mass);
        sizeText.text = "Size: " + String.Format("{0:0.00}", bio.size) + String.Format(" (Max: {0:0.00})", bio.maxSize);
    }

    private void setEnergyUI(Biology bio) {
        // Energy bar variables
        float energyPercent = bio.currentEnergyLevel / bio.normalEnergyLevel;
        energyLevelText.text = String.Format("{0:000.#}%", energyPercent * 100);
        if (energyPercent > 1.0f) {
            energySourceText.text = "Energy coming from food";
            energyStateText.text = "WIRED";
            energyStateText.color = Color.green;
        } else if (energyPercent == bio.energyDeficiencyRatio) {
            energySourceText.text = "Sacrificing health to maintain EDR";
            energyStateText.text = "DRAINED";
            energyStateText.color = customRed;
        } else {
            energySourceText.text = "Energy coming from food";
            energyStateText.text = "ENERGIZED";
            energyLevelText.color = Color.white;
        }
        // TODO: Energy bar
        //baseEnergySlider;
        //movementEnergySlider;
        //growthEnergySlider;


        // TODO: Percentage displays
        //baseEnergyPercentText;
        //movementEnergyPercentText;
        //growthPercentText;

        // TODO: Sub-percentage displays
        speedText.text = "Speed: " + bio.speed;
        float healthPercent = bio.health / bio.maxHealth;
        if (healthPercent < 1) {
            speedText.text += String.Format(" (Health: x{0:0.00})", healthPercent); 
        }

        //reproductionEnergyPercentText;
        //regenerationEnergyPercentText;
    }
}
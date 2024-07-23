using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private Slider energyBarBackgroundSlider;
    [SerializeField] private TextMeshProUGUI energyLevelText;
    [SerializeField] private TextMeshProUGUI energySourceText;
    [Space(10)]
    [SerializeField] private TextMeshProUGUI baseEnergyPercentText;
    [SerializeField] private TextMeshProUGUI movementEnergyPercentText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI growthEnergyPercentText;
    [SerializeField] private TextMeshProUGUI reproductionEnergyPercentText;
    [SerializeField] private TextMeshProUGUI regenerationEnergyPercentText;

    [Space(10)]
    [Header("Genes Display")]
    [SerializeField] private TextMeshProUGUI offspringMassRatioText;
    [SerializeField] private TextMeshProUGUI energyDeficiencyRatioText;
    [SerializeField] private TextMeshProUGUI offspringToRegenerationRatioText;
    [SerializeField] private TextMeshProUGUI baseSpaceBrainWeightText;
    [SerializeField] private TextMeshProUGUI baseSpaceBrainWeightDescription;
    [SerializeField] private TextMeshProUGUI baseSpaceStomachWeightText;
    [SerializeField] private TextMeshProUGUI baseSpaceHealthWeightText;



    // Colors
    private Color customRed = new Color(1.0f, 0.21f, 0.21f);
    private Color customOrange = new Color(1.0f, 0.69f, 0.21f);
    private Color customYellow = new Color(1.0f, 0.86f, 0.21f);
    private Color customGreen = new Color(0.34f, 1.0f, 0.55f);
    private Color customBlue = new Color(0.0f, 0.72f, 0.74f);
    private Color customPurple = new Color(0.7f, 0.29f, 1.0f);

    // Normal colors lerped halfway to white
    private Color customGreenDull = new Color(0.68f, 1.0f, 0.77f);
    private Color customBlueDull = new Color(0.5f, 0.86f, 0.87f);
    private Color customPurpleDull = new Color(0.85f, 0.65f, 1.0f);

    [SerializeField] private Camera camera;

    private CameraUI camUI;

    public GameObject visionCircle;

    // Used to correct roudning differences and prevent flickering displays
    private float epsilon = 0.0001f;

    float statBarWidth;
    // The initial position of the bar from the side of the screen
    float statBarOffset;

    // Start is called before the first frame update
    void Start()
    {
        selectedCreature = null;
        camUI = camera.GetComponent<CameraUI>();
        RectTransform rt = growthSlider.gameObject.GetComponent<RectTransform>();
        statBarWidth = rt.sizeDelta[0];
        statBarOffset = rt.offsetMin.x;
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
            float visionDistance = bio.bodySpaceBrainRatio * Biology.VISION_CONSTANT * 2;
            visionCircle.transform.localScale = new Vector3 (visionDistance, 1.0f, visionDistance);

            setHealthUI(bio);
            setFoodUI(bio);
            setGrowthUI(bio);
            setEnergyUI(bio);
            setGenesUI(bio);

        } else {
            visionCircle.transform.position = new Vector3(0.0f, -1.0f, 0.0f);
            wholeUI.SetActive(false);
        }
    }

    private void setHealthUI(Biology bio) {
        healthRateText.text = Math.Abs(bio.healthDelta) < 0.05f ? "" : String.Format("{0:+#0.0;-#0.0} / s", bio.healthDelta);
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
            growthSlider.value = bio.growthEnergySpent / bio.totalOffspringEnergyCost;
        } else {
            growthStateText.text = "MATURING...";
            growthSlider.value = bio.growthEnergySpent / bio.growthEnergyCost;
        }

        childrenText.text = "Children: " + bio.offspringCount;
        generationText.text = "Generation: " + bio.generation;
        ageText.text = "Age: " + (int) bio.age;
        massText.text = "Mass: " + String.Format("{0:0.00}", bio.mass);
        sizeText.text = "<color=white>Size: " + String.Format("{0:0.00}", bio.size) + String.Format(" (Max: </color>{0:0.00}<color=white>)", bio.maxSize);
    }

    private void setEnergyUI(Biology bio) {
        // Energy bar variables
        float energyPercent = bio.currentEnergyLevel / bio.normalEnergyLevel;
        if (energyPercent > 0.995f && energyPercent < 1.0f) { energyPercent = 1.0f; }
        energyLevelText.text = ((int) (energyPercent * 100 + 0.01f)) + "%";
        if (energyPercent > 1.0f) {
            energySourceText.text = "Energy coming from food";
            energyStateText.text = "WIRED";
            energyStateText.color = Color.green;
        } else if (Math.Abs(energyPercent - bio.energyDeficiencyRatio) < epsilon) {
            energySourceText.text = "Sacrificing health to maintain EDR";
            energyStateText.text = "DRAINED";
            energyStateText.color = customRed;
        } else {
            energySourceText.text = "Energy coming from food";
            energyStateText.text = "ENERGIZED";
            energyStateText.color = Color.white;
        }

        // TODO: Percentage displays
        int BEP = (int) (bio.currentBaseEnergyExpenditure / bio.currentEnergyLevel * 100 + epsilon);
        baseEnergyPercentText.text = "Brain: <color=#" + customGreenDull.ToHexString() + ">" + BEP + "%</color>";
        int MEP = (int) (bio.currentMovementEnergyExpenditure / bio.currentEnergyLevel * 100 + epsilon);
        movementEnergyPercentText.text = "Movement: <color=#" + customPurpleDull.ToHexString() + ">" + MEP + "%</color>";
        float currentGrowthEnergyExpenditure = bio.currentRegenerationEnergyExpenditure + bio.currentReproductionEnergyExpenditure;
        int GEP = (int) (currentGrowthEnergyExpenditure / bio.currentEnergyLevel * 100 + epsilon);
        growthEnergyPercentText.text = "Growth: <color=#" + customBlueDull.ToHexString() + ">" + GEP + "%</color>";

        // TODO: Sub-percentage displays
        speedText.text = String.Format("Speed: {0:0.00}", bio.speed);
        float healthPercent = bio.health / bio.maxHealth;
        if (healthPercent < 1) {
            speedText.text += String.Format("\t(x{0:0.00} due to low health)", healthPercent); 
        }

        // Energy bar - split into three like a stacked bar chart
        float energyBarScalingConstant = statBarWidth * energyPercent * 0.01f; // Reverses the percentage making value 0 to 1

        RectTransform baseEnergyRT = baseEnergySlider.gameObject.GetComponent<RectTransform>();
        float baseEnergyBarWidth = BEP * energyBarScalingConstant;
        float cumulativeOffset = statBarOffset + baseEnergyBarWidth / 2;
        baseEnergyRT.sizeDelta = new Vector2(baseEnergyBarWidth, baseEnergyRT.sizeDelta[1]);
        baseEnergyRT.position = new Vector3(cumulativeOffset, baseEnergyRT.position.y, baseEnergyRT.position.z);

        RectTransform growthEnergyRT = growthEnergySlider.gameObject.GetComponent<RectTransform>();
        float growthEnergyBarWidth = GEP * energyBarScalingConstant;
        cumulativeOffset += baseEnergyBarWidth / 2 + growthEnergyBarWidth / 2;
        growthEnergyRT.sizeDelta = new Vector2(growthEnergyBarWidth, growthEnergyRT.sizeDelta[1]);
        growthEnergyRT.position = new Vector3(cumulativeOffset, growthEnergyRT.position.y, growthEnergyRT.position.z);
        
        RectTransform movementEnergyRT = movementEnergySlider.gameObject.GetComponent<RectTransform>();
        float movementEnergyBarWidth = MEP * energyBarScalingConstant;
        cumulativeOffset += growthEnergyBarWidth / 2 + movementEnergyBarWidth / 2;
        movementEnergyRT.sizeDelta = new Vector2(movementEnergyBarWidth, movementEnergyRT.sizeDelta[1]);
        movementEnergyRT.position = new Vector3(cumulativeOffset, movementEnergyRT.position.y, movementEnergyRT.position.z);

        RectTransform energyBarBackgroundSliderRT = energyBarBackgroundSlider.gameObject.GetComponent<RectTransform>();
        float energyBarBackgroundWidth;
        if (energyPercent < 0.995) {
            energyBarBackgroundWidth = statBarWidth - baseEnergyBarWidth - growthEnergyBarWidth - movementEnergyBarWidth;
        } else {
            energyBarBackgroundWidth = 0.0f;
        }
        cumulativeOffset += movementEnergyBarWidth / 2 + energyBarBackgroundWidth / 2;
        energyBarBackgroundSliderRT.sizeDelta = new Vector2(energyBarBackgroundWidth, energyBarBackgroundSliderRT.sizeDelta[1]);
        energyBarBackgroundSliderRT.position = new Vector3(cumulativeOffset, energyBarBackgroundSliderRT.position.y, energyBarBackgroundSliderRT.position.z);
        


        int reproductionEP = (int) (bio.currentReproductionEnergyExpenditure / currentGrowthEnergyExpenditure * 100 + epsilon);
        if (bio.mature) {
            reproductionEnergyPercentText.text = "Reproduction: " + reproductionEP + "%";
        } else  {
            reproductionEnergyPercentText.text = "Maturation: " + reproductionEP + "%";
        }
        int regenerationEP = (int) (bio.currentRegenerationEnergyExpenditure / currentGrowthEnergyExpenditure * 100 + epsilon);
        regenerationEnergyPercentText.text = "Regeneration " + regenerationEP + "%";
    }

    private void setGenesUI(Biology bio) {
        offspringMassRatioText.text = ((int) (bio.offspringMassRatio * 100)) + "%";
        energyDeficiencyRatioText.text = ((int) (bio.energyDeficiencyRatio * 100)) + "%";
        offspringToRegenerationRatioText.text = ((int) (bio.offspringToRegenerationWeight * 100)) + "%";

        int baseEnergyPercent = (int) (bio.bodySpaceBrainRatio * 100);
        baseSpaceBrainWeightDescription.text = "Vision distance (demands " + baseEnergyPercent + "% of normal energy)";
        baseSpaceBrainWeightText.text = ((int) (epsilon + bio.bodySpaceBrainRatio * 100)) + "<color=white> / " + Biology.BODY_SPACE_PACKING_BUDGET;
        baseSpaceStomachWeightText.text = ((int) (epsilon + bio.bodySpaceStomachRatio * 100)) + "<color=white> / " + Biology.BODY_SPACE_PACKING_BUDGET;
        baseSpaceHealthWeightText.text = ((int) (epsilon + bio.bodySpaceHealthRatio * 100)) + "<color=white> / " + Biology.BODY_SPACE_PACKING_BUDGET;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureDisplay : MonoBehaviour
{
    private GameObject selectedCreature;

    [SerializeField] private GameObject wholeUI;

    // Health
    [SerializeField] private TextMeshProUGUI healthStateText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthRateText;

    // Food
    [SerializeField] private TextMeshProUGUI foodStateText;
    [SerializeField] private Slider foodSlider;
    [SerializeField] private TextMeshProUGUI foodRateText;

    // Growth
    [SerializeField] private Slider growthSlider;

    // Colors
    private Color customRed = new Color(1.0f, 0.21f, 0.21f);
    private Color customOrange = new Color(1.0f, 0.69f, 0.21f);
    private Color customYellow = new Color(1.0f, 0.86f, 0.21f);

    [SerializeField] private Camera camera;

    private CameraUI camUI;

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

            float healthRate = 100 * (bio.healthDelta / bio.maxHealth);
            healthRateText.text = healthRate == 0 ? "" : String.Format("{0:+#0.0;-#0.0}% / s", healthRate);
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

            float foodRate = 100 * (bio.foodDelta / bio.stomachCapacity);
            foodRateText.text = foodRate == 0 ? "" : String.Format("{0:+#0.0;-#0.0}% / s", foodRate);
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

            // Show maturation or reproduction progress
            if (bio.mature) {
                growthSlider.value = bio.growthEnergySpent / bio.offspringEnergyCutoff;
            } else {
                growthSlider.value = bio.growthEnergySpent / bio.growthEnergyCost;
            }
        } else {
            wholeUI.SetActive(false);
        }

    }
}

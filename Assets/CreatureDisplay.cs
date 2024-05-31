using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CreatureDisplay : MonoBehaviour
{
    private GameObject selectedCreature;

    [SerializeField] private GameObject wholeUI;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider foodSlider;

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
            healthSlider.value = bio.health / bio.maxHealth;
            foodSlider.value = bio.food / bio.stomachCapacity;
        } else {
            wholeUI.SetActive(false);
        }

    }
}

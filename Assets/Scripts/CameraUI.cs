using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class CameraUI : MonoBehaviour
{
    private static float STARTING_CAMERA_HEIGHT = 50.0f;
    private static float MIN_CAMERA_HEIGHT = 5.0f;
    private static float MAX_CAMERA_HEIGHT = 100.0f;

    [SerializeField]
    private static float CAMERA_MOVE_SPEED = 0.5f;
    private static float CAMERA_ZOOM_SPEED = 0.02f;

    private Vector3 cameraMovementDirection;

    private Vector3 cameraZoomDirection;

    [SerializeField] private Camera camera;

    [SerializeField] private LayerMask creatureMask;

    public GameObject selectedCreature;

    // Start is called before the first frame update
    void Start()
    {
        selectedCreature = null;
        transform.position = new Vector3(0.0f, STARTING_CAMERA_HEIGHT, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 overallMovement = cameraMovementDirection + cameraZoomDirection;
        transform.position += overallMovement * Time.deltaTime;
        float cameraHeight = transform.position.y;

        // Check if camera is too close or far
        if (cameraHeight < MIN_CAMERA_HEIGHT) {
            transform.position = new Vector3(transform.position.x, MIN_CAMERA_HEIGHT, transform.position.z);
        } 
        else if (cameraHeight > MAX_CAMERA_HEIGHT) {
            transform.position = new Vector3(transform.position.x, MAX_CAMERA_HEIGHT, transform.position.z);
        }

        // Lock onto creature if selected
        if (selectedCreature) {
            Vector3 creaturePosition = selectedCreature.transform.position;
            transform.position = new Vector3(creaturePosition.x, transform.position.y, creaturePosition.z);
        }
    }

    public void OnMove(InputValue value) {
        Vector2 direction = value.Get<Vector2>();
        Vector3 direction3D = new Vector3(direction.x, 0.0f, direction.y);
        float cameraHeight = transform.position.y;
        cameraMovementDirection = direction3D * cameraHeight * CAMERA_MOVE_SPEED;
    }

    public void OnZoom(InputValue value) {
        float scroll = value.Get<float>();
        Vector3 scrollDirection = new Vector3(0.0f, -scroll, 0.0f);
        float cameraHeight = transform.position.y;
        cameraMovementDirection = scrollDirection * cameraHeight * CAMERA_ZOOM_SPEED;
    }

    public void OnSelect() {
        // Raycast from camera to mouse to see if we're hovering over a creature
        Ray camToWorld = camera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(camToWorld, out RaycastHit hit, float.PositiveInfinity, creatureMask)) {
            selectedCreature = null;
            return;
        }

        // If we've reached this point, we've selected a creature
        selectedCreature = hit.transform.gameObject;
    }
}

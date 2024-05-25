using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraUI : MonoBehaviour
{
    private float cameraHeight;
    private static float MIN_CAMERA_HEIGHT = 10.0f;
    private static float MAX_CAMERA_HEIGHT = 100.0f;

    [SerializeField]
    private static float CAMERA_SPEED = 0.3f;

    private Vector3 cameraMovementDirection;

    // Start is called before the first frame update
    void Start()
    {
        cameraHeight = 75.0f;
        transform.position = new Vector3(0.0f, cameraHeight, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += cameraMovementDirection * Time.deltaTime;
    }

    public void OnMove(InputValue value) {
        Vector2 direction = value.Get<Vector2>();
        Vector3 direction3D = new Vector3(direction.x, 0.0f, direction.y);
        cameraMovementDirection = direction3D * cameraHeight * CAMERA_SPEED;
    }
}

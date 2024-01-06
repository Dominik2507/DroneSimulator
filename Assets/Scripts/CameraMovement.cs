using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Camera camera;
    public float scrollSensitivity = 1f;
    public float dragSpeed = 2f;

    private Vector3 dragOrigin;

    void Update()
    {
        // Zoom with scroll wheel
        float scroll = Input.mouseScrollDelta.y;

        if(scroll != 0) camera.orthographicSize -= scroll * scrollSensitivity;

        // Drag to move
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            Vector3 movePos = (dragOrigin - Input.mousePosition) * dragSpeed * Time.deltaTime;

            // Set the Y component of movePos to 0 to prevent changes in the Y axis
            movePos.z = movePos.y;
            movePos.y = 0f;

            transform.Translate(movePos, Space.World);
            dragOrigin = Input.mousePosition;
        }
    }
}

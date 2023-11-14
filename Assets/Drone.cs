using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public float speed = 1.0f;
    public float vertical_offset = 1f;
    public Vector3 startPosition;

    public SurvailanceManager manager;
    public LayerMask layerMask;
    public int movingTowardsIndex;

    public float distanceBeforeNextPoint = 0.01f;

    void Update()
    {
        if(manager is not null && manager.isRunning) Move();
    }

    public void Move()
    {
        //Debug.Log("Index " + movingTowardsIndex);
        Vector3 nextPoint = manager.navigationCurve[movingTowardsIndex];
        
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = -Vector3.up;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, Mathf.Infinity, layerMask))
        {
            //Debug.Log("Hit something at: " + hit.point);
            nextPoint.y = hit.point.y + vertical_offset;
        }
        else
        {
            //Debug.Log("Did not hit anything");
            nextPoint.y += vertical_offset;
        }

        //CHECK HOW CLOSE IS THE DRONE
        if(Vector3.Distance(nextPoint, transform.position) <= distanceBeforeNextPoint)
        {
            movingTowardsIndex++;

            if (movingTowardsIndex >= manager.navigationCurve.Count) movingTowardsIndex = 0;
        }

        Vector3 newPosition = Vector3.MoveTowards(transform.position, nextPoint, speed * Time.deltaTime);
        gameObject.GetComponent<Rigidbody>().MovePosition(newPosition);
    }

    public void ResetPosition()
    {
        gameObject.GetComponent<Rigidbody>().MovePosition(startPosition);
    }
}

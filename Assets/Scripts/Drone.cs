using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public float speed = 1.0f;
    public float rotationSpeed = 5.0f;
    public float vertical_offset = 4f;
    public Vector3 startPosition;

    public SurvailanceManager manager;
    public LayerMask layerMask;
    public int movingTowardsIndex;
    Camera navigationCamera = null;

    public float distanceBeforeNextPoint = 0.01f;

    private RaycastHit obstacleHit;
    private bool isAvoiding = false;
    private List<Vector3> navigationCurve;

    public float navigationViewDistance = 6.0f;
    public float keepDistanceFromObjects = 3.0f;

    public Vector2 avoidDirection = Vector3.zero;
    private void Start()
    {
        navigationCamera =(new List<Camera>(gameObject.GetComponentsInChildren<Camera>())).Find((Camera cam) => { return cam.gameObject.name == "ForwardNavigationCamera"; });
        navigationCurve = manager.navigationCurve;
    }
    void Update()
    {
        speed = SimulationManager.instance.simSpeed;
        vertical_offset = SimulationManager.instance.verticalOffset;
        
        new List<Camera>(GetComponentsInChildren<Camera>()).Find((Camera cam) => { return cam.name == "ViewingCamera"; }).fieldOfView = SimulationManager.instance.FOV;

        if (manager is not null && manager.isRunning)
        {
            if (CheckForObstacles())
            {
                

                if (
                    Vector3.Distance(navigationCamera.transform.position, obstacleHit.collider.ClosestPoint(navigationCamera.transform.position))
                    <
                    Vector3.Distance(navigationCamera.transform.position, navigationCurve[movingTowardsIndex])
                    )
                {
                    //isAvoiding = true;

                    //AddAvoidancePoints();
                    if (obstacleHit.distance <= keepDistanceFromObjects)
                    {
                        AvoidObstacle();
                    }
                    else
                    {
                        Move();
                    }
                }
                else
                {
                    Move();
                }
                // ADJUST the navigation curve by adding additional points between current and next points
            }
            else
            {
                avoidDirection = Vector2.zero;
                Move();
            }
        }
    }


    public void Move()
    {
        ////Debug.Log("Index " + movingTowardsIndex);
        Vector3 nextPoint = navigationCurve[movingTowardsIndex];
        
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = -Vector3.up;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, Mathf.Infinity, layerMask))
        {
            ////Debug.Log("Hit something at: " + hit.point);
            nextPoint.y = hit.point.y + vertical_offset;
        }
        else
        {
            ////Debug.Log("Did not hit anything");
            nextPoint.y += vertical_offset;
        }

        // Calculate the direction to the next point
        Vector3 directionToNextPoint = nextPoint - transform.position;

        // Check if the direction is not zero (to avoid division by zero)
        if (directionToNextPoint != Vector3.zero)
        {
            // Project the direction onto the horizontal plane (Y-axis)
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(directionToNextPoint, Vector3.up);

            // Calculate the rotation towards the next point, only around the Y-axis
            Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection, Vector3.up);

            // Smoothly rotate the drone towards the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        //CHECK HOW CLOSE IS THE DRONE
        if (Vector3.Distance(nextPoint, transform.position) <= distanceBeforeNextPoint)
        {
            movingTowardsIndex++;
            isAvoiding = false;
            if (movingTowardsIndex >= navigationCurve.Count) movingTowardsIndex = 0;
        }

        Vector3 newPosition = Vector3.MoveTowards(transform.position, nextPoint, speed * Time.deltaTime);
        gameObject.GetComponent<Rigidbody>().MovePosition(newPosition);
    }

    public void ResetPosition()
    {
        gameObject.GetComponent<Rigidbody>().MovePosition(startPosition);
    }

    bool CheckForObstacles()
    {
        return (navigationCamera
            &&
            Physics.Raycast(navigationCamera.transform.position, navigationCamera.transform.forward, out obstacleHit, navigationViewDistance, layerMask)
            );
    }

    void AvoidObstacle()
    {
        int currIndex = movingTowardsIndex > 0 ? movingTowardsIndex - 1 : navigationCurve.Count - 1;
        int nextIndex = movingTowardsIndex;

        Vector3 currPoint = navigationCurve[currIndex];
        Vector3 nextPoint = navigationCurve[nextIndex];


        float checkStep = 0.001f;
        float accumulatedCheck = 0;
        Vector3 cameraForward = navigationCamera.transform.forward;
        bool L, R, U;
        if(avoidDirection == Vector2.zero)
        {
            //Check what direction to avoid
            do
            {
                accumulatedCheck += checkStep;
                Vector3 left = cameraForward - accumulatedCheck * navigationCamera.transform.right;
                Vector3 right = cameraForward + accumulatedCheck * navigationCamera.transform.right;
                Vector3 up = cameraForward + accumulatedCheck * navigationCamera.transform.up;

                L = Physics.Raycast(navigationCamera.transform.position, left, out RaycastHit hitLeft, float.PositiveInfinity, layerMask);
                R = Physics.Raycast(navigationCamera.transform.position, right, out RaycastHit hitRight, float.PositiveInfinity, layerMask);
                U = Physics.Raycast(navigationCamera.transform.position, up, out RaycastHit hitUp, float.PositiveInfinity, layerMask);

            
                if (!L || !R || !U)
                {
                    if (((L ? 0 : 1) + (R ? 0 : 1) + (U ? 0 : 1)) == 3)
                    {
                        // chose random direction of the 3
                        int randomDirection = Random.Range(0, 3);
                        // 0 = right, 1 = left, 2 = up
                        if (randomDirection == 0) avoidDirection = Vector2.right;
                        else if (randomDirection == 1) avoidDirection = Vector2.left;
                        else avoidDirection = Vector2.up;

                    }
                    else if (((L ? 0 : 1) + (R ? 0 : 1) + (U ? 0 : 1)) == 2)
                    {
                        // chose randomn direction of the 2
                        if (L)
                        {
                            // 0 = right, 1 = up
                            int randomDirection = Random.Range(0, 2);
                            if (randomDirection == 0) avoidDirection = Vector2.right;
                            else avoidDirection = Vector2.up;
                        }
                        else if (R)
                        {
                            // 0 = left, 1 = up
                            int randomDirection = Random.Range(0, 2);
                            if (randomDirection == 0) avoidDirection = Vector2.left;
                            else avoidDirection = Vector2.up;
                        }
                        else
                        {
                            // 0 = right, 1 = left
                            int randomDirection = Random.Range(0, 2);
                            if (randomDirection == 0) avoidDirection = Vector2.right;
                            else avoidDirection = Vector2.left;
                        }
                    }
                    else
                    {
                        // only one is not hit go that way
                        if (!L) avoidDirection = Vector2.left;
                        else if (!R) avoidDirection = Vector2.right;
                        else avoidDirection = Vector2.up;


                    }
                }

            } while (L & R & U);


        }
        Vector3 avoid = avoidDirection == Vector2.left ? -1 * navigationCamera.transform.right :
                    avoidDirection == Vector2.right ? navigationCamera.transform.right : navigationCamera.transform.up;

        Vector3 nextMovePoint = transform.position + navigationViewDistance * avoid;

        Vector3 newPosition = Vector3.MoveTowards(transform.position, nextMovePoint, speed * Time.deltaTime);
        gameObject.GetComponent<Rigidbody>().MovePosition(newPosition);


        if (Vector3.Angle((navigationCamera.transform.position - nextPoint), (currPoint - nextPoint)) > 90)
        {
            movingTowardsIndex = movingTowardsIndex == (navigationCurve.Count - 1) ? 0 : movingTowardsIndex + 1; 
        }
        
    }

    void AddAvoidancePoints()
    {
        int currIndex = movingTowardsIndex > 0 ? movingTowardsIndex - 1 : navigationCurve.Count - 1;
        int nextIndex = movingTowardsIndex;

        Vector3 currPoint = navigationCurve[currIndex];
        Vector3 nextPoint = navigationCurve[nextIndex];

        float checkStep = 0.001f;
        float accumulatedCheck = 0;
        Vector3 cameraForward = navigationCamera.transform.forward;
        bool L, R, U;

        do
        {
            accumulatedCheck += checkStep;
            Vector3 left  = cameraForward - accumulatedCheck * navigationCamera.transform.right;
            Vector3 right = cameraForward + accumulatedCheck * navigationCamera.transform.right;
            Vector3 up    = cameraForward + accumulatedCheck * navigationCamera.transform.up;
            
            L = Physics.Raycast(navigationCamera.transform.position, left, out RaycastHit hitLeft, float.PositiveInfinity, layerMask);
            R = Physics.Raycast(navigationCamera.transform.position, right, out RaycastHit hitRight, float.PositiveInfinity, layerMask);
            U = Physics.Raycast(navigationCamera.transform.position, up, out RaycastHit hitUp, float.PositiveInfinity, layerMask);

            if(!L || !R || !U)
            {
                if(((L ? 0 : 1) + (R ? 0 : 1) + (U ? 0 : 1)) == 3)
                {
                    // chose random direction of the 3
                    int randomDirection = Random.Range(0, 3);
                    // 0 = right, 1 = left, 2 = up
                    if (randomDirection == 0) avoidDirection = Vector2.right;
                    else if (randomDirection == 1) avoidDirection = Vector2.left;
                    else avoidDirection = Vector2.up;

                }
                else if(((L ? 0 : 1) + (R ? 0 : 1) + (U ? 0 : 1)) == 2)
                {
                    // chose randomn direction of the 2
                    if (L)
                    {
                        // 0 = right, 1 = up
                        int randomDirection = Random.Range(0, 2);
                        if (randomDirection == 0) avoidDirection = Vector2.right;
                        else avoidDirection = Vector2.up;
                    }
                    else if (R)
                    {
                        // 0 = left, 1 = up
                        int randomDirection = Random.Range(0, 2);
                        if (randomDirection == 0) avoidDirection = Vector2.left;
                        else avoidDirection = Vector2.up;
                    }
                    else
                    {
                        // 0 = right, 1 = left
                        int randomDirection = Random.Range(0, 2);
                        if (randomDirection == 0) avoidDirection = Vector2.right;
                        else avoidDirection = Vector2.left;
                    }
                }
                else
                {
                    // only one is not hit go that way
                    if (!L) avoidDirection = Vector2.left;
                    else if (!R) avoidDirection = Vector2.right;
                    else avoidDirection = Vector2.up;
                }
            }
            
        } while ( L & R & U);

        Vector3 avoid = avoidDirection == Vector2.left  ? -1 * navigationCamera.transform.right :
                avoidDirection == Vector2.right ? navigationCamera.transform.right : navigationCamera.transform.up;
        Vector3 newPointInCurve = 
              obstacleHit.point
            //+ navigationViewDistance * (navigationCamera.transform.forward)
            + (navigationViewDistance + accumulatedCheck) * avoid;
            //+ navigationViewDistance * (accumulatedCheck * 2) * new Vector3(0, avoidDirection.y, avoidDirection.x);

        
        //navigationCurve.Insert(nextIndex, transform.position);
        navigationCurve.Insert(nextIndex, newPointInCurve);
    }

    void OnDrawGizmos()
    {
        if (navigationCamera != null)
        {
            Vector3 avoid = avoidDirection == Vector2.left ? -1 * navigationCamera.transform.right :
                   avoidDirection == Vector2.right ? navigationCamera.transform.right : navigationCamera.transform.up;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(navigationCamera.transform.position, navigationCamera.transform.position + navigationCamera.transform.forward * navigationViewDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(navigationCamera.transform.position, navigationCamera.transform.position + avoid * navigationViewDistance );

        }
    }

}

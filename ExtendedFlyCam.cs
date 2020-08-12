// This is a very quick and dirty implementation of waypoints for the camera to move to
// I did this when I was just starting out with C# and Unity and only used it that once
// I am aware of some definite constraints in the "direction" rotations happen based on me not covering all bases
// I'm sure there are many things that can be done better, and there are also many great (and free) tools out there that can do a lot more
// Also look into Cinemachine

using System;
using UnityEngine;

[System.Serializable]
public class WayPoint
{
    public string description;
    public float rotateYonly = 0f;
    public Vector3 slideToPosition, slideToRotation;
    public float startAfter, slideSpeedMultiplier;
}

public class ExtendedFlyCam : MonoBehaviour
{
    [SerializeField] public float cameraSensitivity = 90;
    [SerializeField] public float climbSpeed = 15;
    [SerializeField] public float normalMoveSpeed = 10;
    [SerializeField] public float slowMoveFactor = 0.25f;
    [SerializeField] public float fastMoveFactor = 3;
    [SerializeField] public Vector2 pitchLimits = new Vector2(-90f, 90f);
    [SerializeField] public bool applyWayPoints = true;
    [SerializeField] WayPoint[] wayPoints;
    
    private float rotationX = 0f, rotationY = 0f, rotationZ = 0f, toRotateX, stepRotation;
    private float multiplier = 1f, timeToSlide = 0f, timeSinceSlide = 0f;
    private bool startSliding = false, turning = false, startSet = false;
    private Vector3 startRotation, tempRotation, tempRotation1, tempRotation2;
    private byte currentWayPoint = 0;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        if (!startSliding) InitiateWayPoint();
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) multiplier = fastMoveFactor;
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) multiplier = slowMoveFactor;
        else multiplier = 1f;
        if (Input.GetKeyDown(KeyCode.Insert)) turning = !turning;
        if (startSliding)
        {
            if (wayPoints[currentWayPoint].rotateYonly != 0f)
            {

                if (toRotateX < 0f) stepRotation = Mathf.Max(toRotateX, wayPoints[currentWayPoint].rotateYonly * wayPoints[currentWayPoint].slideSpeedMultiplier * Time.fixedDeltaTime /360f);
                else stepRotation = Mathf.Min(toRotateX, wayPoints[currentWayPoint].rotateYonly * wayPoints[currentWayPoint].slideSpeedMultiplier * Time.fixedDeltaTime / 360f);
                rotationX += stepRotation;
                toRotateX -= stepRotation;
            }
            else
            {
                if (!startSet)
                {
                    startRotation = transform.eulerAngles;
                    rotationX = Vector3.Dot(startRotation, Vector3.up);
                    rotationY = Vector3.Dot(startRotation, Vector3.left);
                    rotationZ = Vector3.Dot(startRotation, Vector3.forward);
                    startSet = true;
                }
                timeToSlide = (wayPoints[currentWayPoint].slideToPosition - transform.position).magnitude / (wayPoints[currentWayPoint].slideSpeedMultiplier * normalMoveSpeed * multiplier);
                transform.position += Mathf.Min(wayPoints[currentWayPoint].slideSpeedMultiplier * normalMoveSpeed * multiplier * Time.fixedDeltaTime, (wayPoints[currentWayPoint].slideToPosition - transform.position).magnitude) * (wayPoints[currentWayPoint].slideToPosition - transform.position).normalized;
                if (wayPoints[currentWayPoint].slideToRotation.x < -180f) tempRotation1.x = wayPoints[currentWayPoint].slideToRotation.x + 360f; else if (wayPoints[currentWayPoint].slideToRotation.x > 180f) tempRotation1.x = wayPoints[currentWayPoint].slideToRotation.x - 360f; else tempRotation1.x = wayPoints[currentWayPoint].slideToRotation.x;
                if (wayPoints[currentWayPoint].slideToRotation.y < -180f) tempRotation1.y = wayPoints[currentWayPoint].slideToRotation.y + 360f; else if (wayPoints[currentWayPoint].slideToRotation.y > 180f) tempRotation1.y = wayPoints[currentWayPoint].slideToRotation.y - 360f; else tempRotation1.y = wayPoints[currentWayPoint].slideToRotation.y;
                if (wayPoints[currentWayPoint].slideToRotation.z < -180f) tempRotation1.z = wayPoints[currentWayPoint].slideToRotation.x + 360f; else if (wayPoints[currentWayPoint].slideToRotation.z > 180f) tempRotation1.z = wayPoints[currentWayPoint].slideToRotation.z - 360f; else tempRotation1.z = wayPoints[currentWayPoint].slideToRotation.z;
                if (transform.eulerAngles.x < -180f) tempRotation2.x = transform.eulerAngles.x + 360f; else if (transform.eulerAngles.x > 180f) tempRotation2.x = transform.eulerAngles.x - 360f; else tempRotation2.x = transform.eulerAngles.x;
                if (transform.eulerAngles.y < -180f) tempRotation2.y = transform.eulerAngles.y + 360f; else if (transform.eulerAngles.y > 180f) tempRotation2.y = transform.eulerAngles.y - 360f; else tempRotation2.y = transform.eulerAngles.y;
                if (transform.eulerAngles.z < -180f) tempRotation2.z = transform.eulerAngles.z + 360f; else if (transform.eulerAngles.z > 180f) tempRotation2.z = transform.eulerAngles.z - 360f; else tempRotation2.z = transform.eulerAngles.z;
                tempRotation = Mathf.Min((tempRotation1 - tempRotation2).magnitude * Time.fixedDeltaTime / timeToSlide, (tempRotation1 - tempRotation2).magnitude) * (tempRotation1 - tempRotation2).normalized;
                rotationX += Vector3.Dot(tempRotation, Vector3.up);
                rotationY += Vector3.Dot(tempRotation, Vector3.left);
                rotationZ += Vector3.Dot(tempRotation, Vector3.forward);
                //rotationX -= Vector3.Dot(Vector3.down, tempRotation);
                //rotationY -= Vector3.Dot(Vector3.right, tempRotation);
                //rotationZ -= Vector3.Dot(Vector3.back, tempRotation);
            }
            if (((wayPoints[currentWayPoint].rotateYonly != 0f) && (toRotateX == 0f)) ||
                ((wayPoints[currentWayPoint].rotateYonly == 0f) && (wayPoints[currentWayPoint].slideToPosition - transform.position).magnitude < 0.001f && ((tempRotation1 - tempRotation2).magnitude < 0.001f))
                )
            {
                startSliding = false;
                currentWayPoint++;
                startRotation = transform.eulerAngles;
                rotationX = Vector3.Dot(startRotation, Vector3.up);
                rotationY = Vector3.Dot(startRotation, Vector3.left);
                rotationZ = Vector3.Dot(startRotation, Vector3.forward);
            }
        }
        else
        {
            if (turning)
            {
                rotationX += 0.2f * multiplier * cameraSensitivity * Time.fixedDeltaTime;
            }
            else if (Cursor.lockState == CursorLockMode.Locked)
            {
                rotationX += Input.GetAxis("Mouse X") * multiplier * cameraSensitivity * Time.fixedDeltaTime;
                rotationY += Input.GetAxis("Mouse Y") * multiplier * cameraSensitivity * Time.fixedDeltaTime;
                rotationY = Mathf.Clamp(rotationY, pitchLimits.x, pitchLimits.y);
            }
        }
        while (rotationX < 0f) rotationX += 360f;
        while (rotationY < 0f) rotationY += 360f;
        while (rotationZ < 0f) rotationZ += 360f;
        transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
        transform.localRotation *= Quaternion.AngleAxis(rotationZ, Vector3.forward);
        //transform.rotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        //transform.rotation *= Quaternion.AngleAxis(rotationY, Vector3.left);
        //transform.rotation *= Quaternion.AngleAxis(rotationZ, Vector3.forward);

        transform.position += transform.forward * normalMoveSpeed * multiplier * Input.GetAxis("Vertical") * Time.fixedDeltaTime;
        transform.position += transform.right * normalMoveSpeed * multiplier * Input.GetAxis("Horizontal") * Time.fixedDeltaTime;

        if (Input.GetKey(KeyCode.Q)) transform.position += transform.up * climbSpeed * multiplier * Time.fixedDeltaTime;
        if (Input.GetKey(KeyCode.E)) transform.position -= transform.up * climbSpeed * multiplier * Time.fixedDeltaTime;

        if (Input.GetKeyDown(KeyCode.End))
        {
            if (Cursor.lockState == CursorLockMode.Locked) Cursor.lockState = CursorLockMode.None;
                else Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void InitiateWayPoint()
    {
        if (applyWayPoints && currentWayPoint < wayPoints.Length)
        {
            wayPoints[currentWayPoint].startAfter -= Time.fixedDeltaTime;
            if (wayPoints[currentWayPoint].startAfter < 0f)
            {
                startSliding = true;
                startSet = false;
                toRotateX = wayPoints[currentWayPoint].rotateYonly;
            }
        }
    }
}
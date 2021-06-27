using UnityEngine;

public class FollowThenLookAt : MonoBehaviour
{
    public Transform followTarget, lookAtTarget, lookDirection;
    public Vector3 offsetFollow = new Vector3(0f, 0f, 0f);
    public Vector3 offsetLookAt = new Vector3(0f, 0f, 0f);
    public float followSpeed = 10f;
    public float lookAtSpeed = 5f;

    private Vector3 newPos, curLook, newLook;

    private void Start()
    {
        if (followTarget != null) transform.position = followTarget.position + offsetFollow;
        if (lookAtTarget != null)
        {
            curLook = lookAtTarget.position + offsetLookAt;
            transform.LookAt(curLook);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (followTarget != null)
        {
            //if ((transform.position - followTarget.position - offsetFollow).magnitude > 0.2f)
            newPos = followTarget.position + offsetFollow;
            transform.position = Vector3.Lerp(transform.position, newPos, followSpeed * Time.deltaTime);
            //transform.position = newPos;
        }
        if (lookAtTarget != null)
        {
            newLook = lookAtTarget.position + offsetLookAt;
            curLook = Vector3.Lerp(curLook, newLook, lookAtSpeed * Time.deltaTime);
            transform.LookAt(curLook);
            //transform.LookAt(newLook);
        }
    }
}

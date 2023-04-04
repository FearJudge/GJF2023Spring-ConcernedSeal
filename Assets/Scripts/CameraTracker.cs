using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public Transform toTrack;
    public float trackingY = 0f;
    public float trackingSpeed = 0.6f;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(toTrack.position.x, trackingY, transform.position.z), trackingSpeed * Time.deltaTime);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public Transform toTrack;
    public Vector2 trackingYRange = new Vector2(-0.5f, 0.5f);
    public float trackingSpeed = 0.6f;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(toTrack.position.x, Mathf.Clamp(toTrack.position.y, trackingYRange.x, trackingYRange.y), transform.position.z), trackingSpeed * Time.deltaTime);
    }
}

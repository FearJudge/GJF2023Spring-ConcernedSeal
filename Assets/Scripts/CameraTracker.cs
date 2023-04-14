using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public enum TrackingTarget
    {
        NoTarget,
        Player,
        Camera
    }

    public static Transform trackableOverride = null;
    public Transform toTrack;
    private Transform initialTracked;
    public Vector2 trackingYRange = new Vector2(-0.5f, 0.5f);
    public float trackingSpeed = 0.6f;

    private void Awake()
    {
        initialTracked = toTrack;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (trackableOverride != null && trackableOverride != toTrack) { toTrack = trackableOverride; }
        else if (trackableOverride == null && toTrack != initialTracked) { toTrack = initialTracked; }
        transform.position = Vector3.Lerp(transform.position, new Vector3(toTrack.position.x, Mathf.Clamp(toTrack.position.y, trackingYRange.x, trackingYRange.y), transform.position.z), trackingSpeed * Time.deltaTime);
    }
}

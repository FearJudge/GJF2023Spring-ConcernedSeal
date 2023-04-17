using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/* 
 * A component that enables a platform to break if hit by an icewave.
 * Can be tweaked so that the platform falls just how you want it to.
 */
[RequireComponent(typeof(BoxCollider2D))]
public class FragileIcePlatform : MonoBehaviour
{
    const float DAMPENING = 5f;

    public bool breakOnContact = false;
    public Rigidbody2D mainParent;
    public SpriteShapeController platformEdges;
    public SpriteShapeController iceBergBG;
    public bool automaticallyDeterminedEdges = true;
    public LayerMask whatBrakesMe;
    [SerializeField] protected IceSinkingVariables sinkingBehaviour;
    private BoxCollider2D fragileIceTrigger;

    [System.Serializable]
    protected class IceSinkingVariables
    {
        [HideInInspector] public bool isSinking = false; 
        public float initialWaveStrength = 1f;
        public float initialWaveRotationAngle = 0.4f;
        public float hangOnTime = 2.5f;
        public float sinkingSpeedInitial = 0.05f;
        public float sinkingSpeedAfterHang = 1f;
        public float sinkingRotationAngle = 0.2f;
        public float destroyWhenReachedDepthsOf = -100f;
    }
    public SpriteShape crackedIceProfile;

    private void Start()
    {
        fragileIceTrigger = GetComponent<BoxCollider2D>();
        if (automaticallyDeterminedEdges && platformEdges != null) { CreateTriggerEdges(); }
        if (breakOnContact) { SetBackgroundProfile(); }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (breakOnContact == false) { return; }
        if (whatBrakesMe == (whatBrakesMe | (1 << collision.gameObject.layer)))
        {
            bool gotWaveData = collision.gameObject.TryGetComponent<WaterWave>(out WaterWave wv);
            if (!gotWaveData) { BreakMe(Vector2.right, 5f); }
            else { BreakMe(wv.movementSpeed, wv.magnitude); }
        }
    }

    /* Create Trigger Edges
     *  Arguments: -
     *  
     *  Sets the fragile platforms collision box.
     */
    void CreateTriggerEdges()
    {
        Vector3 startPos = platformEdges.spline.GetPosition(0);
        Vector3 endPos = platformEdges.spline.GetPosition(platformEdges.spline.GetPointCount() - 1);

        float xSize = endPos.x - startPos.x;
        float xCenter = endPos.x - (xSize / 2f);
        float ySize = endPos.y - startPos.y + 100f;
        float yCenter = endPos.y - (ySize / 2f);

        fragileIceTrigger.offset = new Vector2(xCenter, yCenter);
        fragileIceTrigger.size = new Vector2(xSize, ySize);
    }

    /* Set Background Profile
     *  Arguments: -
     *  
     *  Sets the sprite shape profile, makes the cracked ice platforms a little different.
     */
    void SetBackgroundProfile()
    {
        iceBergBG.spriteShape = crackedIceProfile;
    }

    /* Break Me
     *  Arguments:
     *  direction : The movement of whatever brakes the platform (typically waterwave)
     *  magnitude : The magnitude of the hit. Changes the speed with which to nudge the platform.
     *  
     *  Starts the sinking process, where the platform gets nudged, then starts sinking.
     */
    public void BreakMe(Vector2 direction, float magnitude = 1f)
    {
        float sinkForce = (direction.x > 0 ? 1f : -1f) * magnitude / DAMPENING;
        StartCoroutine(Sinking(sinkForce));
    }

    // Sinking is a coroutine used for Break Me.
    // magnitudeDirection : the magnitude for nuding and direction to nudge (positive is towards right)
    IEnumerator Sinking(float magnitudeDirection)
    {
        if (sinkingBehaviour.isSinking) { yield break; }
        sinkingBehaviour.isSinking = true;
        float timer = 0f;
        float xNudge = sinkingBehaviour.initialWaveStrength * magnitudeDirection;
        SoundManager.PlaySound("FragileIce", transform.position + new Vector3(0f, 0f, Camera.main.transform.position.z));
        while (mainParent.transform.position.y >= sinkingBehaviour.destroyWhenReachedDepthsOf)
        {
            timer += Time.deltaTime;
            yield return null; // One Frame
            if (timer >= sinkingBehaviour.hangOnTime)
            {
                mainParent.transform.RotateAround(transform.TransformPoint(fragileIceTrigger.offset), transform.forward, -sinkingBehaviour.sinkingRotationAngle * Time.deltaTime);
                mainParent.transform.position += new Vector3(xNudge, -sinkingBehaviour.sinkingSpeedAfterHang, 0f) * Time.deltaTime;
            }
            else
            {
                mainParent.transform.RotateAround(transform.TransformPoint(fragileIceTrigger.offset), transform.forward, -sinkingBehaviour.initialWaveRotationAngle * Time.deltaTime);
                mainParent.transform.position += new Vector3(xNudge, -sinkingBehaviour.sinkingSpeedInitial, 0f) * Time.deltaTime;
            }
            xNudge = Mathf.Clamp(xNudge - Time.deltaTime, 0f, 20f);
        }
        Destroy(mainParent);
    }

    /* Time Until Sinking
     *  Arguments: -
     *  
     *  Returns the time the platform can hang.
     *  Returns: FLOAT, Time the platform was set to hang, in seconds.
     */
    public float TimeUntilSinking()
    {
        return sinkingBehaviour.hangOnTime;
    }
}

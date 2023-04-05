using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(EdgeCollider2D))]
public class SlideSection : MonoBehaviour
{
    const float RELEASEDIST = 0.8f;

    EdgeCollider2D ssc;
    public struct SlideData
    {
        public SlideSection slideInstance;
        public Vector2 attachPoint;
        public bool wasSuccessfull;
    }

    // Start is called before the first frame update
    void Start()
    {
        ssc = GetComponent<EdgeCollider2D>();
    }

    public static SlideData SnapToSlidingEdge(Vector2 startPos, Vector2 velocityIn, out Vector2 velocityOut, ContactFilter2D contactFilter, float snap = 4f)
    {
        velocityOut = velocityIn;
        SlideData returnable = new SlideData() { wasSuccessfull = false };
        RaycastHit2D[] rch = new RaycastHit2D[5];
        int hit = Physics2D.Raycast(startPos, Vector2.down, contactFilter, rch, snap);
        if (hit <= 0) { return returnable; }
        returnable.attachPoint = rch[0].point;
        returnable.slideInstance = rch[0].transform.GetComponent<SlideSection>();
        returnable.wasSuccessfull = true;

        bool forwardMomentum = Mathf.Abs(velocityIn.x) > Mathf.Abs(velocityIn.y);

        velocityOut.y = forwardMomentum ? velocityOut.y * Mathf.Abs(rch[0].normal.y) : velocityOut.y * (1f - Mathf.Abs(rch[0].normal.y)) * 0.8f;
        // Rethink... This is awful.
        if (velocityIn.x > 0f && rch[0].normal.x > 0f) { velocityOut.x = forwardMomentum ? velocityOut.x * (1f + Mathf.Abs(rch[0].normal.x)) : velocityOut.x * (0.8f + Mathf.Abs(rch[0].normal.x)); }
        else if (velocityIn.x >= 0f && rch[0].normal.x <= 0f) { velocityOut.x = forwardMomentum ? velocityOut.x * (1f - Mathf.Abs(rch[0].normal.x)) : velocityOut.x - Mathf.Abs(rch[0].normal.x); }
        else if (velocityIn.x <= 0f && rch[0].normal.x >= 0f) { velocityOut.x = forwardMomentum ? velocityOut.x * (1f - Mathf.Abs(rch[0].normal.x)) : velocityOut.x - Mathf.Abs(rch[0].normal.x); }
        else if (velocityIn.x < 0f && rch[0].normal.x < 0f) { velocityOut.x = forwardMomentum ? velocityOut.x * (1f + Mathf.Abs(rch[0].normal.x)) : velocityOut.x * (0.8f + Mathf.Abs(rch[0].normal.x)); }

        return returnable;
    }

    public static int GetNearesPointInSpline(Vector2 reference, Vector2[] spline, Transform transformToReference)
    {
        int indexWithSmallestDistance = 0;
        float smallestDistance = 9000f;
        int breakIn = -1;
        for (int i = 0; i < spline.Length; i++)
        {
            float distance = Vector2.Distance(reference, transformToReference.TransformPoint(spline[i]));
            if (distance < smallestDistance) { smallestDistance = distance; indexWithSmallestDistance = i; breakIn = 3; }
            else if (breakIn >= 0) { breakIn--; if (breakIn == -1) { return indexWithSmallestDistance; } } 
        }
        return indexWithSmallestDistance;
    }

    public Vector2 MoveAlongSlide(Vector2 currentFeetPos, Vector2 velocity, out bool release, out Vector2 releaseVelocity, bool movingRight = true)
    {
        release = false;
        int i = GetNearesPointInSpline(currentFeetPos, ssc.points, transform);
        Vector2 currentPos = currentFeetPos;
        float velocityMagnitude = velocity.magnitude;
        velocityMagnitude *= Time.deltaTime;
        bool breaking = false;
        if (movingRight)
        {
            i++; if (i > ssc.points.Length - 1) { i = ssc.points.Length - 1; }
            while (breaking == false && i < ssc.points.Length - 1)
            {
                float distDelta = Vector2.Distance(currentPos, transform.TransformPoint(ssc.points[i]));
                if (velocityMagnitude - distDelta < 0f) { currentPos = Vector2.MoveTowards(currentPos, transform.TransformPoint(ssc.points[i]), velocityMagnitude); breaking = true; }
                i++;
            }
            if (Vector2.Distance(currentPos, transform.TransformPoint(ssc.points[ssc.points.Length - 1])) <= RELEASEDIST) { release = true; }
        }
        else
        {
            i--; if (i < 0) { i = 0; }
            while (breaking == false && i >= 0)
            {
                float distDelta = Vector2.Distance(currentPos, transform.TransformPoint(ssc.points[i]));
                if (velocityMagnitude - distDelta < 0f) { currentPos = Vector2.MoveTowards(currentPos, transform.TransformPoint(ssc.points[i]), velocityMagnitude); breaking = true; }
                i--;
            }
            if (Vector2.Distance(currentPos, transform.TransformPoint(ssc.points[0])) <= RELEASEDIST) { release = true; }
        }
        if (currentPos == currentFeetPos) { release = true; }
        velocityMagnitude /= Time.deltaTime;
        velocityMagnitude -= (currentPos.y - currentFeetPos.y) * 1.6f;
        velocityMagnitude = Mathf.Clamp(velocityMagnitude, 0f, 500f);
        releaseVelocity = (currentPos - currentFeetPos).normalized * (velocityMagnitude);
        return currentPos;
    }

    public static bool StickToSlide(Rigidbody2D rb, ContactFilter2D contactFilter, float snap = 4f)
    {
        SlideData returnable = new SlideData() { wasSuccessfull = false };
        RaycastHit2D[] rch = new RaycastHit2D[5];
        int hit = Physics2D.Raycast(rb.position, Vector2.down, contactFilter, rch, snap);
        if (hit <= 0) { return false; }
        return true;
    }
}

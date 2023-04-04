using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(EdgeCollider2D))]
public class SlideSection : MonoBehaviour
{
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

    public static SlideData SnapToSlidingEdge(Vector2 startPos, ContactFilter2D contactFilter, float snap = 4f)
    {
        SlideData returnable = new SlideData() { wasSuccessfull = false };
        RaycastHit2D[] rch = new RaycastHit2D[5];
        int hit = Physics2D.Raycast(startPos, Vector2.down, contactFilter, rch, snap);
        if (hit <= 0) { return returnable; }
        returnable.attachPoint = rch[0].point;
        returnable.slideInstance = rch[0].transform.GetComponent<SlideSection>();
        returnable.wasSuccessfull = true;
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

    public Vector2 MoveAlongSlide(Vector2 currentFeetPos, float velocity, bool movingRight = true)
    {
        int i = GetNearesPointInSpline(currentFeetPos, ssc.points, transform);
        Vector2 currentPos = currentFeetPos;
        velocity /= 80f;
        while (velocity > 0f && i < ssc.points.Length)
        {
            float distDelta = Vector2.Distance(currentPos, transform.TransformPoint(ssc.points[i]));
            velocity -= distDelta;
            currentPos = transform.TransformPoint(ssc.points[i]);
            i++;
            Debug.Log(string.Format("Checked index {0} with delta of {1}", i, distDelta));
        }
        Debug.Log(currentPos);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(EdgeCollider2D))]
public class SlideSection : MonoBehaviour
{
    const float RELEASEDIST = 0.7f;
    const float SLIDECOEFFICENT = 0.6f;

    EdgeCollider2D slidableEdge;
    public struct SlideSnapData
    {
        public SlideSection slideInstance;
        public Vector2 attachPoint;
        public int slideIndex;
        public bool wasSuccessfull;
    }

    public struct SlideMoveData
    {
        public Vector2 newPosition;
        public Vector2 newVelocity;
        public int previousIndex;
        public int travelingDirection;
        public bool shouldRelease;
    }

    // Start is called before the first frame update
    void Start()
    {
        slidableEdge = GetComponent<EdgeCollider2D>();
    }

    /* Snap To Sliding Edge
     *  Static, can be called to find a Sliding Edge and Snap to it.
     *  Arguments:
     *  startPos (Vector2) - The position from which attempt the snap into.
     *  velocityIn (Vector2) - The rigidbodys incoming velocity, used to calculate outward velocity.
     *  velocityOut (Vector2) (OUTWARD) - The calculated velocity to use for rigidbody after snapping.
     *  contactFilter (ContactFilter2D) - The mask and other conditions to use for casting to the slide section.
     *  snap (Float) (Optional) - The maximum distance to try and snap to a slide section.
     *  
     *  Casts a ray from start position to try and find an instance of SlideSection, returns a struct
     *  of information, including a reference to the SlideSection.
     *  Returns:
     *  Slide Data, The Slide Section, the point from where it was found and if we even found a slide in the first place.
     */
    public static SlideSnapData SnapToSlidingEdge(Vector2 startPos, Vector2 velocityIn, out Vector2 velocityOut, ContactFilter2D contactFilter, float snap = 4f)
    {
        velocityOut = velocityIn;
        SlideSnapData returnable = new SlideSnapData() { wasSuccessfull = false };
        RaycastHit2D[] rch = new RaycastHit2D[5];
        int hit = Physics2D.Raycast(startPos, Vector2.down, contactFilter, rch, snap);
        if (hit <= 0) { return returnable; }
        returnable.attachPoint = rch[0].point;
        returnable.slideInstance = rch[0].transform.GetComponent<SlideSection>();
        returnable.wasSuccessfull = true;

        velocityOut = (velocityIn + (rch[0].normal * (velocityIn.magnitude * SLIDECOEFFICENT)));

        return returnable;
    }

    /* Get Nearest Point In Spline
     *  Static, can be called to find the closest index in a provided Vector2 array.
     *  Arguments:
     *  reference (Vector2) - The position which we want to find the closest to.
     *  spline (Vector2 Array) - The array from which the closest point will be chosen.
     *  transform To Reference (Transform) - The transform which owns the spline, to make sure the position matches globally, instead of locally.
     *  
     *  Iterates through an array and finds the nearest point in an array, then returns the closest points index.
     *  Cannot reliably cut-off when distances start increasing, as we cannot know the configuration of the spline.
     *  Returns:
     *  integer, index that is closest to reference.
     */
    protected static int GetNearestPointInSpline(Vector2 reference, Vector2[] spline, Transform transformToReference, int startingIndex = -1)
    {
        int indexWithSmallestDistance = 0;
        float smallestDistance = 9000f;

        if (startingIndex >= 0)
        {
            for (int i = Mathf.Clamp(startingIndex - 4, 0, spline.Length - 1); i < Mathf.Clamp(startingIndex + 4, 0, spline.Length - 1); i++)
            {
                float distance = Vector2.Distance(reference, transformToReference.TransformPoint(spline[i]));
                if (distance < smallestDistance) { smallestDistance = distance; indexWithSmallestDistance = i; }
            }
        }
        else
        {
            for (int i = 0; i < spline.Length; i++)
            {
                float distance = Vector2.Distance(reference, transformToReference.TransformPoint(spline[i]));
                if (distance < smallestDistance) { smallestDistance = distance; indexWithSmallestDistance = i; }
            }
        }
        
        return indexWithSmallestDistance;
    }

    /* Get Spline Direction From Index
     *  Static, can be called to determine if the direction of the spline from an index is towards x positive if advancing forward in the spline,
     *  or if it instead goes to towards positive x when moving backwards in the spline.
     *  Arguments:
     *  spline (Vector2 Array) - The array from which to check the direction.
     *  index To Check From (integer) - The index from which we want to know direction of.
     *  transform To Reference (Transform) - The transform which owns the spline, to make sure the position matches globally, instead of locally.
     *  
     *  Checks adjacent indexes from a pline to determine if we can assume a positive x when moving forward in the array, or not.
     *  Returns:
     *  integer, direction match : 1 = Positive in Spline is Positive in X, -1 = Negative in Spline is Positive in X, 0 neither is true.
     */
    public static int GetSplineDirectionFromIndex(Vector2[] spline, int indexToCheckFrom, Transform transformToReference)
    {
        if (spline.Length < indexToCheckFrom || indexToCheckFrom < 0) { return 0; }
        float xOfPoint = transformToReference.TransformPoint(spline[indexToCheckFrom]).x;
        float xOfPointPlus = 0f;
        float xOfPointMinus = 0f;
        if (indexToCheckFrom != spline.Length - 1) { xOfPointPlus = transformToReference.TransformPoint(spline[indexToCheckFrom + 1]).x; }
        if (indexToCheckFrom != 0) { xOfPointMinus = transformToReference.TransformPoint(spline[indexToCheckFrom - 1]).x; }

        if (xOfPointPlus > xOfPoint && xOfPoint > xOfPointMinus) { return 1; }
        if (xOfPointMinus > xOfPoint && xOfPoint > xOfPointPlus) { return -1; }
        if (xOfPointMinus > xOfPointPlus) { return -1; }
        if (xOfPointPlus > xOfPointMinus) { return 1; }
        return 0;
    }

    public SlideMoveData MoveAlongSlide(Vector2 currentFeetPos, Vector2 velocity, int moveDirection, int startIndex = -1)
    {
        int i = GetNearestPointInSpline(currentFeetPos, slidableEdge.points, transform, startIndex);
        SlideMoveData returnPackage = new SlideMoveData() { travelingDirection = moveDirection, previousIndex = i };

        Vector2 currentPos = currentFeetPos;
        float velocityMagnitude = velocity.magnitude;
        velocityMagnitude *= Time.deltaTime;
        bool breaking = false;
        if (moveDirection == 0 && velocity.x > 0f) { moveDirection = GetSplineDirectionFromIndex(slidableEdge.points, i, transform); }
        else if (moveDirection == 0 && velocity.x <= 0f) { moveDirection = -1 * GetSplineDirectionFromIndex(slidableEdge.points, i, transform); }
        if (moveDirection == 1)
        {
            i++; if (i > slidableEdge.points.Length - 1) { i = slidableEdge.points.Length - 1; }
            while (breaking == false && i < slidableEdge.points.Length)
            {
                float distDelta = Vector2.Distance(currentPos, transform.TransformPoint(slidableEdge.points[i]));
                if (velocityMagnitude - distDelta < 0f) { currentPos = Vector2.MoveTowards(currentPos, transform.TransformPoint(slidableEdge.points[i]), velocityMagnitude); breaking = true; }
                i++;
            }
            if (Vector2.Distance(currentPos, transform.TransformPoint(slidableEdge.points[slidableEdge.points.Length - 1])) <= RELEASEDIST) { returnPackage.shouldRelease = true; }
        }
        else if (moveDirection == -1)
        {
            i--; if (i < 0) { i = 0; }
            while (breaking == false && i >= 0)
            {
                float distDelta = Vector2.Distance(currentPos, transform.TransformPoint(slidableEdge.points[i]));
                if (velocityMagnitude - distDelta < 0f) { currentPos = Vector2.MoveTowards(currentPos, transform.TransformPoint(slidableEdge.points[i]), velocityMagnitude); breaking = true; }
                i--;
            }
            if (Vector2.Distance(currentPos, transform.TransformPoint(slidableEdge.points[0])) <= RELEASEDIST) { returnPackage.shouldRelease = true; }
        }
        if (currentPos == currentFeetPos) { returnPackage.shouldRelease = true; }
        velocityMagnitude /= Time.deltaTime;
        velocityMagnitude -= (currentPos.y - currentFeetPos.y) * 1.6f;
        velocityMagnitude = Mathf.Clamp(velocityMagnitude, 0f, 500f);
        returnPackage.newVelocity = (currentPos - currentFeetPos).normalized * (velocityMagnitude);
        returnPackage.travelingDirection = moveDirection;
        if (currentPos.y > currentFeetPos.y && velocityMagnitude < 0.1f) { returnPackage.newVelocity *= -1; }
        returnPackage.newPosition = currentPos;
        return returnPackage;
    }

    public static bool StickToSlide(Rigidbody2D rb, ContactFilter2D contactFilter, float snap = 4f)
    {
        SlideSnapData returnable = new SlideSnapData() { wasSuccessfull = false };
        RaycastHit2D[] rch = new RaycastHit2D[5];
        int hit = Physics2D.Raycast(rb.position, Vector2.down, contactFilter, rch, snap);
        if (hit <= 0) { return false; }
        return true;
    }
}

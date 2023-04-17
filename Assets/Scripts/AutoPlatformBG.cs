using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/* 
 * A class that handles setting up background for platforms.
 * The IcebergSettings allow control over offsets.
 */
public class AutoPlatformBG : MonoBehaviour
{
    public SpriteShapeController backgroundIceBerg;
    public IcebergSettings backgroundSettings;
    protected SpriteShapeController platformShape;
    public bool setZOrderBasedOnYHeight = true;

    [System.Serializable]
    public class IcebergSettings
    {
        [Header("Use Order: Top-Left, Top-Right, Bottom-Left, Bottom-Right")]
        public int[] pointsToStretch = new int[4] { 0, 1, 2, 3 };
        public Vector3 leftBottomOffset = new Vector3(-5f, -70f, 0f);
        public Vector3 rightBottomOffset = new Vector3(5f, -70f, 0f);
    }

    // Start is called before the first frame update
    void Start()
    {
        platformShape = GetComponent<SpriteShapeController>();
        if (backgroundIceBerg != null) { GenerateBackground(); }
        if (setZOrderBasedOnYHeight) { ArrangeBackground(); }
    }

    /* Generate Background
     *  Arguments: -
     *  
     *  Automatically generate all slide section's iceberg texture underneath the slide section.
     */
    void GenerateBackground()
    {
        void SetTangentOfSplinePoint(int index)
        {
            backgroundIceBerg.spline.SetTangentMode(index, platformShape.spline.GetTangentMode(index));
            backgroundIceBerg.spline.SetLeftTangent(index, platformShape.spline.GetLeftTangent(index));
            backgroundIceBerg.spline.SetRightTangent(index, platformShape.spline.GetRightTangent(index));
        }

        Vector3 startPoint = platformShape.spline.GetPosition(0);
        Vector3 endPoint = platformShape.spline.GetPosition(platformShape.spline.GetPointCount() - 1);

        backgroundIceBerg.spline.SetPosition(backgroundSettings.pointsToStretch[0], startPoint);
        backgroundIceBerg.spline.SetPosition(backgroundSettings.pointsToStretch[1], endPoint);
        backgroundIceBerg.spline.SetPosition(backgroundSettings.pointsToStretch[3], startPoint + backgroundSettings.leftBottomOffset);
        backgroundIceBerg.spline.SetPosition(backgroundSettings.pointsToStretch[2], endPoint + backgroundSettings.rightBottomOffset);
        SetTangentOfSplinePoint(0);
        for (int a = 1; a < platformShape.spline.GetPointCount() - 1; a++)
        {
            backgroundIceBerg.spline.InsertPointAt(a, platformShape.spline.GetPosition(a));
            SetTangentOfSplinePoint(a);
        }
        SetTangentOfSplinePoint(platformShape.spline.GetPointCount() - 1);

        // Ensure that the camera actually has the relevant information to draw the
        // reshaped sprite when it enters the screen. Add a bit extra for safety.
        float extraBounds = 4f;
        Bounds spriteshapeBounds = platformShape.spriteShapeRenderer.bounds;
        spriteshapeBounds.size = new Vector3(
            spriteshapeBounds.size.x + Mathf.Abs(backgroundSettings.leftBottomOffset.x) + extraBounds,
            spriteshapeBounds.size.y + Mathf.Abs(backgroundSettings.leftBottomOffset.y) + extraBounds,
            spriteshapeBounds.size.z);
        backgroundIceBerg.spriteShapeRenderer.bounds = spriteshapeBounds;
        Bounds spriteshapeLocalBounds = platformShape.spriteShapeRenderer.localBounds;
        spriteshapeLocalBounds.size = new Vector3(
            spriteshapeLocalBounds.size.x + Mathf.Abs(backgroundSettings.leftBottomOffset.x) + extraBounds,
            spriteshapeLocalBounds.size.y + Mathf.Abs(backgroundSettings.leftBottomOffset.y) + extraBounds,
            spriteshapeLocalBounds.size.z);
        backgroundIceBerg.spriteShapeRenderer.SetLocalAABB(spriteshapeLocalBounds);
    }

    /* Arrange Background
     *  Arguments: -
     *  
     *  Sets Z Order based on transform position if needed.
     */
    void ArrangeBackground()
    {
        bool isValid = TryGetComponent<SpriteShapeRenderer>(out SpriteShapeRenderer ssr_platform);
        if (!isValid) { return; }
        ssr_platform.sortingOrder = Mathf.RoundToInt(-ssr_platform.transform.position.y * 2f);
        if (backgroundIceBerg == null) { return; }
        bool isAlsoValid = backgroundIceBerg.TryGetComponent<SpriteShapeRenderer>(out SpriteShapeRenderer ssr_background);
        if (!isAlsoValid) { return; }
        ssr_background.sortingOrder = Mathf.RoundToInt(-ssr_platform.transform.position.y * 2f - 1);
    }
}

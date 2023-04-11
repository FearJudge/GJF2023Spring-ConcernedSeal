using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

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

    // Automatically generate all slide section's iceberg texture underneath the slide section.
    void GenerateBackground()
    {
        backgroundIceBerg.spline.SetPosition(backgroundSettings.pointsToStretch[0], platformShape.spline.GetPosition(0));
        backgroundIceBerg.spline.SetPosition(backgroundSettings.pointsToStretch[1], platformShape.spline.GetPosition(platformShape.spline.GetPointCount() - 1));
        backgroundIceBerg.spline.SetPosition(backgroundSettings.pointsToStretch[3], platformShape.spline.GetPosition(0) + backgroundSettings.leftBottomOffset);
        backgroundIceBerg.spline.SetPosition(backgroundSettings.pointsToStretch[2], platformShape.spline.GetPosition(platformShape.spline.GetPointCount() - 1) + backgroundSettings.rightBottomOffset);
        for (int a = 1; a < platformShape.spline.GetPointCount() - 1; a++)
        {
            backgroundIceBerg.spline.InsertPointAt(a, platformShape.spline.GetPosition(a));
            backgroundIceBerg.spline.SetTangentMode(a, platformShape.spline.GetTangentMode(a));
            backgroundIceBerg.spline.SetLeftTangent(a, platformShape.spline.GetLeftTangent(a));
            backgroundIceBerg.spline.SetRightTangent(a, platformShape.spline.GetRightTangent(a));
        }
    }

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

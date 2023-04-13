using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBG : MonoBehaviour, WaterRiseScript.IWaterRisable
{
    public float scrollingSpeed = 1f;
    public Transform targetTransform;
    public Vector2 resistance = Vector2.one;
    Vector3 initialOffset;
    Vector3 offset = Vector3.zero;
    public bool isPartOfGlobalWaterPool = false;

    private void Awake()
    {
        offset = transform.position;
        initialOffset = offset;
        if (targetTransform == null) { targetTransform = Camera.main.transform; }
        if (isPartOfGlobalWaterPool) { WaterRiseScript.waterInstances.Add(this); StartCoroutine(WaterRiseScript.StartWaterRising()); }
    }

    private void OnDestroy()
    {
        if (isPartOfGlobalWaterPool) { WaterRiseScript.waterInstances.Remove(this); WaterRiseScript.PauseWaterRising(); }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = (targetTransform.position * resistance * -scrollingSpeed) + new Vector2(offset.x, offset.y);
    }

    public void AdjustWaterLevel()
    {
        offset = initialOffset + new Vector3(0f, WaterRiseScript.waterLevel, 0f);
    }
}

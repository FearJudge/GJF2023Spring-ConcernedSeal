using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterScript : MonoBehaviour
{
    public bool isPartOfGlobalWaterPool = true;
    float startingY = 0f;

    private void Start()
    {
        if (isPartOfGlobalWaterPool) { WaterRiseScript.waterInstances.Add(this); StartCoroutine(WaterRiseScript.StartWaterRising()); }
        startingY = transform.position.y;
    }

    private void OnDestroy()
    {
        if (isPartOfGlobalWaterPool) { WaterRiseScript.waterInstances.Remove(this); WaterRiseScript.PauseWaterRising(); }
    }

    public void AdjustWaterLevel()
    {
        transform.position = new Vector3(transform.position.x, startingY + WaterRiseScript.waterLevel, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            pc.WaterBounce();
        }
    }
}

public static class WaterRiseScript
{
    public static List<WaterScript> waterInstances = new List<WaterScript>();

    public static float waterLevel = 0f;
    public static float waterLevelSpeed = 0.002f;
    public static float waterLevelDelay = 0.1f;
    static bool waterRising = false;

    public static IEnumerator StartWaterRising()
    {
        if (waterRising) { yield break; }
        waterRising = true;
        while (waterRising)
        {
            yield return new WaitForSeconds(waterLevelDelay);
            waterLevel += waterLevelSpeed;
            for (int i = 0; i < waterInstances.Count; i++)
            {
                waterInstances[i].AdjustWaterLevel();
            }
        }
    }

    public static void PauseWaterRising()
    {
        waterRising = false;
    }
}

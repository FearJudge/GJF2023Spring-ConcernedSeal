using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterScript : MonoBehaviour, WaterRiseScript.IWaterRisable
{
    public bool isPartOfGlobalWaterPool = true;
    public bool instantDrown = false;
    public GameObject prefabOfWaterSplash;
    Collider2D ownCollider;

    float startingY = 0f;

    private void Start()
    {
        if (isPartOfGlobalWaterPool) { WaterRiseScript.waterInstances.Add(this); StartCoroutine(WaterRiseScript.StartWaterRising()); }
        startingY = transform.position.y;
        ownCollider = GetComponent<Collider2D>();
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
        Rigidbody2D hitActor = collision.gameObject.GetComponent<Rigidbody2D>();
        if (hitActor != null && prefabOfWaterSplash != null)
        {
            Vector3 pos = collision.ClosestPoint(transform.position);
            GameObject settingsForPrefab = Instantiate(prefabOfWaterSplash, pos, Quaternion.identity);
            ParticleSystem psForPrefab = settingsForPrefab.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule psmmForPrefab = psForPrefab.main;
            float hitImpact = Mathf.Clamp(Mathf.Abs(hitActor.velocity.y) * 0.2f, 0.1f, 4f);
            psmmForPrefab.startSpeedMultiplier = hitImpact;
            psmmForPrefab.startLifetimeMultiplier = Mathf.Clamp(hitImpact, 0.5f, 2f);
            psForPrefab.Play();
        }
        if (collision.gameObject.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            if (instantDrown) { pc.Drowned(true); }
            pc.submerged = true;
            pc.WaterBounce();
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            pc.submerged = false;
            pc.Breathe();
        }
    }
}

public static class WaterRiseScript
{
    public interface IWaterRisable
    {
        void AdjustWaterLevel();
    }

    public static List<IWaterRisable> waterInstances = new List<IWaterRisable>();

    public static float waterLevel = 0f;
    static float waterTempPrivate = 30f;
    public static float WaterTemp { get { return waterTempPrivate; } set { ModifyWaterTemperature(value); } }
    static float waterLevelSpeed = 0.003f;
    static float waterLevelDelay = 0.1f;
    static bool waterRising = false;

    static void ModifyWaterTemperature(float newValue)
    {
        waterTempPrivate = newValue;
        waterLevelDelay = waterTempPrivate / 300f;
        waterLevelSpeed = waterTempPrivate / 10000f;
    }

    public static IEnumerator StartWaterRising()
    {
        if (waterRising) { yield break; }
        waterLevel = 0f;
        waterTempPrivate = LevelManager.waterTemperature;
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

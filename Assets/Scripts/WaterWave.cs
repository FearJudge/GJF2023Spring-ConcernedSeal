using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * A wave used to knock over fragile platforms.
 */
public class WaterWave : MonoBehaviour
{
    public Vector2 movementSpeed = new Vector2(2f, 0f);
    public float magnitude = 5f;

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(movementSpeed.x, movementSpeed.y, 0f) * Time.deltaTime;
    }
}

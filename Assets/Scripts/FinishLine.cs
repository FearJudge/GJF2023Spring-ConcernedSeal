using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isPlayer = collision.gameObject.TryGetComponent<PlayerController>(out PlayerController pc);
        if (!isPlayer) { return; }
        LevelLoader.PlayerHasSucceeded();
    }
}

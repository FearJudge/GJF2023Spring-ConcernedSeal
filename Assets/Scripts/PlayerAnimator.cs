using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public PlayerController controller;
    public SpriteRenderer playerCharacter;

    // Update is called once per frame
    void Update()
    {
        CheckForPlayerDirection();
        CheckForStoredSlide();
    }

    void CheckForPlayerDirection()
    {
        void VelocityBased()
        {
            if (controller.playerPhysics.velocity.x > 0f) { playerCharacter.flipX = false; }
            else if (controller.playerPhysics.velocity.x < 0f) { playerCharacter.flipX = true; }
        }

        void SlideBased()
        {
            if (controller.storedDirection > 0) { playerCharacter.flipX = false; }
            else if (controller.storedDirection < 0) { playerCharacter.flipX = true; }
        }

        if (controller.storedSlide == null) { VelocityBased(); }
        else { SlideBased(); }
    }

    void CheckForStoredSlide()
    {
        if (controller.storedSlide == null) { playerCharacter.transform.localPosition = Vector3.zero; playerCharacter.transform.localRotation = Quaternion.identity; return; }

        float xLimits = 0.6f;
        float yLimits = 1.5f;

        Vector2 ridingAngle = controller.storedNormal;
        playerCharacter.transform.localRotation = Quaternion.AngleAxis(controller.storedDirection > 0
            ? Vector2.SignedAngle(Vector2.up, ridingAngle) : Vector2.SignedAngle(Vector2.up, -ridingAngle), Vector3.forward);
        playerCharacter.transform.localPosition = new Vector3(Mathf.Clamp(controller.storedDirection * ridingAngle.x * xLimits, -xLimits, xLimits),
            Mathf.Clamp(controller.storedDirection * ridingAngle.y * yLimits, -yLimits, 0f), 0f);
    }
}

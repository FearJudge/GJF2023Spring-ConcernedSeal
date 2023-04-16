using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public PlayerController controller;
    public Animator animator;
    public SpriteRenderer playerCharacter;

    public ParticleSystem slideParticles;
    ParticleSystem.MainModule slideParticlesMain;
    public ParticleSystem snowParticles;
    ParticleSystem.MainModule snowParticlesMain;
    AudioSource slideSound = null;
    bool flipInProgress = false;
    public float flipTime = 1f;

    private void Awake()
    {
        if (slideParticles == null) { slideParticles = transform.Find("SlidingParticles").GetComponent<ParticleSystem>(); }
        if (slideParticles != null) { slideParticlesMain = slideParticles.main; }
        if (snowParticles == null) { snowParticles = transform.Find("SnowParticles").GetComponent<ParticleSystem>(); }
        if (snowParticles != null) { snowParticlesMain = snowParticles.main; }
        PlayerController.HaveFinishedLevel += Flip;
        PlayerController.HaveReleasedFromSlide += Flip;
        PlayerController.HaveJumpedFromSlide += QuickFlip;
        PlayerController.HaveHitWater += GotWet;
        PlayerController.HaveLandedOnSnow += SnowPuff;
    }

    private void OnDestroy()
    {
        PlayerController.HaveFinishedLevel -= Flip;
        PlayerController.HaveReleasedFromSlide -= Flip;
        PlayerController.HaveJumpedFromSlide -= QuickFlip;
        PlayerController.HaveHitWater -= GotWet;
        PlayerController.HaveLandedOnSnow -= SnowPuff;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForPlayerDirection();
        CheckForStoredSlide();
        UpdateAnimatorState();
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
        if (controller.storedSlide == null) {
            playerCharacter.transform.localPosition = Vector3.zero;
            if (!flipInProgress)
            {
                playerCharacter.transform.localRotation = Quaternion.identity;
            }
            if (slideParticles.isPlaying) { slideParticles.Stop(); }
            if (slideSound != null) { slideSound.Stop(); slideSound = null; }
            return; }

        float xLimits = 0.6f;
        float yLimits = 1.5f;

        if (slideParticles.isStopped) { slideParticles.Play(); slideSound = SoundManager.PlaySound("slide"); }
        Vector2 ridingAngle = controller.storedNormal;
        playerCharacter.transform.localRotation = Quaternion.AngleAxis(controller.storedDirection > 0
            ? Vector2.SignedAngle(Vector2.up, ridingAngle) : Vector2.SignedAngle(Vector2.up, -ridingAngle), Vector3.forward);
        playerCharacter.transform.localPosition = new Vector3(Mathf.Clamp(controller.storedDirection * ridingAngle.x * xLimits, -xLimits, xLimits),
            Mathf.Clamp(controller.storedDirection * ridingAngle.y * yLimits, -yLimits, 0f), 0f);
        if (slideSound != null)
        {
            float volume = Mathf.Clamp(controller.storedVelocity.magnitude / 20f, 0f, 1f);
            slideSound.volume = volume;
        } 
    }

    void UpdateAnimatorState()
    {
        bool isSliding = controller.storedSlide != null;
        animator.SetBool("Sliding", isSliding);
        if (!isSliding) { animator.SetFloat("Movement", Mathf.Abs(controller.playerPhysics.velocity.x)); }
        else { animator.SetFloat("Movement", Mathf.Abs(controller.storedVelocity.x)); }
    }

    public void Flip()
    {
        StartCoroutine(DoAFlip(flipTime));
    }

    public void QuickFlip()
    {
        StartCoroutine(DoAFlip(flipTime / 3f));
    }

    public void GotWet()
    {
        animator.SetTrigger("Wet");
    }

    public void SnowPuff()
    {
        float impact = controller.playerPhysics.velocity.magnitude * 0.12f;
        snowParticlesMain.startSpeedMultiplier = impact;
        snowParticlesMain.startLifetimeMultiplier = impact;
        snowParticles.Play();
    }

    IEnumerator DoAFlip(float rotTime = 2f)
    {
        if (flipInProgress) { yield break; }
        flipInProgress = true;
        float dir = (controller.playerPhysics.velocity.x > 0f) ? 1f : -1f;
        float rotPerMs = 360f / rotTime;
        float rot = 0f;
        while (rot < 360f)
        {
            rot += rotPerMs * Time.deltaTime;
            rot = Mathf.Clamp(rot, 0f, 360f);
            playerCharacter.transform.localRotation = Quaternion.AngleAxis(dir * rot, Vector3.forward);
            yield return null;
        }
        flipInProgress = false;
    }
}

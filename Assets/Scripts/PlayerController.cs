using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const float SLIDEOFFSETCONST = 0.05f;
    const float MAXVELOCITYAIR = 12f;
    const float MAXVELOCITYGROUND = 14f;

    public delegate void PlayerEvent();
    public static PlayerEvent HaveLandedOnSnow;
    public static PlayerEvent HaveBegunSliding;
    public static PlayerEvent HaveHitWater;
    public static PlayerEvent HaveDrowned;
    public static PlayerEvent HaveJumped;

    [HideInInspector] public Rigidbody2D playerPhysics;
    CapsuleCollider2D playerCollision;
    CircleCollider2D feetCollider;
    [HideInInspector] public SlideSection storedSlide;
    private SlideSection spamPrevention;
    [SerializeField] private PlayerControls playerButtons;
    [SerializeField] private PlayerStats playerVariables;

    [System.Serializable]
    class PlayerControls
    {
        public KeyCode btnForward = KeyCode.D;
        public KeyCode btnBackwards = KeyCode.A;
        public KeyCode btnJump = KeyCode.W;
        public KeyCode btnCrouch = KeyCode.S;
        public KeyCode btnDive = KeyCode.E;
    }

    [System.Serializable]
    class PlayerStats
    {
        public float playerSpeed = 0.10f;
        public float playerSpeedAir = 0.10f;
        public float jumpVelocity = 4f;
        public float startingOxygen = 4f;
        public float wetPenaltyMinDuration = 2f;
        public float wetPenaltyForceMultiplier = 0.15f;
    }

    public LayerMask layerGround;
    public LayerMask layerSlide;
    [HideInInspector] public bool submerged = false;
    float jumps = 1;
    float submergedOxygen = 4f;
    float wetPenaltyTimer = 0f;
    [SerializeField] float wetPenaltyMultiplier = 1f;
    [HideInInspector] public Vector2 storedVelocity = Vector2.zero;
    [HideInInspector] public Vector2 storedNormal = Vector2.up;
    [HideInInspector] public int storedIndex = -1;
    int currentStoredDirection = 0;
    int previousStoredDirection = 0;
    [HideInInspector] public int storedDirection { get { return currentStoredDirection; } set { currentStoredDirection = value; if (value != 0) { previousStoredDirection = value; } } }
    private Vector2 offsetToFeetCheck;
    private Vector2 normalInfluece = Vector2.up;
    private bool storedPlatformExists = false;
    public string storedPlatformName = "";
    private Vector3 storedPlatformPos = Vector3.zero;
    bool amPaused = false;
    Collider2D[] collisionsWithFeet;
    int platformsStandingOnCount;

    /* Start
     * 
     * Called before first frame.
     * Used to initialize critical variables and check settings.
     */
    void Start()
    {
        submergedOxygen = playerVariables.startingOxygen;
        playerPhysics = GetComponent<Rigidbody2D>();
        playerCollision = GetComponent<CapsuleCollider2D>();
        feetCollider = transform.Find("FT_Feet").GetComponent<CircleCollider2D>();
        offsetToFeetCheck = new Vector2(0f, (playerCollision.size.y / 2f) + 0.1f);
    }

    /* Update
     * 
     * Called every frame.
     * Used for movement and other character critical checks.
     */
    void Update()
    {
        if (LevelLoader.pausedPlayer && !amPaused) { playerPhysics.isKinematic = true; amPaused = true; return; }
        else if (!LevelLoader.pausedPlayer && amPaused) { playerPhysics.isKinematic = false; amPaused = false; }
        else if (LevelLoader.pausedPlayer) { return; }

        CheckCollisionToGround(out collisionsWithFeet, out platformsStandingOnCount, false);
        StickToMovingPlatforms();
        CheckCollisionToGround(out collisionsWithFeet, out platformsStandingOnCount, true);
        if (Input.GetKey(playerButtons.btnForward)) { PlayerMove(1f); }
        if (Input.GetKey(playerButtons.btnBackwards)) { PlayerMove(-1f); }
        if (Input.GetKey(playerButtons.btnCrouch) && storedSlide == null) { PlayerSlide(); }
        else if (storedSlide != null) { PlayerSlide(); }
        if (Input.GetKeyDown(playerButtons.btnCrouch) && storedSlide != null) { PlayerSlideStop(); }
        if (Input.GetKeyDown(playerButtons.btnJump)) { bool ignore = PlayerSlideStop(); if (ignore) { PlayerJumpOffSlide(storedNormal); } else { PlayerJump(); } }
        Drowned();
    }

    /* Player Move
     *  Arguments: direction, multiplier for movement that determines wether to go
     *  right (positive) or left (negative).
     *  
     *  Does not handle input directly.
     */
    void PlayerMove(float direction)
    {
        if (storedSlide != null) { storedVelocity += new Vector2(direction * Time.deltaTime, 0f); }
        if ( platformsStandingOnCount <= 0 )
        {
            if (playerPhysics.velocity.magnitude > MAXVELOCITYAIR && direction * playerPhysics.velocity.x > 0f) { return; }
            playerPhysics.AddForce(Vector2.right * direction * playerVariables.playerSpeedAir * Time.deltaTime * wetPenaltyMultiplier);
            return;
        }
        else
        {
            if (wetPenaltyMultiplier != 1f && wetPenaltyTimer <= 0f) { wetPenaltyMultiplier = 1f; wetPenaltyTimer = 0f; }
            if (playerPhysics.velocity.magnitude > MAXVELOCITYGROUND && direction * playerPhysics.velocity.x > 0f) { return; }
            playerPhysics.AddForce(Vector2.right * direction * playerVariables.playerSpeed * Time.deltaTime * wetPenaltyMultiplier);
        }
    }

    /* Player Slide
     *  Arguments: state, boolean that controls wether to slide or not.
     *  
     *  Does not handle input directly.
     */
    void PlayerSlide()
    {
        if (spamPrevention != null) { return; }
        float snapDistance = 0.5f;

        if (storedSlide != null)
        {
            playerPhysics.isKinematic = true;
            playerPhysics.velocity = Vector2.zero;
            SlideSection.SlideMoveData smd = storedSlide.MoveAlongSlide(
                new Vector2(transform.position.x, transform.position.y) - offsetToFeetCheck,
                storedVelocity, storedDirection, storedIndex);

            storedVelocity = smd.newVelocity;
            storedDirection = smd.travelingDirection;
            if (!smd.useOldNormal) { storedNormal = smd.newNormal; }
            storedIndex = smd.previousIndex;

            transform.position = smd.newPosition + offsetToFeetCheck;

            if (smd.shouldDrop) { PlayerSlideStop(); }
            else if (smd.shouldRelease) { PlayerSlideStop(); PlayerJump(true); }
            return;
        }
        SlideSection.SlideSnapData slide = SlideSection.SnapToSlidingEdge(playerPhysics.position,
            playerPhysics.velocity,
            out Vector2 velocityUponImpact,
            new ContactFilter2D() { layerMask = layerSlide, useLayerMask = true},
            (playerCollision.size.y / 2) + snapDistance);
        storedVelocity = velocityUponImpact;
        if (!slide.wasSuccessfull) { return; }
        transform.position = new Vector3(slide.attachPoint.x, slide.attachPoint.y + (playerCollision.size.y/2) + SLIDEOFFSETCONST, transform.position.z);
        storedSlide = slide.slideInstance;
        transform.SetParent(storedSlide.transform);
        HaveBegunSliding?.Invoke();
    }

    void StickToMovingPlatforms()
    {
        void ModifyPositionBasedOnUnderlyingPlatform(Vector3 newPlatformPos)
        {
            Vector3 change = newPlatformPos - storedPlatformPos;
            transform.position += change;
            storedPlatformPos = newPlatformPos;
        }

        if (platformsStandingOnCount <= 0) { storedPlatformExists = false; storedPlatformPos = Vector3.zero; return; }
        else
        {
            if (storedPlatformExists == false) { storedPlatformExists = true; storedPlatformName = collisionsWithFeet[0].gameObject.name; storedPlatformPos = collisionsWithFeet[0].transform.position;
                HaveLandedOnSnow?.Invoke(); return; }
            for (int a = 0; a < collisionsWithFeet.Length; a++)
            {
                if (collisionsWithFeet[a].gameObject.name == storedPlatformName) { ModifyPositionBasedOnUnderlyingPlatform(collisionsWithFeet[a].transform.position); break; }
            }
        }
    }

    private void CheckCollisionToGround(out Collider2D[] colliders, out int a, bool allowTriggers = true)
    {
        colliders = new Collider2D[20];
        a = feetCollider.OverlapCollider(new ContactFilter2D() { useTriggers = allowTriggers, useLayerMask = true, layerMask = layerGround }, colliders);
        if (a > 0 && allowTriggers == false) { jumps = 1; }
    }

    public void Drowned(bool ignoreOxygen = false)
    {
        if (submerged) { submergedOxygen -= Time.deltaTime; GotMoist(); }
        else if (wetPenaltyTimer > 0f) { wetPenaltyTimer -= Time.deltaTime; }
        if (submergedOxygen > 0f && !ignoreOxygen) { return; }
        HaveDrowned?.Invoke();
        LevelLoader.PlayerHasFailed();
    }

    void GotMoist()
    {
        wetPenaltyTimer = playerVariables.wetPenaltyMinDuration;
        wetPenaltyMultiplier = playerVariables.wetPenaltyForceMultiplier;
    }

    public void Breathe()
    {
        submergedOxygen = playerVariables.startingOxygen;
    }
    
    bool PlayerSlideStop()
    {
        if (storedSlide == null) { return false; }
        spamPrevention = storedSlide;
        Invoke("ReleaseSlide", 0.25f);
        playerPhysics.isKinematic = false;
        playerPhysics.velocity = storedVelocity;
        storedVelocity = Vector2.zero;
        // Do not clear stored normal.
        storedDirection = 0;
        storedIndex = -1;
        storedSlide = null;
        transform.SetParent(null);
        return true;
    }
    void ReleaseSlide()
    {
        spamPrevention = null;
    }

    public void WaterBounce()
    {
        if (Mathf.Abs(playerPhysics.velocity.x) < 6f) { return; }
        playerPhysics.velocity = new Vector2(playerPhysics.velocity.x * 0.6f, Mathf.Abs(playerPhysics.velocity.y) * 0.6f);
        GotMoist();
        HaveHitWater?.Invoke();
    }

    /* Player Jump
     *  
     *  Does not handle input directly.
     */
    void PlayerJump(bool ignoreCheck = false)
    {
        if (!ignoreCheck)
        {
            if (platformsStandingOnCount <= 0 || jumps <= 0) { return; }
        }
        playerPhysics.velocity = new Vector2(playerPhysics.velocity.x, playerPhysics.velocity.y + playerVariables.jumpVelocity);
        jumps--;
        HaveJumped?.Invoke();
    }

    void PlayerJumpOffSlide(Vector2 normalOnSlide)
    {
        playerPhysics.velocity = playerPhysics.velocity + ((normalOnSlide) * previousStoredDirection * playerVariables.jumpVelocity);
    }
}

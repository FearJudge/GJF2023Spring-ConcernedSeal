using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Handles player input and movement as well as abilities like sliding, drowning and jumping.
 */
public class PlayerController : MonoBehaviour
{
    const float SLIDEOFFSETCONST = 0.05f;
    const float MAXVELOCITYAIR = 12f;
    const float MAXVELOCITYGROUND = 14f;
    const float TIMESTOPDURATION = 0.25f;

    public delegate void PlayerEvent();
    public static PlayerEvent HaveLandedOnSnow;
    public static PlayerEvent HaveBegunSliding;
    public static PlayerEvent HaveHitWater;
    public static PlayerEvent HaveDrowned;
    public static PlayerEvent HaveJumped;
    public static PlayerEvent HaveReleasedFromSlide;
    public static PlayerEvent HaveJumpedFromSlide;
    public static PlayerEvent HaveFinishedLevel;
    public static PlayerEvent HaveReachedHighVelocityGround;
    public static PlayerEvent HaveMovedToGoal;

    public static float slidingTime = 0f;
    public static int hitsToWater = 0;
    public static string startingInput;

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
        public string btnHorizontalControl = "Horizontal";
        public string btnSlide = "Slide";
        public string btnJump = "Jump";
        public string btnPauseMenu = "Cancel";
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
        public float grabbleSpeed = 2f;
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
    bool finished = false;
    Collider2D[] collisionsWithFeet;
    int platformsStandingOnCount;

    /* Start
     * 
     * Called before first frame.
     * Used to initialize critical variables and check settings.
     */
    void Start()
    {
        startingInput = playerButtons.btnSlide;
        slidingTime = 0f;
        hitsToWater = 0;
        submergedOxygen = playerVariables.startingOxygen;
        playerPhysics = GetComponent<Rigidbody2D>();
        playerCollision = GetComponent<CapsuleCollider2D>();
        feetCollider = transform.Find("FT_Feet").GetComponent<CircleCollider2D>();
        offsetToFeetCheck = new Vector2(0f, (playerCollision.size.y / 2f) + 0.1f);
        FinishLine.FinishedPositionRing += PlayerHaltAndGrabbleTo;
    }

    private void OnDestroy()
    {
        FinishLine.FinishedPositionRing -= PlayerHaltAndGrabbleTo;
    }

    /* Update
     * 
     * Called every frame.
     * Used for movement and other character critical checks.
     */
    void Update()
    {
        if (LevelLoader.pausedPlayer && !amPaused) { playerPhysics.isKinematic = true; playerPhysics.velocity = Vector2.zero; amPaused = true; return; }
        else if (!LevelLoader.pausedPlayer && amPaused) { playerPhysics.isKinematic = false; amPaused = false; }
        else if (LevelLoader.pausedPlayer) { return; }

        CheckCollisionToGround(out collisionsWithFeet, out platformsStandingOnCount, false);
        StickToMovingPlatforms();
        CheckCollisionToGround(out collisionsWithFeet, out platformsStandingOnCount, true);
        CheckPlayerInputs();
        Drowned();
    }

    /* Check Player Inputs
     *  Arguments: -
     *  
     *  Reads Inputs and controls the player accordingly.
     */
    private void CheckPlayerInputs()
    {
        float x = Input.GetAxisRaw(playerButtons.btnHorizontalControl);
        if (x != 0f) { PlayerMove(x); }
        // Sliding
        if (Input.GetButtonDown(playerButtons.btnSlide) && storedSlide != null) { PlayerSlideStop(); }
        else if (Input.GetButton(playerButtons.btnSlide) && storedSlide == null) { PlayerSlide();  }
        else if (storedSlide != null) { PlayerSlide(); }
        // Jump and Jump Off
        if (Input.GetButtonDown(playerButtons.btnJump))
        {
            bool ignore = PlayerSlideStop(); if (ignore)
            { PlayerJumpOffSlide(storedNormal); }
            else { PlayerJump(); }
        }
        // Pausing
        if (Input.GetButtonDown(playerButtons.btnPauseMenu)) { LevelLoader.PlayerIndicatesPauseChange(!LevelLoader.pausedPlayer); }
    }

    /* Player Move
     *  Arguments: direction, multiplier for movement that determines wether to go
     *  right (positive) or left (negative).
     *  
     *  Moves the player left or right
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
     *  Arguments: -
     *  
     *  Tells the character to move forward in a slide or to find a new slide if
     *  they are currently not on one
     */
    void PlayerSlide()
    {
        float snapDistance = 0.5f;

        void ProgressExistingSlide()
        {
            slidingTime += Time.deltaTime;
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
            else if (smd.shouldRelease) { PlayerSlideStop(); PlayerJump(true); HaveReleasedFromSlide?.Invoke(); }
        }


        void FindNewSlide()
        {
            SlideSection.SlideSnapData slide = SlideSection.SnapToSlidingEdge(
                playerPhysics.position,
                playerPhysics.velocity,
                out Vector2 velocityUponImpact,
                new ContactFilter2D() { layerMask = layerSlide, useLayerMask = true },
                (playerCollision.size.y / 2) + snapDistance);

            storedVelocity = velocityUponImpact;
            if (!slide.wasSuccessfull) { return; }
            transform.position = new Vector3(
                slide.attachPoint.x, slide.attachPoint.y + (playerCollision.size.y / 2) + SLIDEOFFSETCONST, transform.position.z);
            storedSlide = slide.slideInstance;
            transform.SetParent(storedSlide.transform);

            HaveBegunSliding?.Invoke();
        }

        if (spamPrevention != null) { return; }

        if (storedSlide != null)
        {
            ProgressExistingSlide();
        }
        else
        {
            FindNewSlide();
        }
    }

    /* Player Halt and Grabble To
     *  Arguments: to, the transform towards which to move and eventually parent to.
     *  
     *  Starts a sequence where the player quickly snaps towards chosen transform.
     */
    public void PlayerHaltAndGrabbleTo(Transform to)
    {
        if (finished) { return; }
        finished = true;
        if (storedSlide != null) { PlayerSlideStop(); }
        HaveFinishedLevel?.Invoke();
        StartCoroutine(PlayerGrabbleTo(to));
    }

    // Player Grabble To {Coroutine used in Player Halt and Grabble To}
    IEnumerator PlayerGrabbleTo(Transform to)
    {
        Vector3 offsetToFloat = new Vector3(0f, 0.25f, 0f);
        yield return new WaitForSeconds(TIMESTOPDURATION);
        while (transform.position != to.position + offsetToFloat)
        {
            transform.position = Vector3.MoveTowards(transform.position, to.position + offsetToFloat, Time.deltaTime * playerVariables.grabbleSpeed);
            if (Vector3.Distance(transform.position, to.position) < 0.1f) { transform.position = to.position + offsetToFloat; }
            yield return null;
        }
        transform.parent = to;
        HaveMovedToGoal?.Invoke();
    }

    /* Stick to Moving Platforms
     *  Arguments: -
     *  
     *  Keeps a reference to any solid platform it stands on. If the platform moves,
     *  move the player accordingly.
     */
    void StickToMovingPlatforms()
    {
        void ModifyPositionBasedOnUnderlyingPlatform(Vector3 newPlatformPos)
        {
            Vector3 change = newPlatformPos - storedPlatformPos;
            transform.position += change;
            storedPlatformPos = newPlatformPos;
        }

        if (platformsStandingOnCount <= 0 || storedSlide != null) { storedPlatformExists = false; storedPlatformPos = Vector3.zero; return; }
        else
        {
            if (storedPlatformExists == false)
            {
                storedPlatformExists = true;
                storedPlatformName = collisionsWithFeet[0].gameObject.name;
                storedPlatformPos = collisionsWithFeet[0].transform.position;
                HaveLandedOnSnow?.Invoke(); return;
            }
            for (int a = 0; a < collisionsWithFeet.Length; a++)
            {
                if (collisionsWithFeet[a] == null) { continue; }
                if (collisionsWithFeet[a].gameObject.name == storedPlatformName)
                {
                    ModifyPositionBasedOnUnderlyingPlatform(collisionsWithFeet[a].transform.position); break;
                }
            }
        }
    }

    /* Check Collisions To Ground
     *  Arguments:
     *  colliders (outward) : A set of colliders that the player is in contact with.
     *  count (outward) : The amount of collisions.
     *  allowTriggers : A switch that determines if non-collidable triggers should be counted as well.
     *  
     *  Checks overlaps in a dedicated feet collider and returns the results.
     */
    private void CheckCollisionToGround(out Collider2D[] colliders, out int count, bool allowTriggers = true)
    {
        colliders = new Collider2D[20];
        count = feetCollider.OverlapCollider(new ContactFilter2D() { useTriggers = allowTriggers, useLayerMask = true, layerMask = layerGround }, colliders);
        if (count > 0 && allowTriggers == false) { jumps = 1; }
    }

    /* Drowned
     *  Arguments: ignoreOxygen : If true, the player fail happens immediately.
     *  
     *  If the player is colliding with water for a set time, will fail the stage.
     *  Method for handling the entire process.
     */
    public void Drowned(bool ignoreOxygen = false)
    {
        if (submerged) { submergedOxygen -= Time.deltaTime; GotMoist(); }
        else if (wetPenaltyTimer > 0f) { wetPenaltyTimer -= Time.deltaTime; }
        if (submergedOxygen > 0f && !ignoreOxygen) { return; }
        HaveDrowned?.Invoke();
        LevelLoader.PlayerHasFailed();
    }

    /* Got Moist
     *  Arguments: -
     *  
     *  Penalizes your movement until you land (and a minimum time) if you hit water.
     */
    void GotMoist()
    {
        if (finished) { return; }
        hitsToWater++;
        wetPenaltyTimer = playerVariables.wetPenaltyMinDuration;
        wetPenaltyMultiplier = playerVariables.wetPenaltyForceMultiplier;
    }

    /* Breathe
     *  Arguments: -
     *  
     *  Refreshes oxygen to full, so if you leave water for even a moment the counter resets.
     */
    public void Breathe()
    {
        submergedOxygen = playerVariables.startingOxygen;
    }

    /* Player Slide Stop
     *  Arguments: -
     *  
     *  Resets slide related variables that might interfere with the next slide instance.
     *  Levaes some intact, if used by things like animations.
     *  Returns: BOOL, stop was made if true, false if no stop was needed.
     */
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
    // Invoked by PlayerSlideStop, releases a slide after a duration.
    void ReleaseSlide()
    {
        spamPrevention = null;
    }

    /* Water Bounce
     *  Arguments: -
     *  
     *  A method to make the player skip across water if in high enough forward velocity.
     */
    public void WaterBounce()
    {
        if (Mathf.Abs(playerPhysics.velocity.x) < 6f) { return; }
        playerPhysics.velocity = new Vector2(playerPhysics.velocity.x * 0.6f, Mathf.Abs(playerPhysics.velocity.y) * 0.6f);
        GotMoist();
        HaveHitWater?.Invoke();
    }

    /* Player Jump
     *  Arguments: ignoreCheck : if True, ignores checking ground and valid jump amount.
     *  
     *  Makes the player jump.
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

    /* Player Jump Off Slide
     *  Arguments: normal On Slide : a perpendicular vector off the slide.
     *  
     *  Makes the player jump off the slide based on the normal of the slide.
     */
    void PlayerJumpOffSlide(Vector2 normalOnSlide)
    {
        playerPhysics.velocity = playerPhysics.velocity + ((normalOnSlide) * previousStoredDirection * playerVariables.jumpVelocity);
        HaveJumpedFromSlide?.Invoke();
    }
}

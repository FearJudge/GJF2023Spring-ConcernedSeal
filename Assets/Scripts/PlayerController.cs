using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const float SLIDEOFFSETCONST = 0.05f;

    Rigidbody2D playerPhysics;
    CapsuleCollider2D playerCollision;
    CircleCollider2D feetCollider;
    [SerializeField] SlideSection storedSlide;
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
        public float jumpVelocity = 4f;
    }

    public LayerMask layerGround;
    private Vector2 storedVelocity = Vector2.zero;
    private Vector2 offsetToFeetCheck;

    /* Start
     * 
     * Called before first frame.
     * Used to initialize critical variables and check settings.
     */
    void Start()
    {
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
        if (Input.GetKey(playerButtons.btnForward)) { PlayerMove(1f); }
        if (Input.GetKey(playerButtons.btnBackwards)) { PlayerMove(-1f); }
        if (Input.GetKey(playerButtons.btnCrouch)) { PlayerSlide(); }
        if (Input.GetKeyUp(playerButtons.btnCrouch) && storedSlide != null) { PlayerSlideStop(); }
        if (Input.GetKeyDown(playerButtons.btnJump)) { PlayerJump(); }
    }

    /* Player Move
     *  Arguments: direction, multiplier for movement that determines wether to go
     *  right (positive) or left (negative).
     *  
     *  Does not handle input directly.
     */
    void PlayerMove(float direction)
    {
        Collider2D[] colliders = new Collider2D[20];
        int a = feetCollider.OverlapCollider(new ContactFilter2D() { useTriggers = true }, colliders);
        if ( a <= 1 ) { return; }
        playerPhysics.AddForce(Vector2.right * direction * playerVariables.playerSpeed * Time.deltaTime);
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
            transform.position = storedSlide.MoveAlongSlide(
                new Vector2(transform.position.x, transform.position.y) - offsetToFeetCheck,
                storedVelocity,
                out bool release, out Vector2 physVel, storedVelocity.x >= 0f)
                + offsetToFeetCheck;
            storedVelocity = physVel;
            if (release) { PlayerSlideStop(); PlayerJump(); }
            return;
        }
        SlideSection.SlideData slide = SlideSection.SnapToSlidingEdge(playerPhysics.position,
            playerPhysics.velocity,
            out Vector2 velocityUponImpact,
            new ContactFilter2D() { layerMask = layerGround, useLayerMask = true},
            (playerCollision.size.y / 2) + snapDistance);
        storedVelocity = velocityUponImpact;
        if (!slide.wasSuccessfull) { return; }
        transform.position = new Vector3(slide.attachPoint.x, slide.attachPoint.y + (playerCollision.size.y/2) + SLIDEOFFSETCONST, transform.position.z);
        storedSlide = slide.slideInstance;
    }

    void PlayerSlideStop()
    {
        spamPrevention = storedSlide;
        Invoke("ReleaseSlide", 1f);
        playerPhysics.isKinematic = false;
        playerPhysics.velocity = storedVelocity;
        storedVelocity = Vector2.zero;
        storedSlide = null;
    }

    void ReleaseSlide()
    {
        spamPrevention = null;
    }

    /* Player Jump
     *  
     *  Does not handle input directly.
     */
    void PlayerJump()
    {
        Collider2D[] colliders = new Collider2D[20];
        int a = feetCollider.OverlapCollider(new ContactFilter2D() { useTriggers = true }, colliders);
        if (a <= 1 || playerPhysics.velocity.y >= 2f) { return; }
        playerPhysics.velocity = new Vector2(playerPhysics.velocity.x, playerPhysics.velocity.y + playerVariables.jumpVelocity);
    }
}

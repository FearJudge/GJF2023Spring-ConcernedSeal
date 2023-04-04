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
    private float storedVelocity = 0f;

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
        if (Input.GetKeyUp(playerButtons.btnCrouch)) { PlayerSlideStop(); }
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
        float snapDistance = 0.5f;

        if (storedSlide != null)
        {
            playerPhysics.isKinematic = true;
            playerPhysics.velocity = Vector2.zero;
            transform.position = storedSlide.MoveAlongSlide(
                new Vector2(transform.position.x, transform.position.y) - new Vector2(0f, (playerCollision.size.y / 2)),
                storedVelocity,
                true)
                + new Vector2(0f, (playerCollision.size.y / 2));
            return;
        }
        SlideSection.SlideData slide = SlideSection.SnapToSlidingEdge(playerPhysics.position,
            new ContactFilter2D() { layerMask = layerGround, useLayerMask = true},
            (playerCollision.size.y / 2) + snapDistance);
        storedVelocity = playerPhysics.velocity.magnitude;
        if (!slide.wasSuccessfull) { return; }
        transform.position = new Vector3(slide.attachPoint.x, slide.attachPoint.y + (playerCollision.size.y/2) + SLIDEOFFSETCONST, transform.position.z);
        storedSlide = slide.slideInstance;
    }

    void PlayerSlideStop()
    {
        storedVelocity = 0f;
        playerPhysics.isKinematic = false;
        storedSlide = null;
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

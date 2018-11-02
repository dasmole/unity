using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour
{

    public float gravity = -.5f;
    public float skinWidth = .03f;
    public float speed = 3f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    public float jumpVelocity = .2f;
    public float maxClimbAngle = 50f;
    public float xDrag = 10f;

    bool canJump = false;
    float horizontalRaySpacing;
    float horizontalSpeed;
    float verticalRaySpacing;


    BoxCollider2D myCollider;

    Vector2 velocity;

    RaycastOrigins raycastOrigins;

    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    void Start()
    {
        myCollider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
        velocity = Vector2.zero;
    }

    public void movement(float inputX, bool jumping)
    {
        MoveSprite();
        UpdateRaycastOrigins();
        UpdateVelocity(inputX, jumping);
        checkVerticalCollisions();
        checkHorizontalCollisions();
    }

    void checkVerticalCollisions() {
        Vector2 rayDirectionVert, rayOriginVert;

        float rayDistanceVert;
        RaycastHit2D hit;

        //check vertical collisions
        rayDirectionVert = (velocity.y <= 0 ? Vector2.down :  Vector2.up);
        rayOriginVert = (velocity.y <= 0 ? raycastOrigins.bottomLeft : raycastOrigins.topLeft);
        rayDistanceVert = (Mathf.Abs(velocity.y) + skinWidth) * rayDirectionVert.y;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Debug.DrawRay(rayOriginVert + Vector2.right * verticalRaySpacing * i, rayDirectionVert * Mathf.Abs(rayDistanceVert), Color.red);
            hit = Physics2D.Raycast(rayOriginVert + Vector2.right * verticalRaySpacing * i, rayDirectionVert, Mathf.Abs(rayDistanceVert));
            if (hit)
            {

                velocity.y = (hit.distance - skinWidth) * rayDirectionVert.y;
                rayDistanceVert = hit.distance;

                //set canJump if we're moving down and there are vertical hits
                if (rayDirectionVert == Vector2.down) {
                    canJump = true;
                }
            }
        }
    }

    void checkHorizontalCollisions()
    {
        Vector2 rayDirectionHoriz, rayOriginHoriz;
        float rayDistanceHoriz;
        RaycastHit2D hit;

        //check vertical collisions 
        rayDirectionHoriz = (velocity.x <= 0 ? Vector2.left : Vector2.right);
        rayOriginHoriz = (velocity.x <= 0 ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight);
        //rayOriginHoriz.x = rayOriginHoriz.x + (rayDirectionHoriz.x * skinWidth * .6f);

        rayDistanceHoriz = (Mathf.Abs(velocity.x) + skinWidth) * rayDirectionHoriz.x;

        for (int i = 0; i < horizontalRayCount; i++)
            {
            Debug.DrawRay(rayOriginHoriz + Vector2.up * horizontalRaySpacing * i, rayDirectionHoriz * Mathf.Abs(rayDistanceHoriz), Color.red);
            hit = Physics2D.Raycast(rayOriginHoriz + Vector2.up * horizontalRaySpacing * i, rayDirectionHoriz, Mathf.Abs(rayDistanceHoriz)) ;

            if (hit)
            {
                if ((i == 0) && (Vector2.Angle(hit.normal, Vector2.up) < maxClimbAngle) && canJump)
                {
                    //adjust for slope
                    slopeAdjustment(hit.normal);
                } else {
                    velocity.x = (hit.distance - skinWidth) * rayDirectionHoriz.x;
                }
            }
        }
    }



    void slopeAdjustment(Vector2 slopeNormal) {
        float slopeAngle = Vector2.Angle(slopeNormal, Vector2.up);
        velocity.y = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * velocity.x * Mathf.Sign(velocity.x);
        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * velocity.x;

    }


    void UpdateVelocity(float inputX, bool jumping) {
        velocity.y += gravity * Time.deltaTime;
        if (jumping) {
            Debug.Log("in jumping, canjump is " + canJump);
        }
        if (jumping && canJump) {
            Debug.Log("Trying to jump. Velocity is " + velocity.ToString("F4"));
            velocity.y += jumpVelocity;
            canJump = false;
            Debug.Log("After jump. Velocity is " + velocity.ToString("F4"));

        }

        //Below is code for drag after left/right input are stopped
        Debug.Log("InputX is " + inputX);
        Debug.Log("Before drag, velocity is " + velocity.x);

        if ((inputX < -.3f) || (inputX > .3f))
        {
            horizontalSpeed = speed;
            velocity.x = inputX * horizontalSpeed * Time.deltaTime;
            Debug.Log("No drag, velocity is " + velocity.x);
        } else {
            // if we've gotten pretty slow, just stop. Otherwise, reduce with drag
            if (horizontalSpeed < .5f) {
                velocity.x = 0.0f;
            } else {
                horizontalSpeed -= (Time.deltaTime * xDrag);
                velocity.x = horizontalSpeed * Time.deltaTime * Mathf.Sign(velocity.x);
                Debug.Log("Just dragged, new velocity is " + velocity.x.ToString("F5"));
            }
        }
    }

    void MoveSprite() {
        //Debug.Log("MOVING velocity is " + velocity.ToString("F4"));
        transform.Translate(velocity);
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = myCollider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = myCollider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
}

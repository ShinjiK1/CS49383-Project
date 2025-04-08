using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellcastController : MonoBehaviour
{
    public GameObject spellCanvas;

    [SerializeField] LineRenderer lr;

    [SerializeField]
    [Tooltip("The reference to the action of entering spellcast mode, finalizing spell drawing and casting the spell itself")]
    public InputActionReference m_SpellCast;

    [SerializeField]
    [Tooltip("The reference to the action of toggling on/off the marker when in draw mode")]
    public InputActionReference m_SpellDraw;

    [SerializeField]
    [Tooltip("The reference to the action of casating the spell itself once it is ready to be fired")]
    public InputActionReference m_SpellFire;

    // startState = 0
    // drawingState = 1
    // firingState = 2
    private int spellCastState;
    private int fireSpellID; // ID for the spell the user can fire (or that they last fired if they can't currently fire a spell).

    Ray ray;
    RaycastHit hit;
    public LayerMask target;

    [SerializeField] Transform rayOrigin;
    [SerializeField] public int markerSize = 10;
    public Material drawMat;
    private Color[] colors;
    private CanvasController drawCanvas;
    private Vector2 hitPos;
    private Vector2 lastHitPos;
    private bool hitLastFrame;
    private bool isDrawing;
    public GameObject spellIndicator; // Indicator for when the user can cast a spell

    // Move/change these last 3 fields once we start to have more than 1 spell type.
    public GameObject projectileObj;
    public Transform spawn;
    public float force = 10f;

    private void Awake()
    {
        m_SpellCast.action.Enable();
        m_SpellDraw.action.Enable();

        m_SpellCast.action.performed += ManageSpellCast;
        m_SpellDraw.action.performed += DrawOnCanvas;
        m_SpellDraw.action.canceled += StopDrawOnCanvas;

        m_SpellFire.action.performed += Shoot;
    }

    private void Start()
    {
        drawCanvas = spellCanvas.GetComponent<CanvasController>();
        colors = Enumerable.Repeat(drawMat.color, markerSize * markerSize).ToArray();
    }

    private void OnDestroy()
    {
        m_SpellCast.action.Disable();
        m_SpellDraw.action.Disable();
    }

    private void ManageSpellCast(InputAction.CallbackContext context)
    {
        if (spellCastState == 0)
        {
            Debug.Log("Starting spell draw");
            spellCanvas.SetActive(true);
            spellCastState = 1;
        }
        else if (spellCastState == 1)
        {
            Debug.Log("Finished drawing spell");
            drawCanvas.texture.SetPixels32(drawCanvas.resetColorArray); // Clear canvas
            drawCanvas.texture.Apply();
            isDrawing = false;
            spellCanvas.SetActive(false);

            // Pass drawing input into symbol recognizer script and get output, which will be the symbol
            // the script matched the drawing to and an accuracy score, or it will be a ? (didn't match
            // to anything).
            fireSpellID = 0;
            int accuracyScore = 1;

            if (accuracyScore > 0.6) // Whatever threshhold for a successful drawing
            {
                spellCastState = 2;
                spellIndicator.SetActive(true);
                // Load the spell that the user can cast
            }
            else // Drawing wasn't accurate enough/matched to a spell symbol
            {
                spellCastState = 0;
            }
        }
    }
    private void DrawOnCanvas(InputAction.CallbackContext context)
    {
        if (spellCastState == 1)
        {
            Debug.Log("Drawing on canvas");
            isDrawing = true;
        }
    }

    private void StopDrawOnCanvas(InputAction.CallbackContext context)
    {
        if (spellCastState == 1)
        {
            Debug.Log("Stopped drawing on canvas");
            isDrawing = false;
        }
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        if (spellCastState == 2)
        {
            Debug.Log("Firing spell");
            // Fire spell. Also probably want a timer so they can't fire a bunch of times at once (if the spell has multiple charges)

            /*
            Change this part once we start to have more than one spell type that the player can cast.
            Instead of storing a projectile and rigidbody field in this class, probably just have a list
            of spellIDs that correspond to casting each spell and pass the id of the spell we are currently
            casting in this function or by setting fireSpellID to that value. Then we would want to
            just instantiate a prefab for the spell we are casting and run the logic for that spell inside of
            its own script so that we don't deal with all that code here. Alternatively we could also just make
            a separate script file for managing spell casts and do all that code over there to not bloat this file.
            */
            GameObject projectile = Instantiate(projectileObj, spawn.position, spawn.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = spawn.forward * force;
            }

            // If can't fire spell anymore
            spellCastState = 0;
            spellIndicator.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spellCastState == 1)
        {
            lr.enabled = true;
            lr.startWidth = 0.01f;
            lr.endWidth = 0.01f;
            lr.SetPosition(0, rayOrigin.position);
            ray = new Ray(rayOrigin.position, rayOrigin.forward * 5);
            float rayLength = rayOrigin.forward.magnitude * 5;
            if (Physics.Raycast(ray, out hit, rayLength, target))
            {
                //Debug.Log("Hit Canvas");
                lr.SetPosition(1, hit.point);

                if (isDrawing)
                {
                    /*
                    if (drawCanvas == null)
                    {
                        drawCanvas = hit.transform.GetComponent<CanvasController>();
                    }
                    */

                    hitPos = new Vector2(hit.textureCoord.x, hit.textureCoord.y); // textureCoords are from 0-1 (like a percentage)

                    var x = (int)(hitPos.x * drawCanvas.textureSize.x - (markerSize / 2));
                    var y = (int)(hitPos.y * drawCanvas.textureSize.y - (markerSize / 2));

                    if (x < 0 || x > drawCanvas.textureSize.x || y < 0 || y > drawCanvas.textureSize.y) //If would be drawing off the canvas
                    {
                        //drawCanvas = null;
                        hitLastFrame = false;
                        return;
                    }

                    if (hitLastFrame)
                    {
                        drawCanvas.texture.SetPixels(x, y, markerSize, markerSize, colors);

                        float dist = Vector2.Distance(lastHitPos, hitPos);

                        int lerpX;
                        int lerpY;

                        // If distance between last draw positions is large enough, we interpolate by 2.5% each step
                        // Else just draw between every 0.01 units of distance
                        if (dist > 0.25f)
                        {
                            for (float f = 0f; f < 1f; f += 0.001f)
                            {
                                lerpX = (int)Mathf.Lerp(lastHitPos.x, x, f);
                                lerpY = (int)Mathf.Lerp(lastHitPos.y, y, f);
                                drawCanvas.texture.SetPixels(lerpX, lerpY, markerSize, markerSize, colors);
                            }
                        }
                        else
                        {
                            for (float f = 0f; f < dist; f += 0.005f)
                            {
                                lerpX = (int)Mathf.Lerp(lastHitPos.x, x, f);
                                lerpY = (int)Mathf.Lerp(lastHitPos.y, y, f);
                                drawCanvas.texture.SetPixels(lerpX, lerpY, markerSize, markerSize, colors);
                            }
                        }

                        drawCanvas.texture.Apply();
                    }
                    //Debug.Log("HERE!!!!");

                    lastHitPos = new Vector2(x, y);
                    hitLastFrame = true;
                    return; 
                }
                else
                {
                    hitLastFrame = false;
                }
            }
            else
            {
                //drawCanvas = null;
                hitLastFrame = false;
                lr.SetPosition(1, rayOrigin.position + rayOrigin.forward * 3);
            }
        }
        else
        {
            lr.enabled = false;
        }
    }
}

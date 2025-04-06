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

    // The same button is used for SpellCastStart, SpellCastFinish and SpellCastFire and only one of them is enabled at any time.
    // SpellCastStart is enabled at the start and when the user is neither in state of drawing a spell, or being able to fire one.
    // Pressing it opens the canvas and allows the user to start drawing their spell.
    // SpellCastFinish is used enabled when the user has pressed SpellCastStart and is in the state where they can draw a spell.
    // Pressing it finalizes the drawing the user made and compares it to a list of symbols to determine if they can cast a
    // spell/what spell they can cast. If successful, the user loads the spell and enters a state that they can fire one.
    // If the drawing is unsuccessful and no spell symbol is recognized, the user returns to the starting state and SpellCastStart is enabled again.
    // SpellCastFire is enabled when the user can fire a spell and pressing it casts the spell. After casting the spell, or
    // casting it a certain number of time, the user goes back to the starting state and SpellCastStart is enabled again.

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

    private void Awake()
    {
        m_SpellCast.action.Enable();
        m_SpellDraw.action.Enable();

        m_SpellCast.action.performed += ManageSpellCast;
        m_SpellDraw.action.performed += DrawOnCanvas;
    }

    private void Start()
    {
        drawCanvas = spellCanvas.GetComponent<CanvasController>();
        colors = Enumerable.Repeat(drawMat.color, markerSize * markerSize).ToArray();
        Debug.Log(string.Join(", ", colors));
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
            spellCanvas.SetActive(false);

            // Pass drawing input into symbol recognizer script and get output, which will be the symbol
            // the script matched the drawing to and an accuracy score, or it will be a ? (didn't match
            // to anything).
            fireSpellID = 0;
            int accuracyScore = 1;

            if (accuracyScore > 0.6) // Whatever threshhold for a successful drawing
            {
                spellCastState = 2;
                // Load the spell that the user can cast
            }
            else // Drawing wasn't accurate enough/matched to a spell symbol
            {
                spellCastState = 0;
            }
        }
        else if (spellCastState == 2)
        {
            Debug.Log("Firing spell");
            // Fire spell. Also probably want a timer so they can't fire a bunch of times at once (if the spell has multiple charges)

            // If can't fire spell anymore
            spellCastState = 0;
        }
        /*
        Debug.Log("Starting spell draw");
        m_SpellCastStart.action.Disable();
        m_SpellCastStart.action.performed -= StartSpellCast;
        m_SpellCastFinish.action.Enable();
        m_SpellDraw.action.Enable();

        spellCanvas.SetActive(true);
        m_SpellDraw.action.performed += DrawOnCanvas;
        m_SpellCastFinish.action.performed += CheckSpellCorrect;
        */
    }
    private void DrawOnCanvas(InputAction.CallbackContext context)
    {
        if (spellCastState == 1)
        {
            Debug.Log("Drawing on canvas");
        }
    }

    /*
    private void StartSpellCast(InputAction.CallbackContext context)
    {
        Debug.Log("Starting spell draw");
        m_SpellCastStart.action.Disable();
        m_SpellCastStart.action.performed -= StartSpellCast;
        m_SpellCastFinish.action.Enable();
        m_SpellDraw.action.Enable();

        spellCanvas.SetActive(true);
        m_SpellDraw.action.performed += DrawOnCanvas;
        m_SpellCastFinish.action.performed += CheckSpellCorrect;
    }

    private void CheckSpellCorrect(InputAction.CallbackContext context)
    {
        Debug.Log("Finished drawing spell");
        m_SpellCastFinish.action.Disable();
        m_SpellDraw.action.Disable();
        m_SpellCastFinish.action.performed -= CheckSpellCorrect;
        m_SpellDraw.action.performed -= DrawOnCanvas;

        spellCanvas.SetActive(false);
        // Convert canvas drawing into some input form that we can send to compare to a symbol
        // recognizer with our list of spell symbols.


        // Pass input into symbol recognizer script and get output, which will be the symbol
        // the script matched the drawing to and an accuracy score, or it will be a ? (didn't match
        // to anything).
        fireSpellID = 0;
        int accuracyScore = 1;

        if (accuracyScore > 0.6) // Whatever threshhold
        {
            m_SpellCastFire.action.Enable();
            // Load the spell that the user can cast
            m_SpellCastFire.action.performed += FireSpell;
        }
        else
        {
            m_SpellCastStart.action.Enable();
            m_SpellCastStart.action.performed += StartSpellCast;
        }
    }

    private void FireSpell(InputAction.CallbackContext context)
    {
        Debug.Log("Firing spell");
        // Fire spell. Also probably want a timer so they can't fire a bunch of times at once (if the spell has multiple charges)

        // If can't fire spell anymore
        m_SpellCastFire.action.Disable();
        m_SpellCastFire.action.performed -= FireSpell;
        m_SpellCastStart.action.Enable();
        m_SpellCastStart.action.performed += StartSpellCast;
    }
    */

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

                if (hit.transform.CompareTag("Draw Canvas")) // Prob won't need this as raycast has a layer mask set
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

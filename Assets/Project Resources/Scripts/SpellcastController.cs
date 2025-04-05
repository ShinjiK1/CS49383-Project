using UnityEngine;
using UnityEngine.InputSystem;

public class SpellcastController : MonoBehaviour
{
    public GameObject spellCanvas;

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

    private void Awake()
    {
        m_SpellCast.action.Enable();
        m_SpellDraw.action.Enable();

        m_SpellCast.action.performed += ManageSpellCast;
        m_SpellDraw.action.performed += DrawOnCanvas;
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
        
    }
}

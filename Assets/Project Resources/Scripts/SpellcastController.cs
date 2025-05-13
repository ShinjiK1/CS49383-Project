using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using PDollarGestureRecognizer;
using QDollarGestureRecognizer;

public class SpellcastController : MonoBehaviour
{
    public GameObject spellCanvas;
    public Transform mainCamera;

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
    public GameObject spellDisplay;
    public GameObject spellDisplayTMP;
    private TMP_Text spellDisplayText;

    // Move/change these last 3 fields once we start to have more than 1 spell type.
    public ProjectileSpawner projectileSpawner;
    //public GameObject projectileObj;
    public Transform spawn;
    public float force = 10f;

    // Stuff for $Q recognizer integration with our drawings
    private int strokeIndex = 0;
    private List<Gesture> trainingSet = new List<Gesture>();
    private List<Point> points = new List<Point>();

    public bool useQRecognizer;
    public bool writeGesture;
    public string writeGestureName;

    private string[] spellNames;
    private string gestureName;

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
        spellNames = new string[] { "Arrow", "Shield", "MagicCircle", "Triangle", "Thunderbolt" };

        drawCanvas = spellCanvas.GetComponent<CanvasController>();
        colors = Enumerable.Repeat(drawMat.color, markerSize * markerSize).ToArray();

        spellDisplayText = spellDisplayTMP.GetComponent<TMP_Text>();

        //Load pre-made gestures
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GameGestures/");
        foreach (TextAsset gestureXml in gesturesXml)
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

        //Load user custom gestures
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (string filePath in filePaths)
            trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
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
            spellCanvas.transform.position = mainCamera.position + new Vector3(mainCamera.forward.x, 0, mainCamera.forward.z).normalized * 1.5f;
            Vector3 cameraRot = mainCamera.localEulerAngles;
            Debug.Log(cameraRot.x + "," + cameraRot.y + "," + cameraRot.z);
            spellCanvas.transform.rotation = Quaternion.Euler(0, cameraRot.y + 90, cameraRot.z - 90);
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
            if (points.Count > 1)
            {
                if (writeGesture)
                {
                    string fileName = String.Format("{0}/{1}-{2}.xml", Application.persistentDataPath, writeGestureName, DateTime.Now.ToFileTime());
                    GestureIO.WriteGesture(points.ToArray(), writeGestureName, fileName);
                }
                string toPrint = "";
                for (int i = 0; i < points.Count; i++)
                {
                    toPrint += "(" + points[i].X + "," + points[i].Y + ")\n";
                }
                Debug.Log(toPrint);

                Gesture candidate = new Gesture(points.ToArray());

                Result gestureResult;
                if (useQRecognizer)
                {
                    gestureResult = QPointCloudRecognizer.Classify(candidate, trainingSet.ToArray());
                }
                else
                {
                    gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());
                }
                gestureName = gestureResult.GestureClass;
                spellDisplayText.text = gestureName;
                //string message = gestureResult.GestureClass + " " + gestureResult.Score;
                Debug.Log(gestureName);
            }
            // fireSpellID = -1 if gesture drawn isn't a valid spell, else represents whatever spell the user drew and can now cast.
            if (gestureName != "")
            {
                fireSpellID = Array.IndexOf(spellNames, gestureName);
            }
            else
            {
                fireSpellID = -1;
            }

            //int accuracyScore = 1; // Don't know how to calculate a good accuracy score for this, so just succeeding if a valid spell is recognized.

            strokeIndex = 0;
            points.Clear();

            gestureName = "";
            if (fireSpellID > -1)
            {
                spellCastState = 2;
                spellIndicator.SetActive(true);
                spellDisplay.SetActive(true);
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

            strokeIndex++;
        }
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        if (spellCastState == 2)
        {
            Debug.Log("Firing spell, " + fireSpellID);
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
            projectileSpawner.SpawnProjectile(spawn.position, spawn.rotation, fireSpellID);

            // If can't fire spell anymore
            spellCastState = 0;
            fireSpellID = -1;
            spellIndicator.SetActive(false);
            spellDisplay.SetActive(false);
            spellDisplayText.text = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spellCastState == 1)
        {
            Vector3 forwardVectorTowardsCamera = (mainCamera.position - spellCanvas.transform.position);
            Vector3 normalizedForwardVector = forwardVectorTowardsCamera.normalized;
            Debug.Log("vector to camera " + forwardVectorTowardsCamera);
            float dotProductResult = Vector3.Dot(mainCamera.forward, normalizedForwardVector);
            Debug.Log("dot product " + dotProductResult);
            // Greater than ~105 degree angle between main camera and the spell canvas (and some distance between them), so we teleport the canvas in front of the camera
            if (dotProductResult > 0.25f && forwardVectorTowardsCamera.magnitude > 1.5f)
            {
                spellCanvas.transform.position = mainCamera.position + new Vector3(mainCamera.forward.x, 0, mainCamera.forward.z).normalized * 1.5f;
                Vector3 cameraRot = mainCamera.localEulerAngles;
                spellCanvas.transform.rotation = Quaternion.Euler(0, cameraRot.y + 90, cameraRot.z - 90);
            }
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
                        int pointX = (int)(hitPos.x * drawCanvas.textureSize.x);
                        int pointY = (int)((1-hitPos.y) * drawCanvas.textureSize.y); // y starts at the top in $Q recognizer (lower y values are closer to the top)
                        /*
                        // We need to rotate our shape by 90 degrees because the canvas is rotated by 90 degrees in the y axis?
                        pointX = pointY;
                        pointY = -1 * pointX;
                        */
                        points.Add(new Point(pointX, pointY, strokeIndex));

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

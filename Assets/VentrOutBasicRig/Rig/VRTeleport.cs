using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum Handness
{
    Left,
    Right
}

public class VRTeleport : MonoBehaviour
{
    [SerializeField] private Handness hand; // enum referencing / enums visible a created outside of vrhand
    [SerializeField] private Transform XRRig; // referencing xrrig because that is thing I will move when teleport
    [SerializeField] public Transform teleportReticle; // something that will  appear on the ground where I want to teleport
    [SerializeField] public MeshRenderer blackScreen;// blackscreen a plane in front of player
    [SerializeField] private LineRenderer line; //visualize our raycast
    [SerializeField] private int lineResolution = 20; // the higher the resolution the better the curvature would look but don't make it to high
    [SerializeField] private float lineCurvate = 1f; // line curvature

    private string buttonName; //choose the button we want to execute teleport
    private bool canTeleport; // keep track of whether we can teleport. eg on floor: yes in air: no
    private bool teleportLocked; //when we choose teleport location we will temporarily lock our ability to teleport so we don't telepert to random location when we can't see anything
    private Vector3 hitPoint; //track of where we are pointing

    public AudioClip teleportSound;// uccomment lines section referring to teleport if you would like a sound to play upon teleporting

    void Start()
    {
        //buttonName = "XRI_" + hand + "_SecondaryButton";
        buttonName = "Cancel";
        line.positionCount = lineResolution;// to add another position to allow for curve
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton(buttonName) && teleportLocked == false) // GetButton presumes button is down AND teleportLocked is false
        {
            line.enabled = true; // line is visible if button is pressed
            line.SetPosition(0, transform.position);//sets starting position to our hand

            RaycastHit hit; // figure out where we are pointing
            if (Physics.Raycast(transform.position, transform.forward, out hit))// lets do a raycast that begins with our  hands and go forward direction and we output info to the variable hit, can add comma and then  distance as float or reference variable range above
            {
                canTeleport = true; //only do this if the if statement is successful
                hitPoint = hit.point; //refers to where the raycast has pointed. end point of our raycast if successful

                SetLinePositions(transform.position, hitPoint); //teleport to new position if only do this if the if statement is successful. transform.position is starting point. hitpoint as end point.

                teleportReticle.gameObject.SetActive(true);//if successfully pointing somewhere we want out teleportReticle to be true.
                teleportReticle.position = hitPoint;
            }
            else
            {
                line.SetPosition(1, transform.position + transform.forward); //set second postion to the hands position plus one unit in forward direction
                canTeleport = false; // why do we need the previous line? what happens if you don't have the previous line?
                teleportReticle.gameObject.SetActive(false);//if cannot teleport turn off reticle
            }
        }
        else if (Input.GetButtonUp(buttonName) && teleportLocked == false)
        {
            line.enabled = false;//disable line when button is let go
            teleportReticle.gameObject.SetActive(false); //turn off reticle if we release button. this is the waiting function
            if (canTeleport == true)
            {
                StartCoroutine(FadedTeleport()); //call faded teleport function. instead of above line we'll start this faded function of teleport. needed when using with IEnumerator

                //audioSource.PlayOneShot(teleportSound, 0.5f);//un comment if you want sound to play when teleporting
                //m_swoosh.Play();
            }

        }
    }

    void SetLinePositions(Vector3 start, Vector3 end) // only void as it doesn't need to return anything; where raycast will start and end
    {
        Vector3 startToEnd = end - start;
        Vector3 midPoint = start + startToEnd / 2 + Vector3.up * lineCurvate;// vector3.Up to shift upwards; curve variable (lineCurvate) determines how high up

        for (int i = 0; i < lineResolution; i++) // loop for three things; 0 starting point and increase by one if less than 20 
        {
            float t = i / (float)lineResolution; // determines where we in our  numbers for our line divided by line resolution; makes integer for this one line into float
            Vector3 startToMid = Vector3.Lerp(start, midPoint, t); // find a point in between start and midpoint with percentage of t value representation the position 
            Vector3 midToEnd = Vector3.Lerp(midPoint, end, t);
            Vector3 curvePoint = Vector3.Lerp(startToMid, midToEnd, t); // creates the points? 


            line.SetPosition(i, curvePoint); //creates the bend of the line
        }
    }

    IEnumerator FadedTeleport()

    {
        teleportLocked = true; // lock the teleport so we cannot teleport anywhere else

        float currentTime = 0; //creates timer counting from 0 to 1

        while (currentTime < 1)// loop will keeping going until it reaches 1
        {
            currentTime += Time.deltaTime; // incrreasing time every frame by a little bit by change in time between each frame.  Time.deltaTime is suppose to be 1 frame/second

            yield return new WaitForEndOfFrame(); //waitForEndOfFrame function so it doesn't happen instantaneously by frame

            blackScreen.material.color = Color.Lerp(Color.clear, Color.black, currentTime); // makes our blackscreen go from clear to black. think of lerping as a % between one value and another (eg. clear and black). currenttime ties it to the timer

        }
        XRRig.position = hitPoint; // once entirely black we move rig to position we pointed at

        yield return new WaitForSeconds(0.5f); // we wait .5 seconds

        while (currentTime > 0) //  current time variable is now 1
        {
            currentTime -= Time.deltaTime; // start decreasing the current time

            yield return new WaitForEndOfFrame(); //decrease per frame

            blackScreen.material.color = Color.Lerp(Color.clear, Color.black, currentTime); // moving blackscreen from black to clear 
        }

        teleportLocked = false; // allows to teleport again to somewhere else

    }
}

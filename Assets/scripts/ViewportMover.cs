using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewportMover : MonoBehaviour
{
    float mouseX, mouseY; //will hold mouse position for rotating viewport
    public float sceneXRot, sceneYRot; //holds camera rotation around the scene
    float oldXRot, oldYRot; //stores the previous rotation values
    public GameObject cameraTarget;
    public float zoomlevel;
    float zoomscale = 0.1f;
    

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Fire2"))//right mouse button was just clicked
        {
            //record initial mouse position when click starts
            mouseX = Input.mousePosition.x;
            mouseY = Input.mousePosition.y;
            oldXRot = sceneXRot;
            oldYRot = sceneYRot;
        }
        if (Input.GetButton("Fire2"))//right mouse button was just released
        { 
        sceneXRot = Input.mousePosition.x - mouseX + oldXRot;
        sceneYRot = Mathf.Clamp( Input.mousePosition.y - mouseY + oldYRot,-300,50);
        }

        cameraTarget.transform.rotation = Quaternion.EulerAngles(-sceneYRot/360, sceneXRot/360, 0);

        zoomlevel -= Input.mouseScrollDelta.y * zoomscale;
        zoomlevel = Mathf.Clamp(zoomlevel, 1, 10);

        Camera.main.transform.localPosition = new Vector3(0, 1, -zoomlevel);
    }
}

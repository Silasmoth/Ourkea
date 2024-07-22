using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using UnityEngine;

public class ViewportMover : MonoBehaviour
{
    public GameObject elevationLine;
    public bool Orthographic = false;
    public int viewtype = 0;
    //0 - perspective
    //1 plan
    //2 elevation

    public Transform Planview;
    public Transform Elevationview;
    public Camera maincam, UIcam;

    float mouseX, mouseY; //will hold mouse position for rotating viewport
    public float sceneXRot, sceneYRot; //holds camera rotation around the scene
    float oldXRot, oldYRot; //stores the previous rotation values
    public GameObject cameraTarget;
    public float zoomlevel;
    float zoomscale = 0.1f;
    public float clampedXRot;
    public float mouseMulti = 1000;
    
    // Update is called once per frame
    void Update()
    {
        if (!Orthographic)
        {

            if (Input.GetButtonDown("Fire2"))//right mouse button was just clicked
            {
                //record initial mouse position when click starts
                mouseX = Input.mousePosition.x;
                mouseY = Input.mousePosition.y;
                oldXRot = sceneXRot;
                oldYRot = sceneYRot;
            }
            if (Input.GetButton("Fire2"))//right mouse button is down
            {
                sceneXRot = ((Input.mousePosition.x - mouseX)/maincam.pixelHeight)*mouseMulti + oldXRot;
                sceneYRot = ((Input.mousePosition.y - mouseY)/maincam.pixelHeight)*mouseMulti + oldYRot;
            }
            sceneYRot = Mathf.Clamp(sceneYRot, -300, 50);
            cameraTarget.transform.rotation = Quaternion.Euler(-sceneYRot / Mathf.PI, sceneXRot / Mathf.PI, 0);

            zoomlevel -= Input.mouseScrollDelta.y * zoomscale;
            zoomlevel = Mathf.Clamp(zoomlevel, 1, 10);

            maincam.transform.localPosition = new Vector3(0, 1, -zoomlevel);

            clampedXRot = (sceneXRot / Mathf.PI) % 360;
            if (clampedXRot < 0)
            {
                clampedXRot += 360;
            }
        }
        else {

            zoomlevel -= Input.mouseScrollDelta.y * zoomscale * 0.5f;
            zoomlevel = Mathf.Clamp(zoomlevel, 0.5f, 10);
           
            float actualzoom = zoomlevel*zoomlevel;
            maincam.orthographicSize = actualzoom;
            UIcam.orthographicSize = actualzoom;
            if (Input.GetButtonDown("Fire2"))//right mouse button was just clicked
            {
                //record initial mouse position when click starts
                

                mouseX = Input.mousePosition.x;
                mouseY = Input.mousePosition.y;
                oldXRot = sceneXRot;
                oldYRot = sceneYRot;
            }
            if (Input.GetButton("Fire2"))//right mouse button is down
            {
                sceneXRot = ((Input.mousePosition.x - mouseX) * actualzoom / maincam.pixelHeight)*mouseMulti*2 + oldXRot;
                sceneYRot = ((Input.mousePosition.y - mouseY) * actualzoom / maincam.pixelHeight)*mouseMulti*2 + oldYRot;
            }

            switch (viewtype)
            {

                case 0:
                    //default perspective view
                   
                    break;
                case 1:
                    //plan view
                    maincam.transform.position = Planview.position + new Vector3(-sceneYRot/1000,0, sceneXRot / 1000);
                    
                    break;

                case 2:
                    //elevation view
                    maincam.transform.position = Elevationview.position + new Vector3(0, -sceneYRot/1000, sceneXRot / 1000);
                    
                    break;

            }
        }
        
    }

    public void Setview(int view)
    {
        viewtype = view;
        switch (viewtype)
        {

            case 0://default perspective view
                elevationLine.SetActive(false);
                zoomlevel = 3;
                sceneXRot = 317;
                sceneYRot = 0;
                Orthographic = false;
                maincam.orthographic = false;
                UIcam.orthographic = false;
                maincam.transform.localRotation = Quaternion.identity;
                break;
            case 1://plan view
                elevationLine.SetActive(false);
                zoomlevel = 1.5f;
                sceneXRot = 0; 
                sceneYRot = 0;
                maincam.transform.position = Planview.position;
                maincam.transform.rotation = Planview.rotation;
                maincam.orthographic = true;
                UIcam.orthographic = true;
                Orthographic = true;
                break;

            case 2://elevation view
                elevationLine.SetActive(true);
                zoomlevel = 1.5f;
                sceneXRot = 0;
                sceneYRot = 0;
                maincam.transform.position = Elevationview.position;
                maincam.transform.rotation = Elevationview.rotation;
                maincam.orthographic = true;
                UIcam.orthographic = true;
                Orthographic = true;
                break;

        }
    }
}

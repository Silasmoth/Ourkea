using System.Collections;
using System.Collections.Generic;
#if UNITY_STANDALONE_WIN

  using System.Windows.Forms;

#endif

using UnityEngine;
using UnityEngine.EventSystems;

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

    //for touch control zooming
    float oldzoom;
    float startdistance;
    float distance;
    bool orbitstart = false;

    // Update is called once per frame
    void Update()
    {
        if (!Orthographic)
        {
#if UNITY_STANDALONE_WIN

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

#else



            
               



            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                bool overUI = false;
                foreach (Touch touch in Input.touches)
                {
                    int id = touch.fingerId;
                    if (EventSystem.current.IsPointerOverGameObject(id))
                    {
                        overUI = true;
                    }
                }

                if (!overUI)
                {
                    orbitstart = true;
                    //record initial mouse position when click starts
                    mouseX = Input.mousePosition.x;
                    mouseY = Input.mousePosition.y;
                    oldXRot = sceneXRot;
                    oldYRot = sceneYRot;
                }
            }

            






            if (Input.GetButtonDown("Fire2"))//right mouse button was just clicked
            {
                orbitstart = false;
                //record initial mouse position when click starts
                //mouseX = Input.mousePosition.x;
                //mouseY = Input.mousePosition.y;
                //oldXRot = sceneXRot;
                //oldYRot = sceneYRot;

                if (Input.touchCount >= 2)
                {
                    Vector2 touch0, touch1;
                    
                    touch0 = Input.GetTouch(0).position;
                    touch1 = Input.GetTouch(1).position;

                    startdistance = Vector2.Distance(touch0, touch1);
                    oldzoom = zoomlevel;
                }
            }
            if (Input.GetButton("Fire2"))//right mouse button is down
            {
                //sceneXRot = ((Input.mousePosition.x - mouseX) / maincam.pixelHeight) * mouseMulti + oldXRot;
                //sceneYRot = ((Input.mousePosition.y - mouseY) / maincam.pixelHeight) * mouseMulti + oldYRot;
                if (Input.touchCount >= 2)
                {
                    Vector2 touch0, touch1;

                    touch0 = Input.GetTouch(0).position;
                    touch1 = Input.GetTouch(1).position;

                    distance = Vector2.Distance(touch0, touch1);
                }

                zoomlevel = oldzoom + (startdistance - distance)/startdistance;

            }

            if (UnityEngine.Input.GetMouseButton(0))
            {
                if (orbitstart)
                {
                    sceneXRot = ((Input.mousePosition.x - mouseX) / maincam.pixelHeight) * mouseMulti + oldXRot;
                    sceneYRot = ((Input.mousePosition.y - mouseY) / maincam.pixelHeight) * mouseMulti + oldYRot;
                }

            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                orbitstart = false;

            }


            sceneYRot = Mathf.Clamp(sceneYRot, -300, 50);
            cameraTarget.transform.rotation = Quaternion.Euler(-sceneYRot / Mathf.PI, sceneXRot / Mathf.PI, 0);

            //zoomlevel -= Input.mouseScrollDelta.y * zoomscale;
            zoomlevel = Mathf.Clamp(zoomlevel, 1, 10);

            maincam.transform.localPosition = new Vector3(0, 1, -zoomlevel);

            clampedXRot = (sceneXRot / Mathf.PI) % 360;
            if (clampedXRot < 0)
            {
                clampedXRot += 360;
            }


#endif

        }
        else {

#if UNITY_STANDALONE_WIN

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
#else

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                bool overUI = false;
                foreach (Touch touch in Input.touches)
                {
                    int id = touch.fingerId;
                    if (EventSystem.current.IsPointerOverGameObject(id))
                    {
                        overUI = true;
                    }
                }

                if (!overUI)
                {
                    orbitstart = true;
                    //record initial mouse position when click starts
                    mouseX = Input.mousePosition.x;
                    mouseY = Input.mousePosition.y;
                    oldXRot = sceneXRot;
                    oldYRot = sceneYRot;
                }
            }

            if (Input.GetButtonDown("Fire2"))//right mouse button was just clicked
            {
                orbitstart = false;
                //record initial mouse position when click starts
                //mouseX = Input.mousePosition.x;
                //mouseY = Input.mousePosition.y;
                //oldXRot = sceneXRot;
                //oldYRot = sceneYRot;

                if (Input.touchCount >= 2)
                {
                    Vector2 touch0, touch1;

                    touch0 = Input.GetTouch(0).position;
                    touch1 = Input.GetTouch(1).position;

                    startdistance = Vector2.Distance(touch0, touch1);
                    oldzoom = zoomlevel;
                }
            }
            if (Input.GetButton("Fire2"))//right mouse button is down
            {
                //sceneXRot = ((Input.mousePosition.x - mouseX) / maincam.pixelHeight) * mouseMulti + oldXRot;
                //sceneYRot = ((Input.mousePosition.y - mouseY) / maincam.pixelHeight) * mouseMulti + oldYRot;
                if (Input.touchCount >= 2)
                {
                    Vector2 touch0, touch1;

                    touch0 = Input.GetTouch(0).position;
                    touch1 = Input.GetTouch(1).position;

                    distance = Vector2.Distance(touch0, touch1);
                }

                zoomlevel = oldzoom + (startdistance - distance) / startdistance;

            }

            if (UnityEngine.Input.GetMouseButton(0))
            {
                if (orbitstart)
                {
                    sceneXRot = ((Input.mousePosition.x - mouseX) / maincam.pixelHeight) * mouseMulti + oldXRot;
                    sceneYRot = ((Input.mousePosition.y - mouseY) / maincam.pixelHeight) * mouseMulti + oldYRot;
                }

            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                orbitstart = false;

            }

            zoomlevel = Mathf.Clamp(zoomlevel, 0.5f, 10);

            float actualzoom = zoomlevel * zoomlevel;
            maincam.orthographicSize = actualzoom;
            UIcam.orthographicSize = actualzoom;


            
#endif
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

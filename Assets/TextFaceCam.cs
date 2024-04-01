using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFaceCam : MonoBehaviour
{

    //used to make the text always face the camera
    // Start is called before the first frame update
    public Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = mainCam.transform.rotation;
    }
}

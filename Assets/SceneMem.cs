using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;

public class SceneMem : MonoBehaviour
{
    public GameObject newProject;
    public GameObject newprojectType;
    public GameObject StartSelection;

    public string openModelPath;

    public bool wallmounted = false;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public int sceneType = -1;
    public void SetSceneType(int type)
    {
        //sets the scene type, -1 for blank scene, 0+ for example project //-2 for open custom project
        sceneType = type;
    }

    public void EnterTool()
    { 
    Application.LoadLevel(1);
    }

    public void Openproject()
    {
        try
        {
            openModelPath = StandaloneFileBrowser.OpenFilePanel("Open File", "", "shelf", false)[0];
        }
        catch (System.Exception)
        {
            //canceled
            Debug.Log("canceled open file, do nothing");
            return;
            throw;
        }
        
        sceneType = -2;
        EnterTool();
    }

    public void NewprojectMenu()
    {
        StartSelection.SetActive(false);
        newProject.SetActive(true);
    }

    public void NewProjectTypeMenu()
    {
        newProject.SetActive(false);
        newprojectType.SetActive(true);
    }

    public void SetWallMounted(bool _mounted)
    {
        wallmounted = _mounted;
    }
}

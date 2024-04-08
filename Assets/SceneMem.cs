using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMem : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public int sceneType = -1;
    public void SetSceneType(int type)
    {
        //sets the scene type, -1 for blank scene, 0+ for example project
        sceneType = type;
    }

    public void EnterTool()
    { 
    Application.LoadLevel(1);
    }
}

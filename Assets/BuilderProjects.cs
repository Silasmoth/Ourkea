using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuilderProjects : MonoBehaviour
{
    public List<Net_Furniture> projectList;
    public byte[] projectToLoad; //this stores the project we want to view
    private void Awake()
    {
        projectList = new List<Net_Furniture>();
    }
    public void AddProject(Net_Furniture _new)
    {
        projectList.Add(_new);
    }
}

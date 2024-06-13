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

    public void RemoveProject(string ID)
    {
        foreach (Net_Furniture p in projectList)
        {
            if (p.furnitureID == ID) {
            //found the furniture to be removed
            
                projectList.Remove(p);
                
                break;
            
            }

        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalableComponent : MonoBehaviour
{
    public bool startChange = false; //set to true to define the origin when a change is made from which to update all other blocks around
    public bool wasChanged = false; //has this object been given a new scale and or is in the correct position

    public GameObject T, TR, R, BR, B, BL, L, TL;   //these are the mesh components of the 9 tile object


    public GameObject[] snappingPoints; //these are the snapping points that this module has
    public ScalableComponent[] ConnectedModules; //this holds connected modules, with the index indicating which snappingpoint they are connected to on the array above
    public float oldBlockWidth,oldBlockHeight; //stores previous dimentions, so that the block knows if something updated or not

    public float blockWidth = 1.0f; //the width of this module
    public float blockHeight = 1.0f;   //the height of this module
    public float blockDepth = 0.4f; //the depth of this module, for now set as 0.4
    public float startingwidth = 3 / 4; //the size of each 9 tile component at scale of 1


    public GameObject componentVolume; //the object with the collider for this module
    public List<ScalableComponent> myConnectionsSides; //connected modules that are linked in height (attatched left or right)
    public List<ScalableComponent> myConnectionsTopBot; //connected modules that are linked in width (attatched below or above)
    public void recalculateDimentions()
    {
        T.transform.localPosition = new Vector3(0, blockHeight / 2,0);
        B.transform.localPosition = new Vector3(0,- blockHeight / 2, 0);
        R.transform.localPosition = new Vector3(0, 0, -blockWidth/2);
        L.transform.localPosition = new Vector3(0, 0, blockWidth / 2);
        T.transform.localScale = new Vector3(1, (blockWidth-2*startingwidth)/startingwidth, 1);
        B.transform.localScale = new Vector3(1, (blockWidth - 2 * startingwidth) / startingwidth, 1);
        R.transform.localScale = new Vector3(1, 1,(blockHeight - 2 * startingwidth) / startingwidth);
        L.transform.localScale = new Vector3(1,1, (blockHeight - 2 * startingwidth) / startingwidth);


        TR.transform.localPosition = new Vector3(0, blockHeight / 2, -blockWidth / 2);
        BR.transform.localPosition = new Vector3(0, -blockHeight / 2, -blockWidth / 2);
        TL.transform.localPosition = new Vector3(0, blockHeight / 2, blockWidth / 2);
        BL.transform.localPosition = new Vector3(0, -blockHeight / 2, blockWidth / 2);

        componentVolume.transform.localScale = new Vector3(blockDepth -0.01f, blockWidth-0.01f, blockHeight-0.01f);

        CheckChangeSclae();
        if (wasChanged)
        {
           
                for (int i = 0; i < ConnectedModules.Length; i++)
                {
                    if (ConnectedModules[i] != null)
                    {
                        if (ConnectedModules[i].wasChanged == false)
                        {
                        ConnectedModules[i].wasChanged = true;
                        ConnectedModules[i].SetPosition(snappingPoints[i].GetComponent<BlockSnap>().snapPos.position, snappingPoints[i].GetComponent<BlockSnap>().targetsnapIndex);
                        ConnectedModules[i].recalculateDimentions();
                        }
                        
                    }
                }
            
        }
        
        
    }

    public bool CheckChangeSclae() //checks to see if dimention was changed, in which case we should update the scale and positions of adjacent blocks
    {
        bool changed = false;
        if (blockWidth != oldBlockWidth)
        {
            oldBlockWidth = blockWidth;
            //block width has been updated, update top/bottom connected objects
            if (myConnectionsTopBot != null && myConnectionsTopBot.Count != 0)  //check to see if we are connected to any modules above or bellow
            {
                foreach (var item in myConnectionsTopBot)
                {
                    item.blockWidth = blockWidth;
                    item.recalculateDimentions();
                }

            }
            changed = true;
        }

        if (blockHeight != oldBlockHeight)
        {
            oldBlockHeight = blockHeight;
            //block height has been updated, update side connections connected objects
            if (myConnectionsSides != null && myConnectionsSides.Count != 0)  //check to see if we are connected to any modules above or bellow
            {
                foreach (var item in myConnectionsSides)
                {
                    item.blockHeight = blockHeight;
                    item.recalculateDimentions();
                }

            }
            changed = true;
        }

        return changed;
    }
    public void SetPosition(Vector3 snapPos, int snapIndex)
    {
        transform.position = snapPos - snappingPoints[snapIndex].transform.localPosition;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        if(ConnectedModules.Length < snappingPoints.Length)
        {
            ConnectedModules = new ScalableComponent[snappingPoints.Length];
        }
    }

    // Update is called once per frame
    void Update()
    {
        wasChanged = false;
        if (startChange)
        {
            wasChanged = true;
            startChange = false;
        }
            

            recalculateDimentions();
        
            wasChanged = false;
            
        
            
        //}
       
    }
}

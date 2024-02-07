using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalableComponent : MonoBehaviour
{

    //Theese are used for updating dimensions, to determine which dimension should be kept and which need to be changed
    public bool startChange = false; //set to true to define the origin when a change is made from which to update all other blocks around
    public bool wasChanged = false; //has this object been given a new scale and or is in the correct position

    public bool isCorner = false; //should this be treated as a corner module or not

    //Flat module mesh components
    public GameObject T, TR, R, BR, B, BL, L, TL;   //these are the mesh components of the sides of the furniture module

    //Corner module mesh Components
    public GameObject TMR, TM, TML, BMR, BM, BML;

    public GameObject[] snappingPoints; //these are the snapping points that this module has
    public ScalableComponent[] ConnectedModules; //this holds connected modules, with the index indicating which snappingpoint they are connected to on the array above
    public float oldBlockWidth,oldBlockHeight,oldBlockWidthB; //stores previous dimentions, so that the block knows if something updated or not

    public float blockWidth = 1.0f; //the width of this module (in the case of a corner module this is the L side width)
    public float blockWidthB = 1.0f; //the other width of this module in the case of a corner module (R side width)
    public float blockHeight = 1.0f;   //the height of this module
    public float blockDepth = 0.4f; //the depth of this module, for now set as 0.4 (should probably also be variable but I'll have to split up the modules even more)
    public float startingwidth = 3 / 4; //the size of each 9 tile component at scale of 1


    public GameObject componentVolume; //the object with the collider for this module
   
    public void recalculateDimentions()
    {
        //For basic flat bocks------------------------------------------------------------
        if (!isCorner)
        {
            #region Flat rectangular shapes
            //updates this modules mesh components positions and scales
            //first do positions for flat sides (top,bottom,left,right)
            T.transform.localPosition = new Vector3(0, blockHeight / 2, 0);
            B.transform.localPosition = new Vector3(0, -blockHeight / 2, 0);
            R.transform.localPosition = new Vector3(0, 0, -blockWidth / 2);
            L.transform.localPosition = new Vector3(0, 0, blockWidth / 2);
            //then stetch the sides to be the right lengths
            T.transform.localScale = new Vector3(1, 1, (blockWidth - 2 * startingwidth) / startingwidth);
            B.transform.localScale = new Vector3(1, 1, (blockWidth - 2 * startingwidth) / startingwidth);
            R.transform.localScale = new Vector3(1, (blockHeight - 2 * startingwidth) / startingwidth, 1);
            L.transform.localScale = new Vector3(1, (blockHeight - 2 * startingwidth) / startingwidth, 1);

            //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
            TR.transform.localPosition = new Vector3(0, blockHeight / 2, -blockWidth / 2);
            BR.transform.localPosition = new Vector3(0, -blockHeight / 2, -blockWidth / 2);
            TL.transform.localPosition = new Vector3(0, blockHeight / 2, blockWidth / 2);
            BL.transform.localPosition = new Vector3(0, -blockHeight / 2, blockWidth / 2);

            //update the volume that fills this module
            componentVolume.transform.localScale = new Vector3(blockDepth - 0.01f, blockHeight - 0.01f, blockWidth - 0.01f);


           

            #endregion
        }
        //For basic corner blocks------------------------------------------------------------
        if(isCorner)
        {
            #region corner blocks
            //updates this modules mesh components positions and scales
            //first do the positions for the top and bottom middle parts 
            TM.transform.localPosition = new Vector3(0, blockHeight / 2, 0);
            BM.transform.localPosition = new Vector3(0, -blockHeight / 2, 0);

            //Then do positions for the other top mesh components that are scaled linearly (TMR TML BML BMR)
            TML.transform.localPosition = new Vector3(0, blockHeight / 2, blockDepth / 2);
            BML.transform.localPosition = new Vector3(0, -blockHeight / 2, blockDepth / 2);
            TMR.transform.localPosition = new Vector3(-blockDepth / 2, blockHeight / 2, 0);
            BMR.transform.localPosition = new Vector3(-blockDepth / 2, -blockHeight / 2, 0);

            //Then do positions for the side mesh components that are scaled vertically

            R.transform.localPosition = new Vector3(-(blockWidthB - blockDepth / 2), 0, 0);
            L.transform.localPosition = new Vector3(0, 0, (blockWidth - blockDepth / 2));


            //then stetch the sides to be the right lengths

            R.transform.localScale = new Vector3(1, (blockHeight - 2 * startingwidth) / startingwidth, 1);
            L.transform.localScale = new Vector3(1, (blockHeight - 2 * startingwidth) / startingwidth, 1);

            TML.transform.localScale = new Vector3(1, 1, (blockWidth - startingwidth - blockDepth) / startingwidth);
            BML.transform.localScale = new Vector3(1, 1, (blockWidth - startingwidth - blockDepth) / startingwidth);

            TMR.transform.localScale = new Vector3((blockWidthB - startingwidth - blockDepth) / startingwidth, 1,1);
            BMR.transform.localScale = new Vector3((blockWidthB - startingwidth - blockDepth) / startingwidth, 1, 1);


            //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
            TR.transform.localPosition = new Vector3(-(blockWidthB - blockDepth / 2), blockHeight / 2, 0);
            BR.transform.localPosition = new Vector3(-(blockWidthB - blockDepth / 2), -blockHeight / 2, 0);
            TL.transform.localPosition = new Vector3(0, blockHeight / 2, (blockWidth - blockDepth / 2));
            BL.transform.localPosition = new Vector3(0, -blockHeight / 2, (blockWidth - blockDepth / 2));

            //update the volume that fills this module - cant use a box for corner, need to find a different way
            //componentVolume.transform.localScale = new Vector3(blockDepth - 0.01f, blockHeight - 0.01f, blockWidth - 0.01f);


            CheckChangeScale(); //check to see if scale was actually changed, if so adjust scale of connected modules accordingly

            if (wasChanged)
            {
                //position has been changed or the origin module for a scale change

                for (int i = 0; i < ConnectedModules.Length; i++) //tell all connected modules to update their positions if they are not already updated
                {
                    if (ConnectedModules[i] != null)//this side has a connected module
                    {
                        if (ConnectedModules[i].wasChanged == false)//has not already had its position updated
                        {
                            ConnectedModules[i].wasChanged = true;
                            ConnectedModules[i].SetPosition(snappingPoints[i].GetComponent<BlockSnap>().snapPos.position, snappingPoints[i].GetComponent<BlockSnap>().targetsnapIndex);//update modules position
                            ConnectedModules[i].recalculateDimentions();//needs to be called to propegte the position update to further connected modules
                        }

                    }
                }

            }
            #endregion
        }

        CheckChangeScale(); //check to see if scale was actually changed, if so adjust scale of connected modules accordingly

        if (wasChanged)
        {
            //position has been changed or the origin module for a scale change

            for (int i = 0; i < ConnectedModules.Length; i++) //tell all connected modules to update their positions if they are not already updated
            {
                if (ConnectedModules[i] != null)//this side has a connected module
                {
                    if (ConnectedModules[i].wasChanged == false)//has not already had its position updated
                    {
                        ConnectedModules[i].wasChanged = true;
                        ConnectedModules[i].SetPosition(snappingPoints[i].GetComponent<BlockSnap>().snapPos.position, snappingPoints[i].GetComponent<BlockSnap>().targetsnapIndex);//update modules position
                        ConnectedModules[i].recalculateDimentions();//needs to be called to propegte the position update to further connected modules
                    }

                }
            }

        }
    }

    public bool CheckChangeScale() //checks to see if dimention was changed, in which case we should update the scale and positions of adjacent blocks
    {
        bool changed = false;
        if (blockWidth != oldBlockWidth || blockWidthB != oldBlockWidthB)
        {
            oldBlockWidth = blockWidth;
            oldBlockWidthB = blockWidthB;
            //block width has been updated, update top/bottom connected objects
            if (ConnectedModules[0] != null)
            {
                ConnectedModules[0].blockWidth = blockWidth;
                ConnectedModules[0].blockWidthB = blockWidthB;//only relevant for corners but no issue giving it to non corners
                ConnectedModules[0].recalculateDimentions();
            }
            if (ConnectedModules[1] != null)
            {
                ConnectedModules[1].blockWidth = blockWidth;
                ConnectedModules[1].blockWidthB = blockWidthB;//only relevant for corners but no issue giving it to non corners
                ConnectedModules[1].recalculateDimentions();
            }

            changed = true;
        }

        if (blockHeight != oldBlockHeight)
        {
            oldBlockHeight = blockHeight;
            //block height has been updated, update side connections connected objects
            if (ConnectedModules[2] != null)
            {
                ConnectedModules[2].blockHeight = blockHeight;
                ConnectedModules[2].recalculateDimentions();
            }
            if (ConnectedModules[3] != null)
            {
                ConnectedModules[3].blockHeight = blockHeight;
                ConnectedModules[3].recalculateDimentions();
            }
            changed = true;
        }

        return changed;
    }
    public void SetPosition(Vector3 snapPos, int snapIndex)
    {
        transform.position = snapPos;// - snappingPoints[snapIndex].transform.localPosition;
        transform.Translate(-snappingPoints[snapIndex].transform.localPosition);
        
    }

    // Start is called before the first frame update
    void Start()
    {
        //make sure the connections array is initialised, normally it gets initialised when the block is placed by the blockplacer, but in case it already existed in the scene
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

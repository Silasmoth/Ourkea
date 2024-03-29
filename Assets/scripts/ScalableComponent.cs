using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalableComponent : MonoBehaviour
{

    //selection
    [Header("Selection")]
    public int materialID;
    public bool selected = false;
    public Material unselectedMat;
    public Material selectedMat;

    [Header("Internal Dividers - Horizontal")]
    public bool allowHorizontalDividers = false;//
    public GameObject HDividerprefab;
    public List<GameObject> HDividers;
    public float minHDividerspacing = 0.1f;
    public int HDividercount;
    public float HdividerStartingWidth = 0.364f;
    public float HdividerEdgeBuffer = 0.036f;

    [Header("Internal Dividers - Vertical")]
    public bool allowVerticalDividers = false;//
    public GameObject VDividerprefab;
    public List<GameObject> VDividers;
    public float minVDividerspacing = 0.1f;
    public int VDividercount;
    public float VdividerStartingWidth = 0.364f;
    public float VdividerEdgeBuffer = 0.036f;

    [Header("Module Settings")]
    //For module Type
    public bool isCounter = false; //is this a counter (no height)
    public bool isCorner = false; //should this be treated as a corner module or not
    public bool reverseCorner = false; //is this a corner or reversed corner, only relevant if isCorner is also true
    [Space(10)]

    [Header("Mesh component references")]
    [Space(10)]
    //Flat module mesh components
    public GameObject T;    //these are the mesh components of the sides of the furniture module
    public GameObject TR, R, BR, B, BL, L, TL, C;
    //Corner module mesh Components
    public GameObject TMR, TM, TML, BMR, BM, BML;

    [Space(10)]

    [Header("Snapping point references")]
    public GameObject[] snappingPoints; //these are the snapping points that this module has

    [Space(10)]
    //Theese are used for updating dimensions, to determine which dimension should be kept and which need to be changed
    [HideInInspector] public bool startChange = false; //set to true to define the origin when a change is made from which to update all other blocks around
    [HideInInspector] public bool wasChanged = false; //has this object been given a new scale and or is in the correct position


    [HideInInspector] public ScalableComponent[] ConnectedModules; //this holds connected modules, with the index indicating which snappingpoint they are connected to on the array above
    [HideInInspector] public float oldBlockWidth,oldBlockHeight,oldBlockWidthB; //stores previous dimentions, so that the block knows if something updated or not


    //Dimention information
    [Header("Block dimention limits")]
    //maximums
    public float maxWidth = 1.5f;
    public float maxWidthB = 1.5f;
    public float Maxheight = 2.0f;

    //minimums
    public float minWidth = 0.3f;
    public float minWidthB = 0.55f;
    public float minheight = 0.3f;
    [Space(10)]

    [Header("Block dimention")]
    public float blockWidth = 1.0f; //the width of this module (in the case of a corner module this is the L side width)
    public float blockWidthB = 1.0f; //the other width of this module in the case of a corner module (R side width)
    public float blockHeight = 1.0f;   //the height of this module
    public float blockDepth = 0.4f; //the depth of this module, for now set as 0.4 (should probably also be variable but I'll have to split up the modules even more)
    [Space(10)]
    public float startingwidth = 3 / 4; //the size of each 9 tile component at scale of 1
    [Space(10)]

    [Header("Module volume collider reference")]
    public GameObject componentVolume; //the object with the collider for this module
    public bool CheckDimentionsMinMax()
    {
        bool rulebroken = false;

        //check if under minimums
        if (blockWidth < minWidth)
        {
            blockWidth = minWidth;
            rulebroken = true;
        }

        if (blockWidthB < minWidthB)
        {
            blockWidthB = minWidthB;
            rulebroken = true;
        }

        if (blockHeight < minheight)
        {
            blockHeight = minheight;
            rulebroken = true;
        }

        //check if over maximums
        if (blockWidth > maxWidth)
        {
            blockWidth = maxWidth;
            rulebroken = true;
        }
        if (blockWidthB > maxWidthB)
        {
            blockWidthB = maxWidthB;
            rulebroken = true;
        }
        if (blockHeight > Maxheight)
        {
            blockHeight = Maxheight;
            rulebroken = true;
        }



        return rulebroken;
    }

    public void recalculateDimentions(bool effectAdjacent)
    {

        if (!isCounter)
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

                //scale the center mesh component if there is one
                if (C != null)
                    C.transform.localScale = new Vector3(1, (blockHeight - 2 * startingwidth) / startingwidth, (blockWidth - 2 * startingwidth) / startingwidth);


                #endregion
            }
            //For basic corner blocks------------------------------------------------------------
            if (isCorner && !reverseCorner)
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

                TMR.transform.localScale = new Vector3((blockWidthB - startingwidth - blockDepth) / startingwidth, 1, 1);
                BMR.transform.localScale = new Vector3((blockWidthB - startingwidth - blockDepth) / startingwidth, 1, 1);


                //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
                TR.transform.localPosition = new Vector3(-(blockWidthB - blockDepth / 2), blockHeight / 2, 0);
                BR.transform.localPosition = new Vector3(-(blockWidthB - blockDepth / 2), -blockHeight / 2, 0);
                TL.transform.localPosition = new Vector3(0, blockHeight / 2, (blockWidth - blockDepth / 2));
                BL.transform.localPosition = new Vector3(0, -blockHeight / 2, (blockWidth - blockDepth / 2));

                //update the volume that fills this module - box is not very accurate for corner, need to find a different way
                componentVolume.transform.localScale = new Vector3(blockWidthB - 0.1f, blockHeight - 0.01f, blockWidth - 0.1f);
                componentVolume.transform.localPosition = new Vector3(-(blockWidthB / 2 - blockDepth / 2), 0, blockWidth / 2 - blockDepth / 2);

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
                                ConnectedModules[i].recalculateDimentions(true);//needs to be called to propegte the position update to further connected modules
                            }

                        }
                    }

                }
                #endregion
            }
            //For reversed corner blocks-------------------------------------------------------
            if (isCorner && reverseCorner)
            {
                #region corner blocks
                //updates this modules mesh components positions and scales
                //first do the positions for the top and bottom middle parts 
                TM.transform.localPosition = new Vector3(0, blockHeight / 2, 0);
                BM.transform.localPosition = new Vector3(0, -blockHeight / 2, 0);

                //Then do positions for the other top mesh components that are scaled linearly (TMR TML BML BMR)
                TML.transform.localPosition = new Vector3(0, blockHeight / 2, blockDepth / 2);
                BML.transform.localPosition = new Vector3(0, -blockHeight / 2, blockDepth / 2);
                TMR.transform.localPosition = new Vector3(blockDepth / 2, blockHeight / 2, 0);
                BMR.transform.localPosition = new Vector3(blockDepth / 2, -blockHeight / 2, 0);

                //Then do positions for the side mesh components that are scaled vertically

                R.transform.localPosition = new Vector3((blockWidthB - blockDepth / 2), 0, 0);
                L.transform.localPosition = new Vector3(0, 0, (blockWidth - blockDepth / 2));


                //then stetch the sides to be the right lengths

                R.transform.localScale = new Vector3(1, (blockHeight - 2 * startingwidth) / startingwidth, 1);
                L.transform.localScale = new Vector3(1, (blockHeight - 2 * startingwidth) / startingwidth, 1);

                TML.transform.localScale = new Vector3(1, 1, (blockWidth - startingwidth - blockDepth) / startingwidth);
                BML.transform.localScale = new Vector3(1, 1, (blockWidth - startingwidth - blockDepth) / startingwidth);

                TMR.transform.localScale = new Vector3((blockWidthB - startingwidth - blockDepth) / startingwidth, 1, 1);
                BMR.transform.localScale = new Vector3((blockWidthB - startingwidth - blockDepth) / startingwidth, 1, 1);


                //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
                TR.transform.localPosition = new Vector3((blockWidthB - blockDepth / 2), blockHeight / 2, 0);
                BR.transform.localPosition = new Vector3((blockWidthB - blockDepth / 2), -blockHeight / 2, 0);
                TL.transform.localPosition = new Vector3(0, blockHeight / 2, (blockWidth - blockDepth / 2));
                BL.transform.localPosition = new Vector3(0, -blockHeight / 2, (blockWidth - blockDepth / 2));

                //update the volume that fills this module - box is not very accurate for corner, need to find a different way
                componentVolume.transform.localScale = new Vector3(blockWidthB - 0.1f, blockHeight - 0.01f, blockWidth - 0.1f);
                componentVolume.transform.localPosition = new Vector3((blockWidthB / 2 - blockDepth / 2), 0, blockWidth / 2 - blockDepth / 2);
                if (effectAdjacent)
                {
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
                                    ConnectedModules[i].recalculateDimentions(true);//needs to be called to propegte the position update to further connected modules
                                }

                            }
                        }

                    }
                }

                #endregion
            }

            if (!isCorner)
            {
                #region normal block update adjacent
                if (effectAdjacent)
                {
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
                                    ConnectedModules[i].recalculateDimentions(true);//needs to be called to propegte the position update to further connected modules
                                }

                            }
                        }

                    }
                }

                #endregion
            }

            if (allowHorizontalDividers || allowVerticalDividers)
            {
                RegenerateDividers();//this should only be done when the number of dividers is changed, I should add another function for only adjusting position/scale of dividers
            }
        }
        else
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
                
                B.transform.localScale = new Vector3(1, 1, (blockWidth - 2 * startingwidth) / startingwidth);
                

                //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes    
                BR.transform.localPosition = new Vector3(0, -blockHeight / 2, -blockWidth / 2);
                BL.transform.localPosition = new Vector3(0, -blockHeight / 2, blockWidth / 2);

                //update the volume that fills this module
                componentVolume.transform.localScale = new Vector3(blockDepth - 0.01f, blockHeight - 0.01f, blockWidth - 0.01f);

               


                #endregion
            }
            //For basic corner blocks------------------------------------------------------------
            if (isCorner && !reverseCorner)
            {
                #region corner blocks
                //updates this modules mesh components positions and scales
                //first do the positions for the top and bottom middle parts 
                TM.transform.localPosition = new Vector3(0, blockHeight / 2, 0);
                BM.transform.localPosition = new Vector3(0, -blockHeight / 2, 0);

               

                //Then do positions for the side mesh components that are scaled vertically

                R.transform.localPosition = new Vector3(-(blockWidthB - blockDepth / 2), 0, 0);
                L.transform.localPosition = new Vector3(0, 0, (blockWidth - blockDepth / 2));


                //then stetch the sides to be the right lengths

                R.transform.localScale = new Vector3(1, (blockHeight - 2 * startingwidth) / startingwidth, 1);
                L.transform.localScale = new Vector3(1, (blockHeight - 2 * startingwidth) / startingwidth, 1);

                

                //update the volume that fills this module - box is not very accurate for corner, need to find a different way
                componentVolume.transform.localScale = new Vector3(blockWidthB - 0.1f, blockHeight - 0.01f, blockWidth - 0.1f);
                componentVolume.transform.localPosition = new Vector3(-(blockWidthB / 2 - blockDepth / 2), 0, blockWidth / 2 - blockDepth / 2);

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
                                ConnectedModules[i].recalculateDimentions(true);//needs to be called to propegte the position update to further connected modules
                            }

                        }
                    }

                }
                #endregion
            }
            //For reversed corner blocks-------------------------------------------------------
            if (isCorner && reverseCorner)
            {
                #region corner blocks
                //updates this modules mesh components positions and scales
                //first do the positions for the top and bottom middle parts 
                TM.transform.localPosition = new Vector3(0, blockHeight / 2, 0);
                BM.transform.localPosition = new Vector3(0, -blockHeight / 2, 0);

                
                //Then do positions for the side mesh components that are scaled vertically

                R.transform.localPosition = new Vector3((blockWidthB - blockDepth / 2), 0, 0);
                L.transform.localPosition = new Vector3(0, 0, (blockWidth - blockDepth / 2));


                //then stetch the sides to be the right lengths

                R.transform.localScale = new Vector3(1, (blockHeight - 2 * startingwidth) / startingwidth, 1);
                L.transform.localScale = new Vector3(1, (blockHeight - 2 * startingwidth) / startingwidth, 1);


                //update the volume that fills this module - box is not very accurate for corner, need to find a different way
                componentVolume.transform.localScale = new Vector3(blockWidthB - 0.1f, blockHeight - 0.01f, blockWidth - 0.1f);
                componentVolume.transform.localPosition = new Vector3((blockWidthB / 2 - blockDepth / 2), 0, blockWidth / 2 - blockDepth / 2);
                if (effectAdjacent)
                {
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
                                    ConnectedModules[i].recalculateDimentions(true);//needs to be called to propegte the position update to further connected modules
                                }

                            }
                        }

                    }
                }

                #endregion
            }

            if (!isCorner)
            {
                #region normal block update adjacent
                if (effectAdjacent)
                {
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
                                    ConnectedModules[i].recalculateDimentions(true);//needs to be called to propegte the position update to further connected modules
                                }

                            }
                        }

                    }
                }

                #endregion
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
                if(!ConnectedModules[0].wasChanged)//make sure it wasnt the block that started the updating
                {
                    ConnectedModules[0].blockWidth = blockWidth;
                    ConnectedModules[0].blockWidthB = blockWidthB;//only relevant for corners but no issue giving it to non corners
                    ConnectedModules[0].recalculateDimentions(true);
                }
                
            }
            if (ConnectedModules[1] != null)
            {
                if(!ConnectedModules[1].wasChanged)//make sure it wasnt the block that started the updating
                {
                    ConnectedModules[1].blockWidth = blockWidth;
                    ConnectedModules[1].blockWidthB = blockWidthB;//only relevant for corners but no issue giving it to non corners
                    ConnectedModules[1].recalculateDimentions(true);
                }
                
            }

            changed = true;
        }

        if (blockHeight != oldBlockHeight)
        {
            oldBlockHeight = blockHeight;
            //block height has been updated, update side connections connected objects
            if (ConnectedModules[2] != null)
            {
                if(!ConnectedModules[2].wasChanged)//make sure it wasnt the block that started the updating
                {
                    ConnectedModules[2].blockHeight = blockHeight;
                    ConnectedModules[2].recalculateDimentions(true);
                }
                
            }
            if (ConnectedModules[3] != null)
            {
                if(!ConnectedModules[3].wasChanged)//make sure it wasnt the block that started the updating
                {
                    ConnectedModules[3].blockHeight = blockHeight;
                    ConnectedModules[3].recalculateDimentions(true);
                }
                
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
    public void SetPositionAndRotation(Transform snappos, int snapIndex)
    {
        transform.rotation = snappos.rotation * Quaternion.Inverse( snappingPoints[snapIndex].transform.localRotation);
        transform.position = snappos.position;// - snappingPoints[snapIndex].transform.localPosition;
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
            recalculateDimentions(true);
        }
            

            
        
            wasChanged = false;
            
        
            
        //}
       
    }

    public float GetBottomY()
    {
        return transform.position.y - blockHeight / 2;
    }

    public void SetSelected(bool _selected)
    {
        selected = _selected;
        var renderers = transform.GetComponentsInChildren<MeshRenderer>( false);

        foreach (var item in renderers)
        {
            if (selected)
            {
                item.sharedMaterial = selectedMat;
            }
            else {
                item.sharedMaterial = unselectedMat;
            }
        }
    }

    public void RegenerateDividers()
    {
        //Horizontal Dividers
        if (allowHorizontalDividers)
        {
            //remove existing horizontal dividers
            for (int i = 0; i < HDividers.Count; i++)
            {
                Destroy(HDividers[i]);
            }

            HDividers = new List<GameObject>();
            if (HDividercount == 0)
            {
                return;
            }

            //instantiate new dividers and add the to the list
            for (int i = 0; i < HDividercount; i++)
            {
                var temp = Instantiate(HDividerprefab, transform);
                HDividers.Add(temp);
            }

            //position dividers
            float spacing = blockHeight / (HDividercount + 1);

            for (int i = 0; i < HDividers.Count; i++)
            {
                HDividers[i].transform.localPosition = new Vector3(0, (-blockHeight / 2) + spacing * (i + 1), 0);
            }

            float desiredwidth = (blockWidth - HdividerEdgeBuffer) / HdividerStartingWidth;
            //scale dividers
            for (int i = 0; i < HDividers.Count; i++)
            {
                HDividers[i].transform.localScale = new Vector3(1, 1, desiredwidth);
            }
        }

        //Vertical Dividers
        if (allowVerticalDividers)
        {
            //remove existing horizontal dividers
            for (int i = 0; i < VDividers.Count; i++)
            {
                Destroy(VDividers[i]);
            }

            VDividers = new List<GameObject>();
            if (VDividercount == 0)
            {
                return;
            }

            //instantiate new dividers and add the to the list
            for (int i = 0; i < VDividercount; i++)
            {
                var temp = Instantiate(VDividerprefab, transform);
                VDividers.Add(temp);
            }

            //position dividers
            float spacing = blockWidth / (VDividercount + 1);

            for (int i = 0; i < VDividers.Count; i++)
            {
                VDividers[i].transform.localPosition = new Vector3(0, 0,(-blockWidth / 2) + spacing * (i + 1));
            }

            float desiredwidth = (blockHeight - VdividerEdgeBuffer) / VdividerStartingWidth;
            //scale dividers
            for (int i = 0; i < VDividers.Count; i++)
            {
                VDividers[i].transform.localScale = new Vector3(1, desiredwidth, 1);
            }
        }


        var renderers = transform.GetComponentsInChildren<MeshRenderer>(false);

        foreach (var item in renderers)
        {
            if (selected)
            {
                item.sharedMaterial = selectedMat;
            }
            else
            {
                item.sharedMaterial = unselectedMat;
            }
        }
    }
}

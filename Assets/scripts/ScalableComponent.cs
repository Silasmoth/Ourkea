using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalableComponent : MonoBehaviour
{
    //for saving and loading
    public int index;//each module is assigned an index to help with remembering which modules are connected to each other in the saving process

    public bool isFixedMaterial;
    public bool mergeVertical = true;
    public Material fixedMat, fixedMatSelected;

    public byte moduleType = 0;
    //
    public bool DoubleWall = true;
    public float wallThickness = 0.02f;

    [Header("Volume Calculaitons")]
    public float matThickness = 0.02f;//thickness of sheet material
    public float WD, WH, HD;//multiples of front/back, sides and top/bottom face areas
    public float subtracttionArea;//static amount to be subtracted for connections etc


    //selection
    [Header("Selection")]
    public int CoreMaterial = 0;//core material
    //0-Premium Pine Plywood = 91.88/2.9729 = 30.90
    //1-spruce plywood = 62.98 /2.9729 = 21.18
    //2-MDF = 56.98/3.066445 = 18.58
    public int FinishMaterial;//finish material
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
    public float startingHeight = 3 / 4; //the size of each 9 tile component at scale of 1
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
                bool shiftRight = ConnectedModules[2] != null;
                bool shiftLeft = ConnectedModules[3] != null;
                bool shiftDown = (ConnectedModules[1] != null) && ConnectedModules[1].mergeVertical;


                //update snapping points positions/scales

                snappingPoints[0].transform.localPosition = new Vector3(0, blockHeight / 2, 0);
                snappingPoints[1].transform.localPosition = new Vector3(0, -blockHeight / 2, 0);
                snappingPoints[2].transform.localPosition = new Vector3(0, 0, -blockWidth / 2);
                snappingPoints[3].transform.localPosition = new Vector3(0, 0, blockWidth / 2);

                //then stetch the sides to be the right lengths
                snappingPoints[0].transform.localScale = new Vector3(1, 1, (blockWidth - 2 * startingwidth) / startingwidth);
                snappingPoints[1].transform.localScale = new Vector3(1, 1, (blockWidth - 2 * startingwidth) / startingwidth);
                snappingPoints[2].transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, 1);
                snappingPoints[3].transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, 1);

                if (DoubleWall || (!shiftDown && !shiftRight && !shiftLeft))
                {
                    if(T!= null)
                        T.transform.localPosition = new Vector3(0, blockHeight / 2, 0);
                    if (B != null)
                        B.transform.localPosition = new Vector3(0, -blockHeight / 2, 0);
                    if (R != null)
                        R.transform.localPosition = new Vector3(0, 0, -blockWidth / 2);
                    if (L != null)
                        L.transform.localPosition = new Vector3(0, 0, blockWidth / 2);

                    //then stetch the sides to be the right lengths
                    if (T != null)
                        T.transform.localScale = new Vector3(1, 1, (blockWidth - 2 * startingwidth) / startingwidth);
                    if (B != null)
                        B.transform.localScale = new Vector3(1, 1, (blockWidth - 2 * startingwidth) / startingwidth);
                    if (R != null)
                        R.transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, 1);
                    if (L != null)
                        L.transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, 1);



                    //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
                    if (TR != null)
                        TR.transform.localPosition = new Vector3(0, blockHeight / 2, -blockWidth / 2);
                    if (BR != null)
                        BR.transform.localPosition = new Vector3(0, -blockHeight / 2, -blockWidth / 2);
                    if (TL != null)
                        TL.transform.localPosition = new Vector3(0, blockHeight / 2, blockWidth / 2);
                    if (BL != null)
                        BL.transform.localPosition = new Vector3(0, -blockHeight / 2, blockWidth / 2);

                    
                    //scale the center mesh component if there is one
                    if (C != null)
                    {
                        C.transform.localPosition = new Vector3(0, 0, 0);
                        C.transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, (blockWidth - 2 * startingwidth) / startingwidth);
                    }
                }
                else {
                    

                    if (shiftDown && shiftLeft && shiftRight)
                    {
                        //all sides
                        if (T != null)
                            T.transform.localPosition = new Vector3(0, blockHeight / 2, 0);//no change
                        if (B != null)
                            B.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, 0);//down by wallthickness
                        if (R != null)
                            R.transform.localPosition = new Vector3(0, -wallThickness/2, -blockWidth / 2 - wallThickness/2);// over by half wallthickness and down by half wallthickness
                        if (L != null)
                            L.transform.localPosition = new Vector3(0, -wallThickness / 2, blockWidth / 2 + wallThickness/2);//over by half wallthickness and down by half wallthickness

                        //then stetch the sides to be the right lengths
                        if (T != null)
                            T.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness- 2 * startingwidth) / startingwidth);//add an extra wallthcikness
                        if (B != null)
                            B.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness - 2 * startingwidth) / startingwidth); //add extra wall thickness
                        if (R != null)
                            R.transform.localScale = new Vector3(1, (blockHeight + wallThickness - 2 * startingHeight) / startingHeight, 1); //add extra wall thickness
                        if (L != null)
                            L.transform.localScale = new Vector3(1, (blockHeight + wallThickness - 2 * startingHeight) / startingHeight, 1); //add an extra wall thickness



                        //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
                        if (TR != null)
                            TR.transform.localPosition = new Vector3(0, blockHeight / 2, -blockWidth / 2 -  wallThickness/2);//move over by half wall thickness 
                        if (BR != null)
                            BR.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, -blockWidth / 2 - wallThickness/2);//move over by half wall thickness and down by wallthickness
                        if (TL != null)
                            TL.transform.localPosition = new Vector3(0, blockHeight / 2, blockWidth / 2 + wallThickness/2);//move over by half wall thickness
                        if (BL != null)
                            BL.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, blockWidth / 2 + wallThickness/2);//move over by half wall thickness and down by wallthickness

                        //scale the center mesh component if there is one
                        if (C != null)
                        {
                            C.transform.localPosition = new Vector3(0, -wallThickness/2, 0);//move down by half wallthickness
                            C.transform.localScale = new Vector3(1, (blockHeight +wallThickness - 2 * startingHeight) / startingHeight, (blockWidth + wallThickness - 2 * startingwidth) / startingwidth);//add extra wall thickness in both directions
                        }
                            
                    }
                    if (shiftDown && shiftRight && !shiftLeft)
                    {
                        //all but left
                        if (T != null)
                            T.transform.localPosition = new Vector3(0, blockHeight / 2, -wallThickness/4);//move over by quarter wall thickness to right
                        if (B != null)
                            B.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, -wallThickness/4);//down by wallthickness and //move over by quarter wall thickness to right
                        if (R != null)
                            R.transform.localPosition = new Vector3(0, -wallThickness / 2, -blockWidth / 2 - wallThickness / 2);// over by half wallthickness and down by half wallthickness
                        if (L != null)
                            L.transform.localPosition = new Vector3(0, -wallThickness / 2, blockWidth / 2 );// down by half wallthickness

                        //then stetch the sides to be the right lengths
                        if (T != null)
                            T.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness/2 - 2 * startingwidth) / startingwidth);//add half an extra wallthcikness
                        if (B != null)
                            B.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness/2 - 2 * startingwidth) / startingwidth); //add half extra wall thickness
                        if (R != null)
                            R.transform.localScale = new Vector3(1, (blockHeight + wallThickness - 2 * startingHeight) / startingHeight, 1); //add extra wall thickness
                        if (L != null)
                            L.transform.localScale = new Vector3(1, (blockHeight + wallThickness - 2 * startingHeight) / startingHeight, 1); //add an extra wall thickness



                        //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
                        if (TR != null)
                            TR.transform.localPosition = new Vector3(0, blockHeight / 2, -blockWidth / 2 - wallThickness / 2);//move over by half wall thickness
                        if (BR != null)
                            BR.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, -blockWidth / 2 - wallThickness / 2);//move over by half wall thickness and down by wallthickness
                        if (TL != null)
                            TL.transform.localPosition = new Vector3(0, blockHeight / 2, blockWidth / 2);//no change
                        if (BL != null)
                            BL.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, blockWidth / 2 );//down by wallthickness

                        //scale the center mesh component if there is one
                        if (C != null)
                        {
                            C.transform.localPosition = new Vector3(0, -wallThickness / 2, -wallThickness/4);//move down by half wallthickness
                            C.transform.localScale = new Vector3(1, (blockHeight + wallThickness- 2 * startingHeight) / startingHeight, (blockWidth + wallThickness/2 - 2 * startingwidth) / startingwidth);//add half wallthickness in z wallthickness in y
                        }
                    }
                    if (shiftDown && !shiftRight && shiftLeft)
                    {
                        //all but right
                        if (T != null)
                            T.transform.localPosition = new Vector3(0, blockHeight / 2, wallThickness / 4);//move over by quarter wall thickness to left
                        if (B != null)
                            B.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, wallThickness / 4);//down by wallthickness and //move over by quarter wall thickness to left
                        if (R != null)
                            R.transform.localPosition = new Vector3(0, -wallThickness / 2, -blockWidth / 2 );// down by half wallthicknes
                        if (L != null)
                            L.transform.localPosition = new Vector3(0, -wallThickness / 2, blockWidth / 2 + wallThickness/2);// sover by half wallthickness and down by half wallthickness

                        //then stetch the sides to be the right lengths
                        if (T != null)
                            T.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness / 2 - 2 * startingwidth) / startingwidth);//add half an extra wallthcikness
                        if (B != null)
                            B.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness / 2 - 2 * startingwidth) / startingwidth); //add half extra wall thickness
                        if (R != null)
                            R.transform.localScale = new Vector3(1, (blockHeight + wallThickness - 2 * startingHeight) / startingHeight, 1); //add extra wall thickness
                        if (L != null)
                            L.transform.localScale = new Vector3(1, (blockHeight + wallThickness - 2 * startingHeight) / startingHeight, 1); //add an extra wall thickness



                        //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
                        if (TR != null)
                            TR.transform.localPosition = new Vector3(0, blockHeight / 2, -blockWidth / 2);//no change
                        if (BR != null)
                            BR.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, -blockWidth / 2 );// down by wallthickness
                        if (TL != null)
                            TL.transform.localPosition = new Vector3(0, blockHeight / 2, blockWidth / 2 + wallThickness / 2);//move over by half wall thickness
                        if (BL != null)
                            BL.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, blockWidth / 2 + wallThickness/2);//down by wallthickness and over by half wallthickness

                        //scale the center mesh component if there is one
                        if (C != null)
                        {
                            C.transform.localPosition = new Vector3(0, -wallThickness / 2, wallThickness / 4);//move down by half wallthickness
                            C.transform.localScale = new Vector3(1, (blockHeight + wallThickness - 2 * startingHeight) / startingHeight, (blockWidth + wallThickness / 2 - 2 * startingwidth) / startingwidth);//add half wallthickness in z wallthickness in y
                        }

                    }
                    if (shiftDown && !shiftRight && !shiftLeft)
                    {
                        //only down
                        if (T != null)
                            T.transform.localPosition = new Vector3(0, blockHeight / 2, 0);//no change 
                        if (B != null)
                            B.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, 0);//down by thickness
                        if (R != null)
                            R.transform.localPosition = new Vector3(0, -wallThickness/2, -blockWidth / 2); //down by half wallthickness
                        if (L != null)
                            L.transform.localPosition = new Vector3(0, -wallThickness / 2, blockWidth / 2);//''

                        //then stetch the sides to be the right lengths
                        if (T != null)
                            T.transform.localScale = new Vector3(1, 1, (blockWidth - 2 * startingwidth) / startingwidth);//no change
                        if (B != null)
                            B.transform.localScale = new Vector3(1, 1, (blockWidth - 2 * startingwidth) / startingwidth);//''
                        if (R != null)
                            R.transform.localScale = new Vector3(1, (blockHeight + wallThickness - 2 * startingHeight) / startingHeight, 1);//add extra wallthickness
                        if (L != null)
                            L.transform.localScale = new Vector3(1, (blockHeight + wallThickness - 2 * startingHeight) / startingHeight, 1);//''



                        //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
                        if (TR != null)
                            TR.transform.localPosition = new Vector3(0, blockHeight / 2, -blockWidth / 2);//no change
                        if (BR != null)
                            BR.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, -blockWidth / 2);//down by wallthickness
                        if (TL != null)
                            TL.transform.localPosition = new Vector3(0, blockHeight / 2, blockWidth / 2);//no change
                        if (BL != null)
                            BL.transform.localPosition = new Vector3(0, -blockHeight / 2 - wallThickness, blockWidth / 2);//down by wallthickness


                        //scale the center mesh component if there is one
                        if (C != null)
                        {
                            C.transform.localPosition = new Vector3(0, -wallThickness/2, 0);//down by half wallthickness
                            C.transform.localScale = new Vector3(1, (blockHeight + wallThickness - 2 * startingHeight) / startingHeight, (blockWidth - 2 * startingwidth) / startingwidth);//add extra wallthickness to y
                        }

                    }
                    if (!shiftDown && shiftRight && shiftLeft)
                    {
                        //all but down
                        if (T != null)
                            T.transform.localPosition = new Vector3(0, blockHeight / 2, 0);//no change
                        if (B != null)
                            B.transform.localPosition = new Vector3(0, -blockHeight / 2 , 0);//no change
                        if (R != null)
                            R.transform.localPosition = new Vector3(0, 0, -blockWidth / 2 - wallThickness / 2);// over by half wallthickness 
                        if (L != null)
                            L.transform.localPosition = new Vector3(0, 0, blockWidth / 2 + wallThickness / 2);//over by half wallthickness 

                        //then stetch the sides to be the right lengths
                        if (T != null)
                            T.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness - 2 * startingwidth) / startingwidth);//add an extra wallthcikness
                        if (B != null)
                            B.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness - 2 * startingwidth) / startingwidth); //add extra wall thickness
                        if (R != null)
                            R.transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, 1); //no change
                        if (L != null)
                            L.transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, 1); //noo change



                        //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
                        if (TR != null)
                            TR.transform.localPosition = new Vector3(0, blockHeight / 2, -blockWidth / 2 - wallThickness / 2);//move over by half wall thickness 
                        if (BR != null)
                            BR.transform.localPosition = new Vector3(0, -blockHeight / 2 , -blockWidth / 2 - wallThickness / 2);//move over by half wall thickness
                        if (TL != null)
                            TL.transform.localPosition = new Vector3(0, blockHeight / 2, blockWidth / 2 + wallThickness / 2);//move over by half wall thickness
                        if (BL != null)
                            BL.transform.localPosition = new Vector3(0, -blockHeight / 2 , blockWidth / 2 + wallThickness / 2);//move over by half wall thickness

                        //scale the center mesh component if there is one
                        if (C != null)
                        {
                            C.transform.localPosition = new Vector3(0, 0, 0);//no change
                            C.transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, (blockWidth + wallThickness - 2 * startingwidth) / startingwidth);//add extra wall thickness in z
                        }
                    }
                    if (!shiftDown && !shiftRight && shiftLeft)
                    {
                        //only left
                        if (T != null)
                            T.transform.localPosition = new Vector3(0, blockHeight / 2, wallThickness / 4);//move over by quarter wall thickness to left
                        if (B != null)
                            B.transform.localPosition = new Vector3(0, -blockHeight / 2 , wallThickness / 4);//move over by quarter wall thickness to left
                        if (R != null)
                            R.transform.localPosition = new Vector3(0, 0, -blockWidth / 2);// no change
                        if (L != null)
                            L.transform.localPosition = new Vector3(0, 0, blockWidth / 2 + wallThickness / 2);// sover by half wallthickness and down by half wallthickness

                        //then stetch the sides to be the right lengths
                        if (T != null)
                            T.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness / 2 - 2 * startingwidth) / startingwidth);//add half an extra wallthcikness
                        if (B != null)
                            B.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness / 2 - 2 * startingwidth) / startingwidth); //add half extra wall thickness
                        if (R != null)
                            R.transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, 1); //no chnge
                        if (L != null)
                            L.transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, 1); //no change



                        //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
                        if (TR != null)
                            TR.transform.localPosition = new Vector3(0, blockHeight / 2, -blockWidth / 2);//no change
                        if (BR != null)
                            BR.transform.localPosition = new Vector3(0, -blockHeight / 2 , -blockWidth / 2);// no change
                        if (TL != null)
                            TL.transform.localPosition = new Vector3(0, blockHeight / 2, blockWidth / 2 + wallThickness / 2);//move over by half wall thickness
                        if (BL != null)
                            BL.transform.localPosition = new Vector3(0, -blockHeight / 2, blockWidth / 2 + wallThickness / 2);//over by half wallthickness

                        //scale the center mesh component if there is one
                        if (C != null)
                        {
                            C.transform.localPosition = new Vector3(0, 0, wallThickness / 4);//move over by quarter thickness
                            C.transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, (blockWidth + wallThickness / 2 - 2 * startingwidth) / startingwidth);//add half wallthickness in z
                        }

                    }

                    if (!shiftDown && shiftRight && !shiftLeft)
                    {
                        //only right
                        if (T != null)
                            T.transform.localPosition = new Vector3(0, blockHeight / 2, -wallThickness / 4);//move over by quarter wall thickness to right
                        if (B != null)
                            B.transform.localPosition = new Vector3(0, -blockHeight / 2, -wallThickness / 4);//move over by quarter wall thickness to right
                        if (R != null)
                            R.transform.localPosition = new Vector3(0, 0, -blockWidth / 2 - wallThickness / 2);// over by half wallthickness
                        if (L != null)
                            L.transform.localPosition = new Vector3(0, 0, blockWidth / 2);// no change

                        //then stetch the sides to be the right lengths
                        if (T != null)
                            T.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness / 2 - 2 * startingwidth) / startingwidth);//add half an extra wallthcikness
                        if (B != null)
                            B.transform.localScale = new Vector3(1, 1, (blockWidth + wallThickness / 2 - 2 * startingwidth) / startingwidth); //add half extra wall thickness
                        if (R != null)
                            R.transform.localScale = new Vector3(1, (blockHeight  - 2 * startingHeight) / startingHeight, 1); //no change
                        if (L != null)
                            L.transform.localScale = new Vector3(1, (blockHeight  - 2 * startingHeight) / startingHeight, 1); //no change



                        //then set the corner meshes to the right positions, no need to scale them since there are fixed sizes
                        if (TR != null)
                            TR.transform.localPosition = new Vector3(0, blockHeight / 2, -blockWidth / 2 - wallThickness / 2);//move over by half wall thickness
                        if (BR != null)
                            BR.transform.localPosition = new Vector3(0, -blockHeight / 2, -blockWidth / 2 - wallThickness / 2);//move over by half wall thickness
                        if (TL != null)
                            TL.transform.localPosition = new Vector3(0, blockHeight / 2, blockWidth / 2);//no change
                        if (BL != null)
                            BL.transform.localPosition = new Vector3(0, -blockHeight / 2, blockWidth / 2);//no change

                        //scale the center mesh component if there is one
                        if (C != null)
                        {
                            C.transform.localPosition = new Vector3(0, 0, -wallThickness / 4);//move over by quarterthickness
                            C.transform.localScale = new Vector3(1, (blockHeight - 2 * startingHeight) / startingHeight, (blockWidth + wallThickness / 2 - 2 * startingwidth) / startingwidth);//add half wallthickness in z 
                        }
                    }
                    



                }

                
                    

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
    void Awake()
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
        if (isFixedMaterial)
        {
            selectedMat = fixedMatSelected;
            unselectedMat = fixedMat;
        }
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
            float desiredwidth = (blockWidth - HdividerEdgeBuffer) / HdividerStartingWidth;

            if (!DoubleWall)
            {
                if (ConnectedModules[2] != null && ConnectedModules[3] != null)
                {
                    desiredwidth = (blockWidth - HdividerEdgeBuffer + wallThickness) / HdividerStartingWidth;

                }
                else
                {
                    if (ConnectedModules[2] != null || ConnectedModules[3] != null)
                    {
                        desiredwidth = (blockWidth - HdividerEdgeBuffer + wallThickness/2) / HdividerStartingWidth;
                    }
                    
                }
            }
            
            
            for (int i = 0; i < HDividers.Count; i++)
            {

                if (!DoubleWall && (ConnectedModules[2] != null || ConnectedModules[3] != null))
                {
                    if (ConnectedModules[2] != null && ConnectedModules[3] != null)
                    {
                        HDividers[i].transform.localPosition = new Vector3(0, (-blockHeight / 2) + spacing * (i + 1), 0);

                    }
                    else
                    {
                        if (ConnectedModules[2] != null)
                        {
                            HDividers[i].transform.localPosition = new Vector3(0, (-blockHeight / 2) + spacing * (i + 1), -wallThickness / 4);
                        }
                        if (ConnectedModules[3] != null)
                        {
                            HDividers[i].transform.localPosition = new Vector3(0, (-blockHeight / 2) + spacing * (i + 1), wallThickness / 4);
                        }
                    }
                }
                else
                {
                    HDividers[i].transform.localPosition = new Vector3(0, (-blockHeight / 2) + spacing * (i + 1), 0);
                }

                    HDividers[i].transform.localScale = new Vector3(1, 1, desiredwidth);
                

            }

            
            //scale dividers
            for (int i = 0; i < HDividers.Count; i++)
            {
                
                
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
                if (!DoubleWall && ConnectedModules[1] != null)
                {
                    VDividers[i].transform.localPosition = new Vector3(0, -wallThickness/2, (-blockWidth / 2) + spacing * (i + 1));
                }
                else {
                    VDividers[i].transform.localPosition = new Vector3(0, 0, (-blockWidth / 2) + spacing * (i + 1));
                }
                
            }

            float desiredwidth = (blockHeight - VdividerEdgeBuffer) / VdividerStartingWidth;
            if (!DoubleWall && ConnectedModules[1] != null)
            {
            desiredwidth = (blockHeight - VdividerEdgeBuffer + wallThickness) / VdividerStartingWidth;
            }

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

    public float GetMaxX()
    {

        float maxX = -999999 ;
        //chek right side
        if (R.transform.position.x > maxX)
        {
            maxX = R.transform.position.x;
        }

        //chek left side
        if (L.transform.position.x > maxX)
        {
            maxX = L.transform.position.x;
        }

        if (transform.position.x + blockDepth / 2 > maxX)
        {
            maxX = transform.position.x + blockDepth / 2;
        }
        return maxX;

    }

    public float GetMinX()
    {

        float minX = 999999;
        //chek right side
        if (R.transform.position.x < minX)
        {
            minX = R.transform.position.x;
        }

        //chek left side
        if (L.transform.position.x < minX)
        {
            minX = L.transform.position.x;
        }

        if (transform.position.x - blockDepth / 2 < minX)
        {
            minX = transform.position.x - blockDepth / 2;
        }
        return minX;

    }

    public float GetMaxZ()
    {

        float maxZ = -99999;
        //chek right side
        if (R.transform.position.z > maxZ)
        {
            maxZ = R.transform.position.z;
        }

        //chek left side
        if (L.transform.position.z > maxZ)
        {
            maxZ = L.transform.position.z;
        }

        if (transform.position.z + blockDepth / 2 > maxZ)
        {
            maxZ = transform.position.z + blockDepth / 2;
        }
        return maxZ;

    }

    public float GetMinZ()
    {

        float minZ = 9999;
        //chek right side
        if (R.transform.position.z < minZ)
        {
            minZ = R.transform.position.z;
        }

        //chek left side
        if (L.transform.position.z < minZ)
        {
            minZ = L.transform.position.z;
        }

        if (transform.position.z - blockDepth / 2 < minZ)
        {
            minZ = transform.position.z - blockDepth / 2;
        }
        return minZ;

    }

    public float GetStorageVolume()
    {
        float volume = 0;
        if (isCounter)
        {
            return 0;
        }
        if (isCorner)
        {
            volume = (blockWidth * blockDepth * blockHeight) + ((blockWidthB - blockDepth) * blockDepth * blockHeight);
            volume -= GetmaterialVolume();
            return volume;
        }
        else {
            volume = blockWidth * blockDepth * blockHeight;

           
            volume -= GetmaterialVolume();
            return volume;
        }
    }

    public float GetShelfArea()
    {
        float shelfArea = 0;
        if (isCorner)
        {
            shelfArea = (blockWidth * blockDepth) + ((blockWidthB - blockDepth) * blockDepth);

            if (ConnectedModules[0] == null && !isCounter)
            { 
            // no shelf above
            shelfArea += (blockWidth * blockDepth) + ((blockWidthB - blockDepth) * blockDepth);
            }
            return shelfArea;
        }
        else
        {
            shelfArea = blockWidth * blockDepth;
            
            if (allowHorizontalDividers)
            {
                shelfArea *= 1 + HDividercount;
            }

            if (ConnectedModules[0] == null && !isCounter)
            {
                // no shelf above
                shelfArea += blockWidth * blockDepth;
            }

            return shelfArea;
        }

        
    }

    public float GetMaterialArea()
    {
        float area = 0;
        area += blockHeight * blockDepth * HD;
        area += blockWidth * blockDepth * WD;
        area += blockWidth * blockHeight * WH;
        area -= subtracttionArea;
        if (allowHorizontalDividers)
        {
            area += blockWidth * blockDepth * HDividercount;
        }
        if (allowVerticalDividers)
        {
            area += blockHeight * blockDepth * VDividercount;
        }
        if (!DoubleWall)
        {
            if (ConnectedModules[1] != null)
            {
                area -= blockDepth * blockWidth;//block connected below, no need for bottom plate
            }
            if (ConnectedModules[2] != null)
            {
                area -= blockDepth * blockHeight;//block connected beside, remove one side plate
            }
        }
        return area;
    }

    public float GetmaterialVolume()
    {
        float volume = GetMaterialArea() * matThickness;
        return volume;
    }

    public float GetCost()
    {
        //costs per m2
        //Premium Pine Plywood = 91.88/2.9729 = 30.90
        //spruce plywood = 62.98 /2.9729 = 21.18
        //MDF = 56.98/3.066445 = 18.58

        float cost = 0;
        float costperm2 = 0;
        switch (CoreMaterial)
        {
            case (0):
                costperm2 = 30.90f;
                break;
            case (1):
                costperm2 = 21.18f;
                break;
            case (2):
                costperm2 = 18.58f;
                break;
            default:
                break;
        }
        cost = GetMaterialArea() * costperm2;
        return cost;
    }

    public float Getmass()
    {
        //costs per m2
        //Premium Pine Plywood = 91.88/2.9729 = 30.90
        //spruce plywood = 62.98 /2.9729 = 21.18
        //MDF = 56.98/3.066445 = 18.58

        float mass = 0;
        float massPerM3 = 0;
        switch (CoreMaterial)
        {
            case (0):
                massPerM3 = 650;
                break;
            case (1):
                massPerM3 = 650;
                break;
            case (2):
                massPerM3 = 700;
                break;
            default:
                break;
        }
        mass = GetmaterialVolume() * massPerM3;
        return mass;
    }

    public float GetMaxY()
    {
        if (!isCounter)
        {
            return transform.position.y + blockHeight / 2;
        }
        else
        { 
        return transform.position.y - blockHeight / 2 + matThickness;
        }
        
    }

    public FlatShape[] getShapesInside()
    {
        //firs see if there are any shelves

        int shapes = 0;
        if (allowHorizontalDividers)
        {
            shapes += HDividercount;
        }
        else {
            if (allowVerticalDividers)
            {
                shapes += VDividercount;
            }
        }

        switch (moduleType)
        {
            case (0):
                //closed shelf
                shapes += 1;
                break;

            case (1):
                //corner
                break;

            case (2):
                //corner reverse
                break;

            case (3):
                //closed helf H div
                shapes += 1;
                break;

            case (4):
                //closed shelf drawer
                shapes += 6;
                break;

            case (5):
                //open shelf
                break;

            case (6):
                //closed shelf V div
                shapes += 1;
                break;

            case (7):
                //counter
                break;

            case (8):
                //closed swing door
                shapes += 2;
                break;

        }

        FlatShape[] results = new FlatShape[shapes];

        int nextindex = 0;
        switch (moduleType)
        {
            case (0):
                //closed shelf
                results[0] = new FlatShape();
                results[0].depth = blockHeight; //should be smaller if it fits in between the shelves
                results[0].width = blockWidth;//should depend on if doublewalls is active or not
                nextindex++;
                break;

            case (1):
                //corner
                break;

            case (2):
                //corner reverse
                break;

            case (3):
                //closed helf H div
                results[0] = new FlatShape();
                results[0].depth = blockHeight; //should be smaller if it fits in between the shelves
                results[0].width = blockWidth;//should depend on if doublewalls is active or not
                nextindex++;
                break;

            case (4):
                //closed shelf drawer
               
                //0 - back plate
                results[0] = new FlatShape();
                results[0].depth = blockHeight; //should be smaller if it fits in between the shelves
                results[0].width = blockWidth;//should depend on if doublewalls is active or not
                nextindex++;

                //1 -  back of drawer
                results[1] = new FlatShape();
                results[1].depth = blockHeight -  matThickness*2; 
                results[1].width = blockWidth - matThickness * 2;
                nextindex++;

                //2 -  front of drawer
                results[2] = new FlatShape();
                results[2].shapeType = 1;
                results[2].depth = blockHeight - matThickness * 2;
                results[2].width = blockWidth - matThickness * 2;
                nextindex++;

                //3 -  bottom of drawer
                results[3] = new FlatShape();
                results[3].depth = blockDepth - matThickness;
                results[3].width = blockWidth - matThickness * 2;
                nextindex++;

                //4 -  side of drawer
                results[4] = new FlatShape();
                results[4].depth = blockDepth - matThickness;
                results[4].width = blockHeight - matThickness * 2;
                nextindex++;

                //5 -  other side of drawer
                results[5] = new FlatShape();
                results[5].depth = blockDepth - matThickness;
                results[5].width = blockHeight - matThickness * 2;
                nextindex++;

                break;

            case (5):
                //open shelf
                break;

            case (6):
                //closed shelf V div
                results[0] = new FlatShape();
                results[0].depth = blockHeight; //should be smaller if it fits in between the shelves
                results[0].width = blockWidth;//should depend on if doublewalls is active or not
                nextindex++;
                break;

            case (7):
                //counter
                break;

            case (8):
                //closed swing door
                results[0] = new FlatShape();
                results[0].depth = blockHeight; //should be smaller if it fits in between the shelves
                results[0].width = blockWidth;//should depend on if doublewalls is active or not
                nextindex++;

                //the front swing door
                results[1] = new FlatShape();
                results[1].depth = blockHeight -  matThickness; 
                results[1].width = blockWidth - matThickness;
                nextindex++;
                break;

        }


        if (allowHorizontalDividers)
        {
            
            for (int i = 0; i < HDividercount; i++)
            {
                results[nextindex + i] = new FlatShape();
                results[nextindex + i].depth = blockDepth - matThickness; 
                results[nextindex + i].width = blockWidth - matThickness*2; //should depend on if doublewalls is active or not
            }
        }
        else
        {
            if (allowVerticalDividers)
            {
                for (int i = 0; i < VDividercount; i++)
                {
                    results[nextindex + i] = new FlatShape();
                    results[nextindex + i].depth = blockDepth - matThickness; 
                    results[nextindex + i].width = blockHeight - matThickness * 2; //should depend on if doublewalls is active or not
                }
            }
        }

        return results;
    }

    public FlatShape[] getShapesTopBottom(bool removeBottom)
    {
        //does not work with corner peices or counter peices yet

        //returns the top and bottom faces of this module, bottom face is not returned if removebottom is true and there is a module (non counter) connected below
        if (removeBottom && ConnectedModules[1] != null && !ConnectedModules[1].isCounter)
        {
            //only return top face
            FlatShape[] results = new FlatShape[1];
            results[0] = new FlatShape();
            results[0].width = blockWidth - matThickness; //since doublewalls is false, only 1 matthickness is removed
            results[0].depth = blockDepth - matThickness;
            return results;
        }
        else {
            if (removeBottom)
            {
                //return top and bottom face
                FlatShape[] results = new FlatShape[2];
                results[0] = new FlatShape();
                results[0].width = blockWidth - matThickness; //since doublewalls is false, only 1 matthickness is removed
                results[0].depth = blockDepth - matThickness;
                results[1] = new FlatShape();
                results[1].width = blockWidth - matThickness; //since doublewalls is false, only 1 matthickness is removed
                results[1].depth = blockDepth - matThickness;
                return results;
            }
            else {
                //return top and bottom face
                FlatShape[] results = new FlatShape[2];
                results[0] = new FlatShape();
                results[0].width = blockWidth - matThickness *2;
                results[0].depth = blockDepth - matThickness;
                results[1] = new FlatShape();
                results[1].width = blockWidth - matThickness *2;
                results[1].depth = blockDepth - matThickness;
                return results;
            }
            
        }

    }

    public FlatShape[] getShapesSides(bool removeRight, bool joinbellow)
    {
        if (isCounter)
        {
            return null;
        }
        //returns the vertical shapes on either side of the unit, if remove right is true it will not return the right if there is a module connected to the right
        //if joinbelow is true, it will not return any shapes if there is a module connected above, and if there are any connected below it will add their heights to this module

        //first lets get the combined height
        float combinedheight = blockHeight;
        if (joinbellow)
        {
            
            ScalableComponent temp = ConnectedModules[1];
            while (temp != null && !temp.isCounter)
            {
                combinedheight += temp.blockHeight;
                temp = temp.ConnectedModules[1];
            }

            if (ConnectedModules[0] != null)
            {
                //there is a module above this, return nothing
                return null;
            }
        }

        if (removeRight && ConnectedModules[2] != null)
        {
            //only return left side
            FlatShape[] results = new FlatShape[1];
            results[0] = new FlatShape();
            results[0].width = blockDepth;
            results[0].depth = combinedheight;
            return results;
        }
        else {
            //return both sides
            //only return left side
            FlatShape[] results = new FlatShape[2];
            results[0] = new FlatShape();
            results[0].width = blockDepth;
            results[0].depth = combinedheight;
            results[1] = new FlatShape();
            results[1].width = blockDepth;
            results[1].depth = combinedheight;
            return results;
        }
    
    }
    
}

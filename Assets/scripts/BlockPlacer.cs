using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using AsciiFBXExporter;
using SFB;
public class BlockPlacer : MonoBehaviour
{
    public bool useDimensionLimits = true; //is the tool using limits for the modules sizes (can cause glitches or unrealistic models if dissabled)

    public ScalableComponent starterBlock;//reference to the block that already exists in the scene


    [SerializeField] private LayerMask SelecionLayer; //the layermask that is used for selecting modules
    [SerializeField] private LayerMask PlacementLayers; //the layermask that is used when placing new modules
    //logic for selecting and modifying selected blocks
    public ScalableComponent SelectedModule; //this holds a reference to the currently selected module, null if there are non selected

    //references to input fields and UI for modyfiying selection
    public TMP_InputField HeightInput, WidthInput, WidthBInput, ShelfInput;//the input feilds for changing block dimensions
    public GameObject SelectionPannel; //this is the UI pannel with all of the tools for modifying the selection

    //Block Index vertical compatabilities
    // Flat blocks : 0
    // Concave Blocks (open inside):1
    //Convex Blocks (open outside):2
    public int placingBlockIndex = 0;
    public GameObject[] blockPrefab;
    public GameObject[] previewBlockPrefab;
    public Camera camera;
    public int DisplayMode;
    //0-working/show snapping points
    //1 - show finished shelf

    //Drag and drop
    public ModuleUI[] DragableModules;
    public bool Dragingblock = false;

    public List<ScalableComponent> AllBlocks; //a list containing all placed modules
    public GameObject ModulePreview;//the preview of the module being dragged
    public GameObject oldModulePreview; //used for gc


    //For exporting model
    public RuntimeExporterMono Exporter;
    private string _path;

    //material options
    public TMP_Dropdown materialDropdown;
    public Material[] UnselectedMat;
    public Material[] SelectedMat;



    // Start is called before the first frame update
    void Start()
    {
        SelectionPannel.SetActive(false);//turn off the selection pannel to start since there is no block selected
        AllBlocks = new List<ScalableComponent>();//initialize list that will store all placed blocks
        if (starterBlock != null)//add the starter block to the list if there is one
            AllBlocks.Add(starterBlock);
    }


    // Update is called once per frame
    void Update()
    {

        //this is just temporary way of switching bewteen block types, it should be something visible
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            //mouse down
            //there was a click

            if (EventSystem.current.IsPointerOverGameObject())
            {
                //mouse is over UI
                //check to see if the mouse is over any modules
                for (int i = 0; i < DragableModules.Length; i++)
                {
                    if (DragableModules[i].mouseOverItemDropLocation)
                    {
                        placingBlockIndex = DragableModules[i].ModuleType;
                        Dragingblock = true;
                        oldModulePreview = ModulePreview;
                        Destroy(oldModulePreview);

                        ModulePreview = Instantiate(previewBlockPrefab[placingBlockIndex]);
                        ModulePreview.layer = 2;
                    }
                }

                //clicked On the UI
                //if (EventSystem.current.currentSelectedGameObject.GetComponent<ModuleUI>() != null)
                //{
                //    placingBlockIndex = EventSystem.current.currentSelectedGameObject.GetComponent<ModuleUI>().ModuleType;
                //    Dragingblock = true;
                //}
            }
            else
            {
                Dragingblock = false;
                RaycastHit hit2;
                if (Physics.Raycast(ray, out hit2, 1000f, SelecionLayer))
                {
                    Debug.Log(hit2.collider.gameObject.name + " with layer " + hit2.collider.gameObject.layer);
                    //did we hit any component volumes
                    var hitComponent = hit2.collider.gameObject.GetComponentInParent<ScalableComponent>();

                    if (hitComponent != null)
                    {
                        if (SelectedModule != null)
                        {
                            SelectedModule.SetSelected(false);
                        }
                        SelectedModule = hitComponent;
                        SelectedModule.SetSelected(true);
                        UpdateSelectionUI(true);
                    }
                }
                else
                {
                    //clicked but didnt hit UI, block volumes or snapping points
                    if (SelectedModule != null)
                    {
                        SelectedModule.SetSelected(false);
                    }
                    SelectedModule = null;
                    UpdateSelectionUI(false);
                }



            }



        }

        if (Dragingblock)
        {
            //update the visual of the ModulePreview
            //first see if we are in a plausible location for scnapping
            

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, PlacementLayers))
            {
                var HitBlockSnap = hit.collider.gameObject.GetComponent<BlockSnap>(); //stores the blocksnap of the blocksnap that was clicked
                var HitBlockComponent = hit.collider.gameObject.GetComponentInParent<ScalableComponent>(); //stores the scalablecomponent of the block that was clicked on

                
                if (HitBlockSnap != null && HitBlockComponent.ConnectedModules[HitBlockSnap.mySnapIndex] == null)
                {
                    //mouse over snaping point of furniture
                    var PlacedBlockComponent = ModulePreview.GetComponent<ScalableComponent>(); //this holds a local reference to the scalable component on the newly created block so that I dont have to keep using Getcomponent, which is kinda slow

                    PlacedBlockComponent.ConnectedModules = new ScalableComponent[PlacedBlockComponent.snappingPoints.Length];//initialise the list for storring all snapped blocks for the newly created block

                    //HitBlockComponent.ConnectedModules[HitBlockSnap.mySnapIndex] = PlacedBlockComponent; //sets the reference on the clicked block to the new block at the correct index in its connectedmodules array
                    PlacedBlockComponent.ConnectedModules[HitBlockSnap.targetsnapIndex] = HitBlockComponent; //sets the reference on the new block to the clicked bloxk at the correct index in its connectedmodules array




                    //we should probably also check if the newly placed block is adjacent to any other blocks, in which case they should probably also be connected

                    var leftBlock = HitBlockComponent.ConnectedModules[3];//the block to the left of the block that we are connecting to
                    var rightBlock = HitBlockComponent.ConnectedModules[2];//the block to the right of the block we are connecting to
                    var topBlock = HitBlockComponent.ConnectedModules[0];//the block to the top of the block that we are connecting to
                    var bottomBlock = HitBlockComponent.ConnectedModules[1];//the block to the bottom of the block we are connecting to

                    switch (HitBlockSnap.mySnapIndex)
                    {
                        case 0:
                            //top side
                            PlacedBlockComponent.blockWidth = HitBlockComponent.blockWidth;
                            if (leftBlock != null)//there is a block to the lower left
                            {
                                leftBlock = leftBlock.ConnectedModules[0];//left block is not the block that is directly left of the newly placed block

                                if (leftBlock != null)
                                {

                                    PlacedBlockComponent.blockHeight = leftBlock.blockHeight;
                                }

                            }
                            //now do the same thing for the right side
                            if (rightBlock != null)
                            {
                                rightBlock = rightBlock.ConnectedModules[0];

                                if (rightBlock != null)
                                {

                                    PlacedBlockComponent.blockHeight = rightBlock.blockHeight;

                                }

                            }
                            break;

                        case 1:
                            //bottom side
                            PlacedBlockComponent.blockWidth = HitBlockComponent.blockWidth;
                            if (leftBlock != null)//there is a block to the upper left
                            {
                                leftBlock = leftBlock.ConnectedModules[1];//left block is not the block that is directly left of the newly placed block
                                if (leftBlock != null)
                                {

                                    PlacedBlockComponent.blockHeight = leftBlock.blockHeight;
                                }
                            }
                            //now do the same thing for the right side
                            if (rightBlock != null)
                            {
                                rightBlock = rightBlock.ConnectedModules[1];
                                if (rightBlock != null)
                                {

                                    PlacedBlockComponent.blockHeight = rightBlock.blockHeight;

                                }
                            }
                            break;

                        case 2:
                            //right side
                            PlacedBlockComponent.blockHeight = HitBlockComponent.blockHeight;
                            //start with above
                            if (topBlock != null)//there is a block to the upper right of the newly placed block
                            {
                                topBlock = topBlock.ConnectedModules[2];

                                if (topBlock != null)
                                {

                                    PlacedBlockComponent.blockWidth = topBlock.blockWidth;
                                }

                            }
                            //now do bottom
                            if (bottomBlock != null)//there is a block to the lower right of the newly placed block
                            {
                                bottomBlock = bottomBlock.ConnectedModules[2];

                                if (bottomBlock != null)
                                {

                                    PlacedBlockComponent.blockWidth = bottomBlock.blockWidth;
                                }

                            }
                            break;

                        case 3:
                            //left side
                            PlacedBlockComponent.blockHeight = HitBlockComponent.blockHeight;
                            //start with above
                            if (topBlock != null)//there is a block to the upper left of the newly placed block
                            {
                                topBlock = topBlock.ConnectedModules[3];
                                if (topBlock != null)
                                {

                                    PlacedBlockComponent.blockWidth = topBlock.blockWidth;
                                }
                            }
                            //now do bottom
                            if (bottomBlock != null)//there is a block to the lower left of the newly placed block
                            {
                                bottomBlock = bottomBlock.ConnectedModules[3];
                                if (bottomBlock != null)
                                {

                                    PlacedBlockComponent.blockWidth = bottomBlock.blockWidth;
                                }
                            }
                            break;

                    }

                    PlacedBlockComponent.recalculateDimentions(false);
                    //recalculate the dimentions for the newly placed block
                    PlacedBlockComponent.SetPositionAndRotation(HitBlockSnap.snapPos, HitBlockSnap.targetsnapIndex);
                    //set the position of the newly created block
                }
                else {
                    if(AllBlocks.Count == 0)
                    {
                        //there are no blocks in the scene, let them place one on the ground
                        ModulePreview.transform.position = hit.point + new Vector3(0,ModulePreview.GetComponent<ScalableComponent>().blockHeight/2,0);
                    }else
                    {
                        DragPreviewToMouse();
                    }

                    
                }
            }
            else {
                //not in a snapping location
                DragPreviewToMouse();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            //mouse was released
            if (Dragingblock)
            {
                Dragingblock = false;
                oldModulePreview = ModulePreview;
                Destroy(oldModulePreview);

                PlaceBlockAtMouse();
            }
        }
    }

    void PlaceBlockAtMouse()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, PlacementLayers))
        {
            if (AllBlocks.Count == 0)
            {
                //there are no blocks in the scene, place this one at mouse position : hit.point + new Vector3(0,ModulePreview.GetComponent<ScalableComponent>().blockHeight/2,0);
                GameObject PlacedBlock = Instantiate(blockPrefab[placingBlockIndex], hit.point + new Vector3(0, ModulePreview.GetComponent<ScalableComponent>().blockHeight / 2, 0), Quaternion.identity);//create the new block
                var PlacedBlockComponent = PlacedBlock.GetComponent<ScalableComponent>(); //this holds a local reference to the scalable component on the newly created block so that I dont have to keep using Getcomponent, which is kinda slow
                AllBlocks.Add(PlacedBlockComponent);
                PlacedBlockComponent.ConnectedModules = new ScalableComponent[PlacedBlockComponent.snappingPoints.Length];//initialise the list for storring all snapped blocks for the newly created block
                PlacedBlockComponent.recalculateDimentions(true);
            }
            else
            {
                var HitBlockSnap = hit.collider.gameObject.GetComponent<BlockSnap>(); //stores the blocksnap of the blocksnap that was clicked
                var HitBlockComponent = hit.collider.gameObject.GetComponentInParent<ScalableComponent>(); //stores the scalablecomponent of the block that was clicked on


                if (HitBlockSnap != null && HitBlockComponent.ConnectedModules[HitBlockSnap.mySnapIndex] == null)
                {
                    //mouse over snaping point of furniture
                    GameObject PlacedBlock = Instantiate(blockPrefab[placingBlockIndex], HitBlockSnap.snapPos.position, HitBlockSnap.snapPos.rotation);//create the new block
                    var PlacedBlockComponent = PlacedBlock.GetComponent<ScalableComponent>(); //this holds a local reference to the scalable component on the newly created block so that I dont have to keep using Getcomponent, which is kinda slow
                    AllBlocks.Add(PlacedBlockComponent);
                    PlacedBlockComponent.ConnectedModules = new ScalableComponent[PlacedBlockComponent.snappingPoints.Length];//initialise the list for storring all snapped blocks for the newly created block

                    HitBlockComponent.ConnectedModules[HitBlockSnap.mySnapIndex] = PlacedBlockComponent; //sets the reference on the clicked block to the new block at the correct index in its connectedmodules array
                    PlacedBlockComponent.ConnectedModules[HitBlockSnap.targetsnapIndex] = HitBlockComponent; //sets the reference on the new clock to the clicked bloxk at the correct index in its connectedmodules array




                    //we should probably also check if the newly placed block is adjacent to any other blocks, in which case they should probably also be connected

                    var leftBlock = HitBlockComponent.ConnectedModules[3];//the block to the left of the block that we are connecting to
                    var rightBlock = HitBlockComponent.ConnectedModules[2];//the block to the right of the block we are connecting to
                    var topBlock = HitBlockComponent.ConnectedModules[0];//the block to the top of the block that we are connecting to
                    var bottomBlock = HitBlockComponent.ConnectedModules[1];//the block to the bottom of the block we are connecting to

                    switch (HitBlockSnap.mySnapIndex)
                    {
                        case 0:
                            //top side
                            PlacedBlockComponent.blockWidth = HitBlockComponent.blockWidth;
                            PlacedBlockComponent.blockWidthB = HitBlockComponent.blockWidthB;
                            if (leftBlock != null)//there is a block to the lower left
                            {
                                leftBlock = leftBlock.ConnectedModules[0];//left block is not the block that is directly left of the newly placed block

                                if (leftBlock != null)
                                {
                                    PlacedBlockComponent.ConnectedModules[3] = leftBlock;//null if there is no block to the left
                                    leftBlock.ConnectedModules[2] = PlacedBlockComponent;
                                    PlacedBlockComponent.blockHeight = leftBlock.blockHeight;

                                    leftBlock = leftBlock.ConnectedModules[0];
                                    if (leftBlock != null)
                                    {
                                        leftBlock = leftBlock.ConnectedModules[2];
                                        if (leftBlock != null)
                                        {
                                            PlacedBlockComponent.ConnectedModules[0] = leftBlock;//null if there is no block to the left
                                            leftBlock.ConnectedModules[1] = PlacedBlockComponent;
                                            PlacedBlockComponent.blockWidth = leftBlock.blockWidth;
                                            PlacedBlockComponent.blockWidthB = leftBlock.blockWidthB;
                                        }
                                    }
                                }

                            }
                            //now do the same thing for the right side
                            if (rightBlock != null)
                            {
                                rightBlock = rightBlock.ConnectedModules[0];

                                if (rightBlock != null)
                                {
                                    PlacedBlockComponent.ConnectedModules[2] = rightBlock;
                                    rightBlock.ConnectedModules[3] = PlacedBlockComponent;
                                    PlacedBlockComponent.blockHeight = rightBlock.blockHeight;


                                    rightBlock = rightBlock.ConnectedModules[0];
                                    if (rightBlock != null)
                                    {
                                        rightBlock = rightBlock.ConnectedModules[3];
                                        if (rightBlock != null)
                                        {
                                            PlacedBlockComponent.ConnectedModules[0] = rightBlock;//null if there is no block to the left
                                            rightBlock.ConnectedModules[1] = PlacedBlockComponent;
                                            PlacedBlockComponent.blockWidth = rightBlock.blockWidth;
                                            PlacedBlockComponent.blockWidthB = rightBlock.blockWidthB;
                                        }
                                    }
                                }

                            }
                            break;

                        case 1:
                            //bottom side
                            PlacedBlockComponent.blockWidth = HitBlockComponent.blockWidth;
                            PlacedBlockComponent.blockWidthB = HitBlockComponent.blockWidthB;
                            if (leftBlock != null)//there is a block to the upper left
                            {
                                leftBlock = leftBlock.ConnectedModules[1];//left block is not the block that is directly left of the newly placed block
                                if (leftBlock != null)
                                {
                                    PlacedBlockComponent.ConnectedModules[3] = leftBlock;//null if there is no block to the left
                                    leftBlock.ConnectedModules[2] = PlacedBlockComponent;
                                    PlacedBlockComponent.blockHeight = leftBlock.blockHeight;

                                    leftBlock = leftBlock.ConnectedModules[1];
                                    if (leftBlock != null)
                                    {
                                        leftBlock = leftBlock.ConnectedModules[2];
                                        if (leftBlock != null)
                                        {
                                            PlacedBlockComponent.ConnectedModules[1] = leftBlock;//null if there is no block to the left
                                            leftBlock.ConnectedModules[0] = PlacedBlockComponent;
                                            PlacedBlockComponent.blockWidth = leftBlock.blockWidth;
                                            PlacedBlockComponent.blockWidthB = leftBlock.blockWidthB;
                                        }
                                    }
                                }
                            }
                            //now do the same thing for the right side
                            if (rightBlock != null)
                            {
                                rightBlock = rightBlock.ConnectedModules[1];
                                if (rightBlock != null)
                                {
                                    PlacedBlockComponent.ConnectedModules[2] = rightBlock;
                                    rightBlock.ConnectedModules[3] = PlacedBlockComponent;
                                    PlacedBlockComponent.blockHeight = rightBlock.blockHeight;

                                    rightBlock = rightBlock.ConnectedModules[1];
                                    if (rightBlock != null)
                                    {
                                        rightBlock = rightBlock.ConnectedModules[3];
                                        if (rightBlock != null)
                                        {
                                            PlacedBlockComponent.ConnectedModules[1] = rightBlock;//null if there is no block to the left
                                            rightBlock.ConnectedModules[0] = PlacedBlockComponent;
                                            PlacedBlockComponent.blockWidth = rightBlock.blockWidth;
                                            PlacedBlockComponent.blockWidthB = rightBlock.blockWidthB;
                                        }
                                    }

                                }
                            }
                            break;

                        case 2:
                            //right side
                            PlacedBlockComponent.blockHeight = HitBlockComponent.blockHeight;
                            //start with above
                            if (topBlock != null)//there is a block to the upper right of the newly placed block
                            {
                                topBlock = topBlock.ConnectedModules[2];

                                if (topBlock != null)
                                {
                                    PlacedBlockComponent.ConnectedModules[0] = topBlock;
                                    topBlock.ConnectedModules[1] = PlacedBlockComponent;
                                    PlacedBlockComponent.blockWidth = topBlock.blockWidth;
                                    PlacedBlockComponent.blockWidthB = topBlock.blockWidthB;

                                    topBlock = topBlock.ConnectedModules[2];
                                    if (topBlock != null)
                                    {
                                        topBlock = topBlock.ConnectedModules[1];
                                        if (topBlock != null)
                                        {
                                            PlacedBlockComponent.ConnectedModules[2] = topBlock;//null if there is no block to the left
                                            topBlock.ConnectedModules[3] = PlacedBlockComponent;
                                            PlacedBlockComponent.blockHeight = topBlock.blockHeight;
                                        }
                                    }
                                }

                            }
                            //now do bottom
                            if (bottomBlock != null)//there is a block to the lower right of the newly placed block
                            {
                                bottomBlock = bottomBlock.ConnectedModules[2];

                                if (bottomBlock != null)
                                {
                                    PlacedBlockComponent.ConnectedModules[1] = bottomBlock;
                                    bottomBlock.ConnectedModules[0] = PlacedBlockComponent;
                                    PlacedBlockComponent.blockWidth = bottomBlock.blockWidth;
                                    PlacedBlockComponent.blockWidthB = bottomBlock.blockWidthB;

                                    bottomBlock = bottomBlock.ConnectedModules[2];
                                    if (bottomBlock != null)
                                    {
                                        bottomBlock = bottomBlock.ConnectedModules[0];
                                        if (bottomBlock != null)
                                        {
                                            PlacedBlockComponent.ConnectedModules[2] = bottomBlock;//null if there is no block to the left
                                            bottomBlock.ConnectedModules[3] = PlacedBlockComponent;
                                            PlacedBlockComponent.blockHeight = bottomBlock.blockHeight;
                                        }
                                    }
                                }

                            }
                            break;

                        case 3:
                            //left side
                            PlacedBlockComponent.blockHeight = HitBlockComponent.blockHeight;
                            //start with above
                            if (topBlock != null)//there is a block to the upper left of the newly placed block
                            {
                                topBlock = topBlock.ConnectedModules[3];
                                if (topBlock != null)
                                {
                                    PlacedBlockComponent.ConnectedModules[0] = topBlock;
                                    topBlock.ConnectedModules[1] = PlacedBlockComponent;
                                    PlacedBlockComponent.blockWidth = topBlock.blockWidth;
                                    PlacedBlockComponent.blockWidthB = topBlock.blockWidthB;

                                    topBlock = topBlock.ConnectedModules[3];
                                    if (topBlock != null)
                                    {
                                        topBlock = topBlock.ConnectedModules[1];
                                        if (topBlock != null)
                                        {
                                            PlacedBlockComponent.ConnectedModules[3] = topBlock;//null if there is no block to the left
                                            topBlock.ConnectedModules[2] = PlacedBlockComponent;
                                            PlacedBlockComponent.blockHeight = topBlock.blockHeight;
                                        }
                                    }
                                }
                            }
                            //now do bottom
                            if (bottomBlock != null)//there is a block to the lower left of the newly placed block
                            {
                                bottomBlock = bottomBlock.ConnectedModules[3];
                                if (bottomBlock != null)
                                {
                                    PlacedBlockComponent.ConnectedModules[1] = bottomBlock;
                                    bottomBlock.ConnectedModules[0] = PlacedBlockComponent;
                                    PlacedBlockComponent.blockWidth = bottomBlock.blockWidth;
                                    PlacedBlockComponent.blockWidthB = bottomBlock.blockWidthB;

                                    bottomBlock = bottomBlock.ConnectedModules[3];
                                    if (bottomBlock != null)
                                    {
                                        bottomBlock = bottomBlock.ConnectedModules[0];
                                        if (bottomBlock != null)
                                        {
                                            PlacedBlockComponent.ConnectedModules[3] = bottomBlock;//null if there is no block to the left
                                            bottomBlock.ConnectedModules[2] = PlacedBlockComponent;
                                            PlacedBlockComponent.blockHeight = bottomBlock.blockHeight;
                                        }
                                    }
                                }
                            }
                            break;

                    }

                    PlacedBlockComponent.recalculateDimentions(true);
                    //recalculate the dimentions for the newly placed block
                    PlacedBlockComponent.SetPositionAndRotation(HitBlockSnap.snapPos, HitBlockSnap.targetsnapIndex);
                    //set the position of the newly created block

                    if (SelectedModule != null)
                    {
                        SelectedModule.SetSelected(false);
                    }

                    SelectedModule = PlacedBlockComponent;
                    SelectedModule.SetSelected(true);
                    UpdateSelectionUI(true);
                    ApplyUpdatesToSelection();


                }
            }

           
        }
    }

    void DragPreviewToMouse()
    {
        Plane m_Plane;



        m_Plane = new Plane(camera.transform.forward, Vector3.zero);

        //Create a ray from the Mouse click position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Initialise the enter variable
        float enter = 0.0f;

        if (m_Plane.Raycast(ray, out enter))
        {
            //Get the point that is clicked
            Vector3 hitPoint = ray.GetPoint(enter);

            //Move your cube GameObject to the point where you clicked
            ModulePreview.transform.position = hitPoint;
        }
    }
    public void RepositionFurnitureOnGround()
    {
        float currentBottomY = 10000;//will store the lowest current point on the furniture
        foreach (var item in AllBlocks)
        {
            if (item != null)
            {
                if (item.GetBottomY() < currentBottomY)
                {
                    currentBottomY = item.GetBottomY();
                }
            }
        }

        float RequiredYchange = -currentBottomY;

        foreach (var item in AllBlocks)
        {
            if (item != null)
            {
                item.transform.position += new Vector3(0, RequiredYchange, 0);
            }
        }
    }

    public void ApplyUpdatesToSelection()
    {
        if (SelectedModule == null)
        {
            
            SelectionPannel.SetActive(false);
            return;//there is no module selected so we cant update it
        }


        SelectedModule.blockWidth = float.Parse(WidthInput.text);
        SelectedModule.blockWidthB = float.Parse(WidthBInput.text);
        SelectedModule.blockHeight = float.Parse(HeightInput.text);
        SelectedModule.startChange = true;
        SelectedModule.wasChanged = true;
        if (useDimensionLimits)
        {
            if (SelectedModule.CheckDimentionsMinMax())
            {
                //a dimension limit min or max was broken, update UI to reflect this
                UpdateSelectionUI(true);
            }
        }

        if (SelectedModule.allowHorizontalDividers)
        {
            SelectedModule.HDividercount = int.Parse(ShelfInput.text);
        }

        if (SelectedModule.allowVerticalDividers)
        {
            SelectedModule.VDividercount = int.Parse(ShelfInput.text);
        }
        SelectedModule.recalculateDimentions(true);

        RepositionFurnitureOnGround();

        //should add a check here to avoid an out of bounds error
        SelectedModule.materialID = materialDropdown.value;
        SelectedModule.selectedMat = SelectedMat[materialDropdown.value];
        SelectedModule.unselectedMat = UnselectedMat[materialDropdown.value];
        SelectedModule.SetSelected(true);
    }

    public void ApplyMaterialToAll()
    {
        foreach (var item in AllBlocks)
        {
            item.materialID = materialDropdown.value;
            item.selectedMat = SelectedMat[materialDropdown.value];
            item.unselectedMat = UnselectedMat[materialDropdown.value];
            item.SetSelected(false);
            
        }
        SelectedModule.SetSelected(true);
    }
    public void DeleteSelection()
    {
        if (SelectedModule == null)
        {
           
            return;//there is no module selected to delete
        }
        
        AllBlocks.Remove(SelectedModule);
        //clear all references that other blocks have to this block
        for (int i = 0; i < SelectedModule.ConnectedModules.Length; i++)
        {
            if (SelectedModule.ConnectedModules[i] != null)
            {
                //we are connected to this module
                for (int ii = 0; ii < SelectedModule.ConnectedModules[i].ConnectedModules.Length; ii++)
                {
                    if (SelectedModule.ConnectedModules[i].ConnectedModules[ii] != null)
                    {
                        if (SelectedModule.ConnectedModules[i].ConnectedModules[ii].Equals(SelectedModule))
                        {
                            //found the spot where we are connected
                            SelectedModule.ConnectedModules[i].ConnectedModules[ii] = null;
                            break;
                        }
                    }

                }

            }
        }

        Destroy(SelectedModule.gameObject);
        UpdateSelectionUI(false);

        RepositionFurnitureOnGround();

        
    }

    public void UpdateSelectionUI(bool Show)
    {
        if (Show)
        {
            SelectionPannel.SetActive(true);
            HeightInput.text = SelectedModule.blockHeight + "";
            WidthInput.text = SelectedModule.blockWidth + "";
            WidthBInput.text = SelectedModule.blockWidthB + "";
            materialDropdown.value = SelectedModule.materialID;

            if (SelectedModule.allowHorizontalDividers)
            {
                ShelfInput.text = SelectedModule.HDividercount + "";
            }

            if (SelectedModule.allowVerticalDividers)
            {
                ShelfInput.text = SelectedModule.VDividercount + "";
            }
        }
        else {
            SelectionPannel.SetActive(false);
        }
        
    }

    public void Exportmodel()
    {
        //get file location and file name
        _path = StandaloneFileBrowser.SaveFilePanel("Export File", "", "Custom_Furniture", ".fbx");
        Exporter.Fullpath = _path;
        for (int i = 0; i < AllBlocks.Count; i++)
        {
            AllBlocks[i].transform.SetParent(this.transform);
        }
        Exporter.rootObjectToExport = this.gameObject;
        Exporter.ExportGameObject();
    }

    
}

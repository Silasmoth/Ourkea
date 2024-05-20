using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using AsciiFBXExporter;
using SFB;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
//using UnityEngine.Windows;

public class BlockPlacer : MonoBehaviour
{
    public bool ViewOnly = false;
    //this to dissable in view only mode
    public GameObject[] HideInViewMode;

    public bool awaitingServer = false;//is the client waiting for a response from the server? if so just wait
    public bool awaitingpopup = false;

    public bool ShowBackWall = false;//used to determine if back wall should be shown or not
    public TMP_Text BackwallButton; //the text on the toggle back wall button
    public GameObject BackWall; //used for previewing furniture with a back wall

    const int BYTE_SIZE = 8000;//used for saving, means max blocks around 100
    public ScrollRect moduleScroll;//used to allow/prevent scrolling while dragging
    public bool doubleWalls; //if true keep double walls in between modules, if false make them single walls
    public TMP_Text doubleWallsButton; // the text on the toggle double walls button
    
    public GameObject ScaleFigure;
    public bool showFigure = false;
    public TMP_Text ShowFigureButton; // the text on the "show/hide scale figure" button

    public ViewportMover viewport;// reference to the viewport mover, used to get the camera rotation when placing the dimention lines

    public bool showstats = false; //show the millwork stats or not
    public TMP_Text showStatsButton; // the text on the "show/hide stats" button
    public TMP_Text StatisticsText; // the text for showing stats
    
    public bool ShowDim = true; //show the dimension lines or not
    public TMP_Text showdimButton; // the text on the "show/hide dimentions" button


    public DimentionDisplay VerticalDimDisplay; //the dimdiesplay for Y
    public DimentionDisplay XlDimDisplay; // dimentiondisplay for X
    public DimentionDisplay ZlDimDisplay; // dimentionDisplay for z


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
    public TMP_Dropdown SurfaceMaterialDropdown;
    public TMP_Dropdown CoreMaterialDropDown;
    public Material[] UnselectedMat;
    public Material[] SelectedMat;

    SceneMem sceneMem;

    //furniture bounds storage
    float maxX, minX, maxY, minY, maxZ, minZ;

    //for creating just the shapes
    public GameObject shapeSimple;


    //for uploading the model to the server
    string email;
    public GameObject uploadePannel; //the UI pannel with all of the uploading UI
    public NetworkClient client;
    public TMP_InputField uploadProjectName;
    public TMP_InputField uploadClientName;
    public TMP_InputField uploadClientEmail;
    public TMP_InputField uploadClientAddress;

    //for poopups
    public GameObject PopUpPannel;
    public TextMeshProUGUI PopupText;



    // Start is called before the first frame update
    void Start()
    {
        closePopup();

        SelectionPannel.SetActive(false);//turn off the selection pannel to start since there is no block selected
        AllBlocks = new List<ScalableComponent>();//initialize list that will store all placed blocks
        if (starterBlock != null)//add the starter block to the list if there is one
            AllBlocks.Add(starterBlock);

        refreshToggles();//refresh all of the toggle buttons so that things are correctly shown/hidden and the buttons have correct text
        UpdateMaxMins();
        OpenCloseUploadPannel(false);//close the upload pannel
        //see if there is a scene memory in which case find it then load correct project (also get reference to the client now while we're at it)
        try
        {
            sceneMem = GameObject.Find("SceneMemory").GetComponent<SceneMem>();

            client = sceneMem.GetComponent<NetworkClient>();
            client.blockPlacer = this;
            if (sceneMem.sceneType >= 0)
            {
                LoadExampleModel(sceneMem.sceneType);
            }
            
            if (sceneMem.sceneType == -2)
            {
                OpenModelFromFile(sceneMem.openModelPath);
            }

            if (sceneMem.sceneType == -1)
            {
                ToggleBackWall(sceneMem.wallmounted);
            }
            

        }
        catch (System.Exception)
        {
            
            
        }

        try
        {
            BuilderProjects viewer = GameObject.Find("NetClient").GetComponent<BuilderProjects>();
            
            OpenModel( BytesToModel(viewer.projectToLoad));

            for (int i = 0; i < HideInViewMode.Length; i++)
            {
                HideInViewMode[i].SetActive(false);
            }
            ViewOnly = true;

        }
        catch (System.Exception)
        {
            
           
        }



    }


    // Update is called once per frame
    void Update()
    {
        if (awaitingServer || awaitingpopup)
        {
            return;

        }
        //this is just temporary way of switching bewteen block types, it should be something visible
        Ray ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);

        if (!ViewOnly)
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
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
                moduleScroll.enabled = false;
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

                        bool allowPlacement = true;
                        //check to see if the snapping point is compatible with the block we are dragging in
                        if (HitBlockSnap.mySnapIndex == 0 || HitBlockSnap.mySnapIndex == 1)
                        {
                            //we are snapping to the top or bottom of a block, make sure we only are placing a corner on corner
                            if ((HitBlockComponent.isCorner == PlacedBlockComponent.isCorner) && (HitBlockComponent.reverseCorner == PlacedBlockComponent.reverseCorner))
                            {
                                //block type matches


                            }
                            else
                            {
                                //block type missmatch
                                allowPlacement = false;

                            }
                        }

                        if (allowPlacement)
                        {
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
                        else
                        {
                            DragPreviewToMouse();
                            if (ModulePreview.transform.position.y - ModulePreview.GetComponent<ScalableComponent>().blockHeight / 2 <= 0)
                            {
                                ModulePreview.transform.position = hit.point + new Vector3(0, ModulePreview.GetComponent<ScalableComponent>().blockHeight / 2, 0);
                            }
                        }



                    }
                    else
                    {
                        if (AllBlocks.Count == 0)
                        {
                            //there are no blocks in the scene, let them place one on the ground
                            if (ShowBackWall)
                            {
                                ModulePreview.transform.position = new Vector3(-ModulePreview.GetComponent<ScalableComponent>().blockDepth / 2, Mathf.Clamp(hit.point.y, ModulePreview.GetComponent<ScalableComponent>().blockHeight / 2, 100), hit.point.z);
                            }
                            else
                            {
                                ModulePreview.transform.position = new Vector3(hit.point.x, Mathf.Clamp(hit.point.y, ModulePreview.GetComponent<ScalableComponent>().blockHeight / 2, 100), hit.point.z);
                            }


                        }
                        else
                        {

                            if (ShowBackWall)
                            {
                                ModulePreview.transform.position = new Vector3(Mathf.Clamp(hit.point.x, -99, GetMaxX() - ModulePreview.GetComponent<ScalableComponent>().blockDepth / 2), Mathf.Clamp(hit.point.y, ModulePreview.GetComponent<ScalableComponent>().blockHeight / 2, 100), hit.point.z);
                            }
                            else
                            {
                                DragPreviewToMouse();
                                if (ModulePreview.transform.position.y - ModulePreview.GetComponent<ScalableComponent>().blockHeight / 2 <= 0)
                                {
                                    ModulePreview.transform.position = hit.point + new Vector3(0, ModulePreview.GetComponent<ScalableComponent>().blockHeight / 2, 0);
                                }
                            }



                        }


                    }
                }
                else
                {
                    //not in a snapping location
                    DragPreviewToMouse();
                }
            }
            else
            {
                moduleScroll.enabled = true;
            }


            if (UnityEngine.Input.GetMouseButtonUp(0))
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
       

        if (ShowDim)
        {
            float maxX = GetMaxX();
            float minX = GetMinX();
            float maxY = GetMaxY();
            float maxZ = GetMaxZ();
            float minZ = GetMinZ();

            if (viewport.clampedXRot >= 90 && viewport.clampedXRot <= 180)
            {
                VerticalDimDisplay.Coord1 = new Vector3(maxX, 0, maxZ);
                VerticalDimDisplay.Coord2 = new Vector3(maxX, maxY, maxZ);
                VerticalDimDisplay.DisplayOffset = new Vector3(0.1f, 0, 0.1f);
            }
            else {

                if (viewport.clampedXRot >= 180 && viewport.clampedXRot < 270)
                {
                    VerticalDimDisplay.Coord1 = new Vector3(maxX, 0, minZ);
                    VerticalDimDisplay.Coord2 = new Vector3(maxX, maxY, minZ);
                    VerticalDimDisplay.DisplayOffset = new Vector3(0.1f, 0, -0.1f);
                }
                else
                {
                    if (viewport.clampedXRot >= 270 && viewport.clampedXRot < 360)
                    {
                        VerticalDimDisplay.Coord1 = new Vector3(minX, 0, minZ);
                        VerticalDimDisplay.Coord2 = new Vector3(minX, maxY, minZ);
                        VerticalDimDisplay.DisplayOffset = new Vector3(-0.1f, 0, -0.1f);
                    }
                    else
                    {
                        VerticalDimDisplay.Coord1 = new Vector3(minX, 0, maxZ);
                        VerticalDimDisplay.Coord2 = new Vector3(minX, maxY, maxZ);
                        VerticalDimDisplay.DisplayOffset = new Vector3(-0.1f, 0, 0.1f);

                    }

                }
            }
            

            XlDimDisplay.Coord1 = new Vector3(minX, 0, maxZ);
            XlDimDisplay.Coord2 = new Vector3(maxX, 0, maxZ);
            XlDimDisplay.DisplayOffset = new Vector3(0, 0, 0.1f);


            if (viewport.clampedXRot >= 0 && viewport.clampedXRot <= 180)
            {
                ZlDimDisplay.Coord1 = new Vector3(minX, 0, minZ);
                ZlDimDisplay.Coord2 = new Vector3(minX, 0, maxZ);
                ZlDimDisplay.DisplayOffset = new Vector3(-0.1f, 0, 0);
            }
            else {
                ZlDimDisplay.Coord1 = new Vector3(maxX, 0, minZ);
                ZlDimDisplay.Coord2 = new Vector3(maxX, 0, maxZ);
                ZlDimDisplay.DisplayOffset = new Vector3(0.1f, 0, 0);
            }

            if ((viewport.clampedXRot >= 90 && viewport.clampedXRot <= 180) || (viewport.clampedXRot >= 270 && viewport.clampedXRot <= 360))
            {
                XlDimDisplay.textoffset = 2;
                ZlDimDisplay.textoffset = 0;
            }
            else {
                XlDimDisplay.textoffset = 0;
                ZlDimDisplay.textoffset = 2;
            }
            


            if (viewport.clampedXRot >= 90 && viewport.clampedXRot <= 270)
            {
                XlDimDisplay.Coord1 = new Vector3(minX, 0, maxZ);
                XlDimDisplay.Coord2 = new Vector3(maxX, 0, maxZ);
                XlDimDisplay.DisplayOffset = new Vector3(0, 0, 0.1f);
            }
            else
            {
                XlDimDisplay.Coord1 = new Vector3(minX, 0, minZ);
                XlDimDisplay.Coord2 = new Vector3(maxX, 0, minZ);
                XlDimDisplay.DisplayOffset = new Vector3(0, 0, -0.1f);
            }

            VerticalDimDisplay.UpdateCoords();
            XlDimDisplay.UpdateCoords();
            ZlDimDisplay.UpdateCoords();
        }

        if (showstats)
        {
            StatisticsText.text = "Storage Volume: " + Mathf.Round(GetTotalStorageVolume() * 1000f)/1000 + "m^3 \nShelf Area: " + Mathf.Round(GetTotalShelfArea() * 1000f) / 1000 + "m^2 \nSheet Material Area: " + Mathf.Round(GetTotalMaterialArea() * 1000f) / 1000 + "m^2 \nFurniture Mass: " + Mathf.Round(GetTotalMaterialVolume() * 1000f) / 1000 * 600 + "kg \nCost: $" + Mathf.Round(GetTotalCost() * 100f) / 100 + "CAD";
        }
        else {
            StatisticsText.text = "";
        }

        if (showFigure)
        {
            
            UpdateFigurePos();
        }

    
    }

    public void refreshToggles()
    {
        //update button
        if (ShowDim)
        {
            VerticalDimDisplay.gameObject.SetActive(true);
            XlDimDisplay.gameObject.SetActive(true);
            ZlDimDisplay.gameObject.SetActive(true);
            showdimButton.text = "Hide Dimensions";

            VerticalDimDisplay.Coord1 = new Vector3(GetMaxX(), 0, GetMaxZ());
            VerticalDimDisplay.Coord2 = new Vector3(GetMaxX(), GetMaxY(), GetMaxZ());
            VerticalDimDisplay.DisplayOffset = new Vector3(0.1f, 0, 0.1f);

            XlDimDisplay.Coord1 = new Vector3(GetMinX(), 0, GetMaxZ());
            XlDimDisplay.Coord2 = new Vector3(GetMaxX(), 0, GetMaxZ());
            XlDimDisplay.DisplayOffset = new Vector3(0, 0, 0.1f);

            ZlDimDisplay.Coord1 = new Vector3(GetMinX(), 0, GetMinZ());
            ZlDimDisplay.Coord2 = new Vector3(GetMinX(), 0, GetMaxZ());
            ZlDimDisplay.DisplayOffset = new Vector3(-0.1f, 0, 0);

            VerticalDimDisplay.UpdateCoords();
            XlDimDisplay.UpdateCoords();
            ZlDimDisplay.UpdateCoords();
        }
        else
        {
            VerticalDimDisplay.gameObject.SetActive(false);
            XlDimDisplay.gameObject.SetActive(false);
            ZlDimDisplay.gameObject.SetActive(false);
            showdimButton.text = "Show Dimensions";
        }

        if (showstats)
        {
            showStatsButton.text = "Hide Statistics";
        }
        else
        {
            showStatsButton.text = "Show Statistics";
        }

        ScaleFigure.SetActive(showFigure);
        if (showFigure)
        {
            ShowFigureButton.text = "Hide Scale Figure";
        }
        else
        {
            ShowFigureButton.text = "Show Scale Figure";
        }

        foreach (var item in AllBlocks)
        {
            item.DoubleWall = doubleWalls;
            item.recalculateDimentions(false);
        }

        if (doubleWalls)
        {
            doubleWallsButton.text = "Double Wall Thickness";
        }
        else
        {
            doubleWallsButton.text = "Single Wall Thickness";
        }

        BackWall.SetActive(ShowBackWall);
        if (ShowBackWall)
        {
            BackwallButton.text = "Hide Back Wall";
        }
        else
        {
            BackwallButton.text = "Show Back Wall";
        }
    }

    #region Settings Toggles
    public void ToggleBackWall()
    {
        ShowBackWall = !ShowBackWall;
        BackWall.SetActive(ShowBackWall);
        if (ShowBackWall)
        {
            BackwallButton.text = "Hide Back Wall";
        }
        else
        {
            BackwallButton.text = "Show Back Wall";
        }
        UpdateMaxMins();
    }

    public void ToggleBackWall(bool _enabled)
    {
        ShowBackWall = _enabled;
        BackWall.SetActive(ShowBackWall);
        if (ShowBackWall)
        {
            BackwallButton.text = "Hide Back Wall";
        }
        else
        {
            BackwallButton.text = "Show Back Wall";
        }
        UpdateMaxMins();
    }
    public void ToggleDim()
    {
        //toggle showDim
        ShowDim = !ShowDim;

        //update button
        if (ShowDim)
        {
            VerticalDimDisplay.gameObject.SetActive(true);
            XlDimDisplay.gameObject.SetActive(true);
            ZlDimDisplay.gameObject.SetActive(true);
            showdimButton.text = "Hide Dimensions";

            VerticalDimDisplay.Coord1 = new Vector3(GetMaxX(), 0, GetMaxZ());
            VerticalDimDisplay.Coord2 = new Vector3(GetMaxX(), GetMaxY(), GetMaxZ());
            VerticalDimDisplay.DisplayOffset = new Vector3(0.1f, 0, 0.1f);

            XlDimDisplay.Coord1 = new Vector3(GetMinX(), 0, GetMaxZ());
            XlDimDisplay.Coord2 = new Vector3(GetMaxX(), 0, GetMaxZ());
            XlDimDisplay.DisplayOffset = new Vector3(0, 0, 0.1f);

            ZlDimDisplay.Coord1 = new Vector3(GetMinX(), 0, GetMinZ());
            ZlDimDisplay.Coord2 = new Vector3(GetMinX(), 0, GetMaxZ());
            ZlDimDisplay.DisplayOffset = new Vector3(-0.1f, 0, 0);

            VerticalDimDisplay.UpdateCoords();
            XlDimDisplay.UpdateCoords();
            ZlDimDisplay.UpdateCoords();
        }
        else {
            VerticalDimDisplay.gameObject.SetActive(false);
            XlDimDisplay.gameObject.SetActive(false);
            ZlDimDisplay.gameObject.SetActive(false);
            showdimButton.text = "Show Dimensions";
        }

    }

    public void ToggleStats()
    {
        showstats = !showstats;
        if (showstats)
        {
            showStatsButton.text = "Hide Statistics";
        }
        else {
            showStatsButton.text = "Show Statistics";
        }
    }

    public void ToggleScaleFigure()
    {
        showFigure = !showFigure;

        ScaleFigure.SetActive(showFigure);
        if (showFigure)
        {
            ShowFigureButton.text = "Hide Scale Figure";
        }
        else
        {
            ShowFigureButton.text = "Show Scale Figure";
        }

    }

    public void ToggleDoubleWalls()
    {
        doubleWalls = !doubleWalls;
        foreach (var item in AllBlocks)
        {
            item.DoubleWall = doubleWalls;
            item.recalculateDimentions(false);
        }
       
        if (doubleWalls)
        {
            doubleWallsButton.text = "Double Wall Thickness";
        }
        else
        {
            doubleWallsButton.text = "Single Wall Thickness";
        }

    }

    #endregion

    

    #region Getting bounds
    float GetMaxY()
    {
        float maxY = 0f;

        for (int i = 0; i < AllBlocks.Count; i++)
        {

            if (AllBlocks[i].GetMaxY() > maxY)
            {
                maxY = AllBlocks[i].GetMaxY();
            }
        }

        return maxY;
    }

    float GetMinX()
    {
        float minX = 999;
        foreach (var item in AllBlocks)
        {
            var temp = item.GetMinX();
            if (temp < minX)
            {
                minX = temp;
            }
        }
        return minX;
    }

    float GetMaxX()
    {
        float maxX = -999;
        foreach (var item in AllBlocks)
        {
            var temp = item.GetMaxX();
            if (temp > maxX)
            {
                maxX = temp;
            }
        }
        return maxX;
    }

    float GetMaxZ()
    {
        float maxZ = -999;
        foreach (var item in AllBlocks)
        {
            var temp = item.GetMaxZ();
            if (temp > maxZ)
            {
                maxZ = temp;
            }
        }
        return maxZ;
    }
    float GetMinZ()
    {
        float minZ = 9999;
        foreach (var item in AllBlocks)
        {
            var temp = item.GetMinZ();
            if (temp < minZ)
            {
                minZ = temp;
            }
        }
        return minZ;
    }

    #endregion

    #region block placement
    void PlaceBlockAtMouse()
    {
        Ray ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, PlacementLayers))
        {
            if (AllBlocks.Count == 0)
            {
                //there are no blocks in the scene, place this one at mouse position : hit.point + new Vector3(0,ModulePreview.GetComponent<ScalableComponent>().blockHeight/2,0);
                Vector3 spawnpos;
                if (ShowBackWall)
                {
                    spawnpos = new Vector3(-ModulePreview.GetComponent<ScalableComponent>().blockDepth / 2, Mathf.Clamp(hit.point.y, ModulePreview.GetComponent<ScalableComponent>().blockHeight / 2, 100), hit.point.z);
                }
                else
                {
                    spawnpos = new Vector3(hit.point.x, Mathf.Clamp(hit.point.y, ModulePreview.GetComponent<ScalableComponent>().blockHeight / 2, 100), hit.point.z);
                }
                GameObject PlacedBlock = Instantiate(blockPrefab[placingBlockIndex], spawnpos, Quaternion.identity);//create the new block
                var PlacedBlockComponent = PlacedBlock.GetComponent<ScalableComponent>(); //this holds a local reference to the scalable component on the newly created block so that I dont have to keep using Getcomponent, which is kinda slow
                AllBlocks.Add(PlacedBlockComponent);
                PlacedBlockComponent.DoubleWall = doubleWalls;
                PlacedBlockComponent.ConnectedModules = new ScalableComponent[PlacedBlockComponent.snappingPoints.Length];//initialise the list for storring all snapped blocks for the newly created block
                PlacedBlockComponent.recalculateDimentions(true);

                if (SelectedModule != null)
                {
                    SelectedModule.SetSelected(false);
                }

                SelectedModule = PlacedBlockComponent;
                SelectedModule.SetSelected(true);
                UpdateSelectionUI(true);
                ApplyUpdatesToSelection();
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

                    bool allowPlacement = true;
                    //check to see if the snapping point is compatible with the block we are dragging in
                    if (HitBlockSnap.mySnapIndex == 0 || HitBlockSnap.mySnapIndex == 1)
                    {
                        //we are snapping to the top or bottom of a block, make sure we only are placing a corner on corner
                        if ((HitBlockComponent.isCorner == PlacedBlockComponent.isCorner) && (HitBlockComponent.reverseCorner == PlacedBlockComponent.reverseCorner))
                        {
                            //block type matches


                        }
                        else
                        {
                            //block type missmatch
                            allowPlacement = false;
                            Destroy(PlacedBlock); //we were not supposed to be able to place a block here, cancel the placement
                            return;

                        }
                    }

                    AllBlocks.Add(PlacedBlockComponent);
                    PlacedBlockComponent.DoubleWall = doubleWalls;
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

        UpdateMaxMins();
    }

    void DragPreviewToMouse()
    {
        Plane m_Plane;



        m_Plane = new Plane(camera.transform.forward, Vector3.zero);

        //Create a ray from the Mouse click position
        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

        //Initialise the enter variable
        float enter = 0.0f;

        if (m_Plane.Raycast(ray, out enter))
        {
            //Get the point that is clicked
            Vector3 hitPoint = ray.GetPoint(enter);

            
            ModulePreview.transform.position = hitPoint;
        }
    }

    #endregion

    #region repositioning

    public void UpdateMaxMins()
    {
        //updates all of the furnitures maximum bounds so that it oesnt need to be done every frame
        maxX = GetMaxX();
        maxY = GetMaxY();
        maxZ = GetMaxZ();
        minX = GetMinX();
        minZ = GetMinZ();
        if (ShowBackWall)
        {
            UpdateBackWallPos();
        }

    }

    public void UpdateBackWallPos()
    {
        if (AllBlocks.Count == 0)
        {
            BackWall.transform.position = new Vector3(0, 5, 0);//probably dont need to get max X every frame
        }
        else
        {
            BackWall.transform.position = new Vector3(maxX, 5, 0);//probably dont need to get max X every frame
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

        UpdateMaxMins();
    }
    void UpdateFigurePos()
    {
        if (ShowBackWall)
        {

            if (AllBlocks.Count == 0)
            {
                ScaleFigure.transform.position = new Vector3(-1, 0, -1);
            }
            else
            {
                ScaleFigure.transform.position = new Vector3((GetMaxX() + GetMinX()) / 2 - 1, 0, GetMinZ() - 1);
            }

        }
        else
        {
            if (AllBlocks.Count == 0)
            {
                ScaleFigure.transform.position = new Vector3(0, 0, -1);
            }
            else
            {
                ScaleFigure.transform.position = new Vector3((GetMaxX() + GetMinX()) / 2, 0, GetMinZ() - 1);
            }

        }

    }

    #endregion

    #region edit Selection
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
        SelectedModule.CoreMaterial = CoreMaterialDropDown.value;
        SelectedModule.FinishMaterial = SurfaceMaterialDropdown.value;
        SelectedModule.selectedMat = SelectedMat[SurfaceMaterialDropdown.value];
        SelectedModule.unselectedMat = UnselectedMat[SurfaceMaterialDropdown.value];
        SelectedModule.SetSelected(true);

        UpdateMaxMins();
    }

    public void ApplyMaterialToAll()
    {
        foreach (var item in AllBlocks)
        {
            item.FinishMaterial = SurfaceMaterialDropdown.value;
            item.selectedMat = SelectedMat[SurfaceMaterialDropdown.value];
            item.unselectedMat = UnselectedMat[SurfaceMaterialDropdown.value];
            item.CoreMaterial = CoreMaterialDropDown.value;
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
            

            if (SelectedModule.allowHorizontalDividers)
            {
                ShelfInput.text = SelectedModule.HDividercount + "";
            }

            if (SelectedModule.allowVerticalDividers)
            {
                ShelfInput.text = SelectedModule.VDividercount + "";
            }

            SurfaceMaterialDropdown.value = SelectedModule.FinishMaterial;
            CoreMaterialDropDown.value = SelectedModule.CoreMaterial;
        }
        else {
            SelectionPannel.SetActive(false);
        }
        
    }

    #endregion

    

    #region get Statistics
    public float GetTotalStorageVolume()
    {
        float volume = 0f;
        foreach (var item in AllBlocks)
        {
            volume += item.GetStorageVolume();
        }

        return volume;
    }

    public float GetTotalShelfArea()
    {
        float area = 0f;
        foreach (var item in AllBlocks)
        {
            area += item.GetShelfArea();
        }
        return area;
    }

    public float GetTotalMaterialArea()
    {
        float area = 0;
        foreach (var item in AllBlocks)
        {
            area += item.GetMaterialArea();
        }
        return area;
    }

    public float GetTotalMaterialVolume()
    {
        float volume = 0;
        foreach (var item in AllBlocks)
        {
            volume += item.GetmaterialVolume();
        }
        return volume;
    }

    public float GetTotalCost()
    {
        float cost = 0;
        foreach (var item in AllBlocks)
        {
            cost += item.GetCost();
        }
        return cost;
    }

    public float GetTotalMass()
    {
        float mass = 0;
        foreach (var item in AllBlocks)
        {
            mass += item.Getmass();
        }
        return mass;
    }

    #endregion

    #region Model saving/loading/export

    public void Exportmodel()
    {
        //get file location and file name
        _path = StandaloneFileBrowser.SaveFilePanel("Export File", "", "Custom_Furniture", "fbx");
        Exporter.Fullpath = _path;
        for (int i = 0; i < AllBlocks.Count; i++)
        {
            AllBlocks[i].transform.SetParent(this.transform);
        }
        Exporter.rootObjectToExport = this.gameObject;
        Exporter.ExportGameObject();
    }
    public void ReloadModel() //saves and them imediately loads the saved model, for testing save/load
    {
        var temp = SaveModel();

        OpenModel(temp);
    }
    public ModelDescription SaveModel()
    {
        ModelDescription output = new ModelDescription();
        output.DoubleWall = doubleWalls;

        

        //fill out all module types, positions and rotations
        output.ModuleType = new byte[AllBlocks.Count];
        output.ModulePosX = new float[AllBlocks.Count];
        output.ModulePosY = new float[AllBlocks.Count];
        output.ModulePosZ = new float[AllBlocks.Count];
        output.ModuleRotY = new float[AllBlocks.Count];
        output.CoreMaterial = new byte[AllBlocks.Count];
        output.FinishMaterial = new byte[AllBlocks.Count];
        output.Width = new float[AllBlocks.Count];
        output.WidthB = new float[AllBlocks.Count];
        output.Height = new float[AllBlocks.Count];
        output.ShelfCount = new byte[AllBlocks.Count];
        for (int i = 0; i < AllBlocks.Count; i++)
        {
            AllBlocks[i].index = i;
            output.ModuleType[i] = AllBlocks[i].moduleType;
            output.ModulePosX[i] = AllBlocks[i].transform.position.x;
            output.ModulePosY[i] = AllBlocks[i].transform.position.y;
            output.ModulePosZ[i] = AllBlocks[i].transform.position.z;
            output.ModuleRotY[i] = AllBlocks[i].transform.eulerAngles.y;
            output.CoreMaterial[i] = (byte)AllBlocks[i].CoreMaterial;
            output.FinishMaterial[i] = (byte)AllBlocks[i].FinishMaterial;
            output.Width[i] = AllBlocks[i].blockWidth;
            output.WidthB[i] = AllBlocks[i].blockWidthB;
            output.Height[i] = AllBlocks[i].blockHeight;
            if (AllBlocks[i].allowHorizontalDividers)
            {
                output.ShelfCount[i] = (byte)AllBlocks[i].HDividercount;
            }
            else
            {
                output.ShelfCount[i] = (byte)AllBlocks[i].VDividercount;
            }
        }


        //fill out all connections
        //first find how many connections there are
        //only look at top and right connections to avoid getting double connections in array
        int connectioncount = 0;
        foreach (var item in AllBlocks)
        {
            if (item.ConnectedModules[0] != null)
            {
                //connection top side
                connectioncount++;
            }
            if (item.ConnectedModules[2] != null)
            {
                //connection right side
                connectioncount++;
            }
        }

        output.Mod1 = new int[connectioncount];
        output.Mod2 = new int[connectioncount];
        output.ConnectionSlotMod1 = new byte[connectioncount];
        output.ConnectionSlotMod2 = new byte[connectioncount];

        int counter = 0;
        foreach (var item in AllBlocks)
        {
            if (item.ConnectedModules[0] != null)
            {
                //connection top side
                output.Mod1[counter] = item.index;
                output.Mod2[counter] = item.ConnectedModules[0].index;
                output.ConnectionSlotMod1[counter] = 0;
                output.ConnectionSlotMod2[counter] = 1;
                counter++;
            }
            if (item.ConnectedModules[2] != null)
            {
                //connection right side
                output.Mod1[counter] = item.index;
                output.Mod2[counter] = item.ConnectedModules[2].index;
                output.ConnectionSlotMod1[counter] = 2;
                output.ConnectionSlotMod2[counter] = 3;
                counter++;
            }
        }

        
        return output;
    }

    public void OpenModel(ModelDescription _Model)
    {

        //first, clear up existing model
        SelectedModule = null;
        UpdateSelectionUI(false);
        List<GameObject> toDelete = new List<GameObject>();
        foreach (var item in AllBlocks)
        {
            
            Destroy(item.gameObject);
        }

        doubleWalls = _Model.DoubleWall;
        //now, create all modules
        AllBlocks = new List<ScalableComponent>();
        for (int i = 0; i < _Model.ModuleType.Length; i++)
        {
            GameObject PlacedBlock = Instantiate(blockPrefab[_Model.ModuleType[i]], new Vector3(_Model.ModulePosX[i], _Model.ModulePosY[i], _Model.ModulePosZ[i]), Quaternion.Euler(0,_Model.ModuleRotY[i],0));//create the new block
            var PlacedBlockComponent = PlacedBlock.GetComponent<ScalableComponent>(); //this holds a local reference to the scalable component on the newly created block so that I dont have to keep using Getcomponent, which is kinda slow
            
            AllBlocks.Add(PlacedBlockComponent);
            PlacedBlockComponent.DoubleWall = doubleWalls;
            PlacedBlockComponent.ConnectedModules = new ScalableComponent[PlacedBlockComponent.snappingPoints.Length];//initialise the list for storring all snapped blocks for the newly created block
            PlacedBlockComponent.CoreMaterial = _Model.CoreMaterial[i];
            PlacedBlockComponent.FinishMaterial = _Model.FinishMaterial[i];
            PlacedBlockComponent.blockWidth = _Model.Width[i];
            PlacedBlockComponent.blockWidthB = _Model.WidthB[i];
            PlacedBlockComponent.blockHeight = _Model.Height[i];
            if (PlacedBlockComponent.allowHorizontalDividers)
            {
                PlacedBlockComponent.HDividercount = _Model.ShelfCount[i];

            }
            else
            {
                PlacedBlockComponent.VDividercount = _Model.ShelfCount[i];
            }
            PlacedBlockComponent.index = i;
            PlacedBlockComponent.recalculateDimentions(false);
            PlacedBlockComponent.selectedMat = SelectedMat[_Model.FinishMaterial[i]];
            PlacedBlockComponent.unselectedMat = UnselectedMat[_Model.FinishMaterial[i]];
            PlacedBlockComponent.SetSelected(false);

        }

        //now create all connections
        for (int i = 0; i < _Model.Mod1.Length; i++)
        {
            AllBlocks[_Model.Mod1[i]].ConnectedModules[_Model.ConnectionSlotMod1[i]] = AllBlocks[_Model.Mod2[i]];
            AllBlocks[_Model.Mod2[i]].ConnectedModules[_Model.ConnectionSlotMod2[i]] = AllBlocks[_Model.Mod1[i]];
        }

        foreach (var item in AllBlocks)
        {
            item.DoubleWall = doubleWalls;
            item.recalculateDimentions(false);
        }

        UpdateMaxMins();
    }

    public byte[] ModelToBytes(ModelDescription _Model)
    {
        
        byte[] buffer = new byte[BYTE_SIZE];

        //this is where we crush data into byte array
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer);
        formatter.Serialize(ms, _Model);
        var shortbuffer = new byte[ms.Position + 1];
        for (int i = 0; i < shortbuffer.Length; i++)
        {
            shortbuffer[i] = buffer[i];
        }

        return shortbuffer;
    }

    public ModelDescription BytesToModel(byte[] _bytes)
    {
        ModelDescription output = new ModelDescription();

        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(_bytes);
        output = (ModelDescription)formatter.Deserialize(ms);
        return output;
    }
    public void SaveModelAs()
    {
        //get file location and file name
        _path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "Custom_Furniture", "shelf");
        var modelDesc = SaveModel();

        var modelBytes = ModelToBytes(modelDesc);

        SaveData(_path, modelBytes);
    }
    protected bool SaveData(string FileName, byte[] Data)
    {
        BinaryWriter Writer = null;
       

        try
        {
            // Create a new stream to write to the file
            Writer = new BinaryWriter(System.IO.File.OpenWrite(FileName));

            // Writer raw data                
            Writer.Write(Data);
            Writer.Flush();
            Writer.Close();
        }
        catch
        {
            //...
            return false;
        }

        return true;
    }

    public void OpenModelFromFile()
    {
        _path = StandaloneFileBrowser.OpenFilePanel("Open File", "", "shelf", false)[0];

        BinaryReader Reader = null;
        byte[] buffer = File.ReadAllBytes(_path);

        var modelDesc = BytesToModel(buffer);

        OpenModel(modelDesc);
    }

    public void OpenModelFromFile(string path)
    {
        _path = path;

        BinaryReader Reader = null;
        byte[] buffer = File.ReadAllBytes(_path);

        var modelDesc = BytesToModel(buffer);

        OpenModel(modelDesc);
    }

    public void LoadExampleModel(int _exampleNum)
    {
        switch (_exampleNum)
        {

            case (0):
                _path = Application.dataPath + "/StreamingAssets/Example1.shelf";
                break;

            case (1):
                _path = Application.dataPath + "/StreamingAssets/Example2.shelf";
                break;

            default:
                break;
        }

        BinaryReader Reader = null;
        byte[] buffer = File.ReadAllBytes(_path);

        var modelDesc = BytesToModel(buffer);

        OpenModel(modelDesc);

    }
    #endregion/export

    #region uploading model To builder
    public void OpenCloseUploadPannel(bool _Open)
    {
        //used to open and close the window used for uploading the project to the server
        if (_Open)
        {
            //should probably pause other things in the background while this is happening
            uploadePannel.SetActive(true);
        }
        else {
            //close the pannel and resume anything that was paused while it was opened
            uploadePannel.SetActive(false);
        }
    }

    public void TryuploadModel()
    {
        if (awaitingServer)
        {
            showPopup("Please wait for the first upload to complete before uploading a second project.");
            return;
        }
        //make sure we are actually connected to a server
        if (client == null)
        {
            //we aint connected
            showPopup("unable to connect to the server, try again later or email shc@straymoth.ca for support");
            return;
        }

        if (uploadProjectName.text == "")
        {
            //no project name
            showPopup("please enter a project name");
            return;
        }

        if (uploadClientName.text == "")
        {
            //name is empty
            showPopup("please enter your name");
            return;
        }
        //make sure everything is formatted properly in the upload input fields (email name etc)

        //is email valid
        if (Utility.IsValidEmail(uploadClientEmail.text))
        {
            email = uploadClientEmail.text;
            //email is valid (at least the format is)
        }
        else
        {
            Debug.Log("invalid email");
            //tell the user that email is invalid
            showPopup("please enter a valid email");
            return;

        }

        if (uploadClientAddress.text == "")
        {
            //no address - maybe in this case we should just use ip based location?
            showPopup("please enter an address");
            return;
        }

        //inputs are more or less valid (should probably also limit input lengths and maybe even add an email confirmation requirement)


        //set up the net_furniture message
        Net_Furniture msg = new Net_Furniture();

        msg.ModelDescription = ModelToBytes(SaveModel());
        msg.projectName = uploadProjectName.text;
        msg.clientName = uploadClientName.text;
        msg.clientEmail = uploadClientEmail.text;
        msg.clientLocation = uploadClientAddress.text;
        msg.jobType = 0;

        client.SendServerBig(msg);
        showPopup("Uploading project to server... this should only take a couple seconds.");
        awaitingServer = true;
        //message sent to server, should await response
    }

    public void UploadResponce(bool success)
    {
        awaitingServer = false;
        if (success)
        {
            //let the client know that the model was uploaded successfully
            OpenCloseUploadPannel(false);
            showPopup("Upload was successful, check your email (" + email + ") for further information");
        }
        else {
            showPopup("Upload was unsuccessful, try double checking your information and try again");
        }
    }

    public void showPopup(string PopupMessage)
    {
        awaitingpopup = true;
        PopUpPannel.SetActive(true);
        PopupText.text = PopupMessage;
    }

    public void closePopup()
    {
        awaitingpopup = false;
        PopUpPannel.SetActive(false);
    }
    #endregion

    #region conversion to components/shapes
    public FlatShape[] FurnitureToComponents()
    {
        //converts the whole scene into sheets that could make up the furninture
        List<FlatShape> results = new List<FlatShape>();

        
        foreach (var item in AllBlocks)
        {

            //first lets get all of the interior shapes (not top/bottom or sides)
            var temp = item.getShapesInside();
            foreach (var item2 in temp)
            {
                results.Add(item2);
            }

            //nexte lets get the horizontal surfaces
            temp = item.getShapesSides(!doubleWalls,!doubleWalls);
            if (temp != null)
            {
                foreach (var item2 in temp)
                {
                    results.Add(item2);
                }
            }
            

            //nexte lets get the vertical shapes
            temp = item.getShapesTopBottom(!doubleWalls);
            foreach (var item2 in temp)
            {
                results.Add(item2);
            }
        }
        return results.ToArray(); ;
    }

    public void PlaceAllShapes()
    {

        //clean up all children from the last time all shapes were placed
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }


        var allShapes = FurnitureToComponents();

        float widthoffset = 0;
        for (int i = 0; i < allShapes.Length; i++)
        {
            var temp = (GameObject)Instantiate(shapeSimple, transform);
            temp.transform.position = new Vector3( widthoffset, 0,0);

            widthoffset += allShapes[i].width/2 + 0.1f;
            if(i < allShapes.Length - 1)
            {
                widthoffset += allShapes[i + 1].width / 2 + 0.1f;
            }
            

            temp.transform.localScale = new Vector3(allShapes[i].width, 0.019f, allShapes[i].depth);

        }
    }

    #endregion

}

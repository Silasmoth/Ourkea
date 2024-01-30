using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlacer : MonoBehaviour
{
    public GameObject blockPrefab;
    public Camera camera;
    public int DisplayMode;
    //0-working/show snapping points
    //1 - show finished shelf

    public BlockController[] placedBlocks;

    public void SetBlockDisplay(int _displayType)
    {
        for (int i = 0; i < placedBlocks.Length; i++)
        {
            if (placedBlocks[i] != null)
            {
                placedBlocks[i].SetDisplay(_displayType);
            }
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        { 
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            LayerMask mask = LayerMask.GetMask("SnappingPoints");
            if (Physics.Raycast(ray, out hit,mask))
            {
                var HitBlockSnap = hit.collider.gameObject.GetComponent<BlockSnap>(); //stores the blocksnap of the blocksnap that was clicked
                var HitBlockComponent = hit.collider.gameObject.GetComponentInParent<ScalableComponent>(); //stores the scalablecomponent of the block that was clicked on


                if (HitBlockSnap != null)
                {
                    //mouse over snaping point of furniture
                    GameObject PlacedBlock = Instantiate(blockPrefab, hit.collider.gameObject.GetComponent<BlockSnap>().snapPos.position, Quaternion.identity);//create the new block
                    var PlacedBlockComponent = PlacedBlock.GetComponent<ScalableComponent>(); //this holds a reference to the scalable component on the newly created block
                    PlacedBlockComponent.ConnectedModules = new ScalableComponent[PlacedBlockComponent.snappingPoints.Length];//initialise the list for storring all snapped blocks for the newly created block

                    HitBlockComponent.ConnectedModules[HitBlockSnap.mySnapIndex] = PlacedBlockComponent; //sets the reference on the clicked block to the new block at the correct index
                    PlacedBlockComponent.ConnectedModules[HitBlockSnap.targetsnapIndex] = HitBlockComponent; //sets the reference on the new clock to the clicked bloxk at the correct reference
                    PlacedBlockComponent.myConnectionsSides = new List<ScalableComponent>(); //initialise list on newly created block
                    PlacedBlockComponent.myConnectionsTopBot = new List<ScalableComponent>(); //initialise list on newly created block

                    if (HitBlockSnap.SideSnap)
                    {
                        HitBlockComponent.myConnectionsSides.Add(PlacedBlockComponent);
                        PlacedBlockComponent.myConnectionsSides.Add(HitBlockComponent);
                        PlacedBlockComponent.blockHeight = HitBlockComponent.blockHeight;
                    }
                    if (HitBlockSnap.TopBotSnap)
                    {
                        HitBlockComponent.myConnectionsTopBot.Add(PlacedBlockComponent);
                        PlacedBlockComponent.myConnectionsTopBot.Add(HitBlockComponent);
                        PlacedBlockComponent.blockWidth = HitBlockComponent.blockWidth;
                    }

                    PlacedBlockComponent.recalculateDimentions();
                    PlacedBlockComponent.SetPosition(HitBlockSnap.snapPos.position, HitBlockSnap.targetsnapIndex);
                   
                }
                
            }
        }

        
    }
}

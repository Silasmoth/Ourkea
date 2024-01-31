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
                    GameObject PlacedBlock = Instantiate(blockPrefab, HitBlockSnap.snapPos.position, HitBlockSnap.snapPos.rotation);//create the new block
                    var PlacedBlockComponent = PlacedBlock.GetComponent<ScalableComponent>(); //this holds a local reference to the scalable component on the newly created block so that I dont have to keep using Getcomponent, which is kinda slow
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
                            if (leftBlock != null)//there is a block to the lower left
                            {
                                leftBlock = leftBlock.ConnectedModules[0];//left block is not the block that is directly left of the newly placed block
                               
                                if(leftBlock!= null)
                                {
                                    PlacedBlockComponent.ConnectedModules[3] = leftBlock;//null if there is no block to the left
                                    leftBlock.ConnectedModules[2] = PlacedBlockComponent;
                                    PlacedBlockComponent.blockHeight = leftBlock.blockHeight;
                                }
                                   
                            }
                            //now do the same thing for the right side
                            if(rightBlock != null)
                            {
                                rightBlock = rightBlock.ConnectedModules[0];
                                
                                if(rightBlock != null)
                                {
                                    PlacedBlockComponent.ConnectedModules[2] = rightBlock;
                                    rightBlock.ConnectedModules[3] = PlacedBlockComponent;
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
                                    PlacedBlockComponent.ConnectedModules[3] = leftBlock;//null if there is no block to the left
                                    leftBlock.ConnectedModules[2] = PlacedBlockComponent;
                                    PlacedBlockComponent.blockHeight = leftBlock.blockHeight;
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
                                
                                if(topBlock != null)
                                {
                                    PlacedBlockComponent.ConnectedModules[0] = topBlock;
                                    topBlock.ConnectedModules[1] = PlacedBlockComponent;
                                    PlacedBlockComponent.blockWidth = topBlock.blockWidth;
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
                                }
                            }
                            break;

                    }

                    PlacedBlockComponent.recalculateDimentions();
                    //recalculate the dimentions for the newly placed block
                    PlacedBlockComponent.SetPosition(HitBlockSnap.snapPos.position, HitBlockSnap.targetsnapIndex);
                    //set the position of the newly created block

                }

            }
        }

        
    }
}

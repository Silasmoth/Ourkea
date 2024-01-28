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
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.GetComponent<BlockSnap>() != null)
                {
                    //mouse over snaping point of furniture
                    GameObject temp = Instantiate(blockPrefab, hit.collider.gameObject.GetComponent<BlockSnap>().snapPos.position, hit.collider.gameObject.GetComponent<BlockSnap>().snapPos.rotation);
                    temp.GetComponent<BlockController>().placer = this;
                    temp.GetComponent<BlockController>().SetDisplay(DisplayMode);
                }
                
            }
        }

        
    }
}

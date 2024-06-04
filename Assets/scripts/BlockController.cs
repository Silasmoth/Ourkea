using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{

    public BlockPlacer placer;
    public Renderer[] snappingRenderers;
    public Renderer[] ActualRenderers;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    

    public void SetDisplay(int _type)
    {
        if (_type == 0)
        {
            //show all snapping points
            for (int i = 0; i < snappingRenderers.Length; i++)
            {
                snappingRenderers[i].enabled = true;
            }
            for (int i = 0; i < ActualRenderers.Length; i++)
            {
                ActualRenderers[i].enabled = false;
            }
        }

        if (_type == 1)
        {
            //show all snapping points
            for (int i = 0; i < snappingRenderers.Length; i++)
            {
                snappingRenderers[i].enabled = false;
            }
            for (int i = 0; i < ActualRenderers.Length; i++)
            {
                ActualRenderers[i].enabled = true;
            }
        }
    }
}

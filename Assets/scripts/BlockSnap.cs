using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSnap : MonoBehaviour
{
    public ScalableComponent myParent;
    public Transform snapPos;
    public int targetsnapIndex;//holds the snap index that this should connect to on the added block
    public int mySnapIndex;//the snap index of this snap component
    public bool SideSnap;//is this snapping point on the side
    public bool TopBotSnap;//is this snapping point on the top/bottom
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

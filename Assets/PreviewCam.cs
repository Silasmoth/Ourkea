using UnityEngine;

public class PreviewCam : MonoBehaviour
{
    public Camera cam;
    public Vector3 furnitureCenter;
    public Vector3 camOffset;
    public float DistanceMultiplier = 1;
    public void RenderPreview()
    { 
        transform.position = furnitureCenter + camOffset * DistanceMultiplier;
        cam.Render();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

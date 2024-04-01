using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
public class DimentionDisplay : MonoBehaviour
{
    public Vector3 Coord1;
    public Vector3 Coord2;
    public Vector3 DisplayOffset;
    // Start is called before the first frame update

    public LineRenderer MainLine;
    public LineRenderer Topline;
    public LineRenderer BottomLine;
    public TextMeshPro DimText;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCoords();//not needed every frame
    }

    public void UpdateCoords()
    {
        MainLine.SetPosition(0, Coord1 + DisplayOffset);
        MainLine.SetPosition(1, Coord2 + DisplayOffset);
        Topline.SetPosition(1, Coord1);
        Topline.SetPosition(0, Coord1 + DisplayOffset);
        BottomLine.SetPosition(1, Coord2);
        BottomLine.SetPosition(0, Coord2 + DisplayOffset);


        DimText.transform.position = ((Coord1 + Coord2) / 2) + DisplayOffset*2;
        DimText.text =  Mathf.Round((Coord1 - Coord2).magnitude * 1000f) + "mm";
    }
}

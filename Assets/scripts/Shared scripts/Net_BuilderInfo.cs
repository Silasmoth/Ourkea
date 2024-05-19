
[System.Serializable]
public class Net_BuilderInfo : NetMsg
{
    
    public Net_BuilderInfo()
    {
        OP = NetOP.BuilderInfo;
    }
    //this message stores all of the builders account settings like their name, location and all of their project preferences
    public string name { get; set; }//builders name
    public string address { get; set; }//builders address, stored as a plain text query, formatted address information will be found serverside using nominatim

    public float serviceRange { get; set; }//what distance from the builders workshop are they willing to service clients (aka delivery range)
    public bool[] materialpreferences { get; set; }//the setting for every material, determining if the builder will be assigned projects that contain that material or not

    public float maxDim1 { get; set; }//maximum component dimention the builder is willing to make
    public float maxDim2 { get; set; }//other maximum component dimention the builder is willing to make

    

   
}

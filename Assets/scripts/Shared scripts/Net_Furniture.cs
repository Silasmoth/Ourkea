
[System.Serializable]
public class Net_Furniture : NetMsg
{
    
    public Net_Furniture()
    {
        OP = NetOP.Furniture;
    }


    public byte[] ModelDescription { get; set; } //this is the byte array generated by formatting a modeldescription - might be more efficient to just have a modeldescription object but this is easier

    public string clientName { get; set; }

    public string clientEmail { get; set; }

    public string clientLocation { get; set; } //it might be better to store this in a different format, depending on if I store the location as an address or as coordinates

    public byte jobType { get; set; }//stores what type of job this is (new or adaptation) storing as a byte so that in the future I can add other job types or just use this to indicate message context
    //0-new build
    //1-adaptation (existing model)
    //2-adaptation (new design)
}


[System.Serializable]
public class Net_ProjectUpdate : NetMsg
{
    //data bytes 32 + string
    public Net_ProjectUpdate()
    {
        OP = NetOP.ProjectUpdate;
    }

    public string ProjectID { get; set; }//the project we are refering to  
    
    public byte action { get; set; }//what is happening with this project
    //0 - Accepted
    //1 - Declined
    //2 - Deleted

}


[System.Serializable]
public class ModelDescription
{
    //global parameters
    public bool DoubleWall { get; set; }//does this furniture use double walls

    //per module parameters
    //32 bytes per
    public byte[] ModuleType { get; set; } //which module type is this byte size 1
    
    //module position
    public float[] ModulePosX { get; set; } //5 bytes
    public float[] ModulePosY { get; set; }//9 bytes
    public float[] ModulePosZ { get; set; }//13 bytes

    //module rotation
    public float[] ModuleRotY { get; set; }//17 bytes

    //module custom variables

    public byte[] CoreMaterial { get; set; }//18 bytes
    public byte[] FinishMaterial { get; set; }//19 bytes
    public float[] Width { get; set; }//23 bytes
    public float[]WidthB { get; set; }//27 bytes
    public float[] Height { get; set; }//31 bytes
    public byte[] ShelfCount { get; set; }//32 bytes
    public bool[] EdgeFinish { get; set; }//1 byte


    //module connections
    //10 bytes per
    public int[] Mod1 { get; set; }//stores the Index of the module to connect from 4 bytes
    public int[] Mod2 { get; set; }//stores the index of the module to connect to
    public byte[] ConnectionSlotMod1 { get; set; }//stores the connection slot (on Mod1) that the connection is in
    public byte[] ConnectionSlotMod2 { get; set; }//stores the connection slot (on Mod2) that the connection is in
}
   

public struct Voxel
{
    public byte ID;
    public bool isSolid
    {
        get { return ID != 0; }
    }
}

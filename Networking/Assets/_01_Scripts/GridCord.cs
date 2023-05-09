public struct GridCord
{
    uint x, y;

    public GridCord(uint x, uint y)
    {
        this.x = x;
        this.y = y;
    }

    public GridCord[] GetNeighbourCords()
    {
        GridCord[] result = new GridCord[4];
        result[0] = new GridCord(x + 1, y);
        result[1] = new GridCord(x - 1, y);
        result[2] = new GridCord(x, y + 1);
        result[3] = new GridCord(x, y - 1);
        return result;
    }
}

public class BiomeMapData
{
    public int Resolution { get; private set; }

    public float[] ForestWeight { get; private set; }

    public BiomeMapData(int resolution)
    {
        Resolution = resolution;

        ForestWeight = new float[resolution * resolution];
    }

    public int GetIndex(int x, int y)
    {
        return y * Resolution + x;
    }
}

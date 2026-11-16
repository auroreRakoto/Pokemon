
public class MeadowData
{
    public int Resolution { get; private set; }

    public float[] Vegetation { get; private set; }
    public float[] Flora { get; private set; }
    public float[] Rocks { get; private set; }
    public float[] Bushes { get; private set; }
    public float[] Water { get; private set; }
    public float[] Humidity { get; private set; }

    public MeadowData(int resolution)
    {
        Resolution = resolution;

        int size = resolution * resolution;

        Vegetation = new float[size];
        Flora = new float[size];
        Rocks = new float[size];
        Bushes = new float[size];
        Water = new float[size];
        Humidity = new float[size];
    }

    public int GetIndex(int x, int y)
    {
        return y * Resolution + x;
    }
}

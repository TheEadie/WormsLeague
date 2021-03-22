namespace WormsRandomizer.Random
{
    public interface IRng
    {
        int Next(int maxValue);
        int Next(int lower, int upper);
    }
}
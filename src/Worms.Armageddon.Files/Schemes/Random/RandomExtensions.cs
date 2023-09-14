namespace Worms.Armageddon.Files.Schemes.Random;

internal static class RandomExtensions
{
    public static T RandomChoice<T>(this IReadOnlyList<T> list, System.Random rng)
    {
        var index = rng.Next(list.Count);
        return list[index];
    }

    public static double FractionThrough<T>(this IReadOnlyList<T> list, T item)
    {
        for (var i = 0; i < list.Count; ++i)
        {
            if (list[i].Equals(item))
            {
                return (i + 1) / (double) list.Count;
            }
        }
        return 0;
    }

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable, System.Random rng)
    {
        var arr = enumerable.ToArray();
        for (var i = 0; i < arr.Length - 1; ++i)
        {
            var swapIndex = rng.Next(i, arr.Length);
            (arr[i], arr[swapIndex]) = (arr[swapIndex], arr[i]);
        }
        return arr;
    }

    public static T RouletteWheel<T>(this IEnumerable<Tuple<T, int>> enumerable, System.Random rng)
    {
        var wheel = enumerable.ToArray();
        var maxValue = wheel.Sum(section => section.Item2);
        var chosenValue = rng.Next(maxValue);

        var upperLimit = 0;
        foreach (var section in wheel)
        {
            upperLimit += section.Item2;
            if (upperLimit > chosenValue)
            {
                return section.Item1;
            }
        }

        return wheel.Last().Item1;
    }
}

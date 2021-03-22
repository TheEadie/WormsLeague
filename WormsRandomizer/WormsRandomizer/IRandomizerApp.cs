namespace WormsRandomizer
{
    internal interface IRandomizerApp
    {
        void DoRandomizer(string[] args);
        void PrintHelp();
        void PrintWeaponList();
    }
}
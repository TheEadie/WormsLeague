namespace Worms.Armageddon.Game.Win;

internal class FileVersionInfo : IFileVersionInfo
{
    public Version GetVersionInfo(string fileName)
    {
        var fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(fileName);
        return new Version(
            fileVersionInfo.ProductMajorPart,
            fileVersionInfo.ProductMinorPart,
            fileVersionInfo.ProductBuildPart,
            fileVersionInfo.ProductPrivatePart);
    }
}

namespace Worms.Armageddon.Game.System;

internal class FileVersionInfo : IFileVersionInfo
{
    public Version GetVersionInfo(string fileName)
    {
        var fileVersionInfo = global::System.Diagnostics.FileVersionInfo.GetVersionInfo(fileName);
        return new Version(
            fileVersionInfo.ProductMajorPart,
            fileVersionInfo.ProductMinorPart,
            fileVersionInfo.ProductBuildPart,
            fileVersionInfo.ProductPrivatePart);
    }
}

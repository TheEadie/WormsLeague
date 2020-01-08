namespace Worms.Updates.Installers
{
    public interface IFileCopierInstaller
    {
        void Install(string installFrom, string installTo);
    }
}
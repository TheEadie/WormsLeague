using System.IO.Abstractions;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Worms.Cli.Resources.Remote.Auth;

internal sealed class TokenStore : ITokenStore
{
    private readonly IFileSystem _fileSystem;
    private readonly IDataProtector _dataProtector;
    private readonly string _tokenStoreFolder;
    private readonly string _tokenStorePath;

    public TokenStore(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;

        var dataProtectionProvider = DataProtectionProvider.Create("Worms CLI");
        _dataProtector = dataProtectionProvider.CreateProtector("API tokens");
        _tokenStoreFolder = _fileSystem.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programs",
            "Worms");
        _tokenStorePath = _fileSystem.Path.Combine(_tokenStoreFolder, "tokens.json");
    }

    public AccessTokens GetAccessTokens()
    {
        if (_fileSystem.File.Exists(_tokenStorePath))
        {
            var fileContent = _fileSystem.File.ReadAllText(_tokenStorePath);
            var protectedTokens = JsonSerializer.Deserialize(fileContent, JsonContext.Default.AccessTokens);

            if (protectedTokens is null)
            {
                return new AccessTokens(null, null);
            }

            var accessToken = protectedTokens.AccessToken is not null
                ? _dataProtector.Unprotect(protectedTokens.AccessToken)
                : null;
            var refreshToken = protectedTokens.RefreshToken is not null
                ? _dataProtector.Unprotect(protectedTokens.RefreshToken)
                : null;
            return new AccessTokens(accessToken, refreshToken);
        }
        else
        {
            return new AccessTokens(null, null);
        }
    }

    public void StoreAccessTokens(AccessTokens accessTokens)
    {
        var accessToken = accessTokens.AccessToken is not null
            ? _dataProtector.Protect(accessTokens.AccessToken)
            : null;
        var refreshToken = accessTokens.RefreshToken is not null
            ? _dataProtector.Protect(accessTokens.RefreshToken)
            : null;
        var protectedTokens = new AccessTokens(accessToken, refreshToken);

        if (!_fileSystem.Directory.Exists(_tokenStoreFolder))
        {
            _ = _fileSystem.Directory.CreateDirectory(_tokenStoreFolder);
        }

        _fileSystem.File.WriteAllText(
            _tokenStorePath,
            JsonSerializer.Serialize(protectedTokens, JsonContext.Default.AccessTokens));
    }
}

using System;
using System.IO.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace Worms.Server.Auth
{
    public class TokenStore : ITokenStore
    {
        private readonly IFileSystem _fileSystem;
        private readonly IDataProtector _dataProtector;
        private readonly string _tokenStorePath;

        public TokenStore(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;

            var dataProtectionProvider = DataProtectionProvider.Create("Worms CLI");
            _dataProtector = dataProtectionProvider.CreateProtector("API tokens");
            _tokenStorePath = _fileSystem.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs",
                "Worms",
                "tokens.json");
        }

        public AccessTokens GetAccessTokens()
        {
            if (_fileSystem.File.Exists(_tokenStorePath))
            {
                var fileContent = _fileSystem.File.ReadAllText(_tokenStorePath);
                var protectedTokens = JsonConvert.DeserializeObject<AccessTokens>(fileContent);
                var accessToken = _dataProtector.Unprotect(protectedTokens.AccessToken);
                var refreshToken = _dataProtector.Unprotect(protectedTokens.RefreshToken);
                return new AccessTokens(accessToken, refreshToken);
            }
            else
            {
                return new AccessTokens(null, null);
            }
        }

        public void StoreAccessTokens(AccessTokens accessTokens)
        {
            var accessToken = _dataProtector.Protect(accessTokens.AccessToken);
            var refreshToken = _dataProtector.Protect(accessTokens.RefreshToken);
            var protectedTokens = new AccessTokens(accessToken, refreshToken);
            _fileSystem.File.WriteAllText(_tokenStorePath, JsonConvert.SerializeObject(protectedTokens));
        }
    }
}

﻿using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net;
using System.Text;

namespace mct_timer.Models
{
    public interface IKeyVaultMng
    {
        public string Decrypt(string txt);
        public string Encrypt(string txt);
        public string GetKeyPublic();
    }


    public class KeyVaultMng: IKeyVaultMng
    {
        CryptographyClient _crypto = null;
        string _key;
        string _keyvault;
        string _tenant;
        TelemetryClient _appInsights;

        public KeyVaultMng(string keyvault, string key, string tenant, TelemetryClient clt)
        {
            _appInsights = clt;
            _key = key;
            _keyvault = keyvault;
            _tenant = tenant;

        }

        public void Init()
        {

            try
            {
                var cred = new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions()
                    {
                        TenantId = _tenant,
                        AdditionallyAllowedTenants = { "*" },
                    });

                var client = new KeyClient(new Uri(_keyvault), cred);
                KeyVaultKey key = client.GetKey(_key);
                _crypto = new CryptographyClient(key.Id, cred);
            }
            catch (Exception ex)
            {
                _appInsights.TrackException(ex);
            }
        }


        public string GetKeyPublic()
        {
            if (_crypto == null) Init();
            return  _crypto.KeyId;
        }


        public string Encrypt(string txt)
        {
            if (_crypto==null) Init();

            byte[] textAsBytes = Encoding.UTF8.GetBytes(txt);
            EncryptResult encryptResult = _crypto.Encrypt(EncryptionAlgorithm.RsaOaep256, textAsBytes);
            return Convert.ToBase64String(encryptResult.Ciphertext);
        }

        public string Decrypt(string txt)
        {
            if (_crypto == null) Init();

            var dtext = Convert.FromBase64String(txt);
            DecryptResult decryptResult = _crypto.Decrypt(EncryptionAlgorithm.RsaOaep256, dtext);
            return Encoding.UTF8.GetString( decryptResult.Plaintext);
        }

    }
}

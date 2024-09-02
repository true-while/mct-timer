using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Microsoft.Azure.Cosmos.Core;
using System.Net;
using System.Text;

namespace mct_timer.Models
{
    public interface IKeyVaultMng
    {
        public string Decrypt(string txt);
        public string Encrypt(string txt);
    }


    public class KeyVaultMng: IKeyVaultMng
    {
        KeyClient _client;
        KeyVaultKey _key;
        CryptographyClient _crypto;

        public KeyVaultMng(string keyvault, string key)
        {
            var cred = new DefaultAzureCredential(
                new DefaultAzureCredentialOptions()
                {
                    AdditionallyAllowedTenants = { "*" },
                });

            _client = new KeyClient(new Uri(keyvault),cred);
            _key = _client.GetKey(key);
            _crypto = new CryptographyClient(_key.Id, cred);

        }

        public string Encrypt(string txt)
        {
            byte[] textAsBytes = Encoding.UTF8.GetBytes(txt);
            EncryptResult encryptResult = _crypto.Encrypt(EncryptionAlgorithm.RsaOaep256, textAsBytes);
            return Convert.ToBase64String(encryptResult.Ciphertext);
        }

        public string Decrypt(string txt)
        {
            var dtext = Convert.FromBase64String(txt);
            DecryptResult decryptResult = _crypto.Decrypt(EncryptionAlgorithm.RsaOaep256, dtext);
            return Encoding.UTF8.GetString( decryptResult.Plaintext);
        }

    }
}

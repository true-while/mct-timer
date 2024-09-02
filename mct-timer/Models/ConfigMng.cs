namespace mct_timer.Models
{
    public class ConfigMng
    {
        public string OpenAIKey { get; set; }              //open ai service key
        public string OpenAIEndpoint { get; set; }         //open ai service url
        public string StorageAccountString { get; set; }   //storage account key for keep images 
        public string Container { get; set; }              //container for keep images
        public string OpenAIModel { get; set; }            //name of open ai dale model
        public string JWT { get; set; }                    // jwt encryption string
        public string KeyVault { get; set; }               // keyvault url
        public string PssKey { get; set; }                 // name of the key for pwd encryption
    }
}

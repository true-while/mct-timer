namespace mct_timer.Models
{
    public class ConfigMng
    {
        public string OpenAIKey { get; set; }              //open ai service key
        public string OpenAIEndpoint { get; set; }         //open ai service url
        public string StorageAccountName { get; set; }   //storage account key for keep images 
        public string ContainerName { get; set; }              //container for keep images
        public string OpenAIModel { get; set; }            //name of open ai dale model
        public string MaxAIinTheDay { get; set; }          // max ai gen images in the day
        public string JWT { get; set; }                    // jwt encryption string
        public string KeyVault { get; set; }               // keyvault url
        public string PssKey { get; set; }                 // name of the key for pwd encryption
        public int FileSizeLimit { get; set; }             // max upload file size
        public string WebCDN { get; set; }                 // address of the storage account (web static site)
        public string CosmosDBEndpoint { get; set; }       // cosmos db 
        public string TenantID { get; set; }               // tenant Id required for managed identity and tests 
        public string SubscriptionID { get; set; }          // subscription id
        public string PwdResetRequestUrl { get; set; }  //logic app url to sent Email for Password reset
    }
}

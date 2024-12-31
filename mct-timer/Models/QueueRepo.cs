using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace mct_timer.Models
{
    public interface IQueueRepo
    {
        public Task<string> SendMessageAsync(string msg);
        public Task<string> PeakMessageAsync();
    }


    public class QueueRepo: IQueueRepo
    {

        QueueClient _client;
        string _queuename;
        string _accountname;
        string _tenantid;

        public QueueRepo(string accountName, string queueName, string tenantid)
        {
            _accountname = accountName;
            _queuename = queueName;
            _tenantid = tenantid;
        }

        private void CreateQueue()
        {
            if (_client == null)
            {
                var queUri = new Uri($"https://{_accountname}.queue.core.windows.net/{_queuename}");

                var cred = new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions()
                    {
                        TenantId = _tenantid,
                        AdditionallyAllowedTenants = { "*" },
                    });
                _client = new QueueClient(queUri, cred);
                _client.CreateIfNotExists();
            }
        }

        public async Task<string> SendMessageAsync(string msg)
        {
            CreateQueue();            
            SendReceipt receipt  = await _client.SendMessageAsync(msg);

            return receipt.MessageId;
        }

        public async Task<string> PeakMessageAsync()
        {
            CreateQueue();

            Azure.Response<QueueMessage> msg = await _client.ReceiveMessageAsync();

            return  UTF32Encoding.UTF8.GetString( msg.Value.Body);
        }
    }
}

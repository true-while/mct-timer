﻿using Microsoft.Extensions.Options;

namespace mct_timer.Models
{
    public class AvTest
    {
        private IOptions<ConfigMng> config;
        private IBlobRepo blobRepo;
        private UsersContext ac_context;
        private IKeyVaultMng keyVaultMng;
        private IDalleGenerator dalle;

        public AvTest(IOptions<ConfigMng> config, IBlobRepo blobRepo, UsersContext ac_context, IKeyVaultMng keyVaultMng, IDalleGenerator dalle)
        {
            this.config = config;
            this.blobRepo = blobRepo;
            this.ac_context = ac_context;
            this.keyVaultMng = keyVaultMng;
            this.dalle = dalle;
        }

        public string GetConfigValue
        {
            get{

                return config.Value.WebCDN;
            }
        }

        public string GetKeyvault
        {
            get
            {
                return keyVaultMng.GetKeyPublic().Substring(0,12);
            }
        }

        public string GetBlobRepo
        {
            get
            {
                return blobRepo.TestConnection().ToString().Substring(0,12);
            }
        }

        public string GetCosmosDB
        {
            get
            {
                return ac_context.ContextId.InstanceId.ToString().Substring(0,12);
            }
        }

        public bool GetDalle
        {
            get
            {
                return dalle.TestConnection();
            }
        }
    }
}


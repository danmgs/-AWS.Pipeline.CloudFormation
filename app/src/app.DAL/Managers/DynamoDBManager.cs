using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using app.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace app.DAL.Managers
{
    public class DynamoDBManager<T> : IDBManager<T>
                                        where T : IDbEntity, new()
    {
        static readonly ILog _log = LogManager.GetLogger(typeof(DynamoDBManager<T>));

        AmazonDynamoDBClient _client;

        DynamoDBContext _context;

        //string _tableName;

        public DynamoDBManager()
        {
            AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();

            // This client will access the following region.
            //clientConfig.RegionEndpoint = RegionEndpoint.EUWest3;

            _client = CreateClient(clientConfig);
            _context = new DynamoDBContext(_client, new DynamoDBContextConfig
            {
                ConsistentRead = false
                // TableNamePrefix = "Env_"
            });
        }

        AmazonDynamoDBClient CreateClient(AmazonDynamoDBConfig regionEndpoint)
        {
            AmazonDynamoDBClient client;
            try { client = new AmazonDynamoDBClient(); }
            catch (Exception ex)
            {
                _log.Fatal("FAILED to create a DynamoDB client", ex);
                throw;
            }

            return client;
        }

        public List<T> Scan()
        {
            var conditions = new List<ScanCondition>();
            var response = _context.ScanAsync<T>(conditions).GetNextSetAsync();
            return response.Result;
        }

        public async Task<T> Get(string id)
        {
            return await Get(new T() { Id = id });
        }

        public async Task<T> Get(T entity)
        {
            var res = await _context.LoadAsync<T>(entity);
            return res;
        }

        public async Task<bool> Create(T entity)
        {
            try
            {
                await _context.SaveAsync<T>(entity);

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Create in error", ex);
                return false;
            }
        }

        public async Task<bool> Update(T entity)
        {
            return await Create(entity);
        }

        public async Task<bool> Delete(string id)
        {
            return await Delete(new T() { Id = id });
        }

        public async Task<bool> Delete(T entity)
        {
            try
            {
                await _context.DeleteAsync<T>(entity);

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Delete in error", ex);
                return false;
            }
        }

        public void Dispose()
        {
            if (_client != null)
                _client = null;
        }

        #region Private methods

        static void PrintItem(
            Dictionary<string, AttributeValue> attributeList)
        {
            foreach (KeyValuePair<string, AttributeValue> kvp in attributeList)
            {
                string attributeName = kvp.Key;
                AttributeValue value = kvp.Value;

                Console.WriteLine(
                    attributeName + " " +
                    (value.S == null ? "" : "S=[" + value.S + "]") +
                    (value.N == null ? "" : "N=[" + value.N + "]") +
                    (value.SS == null ? "" : "SS=[" + string.Join(",", value.SS.ToArray()) + "]") +
                    (value.NS == null ? "" : "NS=[" + string.Join(",", value.NS.ToArray()) + "]")
                    );
            }
            Console.WriteLine("************************************************");
        }

        #endregion

    }
}

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SharpRepository.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeyhdBot.MongoDb.Model;
using WeyhdBot.MongoDb.Options;

namespace WeyhdBot.MongoDb
{
    public class MongoDBConnector : IMongoDBConnector
    {
        private RepositoryFactory _factory { get; }

        public MongoDBConnector(RepositoryFactory factory)
        {
            _factory = factory;
        }

        public async Task CreateDocumentAsync<T>(string dbName, string id, T data)
        {
            var mongo = _factory.GetInstance<MongoDBEntry<T>, string>(dbName);
            await Task.Factory.StartNew(()=> mongo.Add(new MongoDBEntry<T>(id, data)));
            
        }

        public IQueryable<MongoDBEntry<T>> CreateDocumentQuery<T>(string dbName)
        {
            var mongo = _factory.GetInstance<MongoDBEntry<T>, string>(dbName);
            
            return mongo.GetAll().AsQueryable();
        }

        public async Task UpsertDocumentAsync<T>(string dbName, string id, T data)
        {
            var mongo = _factory.GetInstance<MongoDBEntry<T>, string>(dbName);
            await Task.Factory.StartNew(() => mongo.Update(new MongoDBEntry<T>(id, data)));
        }
    }
}

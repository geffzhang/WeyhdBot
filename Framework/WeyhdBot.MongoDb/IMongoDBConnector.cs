using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeyhdBot.MongoDb.Model;

namespace WeyhdBot.MongoDb
{
    public interface IMongoDBConnector
    {
        /// <summary>
        /// Creates a new document in the database.
        /// </summary>
        Task CreateDocumentAsync<T>(string dbName, string id, T data);
        /// <summary>
        /// Creates a new document in the database or updates an existing one of the ID matches
        /// </summary>
        Task UpsertDocumentAsync<T>(string dbName, string id, T data);
        /// <summary>
        /// Retrieves a queryable path into a collection
        /// </summary>
        IQueryable<MongoDBEntry<T>> CreateDocumentQuery<T>(string dbName);
    }
}

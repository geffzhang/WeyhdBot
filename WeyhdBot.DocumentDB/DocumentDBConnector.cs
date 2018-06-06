using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using WeyhdBot.DocumentDB.Model;
using WeyhdBot.DocumentDB.Options;

namespace WeyhdBot.DocumentDB
{
    public class DocumentDBConnector : IDocumentDBConnector
    {
        private DocumentClient Client { get; }

        public DocumentDBConnector(IOptions<DocumentDBOptions> options)
        {
            var uri = new Uri(options.Value.Uri);
            var key = options.Value.Key;

            Client = new DocumentClient(uri, key);
        }

        public async Task CreateDocumentAsync<T>(string dbName, string collectionName, string id, T data)
        {
            var doc = new DocumentDBEntry<T>(id, data);
            var collectionUri = UriFactory.CreateDocumentCollectionUri(dbName, collectionName);
            await Client.CreateDocumentAsync(collectionUri, doc);
        }

        public async Task UpsertDocumentAsync<T>(string dbName, string collectionName, string id, T data)
        {
            var doc = new DocumentDBEntry<T>(id, data);
            var collectionUri = UriFactory.CreateDocumentCollectionUri(dbName, collectionName);
            await Client.UpsertDocumentAsync(collectionUri, doc);
        }

        public IQueryable<DocumentDBEntry<T>> CreateDocumentQuery<T>(string dbName, string collectionName)
        {
            var queryOptions = new FeedOptions { MaxItemCount = -1 };
            var collectionUri = UriFactory.CreateDocumentCollectionUri(dbName, collectionName);

            return Client.CreateDocumentQuery<DocumentDBEntry<T>>(collectionUri, queryOptions);
        }
    }
}

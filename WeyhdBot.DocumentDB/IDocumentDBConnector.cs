using System.Linq;
using System.Threading.Tasks;
using WeyhdBot.DocumentDB.Model;

namespace WeyhdBot.DocumentDB
{
    public interface IDocumentDBConnector
    {
        /// <summary>
        /// Creates a new document in the database.
        /// </summary>
        Task CreateDocumentAsync<T>(string dbName, string collectionName, string id, T data);
        /// <summary>
        /// Creates a new document in the database or updates an existing one of the ID matches
        /// </summary>
        Task UpsertDocumentAsync<T>(string dbName, string collectionName, string id, T data);
        /// <summary>
        /// Retrieves a queryable path into a collection
        /// </summary>
        IQueryable<DocumentDBEntry<T>> CreateDocumentQuery<T>(string dbName, string collectionName);
    }
}

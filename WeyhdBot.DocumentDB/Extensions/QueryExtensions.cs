using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeyhdBot.DocumentDB.Extensions
{
    public static class QueryExtensions
    {
        public static async Task<IEnumerable<T>> AsDocumentQueryAsync<T>(this IQueryable<T> q)
        {
            var docQuery = q.AsDocumentQuery();
            var batches = new List<IEnumerable<T>>();

            while (docQuery.HasMoreResults)
            {
                var batch = await docQuery.ExecuteNextAsync<T>();
                batches.Add(batch);
            }

            return batches.SelectMany(item => item);
        }
    }
}

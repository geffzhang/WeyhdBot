using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot.Builder.Redis.Storage
{
    public class DocumentItem
    {
        /// <summary>
        /// Gets or sets the Id/Key.
        /// </summary>
        /// <value>
        /// The Id/Key.
        /// </value>
        public string RealId { get; set; }

        /// <summary>
        /// Gets or sets the persisted object's state.
        /// </summary>
        /// <value>
        /// The persisted object's state.
        /// </value>
        public JObject Document { get; set; }

        /// <summary>
        /// Gets or sets the current timestamp.
        /// </summary>
        /// <value>
        /// The current timestamp.
        /// </value>
        public DateTime Timestamp { get; set; }
    }
}

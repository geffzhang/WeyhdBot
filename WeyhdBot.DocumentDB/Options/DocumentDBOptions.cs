using System;
using System.Collections.Generic;
using System.Text;

namespace WeyhdBot.DocumentDB.Options
{
    [Serializable]
    public class DocumentDBOptions
    {
        public string Uri { get; set; }
        public string Key { get; set; }
    }
}

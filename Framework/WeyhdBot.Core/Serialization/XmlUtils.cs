using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace WeyhdBot.Core.Serialization
{
    public static class XmlUtils
    {
        static private readonly IDictionary<Type, XmlSerializer> _serializers = new Dictionary<Type, XmlSerializer>();

        public static string SerializeAsXML(object obj)
        {
            //WeChat does support the XML declaration or schemas
            var serializer = GetSerializer(obj.GetType());
            var settings = new XmlWriterSettings();

            settings.OmitXmlDeclaration = true;

            using (var sw = new StringWriter())
            using (var writer = XmlWriter.Create(sw, settings))
            {
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", "");

                serializer.Serialize(writer, obj, namespaces);
                return sw.ToString();
            }
        }

        public static T Deserialize<T>(string xml)
        {
            var serializer = GetSerializer(typeof(T));
            using (TextReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        private static XmlSerializer GetSerializer(Type type)
        {
            XmlSerializer serializer;
            if (_serializers.TryGetValue(type, out serializer))
                return serializer;

            serializer = new XmlSerializer(type);
            _serializers[type] = serializer;

            return serializer;
        }
    }
}

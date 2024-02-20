using System;

namespace Colosoft.Reflection
{
    public static class ReflectionNamespace
    {
        public const string Data = "http://colosoft.com.br/2013/webservices/reflection";
        public const string SchemaInstance = "http://www.w3.org/2001/XMLSchema-instance";

        private static System.Xml.Schema.XmlSchema typeNameSchema;

        public static System.Xml.Schema.XmlSchema ReflectionSchema
        {
            get
            {
                if (typeNameSchema == null)
                {
                    var path = "Colosoft.Xsd.Reflection.xsd";
                    System.Xml.Schema.XmlSchema schema = null;

                    var schemaSerializer = new System.Xml.Serialization.XmlSerializer(typeof(System.Xml.Schema.XmlSchema));

                    using (var stream = typeof(ReflectionNamespace).Assembly.GetManifestResourceStream(path))
                    {
                        if (stream == null)
                        {
                            return null;
                        }

                        using (var reader = new System.Xml.XmlTextReader(stream))
                        {
                            schema = (System.Xml.Schema.XmlSchema)schemaSerializer.Deserialize(reader, null);
                        }

                        typeNameSchema = schema;
                    }
                }

                return typeNameSchema;
            }
        }

        public static void ResolveReflectionSchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            if (xs is null)
            {
                throw new ArgumentNullException(nameof(xs));
            }

            var querySchema = ReflectionSchema;

            if (!xs.Contains(querySchema))
            {
                xs.XmlResolver = new System.Xml.XmlUrlResolver();
                xs.Add(querySchema);
            }
        }
    }
}

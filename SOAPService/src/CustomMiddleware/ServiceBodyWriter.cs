using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml;

namespace CustomMiddleware
{
    public class ServiceBodyWriter : BodyWriter
    {
        string ServiceNamespace;
        string EnvelopeName;
        string ResultName;
        object Result;

        public ServiceBodyWriter(string serviceNamespace, string envelopeName, string resultName, object result) : base(isBuffered: true)
        {
            ServiceNamespace = serviceNamespace;
            EnvelopeName = envelopeName;
            ResultName = resultName;
            Result = result;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            writer.WriteStartElement(EnvelopeName, ServiceNamespace);
            var serializer = new DataContractSerializer(Result.GetType(), ResultName, ServiceNamespace);
            serializer.WriteObject(writer, Result);
            writer.WriteEndElement();
        }
    }
}

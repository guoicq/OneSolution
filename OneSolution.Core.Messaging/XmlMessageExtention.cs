using System;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;

namespace OneSolution.ServiceBus
{
    public class ServiceBusBroadcaster<T>
    {
        [DataContract]
        public class MessageEnvelope
        {
            [DataMember]
            public string OwnerId;
            [DataMember]
            public T Body;
        }
    }

    public static class XmlMessageExtention
    {

        public static T ParseXmlMessage<T>(this Message message)
        {
            var reader = XmlDictionaryReader.CreateBinaryReader(message.Body, new XmlDictionaryReaderQuotas());
            var binarySerializer = DataContractBinarySerializer<ServiceBusBroadcaster<T>.MessageEnvelope>.Instance;
            var brokerMessage = (ServiceBusBroadcaster<T>.MessageEnvelope)binarySerializer.ReadObject(reader);
            var payload = brokerMessage.Body;
            return payload;
        }
    }
}
using System;

namespace OneSolution.ServiceBus.MessageTypes
{

    public class ReleaseNotifierMessage
    {
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid DataId { get; set; }

    }

}
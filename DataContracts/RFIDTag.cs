using System;
using System.Runtime.Serialization;

namespace RFID.DataContracts
{
    [DataContract]
    public class RFIDTag
    {
        [DataMember]
        public string TagId { get; private set; }

        [DataMember]
        public string Product { get; set; }

        [DataMember]
        public double Price { get; set; }

        public RFIDTag()
        {
            TagId = Guid.NewGuid().ToString();
        }
    }
}

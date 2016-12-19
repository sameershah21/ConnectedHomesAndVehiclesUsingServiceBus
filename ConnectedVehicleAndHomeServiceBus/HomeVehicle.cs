using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ConnectedVehicleAndHomeServiceBus
{
    [DataContract]
    public class HomeVehicle
    {
        [DataMember(Order = 1)]
        public string ID { get; set; }
        [DataMember(Order = 2)]
        public string Name { get; set; }
        [DataMember(Order = 3)]
        public string Type { get; set; }
        [DataMember(Order = 4)]
        public string Tag { get; set; }
        [DataMember(Order = 5)]
        public string theftDetected { get; set; }
        [DataMember(Order = 6)]
        public string temperature { get; set; }

      public   HomeVehicle()
        {
            this.Tag = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return String.Format("{0};{1};{2};{3};{4}", ID, Name, Type, theftDetected, temperature);
        }

    }
}

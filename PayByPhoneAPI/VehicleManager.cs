using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayByPhoneAPI.Items;

namespace PayByPhoneAPI
{
    class VehicleManager
    {
        private PayByPhoneAPI myAPI;

        public List<Vehicle> Vehicles { get; set; }

        public VehicleManager(PayByPhoneAPI api)
        {
            myAPI = api;
            Vehicles = new List<Items.Vehicle>();
        }

        public bool LoadVehicles()
        {
            return true;
        }

        public bool CreateVehicle(Vehicle vehicle)
        {
            return true;
        }

        public bool UpdateVehicle(Vehicle vehicle)
        {
            return true;
        }

        public bool DeleteVehicle(Vehicle vehicle)
        {
            return true;
        }
    }
}

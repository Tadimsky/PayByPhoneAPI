using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayByPhoneAPI.Items;
using System.Collections.Specialized;

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

        public async Task<bool> LoadVehicles()
        {
            List<Items.Vehicle> loadedVehicles = new List<Items.Vehicle>();
            Vehicles.Clear();

            await myAPI.CallAPI("OtherOptions.aspx", false);

            NameValueCollection info = new NameValueCollection();
            info.Add("__EVENTTARGET", "ctl00$ContentPlaceHolder1$EditVehiclesButton");
            var doc = await myAPI.CallAPI("OtherOptions.aspx", true, info);

            var editVehiclesTable = doc.GetElementbyId("ctl00_ContentPlaceHolder1_EditVehiclesGridView");
            if (editVehiclesTable != null)
            {
                var vehicleRows = editVehiclesTable.SelectNodes("tr");
                foreach (var vehicle in vehicleRows)
                {
                    Items.Vehicle newVehicle = Items.Vehicle.Parse(vehicle);
                    if (newVehicle != null)
                    {
                        loadedVehicles.Add(newVehicle);
                    }
                }
            }
            else
            {
                return false;
            }

            Vehicles.AddRange(loadedVehicles);

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

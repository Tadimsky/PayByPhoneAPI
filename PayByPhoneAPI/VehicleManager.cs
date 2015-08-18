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

        private bool updatedList;
        
        public VehicleManager(PayByPhoneAPI api)
        {
            myAPI = api;
            Vehicles = new List<Items.Vehicle>();
            updatedList = false;
        }

        private bool ShouldUpdate()
        {
            return !updatedList;
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
            Vehicles.Sort((a, b) => a.LicensePlate.CompareTo(b.LicensePlate));
            //updatedList = true;

            return true;
        }

        public async Task<bool> CreateVehicle(Vehicle vehicle)
        {
            if (vehicle == null)
            {
                return false;
            }

            if (ShouldUpdate())
            {
                // update vehicles first
                await this.LoadVehicles();
            }

            // check that vehicle does not exist

            var sortedVehicles = new List<Vehicle>(Vehicles);
            sortedVehicles.Sort((a, b) => a.WebData.LicensePlateHiddenField.CompareTo(b.WebData.LicensePlateHiddenField));

            if (sortedVehicles.Find(v => v.LicensePlate.Equals(vehicle.LicensePlate)) == null)
            {
                // does not exist
                // find the last one that exists
                var lastVehicle = sortedVehicles.Last();

                VehicleWebData vwd = VehicleWebData.NextIncrement(lastVehicle.WebData);
            }
            else
            {
                return false;
            }

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

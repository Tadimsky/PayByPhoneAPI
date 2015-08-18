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
        private const int MaximumVehicles = 4;

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
            Vehicles.Sort((a, b) => a.WebData.LicensePlateHiddenField.CompareTo(b.WebData.LicensePlateHiddenField));
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

            if (Vehicles.Count >= MaximumVehicles)
            {
                // cannot create more cars, can we?
                return false;
            }

            // check that vehicle does not exist

            if (Vehicles.Find(v => v.LicensePlate.Equals(vehicle.LicensePlate)) == null)
            {
                // does not exist
                // find the last one that exists
                var lastVehicle = Vehicles.Last();
                VehicleWebData vwd = VehicleWebData.NextIncrement(lastVehicle.WebData);
                // have new web data for new vehicle
                vehicle.WebData = vwd;

                // now we have all the vehicles we want
                // post everything to server
                return await UploadVehicles();
            }
            else
            {
                return false;
            }

            return true;
        }

        private async Task<bool> UploadVehicles()
        {            
            NameValueCollection allFields = new NameValueCollection();
            allFields.Add("ctl00$ContentPlaceHolder1$UpdateButton", "update");

            foreach (Vehicle v in Vehicles)
            {
                allFields.Add(v.WebFormData);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PayByPhoneAPI.Items;
using System.Collections.Specialized;

namespace PayByPhoneAPI
{
    class VehicleManager : ApiSection
    {
        private int _myMaximumVehicles = 4;
        
        public List<Vehicle> Vehicles { get; set; }

        private bool _updatedList;
        
        public VehicleManager(PayByPhoneApi api) : base(api)
        {            
            Vehicles = new List<Items.Vehicle>();
            _updatedList = false;
        }

        private bool ShouldUpdate()
        {
            return !_updatedList;
        }

        public async Task<bool> LoadVehicles()
        {
            List<Items.Vehicle> loadedVehicles = new List<Items.Vehicle>();
            Vehicles.Clear();

            var doc = await this.LoadOptions(Sections.Button.Vehicles);

            var editVehiclesTable = doc.GetElementbyId(FormInputNames.VehicleDetails.VehicleTable);
            if (editVehiclesTable != null)
            {
                var vehicleRows = editVehiclesTable.SelectNodes("tr");
                // max cars we can deal with right now is vehicleRows - 1
                this._myMaximumVehicles = vehicleRows.Count - 1;
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

            if (Vehicles.Count >= _myMaximumVehicles)
            {
                // cannot create more cars, can we?
                return false;
            }

            // check that vehicle does not exist

            if (Vehicles.Find(v => v.LicensePlate.Equals(vehicle.LicensePlate, StringComparison.OrdinalIgnoreCase)) == null)
            {
                // does not exist
                // find the last one that exists
                var lastVehicle = Vehicles.Last();
                VehicleWebData vwd = VehicleWebData.NextIncrement(lastVehicle.WebData);
                // have new web data for new vehicle
                vehicle.WebData = vwd;

                Vehicles.Add(vehicle);

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
            allFields.Add(FormInputNames.VehicleDetails.UpdateButton, FormInputNames.VehicleDetails.UpdateButtonValue);

            foreach (Vehicle v in Vehicles)
            {
                allFields.Add(v.WebFormData);
            }

            // this is the last vehicle - could be the new one
            var lastVehicle = Vehicles.Last();
            
            // fill up missing items with blank data
            for (int blankVehicles = Vehicles.Count; blankVehicles < _myMaximumVehicles; blankVehicles++)
            {
                Vehicle v = new Vehicle();
                v.WebData = VehicleWebData.NextIncrement(lastVehicle.WebData);
                lastVehicle = v;
                allFields.Add(v.WebFormData);
            }

            var doc = await this.LoadOptions(Sections.Button.Vehicles);

            // upload our current info
            // hopefully no changes in here
            doc = await MyApi.CallApi("EditVehicles.aspx", true, allFields);

            PayByPhoneApi.VerifyMessage(doc);

            return true;
        }

        public async Task<bool> UpdateVehicle(Vehicle vehicle)
        {
            // something changed
            // we don't change the id or the hidden license plate
            return await UploadVehicles();            
        }

        public async Task<bool> DeleteVehicle(Vehicle vehicle)
        {
            // simply clear the license plate data
            vehicle.LicensePlate = "";
            return await this.UploadVehicles();         
        }
    }

    namespace FormInputNames
    {
        static class VehicleDetails
        {
            public const string VehicleTable = "ctl00_ContentPlaceHolder1_EditVehiclesGridView";
            public const string UpdateButton = "ctl00$ContentPlaceHolder1$UpdateButton";
            public const string UpdateButtonValue = "update";
        }
    }
}

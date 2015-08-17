using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayByPhoneAPI
{
    namespace Items {
        class Vehicle
        {
            private string myLicensePlate;
            private VehicleType myType;

            public string LicensePlate
            {
                get
                {
                    return myLicensePlate;
                }

                set
                {
                    myLicensePlate = value;
                }
            }

            public VehicleType Type
            {
                get
                {
                    return myType;
                }

                set
                {
                    myType = value;
                }
            }
        }
        public enum VehicleType
        {
            Car = 1,
            Motorcycle = 2, 
            ElectricMotorcycle = 3,
            HeavyGoodsVehicle = 4
        }
    }
    
}

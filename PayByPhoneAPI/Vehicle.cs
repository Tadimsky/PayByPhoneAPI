using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PayByPhoneAPI
{
    namespace Items {
        class Vehicle
        {
            private string myLicensePlate;
            private VehicleType myType;
            private string myVehicleID;
            public VehicleWebData WebData { get; set; }
            private bool isFromWebsite;

            public Vehicle()
            {
                myLicensePlate = "";
                myType = VehicleType.Car;
                myVehicleID = "0";
                WebData = new VehicleWebData();
                isFromWebsite = false;
            }

            public static Vehicle Parse(HtmlAgilityPack.HtmlNode htmlData)
            {
                var vehicleInput = htmlData.SelectSingleNode(".//input[@type='text']/@value");
                if (vehicleInput == null)
                {
                    // not a vehicle row
                    return null;
                }
                return new Vehicle(htmlData);
            }

            private Vehicle(HtmlAgilityPack.HtmlNode htmlData)
            {
                // parse the html node to get the info out

                var vehicleInput = htmlData.SelectSingleNode(".//input[@type='text']/@value");
                string licensePlate = vehicleInput?.Attributes["value"].Value;                

                var vehicleType = htmlData.SelectSingleNode(".//select/option[@selected='selected']/@value");
                string vehicleTypeVal = vehicleType?.Attributes["value"].Value;

                var hiddenData = htmlData.SelectNodes(".//input[@type='hidden']");
                var hiddenIDInputCol = hiddenData.Where(input => input.Attributes["name"]?.Value.Contains("VehicleUid") == true);
                var hiddenIDInput = hiddenIDInputCol.First();
                var hiddenID = hiddenIDInput?.Attributes["value"]?.Value;                


                myType = (VehicleType)int.Parse(vehicleTypeVal);
                myLicensePlate = licensePlate;
                myVehicleID = hiddenID;

                WebData = new VehicleWebData(htmlData);
                isFromWebsite = true;
            }


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

            public string VehicleID
            {
                get
                {
                    return myVehicleID;
                }

                set
                {
                    myVehicleID = value;
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

            public NameValueCollection WebFormData
            {
                get
                {
                    NameValueCollection nvc = new NameValueCollection();
                    nvc.Add(WebData.LicensePlateTextBox, myLicensePlate);
                    nvc.Add(WebData.VehicleUIDHiddenField, myVehicleID);
                    nvc.Add(WebData.LicensePlateHiddenField, isFromWebsite ? myLicensePlate : "");
                    nvc.Add(WebData.VehicleTypeHiddenField, myType.ToString());
                    nvc.Add(WebData.VehicleTypeDropDown, myType.ToString());
                    return nvc;
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

        class VehicleWebData
        {
            public string LicensePlateTextBox { get; set; }
            public string VehicleUIDHiddenField { get; set; }
            public string LicensePlateHiddenField { get; set; }
            public string VehicleTypeHiddenField { get; set; }
            public string VehicleTypeDropDown { get; set; }

            public VehicleWebData()
            {
            }

            public static VehicleWebData NextIncrement(VehicleWebData previous)
            {
                var oldInfo = previous.LicensePlateTextBox;
                Regex matcher = new Regex(@"(\S+)(\$ctl)(\d+)(\S+)");
                var matches = matcher.Match(oldInfo);
                var editVal = (matches.Groups.Count == 5) ? matches.Groups[3]?.Value : "";

                int number = int.Parse(editVal);
                number++;

                MatchEvaluator incrementer = m => m.Groups[1].Value + m.Groups[2].Value + number.ToString("00") + m.Groups[4].Value;

                VehicleWebData newData = new VehicleWebData();
                newData.LicensePlateTextBox = matcher.Replace(previous.LicensePlateTextBox, incrementer);
                newData.LicensePlateHiddenField = matcher.Replace(previous.LicensePlateHiddenField, incrementer);
                newData.VehicleUIDHiddenField = matcher.Replace(previous.VehicleUIDHiddenField, incrementer);
                newData.VehicleTypeHiddenField = matcher.Replace(previous.VehicleTypeHiddenField, incrementer);
                newData.VehicleTypeDropDown= matcher.Replace(previous.VehicleTypeDropDown, incrementer);

                return newData;
            }

            public VehicleWebData(HtmlAgilityPack.HtmlNode vehicleData)
            {
                var allInputs = vehicleData.SelectNodes(".//input");
                var select = vehicleData.SelectSingleNode(".//select");

                LicensePlateTextBox = allInputs.Where(input => input.Attributes["name"]?.Value?.Contains("LicensePlateTextBox") == true).First()?.GetAttributeValue("name", "");
                LicensePlateHiddenField = allInputs.Where(input => input.Attributes["name"]?.Value?.Contains("LicensePlateHiddenField") == true).First()?.GetAttributeValue("name", "");
                VehicleUIDHiddenField = allInputs.Where(input => input.Attributes["name"]?.Value?.Contains("VehicleUidHiddenField") == true).First()?.GetAttributeValue("name", "");
                VehicleTypeHiddenField = allInputs.Where(input => input.Attributes["name"]?.Value?.Contains("VehicleTypeUidHiddenField") == true).First()?.GetAttributeValue("name", "");
                VehicleTypeDropDown = select?.GetAttributeValue("name", "");
            }
        }
    }
    
}

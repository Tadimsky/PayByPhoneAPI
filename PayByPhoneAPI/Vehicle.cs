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
            private string _myLicensePlate;
            private VehicleType _myType;
            private string _myVehicleId;
            private string _myHiddenLicensePlate;

            public VehicleWebData WebData { get; set; }
            private bool _isFromWebsite;

            public Vehicle()
            {
                _myLicensePlate = "";
                _myHiddenLicensePlate = "";
                _myType = VehicleType.Car;
                _myVehicleId = "0";
                WebData = new VehicleWebData();
                _isFromWebsite = false;
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

                string licensePlate = htmlData.SelectSingleNode(".//input[@type='text']/@value")?.GetAttributeValue("value", "");

                string vehicleTypeVal = htmlData.SelectSingleNode(".//select/option[@selected='selected']/@value")?.GetAttributeValue("value", "");                

                var hiddenData = htmlData.SelectNodes(".//input[@type='hidden']");
                var hiddenId = hiddenData.Where(input => input.Attributes["name"]?.Value.Contains("VehicleUid") == true)?.First()?.GetAttributeValue("value", "");
                var hiddenLicPlate = hiddenData.Where(input => input.Attributes["name"]?.Value.Contains("LicensePlateHiddenField") == true)?.First()?.GetAttributeValue("value", "");

                _myType = (VehicleType)int.Parse(vehicleTypeVal);
                _myLicensePlate = licensePlate;
                _myHiddenLicensePlate = hiddenLicPlate;
                _myVehicleId = hiddenId;

                WebData = new VehicleWebData(htmlData);
                _isFromWebsite = true;
            }


            public string LicensePlate
            {
                get
                {
                    return _myLicensePlate;
                }

                set
                {
                    _myLicensePlate = value;
                }
            }

            public string VehicleId
            {
                get
                {
                    return _myVehicleId;
                }

                set
                {
                    _myVehicleId = value;
                }
            }

            public VehicleType Type
            {
                get
                {
                    return _myType;
                }

                set
                {
                    _myType = value;
                }
            }

            public NameValueCollection WebFormData
            {
                get
                {
                    string vehicleType = _isFromWebsite ? String.Format("{0}", (int)_myType) : "1";
                    NameValueCollection nvc = new NameValueCollection();
                    nvc.Add(WebData.LicensePlateTextBox, _myLicensePlate);
                    nvc.Add(WebData.VehicleUidHiddenField, _myVehicleId);
                    // if this is a new vehicle / not loaded from website, do not know the hidden lic plate or hidden type
                    nvc.Add(WebData.LicensePlateHiddenField, _isFromWebsite ? _myHiddenLicensePlate : "");
                    nvc.Add(WebData.VehicleTypeHiddenField, vehicleType);

                    nvc.Add(WebData.VehicleTypeDropDown, vehicleType);
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
            public string VehicleUidHiddenField { get; set; }
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
                newData.VehicleUidHiddenField = matcher.Replace(previous.VehicleUidHiddenField, incrementer);
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
                VehicleUidHiddenField = allInputs.Where(input => input.Attributes["name"]?.Value?.Contains("VehicleUidHiddenField") == true).First()?.GetAttributeValue("name", "");
                VehicleTypeHiddenField = allInputs.Where(input => input.Attributes["name"]?.Value?.Contains("VehicleTypeUidHiddenField") == true).First()?.GetAttributeValue("name", "");
                VehicleTypeDropDown = select?.GetAttributeValue("name", "");
            }
        }
    }
    
}

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;

namespace PayByPhoneAPI
{
    class PayByPhoneAPI
    {
        private string myViewState;
        private string myEventValidation;
        private string myViewStateGenerator;

        private WebClient myWebClient;
        

        public PayByPhoneAPI()
        {
            myWebClient = new CookieWebClient();
            myWebClient.BaseAddress = "https://m.paybyphone.com";
            myWebClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            myWebClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.130 Safari/537.36";
            myWebClient.Headers["Origin"] = "https://m.paybyphone.com";
        }


        public bool Login(string Username, string Password)
        {
            // fetch the view state and whatnot first
            CallAPI("Default.aspx", false);

            NameValueCollection info = new NameValueCollection();
            info.Add("ctl00$ContentPlaceHolder1$CallingCodeDropDownList", "-1");
            info.Add("ctl00$ContentPlaceHolder1$AccountTextBox", Username);
            info.Add("ctl00$ContentPlaceHolder1$PinOrLast4DigitCcTextBox", Password);
            info.Add("ctl00$ContentPlaceHolder1$LoginButton", "sign+in");
            info.Add("ctl00$ContentPlaceHolder1$RememberPinCheckBox", "on");

            var doc = CallAPI("Default.aspx", true, info);

            var form = doc.GetElementbyId("aspnetForm");            
            if (form != null)
            {
                HtmlAttribute action = form.Attributes["action"];
                if (action != null)
                {
                    if (action.Value.Equals("ChooseLocation.aspx"))
                    {
                        // definitely logged in
                        return true;
                    }
                    else
                    {
                        // back on login page
                        // check error message
                        var messageTable = doc.DocumentNode.SelectSingleNode("//table[@id='MessageTable']");
                        var messages = messageTable.SelectNodes("//userlog");
                        foreach (var message in messages)
                        {
                            Console.WriteLine(message.InnerText);
                        }
                    }
                }
            }
            return false;
        }

        public bool Logout()
        {
            NameValueCollection values = new NameValueCollection();
            values.Add("__EVENTTARGET", "ctl00$ContentPlaceHolder1$LogOutButton");
            var doc = CallAPI("OtherOptions.aspx", true, values);

            // can you fail a logout?
            return true;           
        }

        public List<Items.Vehicle> GetVehicles()
        {
            List<Items.Vehicle> myVehicles = new List<Items.Vehicle>();

            CallAPI("OtherOptions.aspx", false);

            NameValueCollection info = new NameValueCollection();
            info.Add("__EVENTTARGET", "ctl00$ContentPlaceHolder1$EditVehiclesButton");
            var doc = CallAPI("OtherOptions.aspx", true, info);

            var editVehiclesTable = doc.GetElementbyId("ctl00_ContentPlaceHolder1_EditVehiclesGridView");
            if (editVehiclesTable != null)
            {
                var vehicleRows = editVehiclesTable.SelectNodes("tr");
                foreach (var vehicle in vehicleRows)
                {
                    var vehicleInput = vehicle.SelectSingleNode(".//input[@type='text']/@value");
                    if (vehicleInput == null)
                    {
                        // not a vehicle row
                        continue;
                    }
                    var licenseAttrib = vehicleInput.Attributes["value"];
                    string licensePlate = licenseAttrib.Value;

                    string vehicleTypeVal = "";

                    var vehicleType = vehicle.SelectSingleNode(".//select/option[@selected='selected']/@value");
                    if (vehicleType != null)
                    {
                        var selectedAttrib = vehicleType.Attributes["value"];
                        vehicleTypeVal = selectedAttrib.Value;
                    }

                    if (!String.IsNullOrEmpty(vehicleTypeVal) && !String.IsNullOrEmpty(licensePlate))
                    {
                        Items.VehicleType vType = (Items.VehicleType)int.Parse(vehicleTypeVal);
                        Items.Vehicle newVehicle = new Items.Vehicle {
                            LicensePlate = licensePlate,
                            Type = vType
                        };
                        myVehicles.Add(newVehicle);
                    }
                }
            }

            return myVehicles;
        }

        private HtmlDocument CallAPI(string url, bool post = true, NameValueCollection content = null)
        {   
            string response;

            if (post)
            {
                if (content == null)
                {
                    content = new NameValueCollection();
                }
                if (content.Get("__EVENTTARGET") == null)
                {
                    content.Add("__EVENTARGUMENT", "");
                }
                
                content.Add("__VIEWSTATE", myViewState);
                content.Add("__VIEWSTATEGENERATOR", myViewStateGenerator);
                content.Add("__EVENTVALIDATION", myEventValidation);

                var result = myWebClient.UploadValues(url, "POST", content);
                response = Encoding.UTF8.GetString(result);
            }
            else
            {
                response = myWebClient.DownloadString(url);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            processState(doc);

            return doc;
        }


        private void processState(HtmlDocument html)
        {
            var viewState = html.GetElementbyId("__VIEWSTATE");
            int countChanges = 0;

            if (viewState != null)
            {
                var value = viewState.GetAttributeValue("value", "");
                myViewState = value;
                countChanges++;
            }
            var eventValidation = html.GetElementbyId("__EVENTVALIDATION");
            if (eventValidation != null)
            {
                var value = eventValidation.GetAttributeValue("value", "");
                myEventValidation = value;
                countChanges++;
            }

            var viewStateGenerator = html.GetElementbyId("__VIEWSTATEGENERATOR");
            if (viewStateGenerator != null)
            {
                var value = viewStateGenerator.GetAttributeValue("value", "");
                myViewStateGenerator = value;
                countChanges++;
            }
            

            if (countChanges != 3)
            {
                Console.WriteLine("State Changes not 2");
            }            
        }
    }
}

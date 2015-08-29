using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using PayByPhoneAPI.Constants;

namespace PayByPhoneAPI
{
    class PayByPhoneApi
    {
        private string _myViewState;
        private string _myEventValidation;
        private string _myViewStateGenerator;

        private WebClient _myWebClient;

        private VehicleManager _myVehicleManager;
        private PaymentDetails _myPaymentDetails;
        private EmailSettings _myEmailSettings;
        private SecuritySettings _mySecuritySettings;
        private TermsConditions _myTermsConditions;
        private LocationManager _myLocationManager;
        

        public PayByPhoneApi()
        {
            _myWebClient = new CookieWebClient();
            _myWebClient.BaseAddress = "https://m.paybyphone.com";
            _myWebClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            _myWebClient.Headers[HttpRequestHeader.UserAgent] = Api.UserAgent;
            _myWebClient.Headers["Origin"] = "https://m.paybyphone.com";

            _myVehicleManager = new VehicleManager(this);
            _myPaymentDetails = new PaymentDetails(this);
            _myEmailSettings = new EmailSettings(this);
            _mySecuritySettings = new SecuritySettings(this);
            _myTermsConditions = new TermsConditions(this);
            _myLocationManager = new LocationManager(this);
        }


        public async Task<bool> Login(string username, string password)
        {
            // fetch the view state and whatnot first
            await CallApi("Default.aspx", false);

            NameValueCollection info = new NameValueCollection();
            info.Add("ctl00$ContentPlaceHolder1$CallingCodeDropDownList", "-1");
            info.Add("ctl00$ContentPlaceHolder1$AccountTextBox", username);
            info.Add("ctl00$ContentPlaceHolder1$PinOrLast4DigitCcTextBox", password);
            info.Add("ctl00$ContentPlaceHolder1$LoginButton", "sign+in");
            info.Add("ctl00$ContentPlaceHolder1$RememberPinCheckBox", "on");

            var doc = await CallApi("Default.aspx", true, info);

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
            values.Add(Constants.Api.EventTarget, "ctl00$ContentPlaceHolder1$LogOutButton");
            var doc = CallApi("OtherOptions.aspx", true, values);

            // can you fail a logout?
            // TODO: clear cookies for fun

            return true;           
        }

        public async Task<List<Items.Vehicle>> GetVehicles()
        {
            await _myVehicleManager.LoadVehicles();
            return _myVehicleManager.Vehicles;
        }        

        public async Task<bool> CreateVehicle(Items.Vehicle vehicle)
        {
            return await _myVehicleManager.CreateVehicle(vehicle);
        }

        public async Task<bool> UpdateVehicle(Items.Vehicle vehicle)
        {
            return await _myVehicleManager.UpdateVehicle(vehicle);
        }

        public async Task<bool> DeleteVehicle(Items.Vehicle vehicle)
        {
            return await _myVehicleManager.DeleteVehicle(vehicle);
        }

        public async Task<bool> UploadCard(Items.CreditCard creditcard)
        {
            return await _myPaymentDetails.SaveCard(creditcard);
        }

        public async Task<Items.EmailSetting> GetEmailSetting()
        {
            return await _myEmailSettings.GetEmailSetting();
        }

        public async Task<bool> UpdateEmailSetting(Items.EmailSetting email)
        {
            return await _myEmailSettings.SaveEmail(email);
        }

        public async Task<Items.SecuritySetting> GetSecuritySetting()
        {
            return await _mySecuritySettings.GetSecuritySetting();
        }

        public async Task<bool> UpdateSecuritySetting(Items.SecuritySetting security)
        {
            return await _mySecuritySettings.SaveSettings(security);
        }

        public async Task<Items.TermsConditions> GetTermsAndConditions()
        {
            return await _myTermsConditions.GetTermsConditions();
        }

        public void Test()
        {
            _myLocationManager.Test();
        }

        public async Task<List<RecentLocation>> GetRecentLocations()
        {
            await _myLocationManager.LoadLocations();
            return _myLocationManager.RecentLocations;
        }

        public async Task<HtmlDocument> CallApi(string url, bool post = true, NameValueCollection content = null)
        {   
            string response;

            if (post)
            {
                if (content == null)
                {
                    content = new NameValueCollection();
                }
                if (content.Get(Constants.Api.EventTarget) == null)
                {
                    content.Add(Api.EventArgument, "");
                }
                
                content.Add(Api.ViewState, _myViewState);
                content.Add(Api.ViewStateGenerator, _myViewStateGenerator);
                content.Add(Api.EventValidation, _myEventValidation);
                
                var result = await _myWebClient.UploadValuesTaskAsync(url, "POST", content);
                response = Encoding.UTF8.GetString(result);
            }
            else
            {
                response = await _myWebClient.DownloadStringTaskAsync(url);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            ProcessState(doc);

            return doc;
        }

        public async Task<LocationResult> SelectLocation(SearchLocation location)
        {
            return await _myLocationManager.SelectLocation(location);
        }

        public static bool VerifyMessage(HtmlDocument document, string text = "Details updated.")
        {
            var receivedMessage = "";
            foreach (var log in document.DocumentNode.SelectNodes("//userlog"))
            {
                receivedMessage = log.InnerText;
                if (receivedMessage.Equals(text))
                {
                    return true;
                }

            }
            throw new UnexpectedResponseException(text, receivedMessage);
        }

        private void ProcessState(HtmlDocument html)
        {
            var viewState = html.GetElementbyId(Api.ViewState);
            int countChanges = 0;

            if (viewState != null)
            {
                var value = viewState.GetAttributeValue("value", "");
                _myViewState = value;
                countChanges++;
            }
            var eventValidation = html.GetElementbyId(Api.EventValidation);
            if (eventValidation != null)
            {
                var value = eventValidation.GetAttributeValue("value", "");
                _myEventValidation = value;
                countChanges++;
            }

            var viewStateGenerator = html.GetElementbyId(Api.ViewStateGenerator);
            if (viewStateGenerator != null)
            {
                var value = viewStateGenerator.GetAttributeValue("value", "");
                _myViewStateGenerator = value;
                countChanges++;
            }
            

            if (countChanges != 3)
            {
                Console.WriteLine("State Changes not 3 - why?");
            }            
        }
    }

    class UnexpectedResponseException : Exception
    {
        public string ExpectedMessage { get; private set; }
        public string ReceivedMessage { get; private set; }

        public UnexpectedResponseException(string expected, string actual)
        {
            ExpectedMessage = expected;
            ReceivedMessage = actual;
        }

        public override string ToString()
        {
            return $"Received Response: {ReceivedMessage}\nExpected: {ExpectedMessage}";
        }
    }

    namespace Constants
    {
        public static class Api
        {
            public const string EventTarget = "__EVENTTARGET";
            public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.130 Safari/537.36";
            public const string ViewState = "__VIEWSTATE";
            public const string EventValidation = "__EVENTVALIDATION";
            public const string ViewStateGenerator = "__VIEWSTATEGENERATOR";
            public const string EventArgument = "__EVENTARGUMENT";
        }
    }
}

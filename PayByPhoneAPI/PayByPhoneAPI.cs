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

        private VehicleManager myVehicleManager;
        private PaymentDetails myPaymentDetails;
        

        public PayByPhoneAPI()
        {
            myWebClient = new CookieWebClient();
            myWebClient.BaseAddress = "https://m.paybyphone.com";
            myWebClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            myWebClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.130 Safari/537.36";
            myWebClient.Headers["Origin"] = "https://m.paybyphone.com";

            myVehicleManager = new VehicleManager(this);
            myPaymentDetails = new PaymentDetails(this);
        }


        public async Task<bool> Login(string Username, string Password)
        {
            // fetch the view state and whatnot first
            await CallAPI("Default.aspx", false);

            NameValueCollection info = new NameValueCollection();
            info.Add("ctl00$ContentPlaceHolder1$CallingCodeDropDownList", "-1");
            info.Add("ctl00$ContentPlaceHolder1$AccountTextBox", Username);
            info.Add("ctl00$ContentPlaceHolder1$PinOrLast4DigitCcTextBox", Password);
            info.Add("ctl00$ContentPlaceHolder1$LoginButton", "sign+in");
            info.Add("ctl00$ContentPlaceHolder1$RememberPinCheckBox", "on");

            var doc = await CallAPI("Default.aspx", true, info);

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
            // TODO: clear cookies for fun

            return true;           
        }

        public async Task<List<Items.Vehicle>> GetVehicles()
        {
            await myVehicleManager.LoadVehicles();
            return myVehicleManager.Vehicles;
        }

        

        public async Task<bool> CreateVehicle(Items.Vehicle vehicle)
        {
            return await myVehicleManager.CreateVehicle(vehicle);
        }

        public async Task<bool> UpdateVehicle(Items.Vehicle vehicle)
        {
            return await myVehicleManager.UpdateVehicle(vehicle);
        }

        public async Task<bool> DeleteVehicle(Items.Vehicle vehicle)
        {
            return await myVehicleManager.DeleteVehicle(vehicle);
        }

        public async Task<bool> UploadCard(Items.CreditCard creditcard)
        {
            return await myPaymentDetails.SaveCard(creditcard);
        }


        public async Task<HtmlDocument> CallAPI(string url, bool post = true, NameValueCollection content = null)
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
                
                var result = await myWebClient.UploadValuesTaskAsync(url, "POST", content);
                response = Encoding.UTF8.GetString(result);
            }
            else
            {
                response = await myWebClient.DownloadStringTaskAsync(url);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            processState(doc);

            return doc;
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
            return String.Format("Received Response: {0}\nExpected: {1}", ReceivedMessage, ExpectedMessage);
        }
    }
}

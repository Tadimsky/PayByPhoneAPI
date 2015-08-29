using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayByPhoneAPI
{
    class ApiSection
    {
        protected PayByPhoneApi MyApi;
        public ApiSection(PayByPhoneApi api)
        {
            MyApi = api;
        }

        protected async Task<HtmlAgilityPack.HtmlDocument> LoadOptions(string optionTarget)
        {
            // load the options page
            await MyApi.CallApi("OtherOptions.aspx", false);

            // send request to get to card page
            NameValueCollection info = new NameValueCollection();
            info.Add("__EVENTTARGET", optionTarget);
            var doc = await MyApi.CallApi("OtherOptions.aspx", true, info);

            // return the card info
            return doc;
        }
    }

    namespace Sections
    {
        static class Button
        {
            public const string PaymentDetails = "ctl00$ContentPlaceHolder1$PaymentDetailsButton";
            public const string Vehicles = "ctl00$ContentPlaceHolder1$EditVehiclesButton";
            public const string EmailSettings = "ctl00$ContentPlaceHolder1$TextEmailSettingsButton";
            public const string SecuritySettings = "ctl00$ContentPlaceHolder1$SecuritySettingsButton";
            public const string TermsConditions = "ctl00$ContentPlaceHolder1$TermsAndConditionsButton";
        }
    }
}

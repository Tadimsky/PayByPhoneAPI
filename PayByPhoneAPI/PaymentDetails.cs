using PayByPhoneAPI.Items;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayByPhoneAPI
{
    class PaymentDetails : ApiSection
    {
       public PaymentDetails(PayByPhoneApi api) : base(api)
        {
        }

        private async Task<HtmlAgilityPack.HtmlDocument> LoadCardPage()
        {
            return await LoadOptions(Sections.Button.PaymentDetails);
        }

        public async Task<bool> SaveCard(CreditCard card)
        {
            return await this.UploadCard(card);
        }

        private async Task<bool> UploadCard(CreditCard card)
        {
            await this.LoadCardPage();

            // post the data to the info
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add(card.WebFormData);
            nvc.Add(FormInputNames.PaymentDetails.UpdateButton, FormInputNames.PaymentDetails.UpdateButtonValue);

            var doc = await MyApi.CallApi("PaymentDetails.aspx", true, nvc);
            try {
                PayByPhoneApi.VerifyMessage(doc);
            } 
            catch (UnexpectedResponseException ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }
    }
    namespace Items
    {
        class CreditCard
        {
            public string Number { get; set; }
            public string ExpiryMonth{ get; set; }
            public string ExpiryYear{ get; set; }
            public string Name{ get; set; }

            public CreditCard()
            {
                Name = "";
                Number = "";                
                ExpiryMonth = DateTime.Today.Month.ToString("00");
                ExpiryYear = DateTime.Today.Year.ToString("0000");
            }

            public CreditCard(HtmlAgilityPack.HtmlNode vehicleData)
            {
                // process the data to get the credit card information
                // cannot load data from website currently
            }

            public NameValueCollection WebFormData
            {
                get
                {
                    NameValueCollection nvc = new NameValueCollection();
                    nvc.Add(FormInputNames.PaymentDetails.CreditCardNumber, Number);
                    nvc.Add(FormInputNames.PaymentDetails.CreditCardExpiryMonth, ExpiryMonth);
                    nvc.Add(FormInputNames.PaymentDetails.CreditCardExpiryYear, ExpiryYear);
                    nvc.Add(FormInputNames.PaymentDetails.CreditCardNameOnCard, Name);
                    return nvc;
                }
            }
        }
    }

    namespace FormInputNames
    {
        static class PaymentDetails
        {
            public const string CreditCardNumber = "ctl00$ContentPlaceHolder1$CcNumberTextBox";
            public const string CreditCardExpiryMonth = "ctl00$ContentPlaceHolder1$CcExpiryMonthDropDownList";
            public const string CreditCardExpiryYear = "ctl00$ContentPlaceHolder1$CcExpiryYearDropDownList";
            public const string CreditCardNameOnCard = "ctl00$ContentPlaceHolder1$NameOnCardTextBox";
            public const string UpdateButton = "ctl00$ContentPlaceHolder1$UpdateButton";
            public const string UpdateButtonValue = "update";
        }
    }
}

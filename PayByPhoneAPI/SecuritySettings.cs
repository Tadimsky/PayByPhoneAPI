using PayByPhoneAPI.Items;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayByPhoneAPI
{
    class SecuritySettings : APISection
    {
       public SecuritySettings(PayByPhoneAPI api) : base(api)
        {
        }
        

        public async Task<bool> SaveEmail(EmailSetting email)
        {
            await this.loadOptions(Sections.Button.EmailSettings);

            // post the data to the info
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add(email.WebFormData);
            nvc.Add(FormInputNames.EmailSettings.UpdateButton, FormInputNames.EmailSettings.UpdateButtonValue);

            var doc = await myAPI.CallAPI("TextEmailSettings.aspx", true, nvc);
            try
            {
                PayByPhoneAPI.VerifyMessage(doc);
            }
            catch (UnexpectedResponseException ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        public async Task<EmailSetting> GetEmailSetting()
        {
            var doc = await this.loadOptions(Sections.Button.EmailSettings);
            
            // process this doc
            return new EmailSetting(doc.DocumentNode);
        }
    }
    namespace Items
    {

        class SecuritySetting
        {
            public string EmailAddress { get; set; }
            public bool EmailReceipts { get; set; }
            public bool TextReminders { get; set; }
            public string Language { get; set; }

            public SecuritySetting()
            {
                EmailAddress = "";
                EmailReceipts = false;
                TextReminders = false;
                Language = "1";
            }

            public SecuritySetting(HtmlAgilityPack.HtmlNode securityData)
            {
                // process the data to get the email settings
                
                EmailAddress = securityData.SelectSingleNode(String.Format(".//input[@name='{0}']", FormInputNames.EmailSettings.EmailAddress))?.GetAttributeValue("value", "");
                EmailReceipts = securityData.SelectSingleNode(String.Format(".//input[@name='{0}']", FormInputNames.EmailSettings.EmailRecipientsCheckBox))?.GetAttributeValue("checked", "off") == "checked" ? true : false;
                TextReminders = securityData.SelectSingleNode(String.Format(".//input[@name='{0}']", FormInputNames.EmailSettings.TextRemindersCheckBox))?.GetAttributeValue("checked", "off") == "checked" ? true : false;
                var select = securityData.SelectSingleNode(String.Format(".//select[@name='{0}']", FormInputNames.EmailSettings.DropDownPreferredLanguage));
                Language = select.SelectSingleNode(".//option[@selected='selected']")?.GetAttributeValue("value", "");
            }

            public NameValueCollection WebFormData
            {
                get
                {
                    NameValueCollection nvc = new NameValueCollection();
                    nvc.Add(FormInputNames.EmailSettings.EmailAddress, EmailAddress);
                    if (EmailReceipts)
                    {
                        nvc.Add(FormInputNames.EmailSettings.EmailRecipientsCheckBox, "on");
                    }
                    if (TextReminders)
                    {
                        nvc.Add(FormInputNames.EmailSettings.TextRemindersCheckBox, "on");
                    }
                    nvc.Add(FormInputNames.EmailSettings.DropDownPreferredLanguage, Language);
                    return nvc;
                }
            }

            public override string ToString()
            {
                return String.Format("Email: {0}\tReceipts: {1}\tReminders: {2}\nLanguage: {3}", EmailAddress, EmailReceipts, TextReminders, Language);
            }
        }
    }

    namespace FormInputNames
    {
        static class SecuritySettings
        {            
            public const string CurrentPin = "ctl00$ContentPlaceHolder1$CurrentPinTextBox";
            public const string NewPin = "ctl00$ContentPlaceHolder1$NewPinTextBox";
            public const string ConfirmPin = "ctl00$ContentPlaceHolder1$ConfirmPinTextBox";
            public const string VoiceSkipPin = "ctl00$ContentPlaceHolder1$SkipPinCheckBox";
            public const string RememberPin = "ctl00$ContentPlaceHolder1$RememberPinCheckBox";
            public const string UpdateButtonValue = "update";
        }
    }
}

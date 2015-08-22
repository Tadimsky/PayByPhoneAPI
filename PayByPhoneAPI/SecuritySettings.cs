﻿using PayByPhoneAPI.Items;
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
            public string Pin { get; private set; }
            public bool RememberPin { get; set; }
            public bool SkipVoicePin{ get; set; }

            private string oldPin;

            public SecuritySetting()
            {
                Pin = "";
                oldPin = "";
                RememberPin = false;
                SkipVoicePin = false;
            }

            public bool ChangePin(string currentPin, string newPin, string confirmPin)
            {                
                if (newPin.Equals(confirmPin))
                {
                    oldPin = Pin;
                    Pin = newPin;
                    return true;
                }
                else
                {
                    throw new Exception("Pins do not match.");
                }                        
            }

            public SecuritySetting(HtmlAgilityPack.HtmlNode securityData)
            {
                // process the data to get the security settings                                
                RememberPin = securityData.SelectSingleNode(String.Format(".//input[@name='{0}']", FormInputNames.SecuritySettings.RememberPin))?.GetAttributeValue("checked", "off") == "checked" ? true : false;
                SkipVoicePin = securityData.SelectSingleNode(String.Format(".//input[@name='{0}']", FormInputNames.SecuritySettings.VoiceSkipPin))?.GetAttributeValue("checked", "off") == "checked" ? true : false;                
            }

            public NameValueCollection WebFormData
            {
                get
                {
                    NameValueCollection nvc = new NameValueCollection();
                    nvc.Add(FormInputNames.SecuritySettings.CurrentPin, oldPin);
                    nvc.Add(FormInputNames.SecuritySettings.NewPin, Pin);
                    nvc.Add(FormInputNames.SecuritySettings.ConfirmPin, Pin);

                    if (SkipVoicePin)
                    {
                        nvc.Add(FormInputNames.SecuritySettings.VoiceSkipPin, "on");
                    }
                    if (RememberPin)
                    {
                        nvc.Add(FormInputNames.SecuritySettings.RememberPin, "on");
                    }
                    return nvc;
                }
            }

            public override string ToString()
            {
                return String.Format("Remember Me: {0}\tSkip Voice Pin: {1}", RememberPin, SkipVoicePin);
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
            public const string UpdateButton = "ctl00$ContentPlaceHolder1$NextButton";
            public const string UpdateButtonValue = "update";
        }
    }
}

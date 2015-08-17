using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PayByPhoneAPI
{
    class Program
    {
        private CookieContainer myCookies;

        private string myViewState;
        private string myEventValidation;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Login();
            Console.ReadLine();
        }

        private void Login()
        {            
            myCookies = new CookieContainer();
            // load the initial cookies
            var doc = CallAPI("Default.aspx", false);

            NameValueCollection info = new NameValueCollection();
            info.Add("ctl00$ContentPlaceHolder1$CallingCodeDropDownList", "-1");
            info.Add("ctl00$ContentPlaceHolder1$AccountTextBox", "7202564696");
            info.Add("ctl00$ContentPlaceHolder1$PinOrLast4DigitCcTextBox", "2343");
            info.Add("ctl00$ContentPlaceHolder1$LoginButton", "sign+in");
            CallAPI("Default.aspx", true, info);                 
        }

        private HtmlDocument CallAPI(string url, bool post = true, NameValueCollection content = null)
        {
            CookieWebClient webClient = new CookieWebClient();
            webClient.BaseAddress = "https://m.paybyphone.com";
            webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.130 Safari/537.36";
            webClient.Headers["Origin"] = "https://m.paybyphone.com";

            string response;

            if (post)
            {
                if (content == null)
                {
                    content = new NameValueCollection();                   
                }

                content.Add("__EVENTTARGET", "");
                content.Add("__EVENTARGUMENT", "");
                content.Add("__VIEWSTATE", myViewState);
                content.Add("__VIEWSTATEGENERATOR", "");
                content.Add("__EVENTVALIDATION", myEventValidation);

                var result = webClient.UploadValues(url, "POST", content);
                response = Encoding.UTF8.GetString(result);
            }  
            else
            {
                response = webClient.DownloadString(url);

            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(response);
            processState(doc);

            //Console.WriteLine(doc.GetElementbyId("wrapper").InnerHtml);
               
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

            Console.WriteLine("State Changes: {0}", countChanges);
        }
    }
}

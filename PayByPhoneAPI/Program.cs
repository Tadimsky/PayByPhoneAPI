using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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
            var doc = CallAPI("https://m.paybyphone.com/Default.aspx", false);

            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("ctl00$ContentPlaceHolder1$CallingCodeDropDownList={0}", "-1"));
            sb.Append(String.Format("&ctl00$ContentPlaceHolder1$AccountTextBox={0}", "7202564696"));
            sb.Append(String.Format("&ctl00$ContentPlaceHolder1$PinOrLast4DigitCcTextBox={0}", "2343"));
            sb.Append(String.Format("&ctl00$ContentPlaceHolder1$LoginButton={0}", "sign+in"));
            CallAPI("https://m.paybyphone.com/Default.aspx", true, sb);                 
        }

        private HtmlDocument CallAPI(string url, bool post = true, StringBuilder content = null)
        {
            HttpWebRequest initial = WebRequest.CreateHttp(url);
            initial.CookieContainer = myCookies;
            initial.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.130 Safari/537.36";
            initial.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            initial.Headers["Origin"] = "https://m.paybyphone.com";
            initial.Connection = "keep-alive";

            HttpClient hc;
            /*
Cache-Control: max-age=0
Upgrade-Insecure-Requests: 1
DNT: 1
Referer: https://m.paybyphone.com/Default.aspx
Accept-Encoding: gzip, deflate
Accept-Language: en-US,en;q=0.8
*/

            initial.KeepAlive = true;          
            if (post)
            {
                initial.Method = "POST";
                initial.ContentType = "application/x-www-form-urlencoded";

                if (content == null)
                {
                    content = new StringBuilder();                    
                    content.Append(String.Format("__VIEWSTATE={0}", myViewState));
                }
                else
                {
                    content.Append(String.Format("&__VIEWSTATE={0}", myViewState));
                }
                content.Append(String.Format("&__EVENTVALIDATION={0}", myEventValidation));
                content.Append("&__EVENTTARGET=");
                content.Append("&__EVENTARGUMENT=");

                string f = content.ToString();

                using (StreamWriter sr = new StreamWriter(initial.GetRequestStream()))
                {
                    sr.Write(content.ToString());
                }
            }  
            else
            {
                initial.Method = "GET";
            }

            var page = initial.GetResponse();
            Console.WriteLine(page.Headers);  
                             

            var doc = this.loadPage(page);            
            return doc;
        }

        private HtmlDocument loadPage(WebResponse req)
        {
            string content;
            using (StreamReader sr = new StreamReader(req.GetResponseStream()))
            {
                content = sr.ReadToEnd();
            }
            Console.WriteLine(content);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            processState(doc);
            return doc;
        }

        private void processState(HtmlDocument html)
        {
            var viewState = html.GetElementbyId("__VIEWSTATE");
            if (viewState != null)
            {
                var value = viewState.GetAttributeValue("value", "");
                value = value.Replace(" ", "");
                myViewState = value;
            }
            Console.WriteLine();
            var eventValidation = html.GetElementbyId("__EVENTVALIDATION");
            if (eventValidation != null)
            {
                var value = eventValidation.GetAttributeValue("value", "");
                value = value.Replace(" ", "");
                myEventValidation = value;
            }
        }
    }
}

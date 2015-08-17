using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PayByPhoneAPI
{
    class CookieWebClient : WebClient
    {
        private readonly CookieContainer mCookieContainer = new CookieContainer();

        public CookieContainer CookieContainer
        {
            get
            {
                return mCookieContainer;
            }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            HttpWebRequest webRequest = request as HttpWebRequest;

            if (webRequest != null)
            {
                webRequest.CookieContainer = mCookieContainer;
            }
            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse wr = base.GetWebResponse(request);
            ReadCookies(wr);
            return wr;
        }

        protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
        {
            WebResponse wr = base.GetWebResponse(request, result);
            ReadCookies(wr);
            return wr;
        }

        private void ReadCookies(WebResponse r)
        {
            var response = r as HttpWebResponse;
            if (response != null)
            {
                CookieCollection cookies = response.Cookies;
                mCookieContainer.Add(cookies);
            }
        }
    }
}

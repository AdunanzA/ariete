using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ariete.Core
{
    public class MyWebClient : WebClient
    {
        public CookieContainer _mContainer = new CookieContainer();
        public Uri _responseUri;
        public Dictionary<string, string> responseParams = new Dictionary<string, string>();

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = _mContainer;
            }
            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var response = base.GetWebResponse(request);
            if (response is HttpWebResponse)
            {
                _mContainer.Add((response as HttpWebResponse).Cookies);
                _responseUri = response.ResponseUri;

                var query = HttpUtility.ParseQueryString(response.ResponseUri.Query);
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (var param in query)
                {
                    try
                    {
                        if ((param != null) && (query.GetValues(param.ToString()) != null))
                        {
                            responseParams.Add(param.ToString(), query.Get(param.ToString()));
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Key gia' esistente nel dizionario");
                    }
                }
            }
            return response;
        }

        public void ClearCookies()
        {
            _mContainer = new CookieContainer();
        }

    }
}

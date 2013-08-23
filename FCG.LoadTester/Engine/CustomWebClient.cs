using System.Net;

namespace FCG.LoadTester
{
    class CustomWebClient : WebClient
    {
        private WebResponse _response;

        public WebResponse Response
        {
            get { return _response; }
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            try
            {
                WebResponse response = base.GetWebResponse(request);
                _response = response;
                return response;
            }
            catch (WebException)
            {
                _response = null;
                throw;
            }
        }
    }
}
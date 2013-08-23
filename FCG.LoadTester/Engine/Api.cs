using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using FCG.LoadTester.Engine;
using NUnit.Framework;

namespace FCG.LoadTester
{
    public class Api
    {
        enum PostType { Form, Json }

        private readonly LoadTester _loadTester;

        public TestingStatus Status
        {
            get
            {
                return _loadTester.Status;
            }
        }

        private CustomWebClient _client;

        private byte[] _data;

        public List<TesterEvent> EventList { get; private set; }

        public Api(LoadTester loadTester)
        {
            _loadTester = loadTester;
            EventList = new List<TesterEvent>();
        }

        public void Assert(bool condition)
        {
            if (!condition)
            {
                throw new AssertionException("Assertion failed.");
            }
        }

        public Response Get(string url, object parameters = null, string stepName = null)
        {
            RecordStepStart(stepName);
            var client = GetWebClient();
            var nvc = NameValueCollectionConversions.ConvertFromObject(parameters);
            var queryString = string.Join("&",
                                          Array.ConvertAll(nvc.AllKeys,
                                                           key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))));
            try
            {
                _data = client.DownloadData(url + "?" + queryString);
                return BuildResponse(client.Response);
            }
            catch (WebException)
            {
                return BuildErrorResponse();
            }
            finally
            {
                RecordStepEnd(stepName);
            }
        }

        public Response PostForm(string url, object parameters)
        {
            return PostForm(string.Empty, url, parameters);
        }

        public Response PostForm(string stepName, string url, object parameters)
        {
            return Post(stepName, url, parameters, PostType.Form);
        }

        public Response PostJson(string url, object parameters)
        {
            return PostJson(string.Empty, url, parameters);
        }

        public Response PostJson(string stepName, string url, object parameters)
        {
            return Post(stepName, url, parameters, PostType.Json);
        }

        private Response Post(string stepName, string url, object parameters, PostType type)
        {
            RecordStepStart(stepName);

            var client = GetWebClient();
            try
            {
                switch (type)
                {
                    case PostType.Form:
                        _data = client.UploadValues(url, NameValueCollectionConversions.ConvertFromObject(parameters));
                        break;
                    case PostType.Json:
                        var serializer = new JavaScriptSerializer();
                        client.Headers.Add("Content-Type", "application/json; charset=utf-8");
                        _data = client.UploadData(url, Encoding.UTF8.GetBytes(serializer.Serialize(parameters)));
                        client.Headers.Remove("Content-Type");
                        break;
                }

                return BuildResponse(client.Response);
            }
            catch (WebException)
            {
                return BuildErrorResponse();
            }
            finally
            {
                RecordStepEnd(stepName);
            }
        }

        public IDisposable Step(string name)
        {
            RecordStepStart(name);
            return new StepDisposable(name, this);
        }

        private Response BuildErrorResponse()
        {
            return new Response()
                {
                    Url = null,
                    CodeType = ResponseCodes.Error,
                    DataType = ResponseDataTypes.Undefined,
                    DataBag = null
                };
        }

        private Response BuildResponse(WebResponse webResponse)
        {
            var httpWebResponse = (HttpWebResponse)webResponse;
            var dataType = ToResponseDataTypes(httpWebResponse.ContentType);

            dynamic dataBag = null;
            switch (dataType)
            {
                case ResponseDataTypes.HTML:
                    break;
                case ResponseDataTypes.JSON:
                    dataBag = BuildJsonDynamic();
                    break;
                case ResponseDataTypes.XML:
                    throw new NotImplementedException();
            }

            var encoding = GetEncoding(httpWebResponse.CharacterSet);

            var response = new Response
                {
                    Url = httpWebResponse.ResponseUri,
                    CodeType = httpWebResponse.StatusCode == HttpStatusCode.OK ? ResponseCodes.OK : ResponseCodes.Error,
                    DataType = dataType,
                    DataBag = dataBag,
                    DataBytes = _data,
                    DataString = encoding.GetString(_data)
                };
            return response;
        }

        private Encoding GetEncoding(string characterSet)
        {
            try
            {
                return Encoding.GetEncoding(characterSet);
            }
            catch (ArgumentException)
            {
                return Encoding.UTF8;
            }
        }

        dynamic BuildJsonDynamic()
        {
            var client = GetWebClient();
            var response = (HttpWebResponse)client.Response;
            if (response == null || response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }
            var encoding = GetEncoding(response.CharacterSet);
            var jsonText = encoding.GetString(_data);
            return DynamicHelper.FromJson(jsonText);
        }

        public void RecordStepStart(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                EventList.Add(new TesterEvent()
                    {
                        Time = DateTime.Now,
                        Name = name,
                        Type = TesterEventType.Start
                    });
            }
        }

        public void RecordStepEnd(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                EventList.Add(new TesterEvent()
                    {
                        Time = DateTime.Now,
                        Name = name,
                        Type = TesterEventType.End
                    });
            }
        }

        private CustomWebClient GetWebClient()
        {
            return _client ?? (_client = new CustomWebClient());
        }

        private static ResponseDataTypes ToResponseDataTypes(string contentType)
        {
            if (contentType != null)
            {
                var pos = contentType.IndexOf(';');
                if (pos > -1)
                {
                    contentType = contentType.Substring(0, pos);
                }

                if (contentType.Equals("text/html", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ResponseDataTypes.HTML;
                }
                if (contentType.Equals("application/json", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ResponseDataTypes.JSON;
                }
                if (contentType.Equals("text/xml", StringComparison.InvariantCultureIgnoreCase))
                {
                    return ResponseDataTypes.XML;
                }
            }
            return ResponseDataTypes.Undefined;
        }
    }
}
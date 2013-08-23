using System;
using System.Web;
using System.Web.Script.Serialization;

namespace FCG.LoadTester
{
    public static class UriHelper
    {
        public static string GetQueryParameterValue(this Uri uri, string key)
        {
            return HttpUtility.ParseQueryString(uri.Query)[key];
        }
    }

    public static class ObjectHelper
    {
        public static string ToJsonString(this object o)
        {
            return new JavaScriptSerializer().Serialize(o);
        }
    }
}
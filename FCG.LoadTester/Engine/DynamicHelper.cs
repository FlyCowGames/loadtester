using System.Web.Script.Serialization;

namespace FCG.LoadTester.Engine
{
    public static class DynamicHelper
    {
        public static dynamic FromJson(string json)
        {
            var ser = new JavaScriptSerializer();
            ser.RegisterConverters(new[] { new DynamicJsonConverter() });
            return ser.Deserialize(json, typeof(object));
        }
    }
}
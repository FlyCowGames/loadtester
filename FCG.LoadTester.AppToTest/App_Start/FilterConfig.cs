using System.Web;
using System.Web.Mvc;

namespace FCG.LoadTester.AppToTest
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
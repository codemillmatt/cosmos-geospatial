using System;
using Xamarin.Forms;
namespace CosmosGeo.Core
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new GeoSpatialCalcPage());
        }
    }
}

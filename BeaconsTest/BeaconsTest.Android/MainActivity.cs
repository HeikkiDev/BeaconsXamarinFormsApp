
using AltBeaconOrg.BoundBeacon;
using Android.App;
using Android.Content.PM;
using Android.OS;
using BeaconsTest.Services.Beacons;
using Plugin.CurrentActivity;

namespace BeaconsTest.Droid
{
    [Activity(Label = "BeaconsTest", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IBeaconConsumer
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        #region IBeaconConsumer Implementation
        public void OnBeaconServiceConnect()
        {
            var beaconService = Xamarin.Forms.DependencyService.Get<IBeaconMonitoringService>();

            // TODO: Petición de permisos para API >= 23

            //beaconService.InitializeService();
            beaconService.StartMonitoring();
            beaconService.StartRanging();
        }
        #endregion
    }
}


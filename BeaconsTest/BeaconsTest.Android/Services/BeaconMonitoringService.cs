using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AltBeaconOrg.BoundBeacon;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using BeaconsTest.Services.Beacons;
using Plugin.CurrentActivity;

[assembly: Xamarin.Forms.Dependency(typeof(BeaconsTest.Droid.Services.BeaconMonitoringService))]
namespace BeaconsTest.Droid.Services
{
    public class BeaconMonitoringService : IBeaconMonitoringService
    {
        private Context CurrentContext => CrossCurrentActivity.Current.Activity;

        private TaskCompletionSource<bool> tcsPermissions;
        private readonly MonitorNotifier _monitorNotifier;
        private readonly RangeNotifier _rangeNotifier;
        private BeaconManager _beaconManager;

        Region _tagRegion;

        Region _emptyRegion;
        private ListView _list;
        private readonly List<Beacon> _data;

        public BeaconMonitoringService()
        {
            _monitorNotifier = new MonitorNotifier();
            _rangeNotifier = new RangeNotifier();
            _data = new List<Beacon>();
        }

        public event EventHandler<ListChangedEventArgs> ListChanged;
        public event EventHandler DataClearing;

        public BeaconManager BeaconManagerImpl
        {
            get
            {
                if (_beaconManager == null)
                {
                    _beaconManager = InitializeBeaconManager();
                }
                return _beaconManager;
            }
        }

        #region PERMISSIONS
        public Task<bool> GetPermissionsAsync()
        {
            tcsPermissions = new TaskCompletionSource<bool>();

            if ((int)Build.VERSION.SdkInt < 23) // Permissions only for Marshmallow and up
            {
                tcsPermissions.TrySetResult(true);
            }
            else
            {
                //if (ActivityCompat.CheckSelfPermission(CurrentContext, Manifest.Permission.AccessCoarseLocation) != (int)Android.Content.PM.Permission.Granted)
                //{
                //    // TODO...
                //    //RequestBLEPhonePermissions();
                //}
                //else
                    tcsPermissions.TrySetResult(true);
            }

            return tcsPermissions.Task;
        }

        public void OnRequestPermissionsResult(bool isGranted)
        {
            if (isGranted)
            {
                //Permission granted
                tcsPermissions.TrySetResult(true);
            }
            else
            {
                //Permission Denied :(
                tcsPermissions.TrySetResult(false);
            }
        }

        private void RequestBLEPhonePermissions()
        {
            //var currentActivity = (Activity)CurrentContext;
            //if (ActivityCompat.ShouldShowRequestPermissionRationale(currentActivity, Manifest.Permission.AccessCoarseLocation))
            //{
            //    Snackbar.Make(currentActivity.FindViewById((Android.Resource.Id.Content)), "App need location to use with maps.", Snackbar.LengthIndefinite).SetAction("Ok", v =>
            //    {
            //        ((Activity)CurrentContext).RequestPermissions(permissions, LOCATION_PERMISSION_ID);
            //    }).Show();
            //}
            //else
            //{
            //    ActivityCompat.RequestPermissions(((Activity)CurrentContext), permissions, LOCATION_PERMISSION_ID);
            //}
        }
        #endregion

        public void InitializeService()
        {
            _beaconManager = InitializeBeaconManager();
        }

        private BeaconManager InitializeBeaconManager()
        {
            // Enable the BeaconManager 
            BeaconManager bm = BeaconManager.GetInstanceForApplication(CurrentContext);

            #region Set up Beacon Simulator if testing without a BLE device
            //			var beaconSimulator = new BeaconSimulator();
            //			beaconSimulator.CreateBasicSimulatedBeacons();
            //
            //			BeaconManager.BeaconSimulator = beaconSimulator;
            #endregion

            var iBeaconParser = new BeaconParser();
            // iBeacon layout
            iBeaconParser.SetBeaconLayout("m:2-3=0215,i:4-19,i:20-21,i:22-23,p:24-24");
            bm.BeaconParsers.Add(iBeaconParser);

            _monitorNotifier.EnterRegionComplete += EnteredRegion;
            _monitorNotifier.ExitRegionComplete += ExitedRegion;
            _monitorNotifier.DetermineStateForRegionComplete += DeterminedStateForRegionComplete;
            _rangeNotifier.DidRangeBeaconsInRegionComplete += RangingBeaconsInRegion;
            
            _tagRegion = new AltBeaconOrg.BoundBeacon.Region("com.example.myBeaconRegion", Identifier.Parse("39ED98FF-2900-441A-802F-9C398FC199D2"), null, null);

            bm.SetBackgroundMode(false);
            bm.Bind((IBeaconConsumer)CurrentContext);

            return bm;
        }

        public void StartMonitoring()
        {
            BeaconManagerImpl.SetForegroundBetweenScanPeriod(5000); // 5000 milliseconds

            BeaconManagerImpl.AddRangeNotifier(_rangeNotifier);
            _beaconManager.StartMonitoringBeaconsInRegion(_tagRegion);
            _beaconManager.StartMonitoringBeaconsInRegion(_emptyRegion);
        }

        public void StartRanging()
        {
            BeaconManagerImpl.SetForegroundBetweenScanPeriod(5000); // 5000 milliseconds

            BeaconManagerImpl.AddRangeNotifier(_rangeNotifier);
            _beaconManager.StartRangingBeaconsInRegion(_tagRegion);
            _beaconManager.StartRangingBeaconsInRegion(_emptyRegion);
        }

        private void DeterminedStateForRegionComplete(object sender, MonitorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("DeterminedStateForRegionComplete");
        }

        private void ExitedRegion(object sender, MonitorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ExitedRegion");
        }

        private void EnteredRegion(object sender, MonitorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("EnteredRegion");
        }

        async void RangingBeaconsInRegion(object sender, RangeEventArgs e)
        {
            await ClearData();

            var allBeacons = new List<Beacon>();
            if (e.Beacons.Count > 0)
            {
                foreach (var b in e.Beacons)
                {
                    allBeacons.Add(b);
                }

                var orderedBeacons = allBeacons.OrderBy(b => b.Distance).ToList();
                await UpdateData(orderedBeacons);
            }
            else
            {
                // unknown
                await ClearData();
            }
        }

        private async Task UpdateData(List<Beacon> beacons)
        {
            await Task.Run(() =>
            {
                var newBeacons = new List<Beacon>();
                foreach (var beacon in beacons)
                {
                    if (_data.All(b => b.Id1.ToString() == beacon.Id1.ToString()))
                    {
                        newBeacons.Add(beacon);
                    }
                }

                ((Activity)CurrentContext).RunOnUiThread(() =>
                {
                    foreach (var beacon in newBeacons)
                    {
                        _data.Add(beacon);
                    }

                    if (newBeacons.Count > 0)
                    {
                        _data.Sort((x, y) => x.Distance.CompareTo(y.Distance));
                        UpdateList();
                    }
                });
            });
        }

        private async Task ClearData()
        {
            ((Activity)CurrentContext).RunOnUiThread(() =>
            {
                _data.Clear();
                OnDataClearing();
            });
        }

        private void OnDataClearing()
        {
            var handler = DataClearing;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void UpdateList()
        {
            ((Activity)CurrentContext).RunOnUiThread(() =>
            {
                OnListChanged();
            });
        }

        private void OnListChanged()
        {
            var handler = ListChanged;
            if (handler != null)
            {
                var data = new List<SharedBeacon>();
                _data.ForEach(b =>
                {
                    data.Add(new SharedBeacon { Id = b.Id1.ToString(), Distance = string.Format("{0:N2}m", b.Distance) });
                });
                handler(this, new ListChangedEventArgs(data));
            }
        }
    }
}
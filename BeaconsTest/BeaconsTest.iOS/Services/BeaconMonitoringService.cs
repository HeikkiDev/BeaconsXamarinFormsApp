using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeaconsTest.Services.Beacons;
using CoreLocation;
using Foundation;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(BeaconsTest.iOS.Services.BeaconMonitoringService))]
namespace BeaconsTest.iOS.Services
{
    public class BeaconMonitoringService : CLLocationManagerDelegate, IBeaconMonitoringService
    {
        private TaskCompletionSource<bool> tcsPermissions;
        private CLLocationManager locationManager;
        private bool isRangingActive;

        public event EventHandler<ListChangedEventArgs> ListChanged;
        public event EventHandler DataClearing;

        public Task<bool> GetPermissionsAsync()
        {
            tcsPermissions = new TaskCompletionSource<bool>();

            locationManager.RequestAlwaysAuthorization();

            return tcsPermissions.Task;
        }

        // This method is used only for Android
        public void OnRequestPermissionsResult(bool isGranted)
        {
            throw new NotImplementedException();
        }

        public void InitializeService()
        {
            isRangingActive = false;
            locationManager = new CLLocationManager();
            locationManager.Delegate = this;
        }

        public void StartMonitoring()
        {
            if (CLLocationManager.IsMonitoringAvailable(typeof(CLBeaconRegion)))
            {
                // Match all beacons with the specified UUID
                var proximityUUID = new NSUuid("39ED98FF-2900-441A-802F-9C398FC199D2");
                var beaconID = "com.example.myBeaconRegion";

                // Create the region and begin monitoring it.
                var region = new CLBeaconRegion(proximityUUID, beaconID);
                this.locationManager.StartMonitoring(region);
            }
        }

        public void StartRanging()
        {
            isRangingActive = true;
        }

        // Detectar cuando los permisos cambien, y comprobar si han sido concedidos por el usuario
        public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
        {
            if (status == CLAuthorizationStatus.Authorized
                || status == CLAuthorizationStatus.AuthorizedAlways
                || status == CLAuthorizationStatus.AuthorizedWhenInUse)
            {
                tcsPermissions.TrySetResult(true);
            }
            else if (status == CLAuthorizationStatus.Denied || status == CLAuthorizationStatus.Restricted)
            {
                tcsPermissions.TrySetResult(false);
            }
        }

        // When Beacon enter the region created in StartMonitoring()
        public override void RegionEntered(CLLocationManager manager, CLRegion region)
        {
            base.RegionEntered(manager, region);

            if (region is CLBeaconRegion) {
                // Start ranging only if the feature is available and isRangingActive is set to true
                if (CLLocationManager.IsRangingAvailable && isRangingActive) {
                    manager.StartRangingBeacons((CLBeaconRegion)region);
                }
            }
        }

        // When a Beacon exit the region created in MonitorBeacons()
        public override void RegionLeft(CLLocationManager manager, CLRegion region)
        {
            base.RegionLeft(manager, region);

            if (region is CLBeaconRegion)
            {
                // Start ranging only if the feature is available.
                if (CLLocationManager.IsRangingAvailable && isRangingActive)
                {
                    // TODO: Ver qué hace realmente este Stop
                    ///////////////manager.StopRangingBeacons((CLBeaconRegion)region);
                }
            }
        }

        // When ranging is active, the location manager object calls this method whenever there is a change to report.
        // Use this method to take action based on the proximity of nearby beacons.
        public override void DidRangeBeacons(CLLocationManager manager, CLBeacon[] beacons, CLBeaconRegion region)
        {
            base.DidRangeBeacons(manager, beacons, region);

            if (beacons.Length > 0)
            {
                //var nearestBeacon = beacons.First();
                //var major = nearestBeacon.Major;
                //var minor = nearestBeacon.Minor;

                //var proximityInMeters = nearestBeacon.Accuracy;
                //var proximity = nearestBeacon.Proximity;
                
                OnListChanged(beacons);
            }
            else
                OnDataClearing();
        }

        private void OnListChanged(CLBeacon[] beacons)
        {
            var handler = ListChanged;
            if (handler != null)
            {
                var data = new List<SharedBeacon>();

                foreach (var beacon in beacons)
                {
                    data.Add(new SharedBeacon { Id = beacon.ProximityUuid.ToString(), Distance = string.Format("{0:N2}m", beacon.Accuracy) });
                }

                handler(this, new ListChangedEventArgs(data));
            }
        }

        private void OnDataClearing()
        {
            // Clear here local list of Beacons if needed

            DataClearing?.Invoke(this, EventArgs.Empty);
        }
    }
}
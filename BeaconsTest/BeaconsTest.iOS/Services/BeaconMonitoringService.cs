using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using UIKit;

namespace BeaconsTest.iOS.Services
{
    public class BeaconMonitoringService : CLLocationManagerDelegate
    {
        private TaskCompletionSource<bool> tcsPermissions;
        private CLLocationManager locationManager;

        public BeaconMonitoringService()
        {
            locationManager = new CLLocationManager();
            locationManager.Delegate = this;
        }

        public Task<bool> GetPermissionsAsync()
        {
            tcsPermissions = new TaskCompletionSource<bool>();

            locationManager.RequestAlwaysAuthorization();

            return tcsPermissions.Task;
        }

        public void MonitorBeacons()
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

        // When Beacon enter the region created in MonitorBeacons()
        public override void RegionEntered(CLLocationManager manager, CLRegion region)
        {
            base.RegionEntered(manager, region);

            if (region is CLBeaconRegion) {
                // Start ranging only if the feature is available.
                if (CLLocationManager.IsRangingAvailable) {
                    manager.StartRangingBeacons((CLBeaconRegion)region);

                    // TODO: Store the beacon so that ranging can be stopped on demand.
                    //beaconsToRange.append(region as CLBeaconRegion)
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
                if (CLLocationManager.IsRangingAvailable)
                {
                    manager.StopRangingBeacons((CLBeaconRegion)region);

                    // TODO: Remove from local array Store the beacon
                }
            }
        }

        // When ranging is active, the location manager object calls this method whenever there is a change to report.
        // Use this method to take action based on the proximity of nearby beacons.
        public override void DidRangeBeacons(CLLocationManager manager, CLBeacon[] beacons, CLBeaconRegion region)
        {
            base.DidRangeBeacons(manager, beacons, region);

            if (beacons.Length > 0) {
                var nearestBeacon = beacons.First();
                var major = nearestBeacon.Major;
                var minor = nearestBeacon.Minor;

                var proximityInMeters = nearestBeacon.Accuracy;
                var proximity = nearestBeacon.Proximity;
                
                // TODO...
            }
        }
    }
}
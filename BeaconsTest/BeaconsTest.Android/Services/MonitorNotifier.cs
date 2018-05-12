using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AltBeaconOrg.BoundBeacon;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BeaconsTest.Droid.Services
{
    public class MonitorEventArgs : EventArgs
    {
        public Region Region { get; set; }
        public int State { get; set; }
    }

    public class MonitorNotifier : Java.Lang.Object, IMonitorNotifier
    {
        public event EventHandler<MonitorEventArgs> DetermineStateForRegionComplete;
        public event EventHandler<MonitorEventArgs> EnterRegionComplete;
        public event EventHandler<MonitorEventArgs> ExitRegionComplete;

        public void DidDetermineStateForRegion(int state, Region region)
        {
            OnDetermineStateForRegionComplete(state, region);
        }

        public void DidEnterRegion(Region region)
        {
            OnEnterRegionComplete(region);
        }

        public void DidExitRegion(Region region)
        {
            OnExitRegionComplete(region);
        }

        private void OnDetermineStateForRegionComplete(int state, Region region)
        {
            if (DetermineStateForRegionComplete != null)
            {
                DetermineStateForRegionComplete(this, new MonitorEventArgs { State = state, Region = region });
            }
        }

        private void OnEnterRegionComplete(Region region)
        {
            if (EnterRegionComplete != null)
            {
                EnterRegionComplete(this, new MonitorEventArgs { Region = region });
            }
        }

        private void OnExitRegionComplete(Region region)
        {
            if (ExitRegionComplete != null)
            {
                ExitRegionComplete(this, new MonitorEventArgs { Region = region });
            }
        }
    }
}
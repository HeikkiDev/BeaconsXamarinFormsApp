using System;
using System.Collections.Generic;
using System.Text;

namespace BeaconsTest.Services.Beacons
{
    public class ListChangedEventArgs : EventArgs
    {
        public System.Collections.Generic.List<SharedBeacon> Data { get; protected set; }
        public ListChangedEventArgs(System.Collections.Generic.List<SharedBeacon> data)
        {
            Data = data;
        }
    }
}

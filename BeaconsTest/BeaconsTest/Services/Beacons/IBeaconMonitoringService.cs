using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeaconsTest.Services.Beacons
{
    public interface IBeaconMonitoringService
    {
        Task<bool> GetPermissionsAsync();
        void OnRequestPermissionsResult(bool isGranted);

        void InitializeService();
        void StartMonitoring();
        void StartRanging();

        event EventHandler<ListChangedEventArgs> ListChanged;
        event EventHandler DataClearing;
    }
}

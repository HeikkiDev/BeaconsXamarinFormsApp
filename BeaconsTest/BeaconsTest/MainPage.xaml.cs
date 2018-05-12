using BeaconsTest.Services.Beacons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BeaconsTest
{
	public partial class MainPage : ContentPage
	{
        ListView _list;

        public event EventHandler ListChanged;
        public List<SharedBeacon> Data { get; set; }

        public MainPage()
		{
			InitializeComponent();

            //

            Data = new List<SharedBeacon>();

            var beaconService = DependencyService.Get<IBeaconMonitoringService>();
            beaconService.ListChanged += BeaconService_ListChanged;
            beaconService.DataClearing += BeaconService_DataClearing;

            beaconService.InitializeService();
            //

            BackgroundColor = Color.White;
            Title = "AltBeacon Forms Sample";
            //Content = BuildContent();
        }

        private View BuildContent()
        {
            _list = new ListView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                ItemTemplate = new DataTemplate(typeof(ListItemView)),
                RowHeight = 90,
            };

            _list.SetBinding(ListView.ItemsSourceProperty, "Data");

            return _list;
        }

        private void BeaconService_ListChanged(object sender, ListChangedEventArgs e)
        {
            // Data será una Bindable property con la lista de Beacons! :D
            Data = e.Data;
        }

        private void BeaconService_DataClearing(object sender, EventArgs e)
        {
            // Informar al usuario de que no hay Beacons disponibles y limpiar la UI
            Data.Clear();
        }
    }
}

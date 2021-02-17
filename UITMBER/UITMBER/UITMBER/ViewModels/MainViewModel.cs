﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Linq;
using UITMBER.Services.Client.Drivers;
using UITMBER.Services.Location;
using UITMBER.Services.Authentication;
using System.Reflection;
using Xamarin.Forms.GoogleMaps;
using System.Diagnostics;

namespace UITMBER.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public IDriversService DriversService => DependencyService.Get<IDriversService>();
        public ILocationService LocationService => DependencyService.Get<ILocationService>();


        private string text;
        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        private Xamarin.Forms.GoogleMaps.Map mapControl;
        public Xamarin.Forms.GoogleMaps.Map MapControl
        {
            get => mapControl;
            set => SetProperty(ref mapControl, value);
        }

        public ICommand GetCurrentLocationCommand => new Command(async () => await GetMyCurrentLocation());
    
        
        public MainViewModel()
        {
            var mapSpan = Xamarin.Forms.GoogleMaps.MapSpan.FromCenterAndRadius(new Xamarin.Forms.GoogleMaps.Position(50.043604d, 22.0261172d), Xamarin.Forms.GoogleMaps.Distance.FromKilometers(3));

            MapControl = new Xamarin.Forms.GoogleMaps.Map();

            MapControl.MoveToRegion(mapSpan);

            MapControl.MyLocationEnabled = true;

            AddMapStyle();

        }

        private void AddMapStyle()
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream($"UITMBER.MapStyle.json");
            string styleFile;
            using (var reader = new System.IO.StreamReader(stream))
            {
                styleFile = reader.ReadToEnd();
            }

            MapControl.MapStyle = MapStyle.FromJson(styleFile);
        }


        private async Task GetMyCurrentLocation()
        {
            try
            {

                //var result = await AuthService.Authenticate(new Models.Authentication.AuthenticationRequest()
                //{
                //    Login = "test2@test.pl",
                //    Password = "Sm1shn3"
                //});



                var location = await Geolocation.GetLastKnownLocationAsync();

                if(location == null)
                {
                    location = await Geolocation.GetLocationAsync();
                }



                if (location != null)
                {
                    var mapSpan = Xamarin.Forms.GoogleMaps.MapSpan.FromCenterAndRadius(new Xamarin.Forms.GoogleMaps.Position(location.Latitude, location.Longitude), Xamarin.Forms.GoogleMaps.Distance.FromKilometers(1.5));

                    MapControl.MoveToRegion(mapSpan);



                    await LoadNearDrivers(location);
                }

            }
            catch (FeatureNotSupportedException ex)
            {
                //Nie obsługiwana lokalizacja
            }
            catch (FeatureNotEnabledException ex)
            {
                //lokalizacja wyłączona
            }
            catch (PermissionException ex)
            {
                //Brak uprawnien do lokalizacji
            }
            catch (Exception ex)
            {


                Debug.WriteLine(ex.Message);
            }


        }

        private async Task LoadNearDrivers(Location location)
        {

            MapControl.Polylines.Clear();
            MapControl.Pins.Clear();

            //SERVER
            var drivers = await DriversService.GetNerbyDriveres(new Models.LatLong() { Lat = location.Latitude, Long = location.Longitude });

            foreach (var driver in drivers)
            {
                MapControl.Pins.Add(new Xamarin.Forms.GoogleMaps.Pin
                {
                    Type = PinType.Place,
                    Position = new Position(driver.Lat, driver.Long),
                    Label = "Driver",
                    Icon = (Device.RuntimePlatform == Device.Android) ? BitmapDescriptorFactory.FromBundle("ic_car.png") : BitmapDescriptorFactory.FromView(new Image() { Source = "ic_car.png", WidthRequest = 25, HeightRequest = 25 }),
                    Tag = string.Empty
                });
            }


            //MOCKDATA
            //for (int i = 0; i < 10; i++)
            //{
            //    var random = new Random();

            //    MapControl.Pins.Add(new Xamarin.Forms.GoogleMaps.Pin
            //    {
            //        Type = PinType.Place,
            //        Position = new Position(location.Latitude + (random.NextDouble() * 0.04), location.Longitude + (random.NextDouble() * 0.04)),
            //        Label = "Driver",
            //        Icon = (Device.RuntimePlatform == Device.Android) ? BitmapDescriptorFactory.FromBundle("ic_car.png") : BitmapDescriptorFactory.FromView(new Image() { Source = "ic_car.png", WidthRequest = 25, HeightRequest = 25 }),
            //        Tag = string.Empty
            //    });
            //}


        }
    }
}

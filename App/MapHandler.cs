#if ANDROID
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace App
{
    public class MapView : View
    {
    }

    public partial class MapHandler : ViewHandler<MapView, Android.Gms.Maps.MapView>
    {
        public MapHandler() : base(MapMapper)
        { }

        public static IPropertyMapper<MapView, MapHandler> MapMapper = new PropertyMapper<MapView, MapHandler>(ViewMapper)
        { };

        protected override Android.Gms.Maps.MapView CreatePlatformView()
        {
            var mapView = new Android.Gms.Maps.MapView(Android.App.Application.Context);
            mapView.OnCreate(null);
            mapView.GetMapAsync(new OnMapReadyCallback(this));
            return mapView;
        }

        protected override void ConnectHandler(Android.Gms.Maps.MapView platformView)
        {
            base.ConnectHandler(platformView);
            platformView.OnResume();
        }

        protected override void DisconnectHandler(Android.Gms.Maps.MapView platformView)
        {
            base.DisconnectHandler(platformView);
            platformView.OnPause();
            platformView.OnDestroy();
        }

        public void UpdateLocation(LatLng location)
        {
            _googleMap?.MoveCamera(CameraUpdateFactory.NewLatLngZoom(location, 15));
        }

        private class OnMapReadyCallback : Java.Lang.Object, IOnMapReadyCallback
        {
            private readonly MapHandler _mapHandler;

            public OnMapReadyCallback(MapHandler mapHandler)
            {
                _mapHandler = mapHandler;
            }

            public void OnMapReady(GoogleMap googleMap)
            {
                _mapHandler._googleMap = googleMap;
                googleMap.UiSettings.ZoomControlsEnabled = true;
                googleMap.UiSettings.CompassEnabled = true;

                var location = new LatLng(46.056946, 14.505751); // Ljubljana, Slovenia
                googleMap.MoveCamera(CameraUpdateFactory.NewLatLngZoom(location, 10));

                // Enable My Location layer
                if (AndroidX.Core.Content.ContextCompat.CheckSelfPermission(Android.App.Application.Context, Android.Manifest.Permission.AccessFineLocation) == Android.Content.PM.Permission.Granted)
                {
                    googleMap.MyLocationEnabled = true;
                }
            }
        }

        private GoogleMap _googleMap;
    }
}
#endif




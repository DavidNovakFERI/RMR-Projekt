#if ANDROID
using Android.Gms.Maps.Model;
#endif
using Mapsui.UI.Maui;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps.Handlers;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace App;

public partial class StartPage : ContentPage
{
    private Location? _previousLocation;
    private double _totalDistance; // v metrih
    private Stopwatch _stopwatch;
    private bool _isTracking;
    private TrackingData _trackingData;
    private FirebaseAuthProvider authProvider;
    private FirebaseClient firebaseClient;
    private bool _temperatureFetched = false; // Dodajte to spremenljivko

    public StartPage()
    {
        InitializeComponent();
        InitializeMap();
        _stopwatch = new Stopwatch();
        _totalDistance = 0;
        _isTracking = false;
        _trackingData = new TrackingData();
        authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyAv5UMpMuA6rKpIv_nBxwfBjk9BBjTW4jI"));
        firebaseClient = new FirebaseClient("https://aplikacija-7bce5-default-rtdb.europe-west1.firebasedatabase.app/");
    }

    private async void InitializeMap()
    {
        var layout = this.FindByName<Grid>("mainLayout");
        var mapView = new MapView
        {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            VerticalOptions = LayoutOptions.FillAndExpand
        };
        layout.Children.Add(mapView);

#if ANDROID
        // Pridobite trenutno lokacijo
        var location = await Geolocation.GetLastKnownLocationAsync();
        if (location == null)
        {
            location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(30)
            });
        }

        if (location != null)
        {
            // Posodobite zemljevid z trenutno lokacijo
            var handler = mapView.Handler as MapHandler;
            handler?.UpdateLocation(new LatLng(location.Latitude, location.Longitude));

            // Pridobite temperaturo
            if (!_temperatureFetched)
            {
                await FetchTemperature(location.Latitude, location.Longitude);
                _temperatureFetched = true;
            }
        }
#endif
    }

    private async Task FetchTemperature(double latitude, double longitude)
    {
        try
        {
            var apiKey = "0a3620289ba1d246018b963227cf7233"; // Zamenjajte z vašim API kljuèem
            var url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&units=metric&appid={apiKey}";
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var weatherResponse = await response.Content.ReadFromJsonAsync<WeatherResponse>();
                var temperature = weatherResponse?.Main.Temp ?? 0;
                temperatureLabel.Text = $"Temperatura: {temperature} °C";
            }
            else
            {
                await DisplayAlert("Error", $"Failed to fetch temperature: {response.ReasonPhrase}", "OK");
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await DisplayAlert("Error", "Invalid API key. Please check your API key and try again.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to fetch temperature: {ex.Message}", "OK");
        }
    }



    private async void OnStartButtonClicked(object sender, EventArgs e)
    {
        _isTracking = true;
        _stopwatch.Start();
        startButton.IsVisible = false;
        endButton.IsVisible = true;

        // Zaènite spremljati lokacijo
        while (_isTracking)
        {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(30)
            });

            if (location != null)
            {
                UpdateTrackingData(location);
            }

            await Task.Delay(1000); // Poèakajte 1 sekundo pred naslednjim pridobivanjem lokacije
        }
    }

    private async void OnEndButtonClicked(object sender, EventArgs e)
    {
        _isTracking = false;
        _stopwatch.Stop();
        startButton.IsVisible = true;
        endButton.IsVisible = false;

        // Prikažite podatke
        DisplayAlert("Tracking Data", $"Time: {_trackingData.Time}\nDistance: {_trackingData.Distance} m\nSpeed: {_trackingData.Speed} m/s", "OK");

        // Shranite podatke v Firebase
        await SaveTrackingDataToFirebase();

        // Ponastavite timer in spremenljivke
        _stopwatch.Reset();
        _totalDistance = 0;
        _previousLocation = null;
        distanceLabel.Text = "Razdalja: 0 m";
        timeLabel.Text = "Èas: 00:00:00";
        speedLabel.Text = "Hitrost: 0 m/s";
    }

    private void UpdateTrackingData(Location location)
    {
        if (_previousLocation != null)
        {
            var distance = Location.CalculateDistance(_previousLocation, location, DistanceUnits.Kilometers) * 1000; // v metrih
            _totalDistance += distance;
            var time = _stopwatch.Elapsed;
            var speed = _totalDistance / time.TotalSeconds; // v m/s

            _trackingData.Time = time;
            _trackingData.Distance = _totalDistance;
            _trackingData.Speed = speed;

            // Posodobite UI
            distanceLabel.Text = $"Razdalja: {_trackingData.Distance} m";
            timeLabel.Text = $"Èas: {_trackingData.Time:hh\\:mm\\:ss}";
            speedLabel.Text = $"Hitrost: {_trackingData.Speed:F2} m/s";
        }

        _previousLocation = location;
    }

    private async Task SaveTrackingDataToFirebase()
    {
        try
        {
            // Get the currently logged-in user
            var authToken = await SecureStorage.GetAsync("auth_token");
            var user = await authProvider.GetUserAsync(authToken);

            // Generate a new activity ID
            _trackingData.ActivityId = Guid.NewGuid().ToString();
            _trackingData.Date = DateTime.Now; // Nastavite datum in èas aktivnosti
            _trackingData.ActivityType = activityPicker.SelectedItem?.ToString(); // Nastavite vrsto aktivnosti

            // Save the tracking data to Firebase
            await firebaseClient
                .Child("Users")
                .Child(user.LocalId)
                .Child("Activities")
                .Child(_trackingData.ActivityId)
                .PutAsync(_trackingData);

            await DisplayAlert("Uspesno", "Aktivnost shranjena", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save tracking data: {ex.Message}", "OK");
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        var homePage = new NaslovnaStran();
        Navigation.InsertPageBefore(homePage, this);
        await Navigation.PopToRootAsync();
    }

    private async void OnChallangesClicked(object sender, EventArgs e)
    {
        var challangesPage = new ChallangesPage();
        Navigation.InsertPageBefore(challangesPage, this);
        await Navigation.PopToRootAsync();
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        var startPage = new StartPage();
        Navigation.InsertPageBefore(startPage, this);
        await Navigation.PopToRootAsync();
    }

    private async void OnCommunityClicked(object sender, EventArgs e)
    {
        var communityPage = new CommunityPage();
        Navigation.InsertPageBefore(communityPage, this);
        await Navigation.PopToRootAsync();
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        var profilePage = new Profile();
        Navigation.InsertPageBefore(profilePage, this);
        await Navigation.PopToRootAsync();
    }
}

public class WeatherResponse
{
    public Main Main { get; set; }
}

public class Main
{
    public double Temp { get; set; }
}

public class TrackingData
{
    public string ActivityId { get; set; }
    public TimeSpan Time { get; set; }
    public double Distance { get; set; } // v metrih
    public double Speed { get; set; } // v m/s
    public DateTime Date { get; set; }
    public string ActivityType { get; set; }
}

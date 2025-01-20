using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App;

public partial class NaslovnaStran : ContentPage
{
    private FirebaseAuthProvider authProvider;
    private FirebaseClient firebaseClient;

    public NaslovnaStran()
    {
        InitializeComponent();
        NavigationPage.SetHasBackButton(this, false);
        authProvider = FirebaseService.Instance.AuthProvider;
        firebaseClient = FirebaseService.Instance.FirebaseClient;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (Parent is NavigationPage navigationPage)
        {
            navigationPage.BarBackgroundColor = Color.FromHex("#111111");
        }

        // Fetch and display activities
        await FetchAndDisplayActivities();
    }

    private async Task FetchAndDisplayActivities()
    {
        try
        {
            var authToken = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(authToken))
            {
                var user = await authProvider.GetUserAsync(authToken);
                System.Diagnostics.Debug.WriteLine($"User ID: {user.LocalId}");

                var userData = await firebaseClient
                    .Child("Users")
                    .Child(user.LocalId)
                    .OnceSingleAsync<UserData>();

                if (userData != null)
                {
                    System.Diagnostics.Debug.WriteLine($"User Data: {userData.FirstName} {userData.LastName}");
                }

                if (userData != null && userData.Following != null)
                {
                    var activities = new List<(TrackingData, string)>();

                    foreach (var following in userData.Following)
                    {
                        System.Diagnostics.Debug.WriteLine($"Following: {following}");

                        var followingUser = (await firebaseClient
                            .Child("Users")
                            .OnceAsync<UserData>())
                            .FirstOrDefault(u => $"{u.Object.FirstName} {u.Object.LastName}" == following)?.Object;

                        if (followingUser != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Following User Data: {followingUser.FirstName} {followingUser.LastName}");
                        }

                        if (followingUser?.Activities != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Activities found for {followingUser.FirstName} {followingUser.LastName}");
                            activities.AddRange(followingUser.Activities.Values.Select(a => (a, followingUser.FullName)));
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"No activities found for {followingUser.FirstName} {followingUser.LastName}");
                        }
                    }

                    // Razvrstite aktivnosti po datumu (najprej najnovejše) in omejite na 20
                    var sortedActivities = activities
                        .OrderByDescending(a => a.Item1.Date)
                        .Take(20)
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"Total activities to display: {sortedActivities.Count}");

                    // Poèistite obstojeèe aktivnosti
                    activitiesLayout.Children.Clear();

                    // Dodajte vsako aktivnost v `activitiesLayout`
                    foreach (var (activity, fullName) in sortedActivities)
                    {
                        var frame = new Frame
                        {
                            BackgroundColor = Color.FromArgb("#222222"),
                            BorderColor = Color.FromArgb("#555555"),
                            CornerRadius = 10,
                            Padding = 10,
                            Margin = new Thickness(0, 0, 0, 10)
                        };

                        var layout = new VerticalStackLayout
                        {
                            Spacing = 5
                        };

                        layout.Children.Add(new Label { Text = $"Uporabnik: {fullName}", TextColor = Colors.White, FontSize = 16 });
                        layout.Children.Add(new Label { Text = $"Datum: {activity.Date:dd.MM.yyyy HH:mm}", TextColor = Colors.White, FontSize = 16 });
                        layout.Children.Add(new Label { Text = $"Vrsta aktivnosti: {activity.ActivityType}", TextColor = Colors.White, FontSize = 16 });
                        layout.Children.Add(new Label { Text = $"Razdalja: {activity.Distance} m", TextColor = Colors.White, FontSize = 16 });
                        layout.Children.Add(new Label { Text = $"Èas: {activity.Time:hh\\:mm\\:ss}", TextColor = Colors.White, FontSize = 16 });
                        layout.Children.Add(new Label { Text = $"Hitrost: {activity.Speed:F2} m/s", TextColor = Colors.White, FontSize = 16 });

                        frame.Content = layout;
                        activitiesLayout.Children.Add(frame);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("User is not following anyone or no user data found.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Auth token is null or empty.");
            }
        }
        catch (Exception ex)
        {
            // Log the error or handle it as needed, but do not display an error message to the user
            System.Diagnostics.Debug.WriteLine($"Failed to fetch activities: {ex.Message}");
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

public class FirebaseService
{
    private static FirebaseService _instance;
    private static readonly object _lock = new object();
    private FirebaseClient _firebaseClient;
    private FirebaseAuthProvider _authProvider;
    private string apiKey = "AIzaSyAv5UMpMuA6rKpIv_nBxwfBjk9BBjTW4jI";

    private FirebaseService()
    {
        _authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
        _firebaseClient = new FirebaseClient("https://aplikacija-7bce5-default-rtdb.europe-west1.firebasedatabase.app/");
    }

    public static FirebaseService Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new FirebaseService();
                }
                return _instance;
            }
        }
    }

    public FirebaseClient FirebaseClient => _firebaseClient;
    public FirebaseAuthProvider AuthProvider => _authProvider;
}
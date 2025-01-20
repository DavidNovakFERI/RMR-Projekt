using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App;

public partial class Profile : ContentPage
{
    private string apiKey = "AIzaSyAv5UMpMuA6rKpIv_nBxwfBjk9BBjTW4jI";
    private FirebaseAuthProvider authProvider;
    private FirebaseClient firebaseClient;

    public Profile()
    {
        InitializeComponent();
        NavigationPage.SetHasBackButton(this, false);
        authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
        firebaseClient = new FirebaseClient("https://aplikacija-7bce5-default-rtdb.europe-west1.firebasedatabase.app/");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (Parent is NavigationPage navigationPage)
        {
            navigationPage.BarBackgroundColor = Color.FromHex("#111111");
        }

        // Fetch user data and update the label
        await FetchUserData();
    }

    private async Task FetchUserData()
    {
        try
        {
            var authToken = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(authToken))
            {
                var user = await authProvider.GetUserAsync(authToken);
                var userData = await firebaseClient
                    .Child("Users")
                    .Child(user.LocalId)
                    .OnceSingleAsync<UserData>();

                if (userData != null)
                {
                    // Update the label with the fetched data
                    NameLabel.Text = $"{userData.FirstName} {userData.LastName}";
                    VelikostLabel.Text = userData.Velikost;
                    StarostLabel.Text = userData.Starost;
                    TezaLabel.Text = userData.Teza;

                    // Prikažite aktivnosti
                    DisplayActivities(userData.Activities);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to fetch user data: {ex.Message}", "OK");
        }
    }

    private void DisplayActivities(Dictionary<string, TrackingData> activities)
    {
        if (activities == null) return;

        // Razvrstite aktivnosti po datumu (najprej najnovejše)
        var sortedActivities = activities.Values.OrderByDescending(a => a.Date).ToList();

        // Poèistite obstojeèe aktivnosti
        activitiesLayout.Children.Clear();

        // Dodajte vsako aktivnost v `activitiesLayout`
        foreach (var activity in sortedActivities)
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

            layout.Children.Add(new Label { Text = $"Datum: {activity.Date:dd.MM.yyyy HH:mm}", TextColor = Colors.White, FontSize = 16 });
            layout.Children.Add(new Label { Text = $"Vrsta aktivnosti: {activity.ActivityType}", TextColor = Colors.White, FontSize = 16 });
            layout.Children.Add(new Label { Text = $"Razdalja: {activity.Distance} m", TextColor = Colors.White, FontSize = 16 });
            layout.Children.Add(new Label { Text = $"Èas: {activity.Time:hh\\:mm\\:ss}", TextColor = Colors.White, FontSize = 16 });
            layout.Children.Add(new Label { Text = $"Hitrost: {activity.Speed:F2} m/s", TextColor = Colors.White, FontSize = 16 });

            var deleteButton = new Button
            {
                Text = "Izbriši",
                BackgroundColor = Colors.Red,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center
            };
            deleteButton.Clicked += async (sender, e) => await ConfirmAndDeleteActivity(activity.ActivityId);

            layout.Children.Add(deleteButton);
            frame.Content = layout;
            activitiesLayout.Children.Add(frame);
        }
    }

    private async Task ConfirmAndDeleteActivity(string activityId)
    {
        bool confirm = await DisplayAlert("Potrditev", "Ali zares hoèeš izbrisati aktivnost?", "Da", "Ne");
        if (confirm)
        {
            await DeleteActivity(activityId);
        }
    }

    private async Task DeleteActivity(string activityId)
    {
        try
        {
            var authToken = await SecureStorage.GetAsync("auth_token");
            var user = await authProvider.GetUserAsync(authToken);

            // Delete the activity from Firebase
            await firebaseClient
                .Child("Users")
                .Child(user.LocalId)
                .Child("Activities")
                .Child(activityId)
                .DeleteAsync();

            // Refresh the user data to update the UI
            await FetchUserData();

            await DisplayAlert("Uspesno", "Uspesno izbrisana aktivnost", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete activity: {ex.Message}", "OK");
        }
    }

    private async void OnUpdateButtonClicked(object sender, EventArgs e)
    {
        // Navigate to the UpdateProfile page
        await Navigation.PushAsync(new UpdateProfile());
    }

    private async void OnToolbarItemClicked(object sender, EventArgs e)
    {
        // Call the Logout method from App.xaml.cs
        await ((App)Application.Current).Logout();
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

    private async void OnEditProfileClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UpdateProfile());
    }
}



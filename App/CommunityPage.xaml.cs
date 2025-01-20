using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace App;

public partial class CommunityPage : ContentPage
{
    private string apiKey = "AIzaSyAv5UMpMuA6rKpIv_nBxwfBjk9BBjTW4jI";
    private FirebaseAuthProvider authProvider;
    private FirebaseClient firebaseClient;
    private ObservableCollection<UserData> allProfiles;

    public CommunityPage()
    {
        InitializeComponent();
        NavigationPage.SetHasBackButton(this, false);
        authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
        firebaseClient = new FirebaseClient("https://aplikacija-7bce5-default-rtdb.europe-west1.firebasedatabase.app/");
        allProfiles = new ObservableCollection<UserData>();
        ProfilesCollectionView.ItemsSource = allProfiles;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (Parent is NavigationPage navigationPage)
        {
            navigationPage.BarBackgroundColor = Color.FromHex("#111111");
        }

        // Fetch all profiles
        await FetchAllProfiles();

        // Display up to 10 profiles by default
        DisplayProfiles(allProfiles.Take(10).ToList());
    }

    private async Task FetchAllProfiles()
    {
        try
        {
            var profiles = await firebaseClient
                .Child("Users")
                .OnceAsync<UserData>();

            allProfiles.Clear();
            foreach (var profile in profiles)
            {
                allProfiles.Add(new UserData
                {
                    FirstName = profile.Object.FirstName,
                    LastName = profile.Object.LastName
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to fetch profiles: {ex.Message}", "OK");
        }
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue.ToLower();
        var filteredProfiles = allProfiles
            .Where(p => $"{p.FirstName} {p.LastName}".ToLower().Contains(searchText))
            .Take(10) // Limit the results to 10
            .ToList();
        DisplayProfiles(filteredProfiles);
    }

    private void DisplayProfiles(List<UserData> profiles)
    {
        ProfilesCollectionView.ItemsSource = new ObservableCollection<UserData>(profiles);
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

    private async void OnAddButtonClicked(object sender, EventArgs e)
    {
        // Handle the add button click event
        var button = sender as ImageButton;
        var profile = button?.BindingContext as UserData;
        if (profile != null)
        {
            // Get the currently logged-in user
            var authToken = await SecureStorage.GetAsync("auth_token");
            var user = await authProvider.GetUserAsync(authToken);

            // Get the current user's data
            var currentUserData = await firebaseClient
                .Child("Users")
                .Child(user.LocalId)
                .OnceSingleAsync<UserData>();

            if (currentUserData != null)
            {
                // Add the profile to the following list
                if (currentUserData.Following == null)
                {
                    currentUserData.Following = new List<string>();
                }

                if (!currentUserData.Following.Contains(profile.FullName))
                {
                    currentUserData.Following.Add(profile.FullName);

                    // Update the Firebase database
                    await firebaseClient
                        .Child("Users")
                        .Child(user.LocalId)
                        .PutAsync(currentUserData);

                    await DisplayAlert("Follow Profile", $"You are now following {profile.FullName}", "OK");
                }
                else
                {
                    await DisplayAlert("Follow Profile", $"You are already following {profile.FullName}", "OK");
                }
            }
        }
    }


    private async void OnRemoveButtonClicked(object sender, EventArgs e)
    {
        // Handle the remove button click event
        var button = sender as ImageButton;
        var profile = button?.BindingContext as UserData;
        if (profile != null)
        {
            // Get the currently logged-in user
            var authToken = await SecureStorage.GetAsync("auth_token");
            var user = await authProvider.GetUserAsync(authToken);

            // Get the current user's data
            var currentUserData = await firebaseClient
                .Child("Users")
                .Child(user.LocalId)
                .OnceSingleAsync<UserData>();

            if (currentUserData != null && currentUserData.Following != null)
            {
                // Remove the profile from the following list
                if (currentUserData.Following.Contains(profile.FullName))
                {
                    currentUserData.Following.Remove(profile.FullName);

                    // Update the Firebase database
                    await firebaseClient
                        .Child("Users")
                        .Child(user.LocalId)
                        .PutAsync(currentUserData);

                    await DisplayAlert("Unfollow Profile", $"You are no longer following {profile.FullName}", "OK");
                }
                else
                {
                    await DisplayAlert("Unfollow Profile", $"You are not following {profile.FullName}", "OK");
                }
            }
        }
    }



}

public class UserData
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public List<string> Following { get; set; }
    public string Velikost { get; set; }
    public string Starost { get; set; }
    public string Teza { get; set; }
    public Dictionary<string, TrackingData> Activities { get; set; } 
}


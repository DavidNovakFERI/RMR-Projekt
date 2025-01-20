using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microcharts;
using Microcharts.Maui;
using Microsoft.Maui.Controls;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App
{
    public partial class ChallangesPage : ContentPage
    {
        private string apiKey = "AIzaSyAv5UMpMuA6rKpIv_nBxwfBjk9BBjTW4jI";
        private FirebaseAuthProvider authProvider;
        private FirebaseClient firebaseClient;
        private Goal currentGoal;

        public ChallangesPage()
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

            // Fetch and display goal
            await FetchAndDisplayGoal();

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
                    var userData = await firebaseClient
                        .Child("Users")
                        .Child(user.LocalId)
                        .OnceSingleAsync<UserData>();

                    if (userData != null && userData.Activities != null)
                    {
                        var activities = userData.Activities.Values
                            .Where(a => currentGoal == null || a.Date >= currentGoal.DateSet)
                            .OrderByDescending(a => a.Date)
                            .Take(5)
                            .OrderBy(a => a.Date)
                            .ToList();

                        if (activities.Any())
                        {
                            // Create chart entries
                            var entries = activities.Select(a => new ChartEntry((float)a.Distance)
                            {
                                Label = a.Date.ToString("dd.MM.yyyy"),
                                ValueLabel = a.Distance.ToString("F2"),
                                Color = SKColor.Parse("#00CED1"),
                                TextColor = SKColors.White
                            }).ToArray();

                            // Create and set the chart
                            var chart = new LineChart
                            {
                                Entries = entries,
                                LabelTextSize = 30,
                                BackgroundColor = SKColors.Transparent,
                                ValueLabelOrientation = Orientation.Horizontal,
                                LabelOrientation = Orientation.Horizontal
                            };
                            chartView.Chart = chart;

                            // Update progress bar if goal is set
                            if (currentGoal != null)
                            {
                                var totalDistance = activities.Sum(a => a.Distance);
                                progressBar.Progress = totalDistance / currentGoal.TargetDistance;
                            }
                        }
                        else
                        {
                            // Clear the chart and progress bar if no activities are found
                            chartView.Chart = null;
                            progressBar.Progress = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to fetch activities: {ex.Message}", "OK");
            }
        }

        private async Task FetchAndDisplayGoal()
        {
            try
            {
                var authToken = await SecureStorage.GetAsync("auth_token");
                if (!string.IsNullOrEmpty(authToken))
                {
                    var user = await authProvider.GetUserAsync(authToken);
                    currentGoal = await firebaseClient
                        .Child("Users")
                        .Child(user.LocalId)
                        .Child("Goal")
                        .OnceSingleAsync<Goal>();

                    if (currentGoal != null)
                    {
                        UpdateDaysRemaining();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to fetch goal: {ex.Message}", "OK");
            }
        }

        private void UpdateDaysRemaining()
        {
            if (currentGoal != null)
            {
                var daysRemaining = (currentGoal.DateSet.AddDays(7) - DateTime.Now).Days;
                daysRemainingLabel.Text = $"Preostali dnevi: {daysRemaining}";
            }
        }

        private async void OnSetGoalButtonClicked(object sender, EventArgs e)
        {
            var result = await DisplayPromptAsync("Nastavi cilj", "Vnesite dolžino cilja v metrih:", "OK", "Cancel", "1000", 10, Keyboard.Numeric);
            if (double.TryParse(result, out double targetDistance))
            {
                currentGoal = new Goal
                {
                    DateSet = DateTime.Now,
                    TargetDistance = targetDistance
                };

                var authToken = await SecureStorage.GetAsync("auth_token");
                var user = await authProvider.GetUserAsync(authToken);

                await firebaseClient
                    .Child("Users")
                    .Child(user.LocalId)
                    .Child("Goal")
                    .PutAsync(currentGoal);

                UpdateDaysRemaining();
                await FetchAndDisplayActivities();
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

   

    public class Goal
    {
        public DateTime DateSet { get; set; }
        public double TargetDistance { get; set; } // v metrih
    }
}

using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace App
{
    public partial class UpdateProfile : ContentPage
    {
        private string apiKey = "AIzaSyAv5UMpMuA6rKpIv_nBxwfBjk9BBjTW4jI";
        private FirebaseAuthProvider authProvider;
        private FirebaseClient firebaseClient;

        public UpdateProfile()
        {
            InitializeComponent();
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
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                // Preverite, ali so vse tri vrednosti vnesene
                if (string.IsNullOrEmpty(VelikostEntry.Text) || string.IsNullOrEmpty(StarostEntry.Text) || string.IsNullOrEmpty(TezaEntry.Text))
                {
                    await DisplayAlert("Error", "Please enter all three values (Velikost, Starost, Teza).", "OK");
                    return;
                }

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
                        // Ohranite obstojeèe aktivnosti
                        var existingActivities = userData.Activities;

                        userData.Velikost = VelikostEntry.Text;
                        userData.Starost = StarostEntry.Text;
                        userData.Teza = TezaEntry.Text;
                        userData.Activities = existingActivities;

                        await firebaseClient
                            .Child("Users")
                            .Child(user.LocalId)
                            .PutAsync(userData);

                        await DisplayAlert("Success", "Profile updated successfully", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update profile: {ex.Message}", "OK");
            }

            var homePage = new NaslovnaStran();
            Navigation.InsertPageBefore(homePage, this);
            await Navigation.PopToRootAsync();
        }
    }
}






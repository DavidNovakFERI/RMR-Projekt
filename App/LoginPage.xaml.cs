using Firebase.Auth;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace App
{
    public partial class LoginPage : ContentPage
    {
        private string apiKey = "AIzaSyAv5UMpMuA6rKpIv_nBxwfBjk9BBjTW4jI";
        private FirebaseAuthProvider authProvider;

        public LoginPage()
        {
            InitializeComponent();
            authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
        }

        // Login Button Clicked Event
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Get the entered email and password
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            // Attempt to login with Firebase
            try
            {
                var auth = await authProvider.SignInWithEmailAndPasswordAsync(email, password);
                if (auth != null)
                {
                    await SecureStorage.SetAsync("auth_token", auth.FirebaseToken);

                    await DisplayAlert("Success", "Login successful!", "OK");

                    // Set the MainPage to NaslovnaStran
                    Application.Current.MainPage = new NavigationPage(new NaslovnaStran());
                }
                else
                {
                    await DisplayAlert("Error", "Login failed. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                // Handle errors (e.g., incorrect email/password)
                await DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
            }
        }
    }
}

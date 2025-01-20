using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace App
{
    public partial class RegisterPage : ContentPage
    {
        private string apiKey = "AIzaSyAv5UMpMuA6rKpIv_nBxwfBjk9BBjTW4jI";
        private FirebaseAuthProvider authProvider;
        private FirebaseClient firebaseClient;

        public RegisterPage()
        {
            InitializeComponent();
            this.Opacity = 0;
            authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            firebaseClient = new FirebaseClient("https://aplikacija-7bce5-default-rtdb.europe-west1.firebasedatabase.app/");
        }

        // Register Button Clicked Event
        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            string firstName = FirstNameEntry.Text;
            string lastName = LastNameEntry.Text;
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // Validate email contains "@"
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                await DisplayAlert("Error", "Please enter a valid email address with '@'.", "OK");
                return;
            }

            // Validate password length is at least 6 characters
            if (password.Length < 6)
            {
                await DisplayAlert("Error", "Password must be at least 6 characters long.", "OK");
                return;
            }

            // Check if passwords match
            if (password != confirmPassword)
            {
                await DisplayAlert("Error", "Passwords do not match. Please try again.", "OK");
                return;
            }

            // Register the user with Firebase
            try
            {
                var auth = await authProvider.CreateUserWithEmailAndPasswordAsync(email, password);
                if (auth != null)
                {
                    // Save user details to Firebase database
                    await firebaseClient
                        .Child("Users")
                        .Child(auth.User.LocalId)
                        .PutAsync(new { FirstName = firstName, LastName = lastName });

                    

                    await DisplayAlert("Success", "Registration successful!", "OK");

                    // Optionally, you can navigate to another page or perform other actions here
                    await Navigation.PopToRootAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Registration failed. Please try again.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Registration failed: {ex.Message}", "OK");
            }
        }
    }
}



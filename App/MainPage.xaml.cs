using Microsoft.Maui.Controls;

namespace App
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        
        private async void OnLoginTapped(object sender, EventArgs e)
        {
            
                var loginPage = new LoginPage();
                await Navigation.PushAsync(loginPage);
                await loginPage.FadeTo(1, 1000);
            
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var registerPage = new RegisterPage();
            await Navigation.PushAsync(registerPage);
            await registerPage.FadeTo(1, 1000);
        }
    }
}

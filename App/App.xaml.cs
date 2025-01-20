namespace App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage());
        }

        protected override async void OnStart()
        {
            base.OnStart();

            // Check if the authentication token exists
            var authToken = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(authToken))
            {
                // Navigate to the main page
                MainPage = new NavigationPage(new NaslovnaStran());
            }
        }

        public async Task Logout()
        {
            // Clear the authentication token
            SecureStorage.Remove("auth_token");

            // Navigate back to the MainPage and clear the navigation stack
            MainPage = new NavigationPage(new MainPage());
        }
    }
}

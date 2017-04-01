using System;
using System.Net.Http;
using Xamarin.Forms;
using System.Net;
using Pictionary.Views;
using Xamarin.Forms.Xaml;

namespace Pictionary
{
    public partial class LoginPage : ContentPage
    {
        private string responseContent;

        public LoginPage()
        {
            InitializeComponent();
        }

        //event handlers

        private async void LoginBTN_Clicked(object sender, EventArgs args)
        {
            ApiController.Email = EmailENT.Text;
            ApiController.Password = PasswordENT.Text;

            try
            {
                var response = await ApiController.Login();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (HttpContent content = response.Content)
                    {
                        responseContent = await content.ReadAsStringAsync();

                        ApiController.UserId = responseContent.Substring(1, responseContent.Length - 2);
                    }
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    NavigationPage userMainPage = new NavigationPage(new UserMainPage());
                    Application.Current.MainPage = userMainPage;
                }
                else
                {
                    await DisplayAlert("LoginBTN_Clicked: " + response.StatusCode.ToString(), response.ReasonPhrase, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void SignUpBTN_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new SignUpPage());
            }
            catch (Exception ex)
            {
                await DisplayAlert("SignUpBTN_Clicked", ex.Message, "OK");
            }
        }
    }
}

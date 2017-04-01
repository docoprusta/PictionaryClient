using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pictionary.Views
{
    public partial class SignUpPage : ContentPage
    {
        private string responseContent;

        public SignUpPage()
        {
            InitializeComponent();
        }

        private async void SignUpBTN_Clicked(object sender, EventArgs e)
        {
            try
            {
                var response = await ApiController.SignUp(EmailENT.Text, PasswordENT.Text);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    await Navigation.PopAsync();
                    //NavigationPage loginPage = new NavigationPage(new LoginPage());
                    //Application.Current.MainPage = loginPage;
                }
                else
                {
                    await DisplayAlert("SignUpBTN_Clicked: " + response.StatusCode.ToString(), response.ReasonPhrase, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}

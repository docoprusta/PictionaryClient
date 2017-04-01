using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using UIKit;
using Xamarin.Forms;
//using Xamarin.Forms;

namespace Pictionary.Views
{
    public partial class PixabayImagesPage : ContentPage
    {
        /// <summary>
        /// Pixabay URL of the image
        /// </summary>
        public string URL { get; private set; } = "";

        private byte type;

        /// <summary>
        /// Pixabay Id of the image
        /// this is necessary because the image URL may expire
        /// </summary>
        public string Id { get; private set; } = "";

        public PixabayImagesPage(byte type = 0)
        {
            this.type = type;
            InitializeComponent();
        }

        /// <summary>
        /// Set the images sources of the current images page
        /// </summary>
        /// <param name="num">Which image source want to set</param>
        /// <param name="source">The source of the image</param>
        public void SetImageSource(int num, string source)
        {
            switch (num)
            {
                case 1:
                    Image1IMG.Source = source;
                    break;
                case 2:
                    Image2IMG.Source = source;
                    break;
                case 3:
                    Image3IMG.Source = source;
                    break;
            }
        }

        /// <summary>
        /// Set the image of the current Word page
        /// </summary>
        /// <param name="img"></param>
        private async Task SetWordPageImageAsync(Image img)
        {
            URL = img.Source.GetValue(UriImageSource.UriProperty).ToString();

            var stack = Navigation.NavigationStack;

            if (type == 0)
            {
                AddWordPage page = (AddWordPage)stack[1];
                page.SaveSelectedIMG(this);

            }
            else if (type == 1)
            {
                EditWordPage page = (EditWordPage)stack[1];
                page.SaveSelectedIMG(this);
                await page.Save();
            }

        }

        //Event handlers

        private async void Image1IMG_Tapped(object sender, EventArgs e)
        {
            try
            {
                await SetWordPageImageAsync(Image1IMG);

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Image1IMG_Tapped", ex.Message, "OK");
            }
        }

        private async void Image2IMG_Tapped(object sender, EventArgs e)
        {
            try
            {
                await SetWordPageImageAsync(Image2IMG);
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Image2IMG_Tapped", ex.Message, "OK");
            }
        }

        private async void Image3IMG_Tapped(object sender, EventArgs e)
        {
            try
            {
                await SetWordPageImageAsync(Image3IMG);
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Image3IMG_Tapped", ex.Message, "OK");
            }
        }

    }
}

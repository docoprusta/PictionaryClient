using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Pictionary.Views
{
    public partial class AddWordPage : ContentPage
    {
        /// <summary>
        /// Pages that contains images
        /// </summary>
        protected List<ContentPage> pages = new List<ContentPage>();

        private string responseContent = "";

        /// <summary>
        /// Contains image pixabay id and pixabay url
        /// </summary>
        private List<Tuple<string, string>> urls = new List<Tuple<string, string>>();

        /// <summary>
        /// Contains language2 meanings of the word
        /// </summary>
        private List<string> meanings = new List<string>();

        /// <summary>
        /// Contains the meaning Entries
        /// </summary>
        private List<Entry> meaningEntries = new List<Entry>();

        /// <summary>
        ///  Image pixabay id
        /// </summary>
        private string imageId = "";

        public AddWordPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// <para>Call ApiController PostWord function and handle exceptions</para>
        /// <para>Notify user if something went wrong (with http status code)</para>
        /// </summary>
        /// <param name="url">Pixabay image id</param>
        /// <returns></returns>
        private async Task PostWord(string url)
        {
            try
            {
                HttpResponseMessage response = await ApiController.PostWord(url, WordENT.Text, meanings);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var stack = Navigation.NavigationStack;

                    UserMainPage page = (UserMainPage)stack[0];

                    await page.Init();

                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert(response.StatusCode.ToString(), response.ReasonPhrase, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("PostWord: ", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Save selected image id and url
        /// </summary>
        /// <param name="img"></param>
        public void SaveSelectedIMG(PixabayImagesPage img)
        {
            imageId = img.Id;
            SelectedIMG.Source = img.URL;
        }

        /// <summary>
        /// <para>Call ApiController.GetImages. Get images from pixabay, and save these. </para>
        /// <para> Handle exceptions. Notify user if something went wrong (with http status code) </para>
        /// </summary>
        /// <param name="image">Image that want to search</param>
        /// <returns></returns>
        private async Task GetImages(string image)
        {
            try
            {
                urls.Clear();
                HttpResponseMessage response = await ApiController.GetImages(image);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    await DisplayAlert(response.StatusCode.ToString(), response.ReasonPhrase, "OK");
                }

                dynamic url = JsonConvert.DeserializeObject(responseContent);

                foreach (var item in url["hits"])
                {
                    urls.Add(new Tuple<string, string>(item["id"].ToString(), item["webformatURL"].ToString()));
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("GetImages: ", ex.Message, "OK");
            }
        }

        //event handlers

        private async void AddMeaningBTN_Clicked(object sender, EventArgs e)
        {
            Entry en;
            MeaningsSTL.Children.Add(en = new Entry { Placeholder = "Meaning" });
            meaningEntries.Add(en);
        }

        private async void AddImageBTN_Clicked(object sender, EventArgs e)
        {
            try
            {

                pages.Clear();

                await GetImages(WordENT.Text);

                //Create images pages
                for (int i = 0; i < 15; i += 3)
                {
                    PixabayImagesPage imgs1 = new PixabayImagesPage();

                    imgs1.SetImageSource(1, urls[i].Item2);
                    imgs1.SetImageSource(2, urls[i + 1].Item2);
                    imgs1.SetImageSource(3, urls[i + 2].Item2);

                    pages.Add(imgs1);


                }

                //Create new CarouselPage that show the founded pixabay images
                await Navigation.PushAsync(new CarouselPage
                {
                    Children =
                        {
                            pages[0],
                            pages[1],
                            pages[2],
                            pages[3],
                            pages[4]
                        }
                });

            }

            catch (Exception ex)
            {
                await DisplayAlert("AddImageBTN_Clicked: ", ex.Message, "OK");
            }
        }

        private async void SaveBTN_Clicked(object sender, EventArgs e)
        {
            try
            {
                //Save meanings
                foreach (var item in meaningEntries)
                {
                    if (!meanings.Contains(item.Text) && item.Text.Trim() != "")
                    {
                        meanings.Add(item.Text);
                    }
                }

                foreach (var item in urls)
                {
                    if (item.Item2 == SelectedIMG.Source.GetValue(UriImageSource.UriProperty).ToString())
                    {
                        await PostWord(item.Item1);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("SaveBTN_Clicked", "appeared" + ex.Message, "OK");
            }
        }
    }
}

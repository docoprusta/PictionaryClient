using Android;
using Android.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static Android.Content.ClipData;

namespace Pictionary.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditWordPage : ContentPage
    {
        /// <summary>
        /// Pages that contains images
        /// </summary>
        protected List<ContentPage> pages = new List<ContentPage>();

        /// <summary>
        /// Contains image pixabay id and pixabay url
        /// </summary>
        private List<Tuple<string, string>> urls = new List<Tuple<string, string>>();

        /// <summary>
        /// MeaningsLSV SelectedItem as WordViewModel
        /// </summary>
        private WordViewModel wordViewModel;

        /// <summary>
        /// Image urls in JSON
        /// </summary>
        private string responseContent = "";

        /// <summary>
        /// New meaning instead of current meaning
        /// </summary>
        private string modifyValue = "";

        /// <summary>
        ///  Image pixabay id
        /// </summary>
        private string imageId = "";

        /// <summary>
        /// Contains the meaning Entries
        /// </summary>
        private List<Entry> meaningEntries = new List<Entry>();

        /// <summary>
        /// Meanings of the word
        /// </summary>
        public List<string> meanings = new List<string>();

        public EditWordPage(WordViewModel wordViewModel)
        {
            InitializeComponent();

            MeaningsLSV.ItemsSource = null;

            meanings = wordViewModel.MeaningStr.Split(',').ToList();

            MeaningsLSV.ItemsSource = meanings;

            this.wordViewModel = wordViewModel;

            this.Title = wordViewModel.Word;

            SelectedIMG.Source = wordViewModel.Picture;

            Content = MainGRD;
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

        private async void ChangeImgBTN_Clicked(object sender, EventArgs e)
        {
            try
            {
                pages.Clear();

                await GetImages(this.Title);

                //Create images pages
                for (int i = 0; i < 15; i += 3)
                {
                    PixabayImagesPage imgs1 = new PixabayImagesPage(1);

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

        private async Task InitIfResponseIsOk(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                InitUserMainPage();
            }
            else
            {
                await DisplayAlert(response.StatusCode.ToString(), response.ReasonPhrase, "ok");
            }
        }

        private async void AddMeaningBTN_Clicked(object sender, EventArgs e)
        {
            try
            {
                meanings.Add(AddMeaningENT.Text);

                HttpResponseMessage response = await ApiController.AddMeaning(wordViewModel.Id, UserMainPage.Language2, AddMeaningENT.Text);
                MeaningsLSV.ItemsSource = null;

                MeaningsLSV.ItemsSource = meanings;

                await InitIfResponseIsOk(response);
            }
            catch (Exception ex)
            {
                await DisplayAlert("AddMeaningBTN_Clicked", ex.Message, "OK");
            }
        }

        private async void InitUserMainPage()
        {
            var stack = Navigation.NavigationStack;

            UserMainPage page = (UserMainPage)stack[0];

            await page.Init();
        }

        public async Task Save()
        {
            try
            {
                foreach (var item in urls)
                {
                    if (item.Item2 == SelectedIMG.Source.GetValue(UriImageSource.UriProperty).ToString())
                    {
                        imageId = item.Item1;

                        await ApiController.EditWord(
                            wordViewModel.Id, wordViewModel.Word,
                            wordViewModel.ImageId, wordViewModel.Word, imageId
                        );

                        InitUserMainPage();

                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("SaveBTN_Clicked", ex.Message, "OK");
            }
        }

        public async void OnDelete(object sender, EventArgs e)
        {
            try
            {
                if (meanings.Count > 1)
                {

                    MenuItem m = (MenuItem)sender;

                    HttpResponseMessage response;

                    response = await ApiController.DeleteMeaning(wordViewModel.Id, UserMainPage.Language2, m.CommandParameter.ToString());

                    await InitIfResponseIsOk(response);

                    meanings.Remove(m.CommandParameter.ToString());
                    MeaningsLSV.ItemsSource = null;

                    MeaningsLSV.ItemsSource = meanings;
                }
                else
                {
                    await DisplayAlert("Error", "You can't delete last meaning", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("ASD", ex.Message, "OK");
            }
        }

        private async void ModifyMeaningBTN_Clicked(object sender, EventArgs e)
        {
            try
            {
                MeaningsLSV.ItemsSource = null;
                meanings[meanings.IndexOf(modifyValue)] = AddMeaningENT.Text;
                MeaningsLSV.ItemsSource = meanings;

                HttpResponseMessage response =
                    await ApiController.EditWord(
                 wordViewModel.Id, modifyValue,
                 wordViewModel.ImageId, AddMeaningENT.Text
                );

                await InitIfResponseIsOk(response);

                modifyValue = "";
            }
            catch (Exception ex)
            {
                await DisplayAlert("ModifyMeaningBTN_Clicked", ex.Message, "OK");
            }
        }

        private async void MeaningsLSV_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            try
            {
                AddMeaningENT.Text = MeaningsLSV.SelectedItem.ToString();
                modifyValue = MeaningsLSV.SelectedItem.ToString();
                ModifyMeaningBTN.IsEnabled = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("MeaningsLSV_ItemSelected", ex.Message, "OK");
            }
        }
    }
}

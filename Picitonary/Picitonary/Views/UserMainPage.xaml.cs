using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using System.Windows.Media.Imaging;
using Pictionary.Views;

namespace Pictionary
{
    public partial class UserMainPage : ContentPage
    {
        /// <summary>
        /// Contains WordViewModel objects that contains picture pixabay url, word in language1 and meanings in language2
        /// </summary>
        public List<WordViewModel> words { get; set; } = new List<WordViewModel>();

        /// <summary>
        /// Languages of the dictionary
        /// </summary>
        public List<string> lanugages { get; set; }

        public static string Language1 { get; set; }
        public static string Language2 { get; set; }

        private string responseContent;

        /// <summary>
        /// Contains words from server: id, imageurl, word in language1, meanings in language2
        /// </summary>
        private List<Tuple<string, string, string, Tuple<string, string>>> wordsFromServer = new List<Tuple<string, string, string, Tuple<string, string>>>();

        private string meaningStr;

        /// <summary>
        /// Initialize language pickers
        /// </summary>
        private void InitLanguagePickers()
        {
            foreach (var item in ApiController.Languages)
            {
                Language1PCK.Items.Add(item.Item2);
                Language2PCK.Items.Add(item.Item2);
            }
        }

        /// <summary>
        /// Constructor add Appearing event and initialize language pickers
        /// </summary>
        public UserMainPage()
        {
            InitializeComponent();
            InitLanguagePickers();

            try
            {
                Init();
            }
            catch (Exception ex)
            {
                DisplayAlert("title", "On UserMainPage_Appearing " + ex.Message, "OK");
            }
            //this.LayoutChanged += UserMainPage_Appearing;
        }

        /// <summary>
        /// Initialize WordsFromServer
        /// </summary>
        /// <param name="responseContent">Response JSON from server</param>
        /// <returns></returns>
        private async Task GetWordsFromServer(string responseContent)
        {
            wordsFromServer.Clear();
            string responseStr = "";
            try
            {
                if (responseContent != "words not found!")
                {
                    dynamic word = JsonConvert.DeserializeObject(responseContent);
                    foreach (var jsonItem in word)
                    {
                        HttpResponseMessage imageResponse = await ApiController.GetImageUrlById(jsonItem.url.ToString());

                        if (imageResponse.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            responseStr = await imageResponse.Content.ReadAsStringAsync();

                            dynamic imageObject = JObject.Parse(responseStr);

                            dynamic hits = imageObject["hits"][0];

                            string imageUri = hits["previewURL"].ToString();

                            foreach (var oneWordJSON in jsonItem)
                            {
                                if (oneWordJSON.Name == "words")
                                {
                                    foreach (var oneWord in oneWordJSON)
                                    {
                                        foreach (var reallyOneWord in oneWord)
                                        {
                                            wordsFromServer.Add(
                                                new Tuple<string, string, string, Tuple<string, string>>(
                                                    jsonItem._id.ToString(),
                                                    jsonItem.url.ToString(),
                                                    imageUri,
                                                    new Tuple<string, string>(reallyOneWord.language.ToString(), reallyOneWord.meaning.ToString())
                                                )
                                            );
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // pixabay too much request error
                            if (imageResponse.StatusCode.ToString() == "429")
                            {
                                continue;
                            }
                            else
                            {
                                await DisplayAlert("GetWordsFromServer: " + imageResponse.StatusCode.ToString(), imageResponse.ReasonPhrase, "OK");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("GetWordsFromServer", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Initialize WordsLSV
        /// </summary>
        /// <param name="language1"></param>
        /// <param name="language2"></param>
        /// <returns></returns>
        private async Task InitWordsLSV(string language1, string language2)
        {
            try
            {
                WordsLSV.BeginRefresh();
                words.Clear();
                meaningStr = "";

                int i = 0;
                List<string> temp = new List<string>();
                List<string> temp2 = new List<string>();

                foreach (var item in wordsFromServer)
                {
                    if (item.Item4.Item1 == language1)
                    {
                        temp2.Add(item.Item4.Item2);
                    }

                    if (item.Item4.Item1 == language2)
                    {
                        temp.Add(item.Item4.Item2);
                    }

                    if (i == wordsFromServer.Count - 1)
                    {
                        string uri = item.Item2;

                        int j = 0;
                        foreach (string meaning in temp)
                        {
                            if (j == 0)
                            {
                                meaningStr += meaning;
                            }
                            else
                            {
                                meaningStr += "," + meaning;
                            }
                            j++;
                        }

                        foreach (string it in temp2)
                        {
                            if (meaningStr != "")
                            {
                                words.Add(new WordViewModel { Id = item.Item1, ImageId = item.Item2, Picture = item.Item3, Word = it, Meaning = temp, MeaningStr = meaningStr });
                            }
                        }

                        meaningStr = "";

                        temp.Clear();
                        temp2.Clear();
                    }

                    if (i < wordsFromServer.Count - 1 && wordsFromServer[i + 1].Item1 != wordsFromServer[i].Item1)
                    {
                        string uri = item.Item2;

                        int j = 0;
                        foreach (string meaning in temp)
                        {
                            if (j == 0)
                            {
                                meaningStr += meaning;
                            }
                            else
                            {
                                meaningStr += "," + meaning;
                            }
                            j++;
                        }

                        foreach (string it in temp2)
                        {
                            if (meaningStr != "")
                            {
                                words.Add(new WordViewModel { Id = item.Item1, ImageId = item.Item2, Picture = item.Item3, Word = it, Meaning = temp, MeaningStr = meaningStr });
                            }
                        }

                        meaningStr = "";

                        temp.Clear();
                        temp2.Clear();
                    }

                    i++;
                }

                WordsLSV.ItemsSource = null;
                WordsLSV.ItemsSource = words;
                WordsLSV.EndRefresh();
            }
            catch (Exception ex)
            {
                await DisplayAlert("InitWordsLSV", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Initializer function
        /// </summary>
        /// <returns></returns>
        public async Task Init()
        {
            try
            {
                HttpResponseMessage response = await ApiController.GetWords();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                    await GetWordsFromServer(responseContent);

                    await SelectionChanged();
                }
                else
                {
                    await DisplayAlert(response.StatusCode.ToString(), response.ReasonPhrase, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Init", ex.Message, "OK");
            }
        }

        private async Task SelectionChanged()
        {
            try
            {
                if (Language1PCK.SelectedIndex != -1 && Language2PCK.SelectedIndex != -1)
                {
                    Language1 = Language1PCK.Items[Language1PCK.SelectedIndex];
                    Language2 = Language2PCK.Items[Language2PCK.SelectedIndex];

                    await InitWordsLSV(Language1, Language2);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Language1PCK_SelectedIndexChanged", "appeared" + ex.Message, "OK");
            }
        }

        //event handlers

        private async void Language1PCK_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SelectionChanged();
        }

        private async void Language2PCK_SelectedIndexChanged(object sender, EventArgs e)
        {
            await SelectionChanged();
        }

        private async void AddNewWordBTN_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddWordPage());
        }

        private async void WordsLSV_ItemSelected(object sender, EventArgs e)
        {
            try
            {
                WordViewModel selectedItem = (WordViewModel)WordsLSV.SelectedItem;

                WordsLSV.SelectedItem = null;

                await Navigation.PushAsync(new EditWordPage(selectedItem));
            }
            catch { }
        }

        private async void WordsLSVMIT_Clicked(object sender, EventArgs e)
        {
            try
            {
                MenuItem m = (MenuItem)sender;

                WordViewModel wvm = (WordViewModel)m.CommandParameter;

                HttpResponseMessage response;

                response = await ApiController.DeleteWord(wvm.Id);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var stack = Navigation.NavigationStack;

                    UserMainPage page = (UserMainPage)stack[0];

                    await page.Init();
                }
                else
                {
                    await DisplayAlert(response.StatusCode.ToString(), response.ReasonPhrase, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("WordsLSVMIT_Clicked", ex.Message, "OK");
            }
        }

        private void LogoutBTN_Clicked(object sender, EventArgs e)
        {
            NavigationPage loginPage = new NavigationPage(new LoginPage());
            Application.Current.MainPage = loginPage;
        }
    }
}

using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Pictionary
{
    static class ApiController
    {
        public static string Password { get; set; }
        public static string UserId { get; set; }
        public static string Email { get; set; }

        public static readonly string ServerURL = "http://192.168.1.17:3000/";

        public static readonly List<Tuple<string, string>> Languages = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("en", "English"),
            new Tuple<string, string>("hu", "Magyar"),
            new Tuple<string, string>("cs", "čeština"),
            new Tuple<string, string>("da", "dansk"),
            new Tuple<string, string>("de", "Deutsch"),
            new Tuple<string, string>("es", "Español"),
            new Tuple<string, string>("fr", "français"),
            new Tuple<string, string>("id", "Bahasa Indonesia"),
            new Tuple<string, string>("it", "Italiano"),
            new Tuple<string, string>("nl", "Nederlands"),
            new Tuple<string, string>("no", "Norsk"),
            new Tuple<string, string>("pl", "język polski"),
            new Tuple<string, string>("pt", "Português"),
            new Tuple<string, string>("ro", "Română"),
            new Tuple<string, string>("sk", "slovenčina"),
            new Tuple<string, string>("fi", "suomi"),
            new Tuple<string, string>("sv", "svenska"),
            new Tuple<string, string>("tr", "Türkçe"),
            new Tuple<string, string>("vi", "Tiếng Việt"),
            new Tuple<string, string>("th", "ไทย"),
            new Tuple<string, string>("bg", "български език"),
            new Tuple<string, string>("ru", "Русский"),
            new Tuple<string, string>("el", "ελληνικά"),
            new Tuple<string, string>("ja", "日本語 (にほんご)"),
            new Tuple<string, string>("ko", "한국어"),
            new Tuple<string, string>("zh", "isiZulu")
        };

        private static HttpClient client = new HttpClient();


        /// <summary>
        /// Initialize client for pixabay API
        /// it is needed because if I send the basic auth header the pixabay api does not respond
        /// </summary>
        private static HttpClient pixabayClient = new HttpClient();

        /// <summary>
        /// Initialize client for the api server
        /// </summary>
        private static void InitClient()
        {
            client.Timeout = new TimeSpan(30000000);
            var byteArray = Encoding.UTF8.GetBytes(Email + ":" + Password);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        /// <summary>
        /// Get User Id from the API
        /// </summary>
        /// <returns>HttpResponseMessage that received from the API</returns>
        public static async Task<HttpResponseMessage> Login()
        {
            InitClient();

            HttpResponseMessage response = await client.GetAsync(ServerURL + "users?email=" + Email);

            return response;
        }

        /// <summary>
        /// Post word to the API
        /// </summary>
        /// <param name="url">Pixabay image id</param>
        /// <param name="word">The word in language 1</param>
        /// <param name="meanings">The meanings in language 2</param>
        /// <returns>HttpResponseMessage that received from the API</returns>
        public static async Task<HttpResponseMessage> PostWord(string url, string word, List<string> meanings)
        {
            string contentStr = "{\"userId\":\"" + UserId + "\"," + "\"url\": \"" + url + "\",\"words\":[{\"language\":\"" + UserMainPage.Language1 + "\",\"meaning\":\"" + word + "\"}";

            foreach (var item in meanings)
            {
                contentStr += ",{\"language\":\"" + UserMainPage.Language2 + "\",\"meaning\":\"" + item + "\"}";
            }

            contentStr += "]}";

            var content = new StringContent(contentStr, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(ServerURL + "words/", content);

            return response;
        }

        /// <summary>
        /// Get pixabay image url by id
        /// </summary>
        /// <param name="id">Id of the pixabay image</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> GetImageUrlById(string id)
        {
            HttpResponseMessage response = await pixabayClient.GetAsync("https://pixabay.com/api/?key=4596390-5d3b2f6c671970de85afbd0b0&id=" + id);

            return response;
        }

        /// <summary>
        /// Search images on pixabay
        /// </summary>
        /// <param name="image">Images that we want to search</param>
        /// <param name="language">Language of the search</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> GetImages(string image)
        {
            string language = "";

            foreach (var item in Languages)
            {
                if (item.Item2 == UserMainPage.Language1)
                {
                    language = item.Item1;
                }
            }

            HttpResponseMessage response = await pixabayClient.GetAsync("https://pixabay.com/api/?key=4596390-5d3b2f6c671970de85afbd0b0&q=" + image + "&per_page=15&page=1" + "&lang=" + language);

            return response;
        }

        public static async Task<HttpResponseMessage> GetWords()
        {
            HttpResponseMessage response = await client.GetAsync(ServerURL + "words?userId=" + UserId);

            return response;
        }

        public static async Task<HttpResponseMessage> EditWord(string id, string origMeaning, string origUrl, string meaning = "", string url = "")
        {
            string contentStr = "{\"origMeaning\":\"" + origMeaning + "\"," + "\"meaning\":\"" + meaning + "\"," + "\"origUrl\":\"" + origUrl + "\"," + "\"url\":\"" + url + "\"}";

            var content = new StringContent(contentStr, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync(ServerURL + "words/" + id + "/edit", content);
            return response;

        }

        public static async Task<HttpResponseMessage> AddMeaning(string id, string languge, string meaning)
        {
            string contentStr = "{\"language\":\"" + languge + "\"," + "\"meaning\":\"" + meaning + "\"}";

            var content = new StringContent(contentStr, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync(ServerURL + "words/" + id + "/add", content);
            return response;

        }

        public static async Task<HttpResponseMessage> DeleteMeaning(string id, string languge, string meaning)
        {
            string contentStr = "{\"language\":\"" + languge + "\"," + "\"meaning\":\"" + meaning + "\"}";

            var content = new StringContent(contentStr, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PutAsync(ServerURL + "words/" + id + "/del", content);
            return response;

        }

        public static async Task<HttpResponseMessage> DeleteWord(string id)
        {
            HttpResponseMessage response = await client.DeleteAsync(ServerURL + "words/" + id);
            return response;
        }

        public static async Task<HttpResponseMessage> SignUp(string email, string password)
        {
            string contentStr = "{\"email\":\"" + email + "\"," + "\"password\":\"" + password + "\"}";

            var content = new StringContent(contentStr, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(ServerURL + "users/", content);

            return response;
        }

    }

}

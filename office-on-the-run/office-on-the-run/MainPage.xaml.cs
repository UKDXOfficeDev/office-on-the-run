using Microsoft.Band;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace office_on_the_run
{
    public class CommonAPIData
    {
        public string accessToken { get; set; }
        public string groupId { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private CommonAPIData APIData;
        private IBandClient _bandClient;
        private IBandInfo _bandInfo;

        public MainPage()
        {
            this.InitializeComponent();

            // add callback for on load trigger
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Restrict to run once only
            if (APIData == null)
            {
                APIData = new CommonAPIData();

                // Access token must be initialised before using the API
                await InitAcessToken();
                // Now gather all the necessary variables by probing the API
                await InitAPIData();
            }
        }

        private async Task InitAcessToken()
        {
            APIData.accessToken = await AuthenticationHelper.GetTokenHelperAsync();
        }

        private async Task InitAPIData()
        {
            // Request the outlook groups from our tenant and fetch the first ones ID
            var response = await SendGetRequest("https://graph.microsoft.com/beta/dxdev01.onmicrosoft.com/groups?$top");
            var group = JsonConvert.DeserializeObject<RootObject>(response);
            APIData.groupId = group.value.First().objectId;
        }

        private async Task SendPostRequest(string url,
                                           string data,
                                           string contentType = "application/json")
        {
            if (APIData.accessToken != null)
            {
                var content = new HttpStringContent(data, Windows.Storage.Streams.UnicodeEncoding.Utf8, contentType);
                var http = new HttpClient();
                http.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", APIData.accessToken);
                var response = await http.PostAsync(new Uri(url), content);

                // do something with response code
            }
            else
            {
                throw new Exception("An access token must be present before calling the API");
            }
        }

        private async Task<string> SendGetRequest(string url,
                                          string access = "application/json")
        {
            HttpResponseMessage response = null;

            if (APIData.accessToken != null)
            {
                var http = new HttpClient();
                http.DefaultRequestHeaders.Add("Accept", access);
                http.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", APIData.accessToken);
                response = await http.GetAsync(new Uri(url));
            }
            else
            {
                throw new Exception("An access token must be present before calling the API");
            }

            return response.Content.ReadAsStringAsync().GetResults();
        }

        public async Task InitBand()
        {
            if (_bandClient != null)
                return;

            var bands = await BandClientManager.Instance.GetBandsAsync();
            _bandInfo = bands.First();

            _bandClient = await BandClientManager.Instance.ConnectAsync(_bandInfo);

            var uc = _bandClient.SensorManager.HeartRate.GetCurrentUserConsent();
            bool isConsented = false;
            if (uc == UserConsent.NotSpecified)
            {
                isConsented = await _bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
            }

            if (isConsented || uc == UserConsent.Granted)
            {
                _bandClient.SensorManager.HeartRate.ReadingChanged += async (obj, ev) =>
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        HeartRateDisplay.Text = ev.SensorReading.HeartRate.ToString();

                        /*
                            Enter band threshold code here.
                        */
                    });
                };
                await _bandClient.SensorManager.HeartRate.StartReadingsAsync();
            }
        }

        private async Task CreateGroupEvent(string title, string content, DateTime start, DateTime end)
        {
            // Date format: 2015-10-23T18:00:00-08:00

            var eventString = "{" +
                              "\"Subject\": \"" + title + "\"," +
                              "\"Body\": {" +
                                "\"ContentType\": \"HTML\"," +
                                "\"Content\": \"" + content + "\"" +
                              "}," +
                              "\"Start\": \"2015-10-23T18:00:00-08:00\"," +
                              "\"StartTimeZone\": \"Pacific Standard Time\"," +
                              "\"End\": \"2015-10-23T18:00:00-08:00\"," +
                              "\"EndTimeZone\": \"Pacific Standard Time\"" +
                              "}";

            await SendPostRequest("https://graph.microsoft.com/beta/dxdev01.onmicrosoft.com/groups('" + APIData.groupId + "')/events",
                            eventString);
        }

        private async void StartClick(object sender, RoutedEventArgs e)
        {
            await InitBand();
        }

        private async void AddGroupEventClick(object sender, RoutedEventArgs e)
        {
            // Create a mock event
            var start = DateTime.Now;
            var end = start.Add(TimeSpan.FromHours(1));
            var title = "Park 10km Run this weekend!";
            var content = "Please join me in competeing in our local park run.";

            await CreateGroupEvent(title, content, start, end);
        }
    }

    public class Value
    {
        public string objectType { get; set; }
        public string objectId { get; set; }
        public object deletionTimestamp { get; set; }
        public object description { get; set; }
        public object dirSyncEnabled { get; set; }
        public string displayName { get; set; }
        public List<object> creationOptions { get; set; }
        public List<string> groupTypes { get; set; }
        public bool isPublic { get; set; }
        public object lastDirSyncTime { get; set; }
        public string mail { get; set; }
        public string mailNickname { get; set; }
        public bool mailEnabled { get; set; }
        public object onPremisesSecurityIdentifier { get; set; }
        public List<object> provisioningErrors { get; set; }
        public List<string> proxyAddresses { get; set; }
        public bool securityEnabled { get; set; }
    }

    public class RootObject
    {
        public string context { get; set; }
        public List<Value> value { get; set; }
    }
}

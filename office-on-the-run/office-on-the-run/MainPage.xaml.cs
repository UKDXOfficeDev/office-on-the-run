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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IBandClient _bandClient;
        private IBandInfo _bandInfo;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
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
                    });
                };
                await _bandClient.SensorManager.HeartRate.StartReadingsAsync();
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {

            var token = await AuthenticationHelper.GetTokenHelperAsync();

            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Accept", "application/json");
            http.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);

            var resp = await http.GetAsync(new Uri("https://graph.microsoft.com/beta/dxdev01.onmicrosoft.com/groups?$top"));
            var ret = await resp.Content.ReadAsStringAsync();

            var group = JsonConvert.DeserializeObject<RootObject>(ret);

            var objID = group.value.First().objectId;

            var myStr = @"{
  ""Subject"": ""Discuss the Calendar REST API"",
  ""Body"": {
                ""ContentType"": ""HTML"",
    ""Content"": ""I think it will meet our requirements!""
  },
  ""Start"": ""2016-02-02T18:00:00-08:00"",
  ""StartTimeZone"": ""Pacific Standard Time"",
  ""End"": ""2016-02-02T19:00:00-08:00"",
  ""EndTimeZone"": ""Pacific Standard Time""
}";

            var content = new HttpStringContent(myStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");

            var http2 = new HttpClient();
            http2.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);

            resp = await http2.PostAsync(new Uri($"https://graph.microsoft.com/beta/dxdev01.onmicrosoft.com/groups('{objID}')/events"), content);

            //            https://graph.microsoft.com/beta/contoso.com/groups('c75831bd-fad3-4191-9a66-280a48528679')/events
            //            Content - Type: application / json


            //https://graph.microsoft.com/beta/fitnesscloud.co.uk/users?$filter=startswith(mailNickname,+'ad')

                //var resp = await http.PostAsync();
            }

        private async void StartClick(object sender, RoutedEventArgs e)
        {
            await InitBand();
        }

        public async Task AddToCalendar()
        {
            var token = await AuthenticationHelper.GetTokenHelperAsync();

            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Accept", "application/json");
            http.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);

            var resp = await http.GetAsync(new Uri("https://graph.microsoft.com/beta/dxdev01.onmicrosoft.com/groups?$top"));
            var ret = await resp.Content.ReadAsStringAsync();

            var group = JsonConvert.DeserializeObject<RootObject>(ret);

            var objID = group.value.First().objectId;

            var myStr = @"{
  ""Subject"": ""Discuss the Calendar REST API"",
  ""Body"": {
                ""ContentType"": ""HTML"",
    ""Content"": ""I think it will meet our requirements!""
  },
  ""Start"": ""2016-02-02T18:00:00-08:00"",
  ""StartTimeZone"": ""Pacific Standard Time"",
  ""End"": ""2016-02-02T19:00:00-08:00"",
  ""EndTimeZone"": ""Pacific Standard Time""
}";

            var content = new HttpStringContent(myStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");

            var http2 = new HttpClient();
            http2.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);

            resp = await http2.PostAsync(new Uri($"https://graph.microsoft.com/beta/dxdev01.onmicrosoft.com/groups('{objID}')/events"), content);

            //            https://graph.microsoft.com/beta/contoso.com/groups('c75831bd-fad3-4191-9a66-280a48528679')/events
            //            Content - Type: application / json


            //https://graph.microsoft.com/beta/fitnesscloud.co.uk/users?$filter=startswith(mailNickname,+'ad')

            //var resp = await http.PostAsync();

        }

        private async void AddClick(object sender, RoutedEventArgs e)
        {
            await AddToCalendar();
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

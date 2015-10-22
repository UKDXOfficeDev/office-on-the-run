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
        private string token;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
        }

        private async Task Authorise()
        {
            token = await AuthenticationHelper.GetTokenHelperAsync();
        }

        private async Task<string> GetGroupId()
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("Accept", "application/json");
            http.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);

            var resp = await http.GetAsync(new Uri("https://graph.microsoft.com/beta/dxdev01.onmicrosoft.com/groups?$top"));
            var ret = await resp.Content.ReadAsStringAsync();

            var group = JsonConvert.DeserializeObject<RootObject>(ret);

            return group.value.First().objectId;
        }

        private async void PostGroupEvent(string groupId)
        {
            var startDate = DateTime.Now.ToString();
            var endDate = startDate;

            var myStr = @"{
              ""Subject"": ""Jogging request?"",
              ""Body"": {
                            ""ContentType"": ""HTML"",
                ""Content"": ""Fancy coming for a jog?""
              },
              ""Start"": ""2015-10-23T18:00:00-08:00"",
              ""StartTimeZone"": ""Pacific Standard Time"",
              ""End"": ""2015-10-23T19:00:00-08:00"",
              ""EndTimeZone"": ""Pacific Standard Time""
            }";

            var content = new HttpStringContent(myStr, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");

            var http2 = new HttpClient();
            http2.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", token);

            var resp = await http2.PostAsync(new Uri($"https://graph.microsoft.com/beta/dxdev01.onmicrosoft.com/groups('{groupId}')/events"), content);
            System.Diagnostics.Debug.Write(resp);
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await Authorise();
            var groupId = await GetGroupId();
            PostGroupEvent(groupId);
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

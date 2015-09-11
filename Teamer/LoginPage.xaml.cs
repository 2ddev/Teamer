using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Teamer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage
    {
        public LoginPage()
        {
            InitializeComponent();
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values["login"] != null && localSettings.Values["password"] != null)
            {
                LoginTextBox.Text = localSettings.Values["login"].ToString();
                PasswordBox.Password = localSettings.Values["password"].ToString();
                LoginButton.IsEnabled = false;
                LoginTextBox.IsEnabled = false;
                PasswordBox.IsEnabled = false;
                RememberCheckBox.IsEnabled = false;
                TryLogin();
            }
        }
        private async void TryLogin()
        {
            AuthProgressRing.IsActive = true;
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values["tma"]!= null && localSettings.Values["tmb"] != null && localSettings.Values["tmsid"] != null)
            {
                localSettings.Values.Remove("tma");
                localSettings.Values.Remove("tmb");
                localSettings.Values.Remove("tmsid");
                TryLogin();
            }
            else
            {
                var result = await Auth();
                if (!result) { TryLogin(); } else
                {
                    Frame rootFrame = Window.Current.Content as Frame;
                    rootFrame?.Navigate(typeof(MainPage));
                }
            }
            AuthProgressRing.IsActive = false;
        }

        public async Task<bool> Auth()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            int unixTimestamp = (int)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var uri = new Uri("http://www.teamer.ru/login/" + "?timestamp=" + unixTimestamp);
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                CookieContainer = new CookieContainer()
            };
            handler.CookieContainer.Add(uri, new Cookie("tmsid", ""));
            handler.CookieContainer.Add(uri, new Cookie("tma", ""));
            handler.CookieContainer.Add(uri, new Cookie("tmb", ""));
            handler.UseCookies = true;

            var httpClient = new HttpClient(handler);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("login", localSettings.Values["login"].ToString()),
                new KeyValuePair<string, string>("password", localSettings.Values["password"].ToString()),
                new KeyValuePair<string, string>("unsafeip", "1")
            });
            await httpClient.PostAsync(uri, content);

            var cookiecollection = handler.CookieContainer.GetCookies(new Uri("http://www.teamer.ru/")).Cast<Cookie>();
            httpClient.Dispose();
            var enumerable = cookiecollection as IList<Cookie> ?? cookiecollection.ToList();
            if (enumerable.Count == 3)
            {
                foreach(Cookie cookie in enumerable)
                {
                    localSettings.Values[cookie.Name] = cookie.Value;
                }
                return true;

            } else
            {
                return false;
            }
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (LoginTextBox.Text == "" || PasswordBox.Password == "")
            {
                return;
            }
            localSettings.Values["login"] = LoginTextBox.Text;
            localSettings.Values["password"] = PasswordBox.Password;
            LoginButton.IsEnabled = false;
            LoginTextBox.IsEnabled = false;
            PasswordBox.IsEnabled = false;
            RememberCheckBox.IsEnabled = false;
            TryLogin();
            if (RememberCheckBox.IsChecked == false)
            {
                localSettings.Values.Remove("login");
                localSettings.Values.Remove("password");
            }
        }
    }
}
    

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Teamer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values["login"] != null && localSettings.Values["password"] != null)
            {
                this.loginTextBox.Text = localSettings.Values["login"].ToString();
                this.passwordBox.Password = localSettings.Values["password"].ToString();
                this.loginButton.IsEnabled = false;
                this.loginTextBox.IsEnabled = false;
                this.passwordBox.IsEnabled = false;
                this.rememberCheckBox.IsEnabled = false;
                tryLogin();
            }
        }
        private async void tryLogin()
        {
            this.authProgressRing.IsActive = true;
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            if (localSettings.Values["tma"]!= null && localSettings.Values["tmb"] != null && localSettings.Values["tmsid"] != null)
            {
                localSettings.Values.Remove("tma");
                localSettings.Values.Remove("tmb");
                localSettings.Values.Remove("tmsid");
                tryLogin();
            }
            else
            {
                var result = await auth();
                if (!result) { tryLogin(); } else
                {
                    Frame rootFrame = Window.Current.Content as Frame;
                    rootFrame.Navigate(typeof(MainPage));
                }

            }
            this.authProgressRing.IsActive = false;
        }

        public async Task<bool> auth()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            int unixTimestamp = (int)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var uri = new Uri("http://www.teamer.ru/login/" + "?timestamp=" + unixTimestamp);
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            handler.CookieContainer = new CookieContainer();
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
            var wat = await content.ReadAsStringAsync();
            HttpResponseMessage response = (await httpClient.PostAsync(uri, content));
            var stringResp = await response.Content.ReadAsStringAsync();
            var cookiecollection = handler.CookieContainer.GetCookies(new Uri("http://www.teamer.ru/")).Cast<Cookie>();
            httpClient.Dispose();
            if (cookiecollection.Count() == 3)
            {
                foreach(Cookie cookie in cookiecollection)
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
            if (this.loginTextBox.Text == "" || this.passwordBox.Password == "")
            {
                return;
            }
            localSettings.Values["login"] = this.loginTextBox.Text;
            localSettings.Values["password"] = this.passwordBox.Password;
            this.loginButton.IsEnabled = false;
            this.loginTextBox.IsEnabled = false;
            this.passwordBox.IsEnabled = false;
            this.rememberCheckBox.IsEnabled = false;
            tryLogin();
            if (this.rememberCheckBox.IsChecked == false)
            {
                localSettings.Values.Remove("login");
                localSettings.Values.Remove("password");
            }
        }
    }
}
    

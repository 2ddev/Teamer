using HtmlAgilityPack;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Teamer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TaskPage : Page
    {
        public TaskPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.InitializeComponent();
            var parameter = e.Parameter as TeamerTask;
            updateUI(parameter);
            Debug.WriteLine("Whoop");
        }
        
        public async void updateUI(TeamerTask task)
        {

            //refreshLoader(true);
            TeamerTask result = await loadTaskFromTask(task);
            if (result != null)
            {
                showContent(result);

            }
        }
        public void showContent(TeamerTask task)
        {
            
        }
        public void refreshLoader(bool active)
        {
            if (active)
            {
                this.loadingProgressRing.IsActive = true;
            }
            else
            {
                this.loadingProgressRing.IsActive = false;
            }
            double size = Window.Current.Bounds.Height;
            if (Window.Current.Bounds.Height > Window.Current.Bounds.Width)
            {
                size = Window.Current.Bounds.Width;
            }
            this.loadingProgressRing.Height = size;
            this.loadingProgressRing.Width = size;
        }
        

        public async Task<TeamerTask> loadTaskFromTask(TeamerTask task)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var uri = new Uri(task.Link);
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = false;
            handler.CookieContainer = new CookieContainer();
            handler.CookieContainer.Add(uri, new Cookie("tmsid", localSettings.Values["tmsid"].ToString()));
            handler.CookieContainer.Add(uri, new Cookie("tma", localSettings.Values["tma"].ToString()));
            handler.CookieContainer.Add(uri, new Cookie("tmb", localSettings.Values["tmb"].ToString()));
            handler.UseCookies = true;
            handler.AllowAutoRedirect = true;

            var httpClient = new HttpClient(handler);
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            string stringResp = await response.Content.ReadAsStringAsync();
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(stringResp);
            var tasknameNode = html.GetElementbyId("hbc");
           
            var comments = html.GetElementbyId("comments");
            string times = comments.ChildNodes[0].InnerText.Replace("\n", "").Replace("&mdash;", "-").TrimStart(' ').TrimEnd(' ');
            var body = html.DocumentNode.ChildNodes[2].ChildNodes[5];
            var commenttxt = body.ChildNodes[1].ChildNodes[13].InnerText.Replace("\n","").TrimStart(' ').TrimEnd(' ');
            task.Description = commenttxt;
            var start = times.Replace("Задание поставлено ", "").Replace(" пользователем","\tпользователем").Split('\t')[0];
            var deadline = times.TrimEnd('.').Replace("Дедлайн - ", "Дедлайн -\t").Split('\t')[1];
            task.Start = start;
            task.Deadline = deadline;
            httpClient.Dispose();
            return task;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}

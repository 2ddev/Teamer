using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Teamer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TaskPage
    {
        public TaskPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitializeComponent();
            var parameter = e.Parameter as TeamerTask;
            UpdateUi(parameter);
            Debug.WriteLine("Whoop");
        }
        
        public async void UpdateUi(TeamerTask task)
        {

            //refreshLoader(true);
            TeamerTask result = await LoadTaskFromTask(task);
            if (result != null)
            {
                ShowContent(result);

            }
        }
        public void ShowContent(TeamerTask task)
        {
            var projectNameTextBlock = FindNameInSubtree<TextBlock>(this, "ProjectName");
            var taskTextTextBlock = FindNameInSubtree<TextBlock>(this, "TaskText");
            var taskDescriptionTextBlock = FindNameInSubtree<TextBlock>(this, "TaskDescription");
            var taskTimeTextBlock = FindNameInSubtree<TextBlock>(this, "TaskTime");
            projectNameTextBlock.Text = task.ParentProject.Name;
            taskTextTextBlock.Text = task.Text;
            if (task.Description != null)
            {
                taskDescriptionTextBlock.Text = task.Description;
            }
            taskTimeTextBlock.Text = task.Start + " -> " + task.Deadline;
            LoadingProgressRing.IsActive = false;
        }
        public T FindNameInSubtree<T>(FrameworkElement element, string descendantName) where T : FrameworkElement
        {
            if (element == null)
                return null;
            if (element.Name == descendantName)
                return element as T;
            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childrenCount; i++)
            {
                var result = FindNameInSubtree<T>(VisualTreeHelper.GetChild(element, i) as FrameworkElement, descendantName);
                if (result != null)
                    return result;
            }
            return null;
        }
        public void RefreshLoader(bool active)
        {
            LoadingProgressRing.IsActive = active;
            double size = Window.Current.Bounds.Height;
            if (Window.Current.Bounds.Height > Window.Current.Bounds.Width)
            {
                size = Window.Current.Bounds.Width;
            }
            LoadingProgressRing.Height = size;
            LoadingProgressRing.Width = size;
        }
        

        public async Task<TeamerTask> LoadTaskFromTask(TeamerTask task)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var uri = new Uri(task.Link);
            HttpClientHandler handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer(),
                AllowAutoRedirect = false
            };
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
           
            var comments = html.GetElementbyId("comments");
            string times = comments.ChildNodes[0].InnerText.Replace("\n", "").Replace("&mdash;", "-").TrimStart(' ').TrimEnd(' ');
            var body = html.DocumentNode.ChildNodes[2].ChildNodes[5];
            var commenttxt = body.ChildNodes[1].ChildNodes[13].InnerText.Replace("\n","").TrimStart(' ').TrimEnd(' ');
            if (commenttxt != null) task.Description = commenttxt;
            string start = "";
            if (times.Replace("Задание поставлено ", "").Replace(" пользователем","\tпользователем").Split('\t').Length > 0)
            {
                start = times.Replace("Задание поставлено ", "").Replace(" пользователем","\tпользователем").Split('\t')[0];
                
            }
            string deadline = "";
            if (times.TrimEnd('.').Replace("Дедлайн - ", "Дедлайн -\t").Split('\t').Length > 1)
            {
                deadline = times.TrimEnd('.').Replace("Дедлайн - ", "Дедлайн -\t").Split('\t')[1];

            }
            if (start != null) task.Start = start;
            if (deadline != null) task.Deadline = deadline;
            httpClient.Dispose();
            return task;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}

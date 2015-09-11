using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using HtmlAgilityPack;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Teamer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class TeamerTask
    {
        public TeamerTask()
        {
            Text = "";
            Id = "";
            Start = "";
            Deadline = "";
            Creator = "";
            Description = "";
            Link = "";
        }
        public TeamerTask(string text, string id, TeamerProject parentProject)
        {
            Text = text;
            Id = id;
            ParentProject = parentProject;
        }
        public TeamerTask(HtmlNode node)
        {
            Debug.WriteLine("Beep");
            List<HtmlNode> pureNodes = new List<HtmlNode>();
            foreach (HtmlNode child in node.ChildNodes)
            {
                if (child.Name != "#text")
                {
                    pureNodes.Add(child);
                }
            }
            if (pureNodes[0].Name == "th")
            {
                var desc = pureNodes[0].InnerText;
                desc = desc.Replace("\n", "").Replace("    ","\t");
                Text = desc.Split('\t')[0];
                Creator = desc.Split('\t')[1];
                Id = pureNodes[0].Id.Substring(4);

            }
            //if (pureNodes[2].Name == "td")
            //{
            //    var start = pureNodes[2].InnerText;
            //    Start = start;
            //}
            //if (pureNodes[3].Name == "td")
            //{
            //    var deadline = pureNodes[3].InnerText;
            //    Deadline = deadline;
            //}
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(node.InnerHtml);
            Link = doc.GetElementbyId("task" + Id).GetAttributeValue("href","/");

            Debug.WriteLine("Beep");

        }
        public void AddMoreFromHtml(HtmlDocument doc)
        {

        }
        public string Text,Id,Start,Deadline,Creator,Description,Link;
        public TeamerProject ParentProject;
    }
    public class TeamerProject
    {
        public string Name, Id;
        public List<TeamerTask> Tasks;
        public void AddTask(TeamerTask task)
        {
            if (Tasks == null)
            {
                Tasks = new List<TeamerTask>();
            }
            task.ParentProject = this;
            Tasks.Add(task);
        }
        public void AddTask(string text, string id)
        {
            if (Tasks == null)
            {
                Tasks = new List<TeamerTask>();
            }
            TeamerTask task = new TeamerTask(text, id, this);
            Tasks.Add(task);
        }
        public void RemoveTask(string id)
        {
            foreach(TeamerTask task in Tasks)
            {
                if (task.Id == id)
                {
                    Tasks.Remove(task);
                    break;
                }
            }
        }
        public TeamerProject()
        {
            Name = "";
            Id = "";
            Tasks = new List<TeamerTask>();

        }
        public TeamerProject(HtmlNode node)
        {
            Debug.WriteLine("Beep");
            Name = node.ChildNodes[1].ChildNodes[1].ChildNodes[1].InnerText;
            Id = node.Id.Split('_')[1];
            //List<HtmlNode> tasklistHTML = new List<HtmlNode>();
            if (node.ChildNodes.Count >= 6)
            {
                
                foreach(HtmlNode taskHtml in node.ChildNodes[3].ChildNodes)
                {
                    if (taskHtml.Name == "tr")
                    {
                        //tasklistHTML.Add(taskHTML);
                        AddTask(new TeamerTask(taskHtml));
                    }
                }
            }
            Debug.WriteLine("Boop");
        }
    }
    public sealed partial class MainPage
    {
        
        public MainPage()
        {
            InitializeComponent();
            UpdateUi();
        }

        

        public async void UpdateUi()
        {
            List<TeamerProject> result = await LoadContent();
            if (result!=null)
            {
                ShowContent(result);
            }
        }
        public async Task<List<TeamerProject>> LoadContent()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var uri = new Uri("http://www.teamer.ru/");
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                CookieContainer = new CookieContainer()
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
            var projectsNode = html.GetElementbyId("w");
            if (projectsNode == null)
            {
                //return;
            }
            httpClient.Dispose();
            var projectsNodes = html.GetElementbyId("w").ChildNodes;
            List<TeamerProject> teamerprojects = new List<TeamerProject>();
            foreach(HtmlNode node in projectsNodes)
            {
                if (node.Name == "div")
                {
                    teamerprojects.Add(new TeamerProject(node));
                }

            }
            //projects.
            //Debug.WriteLine("hey!");
            return teamerprojects;
        }
        public async Task<HtmlDocument> GetLinkAsync(string link)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var uri = new Uri(link);
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                CookieContainer = new CookieContainer()
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
            return html;
        }
        public void ShowContent(List<TeamerProject> content)
        {
            foreach(TeamerProject project in content)
            {
                if (project.Tasks!=null)
                {
                    var panel = FindNameInSubtree<ListView>(this, "taskListView");
                    ListViewHeaderItem headerItem = new ListViewHeaderItem {Content = project.Name};
                    headerItem.Tapped += headerItem_Tapped;
                    if (panel.Items != null)
                    {
                        panel.Items.Add(headerItem);
                        foreach(TeamerTask task in project.Tasks)
                        {
                            TextBlock textBlock = new TextBlock
                            {
                                Text = task.Text,
                                Tag = task,
                                IsTapEnabled = true
                            };
                            textBlock.Tapped += textBlock_Tapped;
                            panel.Items.Add(textBlock);
                        }
                    }
                }
            }
        }
        private void headerItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
        }
        private void textBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            var textBlock = sender as TextBlock;
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null)
                if (textBlock != null) rootFrame.Navigate(typeof(TaskPage), textBlock.Tag as TeamerTask);
            //XmlDocument doc = new XmlDocument();
            //var doc = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            //XmlNodeList toastTextElements = doc.GetElementsByTagName("text");
            //toastTextElements[0].AppendChild(doc.CreateTextNode(task.Text+" by "+ task.Creator));
            //ToastNotification toast = new ToastNotification(doc);
            //toast.Tag = Guid.NewGuid().GetHashCode().ToString();
            //toast.Group = "Teamer";
            //toast.ExpirationTime = (DateTimeOffset.Now + TimeSpan.FromMinutes(2));
            //ToastNotificationManager.CreateToastNotifier().Show(toast);
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

        private void projectsHubSection_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

        }
    }
}

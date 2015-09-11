using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using HtmlAgilityPack;
using Windows.UI;

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
        public void addMoreFromHtml(HtmlDocument doc)
        {

        }
        public string Text,Id,Start,Deadline,Creator,Description,Link;
        public TeamerProject ParentProject;
    }
    public class TeamerProject
    {
        public string Name, Id;
        public List<TeamerTask> Tasks;
        public void addTask(TeamerTask task)
        {
            if (Tasks == null)
            {
                Tasks = new List<TeamerTask>();
            }
            task.ParentProject = this;
            Tasks.Add(task);
        }
        public void addTask(string text, string id)
        {
            if (Tasks == null)
            {
                Tasks = new List<TeamerTask>();
            }
            TeamerTask task = new TeamerTask(text, id, this);
        }
        public void removeTask(string id)
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
                
                foreach(HtmlNode taskHTML in node.ChildNodes[3].ChildNodes)
                {
                    if (taskHTML.Name == "tr")
                    {
                        //tasklistHTML.Add(taskHTML);
                        this.addTask(new TeamerTask(taskHTML));
                    }
                }
            }
            Debug.WriteLine("Boop");
        }
    }
    public sealed partial class MainPage : Page
    {
        
        public MainPage()
        {
            this.InitializeComponent();
            updateUI();
        }

        

        public async void updateUI()
        {
            List<TeamerProject> result = await loadContent();
            if (result!=null)
            {
                showContent(result);
            }
        }
        public async Task<List<TeamerProject>> loadContent()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var uri = new Uri("http://www.teamer.ru/");
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
            var projectsNode = html.GetElementbyId("w");
            if (projectsNode == null)
            {
                //return;
            }
            httpClient.Dispose();
            var projectsNodes = html.GetElementbyId("w").ChildNodes;
            List<HtmlNode> projects = new List<HtmlNode>();
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
        public async Task<HtmlDocument> getLinkAsync(string link)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var uri = new Uri(link);
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
            return html;
        }
        public void showContent(List<TeamerProject> content)
        {
            foreach(TeamerProject project in content)
            {
                if (project.Tasks!=null)
                {
                    var panel = FindNameInSubtree<ListView>(this, "taskListView");
                    ListViewHeaderItem headerItem = new ListViewHeaderItem();
                    headerItem.Content = project.Name;
                    //headerItem.Tapped += headerItem_Tapped;
                    panel.Items.Add(headerItem);
                    foreach(TeamerTask task in project.Tasks)
                    {
                        TextBlock textBlock = new TextBlock();
                        textBlock.Text = task.Text;
                        textBlock.Tag = task;
                        textBlock.IsTapEnabled = true;
                        textBlock.Tapped += textBlock_Tapped;
                        panel.Items.Add(textBlock);
                    }
                    
                }
            }
        }
        private void headerItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var headerItem = sender as ListViewHeaderItem;
            Frame rootFrame = Window.Current.Content as Frame;

            rootFrame.Navigate(typeof(TaskPage),(headerItem.Tag as TeamerTask).Link);
        }
        private void textBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(TaskPage), textBlock.Tag as TeamerTask);
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SemesterPlanner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationViewMain : Page
    {

        public string CurrentPage = "";


        public NavigationViewMain()
        {
            this.InitializeComponent();
            Loaded += Page_Loaded;

            MasterClass.Cur_NavigationViewMain = this;
            UpdateButtonBinding();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //this will be on first load
            Debug.WriteLine("Page Loaded");

            ChangePageTo("navitem_FileMenu");

        }

        public void ChangePageTo(string page_str)
        {
            Debug.WriteLine("ChangePageTo: " + page_str);

            foreach (NavigationViewItemBase item in NavView.MenuItems)
            {
                if (item is NavigationViewItem && item.Name.ToString() == page_str)
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }

            //when the NavView changes selection, it goes to  NavView_SelectionChanged
        }

        private void NavigateTo(string page_str)
        {
            Debug.WriteLine("Navigating to " + page_str);

            if (frame_nav_content == null) { Debug.WriteLine("Frame is null. Exiting."); return; }

            if (page_str == CurrentPage) { Debug.WriteLine("Requested current page. Exiting."); return; }

            switch (page_str)
            {

                case "navitem_FileMenu":
                    frame_nav_content.Navigate(typeof(FileMenu));
                    break;

                case "navitem_MainPage":
                    frame_nav_content.Navigate(typeof(MainPage));
                    break;

                case "navitem_OverviewPage":
                    frame_nav_content.Navigate(typeof(OverviewPage));
                    break;

                case "navitem_PropertiesPage":
                    frame_nav_content.Navigate(typeof(PropertiesPage));
                    break;

                default:
                    Debug.WriteLine("Invalid page request. Exiting.");
                    return;

            }

            //it gets here if it succeeded in changing the page
            CurrentPage = page_str;



        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {

        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {

        }

        private void ContentFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {

        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem nav_item = (NavigationViewItem)args.SelectedItem;
            string selected_item_name = nav_item.Name;

            NavigateTo(selected_item_name);


        }

        internal void UpdateButtonBinding()
        {
            Debug.WriteLine("UpdateButtonBinding");

            //this sets the enabled binding of the nav buttons (projectloaded = false on new)

            if (MasterClass.Cur_ProjectData == null)
            {
                Debug.WriteLine("  Creating new ProjectData");
                NavView.DataContext = new ProjectData();
            }
            else
            {
                Debug.WriteLine("  Using  MasterClass.Cur_ProjectData");
                NavView.DataContext = MasterClass.Cur_ProjectData;
            }
        }
    }
}

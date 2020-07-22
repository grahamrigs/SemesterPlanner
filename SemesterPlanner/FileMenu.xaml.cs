using System;
using System.Collections.Generic;
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
    public sealed partial class FileMenu : Page
    {
        public FileMenu()
        {
            this.InitializeComponent();
        }

        private void LoadTestFileButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            MasterClass.LoadProject(@"ms-appx:///Assets/TestData/", "Test Semesters 2");
        }

        /*

        //these methods deal with the file menu

        private void FileMenuButton(object sender, RoutedEventArgs e)
        {
            OpenFileMenu();
        }
        public async void OpenFileMenu()
        {
            //on app load, we open the file menu
            contentdialog_file_menu.Visibility = Visibility.Visible;

            ContentDialogResult file_menu_result = await contentdialog_file_menu.ShowAsync();

            if (file_menu_result == ContentDialogResult.Primary)
            {
                //this is for New Project

                if (await VerifiedNewOrLoadProject())
                {
                    MakeNewProject();
                }
            }
            else if (file_menu_result == ContentDialogResult.Secondary)
            {
                //this is for Load Project

                if (await VerifiedNewOrLoadProject())
                {
                    LoadProjectData(@"ms-appx:///Assets/TestData/", "Test Semesters 2");
                }
            }
            else // if (file_menu_result == ContentDialogResult.None)
            {
                //this is for Cancel
                FileMenuCancelled();
            }
        }
        private void FileMenuCancelled()
        {
            //if the user cancelled the file menu but no project is loaded, then we reopen
            if (!glo_ProjectData.ProjectLoaded)
            {
                OpenFileMenu();
            }
        }
        public async Task<bool> VerifiedNewOrLoadProject()
        {
            if (!glo_ProjectData.ProjectLoaded)
            {
                return true;
            }

            ContentDialog contentdialog_close_project = new ContentDialog
            {
                Title = "Close Current Project?",
                Content = "Do you wish to close the current project?",
                PrimaryButtonText = "Close Project",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await contentdialog_close_project.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return true;
            }
            else // if (result == ContentDialogResult.None)
            {
                return false;
            }
        }
        public async void MakeNewProject()
        {
            Debug.WriteLine("MakeNewProject");
            ContentDialogResult make_new_result = await contentdialog_make_new_project.ShowAsync();

            if (make_new_result == ContentDialogResult.Primary)
            {

            }
            else // if (make_new_result == ContentDialogResult.None)
            {
                FileMenuCancelled();
            }
        }

        */

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Size = Windows.Foundation.Size;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SemesterPlanner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        //contains all currently loaded project data, including metadata, entries, columns
        static ProjectData glo_ProjectData = new ProjectData();

        //contains all calculated values that may be needed elsewhere
        static CalculatedData glo_CalculatedData = new CalculatedData();

        //this is the currently selected entryID; default/none-selected is -1
        public string glo_selected_entryID = "";

        //and the currently selected entry, for easy access
        static EntryData glo_selected_EntryData = new EntryData();

        //this is a list of all the property fields with unsaved changes
        List<string> glo_changed_properties_names = new List<string>();

        //this keeps track of if the properties pane is open
        private bool glo_property_pane_open_ = false;
        public bool glo_PropertyPaneOpen
        {
            get { return glo_property_pane_open_; }
            set
            {
                glo_property_pane_open_ = value;

                if (glo_property_pane_open_)
                {
                    OpenPropertiesMethod();
                }
                else
                {
                    ClosePropertiesMethod();
                }

            }
        }

        //this keeps track of if the add new entry window is open
        bool glo_add_new_window_open = false;

        //and the currently selected entry, for easy access
        static EntryData glo_AddNew_EntryData = new EntryData();
        AddNewEntry glo_AddNewEntry = new AddNewEntry();

        //these will store the values in the properties pane, before and after changes
        ChangedProperties glo_ChangedProperties = new ChangedProperties();

        //these are the controls in the properties pane
        static public List<TextBox> glo_txtbox_props_lst = new List<TextBox>();

        //this flag controls if the apply properties needs a verification window
        bool glo_properties_apply_need_verification = false;

        //this is the default titleblock colour
        static public string glo_default_titleblock_colour_hex = "#FFC3C3C3";

        //this is the tag start for entryID
        static public string glo_tag_entryID_prefix = "entryID|";
        static public string glo_tag_colID_prefix = "colID|";


        //these are for handling synchronous scrolling
        private const int ScrollLoopbackTimeout = 500;
        private object _lastScrollingElement;
        private int _lastScrollChange = Environment.TickCount;





        public MainPage()
        {
            this.InitializeComponent();

            glo_txtbox_props_lst.Add(txtbox_prop_entryID);
            glo_txtbox_props_lst.Add(txtbox_prop_title);
            glo_txtbox_props_lst.Add(txtbox_prop_subtitle);

            glo_CalculatedData.ColourPickerChosenHex = "#FFC3C3C3";

        }
        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            //on app start, we will open the file menu
            //OpenFileMenu();
        }





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







        //these methods deal with loading of saved project data

        public async void LoadProjectData(string path, string project_name)
        {
            //show loading dialogue
            //TODO

            //because there's a template in the xaml
            grid_entry_title_blocks.Children.Clear();

            //loads the save files
            await LoadProjectDataMethod(path, project_name);

            UpdateAllComponents();

            //closes the loading dialogue
            //TODO
        }
        public async Task LoadProjectDataMethod(string path_param, string project_name_param)
        {
            Debug.WriteLine("\nLoadProjectDataMethod");

            //this opens the selected project data files

            //basic loading information
            bool test_mode = true;
            //string project_name = "Test Semesters 1";
            string project_name = project_name_param;


            //locations of the data files for test and normal modes
            //string test_data_loc = @"ms-appx:///Assets/TestData/";
            string test_data_loc = path_param;
            string saved_data_loc = ""; //TODO


            //choosing if using the normal or test data location
            string data_loc;
            if (test_mode) { data_loc = test_data_loc; } else { data_loc = saved_data_loc; }


            //getting the filenames of the save file
            string fileName_str_projectdata = project_name + "_save-data.txt";


            //getting the filepaths of the three save files
            string filePath_str_projectdata = data_loc + fileName_str_projectdata;

            //Debug.WriteLine(filePath_str_projectdata);




            //opening up the save file




            //the projectdata file
            Uri filePath_URI_projectdata = new Uri(filePath_str_projectdata);
            StorageFile projectdata_file = await StorageFile.GetFileFromApplicationUriAsync(filePath_URI_projectdata);

            //putting the file line-by-line into this list
            List<string> list_projectdata = new List<string>();
            var read_file_projectdata = await FileIO.ReadLinesAsync(projectdata_file);
            foreach (var line in read_file_projectdata)
            {
                //each line is added to the list
                list_projectdata.Add(line);
            }
            Debug.WriteLine("File loaded into list: " + fileName_str_projectdata);

            //hi there buddy
            //I'm doing a blah blah blah little buddy




            //PrintList("list_projectdata", list_projectdata);



            //will just show some more Debug troubleshooting if wanted
            bool show_all_troubleshooting = false;



            glo_ProjectData = GetSavedDataFromList(list_projectdata, show_all_troubleshooting);
            glo_ProjectData.ProjectLoaded = true;

            glo_ProjectData.PrintProjectDataValues(false, false, false);






            return;





            ////will extract meaningful EntryData from the list
            //List<EntryData> EntryData_lst = GetEntryDataFromList(list_entrydata, show_all_troubleshooting);
            //Debug.WriteLine("EntryData_lst.count = " + EntryData_lst.Count());

            ////will extract meaningful ColumnData from the list
            //List<ColumnData> ColumnData_lst = GetColumnDataFromList(list_columndata, show_all_troubleshooting);
            //Debug.WriteLine("ColumnData_lst.count = " + ColumnData_lst.Count());


            ////will extract meaningful ProjectData from the list and put in the previous EntryData and ColumnData lists
            //glo_ProjectData = GetProjectDataFromList(list_projectdata, EntryData_lst, ColumnData_lst, show_all_troubleshooting);
            //Debug.WriteLine("ProjectData successfully loaded");


            ////this will show all data in the project
            ////glo_ProjectData.PrintProjectDataValues(true, true);

            //return;

        }
        private ProjectData GetSavedDataFromList(List<string> list_projectdata, bool show_troubleshooting)
        {
            Debug.WriteLine("\nGetSavedDataFromList");

            //this is the return ProjectData
            ProjectData return_ProjectData = new ProjectData();

            //these are the lists of ColumnData and EntryData that will be put into return_project_data later
            List<ColumnData> ColumnData_lst = new List<ColumnData>();
            List<EntryData> EntryData_lst = new List<EntryData>();



            //this is checking if the file is formatted correctly, and extracting the project name from the first line
            string first_line = list_projectdata[0];
            string project_name = "";

            if (first_line.StartsWith("project-name="))
            {
                string[] first_line_split = first_line.Split('=');
                project_name = first_line_split[1];
                if (show_troubleshooting) { Debug.WriteLine("project-name= " + project_name); }
            }
            else
            {
                Debug.WriteLine("Error: current reading file does not begin with 'project-name='. exiting with empty list");
                return new ProjectData(); //TODO
            }

            if (show_troubleshooting) { Debug.WriteLine("Now go through all subsequent lines to find project, column, and entry data"); }

            //now go through all subsequent lines to find entry data
            for (int cur_line_num = 1; cur_line_num < list_projectdata.Count; cur_line_num++)
            {
                //the current line
                string cur_line = list_projectdata[cur_line_num];

                if (show_troubleshooting) { Debug.WriteLine("current line: " + cur_line); }

                //any useful data has a line character count of over 7, so skip if not
                if (cur_line.Length < 7) { continue; }


                //for a project data line, it'll be "pro-data", for column "col-data", for entry "ent-data"
                string line_flag = cur_line.Substring(0, 8);

                //printing the matching flag
                List<string> possible_flags = new List<string> { "pro-data", "col-data", "ent-data" };
                if (possible_flags.Contains(line_flag))
                {
                    if (show_troubleshooting) { Debug.WriteLine("\nline_flag: " + line_flag); }
                }

                //switch to determine which type of data it is
                switch (line_flag)
                {
                    //for project data
                    case "pro-data":

                        //the ProjectData class has a method for inputting all the required information, so pass the line to it
                        return_ProjectData.GetProjectDataFromLine(project_name, cur_line);

                        //prints the values of the new EntryData
                        if (show_troubleshooting) { return_ProjectData.PrintProjectDataValues(false, false, false); }

                        break;

                    //for column data
                    case "col-data":

                        //creating a new ColumnData to put in information
                        ColumnData cur_column_data = new ColumnData();

                        //the ColumnData class has a method for inputting all the required information, so pass the line to it
                        cur_column_data.GetColumnDataFromLine(project_name, cur_line);

                        //prints the values of the new ColumnData
                        if (show_troubleshooting) { cur_column_data.PrintColumnDataValues(); }

                        //adding the ColumnData to the ColumnData list
                        ColumnData_lst.Add(cur_column_data);

                        break;

                    //for entry data
                    case "ent-data":

                        //creating a new EntryData to put in information
                        EntryData cur_entry_data = new EntryData();

                        //the EntryData class has a method for inputting all the required information, so pass the line to it
                        cur_entry_data.GetEntryDataFromLine(project_name, cur_line);

                        //prints the values of the new EntryData
                        if (show_troubleshooting) { cur_entry_data.PrintEntryDataValues(); }

                        //adding the EntryData to the EntryData list
                        EntryData_lst.Add(cur_entry_data);

                        break;

                    default:
                        break;
                }


            }

            //adding the EntryData and ColumnData into the overall ProjectData

            return_ProjectData.ColumnData_lst_param = ColumnData_lst;
            return_ProjectData.EntryData_lst_param = EntryData_lst;

            return_ProjectData.CreateEntryIDsList();
            return_ProjectData.CreateColIDsList();

            List<string> entryDatas_validity_errors = return_ProjectData.CheckEntryDataValuesValid(false); //the bool is for detailed debugging
            if (entryDatas_validity_errors.Count == 1 && entryDatas_validity_errors[0] == "no errors")
            {
                //all good
            }
            else
            {
                //not good
                //TODO how to handle these erros on entrydata properties invalid
            }


            //returns the list of EntryData's
            return return_ProjectData;
        }





        //these methods deal with saving of project data

        public void SaveCurrentProject()
        {
            //this is the overall method that will be called whenever the data needs to be saved
            //it will async call on the method that does the actual saving to disk so that it'll run in the background

            Debug.WriteLine("SaveCurrentProject");



            SaveCurrentProjectToDisk(true);


        }
        public void SaveCurrentProjectToDisk(bool updating)
        {
            Debug.WriteLine("Local Storage Folder Path: " + glo_CalculatedData.localFolder.Path);

            //TODO make a checkstop to see if I'm overwriting a file

            //just one last place to end the process if it is a duplicate name if we are not just updating
            //if (GlobalVars.saved_semester_names.IndexOf(semester_name) != -1 & !updating)
            //    return;

            List<string> save_list = FormatCurrentSaveFile();

            //this apparently will save the entire list line-by-line
            //from here https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-write-to-a-text-file
            //System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\WriteLines.txt", lines);

            /*


            string semester_sep = "|";
            string details_sep = "$";

            //the current contents of the save file
            StorageFile SavedSemesterFile_old = await GlobalVars.localFolder.GetFileAsync("SemesterChoicesFile.txt");
            string saved_semester_full_str_old = await FileIO.ReadTextAsync(SavedSemesterFile_old);

            //creating a new string for what is going into the file
            string semester_data_new = "";

            //if the old file was not empty (had data already), we are going to append to the end of it
            if (saved_semester_full_str_old.Length != 0)
                semester_data_new = saved_semester_full_str_old + semester_sep;


            string new_semester_data_to_add = semester_name + details_sep + semester_icon_file + details_sep + semester_season + details_sep + semester_year;
            semester_data_new = semester_data_new + new_semester_data_to_add;




            StorageFile sampleFile = await GlobalVars.localFolder.CreateFileAsync("SemesterChoicesFile.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(sampleFile, semester_data_new);
            Debug.WriteLine("stored to file");

            */
        }
        public List<string> FormatCurrentSaveFile()
        {
            Debug.WriteLine("FormatCurrentSaveFile");

            List<string> save_list = new List<string>();

            string blank_line = "";
            string sep_line = "----------------------------------------";
            string data_separator = ";";


            string title_line = string.Format("project-name={0}", glo_ProjectData.ProjectName);
            save_list.Add(title_line);
            save_list.Add(blank_line);


            save_list.Add(sep_line);
            save_list.Add(blank_line);


            string project_data_header = "project-data_";
            save_list.Add(project_data_header);
            save_list.Add(blank_line);

            string project_data_line = glo_ProjectData.CreateProjectDataSaveLine(data_separator);
            save_list.Add(project_data_line);
            save_list.Add(blank_line);


            save_list.Add(sep_line);
            save_list.Add(blank_line);


            string column_data_header = "column-data_";
            save_list.Add(column_data_header);
            save_list.Add(blank_line);

            foreach(ColumnData cur_columnData in glo_ProjectData.ColumnData_lst_param)
            {
                string cur_column_data_line = cur_columnData.CreateColumnDataSaveLine(data_separator);
                save_list.Add(cur_column_data_line);
            }
            save_list.Add(blank_line);


            save_list.Add(sep_line);
            save_list.Add(blank_line);


            string entry_data_header = "entry-data_";
            save_list.Add(entry_data_header);
            save_list.Add(blank_line);

            foreach (EntryData cur_entryData in glo_ProjectData.EntryData_lst_param)
            {
                string cur_entry_data_line = cur_entryData.CreateEntryDataSaveLine(data_separator);
                save_list.Add(cur_entry_data_line);
            }

            return save_list;
        }





        //these are the overall layout methods

        private void UpdateAllComponents()
        {
            UpdateColumnHeaders();
            UpdateEntryTitleBlocks();
            //UpdateCalendarLayout();
            CreateUsedColourList();
        }
        private void UpdateColumnHeaders()
        {

            //gets rid of all previous headers
            grid_column_headers.Children.Clear();

            Debug.WriteLine("Column headers columndefinitions: " + grid_column_headers.ColumnDefinitions.Count());

            CreateCorrectNumberOfGridRowsColumns("column_headers");

            Debug.WriteLine("Column headers columndefinitions: " + grid_column_headers.ColumnDefinitions.Count());


            //this will sit at (0,0) in the grid and expand to fill the first cell
            Border dummy_border_for_width = new Border();
            dummy_border_for_width.Tag = "dummy|width";
            dummy_border_for_width.SizeChanged += DummyChangedSize;
            grid_column_headers.Children.Add(dummy_border_for_width);


            AddMissingBorders("columns");
            RemoveDeletedColumnHeaders();
        }
        private void UpdateEntryTitleBlocks()
        {

            //gets rid of all previous title blocks
            //grid_entry_title_blocks.Children.Clear();

            Debug.WriteLine("Rowdefinitions: " + grid_entry_title_blocks.RowDefinitions.Count());

            CreateCorrectNumberOfGridRowsColumns("titleblock_rows");

            Debug.WriteLine("Rowdefinitions: " + grid_entry_title_blocks.RowDefinitions.Count());


            //this will sit at (0,0) in the grid and expand to fill the first cell
            Border dummy_border_for_height = new Border();
            dummy_border_for_height.Tag = "dummy|height";
            dummy_border_for_height.SizeChanged += DummyChangedSize;
            //dummy_border_for_height.Background = new SolidColorBrush(Colors.Pink);
            grid_entry_title_blocks.Children.Add(dummy_border_for_height);

            AddMissingBorders("title");
            RemoveDeletedBlocks("title");


        }
        private void UpdateCalendarLayout()
        {

            Debug.WriteLine("Rowdefinitions: " + grid_calendar.RowDefinitions.Count());
            Debug.WriteLine("Columndefinitions: " + grid_calendar.ColumnDefinitions.Count());

            CreateCorrectNumberOfGridRowsColumns("calendar_both");

            Debug.WriteLine("Rowdefinitions: " + grid_calendar.RowDefinitions.Count());
            Debug.WriteLine("Columndefinitions: " + grid_calendar.ColumnDefinitions.Count());

            UpdateCalendarColRowSize();
        }
        private void UpdateCalendarDisplay()
        {
            //this method will update the calendar display according to the ActualColID values in each EntryData

            Debug.WriteLine("\nUpdateCalendarDisplay");


            AddMissingBorders("calendar");
            RemoveDeletedBlocks("calendar");

        }

        private void CreateCorrectNumberOfGridRowsColumns(string grid_selection)
        {
            Grid working_grid = new Grid();

            bool setting_rows = false;
            bool setting_cols = false;


            //based on the method's parameter, will either be adding columns or rows or both, and to a specific grid
            switch (grid_selection)
            {
                case "column_headers":
                    working_grid = grid_column_headers;
                    setting_cols = true;
                    break;

                case "titleblock_rows":
                    working_grid = grid_entry_title_blocks;
                    setting_rows = true;
                    break;

                case "calendar_both":
                    working_grid = grid_calendar;
                    setting_cols = true;
                    setting_rows = true;
                    break;
            }



            if (setting_rows)
            {
                //the amount of rows we want
                int desired_num_of_rows = glo_ProjectData.EntryData_lst_param.Count();

                //will delete rows until it's the proper size
                while (working_grid.RowDefinitions.Count > desired_num_of_rows)
                {
                    working_grid.RowDefinitions.RemoveAt(desired_num_of_rows);
                }

                //will create rows until it's the proper size
                while (working_grid.RowDefinitions.Count < desired_num_of_rows)
                {
                    RowDefinition new_row_def = new RowDefinition();
                    new_row_def.Height = new GridLength(1, GridUnitType.Auto);

                    working_grid.RowDefinitions.Add(new_row_def);
                }
            }

            if (setting_cols)
            {
                //the amount of columns we want
                int desired_num_of_cols = glo_ProjectData.ColumnData_lst_param.Count();


                //will delete columns until it's the proper size
                while (working_grid.ColumnDefinitions.Count > desired_num_of_cols)
                {
                    working_grid.ColumnDefinitions.RemoveAt(desired_num_of_cols);
                }

                //will create columns until it's the proper size
                while (working_grid.ColumnDefinitions.Count < desired_num_of_cols)
                {
                    ColumnDefinition new_col_def = new ColumnDefinition();
                    new_col_def.Width = glo_ProjectData.HeaderWidth;

                    working_grid.ColumnDefinitions.Add(new_col_def);
                }
            }
        }
        private void DummyChangedSize(object sender, SizeChangedEventArgs e)
        {
            //there is a dummy border in the first row of grid_entry_title_blocks and the first column of grid_column_headers
            //these dummy borders expand to fill their entire cell
            //they also have call this method everytime their size changes so we can update the size of the rows/columns in grid_calendar

            //this is the border
            Border sending_dummy = (Border)sender;

            //they each have different tags to differentiate them
            string tag = sending_dummy.Tag.ToString();

            Debug.WriteLine(string.Format("Dummy changed size: {0}", tag));


            //depending on the tag
            switch (tag)
            {
                case "dummy|width":

                    //get the actual width of the dummy, and save it to the glo_CalculatedData class object
                    glo_CalculatedData.ColumnWidth = sending_dummy.ActualWidth;
                    Debug.WriteLine("glo_CalculatedData.ColumnWidth = " + glo_CalculatedData.ColumnWidth);

                    break;

                case "dummy|height":

                    //get the actual height of the dummy, and save it to the glo_CalculatedData class object
                    glo_CalculatedData.RowHeight = sending_dummy.ActualHeight;
                    Debug.WriteLine("glo_CalculatedData.RowHeight = " + glo_CalculatedData.RowHeight);

                    break;
            }
        }
        private void UpdateCalendarColRowSize()
        {
            //will go through the calendar columns and rows and adjust them to be the correct size, as determined earlier by the dummies

            //will now update the column widths in the calendar grid
            foreach (ColumnDefinition cur_columndefinition in grid_calendar.ColumnDefinitions)
            {
                cur_columndefinition.Width = new GridLength(glo_CalculatedData.ColumnWidth, GridUnitType.Pixel);
            }

            //will now update the row heights in the calendar grid
            foreach (RowDefinition cur_rowdefinition in grid_calendar.RowDefinitions)
            {
                cur_rowdefinition.Height = new GridLength(glo_CalculatedData.RowHeight, GridUnitType.Pixel);
            }
        }
        private void SynchronizedScrollerOnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //found at https://stackoverflow.com/questions/15151974/synchronized-scrolling-of-two-scrollviewers-whenever-any-one-is-scrolled-in-wpf

            if (_lastScrollingElement != sender && Environment.TickCount - _lastScrollChange < ScrollLoopbackTimeout) return;

            _lastScrollingElement = sender;
            _lastScrollChange = Environment.TickCount;

            ScrollViewer sourceScrollViewer;
            ScrollViewer targetScrollViewer_hori;
            ScrollViewer targetScrollViewer_vert;


            //scroll_entry_title_blocks -- scrolls vertically
            //scroll_column_headers     -- scrolls horizontally
            //scroll_calendar           -- scrolls vert + hori


            if (sender == scroll_entry_title_blocks)
            {
                sourceScrollViewer = scroll_entry_title_blocks;
                targetScrollViewer_vert = scroll_calendar;


                targetScrollViewer_vert.ChangeView(null, sourceScrollViewer.VerticalOffset, null);
            }
            else if (sender == scroll_column_headers)
            {
                sourceScrollViewer = scroll_column_headers;
                targetScrollViewer_hori = scroll_calendar;


                targetScrollViewer_hori.ChangeView(sourceScrollViewer.HorizontalOffset, null, null);
            }
            else if (sender == scroll_calendar)
            {
                sourceScrollViewer = scroll_calendar;
                targetScrollViewer_vert = scroll_entry_title_blocks;
                targetScrollViewer_hori = scroll_column_headers;


                targetScrollViewer_vert.ChangeView(null, sourceScrollViewer.VerticalOffset, null);
                targetScrollViewer_hori.ChangeView(sourceScrollViewer.HorizontalOffset, null, null);
            }

        }





        //these methods assist in creating titleblocks, calendarblocks, and columnheaders

        public void AddMissingBorders(string title_calendar_columns)
        {
            //this method will go through all stored EntryData and ColumnData, and if one doesn't have a block, it will add it
            //accepts as input a string containing the words  title  calendar  columns
            //at least one of these words must be in the string, and multiples work  (eg.  "title_calendar" is valid)

            Debug.WriteLine("\nAddMisingBorders: " + title_calendar_columns);


            List<string> create_types_to_do = new List<string>();

            if (title_calendar_columns.Contains("title")) { create_types_to_do.Add("title"); }
            if (title_calendar_columns.Contains("calendar")) { create_types_to_do.Add("calendar"); }
            if (title_calendar_columns.Contains("columns")) { create_types_to_do.Add("columns"); }

            if (create_types_to_do.Count == 0) { return; }



            foreach (string cur_type in create_types_to_do)
            {
                List<string> IDs_in_datalists;
                string ID_type;
                string printname;

                switch (cur_type)
                {
                    case "title":
                        IDs_in_datalists = glo_ProjectData.entryIDs_lst_param;
                        ID_type = "entryID";
                        printname = "TitleBlocks";

                        break;

                    case "calendar":
                        IDs_in_datalists = glo_ProjectData.entryIDs_lst_param;
                        ID_type = "entryID";
                        printname = "CalendarBlocks";

                        break;

                    case "columns":
                        IDs_in_datalists = glo_ProjectData.colIDs_lst_param;
                        ID_type = "colID";
                        printname = "ColumnHeaders";

                        break;

                    default:
                        continue;
                }


                List<string> tags_in_borders = CreateBlockTagList_IndexOrder(cur_type);
                List<string> IDs_in_borders = ExtractEntryColIDsFromTagList(tags_in_borders, true, ID_type);
                List<string> IDs_in_datalists_not_in_borders = IDs_in_datalists.Except(IDs_in_borders).ToList();

                Debug.WriteLine(printname + " to add: " + IDs_in_datalists_not_in_borders.Count);

                foreach (string cur_adding_ID in IDs_in_datalists_not_in_borders)
                {
                    Debug.WriteLine("\nAdding: " + cur_adding_ID);

                    EntryData cur_entryData;
                    ColumnData cur_columnData;

                    switch (cur_type)
                    {
                        case "title":
                            cur_entryData = glo_ProjectData.GetEntryDataFromEntryID(cur_adding_ID);
                            CreateAndPlaceNewTitleBlock(cur_entryData);
                            break;

                        case "calendar":
                            cur_entryData = glo_ProjectData.GetEntryDataFromEntryID(cur_adding_ID);
                            CreateAndPlaceNewCalendarBlock(cur_entryData);
                            break;

                        case "columns":
                            cur_columnData = glo_ProjectData.GetColumnDataFromColID(cur_adding_ID);
                            CreateAndPlaceNewColumnHeader(cur_columnData);
                            break;
                    }
                }




            }






        }





        //these methods create the column headers

        public void RemoveDeletedColumnHeaders()
        {

        }
        private void CreateAndPlaceNewColumnHeader(ColumnData creating_columnData)
        {
            Border new_header = CreateColumnHeader(creating_columnData);
            grid_column_headers.Children.Add(new_header);

            creating_columnData.ColumnHeader = new_header;
        }
        private Border CreateColumnHeader(ColumnData cur_columndata)
        {
            Border outer_border = new Border();

            TextBlock txtblc_title = new TextBlock();




            // Set the DataContext of the border
            outer_border.DataContext = cur_columndata;




            // Create the colID tag binding
            Binding binding_ID_tag = new Binding()
            {
                Path = new PropertyPath("ColID"),
                Mode = BindingMode.OneWay,
                Converter = new Converter_ColIDToBorderTag()
            };
            outer_border.SetBinding(TagProperty, binding_ID_tag);


            // Create the title binding
            Binding binding_title = new Binding()
            {
                Path = new PropertyPath("ColTitle"),
                Mode = BindingMode.OneWay
            };
            txtblc_title.SetBinding(TextBlock.TextProperty, binding_title);


            // Create the column binding
            Binding binding_column = new Binding()
            {
                Path = new PropertyPath("ColPosition"),
                Mode = BindingMode.OneWay
            };
            outer_border.SetBinding(Grid.ColumnProperty, binding_column);








            outer_border.Child = txtblc_title;

            outer_border.Height = 32;

            txtblc_title.HorizontalAlignment = HorizontalAlignment.Center;
            txtblc_title.VerticalAlignment = VerticalAlignment.Center;


            return outer_border;



        }





        //these methods create the entry titleblocks

        public void RemoveDeletedBlocks(string title_calendar_both)
        {
            //this method will go through all stored titleblocks and calendarblocks, 
            //  and if one doesn't have a corresponding EntryData, it will remove it

            Debug.WriteLine("\nRemoveDeletedBlocks: " + title_calendar_both);

            List<string> entryIDs_in_datalists = glo_ProjectData.entryIDs_lst_param;

            //this removes the titleblocks
            if (title_calendar_both == "title" || title_calendar_both == "both")
            {
                //this just gets the tags from all the titleblocks
                List<string> tags_in_titleblocks = CreateBlockTagList_IndexOrder("title");

                //this method will strip out all non-entryID values
                List<string> entryIDs_in_titleblocks = ExtractEntryColIDsFromTagList(tags_in_titleblocks, true, "entryID");

                //this will determine which entryIDs exist in the block list but not in the datalist
                List<string> entryIDs_in_titleblocks_not_in_datalists =
                    entryIDs_in_titleblocks.Except(entryIDs_in_datalists).ToList();

                //PrintList("tags_in_titleblocks", tags_in_titleblocks);
                //PrintList("entryIDs_in_titleblocks", entryIDs_in_titleblocks);
                //PrintList("entryIDs_in_datalists", entryIDs_in_datalists);
                //PrintList("entryIDs_in_titleblocks_not_in_datalists", entryIDs_in_titleblocks_not_in_datalists);

                Debug.WriteLine("Titleblocks to delete: " + entryIDs_in_titleblocks_not_in_datalists.Count);

                //removing from the grid.children is hard
                //solution here  https://codeaddiction.net/articles/7/remove-items-while-iterating-a-collection-in-c

                //now to remove the blocks from the titleblocks pane
                foreach (Border cur_border in grid_entry_title_blocks.Children.ToList())
                {
                    //the tag info from the border
                    List<string> tag_info = ExtractTagInfo(cur_border.Tag.ToString());

                    //if the tag starts with  entryID|  and then checks if the  entryID is in the removing list
                    if (tag_info[0] == glo_tag_entryID_prefix.Trim('|') && entryIDs_in_titleblocks_not_in_datalists.Contains(tag_info[1]))
                    {
                        grid_entry_title_blocks.Children.Remove(cur_border);
                    }
                }
            }

            //this removes the calendarblocks (works the exact same as above)
            if (title_calendar_both == "calendar" || title_calendar_both == "both")
            {
                List<string> tags_in_calendarblocks = CreateBlockTagList_IndexOrder("calendar");
                List<string> entryIDs_in_calendarblocks = ExtractEntryColIDsFromTagList(tags_in_calendarblocks, true, "entryID");
                List<string> entryIDs_in_calendarblocks_not_in_datalists =
                    entryIDs_in_calendarblocks.Except(entryIDs_in_datalists).ToList();

                Debug.WriteLine("Calendarblocks to delete: " + entryIDs_in_calendarblocks_not_in_datalists.Count);

                foreach (Border cur_border in grid_calendar.Children.ToList())
                {
                    List<string> tag_info = ExtractTagInfo(cur_border.Tag.ToString());

                    if (tag_info[0] == glo_tag_entryID_prefix.Trim('|') && entryIDs_in_calendarblocks_not_in_datalists.Contains(tag_info[1]))
                    {
                        grid_calendar.Children.Remove(cur_border);
                    }
                }
            }
        }
        private void CreateAndPlaceNewTitleBlock(EntryData creating_entryData)
        {
            Border new_titleblock = CreateEntryTitleBlock(creating_entryData, false) as Border;
            grid_entry_title_blocks.Children.Add(new_titleblock);

            creating_entryData.TitleBlock = new_titleblock;
        }
        private Border CreateEntryTitleBlock(EntryData cur_entryData, bool for_preview)
        {

            Border outer_border = new Border();

            StackPanel name_stack = new StackPanel();
            name_stack.Style = Application.Current.Resources["stack_EntryTitleBlockTitleStack_Hori"] as Style;

            TextBlock txtblc_title = new TextBlock();
            txtblc_title.Style = Application.Current.Resources["txtblc_EntryTitleBlockTitle"] as Style;

            TextBlock txtblc_separator = new TextBlock();
            txtblc_separator.Style = Application.Current.Resources["txtblc_EntryTitleBlockSeparator_Hori"] as Style;

            TextBlock txtblc_subtitle = new TextBlock();
            txtblc_subtitle.Style = Application.Current.Resources["txtblc_EntryTitleBlockSubtitle_Hori"] as Style;




            // Set the DataContext of the border
            outer_border.DataContext = cur_entryData;



            if (!for_preview)
            {
                // Create the entryID tag binding
                Binding binding_ID_tag = new Binding()
                {
                    Path = new PropertyPath("EntryID"),
                    Mode = BindingMode.OneWay,
                    Converter = new Converter_EntryIDToBorderTag()
                };
                outer_border.SetBinding(TagProperty, binding_ID_tag);


                // Create the row binding
                Binding binding_row = new Binding()
                {
                    Path = new PropertyPath("RowPosition"),
                    Mode = BindingMode.OneWay
                };
                outer_border.SetBinding(Grid.RowProperty, binding_row);
            }
            else
            {
                outer_border.Tag = "preview";
            }



            // Create the style binding
            string style_property_;
            if (!for_preview) { style_property_ = "StyleName_Title"; }
            else { style_property_ = "StyleName_Preview"; }

            Binding binding_style = new Binding()
            {
                Path = new PropertyPath(style_property_),
                Mode = BindingMode.OneWay,
                Converter = new Converter_StyleNameToStyle()
            };
            outer_border.SetBinding(StyleProperty, binding_style);


            // Create the colour binding
            Binding binding_colour = new Binding()
            {
                Path = new PropertyPath("ColourHex"),
                Mode = BindingMode.OneWay,
                Converter = new Converter_ColourHexToSolidColorBrush()
            };
            outer_border.SetBinding(BackgroundProperty, binding_colour);


            // Create the title binding
            Binding binding_title = new Binding() 
            {
                Path = new PropertyPath("Title"), 
                Mode = BindingMode.OneWay 
            };
            txtblc_title.SetBinding(TextBlock.TextProperty, binding_title);


            // Create the subtitle binding
            Binding binding_subtitle = new Binding()
            {
                Path = new PropertyPath("Subtitle"),
                Mode = BindingMode.OneWay
            };
            txtblc_subtitle.SetBinding(TextBlock.TextProperty, binding_subtitle);






            if (!for_preview)
            {
                outer_border.RightTapped += TitleCalendarBlockRightTapped;
                outer_border.DoubleTapped += TitleCalendarBlockDoubleTapped;

                outer_border.ContextFlyout = CreateTitleCalendarBlockFlyout();
            }
            outer_border.Tapped += TitleCalendarBlockTapped;


            bool is_title = (cur_entryData.Title.Length > 0);
            bool is_subtitle = (cur_entryData.Subtitle.Length > 0);

            if (!is_title | !is_subtitle)
            {
                txtblc_separator.Text = "";
            }

            name_stack.Children.Add(txtblc_title);
            name_stack.Children.Add(txtblc_separator);
            name_stack.Children.Add(txtblc_subtitle);

            outer_border.Child = name_stack;

            return outer_border;

        }
        private MenuFlyout CreateTitleCalendarBlockFlyout()
        {
            MenuFlyout context_menu_border = new MenuFlyout();

            MenuFlyoutItem menu_item_moveup = new MenuFlyoutItem();
            menu_item_moveup.Text = "Move up";
            FontIcon moveup_icon = new FontIcon();
            moveup_icon.FontFamily = new FontFamily("Segoe MDL2 Assets");
            moveup_icon.Glyph = "\xE110";
            menu_item_moveup.Icon = moveup_icon;
            context_menu_border.Items.Add(menu_item_moveup);

            MenuFlyoutItem menu_item_movedown = new MenuFlyoutItem();
            menu_item_movedown.Text = "Move down";
            FontIcon movedown_icon = new FontIcon();
            movedown_icon.FontFamily = new FontFamily("Segoe MDL2 Assets");
            movedown_icon.Glyph = "\xE74B";
            menu_item_movedown.Icon = movedown_icon;
            context_menu_border.Items.Add(menu_item_movedown);

            MenuFlyoutSeparator menu_sep = new MenuFlyoutSeparator();
            context_menu_border.Items.Add(menu_sep);

            MenuFlyoutItem menu_item_delete = new MenuFlyoutItem();
            menu_item_delete.Text = "Delete";
            menu_item_delete.Icon = new SymbolIcon(Symbol.Delete);
            menu_item_delete.Tapped += PropertiesButtonTapped;
            context_menu_border.Items.Add(menu_item_delete);

            MenuFlyoutItem menu_item_properties = new MenuFlyoutItem();
            //menu_item_properties.Click += PropertiesButtonTapped;
            menu_item_properties.Text = "Properties";
            FontIcon prop_icon = new FontIcon();
            prop_icon.FontFamily = new FontFamily("Segoe MDL2 Assets");
            prop_icon.Glyph = "\xE946";
            menu_item_properties.Icon = prop_icon;
            menu_item_properties.Tapped += PropertiesButtonTapped;
            context_menu_border.Items.Add(menu_item_properties);

            return context_menu_border;
        }





        //these methods create the entry calendarblocks

        private void CreateAndPlaceNewCalendarBlock(EntryData creating_entryData)
        {
            Debug.WriteLine("CreateAndPlaceNewCalendarBlock");

            string cur_actual_colID = creating_entryData.ActualColID;
            Debug.WriteLine(string.Format("  EntryID = {0,-10}   ActualColID = {1,-10}", creating_entryData.EntryID, cur_actual_colID));


            //just to ensure that there was an actual colID
            if (cur_actual_colID == "")
            {
                Debug.WriteLine("  ActualColID was empty; skipping\n");
                return;
            }


            //the corresponding ColumnData
            ColumnData cur_columnData = glo_ProjectData.GetColumnDataFromColID(cur_actual_colID);


            //setting the column index (this is bound to the Grid.Column property of the borders and will auto-update)
            creating_entryData.ColPosition = cur_columnData.ColPosition;


            Border new_calendar_block = CreateCalendarBlock(creating_entryData) as Border;
            grid_calendar.Children.Add(new_calendar_block);
            creating_entryData.CalendarBlock = new_calendar_block;
        }
        private Border CreateCalendarBlock(EntryData cur_entryData)
        {
            Border outer_border = new Border();

            TextBlock txtblc_title = new TextBlock();
            txtblc_title.Style = Application.Current.Resources["txtblc_CalendarBlockTitle"] as Style;




            // Set the DataContext of the border
            outer_border.DataContext = cur_entryData;




            // Create the entryID tag binding
            Binding binding_ID_tag = new Binding()
            {
                Path = new PropertyPath("EntryID"),
                Mode = BindingMode.OneWay,
                Converter = new Converter_EntryIDToBorderTag()
            };
            outer_border.SetBinding(TagProperty, binding_ID_tag);


            // Create the style binding
            Binding binding_style = new Binding()
            {
                Path = new PropertyPath("StyleName_Calendar"),
                Mode = BindingMode.OneWay,
                Converter = new Converter_StyleNameToStyle()
            };
            outer_border.SetBinding(StyleProperty, binding_style);


            // Create the colour binding
            Binding binding_colour = new Binding()
            {
                Path = new PropertyPath("ColourHex"),
                Mode = BindingMode.OneWay,
                Converter = new Converter_ColourHexToSolidColorBrush()
            };
            outer_border.SetBinding(BackgroundProperty, binding_colour);


            // Create the title binding
            Binding binding_title = new Binding()
            {
                Path = new PropertyPath("Title"),
                Mode = BindingMode.OneWay
            };
            txtblc_title.SetBinding(TextBlock.TextProperty, binding_title);


            // Create the row binding
            Binding binding_row = new Binding()
            {
                Path = new PropertyPath("RowPosition"),
                Mode = BindingMode.OneWay
            };
            outer_border.SetBinding(Grid.RowProperty, binding_row);

            // Create the column binding
            Binding binding_column = new Binding()
            {
                Path = new PropertyPath("ColPosition"),
                Mode = BindingMode.OneWay
            };
            outer_border.SetBinding(Grid.ColumnProperty, binding_column);





            outer_border.RightTapped += TitleCalendarBlockRightTapped;
            outer_border.Tapped += TitleCalendarBlockTapped;
            outer_border.DoubleTapped += TitleCalendarBlockDoubleTapped;

            outer_border.ContextFlyout = CreateTitleCalendarBlockFlyout();


            outer_border.Child = txtblc_title;


            return outer_border;
        }





        //these are utilities for the titleblocks and others

        public List<string> CreateBlockTagList_IndexOrder(string title_calendar)
        {
            //this is a little function that will go through either the titleblocks or calendar blocks
            //  and return a list of its tags (ideally entryID) in the correct index

            List<string> return_list = new List<string>();

            Grid working_grid;

            switch (title_calendar)
            {
                case "title":
                    working_grid = grid_entry_title_blocks;
                    break;

                case "calendar":
                    working_grid = grid_calendar;
                    break;

                case "columns":
                    working_grid = grid_column_headers;
                    break;

                default:
                    return return_list;
            }



            foreach (Border cur_border in working_grid.Children)
            {
                return_list.Add(cur_border.Tag.ToString());
            }


            return return_list;
        }
        public List<string> ExtractEntryColIDsFromTagList(List<string> tag_list, bool strip_non_entryIDs, string entryID_or_colID)
        {
            //this method will go through the list of tags and only include the entryID values
            //if strip_non_entryIDs it will delete that indice completely, otherwise that indice will be ""
            //  in order to keep the correct index order


            string cur_global_tag;
            if (entryID_or_colID == "entryID")
            {
                cur_global_tag = glo_tag_entryID_prefix.Trim('|');
            }
            else if (entryID_or_colID == "colID")
            {
                cur_global_tag = glo_tag_colID_prefix.Trim('|');
            }
            else
            {
                return new List<string>();
            }



            List<string> return_list = new List<string>();


            foreach (string cur_tag in tag_list)
            {
                List<string> tag_split = ExtractTagInfo(cur_tag);

                bool is_entryID = (tag_split[0] == cur_global_tag);

                if (is_entryID)
                {
                    return_list.Add(tag_split[1]);
                }
                else if (strip_non_entryIDs)
                {
                    //do not put it in
                }
                else
                {
                    return_list.Add("");
                }
            }

            return return_list;
        }
        public void FixBlockInfo(bool reset_names, bool reset_titles, bool reset_subtitles, bool reset_list_pos, bool reset_colours)
        {
            //this function will go through the title and calendar blocks and fix whatever data is specified

            Debug.WriteLine("FixBlockInfo");

            return;

            List<string> properties_to_update = new List<string>();

            if (reset_names) { properties_to_update.Add("Names"); }
            if (reset_titles) { properties_to_update.Add("Titles"); }
            if (reset_subtitles) { properties_to_update.Add("Subtitles"); }
            if (reset_list_pos) { properties_to_update.Add("List Pos"); }
            if (reset_colours) { properties_to_update.Add("Colours"); }

            foreach (string cur_prop_to_update in properties_to_update)
            {
                Debug.WriteLine("    Updating: " + cur_prop_to_update);
            }
            Debug.WriteLine("");


            //begin with the titleblocks
            foreach (Border cur_titleblock in grid_entry_title_blocks.Children)
            {

                //skips any null
                if (cur_titleblock == null) { continue; }


                //the tag information
                List<string> tag_extracted = ExtractTagInfo(cur_titleblock.Tag.ToString());

                //skipping the dummy and other, only wanting entryID
                if (tag_extracted[0] != "entryID") { continue; }

                //the entryID
                string cur_title_entryID = tag_extracted[1];

                //grabbing the EntryData using the built-in method
                EntryData cur_title_EntryData = glo_ProjectData.GetEntryDataFromEntryID(cur_title_entryID);


                //this is the structure of the titleblock
                /*
                
                <Border>  ------------------------  this has the entryID as its tag, and the Grid.Row property, the background colour, and the border name
                    <Grid>  ----------------------
                        <StackPanel>  ------------
                            <TextBlock/>  --------  this is the title
                            <TextBlock/>  --------  this is the separator
                            <TextBlock/>  --------  this is the subtitle
                        </StackPanel>  -----------
                    </Grid>  ---------------------
                </Border>  -----------------------

                */


                if (reset_names)
                {
                    cur_titleblock.Name = "bor_titleblock_dataID_" + cur_title_EntryData.EntryID;
                }
                if (reset_list_pos)
                {
                    Grid.SetRow(cur_titleblock, Convert.ToInt32(cur_title_EntryData.RowPosition));
                }
                if (reset_colours)
                {
                    cur_titleblock.Background = GetSolidColorBrushFromHex(cur_title_EntryData.ColourHex);
                }
                if (reset_titles || reset_subtitles)
                {
                    Grid cur_inner_grid = cur_titleblock.Child as Grid;
                    StackPanel cur_inner_stack = cur_inner_grid.Children[0] as StackPanel;

                    if (reset_titles)
                    {
                        TextBlock cur_txtblc_title = cur_inner_stack.Children[0] as TextBlock;
                        cur_txtblc_title.Text = cur_title_EntryData.Title;
                    }
                    if (reset_subtitles)
                    {
                        TextBlock cur_txtblc_subtitle = cur_inner_stack.Children[2] as TextBlock;
                        cur_txtblc_subtitle.Text = cur_title_EntryData.Subtitle;
                    }
                }
            }

            //now do the calendarblocks
            foreach (Border cur_calendarblock in grid_calendar.Children)
            {

                //skips any null
                if (cur_calendarblock == null) { continue; }


                //the tag information
                List<string> tag_extracted = ExtractTagInfo(cur_calendarblock.Tag.ToString());

                //skipping the dummy and other, only wanting entryID
                if (tag_extracted[0] != "entryID") { continue; }

                //the entryID
                string cur_calendar_entryID = tag_extracted[1];

                //grabbing the EntryData using the built-in method
                EntryData cur_calendar_EntryData = glo_ProjectData.GetEntryDataFromEntryID(cur_calendar_entryID);


                //this is the structure of the calendarblock
                /*
                
                <Border>  ------------------------  this has the entryID as its tag, and the Grid.Row property, the background colour, and the border name
                    <TextBlock/>  ----------------  this is the title
                </Border>  -----------------------
                
                */


                if (reset_names)
                {
                    cur_calendarblock.Name = "bor_titleblock_dataID_" + cur_calendar_EntryData.EntryID;
                }
                if (reset_list_pos)
                {
                    Grid.SetRow(cur_calendarblock, cur_calendar_EntryData.RowPosition);
                }
                if (reset_colours)
                {
                    cur_calendarblock.Background = GetSolidColorBrushFromHex(cur_calendar_EntryData.ColourHex);
                }
                if (reset_titles)
                {
                    TextBlock cur_txtblc_title = cur_calendarblock.Child as TextBlock;
                    cur_txtblc_title.Text = cur_calendar_EntryData.Title;
                }
            }

        }
        public void FixSingleTitleCalendarBlock(string fixing_entryID, string title_calendar)
        {
            //this method will find the titleblock and/or calendarblock of the given entryID and update its fields according to its EntryData
            //accepts as input a string containing the words  title  calendar
            //at least one of these words must be in the string, and multiples work  (eg.  "title_calendar" is valid)

            Debug.WriteLine("\nFixSingleTitleCalendarBlock:     fixing_entryID = '" + fixing_entryID + "'    type = " + title_calendar);


            if (fixing_entryID == null || fixing_entryID == "" || !glo_ProjectData.entryIDs_lst_param.Contains(fixing_entryID))
            {
                Debug.WriteLine("ERROR: invalid entryID. Exiting.");
                return;
            }


            List<string> create_types_to_do = new List<string>();

            if (title_calendar.Contains("title")) { create_types_to_do.Add("title"); }
            if (title_calendar.Contains("calendar")) { create_types_to_do.Add("calendar"); }

            if (create_types_to_do.Count == 0) { return; }


            //the entrydata being used to fix
            EntryData fixing_entryData = glo_ProjectData.GetEntryDataFromEntryID(fixing_entryID);


            //will go through each type requested, one by one
            foreach (string cur_type in create_types_to_do)
            {

                //the border and the textblocks
                Border fixing_border = new Border();
                TextBlock txtblc_title = new TextBlock();
                TextBlock txtblc_subtitle = new TextBlock();    //will not be assigned if calendarblock


                //the switch will choose the correct controls depending on  title_calendar
                switch (cur_type)
                {
                    case "title":
                        fixing_border = fixing_entryData.TitleBlock;


                        //this is the structure of the titleblock
                        /*

                        <Border>  ------------------------  this has the entryID as its tag, and the Grid.Row property, the background colour, and the border name
                            <Grid>  ----------------------
                                <StackPanel>  ------------
                                    <TextBlock/>  --------  this is the title
                                    <TextBlock/>  --------  this is the separator
                                    <TextBlock/>  --------  this is the subtitle
                                </StackPanel>  -----------
                            </Grid>  ---------------------
                        </Border>  -----------------------

                        */

                        Grid inner_grid = fixing_border.Child as Grid;
                        StackPanel inner_stack = inner_grid.Children[0] as StackPanel;
                        txtblc_title = inner_stack.Children[0] as TextBlock;
                        txtblc_subtitle = inner_stack.Children[2] as TextBlock;

                        break;

                    case "calendar":
                        fixing_border = fixing_entryData.CalendarBlock;


                        //this is the structure of the calendarblock
                        /*

                        <Border>  ------------  this has the entryID as its tag, and the Grid.Row and Grid.Column properties, and the background colour
                            <TextBlock/>  ----  this is the title
                        </Border>  -----------

                        */

                        txtblc_title = fixing_border.Child as TextBlock;

                        break;

                    default:
                        continue;
                }


                //the data that will be put into the border
                string data_title = fixing_entryData.Title;
                string data_subtitle = fixing_entryData.Subtitle;
                string data_colour_hex = fixing_entryData.ColourHex;

                int data_entry_row = fixing_entryData.RowPosition;
                int data_entry_col;



                //now to apply the data

                //the entryID
                fixing_border.Tag = glo_tag_entryID_prefix + fixing_entryID;

                //the colour
                if (data_colour_hex != null && data_colour_hex.Length == 9)
                    fixing_border.Background = GetSolidColorBrushFromHex(data_colour_hex);

                //the title
                if (data_title != null && data_title.Length > 0)
                    txtblc_title.Text = data_title;

                //the row
                if (data_entry_row != -1)
                    Grid.SetRow(fixing_border, data_entry_row);


                if (cur_type == "title")
                {
                    //the subtitle
                    if (cur_type == "title" && data_subtitle != null && data_subtitle.Length > 0)
                        txtblc_subtitle.Text = data_subtitle;
                }

                if (cur_type == "calendar")
                {
                    //the corresponding ColumnData
                    string fixing_colID = fixing_entryData.ActualColID;
                    ColumnData cur_columnData = glo_ProjectData.GetColumnDataFromColID(fixing_colID);

                    data_entry_col = cur_columnData.ColPosition;

                    //the column
                    if (data_entry_col != -1)
                        Grid.SetRow(fixing_border, fixing_entryData.RowPosition);
                }


            }

        }
        private bool CheckIfAllTitleBlocksValid()
        {
            Debug.WriteLine("CheckIfAllTitleblocksPresent");

            //this method will go through the titleblocks and determine if each has a valid entryID, and that all EntryData's are represented

            //a list of "false" that is a parallel to the EntryData list
            bool[] saw_entry = new bool[glo_ProjectData.EntryData_lst_param.Count()];

            //will go through every border and match the entryID to the EntryData
            foreach (Border cur_border1 in grid_entry_title_blocks.Children)
            {
                //Debug.WriteLine("cur_border1.Tag = '" + cur_border1.Tag + "'");


                //the tag information
                List<string> tag_extracted = ExtractTagInfo(cur_border1.Tag.ToString());

                //skipping the dummy and other, only wanting entryID
                if (tag_extracted[0] != "entryID") { continue; }

                //the entryID
                string cur_entryID1 = tag_extracted[1];



                //checking to see if the entryID is in the indexing list
                if (glo_ProjectData.entryIDs_lst_param.Contains(cur_entryID1))
                {
                    //if it is, then we find this index
                    int cur_index1 = glo_ProjectData.entryIDs_lst_param.IndexOf(cur_entryID1);

                    //and get the corresponding EntryData
                    EntryData cur_entryData1 = glo_ProjectData.EntryData_lst_param[cur_index1];

                    //now to check just to make sure that the border's entryID, matched with the index list, and matched with the corresponding EntryData
                    if (cur_entryID1 == cur_entryData1.EntryID)
                    {
                        saw_entry[cur_index1] = true;
                    }
                    else
                    {
                        //TODO
                        Debug.WriteLine("ERROR: There an inconsistency between  glo_ProjectData.EntryData_lst_param  and  glo_ProjectData.entryIDs_lst_param  with entryID '" + cur_entryID1 + "'");
                        return false;
                    }
                }

                //if the entryID is NOT in the indexing list...
                else
                {
                    //TODO
                    Grid inner_grid = cur_border1.Child as Grid;
                    StackPanel inner_stack = inner_grid.Children[0] as StackPanel;
                    TextBlock title_txtblc = inner_stack.Children[0] as TextBlock;

                    Debug.WriteLine(string.Format("ERROR: The titleblock '{0}' (entryID: {1}) does not exist in  glo_ProjectData.entryIDs_lst_param", title_txtblc.Text, cur_entryID1));
                    return false;
                }
            }


            //now if all went well, then saw_entry should be the same length as  glo_ProjectData.EntryData_lst_param  and be filled with true's
            //if it has any false's left then those are EntryData without any corresponding border

            if (saw_entry.Contains(false))
            {
                //TODO
                Debug.WriteLine("ERROR: There are EntryData's in  glo_ProjectData.EntryData_lst_param  that do not have a corresponding titleblock");
                return false;
            }

            //if here, then all titleblocks are good :)
            Debug.WriteLine("CheckIfAllTitleBlocksValid = true");

            return true;
        }
        private List<string> ExtractTagInfo(string given_tag)
        {
            List<string> split_tag = given_tag.Split('|').ToList();

            return split_tag;
        }





        //these methods calculate the logic in the calendar display

        private void UpdateCalendarLogicDisplay(bool detailed_debugging, bool clear_first)
        {
            //this method will perform order calculations and then update the calendar display

            Debug.WriteLine("\nUpdateCalendarLogicDisplay");

            //will first update the layout of the calendar
            UpdateCalendarLayout();

            //this is where the calculations are performed, returns a true/false if it succeeded
            //the parameter bool is for detailed_debugging
            bool successfully_converged = CalculateCalendarLogic(detailed_debugging);

            if (!successfully_converged)
            {
                Debug.WriteLine("\nERROR: Calendar logic did not converge");
                return;
            }

            //otherwise, we're good to update the display
            Debug.WriteLine("\nCalendar logic converged successfully");

            //if we want the calendar to clear its contents first
            if (clear_first)
            {
                grid_calendar.Children.Clear();
            }

            //now to update the calendar display
            UpdateCalendarDisplay();


        }
        private bool CalculateCalendarLogic(bool detailed_debugging)
        {
            //this method will go through all the EntryDatas and ColumnDatas to figure out how all pre/co reqs will work in available columns

            Debug.WriteLine("\nCalculateCalendarLogic");

            //this bool is to say if it was successful in finding a solution
            bool converged = false;

            //this will reset all the EntryData.ActualColID values
            glo_ProjectData.ClearActualColIDs();

            //this will get the ordered list of colID's
            List<string> colID_order_lst = glo_ProjectData.CreateColID_Order_lst();
            if (detailed_debugging) { PrintList("colID_order_lst", colID_order_lst); }


            //this is a hardcopy of the entryID list
            //it will have items removed as they get used
            List<string> to_place_entryID_lst = CreateDeepCopyOfList_string(glo_ProjectData.entryIDs_lst_param);
            int to_place_count = to_place_entryID_lst.Count();


            //this is a list of all placed entryIds
            List<string> placed_entryID_lst = new List<string>();

            if (detailed_debugging)
            {
                PrintList("to_place_entryID_lst - Original", to_place_entryID_lst);
                PrintList("placed_entryID_lst - Original", placed_entryID_lst);
            }

            int pass = 0;
            while (to_place_entryID_lst.Count > 0)
            {
                //increases the pass counter
                pass++;

                Debug.WriteLine("pass = " + pass);

                //this is to detect if we are just looping in circles and not finding a solution
                if (pass > to_place_count * 1.5)
                {
                    //TODO make this more detailed in the info it gives
                    Debug.WriteLine(string.Format("ERROR: No solution for placing EntryData. Tried {0} passes", pass));

                    return converged;
                }



                //every pass, we will go through the entirety of the to_place_entryID_lst (with any removals)
                //we will then determine the available columns that each EntryData can be put into
                //after placing, we will remove them from the to_place list

                List<string> to_remove_from_to_place_lst = new List<string>();

                foreach (string cur_entryID in to_place_entryID_lst)
                {
                    //this is the current EntryData
                    EntryData cur_entryData = glo_ProjectData.GetEntryDataFromEntryID(cur_entryID);


                    List<string> cur_avail_colID = cur_entryData.AvailColIDs;

                    //this is the list of the current EntryData's available colID's, in order according to colID_order_lst
                    List<string> cur_ordered_avail_colID = CalculateAvailColID_InOrder(cur_avail_colID, colID_order_lst);


                    //TODO add functionality for corequisites

                    //will place the entrydata if it has no pre-coreqs, or if those pre/coreqs have already been placed


                    //first takes  placed_entryID_lst  and trims out anything not in  cur_entryData.PrereqEntryIDs, then checks that its the same length as  cur_entryData.PrereqEntryIDs
                    //essentially, ensures that everying in  cur_entryData.PrereqEntryIDs  is in  placed_entryID_lst
                    //i.e. all prereqs placed already; if no prereqs at all then it also works
                    if (placed_entryID_lst.Intersect(cur_entryData.PrereqEntryIDs).ToList().Count() == cur_entryData.PrereqEntryIDs.Count())
                    {
                        //TODO add functionality for setting the colID

                        string with_without = "without";
                        if (cur_entryData.PrereqEntryIDs.Count > 0)
                        {
                            with_without = "with";
                        }

                        Debug.Write(string.Format("Finding ActualColID for  {0,-10} : {1} prereqs", cur_entryID, with_without));
                        foreach (string cur_prereq_entryID_printing in cur_entryData.PrereqEntryIDs)
                        {
                            Debug.Write("  " + cur_prereq_entryID_printing);
                        }
                        Debug.WriteLine("");

                        if (detailed_debugging) { PrintList("cur_entryData.PrereqEntryIDs", cur_entryData.PrereqEntryIDs); }


                        //this is the list of col positions of all the prereqs
                        List<int> prereq_positions = new List<int>();
                        foreach (string cur_prereq_entryID in cur_entryData.PrereqEntryIDs)
                        {
                            EntryData cur_prereq_entryData = glo_ProjectData.GetEntryDataFromEntryID(cur_prereq_entryID);
                            int cur_prereq_col_position = glo_ProjectData.GetColumnPositionFromColID(cur_prereq_entryData.ActualColID);

                            prereq_positions.Add(cur_prereq_col_position);
                        }
                        if (detailed_debugging) { PrintList_int("prereq_positions", prereq_positions); }

                        //this is the default earliest, will remain 0 if there were no prereqs
                        int earliest_col_pos_after_prereqs = 0;
                        if (prereq_positions.Count > 0)
                        {
                            //we will get in here if there were any prereqs
                            earliest_col_pos_after_prereqs = prereq_positions.Max() + 1;
                        }
                        if (detailed_debugging) { Debug.WriteLine("earliest_col_pos_after_prereqs = " + earliest_col_pos_after_prereqs); }

                        if (detailed_debugging) { PrintList("cur_ordered_avail_colID", cur_ordered_avail_colID); }

                        List<int> cur_ordered_avail_col_pos = CalculateAvailColPos_InOrder(cur_ordered_avail_colID);
                        if (detailed_debugging) { PrintList_int("cur_ordered_avail_col_pos", cur_ordered_avail_col_pos); }

                        //this will trim the lowest positions from the list until it's at least  earliest_col_pos_after_prereqs
                        while (cur_ordered_avail_col_pos.Count > 0 && cur_ordered_avail_col_pos[0] < earliest_col_pos_after_prereqs)
                        {
                            cur_ordered_avail_col_pos.RemoveAt(0);
                        }
                        if (detailed_debugging) { PrintList_int("cur_ordered_avail_col_pos", cur_ordered_avail_col_pos); }



                        //setting it to be the first colID
                        //cur_entryData.ActualColID = cur_ordered_avail_colID[0];
                        cur_entryData.ActualColID = glo_ProjectData.GetColumnIDFromPosition(cur_ordered_avail_col_pos[0]);
                        if (detailed_debugging) { Debug.WriteLine("cur_entryData.ActualColID = " + cur_entryData.ActualColID); }

                        //this will remove it from the to_place list
                        to_remove_from_to_place_lst.Add(cur_entryID);
                    }


                }


                //this will go through the entryID's that were successfully placed and remove them from the to_place list
                foreach (string cur_placed_entryID in to_remove_from_to_place_lst)
                {
                    to_place_entryID_lst.Remove(cur_placed_entryID);
                    placed_entryID_lst.Add(cur_placed_entryID);
                }


                if (detailed_debugging)
                {
                    PrintList("to_place_entryID_lst", to_place_entryID_lst);
                    PrintList("placed_entryID_lst", placed_entryID_lst);
                }

                //we will now do another pass through, placing the next round of entryDatas

            }


            //if we get here, then we got through the entire to_place list
            converged = true;

            return converged;




            //TODO on load make sure that all pre/co reqs actually have corresponding EntryData







        }
        private List<int> CalculateAvailColPos_InOrder(List<string> cur_ordered_avail_colID)
        {
            //this will receive an already ordered list of colID's, this changes it to col positions

            List<int> return_list = new List<int>();

            foreach (string cur_colID in cur_ordered_avail_colID)
            {
                ColumnData cur_columnData = glo_ProjectData.GetColumnDataFromColID(cur_colID);
                return_list.Add(cur_columnData.ColPosition);
            }

            return return_list;
        }
        private List<string> CalculateAvailColID_InOrder(List<string> avail_col_IDs, List<string> ordered_col_IDs)
        {
            //this method will take the available col ID list, and order it according to the ordered col ID list

            //just an error check
            if (avail_col_IDs.Count < 1 || ordered_col_IDs.Count < 1) { return new List<string>(); }

            //the return list
            //the intersection command takes the ordered_col_IDs list, and trims out any value that isn't in avail_col_IDs
            List<string> ordered_avail_col_IDs = ordered_col_IDs.Intersect(avail_col_IDs).ToList();


            return ordered_avail_col_IDs;
        }





        //these methods deal with tapping on titleblocks and calendarblocks

        private void TitleCalendarBlockTapped(object sender, TappedRoutedEventArgs e)
        {
            //this is the border if it was the tap_grid
            //Grid sending_grid = (Grid)sender;
            //Grid parent_grid = sending_grid.Parent as Grid;
            //Border sending_border = parent_grid.Parent as Border;






            //if it's just the border itself
            Border sending_border = (Border)sender;
            string sending_tag = sending_border.Tag.ToString();


            if (sending_tag == "preview")
            {
                AddNewEntryPreviewBlockTapped(sending_border);
                return;
            }



            //the tag information
            List<string> tag_extracted = ExtractTagInfo(sending_border.Tag.ToString());

            //skipping the dummy and other, only wanting entryID
            if (tag_extracted[0] != "entryID") { return; }

            //the entryID of the border that was tapped
            string tapped_entryID = tag_extracted[1];


            //this will determine if the selection needs to be changed, and to change it if it is, the false is for left click
            TitleCalendarBlockTappedMethod(tapped_entryID, false);
        }
        private void TitleCalendarBlockRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            //this is the border
            Border sending_border = (Border)sender;


            //the tag information
            List<string> tag_extracted = ExtractTagInfo(sending_border.Tag.ToString());

            //skipping the dummy and other, only wanting entryID
            if (tag_extracted[0] != "entryID") { return; }

            //the entryID of the border that was tapped
            string tapped_entryID = tag_extracted[1];


            //this will determine if the selection needs to be changed, and to change it if it is, the true is for right click
            TitleCalendarBlockTappedMethod(tapped_entryID, true);
        }
        private void TitleCalendarBlockDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Debug.WriteLine("TitleBlockDoubleTapped");
            //await OpenPropertiesWindow();

            glo_PropertyPaneOpen = true;

        }
        private void TitleBlockScrollViewerTapped(object sender, TappedRoutedEventArgs e)
        {
            //UnselectEntryData();
        }

        public async void TitleCalendarBlockTappedMethod(string tapped_entryID, bool right_tapped)
        {
            string cur_tapped_entryID = glo_selected_entryID;

            bool same_entryID = (tapped_entryID == cur_tapped_entryID);


            //if we tapped the already tapped titleblock, we exit
            if (same_entryID) { return; }


            //if the properties pane is open, this method will ask the user if he wishes to save his properties
            bool change_verified = await ChangeSelectionVerified();
            if (!change_verified) { return; }


            //otherwise, will update the selection
            SelectEntryData(tapped_entryID);


            //changes the enabled of the buttons
            EnableTitleBlockButtons(true, glo_selected_EntryData.RowPosition);


            //TODO this really needs some sort of verification to ensure that it's a valid selection
            //like maybe an entry got removed in glo_ProjectData but its titleblock remained for some reason?


        }
        public void SelectEntryData(string new_selected_entryID)
        {
            EntryData cur_selected = glo_selected_EntryData;
            EntryData new_selected = glo_ProjectData.GetEntryDataFromEntryID(new_selected_entryID);


            //if all goes correctly, these parameters should be bound to the style of their respective borders
            cur_selected.Is_Selected = false;
            new_selected.Is_Selected = true;


            //and we set the global flag
            glo_selected_entryID = new_selected.EntryID;

            //and set the global EntryData
            glo_selected_EntryData = new_selected;


            Debug.WriteLine("The new selection:");
            glo_selected_EntryData.PrintEntryDataValues();
        }
        public void UnselectEntryData()
        {
            //this method will unselect whatever is the currently selected titleblock

            //unselects the currently selected entrydata
            glo_selected_EntryData.Is_Selected = false;

            //reset the global flag
            glo_selected_entryID = "";

            //reset the global EntryData
            glo_selected_EntryData = new EntryData();

            //changes the enabled of the buttons
            EnableTitleBlockButtons(false, -1);

        }
        public async Task<bool> ChangeSelectionVerified()
        {
            //this method will check if the properties pane is open with changed values
            //if so, it will ask the user if he wishes to change selection, and either save or discard changes

            //TODO make this method ask the user if they wish to change selection if the properties open


            //if the properties pane isn't open we don't need verification
            if (!glo_PropertyPaneOpen) { return true; }


            //TODO if properties aren't changed



            ContentDialog contentdialog_selectionchange_properties = new ContentDialog
            {
                Title = "Save Changes and Change Selection?",
                Content = "Do you wish to save your unsaved property changes and change selection?",
                PrimaryButtonText = "Save and Change",
                SecondaryButtonText = "Discard and Change",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await contentdialog_selectionchange_properties.ShowAsync();


            if (result == ContentDialogResult.Primary)
            {


                return true;
            }
            else if (result == ContentDialogResult.Secondary)
            {


                return true;
            }
            else
            {
                return false;
            }


        }

        public void SelectedEntryTitleCalendarBlockHighlighted(string chosen_entryID, bool select_unselect)
        {
            //this will change the visual representation of the titleblock and calendarblock to selected or unselected
            //the boolean will determine the behaviour: true selects, false unselects

            Debug.WriteLine("SelectedEntryTitleCalendarBlockHighlighted     chosen_entryID = '" + chosen_entryID + "'");

            //finds the border that will be affected; unfortunately C# doesn't have a findbyID method...
            //(a couple days later)OH WAIT IT KINDA DOES NOW BIATCH

            if (chosen_entryID == null || chosen_entryID == "") { return; }
            Border affected_titleblock = glo_ProjectData.GetTitleBlockFromEntryID(chosen_entryID);
            Border affected_calendarblock = glo_ProjectData.GetCalendarBlockFromEntryID(chosen_entryID);

            //Border affected_border = new Border();
            //bool found_affected = false;
            //foreach (Border cur_border in grid_entry_title_blocks.Children)
            //{
            //    if (cur_border.Tag.ToString() == glo_tag_entryID + chosen_entryID.ToString())
            //    {
            //        affected_border = cur_border;
            //        found_affected = true;
            //        break;
            //    }
            //}

            ////if we didn't find a border with that entryID tag, we exit
            //if (!found_affected) { Debug.WriteLine("Did not find correct border. Exiting"); return; }

            //otherwise we will change its selected visual state
            switch (select_unselect)
            {
                //this means we want to select it
                case true:
                    Debug.WriteLine("Setting to Selected");
                    affected_titleblock.Style = Application.Current.Resources["bor_EntryTitleBlock_Selected"] as Style;
                    affected_calendarblock.Style = Application.Current.Resources["bor_EntryCalendarBlock_Selected"] as Style;
                    break;

                //this means we want to unselect it
                case false:
                    Debug.WriteLine("Setting to Default");
                    affected_titleblock.Style = Application.Current.Resources["bor_EntryTitleBlock"] as Style;
                    affected_calendarblock.Style = Application.Current.Resources["bor_EntryCalendarBlock"] as Style;
                    break;
            }
        }





        //these methods deal with the tapping of the titleblock pane buttons

        public void EnableTitleBlockButtons(bool set_enabled, int list_pos)
        {
            //if we want them all disabled...

            if (!set_enabled)
            {
                Debug.WriteLine("Disabling buttons");

                btn_delete.IsEnabled = false;
                btn_properties.IsEnabled = false;

                btn_clear_selection.IsEnabled = false;

                btn_move_up.IsEnabled = false;
                btn_move_down.IsEnabled = false;

                return;
            }

            //will continue if we want enabled
            Debug.WriteLine("Enabling buttons");

            btn_delete.IsEnabled = true;
            btn_properties.IsEnabled = true;
            btn_clear_selection.IsEnabled = true;

            //if there's only 1 item in list (max index of 0), don't want either move

            int list_max_index = glo_ProjectData.EntryData_lst_param.Count() - 1;

            Debug.WriteLine("list_pos = " + list_pos + "    list_max_index = " + list_max_index);

            if (list_max_index == 0)
            {
                Debug.WriteLine("Disabling move buttons as single-entry list");

                btn_move_up.IsEnabled = false;
                btn_move_down.IsEnabled = false;

                return;
            }


            //if at the start of list, don't want move up

            if (list_pos == 0)
            {
                Debug.WriteLine("Disabling move up as list_pos = " + list_pos);

                btn_move_up.IsEnabled = false;
            }
            else
            {
                Debug.WriteLine("Enabling move up button");

                btn_move_up.IsEnabled = true;
            }


            //if at the end of list, don't want move down

            if (list_pos == list_max_index)
            {
                Debug.WriteLine("Disabling move down as list_pos = " + list_pos);

                btn_move_down.IsEnabled = false;
            }
            else
            {
                Debug.WriteLine("Enabling move down button");

                btn_move_down.IsEnabled = true;
            }


        }
        private void PropertiesButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine("PropertiesButtonTapped");
            //await OpenPropertiesWindow();

            glo_PropertyPaneOpen = true;

        }
        private void DeleteButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            if (glo_selected_entryID != "")
            {
                DeleteEntryMethod();
            }
        }
        public void ClearSelectionButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            UnselectEntryData();
        }
        private void MoveButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            Button sending_button = (Button)sender;

            //string direction = "";
            int move_relative_int = 0;
            switch (sending_button.Name)
            {
                case "btn_move_up":
                    move_relative_int = -1; //moving up is decreasing index
                    break;

                case "btn_move_down":
                    move_relative_int = +1; //moving down is increasing index
                    break;
            }

            //this is the old method
            //MoveEntryUpDown(direction, glo_selected_EntryData.EntryID);

            //this is the new method
            MoveEntryByIndex(move_relative_int, glo_selected_EntryData.EntryID);
        }
        private void AddButtonTapped(object sender, RoutedEventArgs e)
        {
            OpenAddEntryWindow();
        }





        //these methods move entries up or down. MoveEntryUpDown and MoveEntryUpDownVisually are legacy and are not used

        private void MoveEntryUpDown(string direction, string entryID_to_move)
        {
            //this method will move entry titleblocks and calendar boxes up or down

            Debug.WriteLine("MoveEntryUpDown");


            //figuring out the direction requested

            int index_change = 0;
            if (direction == "up")
            {
                //up visually, lower in number
                index_change = -1;
            }
            else if (direction == "down")
            {
                //down visually, higher in number
                index_change = +1;
            }
            else
            {
                Debug.WriteLine("Unknown direction: " + direction);
                return;
            }


            //this is the maximum index that can happen
            int max_index = glo_ProjectData.EntryData_lst_param.Count - 1;


            //this makes a list of all the current positions
            glo_ProjectData.CreateEntryDataListPositionList();

            //just for verification...
            //glo_ProjectData.PrintProjectDataValues(false, true, true);


            //this gets the EntryData we want
            EntryData entryData_to_move = glo_ProjectData.GetEntryDataFromEntryID(entryID_to_move);

            if (entryData_to_move == null)
            {
                Debug.WriteLine("ERROR: glo_ProjectData.GetEntryDataFromEntryID failed");
                return;
            }

            //this is the list position that the titleblock is supposed to be in originally
            int position_original_EntryData = Convert.ToInt32(entryData_to_move.RowPosition);


            //this is the list position that we are wanting to move to
            int position_new_EntryData = position_original_EntryData + index_change;

            if ((position_new_EntryData < 0) | (position_new_EntryData > max_index))
            {
                Debug.WriteLine("ERROR: requested new position out of range");
                return;
            }


            //this is the entrydata that needs to be swapped with
            EntryData entryData_to_be_moved = glo_ProjectData.GetEntryDataFromListPosition(position_new_EntryData);
            string entryID_to_be_moved = entryData_to_be_moved.EntryID;



            //swapping the list positions in the global ProjectData
            Debug.WriteLine("Changing list positions in glo_ProjectData.EntryData_lst_param");
            Debug.WriteLine(entryData_to_move.Title + "(" + entryID_to_move + "): " + position_original_EntryData + " to " + position_new_EntryData);
            Debug.WriteLine(entryData_to_be_moved.Title + "(" + entryID_to_be_moved + "): " + position_new_EntryData + " to " + position_original_EntryData);


            entryData_to_move.RowPosition = position_new_EntryData;
            entryData_to_be_moved.RowPosition = position_original_EntryData;

            //updates the list position list
            glo_ProjectData.CreateEntryDataListPositionList();


            //just for verification...
            //glo_ProjectData.PrintProjectDataValues(false, true, true);


            //now that the global save lists have been updated, this method will update the visual positions of the controls
            MoveEntryUpDownVisually(entryData_to_move, entryData_to_be_moved, position_original_EntryData, position_new_EntryData);


            //this will enable/disable the move up and down buttons if needed
            EnableTitleBlockButtons(true, Convert.ToInt32(entryData_to_move.RowPosition));


            return;

        }
        private void MoveEntryUpDownVisually(EntryData entryData_to_move, EntryData entryData_to_be_moved, 
            int position_original_EntryData, int position_new_EntryData)
        {
            //this method will update the visual position of titleblocks and calendar entries when being moved

            string entryID_to_move = entryData_to_move.EntryID;
            string entryID_to_be_moved = entryData_to_be_moved.EntryID;


            //doing the titleblocks first
            foreach (Border cur_bor_titleblock in grid_entry_title_blocks.Children)
            {
                //finding the to_move and to_be_moved borders

                //the selected one to move
                if (cur_bor_titleblock.Tag.ToString() == glo_tag_entryID_prefix + entryID_to_move.ToString())
                {
                    //so checking if it's in the correct spot originally, just to find out if the order is corrupt
                    if (Grid.GetRow(cur_bor_titleblock) != position_original_EntryData)
                    {
                        FixBlockInfo(false, false, false, true, false);
                        return;
                    }

                    //setting the new row position
                    Grid.SetRow(cur_bor_titleblock, Convert.ToInt32(entryData_to_move.RowPosition));

                }

                //the selected one to be swapped with
                if (cur_bor_titleblock.Tag.ToString() == glo_tag_entryID_prefix + entryID_to_be_moved.ToString())
                {
                    //so checking if it's in the correct spot originally, just to find out if the order is corrupt
                    if (Grid.GetRow(cur_bor_titleblock) != position_new_EntryData)
                    {
                        FixBlockInfo(false, false, false, true, false);
                        return;
                    }

                    //setting the new row position
                    Grid.SetRow(cur_bor_titleblock, Convert.ToInt32(entryData_to_be_moved.RowPosition));

                }


            }


            //now to do the calendar!
            foreach (Border cur_bor_calendarblock in grid_calendar.Children)
            {
                //finding the to_move and to_be_moved borders

                //the selected one to move
                if (cur_bor_calendarblock.Tag.ToString() == glo_tag_entryID_prefix + entryID_to_move.ToString())
                {
                    //so checking if it's in the correct spot originally, just to find out if the order is corrupt
                    if (Grid.GetRow(cur_bor_calendarblock) != position_original_EntryData)
                    {
                        Debug.WriteLine("ERROR: corrupt row position of calendar blocks");
                        return;
                    }

                    //setting the new row position
                    Grid.SetRow(cur_bor_calendarblock, Convert.ToInt32(entryData_to_move.RowPosition));

                }

                //the selected one to be swapped with
                if (cur_bor_calendarblock.Tag.ToString() == glo_tag_entryID_prefix + entryID_to_be_moved.ToString())
                {
                    //so checking if it's in the correct spot originally, just to find out if the order is corrupt
                    if (Grid.GetRow(cur_bor_calendarblock) != position_new_EntryData)
                    {
                        Debug.WriteLine("ERROR: corrupt row position of calendar blocks");
                        return;
                    }

                    //setting the new row position
                    Grid.SetRow(cur_bor_calendarblock, Convert.ToInt32(entryData_to_be_moved.RowPosition));

                }


            }



        }

        private void MoveEntryByIndex(int index_change, string entryID_to_move)
        {
            //this method will move entry titleblocks and calendar boxes up or down by a specific amount

            Debug.WriteLine("\nMoveEntryByIndex     entryID_to_move = '" + 
                entryID_to_move + "'     index_change = " + index_change);



            //this is the maximum index that can happen
            int max_index = glo_ProjectData.EntryData_lst_param.Count - 1;



            //this makes a list of all the current positions
            glo_ProjectData.CreateEntryDataListPositionList();


            //just for verification...
            //glo_ProjectData.PrintProjectDataValues(false, true, true);


            //this gets the EntryData we want
            EntryData entryData_to_move = glo_ProjectData.GetEntryDataFromEntryID(entryID_to_move);

            if (entryData_to_move == null)
            {
                Debug.WriteLine("ERROR: glo_ProjectData.GetEntryDataFromEntryID failed");
                return;
            }

            //these are the start and ending indices of the moving entryData
            int move_from = entryData_to_move.RowPosition;
            int move_to = move_from + index_change;

            //the other entries will move by 1 in the opposite directions
            int collateral_move = Math.Sign(index_change) * -1;


            //stops if the requested index is out of bounds
            if (move_to < 0 || max_index < move_to)
            {
                Debug.WriteLine("ERROR: move_to index (" + move_to + ") out of bounds");
                return;
            }



            //will go through all the EntryData's
            foreach (EntryData cur_entryData in glo_ProjectData.EntryData_lst_param)
            {
                int cur_check_index = cur_entryData.RowPosition;
                string how_should_move = ShouldEntryBeMoved(cur_check_index, move_from, move_to);

                int cur_move_to = -1;
                bool is_moving = false;
                switch (how_should_move)
                {
                    case "requested_move":
                        //then this is the entry that's being moved
                        cur_move_to = move_to;
                        is_moving = true;
                        break;

                    case "collateral_move":
                        //then this is an entry that a collateral move
                        cur_move_to = cur_check_index + collateral_move;
                        is_moving = true;
                        break;

                    case "no_move":
                        //then this is an entry that should not move
                        cur_move_to = cur_check_index;
                        break;
                }

                if (is_moving)
                {
                    Debug.WriteLine(cur_entryData.Title + " (" + cur_entryData.EntryID + "): " + cur_check_index + " to " + cur_move_to);

                    cur_entryData.RowPosition = cur_move_to;
                }


            }


            //updates the list position list
            glo_ProjectData.CreateEntryDataListPositionList();


            //just for verification...
            //glo_ProjectData.PrintProjectDataValues(false, true, true);



            //now that the global save lists have been updated, must update the visual positions of the controls


            //this will refresh all the titleblocks to be in their new position
            //FixBlockInfo(false, false, false, true, false);




            //this will enable/disable the move up and down buttons if needed
            EnableTitleBlockButtons(true, Convert.ToInt32(entryData_to_move.RowPosition));


            Debug.WriteLine("");

            return;
        }
        public string ShouldEntryBeMoved(int check_index, int start_index, int end_index)
        {
            //this will determine if the entry should be moved
            //should be moved if it's at the end index, or between the start and end (non-inclusive)
            //start index is where the requested entry is starting from, and ends at the ending index
            //so the requested entry will move from start to end, and the other entries will move as collateral

            int[] endpoints = new int[] { start_index, end_index };

            int small_index = endpoints.Min();
            int large_index = endpoints.Max();


            if (check_index == start_index)
            {
                return "requested_move";
            }
            else if ((check_index == end_index) || (small_index < check_index && check_index < large_index))
            {
                return "collateral_move";
            }
            else
            {
                return "no_move";
            }
        }





        //these methods deal with the adding of new entries

        private async void OpenAddEntryWindow()
        {
            contentdialog_add_new_entry.Visibility = Visibility.Visible;
            glo_add_new_window_open = true;

            glo_AddNewEntry = new AddNewEntry();

            glo_AddNewEntry.glo_ProjectData_Reference = glo_ProjectData;
            glo_AddNewEntry.glo_MainPage_Reference = this;

            contentdialog_add_new_entry.DataContext = glo_AddNewEntry;

            ContentDialogResult result = await contentdialog_add_new_entry.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                //we close and save the new entry
                CloseAddEntryWindow(true);
            }
            else
            {
                //we close and discard the new entry
                CloseAddEntryWindow(false);
            }

        }
        private void CloseAddEntryWindow(bool save_new_entry)
        {
            //if we want to save the new entry on close
            if (save_new_entry)
            {
                AddEntryMethod();
            }

            //now we reset the form on the content dialog to blanks
            //txtbox_add_new_entry_title.Text = "";
            //txtbox_add_new_entry_subtitle.Text = "";
            //btn_add_new_entry_colour.Tag = "";

            glo_add_new_window_open = false;

        }

        public void AddNewEntryUpdatePreview()
        {
            //this method will update the preview border when adding a new entry, triggered on any textblock or colour change

            Debug.WriteLine("AddNewEntryUpdatePreview");

            Border preview_border = CreateEntryTitleBlock(glo_AddNewEntry.GetEntryDataForPreview(), true);

            //removes the border that's already there
            if (stack_add_new_entry_preview.Children.Count() > 1)
            {
                stack_add_new_entry_preview.Children.RemoveAt(1);
            }

            //and puts in the new one
            stack_add_new_entry_preview.Children.Add(preview_border);
        }
        private void AddNewEntryPreviewBlockTapped(Border sending_preview_titleblock)
        {
            Style unselected_style = Application.Current.Resources["bor_AddNewEntryPreviewTitleBlock"] as Style;
            Style selected_style = Application.Current.Resources["bor_AddNewEntryPreviewTitleBlock_selected"] as Style;

            Style change_to_style;

            if (sending_preview_titleblock.Style == unselected_style)
            {
                change_to_style = selected_style;
            }
            else
            {
                change_to_style = unselected_style;
            }

            sending_preview_titleblock.Style = change_to_style;


        }
        private void AddNewEntryValidConfirmButton(bool confirm_button_enabled)
        {

            //applies the enabled/disabled
            contentdialog_add_new_entry.IsPrimaryButtonEnabled = confirm_button_enabled;
        }
        public bool AddNewEntryTextboxValidInput()
        {
            //TODO this method will do text verification on the textboxes
            //shouldn't be allowed to use ( ‖ † ‡ ) as these are used in the entryIDs

            return true;
        }
        private void AddNewEntryTextboxChanged(object sender, TextChangedEventArgs e)
        {
            //if (!AddNewEntryTextboxValidInput()) { Debug.WriteLine("ERROR: the inputted text is invalid"); return; }

            //TextBox sending_txtbox = (TextBox)sender;

            //string new_value;
            //switch (sending_txtbox.Name)
            //{
            //    case "txtbox_add_new_entry_title":
            //        new_value = txtbox_add_new_entry_title.Text.ToString();
            //        Debug.WriteLine(string.Format("Updating    glo_AddNew_EntryData.{0} = {1}", "Title", new_value));
            //        glo_AddNew_EntryData.Title = new_value;
            //        break;

            //    case "txtbox_add_new_entry_subtitle":
            //        new_value = txtbox_add_new_entry_subtitle.Text.ToString();
            //        Debug.WriteLine(string.Format("Updating    glo_AddNew_EntryData.{0} = {1}", "Subtitle", new_value));
            //        glo_AddNew_EntryData.Subtitle = new_value;
            //        break;
            //}

            //AddNewEntryUpdatePreview();

            //string new_entryID = "";
            //if (AddNewEntryValid())
            //{
            //    AddNewEntryValidConfirmButton(true);
            //    new_entryID = CreateNewEntryID(glo_AddNew_EntryData.Title);
            //}
            //else
            //{
            //    AddNewEntryValidConfirmButton(false);
            //}

            //Debug.WriteLine("new_entryID = " + new_entryID);

            //if (new_entryID == "invalid") { new_entryID = ""; Debug.WriteLine("ERROR: new_entryID = '" + new_entryID + "'"); }

            //glo_AddNew_EntryData.EntryID = new_entryID;
            ////Debug.WriteLine("glo_AddNew_EntryData.EntryID = " + glo_AddNew_EntryData.EntryID);
        }
        public void ShowAddNewEntryTitleInvalidTeachingTip()
        {
            teach_addnewentry_title_invalid.IsOpen = true;
        }

        private void AddEntryMethod()
        {
            //this method will create a new EntryData and corresponding titleblock

            Debug.WriteLine("AddEntryMethod");


            ////just final checks to ensure that all required data is there and it's not a duplicate
            //if (!AddNewEntryValid())
            //{
            //    Debug.WriteLine("ERROR: Add new entry NOT valid");
            //    return;
            //}
            //if (string.IsNullOrEmpty(glo_AddNew_EntryData.EntryID))
            //{
            //    Debug.WriteLine("ERROR: Add new entry has null or empty EntryID");
            //    return;
            //}



            //first to create the new EntryData based off the addnew class
            EntryData new_entryData = new EntryData();
            ApplyAddNewEntryDetailsToEntryData(ref new_entryData);


            //final data that needs to be figured out is the list position

            //creating the master list of positions
            glo_ProjectData.CreateEntryDataListPositionList();

            //TODO add functionality to add at another index

            //this is the new end of the list
            int new_list_pos = glo_ProjectData.entryDataListPos_lst_param.Max() + 1;

            new_entryData.RowPosition = new_list_pos;



            //the avail_col_ids list must also contain values or else it will not work. If the user didn't set these, assume it's allowed in all cols
            if (new_entryData.AvailColIDs.Count == 0)
            {
                new_entryData.AvailColIDs = CreateDeepCopyOfList_string(glo_ProjectData.colIDs_lst_param);
            }



            //this will put the new entrydata into the lists and calendar display
            InsertNewEntryData(new_entryData);

            UpdateCalendarLayout();
        }
        private void ApplyAddNewEntryDetailsToEntryData(ref EntryData new_entryData)
        {
            //this will use the ref and pointers to edit the new_entryData that's being created in the previous method


            //gets the new changed values from the changed properties
            glo_AddNewEntry.GetFieldData(
                out string entryID,
                out string title,
                out string subtitle,
                out string colourhex,
                out List<string> prereq_entryIDs,
                out List<string> coreq_entryIDs,
                out List<string> avail_colIDs);


            if (entryID != null && entryID != "")
            {
                new_entryData.EntryID = entryID;
            }
            else
            {
                new_entryData.EntryID = "invalid_entryID";
            }


            if (title != null && title != "")
            {
                new_entryData.Title = title;
            }
            else
            {
                new_entryData.Title = "invalid_title";
            }


            if (subtitle != null)
            {
                new_entryData.Subtitle = subtitle;
            }
            else
            {
                new_entryData.Subtitle = "invalid_subtitle";
            }


            if (colourhex != null && colourhex != "")
            {
                new_entryData.ColourHex = colourhex;
            }
            else
            {
                new_entryData.ColourHex = glo_default_titleblock_colour_hex;
            }


            if (prereq_entryIDs != null)
            {
                new_entryData.PrereqEntryIDs = prereq_entryIDs;
            }
            else
            {
                new_entryData.PrereqEntryIDs = new List<string>();
            }


            if (coreq_entryIDs != null)
            {
                new_entryData.CoreqEntryIDs = coreq_entryIDs;
            }
            else
            {
                new_entryData.CoreqEntryIDs = new List<string>();
            }


            if (avail_colIDs != null)
            {
                new_entryData.AvailColIDs = avail_colIDs;
            }
            else
            {
                new_entryData.AvailColIDs = new List<string>();
            }
        }
        private void InsertNewEntryData(EntryData inserted_entryData)
        {
            //this method will take the inserted entrydata and will put it into the master list in  glo_ProjectData
            //then will put it into the titleblocks
            //and will do a logic run for the calendar



            //this puts it into the master list, and returns the index in the master list for easy indexing
            int inserted_index = glo_ProjectData.AddNewEntryData(inserted_entryData);


            //deciding to use the index as if later I just used inserted_entryData then maybe the shallow copying will break?


            //now to create the titleblock using the known index of the new entryData
            AddMissingBorders("title_calendar");
            //CreateAndPlaceNewTitleBlock(glo_ProjectData.EntryData_lst_param[inserted_index]);


            //will update the layout of the titleblocks
            CreateCorrectNumberOfGridRowsColumns("titleblock_rows");


            //and now will update the calendar logic and display
            UpdateCalendarLogicDisplay(true, true);


            //will update the used colour list
            CreateUsedColourList();





            //Border new_titleblock = CreateEntryTitleBlock(cur_entryData) as Border;
            //grid_entry_title_blocks.Children.Add(new_titleblock);
        }






        //these methods deal with deleting of entries

        public async void DeleteEntryMethod()
        {
            //this method is called to delete the currently selected entryData

            Debug.WriteLine("DeleteEntryMethod");



            string selected_entryID_to_delete = glo_selected_entryID;

            //ensures that this is a valid entryID
            if (!glo_ProjectData.entryIDs_lst_param.Contains(selected_entryID_to_delete))
            {
                Debug.WriteLine("ERROR: The entryID  '" + selected_entryID_to_delete
                    + "'  does not exist in  glo_ProjectData.entryIDs_lst_param");
                return;
            }


            //this gets user verification on the delete
            bool proceed = await VerifyDelete(selected_entryID_to_delete);

            if (!proceed)
            {
                Debug.WriteLine("Delete of entryID  '" + selected_entryID_to_delete + "'  was not verified. Exiting.");
                return;
            }


            Debug.WriteLine("Delete of entryID  '" + selected_entryID_to_delete + "'  verified. Deleting...");




            //remove it from selected
            UnselectEntryData();

            //remove it from lists
            glo_ProjectData.DeleteEntryDataAtIndex(selected_entryID_to_delete);

            //remove it from grids
            RemoveDeletedBlocks("both");

            //and update all the other titleblocks and calendarblocks to update their rows
            //FixBlockInfo(false, false, false, true, false);


            UpdateCalendarLayout();




        }
        public async Task<bool> VerifyDelete(string entryID_to_be_deleted)
        {
            //TODO make this setting work for do not ask again delete
            bool do_not_ask_for_verification = false;



            //if we are asked not to ask for verification, we return true
            if (do_not_ask_for_verification) { return true; }


            //otherwise, we have to open a verification dialog box
            contentdialog_confirm_delete.Visibility = Visibility.Visible;


            //this fills in fields on the dialog box
            EntryData entryData_to_be_deleted = glo_ProjectData.GetEntryDataFromEntryID(entryID_to_be_deleted);
            txtblc_to_be_deleted_title.Text = entryData_to_be_deleted.Title;
            txtblc_to_be_deleted_subtitle.Text = entryData_to_be_deleted.Subtitle;


            ContentDialogResult result = await contentdialog_confirm_delete.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                //we delete the selected entry
                return true;
            }
            else
            {
                //we do nothing
                return false;
            }

        }
        private void DeleteDoNotAskAgainChecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("DeleteDoNotAskAgainChecked");

            //TODO create this "do not ask again" functionality, stored in a proper setting
        }
        private void DeleteDoNotAskAgainUnchecked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("DeleteDoNotAskAgainUnchecked");
        }





        //these methods deal with the property pane. much of its functionality is data binding with ChangedProperties.cs

        public void OpenPropertiesMethod()
        {
            //glo_property_pane_open = true;

            //this prepares the ChangedProperties class which will keep track of all property editing
            PrepareChangedProperties();

            //will make the properties pane visible
            grid_properties_pane.Visibility = Visibility.Visible;
        }
        public void PrepareChangedProperties()
        {
            glo_ChangedProperties = new ChangedProperties();
            glo_ChangedProperties.LoadOriginalEntryData(glo_selected_EntryData);
            grid_properties_pane.DataContext = glo_ChangedProperties;

            glo_ChangedProperties.glo_ProjectData_Reference = glo_ProjectData;

        }
        private void ClosePropertiesPane_Tapped(object sender, TappedRoutedEventArgs e)
        {
            glo_PropertyPaneOpen = false;
        }
        private void ClosePropertiesMethod()
        {
            //sets a flag saying properties are closed. must happen before the data is cleared or else there's bad errors that happen when the textboxes "change"
            //glo_property_pane_open = false;

            //closes the pane
            grid_properties_pane.Visibility = Visibility.Collapsed;
        }
        public void PrintPropertiesEntryDatas()
        {
            glo_ChangedProperties.PrintProperties(glo_selected_EntryData);
        }

        private void ApplyPropertiesButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ApplyPropertiesMethod(false);
        }
        private void ApplyPropertiesMethod(bool changing_selection)
        {
            Debug.WriteLine("\nApplyPropertiesMethod");


            //apply the new properties
            bool proceed_with_apply = false;


            //if the setting for requiring verification is enabled...
            if (glo_properties_apply_need_verification)
            {
                //ContentDialog applyPropertiesDialog = new ContentDialog
                //{
                //    Title = "Apply property changes?",
                //    Content = "You will not be able to undo these changes", //TODO add a list of all the changes to be made 
                //    PrimaryButtonText = "Apply Changes",
                //    CloseButtonText = "Cancel"
                //};

                //ContentDialogResult result = await applyPropertiesDialog.ShowAsync();

                //// Apply the changes if the user clicked the primary button.
                ///// Otherwise, do nothing.
                //if (result == ContentDialogResult.Primary)
                //{
                //    // Delete the file.
                //}
                //else
                //{
                //    // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                //    // Do nothing.
                //}
            }

            //if the setting isn't enabled, then just proceed
            else
            {
                proceed_with_apply = true;
            }



            //exit if not proceeding
            if (!proceed_with_apply) { return; }


            //if proceeding...
            Debug.WriteLine("\nProceeding with apply");


            //the entryID of the EntryData we are changing
            string applying_entryID = glo_ChangedProperties.EntryID_Old;
            Debug.WriteLine("applying_entryID = '" + applying_entryID + "'");

            //the master EntryData that we are changing
            EntryData master_entryData = glo_ProjectData.GetEntryDataFromEntryID(applying_entryID);


            ApplyNewPropertiesToEntryData(master_entryData);

        }
        private void ApplyNewPropertiesToEntryData(EntryData master_entryData)
        {
            //thanks to pointers, this will update the data in the actual master_entryData


            //gets the new changed values from the changed properties
            glo_ChangedProperties.GetFieldData(
                out string entryID,
                out string title,
                out string subtitle,
                out string colourhex,
                out string setcolID,
                out List<string> prereq_entryIDs,
                out List<string> coreq_entryIDs,
                out List<string> avail_colIDs);


            if (entryID != null && entryID != "")
            {
                master_entryData.EntryID = entryID;
            }

            if (title != null && title != "")
            {
                master_entryData.Title = title;
            }

            if (subtitle != null && subtitle != "")
            {
                master_entryData.Subtitle = subtitle;
            }

            if (colourhex != null && colourhex != "")
            {
                master_entryData.ColourHex = colourhex;
            }

            if (setcolID != null && setcolID != "")
            {
                master_entryData.SetColID = setcolID;
            }

            if (prereq_entryIDs != null && prereq_entryIDs.Count != 0 )
            {
                master_entryData.PrereqEntryIDs = prereq_entryIDs;
            }

            if (coreq_entryIDs != null && coreq_entryIDs.Count != 0)
            {
                master_entryData.CoreqEntryIDs = coreq_entryIDs;
            }

            if (avail_colIDs != null && avail_colIDs.Count != 0)
            {
                master_entryData.AvailColIDs = avail_colIDs;
            }


            //will update the used colour list
            CreateUsedColourList();
        }





        //these methods deal with the colour picker

        private void ColourPickerFlyout_Opened(object sender, object e)
        {
            //for some reason
            if (!glo_add_new_window_open && !glo_property_pane_open_)
            {
                return;
            }

            //puts the correct colour into the colour picker

            if (glo_add_new_window_open)
            {
                myColorPicker.Color = GetSolidColorBrushFromHex(glo_AddNewEntry.ColourHex_Add).Color;
            }
            else //if (glo_property_pane_open_)
            {
                myColorPicker.Color = GetSolidColorBrushFromHex(glo_ChangedProperties.ColourHex_New).Color;
            }


            //clears the used colours variable grid
            vargrid_used_colours.Children.Clear();

            //now to create the new colours in it, if it exists
            if (glo_CalculatedData.UsedColoursHex == null) { return; }
            foreach (string cur_used_colours_hex in glo_CalculatedData.UsedColoursHex)
            {
                //will make a colour grid for each used colour

                //  <Grid Style="{StaticResource grid_UsedColour}" Tapped="InUseColourTapped" Background="Turquoise"/>

                Grid used_colour_grid = new Grid();

                used_colour_grid.Style = Application.Current.Resources["grid_UsedColour"] as Style;
                used_colour_grid.Tapped += InUseColourTapped;

                used_colour_grid.Background = GetSolidColorBrushFromHex(cur_used_colours_hex);

                vargrid_used_colours.Children.Add(used_colour_grid);

            }
        }
        private void InUseColourTapped(object sender, TappedRoutedEventArgs e)
        {
            Grid sending_colour_grid = sender as Grid;
            SolidColorBrush background_brush = (SolidColorBrush)sending_colour_grid.Background;

            Windows.UI.Color background_colour = background_brush.Color;

            myColorPicker.Color = background_colour;

        }
        private void ColourPicker_ColourChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            Windows.UI.Color picker_colour = myColorPicker.Color;

            foreach (Grid cur_colour_grid in vargrid_used_colours.Children)
            {
                SolidColorBrush cur_background_brush = (SolidColorBrush)cur_colour_grid.Background;
                Windows.UI.Color cur_background_colour = cur_background_brush.Color;

                if (picker_colour == cur_background_colour) { cur_colour_grid.Style = Application.Current.Resources["grid_UsedColour_Selected"] as Style; }

                else { cur_colour_grid.Style = Application.Current.Resources["grid_UsedColour"] as Style; }
            }
        }
        private void confirmColor_Click(object sender, RoutedEventArgs e)
        {

            string chosen_colour_hex = GetHexFromColorPicker(myColorPicker.Color);

            //glo_CalculatedData.ColourPickerChosenHex = chosen_colour_hex;



            //now that we have the colour hex, we need to choose where to put it
            //it will be stored in the tag of the button that called the flyout

            if (glo_add_new_window_open)
            {
                //this will happen if the add new entry window is open

                glo_AddNewEntry.ColourHex_Add = chosen_colour_hex;

                //ChangeColour_AddNewEntry(chosen_colour_hex);

                // Close the Flyout.
                btn_add_new_entry_colour.Flyout.Hide();
            }
            else if (glo_PropertyPaneOpen)
            {
                //this will happen if the properties pane is open and the add new entry window isn't

                glo_ChangedProperties.ColourHex_New = chosen_colour_hex;

                //ChangeColour_Properties(chosen_colour_hex);

                // Close the Flyout.
                btn_properties_change_colour.Flyout.Hide();
            }



        }


        private void cancelColor_Click(object sender, RoutedEventArgs e)
        {
            glo_CalculatedData.ColourPickerChosenHex = glo_CalculatedData.DefaultColourChosenHex;




            if (glo_add_new_window_open)
            {
                //this will happen if the add new entry window is open

                // Close the Flyout.
                btn_add_new_entry_colour.Flyout.Hide();
            }
            else if (glo_PropertyPaneOpen)
            {
                //this will happen if the properties pane is open and the add new entry window isn't

                // Close the Flyout.
                btn_properties_change_colour.Flyout.Hide();
            }
        }

        public void ChangeColour_AddNewEntry(string new_colour_hex)
        {
            btn_add_new_entry_colour.Tag = new_colour_hex;


            Debug.WriteLine(string.Format("Updating    glo_AddNew_EntryData.{0} = {1}", "ColourHex", new_colour_hex));
            glo_AddNew_EntryData.ColourHex = new_colour_hex;


            AddNewEntryUpdatePreview();
        }
        public void ChangeColour_Properties(string new_colour_hex)
        {
            glo_ChangedProperties.ColourHex_New = new_colour_hex;
            //btn_properties_change_colour.Tag = new_colour_hex;
        }

        private void CreateUsedColourList()
        {
            Debug.WriteLine("CreateUsedColourList");

            //thiw will make a list of all the used colours
            List<string> return_list = new List<string>();


            foreach (EntryData cur_entry in glo_ProjectData.EntryData_lst_param)
            {
                string cur_hex = cur_entry.ColourHex;

                if (!return_list.Contains(cur_hex))
                {
                    return_list.Add(cur_hex);
                }
            }

            glo_CalculatedData.UsedColoursHex = return_list;
        }





        //these are general utilities

        private List<int> CreateDeepCopyOfList_int(List<int> list_to_be_copied)
        {
            List<int> return_lst = new List<int>(list_to_be_copied.Count);

            for (int i = 0; i < return_lst.Count; i++)
            {
                return_lst[i] = list_to_be_copied[i];
            }

            return return_lst;
        }
        private List<string> CreateDeepCopyOfList_string(List<string> list_to_be_copied)
        {
            //if the given list is null, we return null
            if (list_to_be_copied == null) { return null; }


            List<string> return_lst = new List<string>(new string[list_to_be_copied.Count]);

            for (int i = 0; i < return_lst.Count; i++)
            {
                return_lst[i] = list_to_be_copied[i];
            }

            return return_lst;
        }
        private EntryData CreateDeepCopyOfEntryData(EntryData entryData_to_be_copied)
        {
            EntryData return_entryData = new EntryData();

            EquateEntryData(return_entryData, entryData_to_be_copied);

            return return_entryData;
        }
        private void EquateEntryData(EntryData primary_entryData, EntryData secondary_entryData)
        {
            //due to pointers, this will change the primary entryData to equate the secondary without needing to return it

            Debug.WriteLine("EquateEntryData");

            primary_entryData.Entry_ProjectName     = secondary_entryData.Entry_ProjectName;
            primary_entryData.EntryID               = secondary_entryData.EntryID;
            primary_entryData.Title                 = secondary_entryData.Title;
            primary_entryData.Subtitle              = secondary_entryData.Subtitle;
            primary_entryData.ColourHex             = secondary_entryData.ColourHex;
            primary_entryData.ActualColID           = secondary_entryData.ActualColID;
            primary_entryData.SetColID              = secondary_entryData.SetColID;

            primary_entryData.RowPosition           = secondary_entryData.RowPosition;
            primary_entryData.ColPosition           = secondary_entryData.ColPosition;

            primary_entryData.PrereqEntryIDs        = CreateDeepCopyOfList_string(secondary_entryData.PrereqEntryIDs);
            primary_entryData.CoreqEntryIDs         = CreateDeepCopyOfList_string(secondary_entryData.CoreqEntryIDs);
            primary_entryData.AvailColIDs           = CreateDeepCopyOfList_string(secondary_entryData.AvailColIDs);

            primary_entryData.TitleBlock            = secondary_entryData.TitleBlock;
            primary_entryData.CalendarBlock         = secondary_entryData.CalendarBlock;

            primary_entryData.Is_Selected           = secondary_entryData.Is_Selected;


            /*            

            string Entry_ProjectName
            string EntryID
            string Title
            string Subtitle
            string ColourHex
            string ActualColID
            string SetColID

            int RowPosition
            int ColPosition
            
            List<string> PrereqEntryIDs
            List<string> CoreqEntryIDs
            List<string> AvailColIDs
            
            Border TitleBlock
            Border CalendarBlock

            bool Is_Selected

            string StyleName_Title
            string StyleName_Calendar
            string StyleName_Preview

            */

        }

        static public SolidColorBrush GetSolidColorBrushFromHex(string hex)
        {
            //from http://www.joeljoseph.net/converting-hex-to-color-in-universal-windows-platform-uwp/

            //if the given hex was null, then we just assume the default
            if (hex == null) { hex = glo_default_titleblock_colour_hex; }

            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush;
        }
        public string GetHexFromSolidColorBrush(SolidColorBrush given_SCB)
        {
            //colours are fucked in C#, so to get the hex of a solidcolorbrush, there's steps involved

            Color old_colour = given_SCB.Color;

            string hex_value = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", old_colour.A, old_colour.R, old_colour.G, old_colour.B);

            //System.Drawing.Color converted_colour = System.Drawing.Color.FromArgb(old_colour.A, old_colour.R, old_colour.G, old_colour.B);



            //string hexValue = System.Drawing.ColorTranslator.ToHtml(converted_colour);

            return hex_value;
        }
        public string GetHexFromColorPicker(Windows.UI.Color given_colour)
        {
            //colours are fucked in C#, so to get the hex of a solidcolorbrush, there's steps involved

            string hex_value = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", given_colour.A, given_colour.R, given_colour.G, given_colour.B);

            //System.Drawing.Color converted_colour = System.Drawing.Color.FromArgb(old_colour.A, old_colour.R, old_colour.G, old_colour.B);



            //string hexValue = System.Drawing.ColorTranslator.ToHtml(converted_colour);

            return hex_value;
        }

        private void PrintList(string list_name, List<string> list_to_print)
        {
            Debug.WriteLine("\nPrinting the list: " + list_name + "\n--------------------");

            //checking to see if there are things in the list
            if (list_to_print.Count < 1) { Debug.WriteLine("There are no indices in this list\n"); return; }

            //checking to see if the list contains strings or numbers
            //if (list_to_print[0].GetType() != typeof(string)) { Debug.WriteLine("This \n"); return; } TODO



            for (int i = 0; i < list_to_print.Count; i++)
            {
                Debug.WriteLine(list_to_print[i]);
            }
            Debug.WriteLine("--------------------\nEnd of printed list\n");
        }
        private void PrintList_int(string list_name, List<int> list_to_print)
        {
            Debug.WriteLine("\nPrinting the list: " + list_name + "\n--------------------");

            //checking to see if there are things in the list
            if (list_to_print.Count < 1) { Debug.WriteLine("There are no indices in this list\n"); return; }

            //checking to see if the list contains strings or numbers
            //if (list_to_print[0].GetType() != typeof(string)) { Debug.WriteLine("This \n"); return; } TODO



            for (int i = 0; i < list_to_print.Count; i++)
            {
                Debug.WriteLine(list_to_print[i].ToString());
            }
            Debug.WriteLine("--------------------\nEnd of printed list\n");
        }





        //these methods are the tap handlers for the temporary commandbar buttons, for testing purposes
        private void LoadTestDataButton(object sender, RoutedEventArgs e)
        {
            //LoadProjectData(@"ms-appx:///Assets/TestData/", "Test Semesters 1");
            LoadProjectData(@"ms-appx:///Assets/TestData/", "Test Semesters 2");
        }
        private void ComputeCalendarButton(object sender, RoutedEventArgs e)
        {
            UpdateCalendarLogicDisplay(false, true);
        }
        private void PrintGlobalData(object sender, RoutedEventArgs e)
        {
            glo_ProjectData.PrintProjectDataValues(true, true, true);
        }
        private void EditButton(object sender, RoutedEventArgs e)
        {
            //test_button_datatemplate.ContentTemplate = this.Resources["control_template_test"] as DataTemplate;
            //test_button_datatemplate.DataContext = glo_ProjectData.EntryData_lst_param[2];

            //relpan_custom_window.Visibility = Visibility.Visible;
            //relpan_custom_window.Translation += new Vector3(0, 0, 32);
            //rec_frame_background.Visibility = Visibility.Visible;

            PrintPropertiesEntryDatas();

        }
        private void ContactButton(object sender, RoutedEventArgs e)
        {
            //this is a misc button that I'll use for testing commands


            //List<string> save_list = FormatCurrentSaveFile();

            //foreach (string cur_line in save_list)
            //{
            //    Debug.WriteLine(cur_line);
            //}


            Debug.WriteLine(txtbox_prop_title.Style.ToString());


        }
        private void GlobeButton(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("grid_entry_title_blocks.Children.Count = " + grid_entry_title_blocks.Children.Count);
            Debug.WriteLine("grid_calendar.Children.Count = " + grid_calendar.Children.Count);

            //string test_entryID = "geog_240";

            //Border test_border = glo_ProjectData.GetEntryDataFromEntryID(test_entryID).TitleBlock;
        }

        EntryDataTest glo_entryDataTesting = new EntryDataTest();

        private void PrintAddNewEntryClass(object sender, TappedRoutedEventArgs e)
        {
            glo_AddNewEntry.PrintProperties();
        }
    }
}

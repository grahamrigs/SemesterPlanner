using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Syndication;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SemesterPlanner
{
    public class ProjectData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            //Debug.WriteLine("OnPropertyChanged     " + name);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private bool project_loaded_ = false;
        public bool ProjectLoaded
        {
            get { return project_loaded_; }
            set
            {
                if (project_loaded_ != value)
                {
                    project_loaded_ = value;
                    OnPropertyChanged();
                }
            }
        }



        public StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;

        //public MainPage Glo_MainPage = MasterClass.Cur_MainPage;

        private string project_name_ = "";
        public string ProjectName 
        { 
            get { return project_name_; }
            set
            {
                if (project_name_ != value)
                {
                    project_name_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ColumnType { get; set; }
        public GridLength HeaderWidth { get; set; }

        public DateTime LastModified = DateTime.Now;
        public string LocalRoaming { get; set; }

        public List<string> Parameter_Names = new List<string> { "ProjectName", "ColumnType", "HeaderWidth" };
        public List<string> Parameter_Save_Names = new List<string> { "", "column-type", "header-width" };


        private string glo_selected_entryID = "";
        private EntryData glo_selected_entryData = new EntryData();

        public string Glo_Selected_EntryID
        {
            get { return glo_selected_entryID; }
            set
            {
                if (value != glo_selected_entryID)
                {
                    glo_selected_entryID = value;
                    ChangeSelectedEntry();
                }
            }
        }
        public EntryData Glo_Selected_EntryData
        {
            get { return glo_selected_entryData; }
            set
            {
                if (value != glo_selected_entryData)
                {
                    glo_selected_entryData = value;
                    MasterClass.Cur_MainPage.EnableTitleBlockButtons();
                }
            }
        }

        public AddNewEntry Glo_AddNewEntry = new AddNewEntry();
        public ChangedProperties Glo_ChangedProperties = new ChangedProperties();

        public List<ColumnData> ColumnData_lst_param { get; set; }
        public List<string> ColIDs_lst { get; set; }
        public List<EntryData> EntryData_lst_param { get; set; }
        public List<string> EntryIDs_lst { get; set; }
        public List<string> EntryIDs_Text_lst { get; set; }
        public List<int> EntryDataRowPos_lst { get; set; }


        private bool no_delete_verif_wanted = false;
        public bool No_Delete_Verif_Wanted
        {
            get { return no_delete_verif_wanted; }
            set
            {
                if (value != no_delete_verif_wanted)
                {
                    no_delete_verif_wanted = value;
                    Debug.WriteLine("No_Delete_Verif_Wanted set to: " + No_Delete_Verif_Wanted);
                }
            }
        }

        private bool no_applyproperties_verif_wanted = false;
        public bool No_ApplyProperties_Verif_Wanted
        {
            get { return no_applyproperties_verif_wanted; }
            set
            {
                if (value != no_applyproperties_verif_wanted)
                {
                    no_applyproperties_verif_wanted = value;
                    Debug.WriteLine("No_ApplyProperties_Verif_Wanted set to: " + No_ApplyProperties_Verif_Wanted);
                }
            }
        }

        public List<string> UsedColoursHex { get; set; }



        private string colourhex_picker_ = MasterClass.glo_default_titleblock_colour_hex;
        public string ColourHex_Picker
        {
            get { return colourhex_picker_; }
            set
            {
                if (value != colourhex_picker_)
                {
                    colourhex_picker_ = value;
                    Debug.WriteLine("ColourPicker changed to " + ColourHex_Picker);
                    OnPropertyChanged();
                }
            }
        }



        private bool glo_property_pane_open_ = false;
        public bool Glo_PropertyPaneOpen
        {
            get { return glo_property_pane_open_; }
            set
            {
                if (glo_property_pane_open_ != value)
                {
                    glo_property_pane_open_ = value;

                    if (glo_property_pane_open_)
                    {
                        MasterClass.Cur_MainPage.OpenPropertiesMethod();
                    }
                    else
                    {
                        MasterClass.Cur_MainPage.ClosePropertiesMethod();
                    }
                }

            }
        }

        //this keeps track of if the add new entry window is open
        private bool glo_add_new_window_open_ = false;
        public bool Glo_AddNewWindowOpen
        {
            get { return glo_add_new_window_open_; }
            set
            {
                if (glo_add_new_window_open_ != value)
                {
                    glo_add_new_window_open_ = value;
                }

            }
        }



        public ProjectData()
        {
            //this happens when the class is loaded

            Debug.WriteLine("ProjectData instantiated");
        }
        public ProjectData(string file_path_str, string proj_file_name_str)
        {
            //this happens when the class is loaded with two constructors

            Debug.WriteLine("ProjectData instantiated     Loading Project Data...");

            LoadProjectData(file_path_str, proj_file_name_str);
        }




        //these methods deal with loading of saved project data

        public async void LoadProjectData(string path, string project_name)
        {
            Debug.WriteLine("LoadProjectData");
            //show loading dialogue
            //TODO

            //loads the save files
            await LoadProjectDataMethod(path, project_name);

            MasterClass.Cur_MainPage.UpdateAllComponents();

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



            //PrintList("list_projectdata", list_projectdata);



            //will just show some more Debug troubleshooting if wanted
            bool show_all_troubleshooting = false;



            GetSavedDataFromList(list_projectdata, show_all_troubleshooting);
            ProjectLoaded = true;
            MasterClass.Cur_NavigationViewMain.UpdateButtonBinding();

            UpdateCalendarLogicDisplay(false);

            PrintProjectDataValues(false, false, false);

        }
        private void GetSavedDataFromList(List<string> list_projectdata, bool show_troubleshooting)
        {
            Debug.WriteLine("\nGetSavedDataFromList");

            //this is the return ProjectData
            //ProjectData return_ProjectData = new ProjectData();

            //these are the lists of ColumnData and EntryData that will be put into return_project_data later
            List<ColumnData> ColumnData_lst = new List<ColumnData>();
            List<EntryData> EntryData_lst = new List<EntryData>();

            string project_name;


            //this is checking if the file is formatted correctly, and extracting the project name from the first line
            string first_line = list_projectdata[0];

            if (first_line.StartsWith("project-name="))
            {
                string[] first_line_split = first_line.Split('=');
                project_name = first_line_split[1];
                if (show_troubleshooting) { Debug.WriteLine("project-name= " + project_name); }
            }
            else
            {
                Debug.WriteLine("Error: current reading file does not begin with 'project-name='. exiting with empty list");
                return; //TODO
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
                        GetProjectDataFromLine(project_name, cur_line);

                        //prints the values of the new EntryData
                        if (show_troubleshooting) { PrintProjectDataValues(false, false, false); }

                        break;

                    //for column data
                    case "col-data":

                        //creating a new ColumnData to put in information
                        ColumnData cur_column_data = new ColumnData(project_name, cur_line);

                        //prints the values of the new ColumnData
                        if (show_troubleshooting) { cur_column_data.PrintColumnDataValues(); }

                        //adding the ColumnData to the ColumnData list
                        ColumnData_lst.Add(cur_column_data);

                        break;

                    //for entry data
                    case "ent-data":

                        //creating a new EntryData to put in information with the save data
                        EntryData cur_entry_data = new EntryData(project_name, cur_line);

                        //prints the values of the new EntryData
                        if (show_troubleshooting) { cur_entry_data.PrintEntryDataValues(); }

                        //adding the EntryData to the EntryData list
                        EntryData_lst.Add(cur_entry_data);

                        break;

                    default:
                        break;
                }


            }

            //adding the EntryData and ColumnData

            ColumnData_lst_param = ColumnData_lst;
            EntryData_lst_param = EntryData_lst;

            CreateEntryIDsList();
            CreateColIDsList();

            List<string> entryDatas_validity_errors = CheckEntryDataValuesValid(false); //the bool is for detailed debugging
            if (entryDatas_validity_errors.Count == 1 && entryDatas_validity_errors[0] == "no errors")
            {
                //all good
            }
            else
            {
                //not good
                //TODO how to handle these erros on entrydata properties invalid
            }


        }
        public void GetProjectDataFromLine(string project_name, string cur_line)
        {
            Debug.WriteLine("GetProjectDataFromLine");

            //putting in the project name already extracted
            ProjectName = project_name;

            //now to extract the other data from the supplied entry data line string




            //keeps track of all the data types already put into the EntryData, useful to find errors like two titles, etc.
            List<string> inputted_data_types = new List<string>();

            //splits up all the data of the entry. this list will be of the form {"pro-data", "column-type=custom", ...}
            string[] cur_line_split = cur_line.Split(';');


            //now we will go through every data pair to save it into the EntryData
            foreach (string cur_data_pair in cur_line_split)
            {
                //Debug.WriteLine("cur_data_pair: " + cur_data_pair);

                //the line indicator is "pro-data" and will be skipped here
                if (cur_data_pair == "pro-data") { continue; }


                //this will be of the form {"column-type", "custom"}
                string[] data_pair_split = cur_data_pair.Split('=');

                string property_name = data_pair_split[0];
                string property_value = data_pair_split[1];


                //won't continue if the property type has already been saved
                if (inputted_data_types.Contains(property_name))
                {
                    Debug.WriteLine("EntryData already contains the property: " + property_name + ". Skipping data pair.");
                    continue;
                }

                //if the current property value was blank, we skip
                if (property_value == "") { continue; }


                //a switch based on the name of the property in the data pair
                switch (property_name)
                {
                    case "column-type":
                        ColumnType = property_value;
                        inputted_data_types.Add(property_name);
                        break;

                    case "header-width":

                        //here we need to deal with three cases:    3*    Auto    160
                        if (property_value.EndsWith("*"))
                        {
                            int star_width = Convert.ToInt32(property_value.Trim('*'));
                            HeaderWidth = new GridLength(star_width, GridUnitType.Star);
                        }
                        else if (property_value.ToLower().Contains("auto"))
                        {
                            HeaderWidth = new GridLength(1, GridUnitType.Auto);
                        }
                        else
                        {
                            if (int.TryParse(property_value, out int integer_width))
                                HeaderWidth = new GridLength(integer_width, GridUnitType.Pixel);
                        }

                        break;

                    default:
                        break;

                }
            }

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
            Debug.WriteLine("Local Storage Folder Path: " + LocalFolder.Path);

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


            string title_line = string.Format("project-name={0}", ProjectName);
            save_list.Add(title_line);
            save_list.Add(blank_line);


            save_list.Add(sep_line);
            save_list.Add(blank_line);


            string project_data_header = "project-data_";
            save_list.Add(project_data_header);
            save_list.Add(blank_line);

            string project_data_line = CreateProjectDataSaveLine(data_separator);
            save_list.Add(project_data_line);
            save_list.Add(blank_line);


            save_list.Add(sep_line);
            save_list.Add(blank_line);


            string column_data_header = "column-data_";
            save_list.Add(column_data_header);
            save_list.Add(blank_line);

            foreach (ColumnData cur_columnData in ColumnData_lst_param)
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

            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                string cur_entry_data_line = cur_entryData.CreateEntryDataSaveLine(data_separator);
                save_list.Add(cur_entry_data_line);
            }

            return save_list;
        }






        //these methods deal with the calculation of calendar logic


        public void UpdateCalendarLogicDisplay(bool detailed_debugging)
        {
            //this method will perform order calculations and then update the calendar display

            Debug.WriteLine("\nUpdateCalendarLogicDisplay");
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



            //will first update the layout of the calendar
            //MasterClass.Cur_MainPage.UpdateCalendarLayout();

            //now to update the calendar display
            //MasterClass.Cur_MainPage.UpdateCalendarDisplay();


        }
        public bool CalculateCalendarLogic(bool detailed_debugging)
        {
            //this method will go through all the EntryDatas and ColumnDatas to figure out how all pre/co reqs will work in available columns

            Debug.WriteLine("\nCalculateCalendarLogic");

            //this bool is to say if it was successful in finding a solution
            bool converged = false;

            //this will reset all the EntryData.ActualColID values
            ClearActualColIDs();

            //this will get the ordered list of colID's
            List<string> colID_order_lst = CreateColID_Order_lst();
            if (detailed_debugging) { MasterClass.PrintList("colID_order_lst", colID_order_lst); }


            //this is a hardcopy of the entryID list
            //it will have items removed as they get used
            List<string> to_place_entryID_lst = MasterClass.CreateDeepCopyOfList_string(EntryIDs_lst);
            int to_place_count = to_place_entryID_lst.Count();


            //this is a list of all placed entryIds
            List<string> placed_entryID_lst = new List<string>();

            if (detailed_debugging)
            {
                MasterClass.PrintList("to_place_entryID_lst - Original", to_place_entryID_lst);
                MasterClass.PrintList("placed_entryID_lst - Original", placed_entryID_lst);
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
                    EntryData cur_entryData = GetEntryDataFromEntryID(cur_entryID);


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

                        if (detailed_debugging) { MasterClass.PrintList("cur_entryData.PrereqEntryIDs", cur_entryData.PrereqEntryIDs); }


                        //this is the list of col positions of all the prereqs
                        List<int> prereq_positions = new List<int>();
                        foreach (string cur_prereq_entryID in cur_entryData.PrereqEntryIDs)
                        {
                            EntryData cur_prereq_entryData = GetEntryDataFromEntryID(cur_prereq_entryID);
                            int cur_prereq_col_position = GetColumnPositionFromColID(cur_prereq_entryData.ActualColID);

                            prereq_positions.Add(cur_prereq_col_position);
                        }
                        if (detailed_debugging) { MasterClass.PrintList_int("prereq_positions", prereq_positions); }

                        //this is the default earliest, will remain 0 if there were no prereqs
                        int earliest_col_pos_after_prereqs = 0;
                        if (prereq_positions.Count > 0)
                        {
                            //we will get in here if there were any prereqs
                            earliest_col_pos_after_prereqs = prereq_positions.Max() + 1;
                        }
                        if (detailed_debugging) { Debug.WriteLine("earliest_col_pos_after_prereqs = " + earliest_col_pos_after_prereqs); }

                        if (detailed_debugging) { MasterClass.PrintList("cur_ordered_avail_colID", cur_ordered_avail_colID); }

                        List<int> cur_ordered_avail_col_pos = CalculateAvailColPos_InOrder(cur_ordered_avail_colID);
                        if (detailed_debugging) { MasterClass.PrintList_int("cur_ordered_avail_col_pos", cur_ordered_avail_col_pos); }

                        //this will trim the lowest positions from the list until it's at least  earliest_col_pos_after_prereqs
                        while (cur_ordered_avail_col_pos.Count > 0 && cur_ordered_avail_col_pos[0] < earliest_col_pos_after_prereqs)
                        {
                            cur_ordered_avail_col_pos.RemoveAt(0);
                        }
                        if (detailed_debugging) { MasterClass.PrintList_int("cur_ordered_avail_col_pos", cur_ordered_avail_col_pos); }



                        //setting it to be the first colID
                        //cur_entryData.ActualColID = cur_ordered_avail_colID[0];
                        cur_entryData.ActualColID = GetColumnIDFromPosition(cur_ordered_avail_col_pos[0]);
                        if (detailed_debugging) { Debug.WriteLine("cur_entryData.ActualColID = " + cur_entryData.ActualColID); }

                        ColumnData associated_columnData = GetColumnDataFromColID(cur_entryData.ActualColID);
                        cur_entryData.ColPosition = associated_columnData.ColPosition;

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
                    MasterClass.PrintList("to_place_entryID_lst", to_place_entryID_lst);
                    MasterClass.PrintList("placed_entryID_lst", placed_entryID_lst);
                }

                //we will now do another pass through, placing the next round of entryDatas

            }


            //if we get here, then we got through the entire to_place list
            converged = true;

            return converged;




            //TODO on load make sure that all pre/co reqs actually have corresponding EntryData







        }
        public List<int> CalculateAvailColPos_InOrder(List<string> cur_ordered_avail_colID)
        {
            //this will receive an already ordered list of colID's, this changes it to col positions

            List<int> return_list = new List<int>();

            foreach (string cur_colID in cur_ordered_avail_colID)
            {
                ColumnData cur_columnData = GetColumnDataFromColID(cur_colID);
                return_list.Add(cur_columnData.ColPosition);
            }

            return return_list;
        }
        public List<string> CalculateAvailColID_InOrder(List<string> avail_col_IDs, List<string> ordered_col_IDs)
        {
            //this method will take the available col ID list, and order it according to the ordered col ID list

            //just an error check
            if (avail_col_IDs.Count < 1 || ordered_col_IDs.Count < 1) { return new List<string>(); }

            //the return list
            //the intersection command takes the ordered_col_IDs list, and trims out any value that isn't in avail_col_IDs
            List<string> ordered_avail_col_IDs = ordered_col_IDs.Intersect(avail_col_IDs).ToList();


            return ordered_avail_col_IDs;
        }





        public void ChangeSelectedEntry()
        {

            //will be triggered if  Glo_Selected_EntryID  is changed


            if (Glo_Selected_EntryID == "")
            {
                ClearSelection();
                return;
            }


            //sets the is_selected flag of the current selected entryData to false
            Glo_Selected_EntryData.Is_Selected = false;


            //setting the new selected entryData
            Glo_Selected_EntryData = GetEntryDataFromEntryID(Glo_Selected_EntryID);
            Glo_Selected_EntryData.Is_Selected = true;

            Debug.WriteLine("The new selection:");
            Glo_Selected_EntryData.PrintEntryDataValues();
        }
        public void UnselectEntryData()
        {
            Glo_Selected_EntryID = "";
        }
        public void ClearSelection() 
        {

            //this method will unselect whatever is the currently selected titleblock

            //unselects the currently selected entrydata
            Glo_Selected_EntryData.Is_Selected = false;

            //reset the global flag
            Glo_Selected_EntryID = "";

            //reset the global EntryData
            Glo_Selected_EntryData = new EntryData();

        }




        //these methods move entries up or down

        public void MoveEntryByIndex(int index_change)
        {
            //this method will move entry titleblocks and calendar boxes up or down by a specific amount

            string entryID_to_move = Glo_Selected_EntryID;

            Debug.WriteLine("\nMoveEntryByIndex     entryID_to_move = '" +
                entryID_to_move + "'     index_change = " + index_change);



            //this is the maximum index that can happen
            int max_index = EntryData_lst_param.Count - 1;



            //this makes a list of all the current positions
            CreateEntryDataListPositionList();


            //just for verification...
            //glo_ProjectData.PrintProjectDataValues(false, true, true);


            //this gets the EntryData we want
            EntryData entryData_to_move = GetEntryDataFromEntryID(entryID_to_move);

            if (entryData_to_move == null)
            {
                Debug.WriteLine("ERROR: GetEntryDataFromEntryID failed");
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
            foreach (EntryData cur_entryData in EntryData_lst_param)
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


            //updates the row position list
            CreateEntryDataListPositionList();


            //just for verification...
            //glo_ProjectData.PrintProjectDataValues(false, true, true);



            //now that the global save lists have been updated, must update the visual positions of the controls


            //this will refresh all the titleblocks to be in their new position
            //FixBlockInfo(false, false, false, true, false);




            //this will enable/disable the move up and down buttons if needed
            MasterClass.Cur_MainPage.EnableTitleBlockButtons();


            Debug.WriteLine("");

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

        public void CreateNewAddNewEntry()
        {
            Glo_AddNewEntry = new AddNewEntry
            {
                Glo_ProjectData_Reference = this,
                Glo_MainPage_Reference = MasterClass.Cur_MainPage
            };

        }
        public void SelectAddNewEntry()
        {
            Glo_AddNewEntry.Is_Selected = !Glo_AddNewEntry.Is_Selected;
        }


        public void AddEntryMethod()
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
            ApplyAddNewEntryDetailsToEntryData(out EntryData new_entryData);


            //final data that needs to be figured out is the row_position

            //creating the master list of positions
            CreateEntryDataListPositionList();

            //TODO add functionality to add at another index

            //this is the new end of the list
            int new_row_pos = EntryDataRowPos_lst.Max() + 1;

            new_entryData.RowPosition = new_row_pos;



            //the avail_col_ids list must also contain values or else it will not work. If the user didn't set these, assume it's allowed in all cols
            if (new_entryData.AvailColIDs.Count == 0)
            {
                new_entryData.AvailColIDs = MasterClass.CreateDeepCopyOfList_string(ColIDs_lst);
            }



            //this will put the new entrydata into the lists and calendar display
            InsertNewEntryData(new_entryData);
        }
        private void ApplyAddNewEntryDetailsToEntryData(out EntryData new_entryData)
        {
            //this will use the ref and pointers to edit the new_entryData that's being created in the previous method

            new_entryData = new EntryData();

            //gets the new changed values from the changed properties
            Glo_AddNewEntry.GetFieldData(
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
                new_entryData.ColourHex = MasterClass.glo_default_titleblock_colour_hex;
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
            //this method will take the inserted entrydata and will put it into the master list in  Glo_ProjectData
            //then will put it into the titleblocks
            //and will do a logic run for the calendar



            //this puts it into the master list, and returns the index in the master list for easy indexing
            AddNewEntryData(inserted_entryData);


            //and now will update the calendar logic and display
            UpdateCalendarLogicDisplay(true);

            //puts them into the UI
            MasterClass.Cur_MainPage.InsertNewEntryBorders();

            Glo_AddNewEntry = null;
        }








        //these methods deal with deleting of entries

        public async void DeleteEntryMethod()
        {
            //this method is called to delete the currently selected entryData

            Debug.WriteLine("DeleteEntryMethod");





            string selected_entryID_to_delete = glo_selected_entryID;

            //ensures that this is a valid entryID

            if (selected_entryID_to_delete == "")
            {
                Debug.WriteLine("ERROR: The entryID is blank");
                return;
            }
            if (!EntryIDs_lst.Contains(selected_entryID_to_delete))
            {
                Debug.WriteLine("ERROR: The entryID  '" + selected_entryID_to_delete
                    + "'  does not exist in  Glo_ProjectData.EntryIDs_lst");
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
            DeleteEntryDataAtIndex(selected_entryID_to_delete);

            //removes it in the UI
            MasterClass.Cur_MainPage.RemoveEntryBorders();


        }
        public async Task<bool> VerifyDelete(string entryID_to_be_deleted)
        {

            //if we are asked not to ask for verification, we return true
            if (No_Delete_Verif_Wanted) { return true; }


            //otherwise, we have to open a verification dialog box


            EntryData entryData_to_be_deleted = GetEntryDataFromEntryID(entryID_to_be_deleted);
            return await MasterClass.Cur_MainPage.DeleteConfirmationDialogue(entryData_to_be_deleted);


        }







        //these methods deal with the property pane. much of its functionality is data binding with ChangedProperties.cs

        public void PrepareChangedProperties()
        {
            Glo_ChangedProperties = new ChangedProperties();
            Glo_ChangedProperties.LoadOriginalEntryData(Glo_Selected_EntryData);

            Glo_ChangedProperties.Glo_ProjectData_Reference = this;
            Glo_ChangedProperties.Glo_MainPage_Reference = MasterClass.Cur_MainPage;

        }
        public void PrintPropertiesEntryDatas()
        {
            Glo_ChangedProperties.PrintProperties(glo_selected_entryData);
        }
        public bool ApplyPropertiesMethod(bool changing_selection)
        {
            Debug.WriteLine("\nApplyPropertiesMethod");


            //apply the new properties
            bool proceed_with_apply = false;


            //if the setting for requiring verification is enabled...
            //if the bool is TRUE, then we do NOT want verification
            if (!No_ApplyProperties_Verif_Wanted)
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


                proceed_with_apply = true; //TODO get rid of this override
            }

            //if the setting isn't enabled, then just proceed
            else
            {
                proceed_with_apply = true;
            }



            //exit if not proceeding
            if (!proceed_with_apply) { return false; }


            //if proceeding...
            Debug.WriteLine("\nProceeding with apply");


            //the entryID of the EntryData we are changing
            string applying_entryID = Glo_ChangedProperties.EntryID_Old;
            Debug.WriteLine("applying_entryID = '" + applying_entryID + "'");

            //the master EntryData that we are changing
            EntryData master_entryData = GetEntryDataFromEntryID(applying_entryID);


            ApplyNewPropertiesToEntryData(master_entryData);

            Glo_ChangedProperties = null;

            return true;
        }
        public void ApplyNewPropertiesToEntryData(EntryData master_entryData)
        {
            //thanks to pointers, this will update the data in the actual master_entryData


            //gets the new changed values from the changed properties
            Glo_ChangedProperties.GetFieldData(
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

            if (prereq_entryIDs != null && prereq_entryIDs.Count != 0)
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

        public void CreateUsedColourList()
        {
            Debug.WriteLine("CreateUsedColourList");

            //thiw will make a list of all the used colours
            List<string> return_list = new List<string>();


            foreach (EntryData cur_entry in EntryData_lst_param)
            {
                string cur_hex = cur_entry.ColourHex;

                if (!return_list.Contains(cur_hex))
                {
                    return_list.Add(cur_hex);
                }
            }

            UsedColoursHex = return_list;
        }
        public void GetPathForColourPickerBinding(out string binding_path)
        {

            if (Glo_AddNewWindowOpen)
            {
                binding_path = "addnew";
            }
            else if (Glo_PropertyPaneOpen)
            {
                binding_path = "properties";
            }
            else
            {
                binding_path = "";
            }


        }
        public Windows.UI.Color GetColourForColourPicker()
        {
            string situation = "";
            if (Glo_AddNewWindowOpen)
            {
                situation = "addnew";
            }
            else //if (glo_property_pane_open_)
            {
                situation = "properties";
            }


            if (situation.Length == 0)
            {
                Debug.WriteLine("GetColourForColourPicker");
                Debug.WriteLine("ERROR: PropertiesPane and AddNewEntry both closed");
                return new Windows.UI.Color();
            }

            string colour_hex = "";

            switch (situation)
            {
                case "properties":
                    colour_hex = Glo_ChangedProperties.ColourHex_New;
                    break;

                case "addnew":
                    colour_hex = Glo_AddNewEntry.ColourHex_Add;
                    break;
            }

            Windows.UI.Color return_colour = MasterClass.GetSolidColorBrushFromHex(colour_hex).Color;

            return return_colour;

        }
        public void InitiateColourPicker()
        {

            string set_colour_hex = "";

            if (Glo_AddNewWindowOpen)
            {
                set_colour_hex = Glo_AddNewEntry.ColourHex_Add;
            }
            else if (Glo_PropertyPaneOpen)
            {
                set_colour_hex = Glo_ChangedProperties.ColourHex_New;
            }


            if (set_colour_hex.Length == 0)
            {
                Debug.WriteLine("InitiateColourPicker");
                Debug.WriteLine("ERROR: PropertiesPane and AddNewEntry both closed");
                return;
            }


            ColourHex_Picker = set_colour_hex;
        }
        public void ConfirmColour()
        {

            //glo_CalculatedData.ColourPickerChosenHex = chosen_colour_hex;



            //now that we have the colour hex, we need to choose where to put it
            //it will be stored in the tag of the button that called the flyout

            if (Glo_AddNewWindowOpen)
            {
                //this will happen if the add new entry window is open

                Glo_AddNewEntry.ColourHex_Add = ColourHex_Picker;

                //ChangeColour_AddNewEntry(chosen_colour_hex);

            }
            else if (Glo_PropertyPaneOpen)
            {
                //this will happen if the properties pane is open and the add new entry window isn't

                Glo_ChangedProperties.ColourHex_New = ColourHex_Picker;

                //ChangeColour_Properties(chosen_colour_hex);

            }


        }










        public void PrintProjectDataValues(bool show_columndata, bool show_entrydata, bool show_summarized_lists)
        {
            Debug.WriteLine("\nProjectData class paramters:");


            List<string> param_names = new List<string> { "ProjectName", "ColumnType", "HeaderWidth" };

            List<string> param_vals = new List<string>();

            if (ProjectName != null) { param_vals.Add(ProjectName.ToString()); } else { param_vals.Add(""); }
            if (ColumnType != null) { param_vals.Add(ColumnType.ToString()); } else { param_vals.Add(""); }
            if (HeaderWidth != null) { param_vals.Add(HeaderWidth.ToString()); } else { param_vals.Add(""); }


            for (int i = 0; i < param_names.Count; i++)
            {
                Debug.WriteLine(String.Format("    {0,-17} = {1}", param_names[i], param_vals[i]));
            }
            Debug.WriteLine("");



            if (ColumnData_lst_param != null)
            {
                Debug.WriteLine("Number of ColumnData entries = " + ColumnData_lst_param.Count());
            }
            if (EntryData_lst_param != null)
            {
                Debug.WriteLine("Number of EntryData entries = " + EntryData_lst_param.Count());
            }


            if (show_summarized_lists)
            {
                if (ColIDs_lst != null)
                {
                    Debug.Write("ColID's:");

                    foreach (string cur_colID in ColIDs_lst)
                    {
                        Debug.Write(" '" + cur_colID + "'");
                    }
                    Debug.Write("\n\n");

                }
                if (EntryIDs_lst != null)
                {
                    Debug.Write("EntryID's:");

                    foreach (string cur_entryID in EntryIDs_lst)
                    {
                        Debug.Write(" '" + cur_entryID + "'");
                    }
                    Debug.Write("\n\n");
                }
                if (EntryDataRowPos_lst != null)
                {
                    Debug.Write("Entry Data Row Positions:");

                    foreach (int cur_row_pos in EntryDataRowPos_lst)
                    {
                        Debug.Write(" '" + cur_row_pos + "'");
                    }
                    Debug.Write("\n\n");
                }
                if (EntryIDs_lst != null)
                {
                    Debug.Write("EntryID trunc_titles:");

                    foreach (string cur_entryData_trunc_title in EntryIDs_Text_lst)
                    {
                        Debug.Write(" '" + cur_entryData_trunc_title + "'");
                    }
                    Debug.Write("\n\n");
                }
            }



            if (show_columndata)
            {
                Debug.WriteLine("\nStart of ColumnData entries\n==============================");
                foreach (ColumnData cur_columndata in ColumnData_lst_param) { cur_columndata.PrintColumnDataValues(); }
                Debug.WriteLine("==============================\nEnd of ColumnData entries\n");
            }

            if (show_entrydata)
            {
                Debug.WriteLine("Start of EntryData entries\n==============================");
                foreach (EntryData cur_entrydata in EntryData_lst_param) { cur_entrydata.PrintEntryDataValues(); }
                Debug.WriteLine("==============================\nEnd of EntryData entries\n");
            }


        }


        public void CreateEntryIDsList()
        {
            //making the list of entryID's
            List<string> entryID_lst = new List<string>();

            //this is the list of all unique entryID text portions
            List<string> entryID_text_lst = new List<string>();

            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                string cur_entryID = cur_entryData.EntryID;

                entryID_lst.Add(cur_entryID);

                string cur_entryID_text = cur_entryID.Split('‖')[0];

                if (!entryID_text_lst.Contains(cur_entryID_text))
                {
                    entryID_text_lst.Add(cur_entryID_text);
                }
            }

            EntryIDs_lst = entryID_lst;
            EntryIDs_Text_lst = entryID_text_lst;


            if (EntryIDs_lst.Count != EntryIDs_lst.Distinct().Count())
            {
                Debug.WriteLine("ERROR: there are duplicate entryIDs in the EntryData's");

                //TODO: this will break the program
                int crash_int = Convert.ToInt32("asdf");
            }
        }

        public void CreateEntryDataListPositionList()
        {
            //making the list of row_position's
            List<int> row_position_lst = new List<int>();
            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                row_position_lst.Add(Convert.ToInt32(cur_entryData.RowPosition));
            }


            //just checking to see if there are any duplicate positions

            if (row_position_lst.Count != row_position_lst.Distinct().Count())
            {
                Debug.WriteLine("ERROR: there are duplicate row positions in the EntryData's");
                EntryDataRowPos_lst = null;
                return;
            }

            EntryDataRowPos_lst = row_position_lst;
        }


        public void CreateColIDsList()
        {
            //making the list of colID's
            List<string> colID_lst = new List<string>();
            foreach (ColumnData cur_columnData in ColumnData_lst_param)
            {
                colID_lst.Add(cur_columnData.ColID);
            }
            ColIDs_lst = colID_lst;


            if (ColIDs_lst.Count != ColIDs_lst.Distinct().Count())
            {
                Debug.WriteLine("ERROR: there are duplicate columnIDs in the ColumnData's");

                //TODO: this will break the program
                int crash_int = Convert.ToInt32("asdf");
            }
        }


        public List<string> CreateColID_Order_lst()
        {
            //this is how you instantiate a list of a certain size. apparently should be using arrays... but whatever
            List<string> return_list = new List<string>(new string[ColumnData_lst_param.Count()]);

            //creates a list of colID's in order
            foreach (ColumnData cur_ColumnData in ColumnData_lst_param)
            {
                int cur_col_pos = cur_ColumnData.ColPosition;
                string cur_col_id = cur_ColumnData.ColID;

                return_list[cur_col_pos] = cur_col_id;
            }

            return return_list;
        }



        public List<string> CheckEntryDataValuesValid(bool detailed_debugging)
        {
            //this method will go through every EntryData and ensure that:
            //   1) its pre/coreqs actually exist in EntryIDs_lst
            //   2) its set and available colIDs actually exist

            Debug.WriteLine("CheckEntryDataValuesValid");

            //this will return any errors, if any
            List<string> return_lst = new List<string>();



            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                if (detailed_debugging)
                {
                    Debug.WriteLine("\nEntryID = " + cur_entryData.EntryID);
                }

                //checking the prereqs
                foreach (string cur_prereq in cur_entryData.PrereqEntryIDs)
                {
                    string prereq_exists;

                    //if the current prereq does exist in EntryIDs_lst
                    if (EntryIDs_lst.Contains(cur_prereq))
                    {
                        prereq_exists = "does exist";
                    }

                    //if it doesn't exist...
                    else
                    {
                        return_lst.Add(string.Format("prereq_does_not_exist;{0};{1}", cur_entryData, cur_prereq));
                        prereq_exists = "does NOT exist";
                    }

                    if (detailed_debugging)
                    {
                        Debug.WriteLine(string.Format("    {0, -12}  {1, -12}: {2}", "Prereq", cur_prereq, prereq_exists));
                    }
                }


                //checking the coreqs
                foreach (string cur_coreq in cur_entryData.PrereqEntryIDs)
                {
                    string coreq_exists;

                    //if the current coreq does exist in EntryIDs_lst
                    if (EntryIDs_lst.Contains(cur_coreq))
                    {
                        coreq_exists = "does exist";
                    }

                    //if it doesn't exist...
                    else
                    {
                        return_lst.Add(string.Format("coreq_does_not_exist;{0};{1}", cur_entryData, cur_coreq));
                        coreq_exists = "does NOT exist";
                    }

                    if (detailed_debugging)
                    {
                        Debug.WriteLine(string.Format("    {0, -12}  {1, -12}: {2}", "Coreq", cur_coreq, coreq_exists));
                    }
                }


                //checking the avail colIDs
                foreach (string cur_avail_colID in cur_entryData.AvailColIDs)
                {
                    string avail_colID_exists;

                    //if the current avail colID does exist in ColIDs_lst
                    if (ColIDs_lst.Contains(cur_avail_colID))
                    {
                        avail_colID_exists = "does exist";
                    }

                    //if it doesn't exist...
                    else
                    {
                        return_lst.Add(string.Format("avail_colID_does_not_exist;{0};{1}", cur_entryData, cur_avail_colID));
                        avail_colID_exists = "does NOT exist";
                    }

                    if (detailed_debugging)
                    {
                        Debug.WriteLine(string.Format("    {0, -12}  {1, -12}: {2}", "Avail ColID", cur_avail_colID, avail_colID_exists));
                    }
                }


                //checking the set colID
                if (true)
                {
                    string cur_set_colID = cur_entryData.SetColID;
                    string set_colID_exists;

                    //if the current set colID does exist in ColIDs_lst
                    if (ColIDs_lst.Contains(cur_set_colID))
                    {
                        set_colID_exists = "does exist";
                    }

                    //if it doesn't exist...
                    else
                    {
                        return_lst.Add(string.Format("set_colID_does_not_exist;{0};{1}", cur_entryData, cur_set_colID));
                        set_colID_exists = "does NOT exist";
                    }

                    if (detailed_debugging)
                    {
                        Debug.WriteLine(string.Format("    {0, -12}  {1, -12}: {2}", "Set ColID", cur_set_colID, set_colID_exists));
                    }
                }


            }








            //if there were no errors, state it
            if (return_lst.Count == 0) { return_lst.Add("no_errors"); }

            return return_lst;
        }

        public void ClearActualColIDs()
        {
            //this will clear the ActualColID properties in the EntryData's

            Debug.WriteLine("ClearActualColIDs");

            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                cur_entryData.ActualColID = "";
            }
        }





        public EntryData GetEntryDataFromEntryID(string given_entryID)
        {

            //if the entryID that's given does not exist in the entryID_lst_param we exit
            if (!EntryIDs_lst.Contains(given_entryID))
            {
                Debug.WriteLine("ERROR: The entryID given, '" + given_entryID + "', does not exist in EntryIDs_lst");
                return null;
            }

            //this is the EntryData that's being requested, at the same index that its entryID was found in the entryID list
            EntryData return_EntryData = EntryData_lst_param[EntryIDs_lst.IndexOf(given_entryID)];

            //juuuuust in case this didn't work...
            if (return_EntryData.EntryID != given_entryID)
            {
                Debug.WriteLine("ERROR: The entryID given, '" + given_entryID + "', does not match the entryID found in the indexed EntryData, '" + return_EntryData.EntryID + "'");
                return null;
            }


            return return_EntryData;
        }
        public int GetRowPositionFromEntryID(string given_entryID)
        {
            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                string cur_entryID = cur_entryData.EntryID;

                if (cur_entryID == given_entryID)
                {
                    return cur_entryData.RowPosition;
                }
            }

            return -1;
        }
        public Border GetTitleBlockFromEntryID(string given_entryID)
        {
            return GetEntryDataFromEntryID(given_entryID).TitleBlock;
        }
        public Border GetCalendarBlockFromEntryID(string given_entryID)
        {
            return GetEntryDataFromEntryID(given_entryID).CalendarBlock;
        }

        public int GetColumnPositionFromColID(string sent_colID)
        {
            //first gets the referred columndata
            ColumnData cur_columnData = GetColumnDataFromColID(sent_colID);
            return cur_columnData.ColPosition;
        }
        public ColumnData GetColumnDataFromColID(string given_colID)
        {

            //if the colID that's given does not exist in the ColumnData_lst_param we exit
            if (!ColIDs_lst.Contains(given_colID))
            {
                Debug.WriteLine("ERROR: The colID given, '" + given_colID + "', does not exist in ColIDs_lst");
                return null;
            }

            //this is the ColumnData that's being requested, at the same index that its colID was found in the colID list
            ColumnData return_ColumnData = ColumnData_lst_param[ColIDs_lst.IndexOf(given_colID)];

            //juuuuust in case this didn't work...
            if (return_ColumnData.ColID != given_colID)
            {
                Debug.WriteLine("ERROR: The colID given, '" + given_colID + "', does not match the colID found in the indexed ColumnData, '" + return_ColumnData.ColID + "'");
                return null;
            }


            return return_ColumnData;
        }


        public EntryData GetEntryDataFromListPosition(int given_row_pos)
        {

            //if the row_pos that's given does not exist in the EntryDataRowPos_lst we exit
            if (!EntryDataRowPos_lst.Contains(given_row_pos))
            {
                Debug.WriteLine("ERROR: The row_pos given, '" + given_row_pos + "', does not exist in EntryDataRowPos_lst");
                return null;
            }

            //this is the EntryData that's being requested, at the same index that its entryID was found in the entryID list
            EntryData return_EntryData = EntryData_lst_param[EntryDataRowPos_lst.IndexOf(given_row_pos)];

            //juuuuust in case this didn't work...
            if (return_EntryData.RowPosition != given_row_pos)
            {
                Debug.WriteLine("ERROR: The row_pos given, '" + given_row_pos + "', does not match the row_pos found in the indexed EntryData, '" + return_EntryData.RowPosition + "'");
                return null;
            }


            return return_EntryData;
        }

        public string GetColumnIDFromPosition(int sent_col_pos)
        {

            foreach (ColumnData cur_columnData in ColumnData_lst_param)
            {
                if (cur_columnData.ColPosition != sent_col_pos) { continue; }

                return cur_columnData.ColID;
            }

            return "";
        }



        public string CreateProjectDataSaveLine(string data_separator)
        {
            //we want
            //  pro-data;column-type=custom;header-width=160

            Debug.WriteLine("CreateProjectDataSaveLine");

            string return_data_line;
            string data_line_start = "pro-data";


            return_data_line = data_line_start;


            foreach (string cur_data_parameter in Parameter_Names)
            {
                //should be like this
                // Parameter_Names = { "ProjectName", "ColumnType", "HeaderWidth" };
                // Parameter_Save_Names = { "", "column-type", "header-width" };

                //this one is skipped
                if (cur_data_parameter == "ProjectName") { continue; }


                string cur_data_addition = "";

                string cur_data_save_name = Parameter_Save_Names[Parameter_Names.IndexOf(cur_data_parameter)];
                string data_val = "";

                switch (cur_data_parameter)
                {
                    case "ColumnType":
                        data_val = ColumnType.ToString();
                        break;

                    case "HeaderWidth":
                        data_val = HeaderWidth.ToString();
                        break;
                }

                cur_data_addition = string.Format("{0}={1}", cur_data_save_name, data_val);

                return_data_line += data_separator + cur_data_addition;
            }

            return return_data_line;

        }


        public bool IsTitleSubtitleDuplicate(List<string> check_title_subtitle, List<string> exclude_title_subtitle)
        {
            //defaulting the return bool to true
            bool already_exist = true;



            string check_title = check_title_subtitle[0];
            //string check_subtitle = check_title_subtitle[1];

            string exclude_title = "";
            //string exclude_subtitle = "";
            if (exclude_title_subtitle.Count > 0)
            {
                exclude_title = exclude_title_subtitle[0];
                //exclude_subtitle = exclude_title_subtitle[1];
            }



            //in this method, all  null  will just be  ""
            if (check_title == null) { check_title = ""; }
            //if (check_subtitle == null) { check_subtitle = ""; }
            if (exclude_title == null) { exclude_title = ""; }
            //if (exclude_subtitle == null) { exclude_subtitle = ""; }

            bool ignore_title = check_title == exclude_title;
            //bool ignore_subtitle = check_subtitle == exclude_subtitle;

            //exit condition if we are to ignore the title, can't be a duplicate with itself
            if (ignore_title)
            {
                return false;
            }

            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                //keeping track if the title or subtitle are matches
                bool same_title = false;
                //bool same_subtitle = false;

                //getting the title and subtitle
                string cur_title = cur_entryData.Title;
                //string cur_subtitle = cur_entryData.Subtitle;

                //again, no nulls allowed
                if (cur_title == null) { cur_title = ""; }
                //if (cur_subtitle == null) { cur_subtitle = ""; }

                //finding if we exclude these


                //checks if there are matches
                if (check_title == cur_title) { same_title = true; }
                //if (check_subtitle == cur_subtitle) { same_subtitle = true; }


                //the duplicate conditions. one for every title unique, one for every title+subtitle pair unique
                bool duplicate_condition_title = same_title;
                //bool duplicate_condition_both = same_title & same_subtitle;


                //if the duplicate condition is met
                if (duplicate_condition_title)
                {
                    //then there is a duplicate already there
                    return already_exist;
                }
            }



            //if gets here, it found no duplicate
            already_exist = false;
            return already_exist;
        }
        public void IsNewTitleValid(List<string> check_title_subtitle, List<string> exclude_title_subtitle,
            out bool valid_entry, out bool is_duplicate, out bool is_title_exist)
        {
            valid_entry = false;

            string check_title = check_title_subtitle[0];
            string check_subtitle = check_title_subtitle[1];

            Debug.WriteLine("IsNewTitleValid     check_title = '" + check_title + "'     check_subtitle = '" + check_subtitle + "'");


            //checks if the title is null, and if it's not also check that its length is greater than 0
            is_title_exist = ((check_title != null) && (check_title.Length > 0));

            if (!is_title_exist)
            {
                Debug.WriteLine("IsNewTitleValid");
                Debug.WriteLine("  Title does not exist");
            }

            //checks to see if the title and subtitle will give a duplicate entry (based on a duplicate criterion in the method)
            is_duplicate = IsTitleSubtitleDuplicate(check_title_subtitle, exclude_title_subtitle);

            if (is_duplicate)
            {
                Debug.WriteLine("IsNewTitleValid");
                Debug.WriteLine(string.Format("  Duplicate     title='{0}'     subtitle='{1}'", check_title, check_subtitle));
            }

            if (is_title_exist && !is_duplicate)
            {
                valid_entry = true;
                Debug.WriteLine("  valid_entry = " + valid_entry.ToString());
            }

        }

        public int GetMaxDesignationForEntryIDTitle(string given_trunc_title)
        {

            //just to ensure that there are already entryIDs with this trunctated title
            if (!EntryIDs_Text_lst.Contains(given_trunc_title)) { return -1; }

            Debug.WriteLine("GetMaxDesignationForEntryIDTitle    given_trunc_title = '" + given_trunc_title + "'");

            int max_desig = -1;

            foreach (string cur_entryID in EntryIDs_lst)
            {
                //Debug.WriteLine("cur_entryID = '" + cur_entryID + "'");
                string[] split_entryID = cur_entryID.Split('‖');

                //if it was in the proper format, should have 2 indices
                //if it's not, then it shouldn't be checked
                if (split_entryID.Length != 2) { continue; }

                string cur_trunc_title = split_entryID[0];

                if (cur_trunc_title != given_trunc_title) { continue; }

                //we know that this is one of the duplicate trunc titles now

                string cur_desig_str = split_entryID[1];
                Debug.WriteLine(cur_desig_str);


                //will try to convert it to an int, returning if fails
                bool success = int.TryParse(cur_desig_str, out int cur_desig_int);
                if (!success){ return -1; }

                //so now we have a int designation

                if (cur_desig_int > max_desig)
                {
                    max_desig = cur_desig_int;
                }

            }


            Debug.WriteLine(max_desig);
            return max_desig;

        }

        public int AddNewEntryData(EntryData inserted_entryData)
        {
            //this is how a new entrydata is added, and subsequent lists are updated accordingly
            //it will return the index of the newly added EntryData

            //start by putting it into the main list

            EntryData_lst_param.Add(inserted_entryData);


            //updating the entryID lists
            CreateEntryIDsList();

            //updating the row position list
            CreateEntryDataListPositionList();



            return EntryData_lst_param.IndexOf(inserted_entryData);
        }
        public void DeleteEntryDataAtIndex(string delete_entryID)
        {
            //this is how an entrydata is deleted, and subsequent lists are updated accordingly


            //ensuring that the delete_entryID is valid
            if (!EntryIDs_lst.Contains(delete_entryID)) { return; }


            //this is the row position of the entrydata to be deleted
            int deleting_row_pos = GetRowPositionFromEntryID(delete_entryID);


            //now we start by removing it from the main list
            int delete_index = EntryIDs_lst.IndexOf(delete_entryID);
            EntryData_lst_param.RemoveAt(delete_index);


            //updating the entryID lists
            CreateEntryIDsList();

            //updating the row position list
            CreateEntryDataListPositionList();


            //we now need to go back through the remaining EntryData's to update their row positions
            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                if (cur_entryData.RowPosition >= deleting_row_pos)
                {
                    cur_entryData.RowPosition -= 1;
                }
            }






        }

    }
}

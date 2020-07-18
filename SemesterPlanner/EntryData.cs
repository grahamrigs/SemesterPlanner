using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Streaming.Adaptive;
using Windows.UI.Xaml.Controls;
using Windows.Web.Syndication;

namespace SemesterPlanner
{
    class EntryData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



        private string entry_projectname_ = "";
        private string entryID_ = "";
        private string title_ = "";
        private string subtitle_ = "";
        private string colourhex_ = "";
        private string setcolid_ = "";
        private string actualcolid_ = "";
        private int rowposition_ = 0;
        private int colposition_ = 0;
        private bool is_selected_ = false;
        private string stylename_title_ = stylename_title_lst_[0];
        private string stylename_calendar_ = stylename_calendar_lst_[0];
        private string stylename_preview_ = stylename_preview_lst_[0];


        static private readonly List<string> stylename_title_lst_ = new List<string>
        { "bor_EntryTitleBlock", "bor_EntryTitleBlock_Selected" };

        static private readonly List<string> stylename_calendar_lst_ = new List<string>
        { "bor_EntryCalendarBlock", "bor_EntryCalendarBlock_Selected" };

        static private readonly List<string> stylename_preview_lst_ = new List<string>
        { "bor_AddNewEntryPreviewTitleBlock", "bor_AddNewEntryPreviewTitleBlock_Selected" };






        public string Entry_ProjectName
        {
            get { return entry_projectname_; }
            set
            {
                if (value != entry_projectname_)
                {
                    entry_projectname_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string EntryID
        {
            get { return entryID_; }
            set
            {
                if (value != entryID_)
                {
                    entryID_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Title
        {
            get { return title_; }
            set
            {
                if (value != title_)
                {
                    title_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Subtitle
        {
            get { return subtitle_; }
            set
            {
                if (value != subtitle_)
                {
                    subtitle_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ColourHex
        {
            get { return colourhex_; }
            set
            {
                if (value != colourhex_)
                {
                    colourhex_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SetColID
        {
            get { return setcolid_; }
            set
            {
                if (value != setcolid_)
                {
                    setcolid_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public int RowPosition
        {
            get { return rowposition_; }
            set
            {
                if (value != rowposition_)
                {
                    rowposition_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public int ColPosition
        {
            get { return colposition_; }
            set
            {
                //Debug.WriteLine(string.Format("  Setting ColPosition for '{0}'    from: {1}    to: {2}", EntryID, colposition_, value));

                if (value != colposition_)
                {
                    colposition_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool Is_Selected
        {
            get { return is_selected_; }
            set
            {
                if (value != is_selected_)
                {
                    is_selected_ = value;
                    DetermineStyles();
                    //OnPropertyChanged();
                }
            }
        }
        public string StyleName_Title
        {
            get { return stylename_title_; }
            private set
            {
                if (value != stylename_title_)
                {
                    stylename_title_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string StyleName_Calendar
        {
            get { return stylename_calendar_; }
            private set
            {
                if (value != stylename_calendar_)
                {
                    stylename_calendar_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string StyleName_Preview
        {
            get { return stylename_preview_; }
            private set
            {
                if (value != stylename_preview_)
                {
                    stylename_preview_ = value;
                    OnPropertyChanged();
                }
            }
        }

        private void DetermineStyles()
        {
            int name_index = 0;

            if (is_selected_)
            {
                name_index = 1;
            }

            StyleName_Title = stylename_title_lst_[name_index];
            StyleName_Calendar = stylename_calendar_lst_[name_index];
            StyleName_Preview = stylename_preview_lst_[name_index];
        }


        //this one isn't saved, it's calculated at runtime
        public string ActualColID
        {
            get { return actualcolid_; }
            set
            {
                if (value != actualcolid_)
                {
                    actualcolid_ = value;
                    OnPropertyChanged();
                }
            }
        }



        public List<string> PrereqEntryIDs { get; set; }
        public List<string> CoreqEntryIDs { get; set; }
        public List<string> AvailColIDs { get; set; }


        public Border TitleBlock { get; set; }
        public Border CalendarBlock { get; set; }




        private readonly List<string> Parameter_Names = new List<string> { 
            "Entry_ProjectName", 
            "EntryID", 
            "Title", 
            "Subtitle",
            "ColourHex",
            "SetColID",
            "ActualColID",
            "RowPosition",
            "ColPosition",
            "Is_Selected",
            "PrereqEntryIDs",
            "CoreqEntryIDs",
            "AvailColIDs",
        };
        private readonly List<string> Parameter_Save_Names = new List<string> { 
            "", 
            "entry-id", 
            "title", 
            "subtitle",
            "colour", 
            "prerequisites", 
            "corequisites", 
            "avail-col-ids", 
            "set-col-id", 
            "list-pos" 
        };




        public void GetEntryDataFromLine(string project_name, string cur_line)
        {
            Debug.WriteLine("GetEntryDataFromLine");

            //putting in the project name already extracted
            Entry_ProjectName = project_name;


            //setting all parameters to default values
            EntryID = "";
            Title = "";
            Subtitle = "";
            ColourHex = "";
            PrereqEntryIDs = new List<string>();
            CoreqEntryIDs = new List<string>();
            SetColID = "";
            AvailColIDs = new List<string>();
            RowPosition = -1;
            ActualColID = "";


            //now to extract the other data from the supplied entry data line string


            //keeps track of all the data types already put into the EntryData, useful to find errors like two titles, etc.
            List<string> inputted_data_types = new List<string>();

            //splits up all the data of the entry. this list will be of the form {"ent-data", "entry-id=mech_2222", "title=MECH 2222", "subtitle=Thermodynamics", ...}
            string[] cur_line_split = cur_line.Split(';');


            //now we will go through every data pair to save it into the EntryData
            foreach (string cur_data_pair in cur_line_split)
            {
                //Debug.WriteLine("cur_data_pair: " + cur_data_pair);

                //the line indicator is "ent-data" and will be skipped here
                if (cur_data_pair == "ent-data") { continue; }


                //this will be of the form {"entry-id", "1"}
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
                    case "entry-id":
                        EntryID = property_value;
                        inputted_data_types.Add(property_name);
                        break;

                    case "title":
                        Title = property_value;
                        inputted_data_types.Add(property_name);
                        break;

                    case "subtitle":
                        Subtitle = property_value;
                        inputted_data_types.Add(property_name);
                        break;

                    case "colour":
                        ColourHex = property_value;
                        inputted_data_types.Add(property_name);
                        break;

                    case "prerequisites":
                        PrereqEntryIDs = ExtractListFromString(property_value);
                        inputted_data_types.Add(property_name);
                        break;

                    case "corequisites":
                        CoreqEntryIDs = ExtractListFromString(property_value);
                        inputted_data_types.Add(property_name);
                        break;

                    case "set-col-id":
                        SetColID = property_value;
                        inputted_data_types.Add(property_name);
                        break;

                    case "avail-col-ids":
                        AvailColIDs = ExtractListFromString(property_value);
                        inputted_data_types.Add(property_name);
                        break;

                    case "list-pos":
                        RowPosition = Convert.ToInt32(property_value);
                        inputted_data_types.Add(property_name);
                        break;

                    default:
                        break;
                }
            }
        }

        List<string> ExtractListFromString(string untrimmed_list_str)
        {
            int str_length = untrimmed_list_str.Length;

            //checks if the correct format    [12,34,...]
            if (str_length < 2 || untrimmed_list_str[0] != '[' || untrimmed_list_str[str_length - 1] != ']')
            {
                return new List<string>();
            }

            //this is the return list
            List<string> return_lst = new List<string>();



            string trimmed_brackets = untrimmed_list_str.Trim('[', ']');

            if (trimmed_brackets.Length == 0)
            {
                //then it was an empty list originally    "[]"
                return new List<string>(0);
            }

            //otherwise it has some values

            string[] split_list = trimmed_brackets.Split(',');

            foreach (string cur_split_id_str in split_list)
            {
                if (cur_split_id_str.Length > 0)
                {
                    return_lst.Add(cur_split_id_str);
                }
                else
                { continue; }
            }

            return return_lst;
        }



        public void PrintEntryDataValues()
        {
            Debug.WriteLine("\nEntryData class paramters:");



            foreach (string cur_param_name in Parameter_Names)
            {
                bool basic_formatting = false;
                bool list_formatting = false;

                string cur_param_val = "";
                List<string> cur_param_lst = new List<string>();

                switch (cur_param_name)
                {

                    case "Entry_ProjectName":
                        cur_param_val = Entry_ProjectName.ToString();
                        basic_formatting = true;
                        break;

                    case "EntryID":
                        cur_param_val = EntryID.ToString();
                        basic_formatting = true;
                        break;

                    case "Title":
                        cur_param_val = Title.ToString();
                        basic_formatting = true;
                        break;

                    case "Subtitle":
                        cur_param_val = Subtitle.ToString();
                        basic_formatting = true;
                        break;

                    case "ColourHex":
                        cur_param_val = ColourHex.ToString();
                        basic_formatting = true;
                        break;

                    case "PrereqEntryIDs":
                        cur_param_lst = PrereqEntryIDs;
                        list_formatting = true;
                        break;

                    case "CoreqEntryIDs":
                        cur_param_lst = CoreqEntryIDs;
                        list_formatting = true;
                        break;

                    case "SetColID":
                        cur_param_val = SetColID.ToString();
                        basic_formatting = true;
                        break;

                    case "ActualColID":
                        cur_param_val = ActualColID.ToString();
                        basic_formatting = true;
                        break;

                    case "RowPosition":
                        cur_param_val = RowPosition.ToString();
                        basic_formatting = true;
                        break;

                    case "ColPosition":
                        cur_param_val = ColPosition.ToString();
                        basic_formatting = true;
                        break;

                    case "AvailColIDs":
                        cur_param_lst = AvailColIDs;
                        list_formatting = true;
                        break;

                    case "Is_Selected":
                        cur_param_val = Is_Selected.ToString();
                        basic_formatting = true;
                        break;

                }

                if (basic_formatting)
                {
                    Debug.WriteLine(string.Format("    {0,-18} = {1}", cur_param_name, cur_param_val));
                }
                if (list_formatting)
                {
                    Debug.Write(string.Format("    {0,-18} = ", cur_param_name));

                    foreach (string cur_val in cur_param_lst)
                    {
                        Debug.Write(string.Format("{0} ", cur_val));
                    }
                    Debug.WriteLine("");
                }
            }
            Debug.WriteLine("");


        }



        public string CreateEntryDataSaveLine(string data_separator)
        {
            //we want
            //  ent-data;entry-id=geog_222;title=GEOG 222;subtitle=Introduction to Geomatics;colour=#FFFF7272;prerequisites=[];
            //      corequisites=[];avail-col-ids=[20_fa,21_wi,21_fa,22_wi,22_fa,23_wi,23_fa,24_wi];set-col-id=;list-pos=0

            Debug.WriteLine("CreateEntryDataSaveLine");

            string return_data_line;
            string data_line_start = "ent-data";


            return_data_line = data_line_start;


            foreach (string cur_data_parameter in Parameter_Names)
            {
                //should be like this
                // Parameter_Names = { "Entry_ProjectName", "EntryID", "Title", "Subtitle", 
                //    "ColourHex", "PrereqEntryIDs", "CoreqEntryIDs", "AvailColIDs", "SetColID", "RowPosition" };
                // Parameter_Save_Names = { "", "entry-id", "title", "subtitle",
                //    "colour", "prerequisites", "corequisites", "avail-col-ids", "set-col-id", "list-pos" };

                //this one is skipped
                if (cur_data_parameter == "Col_ProjectName") { continue; }


                string cur_data_addition = "";

                string cur_data_save_name = Parameter_Save_Names[Parameter_Names.IndexOf(cur_data_parameter)];
                string data_val = "";

                switch (cur_data_parameter)
                {
                    case "EntryID":
                        data_val = EntryID.ToString();
                        break;

                    case "Title":
                        data_val = Title.ToString();
                        break;

                    case "Subtitle":
                        data_val = Subtitle.ToString();
                        break;

                    case "ColourHex":
                        data_val = ColourHex.ToString();
                        break;

                    case "PrereqEntryIDs":
                        data_val = FormattedSaveDataFromList(PrereqEntryIDs);
                        break;

                    case "CoreqEntryIDs":
                        data_val = FormattedSaveDataFromList(CoreqEntryIDs);
                        break;

                    case "AvailColIDs":
                        data_val = FormattedSaveDataFromList(AvailColIDs);
                        break;

                    case "SetColID":
                        data_val = SetColID.ToString();
                        break;

                    case "RowPosition":
                        data_val = RowPosition.ToString();
                        break;
                }

                cur_data_addition = string.Format("{0}={1}", cur_data_save_name, data_val);

                return_data_line += data_separator + cur_data_addition;
            }

            return return_data_line;

        }

        public string FormattedSaveDataFromList(List<string> working_list)
        {
            string return_list = "[";

            for (int i = 0; i < working_list.Count; i++)
            {
                if (i != 0)
                {
                    return_list += ",";
                }

                return_list += working_list[i];
            }

            return_list += "]";

            return return_list;
        }






    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace SemesterPlanner
{
    class AddNewEntry : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            Debug.WriteLine("OnPropertyChanged     " + name);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        static public List<string> Styles_TextBox_lst = new List<string> 
        { "txtbox_Property", "txtbox_Property_Invalid" };


        private string entryID_add_ = "";
        private string title_add_ = "";
        private string subtitle_add_ = "";
        private string colourhex_add_ = MainPage.glo_default_titleblock_colour_hex;
        private List<string> prereq_entryIDs_add_ = new List<string>();
        private List<string> coreq_entryIDs_add_ = new List<string>();
        private List<string> avail_colIDs_add_ = new List<string>();

        private bool create_button_enabled_ = false;
        private bool title_valid_ = false;
        private bool entryID_valid_ = false;

        private string title_txtbox_style_ = Styles_TextBox_lst[0];



        public string EntryID_Add
        {
            get { return entryID_add_; }
            set
            {
                if (value != entryID_add_)
                {
                    entryID_add_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Title_Add
        {
            get { return title_add_; }
            set
            {
                if (value != title_add_)
                {
                    title_add_ = value;
                    TitleSubtitleChanged();
                    UpdatePreviewBlock();
                    OnPropertyChanged();
                }
            }
        }
        public string Subtitle_Add
        {
            get { return subtitle_add_; }
            set
            {
                if (value != subtitle_add_)
                {
                    subtitle_add_ = value;
                    TitleSubtitleChanged();
                    UpdatePreviewBlock();
                    OnPropertyChanged();
                }
            }
        }
        public string ColourHex_Add
        {
            get { return colourhex_add_; }
            set
            {
                if (value != colourhex_add_)
                {
                    colourhex_add_ = value;
                    UpdatePreviewBlock();
                    OnPropertyChanged();
                }
            }
        }
        public List<string> PrereqEntryIDs_Add
        {
            get { return prereq_entryIDs_add_; }
            set
            {
                if (value != prereq_entryIDs_add_)
                {
                    prereq_entryIDs_add_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<string> CoreqEntryIDs_Add
        {
            get { return coreq_entryIDs_add_; }
            set
            {
                if (value != coreq_entryIDs_add_)
                {
                    coreq_entryIDs_add_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<string> AvailColIDs_Add
        {
            get { return avail_colIDs_add_; }
            set
            {
                if (value != avail_colIDs_add_)
                {
                    avail_colIDs_add_ = value;
                    OnPropertyChanged();
                }
            }
        }


        public bool CreateButton_Enabled
        {
            get { return create_button_enabled_; }
            private set
            {
                create_button_enabled_ = value;
                OnPropertyChanged();
            }
        }
        public bool Title_Valid
        {
            get { return title_valid_; }
            private set
            {
                if (value != title_valid_)
                {
                    title_valid_ = value;
                    ValidChanged();
                    OnPropertyChanged();
                }
            }
        }
        public bool EntryID_Valid
        {
            get { return entryID_valid_; }
            private set
            {
                if (value != entryID_valid_)
                {
                    entryID_valid_ = value;
                    ValidChanged();
                    OnPropertyChanged();
                }
            }
        }

        public string Title_Txtbox_Style
        {
            get { return title_txtbox_style_; }
            private set
            {
                if (value != title_txtbox_style_)
                {
                    title_txtbox_style_ = value;
                    OnPropertyChanged();
                }
            }
        }



        public ProjectData glo_ProjectData_Reference { get; set; }
        public MainPage glo_MainPage_Reference { get; set; }



        public void GetFieldData(out string entryID, out string title, out string subtitle, out string colourhex,
            out List<string> prereq_entryIDs, out List<string> coreq_entryIDs, out List<string> avail_colIDs)
        {
            entryID = EntryID_Add;
            title = Title_Add;
            subtitle = Subtitle_Add;
            colourhex = ColourHex_Add;
            prereq_entryIDs = PrereqEntryIDs_Add;
            coreq_entryIDs = CoreqEntryIDs_Add;
            avail_colIDs = AvailColIDs_Add;
        }
        public EntryData GetEntryDataForPreview()
        {
            EntryData return_entryData = new EntryData();



            return_entryData.Title = Title_Add;
            return_entryData.Subtitle = Subtitle_Add;
            return_entryData.ColourHex = ColourHex_Add;
            return_entryData.EntryID = "preview";



            return return_entryData;
        }



        private void TitleSubtitleChanged()
        {
            AddNewEntryValid();

            if (Title_Valid)
            {
                CreateNewEntryID(Title_Add, out bool is_entryID_valid, out string entryID_created);

                if (is_entryID_valid)
                {
                    EntryID_Add = entryID_created;
                }
                else
                {
                    EntryID_Add = "";
                }

                EntryID_Valid = is_entryID_valid;

                Debug.WriteLine("EntryID_Valid = " + EntryID_Valid.ToString() + "     EntryID_Add = " + EntryID_Add);
            }
            else
            {
                EntryID_Add = "";
                EntryID_Valid = false;

                glo_MainPage_Reference.ShowAddNewEntryTitleInvalidTeachingTip();
            }


            if (!Title_Valid) { Title_Txtbox_Style = Styles_TextBox_lst[1]; }
            else { Title_Txtbox_Style = Styles_TextBox_lst[0]; }
        }

        public void AddNewEntryValid()
        {
            //will determine if the add new entry is valid


            List<string> check_title_subtitle = new List<string> { Title_Add, Subtitle_Add };
            List<string> exclude_lst = new List<string>();

            glo_ProjectData_Reference.IsNewTitleValid(check_title_subtitle, exclude_lst, out bool valid_entry,
                out bool is_duplicate, out bool is_title_exist);


            //this will only display if it's going to say it's invalid
            if (!valid_entry)
            {
                Debug.WriteLine("\nAddNewEntryValid");
                Debug.WriteLine(string.Format("    {0,-15} = {1}", "Title", Title_Add));
                Debug.WriteLine(string.Format("    {0,-15} = {1}", "Subtitle", Subtitle_Add));
                Debug.WriteLine(string.Format("    {0,-15} = {1}", "is_duplicate", is_duplicate));
                Debug.WriteLine(string.Format("    {0,-15} = {1}", "is_title_exist", is_title_exist));
                Debug.WriteLine(string.Format("    {0,-15} = {1}", "valid_entry", valid_entry));
                Debug.WriteLine("");
            }


            Title_Valid = valid_entry;
        }
        public void CreateNewEntryID(string given_title, out bool is_entryID_valid, out string entryID_created)
        {
            //will create a new entryID given an entry title

            Debug.WriteLine("CreateNewEntryID     title='" + given_title + "'");

            //the default return items
            is_entryID_valid = false;
            entryID_created = "";


            //just to ensure that the given title is valid
            if (string.IsNullOrEmpty(given_title)) 
            {
                Debug.WriteLine("ERROR: given_title not valid"); 
                return;
            }


            //we want to turn something like  title="Mech 2202"  into  entry_ID="mech†2202‡‡‡‡‡‡‡‖0001"
            //the title will be made lowercase, spaces converted to †, and truncated with ‡ trailing spaces
            //the second part will be a string number, in case the first section is a duplicate

            int truncated_title_length = 16;
            int designation_length = 4;
            int designation_start = 1;
            char space_replacement = '†';   //this is the "dagger" symbol
            char trailing_space = '‡';      //this is the "double dagger" symbol
            char title_des_sep = '‖';       //this is the "double vertical line" symbol


            string title_lower = given_title.ToLower();
            string conv_spaces = title_lower.Replace(' ', space_replacement);

            int original_length = conv_spaces.Length;

            string trunc_title;
            if (original_length > truncated_title_length)
            {
                trunc_title = conv_spaces.Substring(0, truncated_title_length);

                //will convert      "Mech 2202 New Class Blah"   to   "mech†2202†new†cl"
            }
            else //original_length <= truncated_title_length
            {
                trunc_title = conv_spaces.PadRight(truncated_title_length, trailing_space);

                //will convert      "Mech 2202"   to   "mech†2202‡‡‡‡‡‡‡"
            }

            //this will be the number portion
            int designation_number;

            //now will check if this truncated title portion already exists
            bool trunc_title_exists = glo_ProjectData_Reference.entryIDs_text_lst_param.Contains(trunc_title);


            //if it doesn't exist, then it's simple
            if (!trunc_title_exists)
            {
                designation_number = designation_start;
            }
            //otherwise we need to increase the counter
            else
            {
                designation_number = glo_ProjectData_Reference.GetMaxDesignationForEntryIDTitle(trunc_title) + 1;
            }

            //this was the error number for designation
            if (designation_number == -1) 
            {
                Debug.WriteLine("ERROR: designation_number = " + designation_number);
                return;
            }


            //will have made  34  into  "0034"
            string designation_final = designation_number.ToString().PadLeft(designation_length, '0');


            //should be the final format of  entry_ID="mech†2202‡‡‡‡‡‡‡‖0001"
            entryID_created = trunc_title + title_des_sep + designation_final;
            is_entryID_valid = true;


            return;
        }

        public void ValidChanged()
        {
            bool both_valid = (Title_Valid && EntryID_Valid);

            CreateButton_Enabled = both_valid;
        }



        private void UpdatePreviewBlock()
        {
            glo_MainPage_Reference.AddNewEntryUpdatePreview();
        }



        public void PrintProperties()
        {

            Debug.WriteLine("                     |          Value");
            Debug.WriteLine("---------------------|----------------------");

            List<string> property_names = new List<string> {
                "EntryID",
                "Title",
                "Subtitle",
                "ColourHex" };

            List<string> property_vals_changed = new List<string> {
                EntryID_Add.ToString(),
                Title_Add.ToString(),
                Subtitle_Add.ToString(),
                ColourHex_Add.ToString() };

            for (int i = 0; i < property_names.Count(); i++)
            {
                Debug.WriteLine(string.Format("{0, 20} | {1, 20}", property_names[i], property_vals_changed[i]));
            }



            List<string> changed_names = new List<string> { "CreateButton_Enabled","Title_Valid" };

            List<bool> changed_bools = new List<bool> { CreateButton_Enabled, Title_Valid };

            Debug.WriteLine("");
            for (int i = 0; i < changed_names.Count; i++)
            {
                Debug.WriteLine(string.Format("{0,-22} = {1}", changed_names[i], changed_bools[i].ToString()));
            }
        }


    }
}

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
    public class ChangedProperties : INotifyPropertyChanged
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
        { "txtbox_Property", "txtbox_Property_Changed" , "txtbox_Property_Invalid" };


        private string entryID_new_ = "";
        private string title_new_ = "";
        private string subtitle_new_ = "";
        private string colourhex_new_ = MasterClass.glo_default_titleblock_colour_hex;
        private string setcolid_new_ = "";
        private List<string> prereq_entryIDs_new_ = new List<string>();
        private List<string> coreq_entryIDs_new_ = new List<string>();
        private List<string> avail_colIDs_new_ = new List<string>();


        private bool entryID_changed_ = false;
        private bool title_changed_ = false;
        private bool subtitle_changed_ = false;
        private bool colourhex_changed_ = false;
        private bool setcolid_changed_ = false;
        private bool prereq_entryIDs_changed_ = false;
        private bool coreq_entryIDs_changed_ = false;
        private bool avail_colIDs_changed_ = false;

        private bool apply_button_enabled_ = false;
        private bool title_valid_ = true;
        private bool subtitle_valid_ = true;

        private string title_txtbox_style_ = Styles_TextBox_lst[0];



        public string EntryID_Old { get; private set; }
        public string Title_Old { get; private set; }
        public string Subtitle_Old { get; private set; }
        public string ColourHex_Old { get; private set; }
        public string SetColID_Old { get; private set; }
        public List<string> PrereqEntryIDs_Old { get; private set; }
        public List<string> CoreqEntryIDs_Old { get; private set; }
        public List<string> AvailColIDs_Old { get; private set; }



        public string EntryID_New
        {
            get { return entryID_new_; }
            set
            {
                if (value != entryID_new_)
                {
                    entryID_new_ = value;
                    UpdateChangedBool("entryID");
                    OnPropertyChanged();
                }
            }
        }
        public string Title_New
        {
            get { return title_new_; }
            set
            {
                if (value != title_new_)
                {
                    title_new_ = value;

                    IsValidChangeTitleSubtitle();
                    UpdateChangedBool("title");

                    //Glo_MainPage_Reference.CheckIfPropertiesChanged(true);

                    OnPropertyChanged();
                }
            }
        }
        public string Subtitle_New
        {
            get { return subtitle_new_; }
            set
            {
                if (value != subtitle_new_)
                {
                    subtitle_new_ = value;

                    //IsValidChangeTitleSubtitle();
                    UpdateChangedBool("subtitle");

                    //Glo_MainPage_Reference.CheckIfPropertiesChanged(true);

                    OnPropertyChanged();
                }
            }
        }
        public string ColourHex_New
        {
            get { return colourhex_new_; }
            set
            {
                if (value != colourhex_new_)
                {
                    colourhex_new_ = value;
                    UpdateChangedBool("colourhex");
                    OnPropertyChanged();
                }
            }
        }
        public string SetColID_New
        {
            get { return setcolid_new_; }
            set
            {
                if (value != setcolid_new_)
                {
                    setcolid_new_ = value;
                    UpdateChangedBool("setcolID");
                    OnPropertyChanged();
                }
            }
        }
        public List<string> PrereqEntryIDs_New
        {
            get { return prereq_entryIDs_new_; }
            set
            {
                if (value != prereq_entryIDs_new_)
                {
                    prereq_entryIDs_new_ = value;
                    UpdateChangedBool("prereq_entryIDs");
                    OnPropertyChanged();
                }
            }
        }
        public List<string> CoreqEntryIDs_New
        {
            get { return coreq_entryIDs_new_; }
            set
            {
                if (value != coreq_entryIDs_new_)
                {
                    coreq_entryIDs_new_ = value;
                    UpdateChangedBool("coreq_entryIDs");
                    OnPropertyChanged();
                }
            }
        }
        public List<string> AvailColIDs_New
        {
            get { return avail_colIDs_new_; }
            set
            {
                if (value != avail_colIDs_new_)
                {
                    avail_colIDs_new_ = value;
                    UpdateChangedBool("avail_colIDs");
                    OnPropertyChanged();
                }
            }
        }



        public bool EntryID_Changed
        {
            get { return entryID_changed_; }
            private set
            {
                if (value != entryID_changed_)
                {
                    entryID_changed_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool Title_Changed
        {
            get { return title_changed_; }
            private set
            {
                if (value != title_changed_)
                {
                    title_changed_ = value;
                    DetermineTitleTextboxStyle();
                    OnPropertyChanged();
                }
            }
        }
        public bool Subtitle_Changed
        {
            get { return subtitle_changed_; }
            private set
            {
                if (value != subtitle_changed_)
                {
                    subtitle_changed_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool ColourHex_Changed
        {
            get { return colourhex_changed_; }
            private set
            {
                if (value != colourhex_changed_)
                {
                    colourhex_changed_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool SetColID_Changed
        {
            get { return setcolid_changed_; }
            private set
            {
                if (value != setcolid_changed_)
                {
                    setcolid_changed_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool PrereqEntryIDs_Changed
        {
            get { return prereq_entryIDs_changed_; }
            private set
            {
                if (value != prereq_entryIDs_changed_)
                {
                    prereq_entryIDs_changed_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool CoreqEntryIDs_Changed
        {
            get { return coreq_entryIDs_changed_; }
            private set
            {
                if (value != coreq_entryIDs_changed_)
                {
                    coreq_entryIDs_changed_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool AvailColIDs_Changed
        {
            get { return avail_colIDs_changed_; }
            private set
            {
                if (value != avail_colIDs_changed_)
                {
                    avail_colIDs_changed_ = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AnyProperties_Changed
        {
            get { return apply_button_enabled_; }
            private set
            {
                apply_button_enabled_ = value;
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
                    DetermineTitleTextboxStyle();
                    OnPropertyChanged();
                }
            }
        }
        public bool Subtitle_Valid
        {
            get { return subtitle_valid_; }
            private set
            {
                if (value != subtitle_valid_)
                {
                    subtitle_valid_ = value;
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



        public ProjectData Glo_ProjectData_Reference { get; set; }
        public MainPage Glo_MainPage_Reference { get; set; }



        public void LoadOriginalEntryData(EntryData ori_entryData)
        {
            EntryID_Old             = ori_entryData.EntryID;
            Title_Old               = ori_entryData.Title;
            Subtitle_Old            = ori_entryData.Subtitle;
            ColourHex_Old           = ori_entryData.ColourHex;
            SetColID_Old            = ori_entryData.SetColID;
            PrereqEntryIDs_Old      = ori_entryData.PrereqEntryIDs;
            CoreqEntryIDs_Old       = ori_entryData.CoreqEntryIDs;
            AvailColIDs_Old         = ori_entryData.AvailColIDs;


            EntryID_New             = EntryID_Old;
            Title_New               = Title_Old;
            Subtitle_New            = Subtitle_Old;
            ColourHex_New           = ColourHex_Old;
            SetColID_New            = SetColID_Old;
            PrereqEntryIDs_New      = PrereqEntryIDs_Old;
            CoreqEntryIDs_New       = CoreqEntryIDs_Old;
            AvailColIDs_New         = AvailColIDs_Old;
        }
        public EntryData GetChangedEntryData()
        {
            EntryData return_entryData = new EntryData
            {
                EntryID = EntryID_New,
                Title = Title_New,
                Subtitle = Subtitle_New,
                ColourHex = ColourHex_New,
                SetColID = SetColID_New,
                PrereqEntryIDs = PrereqEntryIDs_New,
                CoreqEntryIDs = CoreqEntryIDs_New,
                AvailColIDs = AvailColIDs_New
            };


            return return_entryData;
        }
        public void GetFieldData(out string entryID, out string title, out string subtitle, out string colourhex, out string setcolID, 
            out List<string> prereq_entryIDs, out List<string> coreq_entryIDs, out List<string> avail_colIDs )
        {
            entryID = EntryID_New;
            title = Title_New;
            subtitle = Subtitle_New;
            colourhex = ColourHex_New;
            setcolID = SetColID_New;
            prereq_entryIDs = PrereqEntryIDs_New;
            coreq_entryIDs = CoreqEntryIDs_New;
            avail_colIDs = AvailColIDs_New;
        }


        public List<string> GetListOfChangedProperties()
        {
            List<string> return_list = new List<string>();

            return return_list;
        }



        private void UpdateChangedBool(string changed_property)
        {


            switch (changed_property)
            {
                case "entryID":
                    EntryID_Changed = (EntryID_Old != entryID_new_);
                    break;

                case "title":
                    Title_Changed = (Title_Old != title_new_);
                    break;

                case "subtitle":
                    Subtitle_Changed = (Subtitle_Old != subtitle_new_);
                    break;

                case "colourhex":
                    ColourHex_Changed = (ColourHex_Old != colourhex_new_);
                    break;

                case "setcolID":
                    SetColID_Changed = (SetColID_Old != setcolid_new_);
                    break;

                case "prereq_entryIDs":
                    PrereqEntryIDs_Changed = (PrereqEntryIDs_Old != prereq_entryIDs_new_);
                    break;

                case "coreq_entryIDs":
                    CoreqEntryIDs_Changed = (CoreqEntryIDs_Old != coreq_entryIDs_new_);
                    break;

                case "avail_colIDs":
                    AvailColIDs_Changed = (AvailColIDs_Old != avail_colIDs_new_);
                    break;

            }

            Debug.WriteLine("before any assignment");
            Debug.WriteLine("apply_button_enabled_ = " + apply_button_enabled_);

            bool[] changed_lst = new bool[] { entryID_changed_, title_changed_, subtitle_changed_, colourhex_changed_, 
                setcolid_changed_, prereq_entryIDs_changed_, coreq_entryIDs_changed_, avail_colIDs_changed_ };

            AnyProperties_Changed = changed_lst.Contains(true) && title_valid_ && subtitle_valid_;

            Debug.WriteLine("after any assignment");
            Debug.WriteLine("apply_button_enabled_ = " + apply_button_enabled_);

            //Testing();


        }
        private void IsValidChangeTitleSubtitle()
        {

            List<string> check_title_subtitle = new List<string> { title_new_, subtitle_new_ };
            List<string> exclude_lst = new List<string> { Title_Old, Subtitle_Old };

            if (Glo_ProjectData_Reference == null) { return; }

            Glo_ProjectData_Reference.IsNewTitleValid(check_title_subtitle, exclude_lst, out bool valid_entry,
                out _, out _);



            Title_Valid = valid_entry;


            if (!Title_Valid)
            {
                Glo_MainPage_Reference.ShowPropertiesTitleInvalidTeachingTip();
            }


        }

        public void PrintProperties(EntryData given_selected_entryData)
        {

            Debug.WriteLine("                     |       Selected       |       Original       |         Changed");
            Debug.WriteLine("---------------------|----------------------|----------------------|----------------------");

            List<string> property_names = new List<string> {
                "EntryID",
                "Title",
                "Subtitle",
                "ColourHex" };

            List<string> selected_vals = new List<string> {
                given_selected_entryData.EntryID.ToString(),
                given_selected_entryData.Title.ToString(),
                given_selected_entryData.Subtitle.ToString(),
                given_selected_entryData.ColourHex.ToString() };

            List<string> property_vals_original = new List<string> {
                EntryID_Old.ToString(),
                Title_Old.ToString(),
                Subtitle_Old.ToString(),
                ColourHex_Old.ToString() };

            List<string> property_vals_changed = new List<string> {
                EntryID_New.ToString(),
                Title_New.ToString(),
                Subtitle_New.ToString(),
                ColourHex_New.ToString() };

            for (int i = 0; i < property_names.Count(); i++)
            {
                Debug.WriteLine(string.Format("{0, 20} | {1, 20} | {2, 20} | {3, 20}", property_names[i], selected_vals[i], property_vals_original[i], property_vals_changed[i]));
            }



            List<string> changed_names = new List<string> { "EntryID_Changed", "Title_Changed", "Subtitle_Changed",
                "ColourHex_Changed", "SetColID_Changed", "PrereqEntryIDs_Changed", "CoreqEntryIDs_Changed",
                "AvailColIDs_Changed", "AnyProperties_Changed","Title_Valid", "Subtitle_Valid" };

            List<bool> changed_bools = new List<bool> { EntryID_Changed, Title_Changed, Subtitle_Changed, ColourHex_Changed, 
                SetColID_Changed, PrereqEntryIDs_Changed, CoreqEntryIDs_Changed, AvailColIDs_Changed, AnyProperties_Changed,
                Title_Valid, Subtitle_Valid};

            Debug.WriteLine("");
            for (int i = 0; i < changed_names.Count; i++)
            {
                Debug.WriteLine(string.Format("{0,-22} = {1}", changed_names[i], changed_bools[i].ToString()));
            }
        }


        private void DetermineTitleTextboxStyle()
        {
            string style_name;

            if (!Title_Valid)
            {
                style_name = Styles_TextBox_lst[2];
            }
            else if (Title_Changed)
            {
                style_name = Styles_TextBox_lst[1];
            }
            else
            {
                style_name = Styles_TextBox_lst[0];
            }

            Title_Txtbox_Style = style_name;
        }

    }
}

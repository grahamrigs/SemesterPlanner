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

        static public List<string> Styles_TextBox_lst = new List<string> { "txtbox_Property", "txtbox_Property_Changed" };


        private string entryID_new_ = "";
        private string title_new_ = "";
        private string subtitle_new_ = "";
        private string colourhex_new_ = "";
        private string setcolid_new_ = "";
        private List<string> prereq_entryIDs_new_ = new List<string>();
        private List<string> coreq_entryIDs_new_ = new List<string>();
        private List<string> avail_colIDs_new_ = new List<string>();

        private bool create_button_enabled_ = false;
        private bool title_valid_ = true;
        private bool subtitle_valid_ = true;



        public string EntryID_New
        {
            get { return entryID_new_; }
            set
            {
                if (value != entryID_new_)
                {
                    entryID_new_ = value;
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



        public ProjectData glo_ProjectData_Reference { get; set; }



        public void GetFieldData(out string entryID, out string title, out string subtitle, out string colourhex, out string setcolID,
            out List<string> prereq_entryIDs, out List<string> coreq_entryIDs, out List<string> avail_colIDs)
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






        private void IsValidChangeTitleSubtitle()
        {

            List<string> check_title_subtitle = new List<string> { title_new_, subtitle_new_ };
            List<string> exclude_lst = new List<string>();

            glo_ProjectData_Reference.IsNewTitleValid(check_title_subtitle, exclude_lst, out bool valid_entry,
                out bool is_duplicate, out bool is_title_exist);

            Title_Valid = valid_entry;
        }

        public void PrintProperties(EntryData given_selected_entryData)
        {

            Debug.WriteLine("                     |          Value");
            Debug.WriteLine("---------------------|----------------------");

            List<string> property_names = new List<string> {
                "EntryID",
                "Title",
                "Subtitle",
                "ColourHex" };

            List<string> property_vals_changed = new List<string> {
                EntryID_New.ToString(),
                Title_New.ToString(),
                Subtitle_New.ToString(),
                ColourHex_New.ToString() };

            for (int i = 0; i < property_names.Count(); i++)
            {
                Debug.WriteLine(string.Format("{0, 20} | {1, 20}", property_names[i], property_vals_changed[i]));
            }



            List<string> changed_names = new List<string> { "CreateButton_Enabled","Title_Valid", "Subtitle_Valid" };

            List<bool> changed_bools = new List<bool> { CreateButton_Enabled, Title_Valid, Subtitle_Valid };

            Debug.WriteLine("");
            for (int i = 0; i < changed_names.Count; i++)
            {
                Debug.WriteLine(string.Format("{0,-22} = {1}", changed_names[i], changed_bools[i].ToString()));
            }
        }




    }
}

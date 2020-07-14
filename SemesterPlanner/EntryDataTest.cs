using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SemesterPlanner
{
    public class EntryDataTest : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private string title = "default title";
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }

        private string subtitle = "default subtitle";
        public string Subtitle
        {
            get { return subtitle; }
            set
            {
                subtitle = value;
                // Call OnPropertyChanged whenever the property is updated
                OnPropertyChanged();
            }
        }



        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}

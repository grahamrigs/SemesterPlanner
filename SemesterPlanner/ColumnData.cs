using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace SemesterPlanner
{
    class ColumnData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



        private string col_projectname_ = "";
        private string colID_ = "";
        private string coltitle_ = "";
        private string colsubtitle_ = "";
        private int colposition_ = -1;



        public string Col_ProjectName
        {
            get { return col_projectname_; }
            set
            {
                if (value != col_projectname_)
                {
                    col_projectname_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ColID
        {
            get { return colID_; }
            set
            {
                if (value != colID_)
                {
                    colID_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ColTitle
        {
            get { return coltitle_; }
            set
            {
                if (value != coltitle_)
                {
                    coltitle_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ColSubtitle
        {
            get { return colsubtitle_; }
            set
            {
                if (value != colsubtitle_)
                {
                    colsubtitle_ = value;
                    OnPropertyChanged();
                }
            }
        }
        public int ColPosition
        {
            get { return colposition_; }
            set
            {
                if (value != colposition_)
                {
                    colposition_ = value;
                    OnPropertyChanged();
                }
            }
        }




        public List<string> Parameter_Names = new List<string> { "Col_ProjectName", "ColID", "ColTitle", "ColSubtitle", "ColPosition" };
        public List<string> Parameter_Save_Names = new List<string> { "", "col-id", "col-title", "col-subtitle", "col-pos" };

        public Border ColumnHeader { get; set; }


        public void GetColumnDataFromLine(string project_name, string cur_line)
        {
            Debug.WriteLine("GetColumnDataFromLine");

            //putting in the project name already extracted
            Col_ProjectName = project_name;


            //setting all parameters to default values
            ColID = "";
            ColTitle = "";
            ColSubtitle = "";
            ColPosition = -1;


            //now to extract the other data from the supplied column data line string


            //keeps track of all the data types already put into the EntryData, useful to find errors like two titles, etc.
            List<string> inputted_data_types = new List<string>();

            //splits up all the data of the column. this list will be of the form {"col-data", "col-id=5", "col-name=Winter 2022", ...}
            string[] cur_line_split = cur_line.Split(';');


            //now we will go through every data pair to save it into the EntryData
            foreach (string cur_data_pair in cur_line_split)
            {
                //Debug.WriteLine("cur_data_pair: " + cur_data_pair);

                //the line indicator is "col-data" and will be skipped here
                if (cur_data_pair == "col-data") { continue; }


                //this will be of the form {"col-id", "5"}
                string[] data_pair_split = cur_data_pair.Split('=');

                string property_name = data_pair_split[0];
                string property_value = data_pair_split[1];


                //won't continue if the property type has already been saved
                if (inputted_data_types.Contains(property_name))
                {
                    Debug.WriteLine("ColumnData already contains the property: " + property_name + ". Skipping data pair.");
                    continue;
                }

                //if the current property value was blank, we skip
                if (property_value == "") { continue; }


                //a switch based on the name of the property in the data pair
                switch (property_name)
                {
                    case "col-id":
                        ColID = property_value;
                        inputted_data_types.Add(property_name);
                        break;

                    case "col-title":
                        ColTitle = property_value;
                        inputted_data_types.Add(property_name);
                        break;

                    case "col-subtitle":
                        ColSubtitle = property_value;
                        inputted_data_types.Add(property_name);
                        break;

                    case "col-pos":
                        ColPosition = Convert.ToInt32(property_value);
                        inputted_data_types.Add(property_name);
                        break;

                    default:
                        break;
                }
            }

        }

        public void PrintColumnDataValues()
        {
            Debug.WriteLine("\nColumnData class paramters:");


            List<string> param_names = new List<string> { "Col_ProjectName", "ColID", "ColTitle", "ColSubtitle", "ColPosition" };


            foreach (string cur_param_name in param_names)
            {
                bool basic_formatting = false;
                bool list_formatting = false;

                string cur_param_val = "";
                List<int> cur_param_lst = new List<int>();

                switch (cur_param_name)
                {
                    case "Col_ProjectName":
                        cur_param_val = Col_ProjectName.ToString();
                        basic_formatting = true;
                        break;

                    case "ColID":
                        cur_param_val = ColID.ToString();
                        basic_formatting = true;
                        break;

                    case "ColTitle":
                        cur_param_val = ColTitle.ToString();
                        basic_formatting = true;
                        break;

                    case "ColSubtitle":
                        cur_param_val = ColSubtitle.ToString();
                        basic_formatting = true;
                        break;

                    case "ColPosition":
                        cur_param_val = ColPosition.ToString();
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

                    foreach (int cur_val in cur_param_lst)
                    {
                        Debug.Write(string.Format("{0} ", cur_val.ToString()));
                    }
                    Debug.WriteLine("");
                }
            }
            Debug.WriteLine("");


        }



        public string CreateColumnDataSaveLine(string data_separator)
        {
            //we want
            //  col-data;col-id=20_fa;col-title=Fall 2020;col-subtitle=;col-pos=0

            Debug.WriteLine("CreateColumnDataSaveLine");

            string return_data_line;
            string data_line_start = "col-data";


            return_data_line = data_line_start;


            foreach (string cur_data_parameter in Parameter_Names)
            {
                //should be like this
                // Parameter_Names = { "Col_ProjectName", "ColID", "ColTitle", "ColSubtitle", "ColPosition" };
                // Parameter_Save_Names = { "", "col-id", "col-title", "col-subtitle", "col-pos" };

                //this one is skipped
                if (cur_data_parameter == "Col_ProjectName") { continue; }


                string cur_data_addition = "";

                string cur_data_save_name = Parameter_Save_Names[Parameter_Names.IndexOf(cur_data_parameter)];
                string data_val = "";

                switch (cur_data_parameter)
                {
                    case "ColID":
                        data_val = ColID.ToString();
                        break;

                    case "ColTitle":
                        data_val = ColTitle.ToString();
                        break;

                    case "ColSubtitle":
                        data_val = ColSubtitle.ToString();
                        break;

                    case "ColPosition":
                        data_val = ColPosition.ToString();
                        break;
                }

                cur_data_addition = string.Format("{0}={1}", cur_data_save_name, data_val);

                return_data_line += data_separator + cur_data_addition;
            }

            return return_data_line;

        }
    }
}

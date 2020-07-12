using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.Web.Syndication;

namespace SemesterPlanner
{
    class ProjectData
    {

        public string ProjectName { get; set; }
        public string ColumnType { get; set; }
        public GridLength HeaderWidth { get; set; }

        public List<string> Parameter_Names = new List<string> { "ProjectName", "ColumnType", "HeaderWidth" };
        public List<string> Parameter_Save_Names = new List<string> { "", "column-type", "header-width" };


        public List<ColumnData> ColumnData_lst_param { get; set; }
        public List<string> colIDs_lst_param { get; set; }
        public List<EntryData> EntryData_lst_param { get; set; }
        public List<string> entryIDs_lst_param { get; set; }
        public List<string> entryIDs_text_lst_param { get; set; }
        public List<int> entryDataListPos_lst_param { get; set; }





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
                            int integer_width;
                            if (int.TryParse(property_value, out integer_width))
                                HeaderWidth = new GridLength(integer_width, GridUnitType.Pixel);
                        }

                        break;

                    default:
                        break;

                }
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
                if (colIDs_lst_param != null)
                {
                    Debug.Write("ColID's:");

                    foreach (string cur_colID in colIDs_lst_param)
                    {
                        Debug.Write(" '" + cur_colID + "'");
                    }
                    Debug.Write("\n\n");

                }
                if (entryIDs_lst_param != null)
                {
                    Debug.Write("EntryID's:");

                    foreach (string cur_entryID in entryIDs_lst_param)
                    {
                        Debug.Write(" '" + cur_entryID + "'");
                    }
                    Debug.Write("\n\n");
                }
                if (entryDataListPos_lst_param != null)
                {
                    Debug.Write("Entry Data List Positions:");

                    foreach (int cur_list_pos in entryDataListPos_lst_param)
                    {
                        Debug.Write(" '" + cur_list_pos + "'");
                    }
                    Debug.Write("\n\n");
                }
                if (entryIDs_lst_param != null)
                {
                    Debug.Write("EntryID trunc_titles:");

                    foreach (string cur_entryData_trunc_title in entryIDs_text_lst_param)
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

            entryIDs_lst_param = entryID_lst;
            entryIDs_text_lst_param = entryID_text_lst;


            if (entryIDs_lst_param.Count != entryIDs_lst_param.Distinct().Count())
            {
                Debug.WriteLine("ERROR: there are duplicate entryIDs in the EntryData's");

                //TODO: this will break the program
                int crash_int = Convert.ToInt32("asdf");
            }
        }

        public void CreateEntryDataListPositionList()
        {
            //making the list of list_position's
            List<int> list_position_lst = new List<int>();
            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                list_position_lst.Add(Convert.ToInt32(cur_entryData.ListPosition));
            }


            //just checking to see if there are any duplicate positions

            if (list_position_lst.Count != list_position_lst.Distinct().Count())
            {
                Debug.WriteLine("ERROR: there are duplicate list positions in the EntryData's");
                entryDataListPos_lst_param = null;
                return;
            }

            entryDataListPos_lst_param = list_position_lst;
        }


        public void CreateColIDsList()
        {
            //making the list of colID's
            List<string> colID_lst = new List<string>();
            foreach (ColumnData cur_columnData in ColumnData_lst_param)
            {
                colID_lst.Add(cur_columnData.ColID);
            }
            colIDs_lst_param = colID_lst;


            if (colIDs_lst_param.Count != colIDs_lst_param.Distinct().Count())
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
            //   1) its pre/coreqs actually exist in entryIDs_lst_param
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

                    //if the current prereq does exist in entryIDs_lst_param
                    if (entryIDs_lst_param.Contains(cur_prereq))
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

                    //if the current coreq does exist in entryIDs_lst_param
                    if (entryIDs_lst_param.Contains(cur_coreq))
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

                    //if the current avail colID does exist in colIDs_lst_param
                    if (colIDs_lst_param.Contains(cur_avail_colID))
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

                    //if the current set colID does exist in colIDs_lst_param
                    if (colIDs_lst_param.Contains(cur_set_colID))
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
            EntryData return_EntryData = new EntryData();


            //if the entryID that's given does not exist in the entryID_lst_param we exit
            if (!entryIDs_lst_param.Contains(given_entryID))
            {
                Debug.WriteLine("ERROR: The entryID given, '" + given_entryID + "', does not exist in entryIDs_lst_param");
                return null;
            }

            //this is the EntryData that's being requested, at the same index that its entryID was found in the entryID list
            return_EntryData = EntryData_lst_param[entryIDs_lst_param.IndexOf(given_entryID)];

            //juuuuust in case this didn't work...
            if (return_EntryData.EntryID != given_entryID)
            {
                Debug.WriteLine("ERROR: The entryID given, '" + given_entryID + "', does not match the entryID found in the indexed EntryData, '" + return_EntryData.EntryID + "'");
                return null;
            }


            return return_EntryData;
        }
        public ColumnData GetColumnDataFromColID(string given_colID)
        {
            ColumnData return_ColumnData = new ColumnData();


            //if the colID that's given does not exist in the ColumnData_lst_param we exit
            if (!colIDs_lst_param.Contains(given_colID))
            {
                Debug.WriteLine("ERROR: The colID given, '" + given_colID + "', does not exist in colIDs_lst_param");
                return null;
            }

            //this is the ColumnData that's being requested, at the same index that its colID was found in the colID list
            return_ColumnData = ColumnData_lst_param[colIDs_lst_param.IndexOf(given_colID)];

            //juuuuust in case this didn't work...
            if (return_ColumnData.ColID != given_colID)
            {
                Debug.WriteLine("ERROR: The colID given, '" + given_colID + "', does not match the colID found in the indexed ColumnData, '" + return_ColumnData.ColID + "'");
                return null;
            }


            return return_ColumnData;
        }
        public int GetColumnPositionFromColID(string sent_colID)
        {
            //first gets the referred columndata
            ColumnData cur_columnData = GetColumnDataFromColID(sent_colID);
            return cur_columnData.ColPosition;
        }
        public int GetListPositionFromEntryID(string given_entryID)
        {
            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                string cur_entryID = cur_entryData.EntryID;

                if (cur_entryID == given_entryID)
                {
                    return cur_entryData.ListPosition;
                }
            }

            return -1;
        }


        public EntryData GetEntryDataFromListPosition(int given_list_pos)
        {
            EntryData return_EntryData = new EntryData();


            //if the list_pos that's given does not exist in the entryDataListPos_lst_param we exit
            if (!entryDataListPos_lst_param.Contains(given_list_pos))
            {
                Debug.WriteLine("ERROR: The list_pos given, '" + given_list_pos + "', does not exist in entryDataListPos_lst_param");
                return null;
            }

            //this is the EntryData that's being requested, at the same index that its entryID was found in the entryID list
            return_EntryData = EntryData_lst_param[entryDataListPos_lst_param.IndexOf(given_list_pos)];

            //juuuuust in case this didn't work...
            if (return_EntryData.ListPosition != given_list_pos)
            {
                Debug.WriteLine("ERROR: The list_pos given, '" + given_list_pos + "', does not match the list_pos found in the indexed EntryData, '" + return_EntryData.ListPosition + "'");
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


        public bool IsTitleSubtitleDuplicate(string check_title, string check_subtitle)
        {
            //defaulting the return bool to true
            bool already_exist = true;

            //in this method, all  null  will just be  ""
            if (check_title == null) { check_title = ""; }
            if (check_subtitle == null) { check_subtitle = ""; }


            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                //keeping track if the title or subtitle are matches
                bool same_title = false;
                bool same_subtitle = false;

                //getting the title and subtitle
                string cur_title = cur_entryData.Title;
                string cur_subtitle = cur_entryData.Subtitle;

                //again, no nulls allowed
                if (cur_title == null) { cur_title = ""; }
                if (cur_subtitle == null) { cur_subtitle = ""; }

                //checks if there are matches
                if (check_title == cur_title) { same_title = true; }
                if (check_subtitle == cur_subtitle) { same_subtitle = true; }


                //the duplicate conditions. one for every title unique, one for every title+subtitle pair unique
                bool duplicate_condition_title = same_title;
                bool duplicate_condition_both = same_title & same_subtitle;


                //if the duplicate condition is met
                if (duplicate_condition_title)
                {
                    //then there is a duplicate already there
                    Debug.WriteLine(string.Format("Duplicate     title='{0}'     subtitle='{1}'", check_title, check_subtitle));
                    return already_exist;
                }
            }



            //if gets here, it found no duplicate
            already_exist = false;
            return already_exist;
        }

        public int GetMaxDesignationForEntryIDTitle(string given_trunc_title)
        {

            //just to ensure that there are already entryIDs with this trunctated title
            if (!entryIDs_text_lst_param.Contains(given_trunc_title)) { return -1; }

            Debug.WriteLine("GetMaxDesignationForEntryIDTitle    given_trunc_title = '" + given_trunc_title + "'");

            int max_desig = -1;

            foreach (string cur_entryID in entryIDs_lst_param)
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

                int cur_desig_int;

                //will try to convert it to an int, returning if fails
                bool success = int.TryParse(cur_desig_str, out cur_desig_int);
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

            //updating the list position list
            CreateEntryDataListPositionList();



            return EntryData_lst_param.IndexOf(inserted_entryData);
        }
        public void DeleteEntryDataAtIndex(string delete_entryID)
        {
            //this is how an entrydata is deleted, and subsequent lists are updated accordingly


            //ensuring that the delete_entryID is valid
            if (!entryIDs_lst_param.Contains(delete_entryID)) { return; }


            //this is the list position of the entrydata to be deleted
            int deleting_list_pos = GetListPositionFromEntryID(delete_entryID);


            //now we start by removing it from the main list
            int delete_index = entryIDs_lst_param.IndexOf(delete_entryID);
            EntryData_lst_param.RemoveAt(delete_index);


            //updating the entryID lists
            CreateEntryIDsList();

            //updating the list position list
            CreateEntryDataListPositionList();


            //we now need to go back through the remaining EntryData's to update their list positions
            foreach (EntryData cur_entryData in EntryData_lst_param)
            {
                if (cur_entryData.ListPosition >= deleting_list_pos)
                {
                    cur_entryData.ListPosition -= 1;
                }
            }






        }

    }
}

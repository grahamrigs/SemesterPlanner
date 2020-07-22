using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace SemesterPlanner
{
    public static class MasterClass
    {
        public static ProjectData Cur_ProjectData;
        public static MainPage Cur_MainPage;
        public static OverviewPage Cur_OverviewPage;
        public static PropertiesPage Cur_PropertiesPage;
        public static NavigationViewMain Cur_NavigationViewMain;

        //this is the default titleblock colour
        public static string glo_default_titleblock_colour_hex = "#FFC3C3C3";


        //this is the tag start for entryID
        public static string Glo_Tag_EntryID_Prefix = "entryID|";
        public static string Glo_Tag_ColID_Prefix = "colID|";


        public static void LoadProject(string file_path_str, string proj_file_name_str)
        {
            Cur_ProjectData = new ProjectData(file_path_str, proj_file_name_str);
            Cur_NavigationViewMain.ChangePageTo("navitem_MainPage");
        }






        public static void PrintList(string list_name, List<string> list_to_print)
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
        public static void PrintList_int(string list_name, List<int> list_to_print)
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


        public static SolidColorBrush GetSolidColorBrushFromHex(string hex)
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
        public static string GetHexFromSolidColorBrush(SolidColorBrush given_SCB)
        {
            //colours are fucked in C#, so to get the hex of a solidcolorbrush, there's steps involved

            Color old_colour = given_SCB.Color;

            string hex_value = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", old_colour.A, old_colour.R, old_colour.G, old_colour.B);

            //System.Drawing.Color converted_colour = System.Drawing.Color.FromArgb(old_colour.A, old_colour.R, old_colour.G, old_colour.B);



            //string hexValue = System.Drawing.ColorTranslator.ToHtml(converted_colour);

            return hex_value;
        }
        public static string GetHexFromColorPicker(Windows.UI.Color given_colour)
        {
            //colours are fucked in C#, so to get the hex of a solidcolorbrush, there's steps involved

            string hex_value = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", given_colour.A, given_colour.R, given_colour.G, given_colour.B);

            //System.Drawing.Color converted_colour = System.Drawing.Color.FromArgb(old_colour.A, old_colour.R, old_colour.G, old_colour.B);



            //string hexValue = System.Drawing.ColorTranslator.ToHtml(converted_colour);

            return hex_value;
        }


        public static List<int> CreateDeepCopyOfList_int(List<int> list_to_be_copied)
        {
            List<int> return_lst = new List<int>(list_to_be_copied.Count);

            for (int i = 0; i < return_lst.Count; i++)
            {
                return_lst[i] = list_to_be_copied[i];
            }

            return return_lst;
        }
        public static List<string> CreateDeepCopyOfList_string(List<string> list_to_be_copied)
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
        public static EntryData CreateDeepCopyOfEntryData(EntryData entryData_to_be_copied)
        {
            EntryData return_entryData = new EntryData();

            EquateEntryData(return_entryData, entryData_to_be_copied);

            return return_entryData;
        }

        public static void EquateEntryData(EntryData primary_entryData, EntryData secondary_entryData)
        {
            //due to pointers, this will change the primary entryData to equate the secondary without needing to return it

            Debug.WriteLine("EquateEntryData");

            primary_entryData.Entry_ProjectName = secondary_entryData.Entry_ProjectName;
            primary_entryData.EntryID = secondary_entryData.EntryID;
            primary_entryData.Title = secondary_entryData.Title;
            primary_entryData.Subtitle = secondary_entryData.Subtitle;
            primary_entryData.ColourHex = secondary_entryData.ColourHex;
            primary_entryData.ActualColID = secondary_entryData.ActualColID;
            primary_entryData.SetColID = secondary_entryData.SetColID;

            primary_entryData.RowPosition = secondary_entryData.RowPosition;
            primary_entryData.ColPosition = secondary_entryData.ColPosition;

            primary_entryData.PrereqEntryIDs = CreateDeepCopyOfList_string(secondary_entryData.PrereqEntryIDs);
            primary_entryData.CoreqEntryIDs = CreateDeepCopyOfList_string(secondary_entryData.CoreqEntryIDs);
            primary_entryData.AvailColIDs = CreateDeepCopyOfList_string(secondary_entryData.AvailColIDs);

            primary_entryData.TitleBlock = secondary_entryData.TitleBlock;
            primary_entryData.CalendarBlock = secondary_entryData.CalendarBlock;

            primary_entryData.Is_Selected = secondary_entryData.Is_Selected;


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
    }
}

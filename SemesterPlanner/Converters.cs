using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace SemesterPlanner
{
    class Converters : IValueConverter
    {
        //should be  "entryID|"
        string entryID_prefix = MainPage.glo_tag_entryID_prefix;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string entryID_raw = (string)value;

            string entryID_with_prefix = entryID_prefix + entryID_raw;

            return entryID_with_prefix;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }



    class Converter_StyleNameToStyle : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            //value should be one of  [ "bor_EntryTitleBlock", "bor_EntryTitleBlock_Selected",
            //                          "bor_AddNewEntryPreviewTitleBlock", "bor_AddNewEntryPreviewTitleBlock_Selected" ]

            Style style_ = Application.Current.Resources[(string)value] as Style;

            return style_;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }



    class Converter_BoolToVisibility : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isChecked = (bool)value;
            return isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }



    class Converter_ColIDToBorderTag : IValueConverter
    {
        //should be  "colID|"
        string colID_prefix = MainPage.glo_tag_colID_prefix;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string colID_raw = (string)value;

            string colID_with_prefix = colID_prefix + colID_raw;

            return colID_with_prefix;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }



    class Converter_ColourHexToSolidColorBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            SolidColorBrush solid_colour_brush = MainPage.GetSolidColorBrushFromHex((string)value);

            return solid_colour_brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }



    class Converter_TextBoxChangedToStyle : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            //the value will be a bool for if it's a changed textbox
            //the output style will be from this list  [ "txtbox_Property", "txtbox_Property_Changed" ]

            List<string> txtbox_styles = ChangedProperties.Styles_TextBox_lst;

            bool is_changed_txtbox = (bool)value;
            string output_style_name;

            if (!is_changed_txtbox) { output_style_name = txtbox_styles[0]; }
            else { output_style_name = txtbox_styles[1]; }


            Style style_ = Application.Current.Resources[output_style_name] as Style;

            return style_;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

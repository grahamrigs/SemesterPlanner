using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace SemesterPlanner
{
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace SemesterPlanner
{
    class Converter_EntryIDToBorderTag : IValueConverter
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
}

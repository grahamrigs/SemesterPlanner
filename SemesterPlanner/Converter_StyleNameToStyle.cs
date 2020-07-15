using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace SemesterPlanner
{
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
}

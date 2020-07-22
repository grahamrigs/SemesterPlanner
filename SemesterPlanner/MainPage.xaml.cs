using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Size = Windows.Foundation.Size;

namespace SemesterPlanner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        //contains all currently loaded project data, including metadata, entries, columns
        //ProjectData Glo_ProjectData = new ProjectData();

        //contains all calculated values that may be needed elsewhere
        //CalculatedData glo_CalculatedData = new CalculatedData();

        //this is the currently selected entryID; default/none-selected is -1
        //public string glo_selected_entryID = "";

        //and the currently selected entry, for easy access
        //static EntryData glo_selected_EntryData = new EntryData();

        //this keeps track of if the properties pane is open

        private double column_width_ = 0;
        private double row_height_ = 0;

        public double ColumnWidth 
        { 
            get { return column_width_; }
            set
            {
                if (column_width_ != value)
                {
                    column_width_ = value;
                    UpdateCalendarColRowSize("columns");
                }
            }
        }
        public double RowHeight 
        { 
            get { return row_height_; }
            set
            {
                if (row_height_ != value)
                {
                    row_height_ = value;
                    UpdateCalendarColRowSize("rows");
                }
            }
        }

        //and the currently selected entry, for easy access
        //AddNewEntry glo_AddNewEntry = new AddNewEntry();

        //these will store the values in the properties pane, before and after changes
        //ChangedProperties glo_ChangedProperties = new ChangedProperties();

        //this flag controls if the apply properties needs a verification window
        //bool glo_properties_apply_need_verification = false;


        //these are for handling synchronous scrolling
        private const int ScrollLoopbackTimeout = 500;
        private object _lastScrollingElement;
        private int _lastScrollChange = Environment.TickCount;

        public static int MainPage_Instance_Count = 0;


        public int gridlines_count = 0;



        private ProjectData Glo_ProjectData;



        public MainPage()
        {
            MainPage_Instance_Count++;

            this.InitializeComponent();

            Debug.WriteLine("MainPage instantiated");

            MasterClass.Cur_MainPage = this;
            Glo_ProjectData = MasterClass.Cur_ProjectData;

            txtblc_project_name.DataContext = Glo_ProjectData;

            //glo_CalculatedData.ColourPickerChosenHex = "#FFC3C3C3";

        }
        ~MainPage()
        {
            MainPage_Instance_Count--;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Debug.WriteLine("Clearing border grids");

            grid_column_headers.Children.Clear();
            grid_entry_title_blocks.Children.Clear();
            grid_calendar.Children.Clear();

            base.OnNavigatedFrom(e);
        }






        private void MainPageLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MainPage loaded");

            UpdateAllComponents();
        }









        //these are the overall layout methods

        public void UpdateAllComponents()
        {
            Debug.WriteLine("UpdateAllComponents");

            if (!Glo_ProjectData.ProjectLoaded)
            {
                Debug.WriteLine("  Project not yet loaded. Exiting.");
                return;
            }
            else
            {
                Debug.WriteLine("  Project is loaded. Continuing.");
            }

            UpdateColumnHeaders();
            UpdateEntryTitleBlocks();
            UpdateCalendarLayout();
            UpdateCalendarDisplay();
            Glo_ProjectData.CreateUsedColourList();
        }
        public void UpdateColumnHeaders()
        {

            //gets rid of all previous headers
            grid_column_headers.Children.Clear();

            Debug.WriteLine("Column headers columndefinitions: " + grid_column_headers.ColumnDefinitions.Count());

            CreateCorrectNumberOfGridRowsColumns("column_headers");

            Debug.WriteLine("Column headers columndefinitions: " + grid_column_headers.ColumnDefinitions.Count());


            //this will sit at (0,0) in the grid and expand to fill the first cell
            Border dummy_border_for_width = new Border
            {
                Tag = "dummy|width"
            };

            dummy_border_for_width.SizeChanged += DummyChangedSize;
            grid_column_headers.Children.Add(dummy_border_for_width);


            AddMissingBorders("columns");
            RemoveDeletedColumnHeaders();
        }
        public void UpdateEntryTitleBlocks()
        {

            //gets rid of all previous title blocks
            //grid_entry_title_blocks.Children.Clear();

            Debug.WriteLine("Rowdefinitions: " + grid_entry_title_blocks.RowDefinitions.Count());

            CreateCorrectNumberOfGridRowsColumns("titleblock_rows");

            Debug.WriteLine("Rowdefinitions: " + grid_entry_title_blocks.RowDefinitions.Count());


            //this will sit at (0,0) in the grid and expand to fill the first cell
            Border dummy_border_for_height = new Border
            {
                Tag = "dummy|height"
            };

            dummy_border_for_height.SizeChanged += DummyChangedSize;
            grid_entry_title_blocks.Children.Add(dummy_border_for_height);

            AddMissingBorders("title");
            RemoveDeletedBlocks("title");


        }
        public void UpdateCalendarLayout()
        {

            Debug.WriteLine("Rowdefinitions: " + grid_calendar.RowDefinitions.Count());
            Debug.WriteLine("Columndefinitions: " + grid_calendar.ColumnDefinitions.Count());

            CreateCorrectNumberOfGridRowsColumns("calendar_both");

            Debug.WriteLine("Rowdefinitions: " + grid_calendar.RowDefinitions.Count());
            Debug.WriteLine("Columndefinitions: " + grid_calendar.ColumnDefinitions.Count());

            //UpdateCalendarColRowSize();
            CreateGridlines();
        }
        public void UpdateCalendarDisplay()
        {
            //this method will update the calendar display according to the ActualColID values in each EntryData

            Debug.WriteLine("\nUpdateCalendarDisplay");




            AddMissingBorders("calendar");
            RemoveDeletedBlocks("calendar");

        }
        public void CreateGridlines()
        {
            Debug.WriteLine("gridlines_count = " + gridlines_count);

            //first clear all gridlines that may have been there
            foreach (UIElement cur_control in grid_calendar.Children.ToList())
            {
                if (!(cur_control is Rectangle)) { continue; }

                grid_calendar.Children.Remove(cur_control);
                gridlines_count--;
            }

            Debug.WriteLine("gridlines_count = " + gridlines_count);

            //now create new ones

            int row_count = Glo_ProjectData.EntryData_lst_param.Count();
            int col_count = Glo_ProjectData.ColumnData_lst_param.Count();

            Debug.WriteLine("row_count = " + row_count + "     col_count = " + col_count);

            //rows first
            for (int i = 0; i <= row_count; i++)
            {
                Rectangle cur_hori_gridline = new Rectangle
                {
                    DataContext = Glo_ProjectData
                };
                Grid.SetRow(cur_hori_gridline, i);

                if (i == row_count)
                {
                    cur_hori_gridline.VerticalAlignment = VerticalAlignment.Bottom;
                }


                // Create the style binding
                Binding binding_style = new Binding()
                {
                    Path = new PropertyPath("Gridlines_Hori_Style"),
                    Mode = BindingMode.OneWay,
                    Converter = new Converter_StyleNameToStyle()
                };
                cur_hori_gridline.SetBinding(StyleProperty, binding_style);


                grid_calendar.Children.Add(cur_hori_gridline);

                gridlines_count++;
            }

            //cols now
            for (int j = 0; j <= col_count; j++)
            {
                Rectangle cur_vert_gridline = new Rectangle
                {
                    DataContext = Glo_ProjectData
                };
                Grid.SetColumn(cur_vert_gridline, j);

                if (j == col_count)
                {
                    cur_vert_gridline.HorizontalAlignment = HorizontalAlignment.Right;
                }


                // Create the style binding
                Binding binding_style = new Binding()
                {
                    Path = new PropertyPath("Gridlines_Vert_Style"),
                    Mode = BindingMode.OneWay,
                    Converter = new Converter_StyleNameToStyle()
                };
                cur_vert_gridline.SetBinding(StyleProperty, binding_style);


                grid_calendar.Children.Add(cur_vert_gridline);

                gridlines_count++;
            }

            Debug.WriteLine("gridlines_count = " + gridlines_count);

        }

        private void CreateCorrectNumberOfGridRowsColumns(string grid_selection)
        {
            Grid working_grid = new Grid();

            bool setting_rows = false;
            bool setting_cols = false;


            //based on the method's parameter, will either be adding columns or rows or both, and to a specific grid
            switch (grid_selection)
            {
                case "column_headers":
                    working_grid = grid_column_headers;
                    setting_cols = true;
                    break;

                case "titleblock_rows":
                    working_grid = grid_entry_title_blocks;
                    setting_rows = true;
                    break;

                case "calendar_both":
                    working_grid = grid_calendar;
                    setting_cols = true;
                    setting_rows = true;
                    break;
            }



            if (setting_rows)
            {
                //the amount of rows we want
                int desired_num_of_rows = Glo_ProjectData.EntryData_lst_param.Count();

                //will delete rows until it's the proper size
                while (working_grid.RowDefinitions.Count > desired_num_of_rows)
                {
                    working_grid.RowDefinitions.RemoveAt(desired_num_of_rows);
                }

                //will create rows until it's the proper size
                while (working_grid.RowDefinitions.Count < desired_num_of_rows)
                {
                    RowDefinition new_row_def = new RowDefinition
                    {
                        Height = new GridLength(1, GridUnitType.Auto)
                    };

                    working_grid.RowDefinitions.Add(new_row_def);
                }
            }

            if (setting_cols)
            {
                //the amount of columns we want
                int desired_num_of_cols = Glo_ProjectData.ColumnData_lst_param.Count();


                //will delete columns until it's the proper size
                while (working_grid.ColumnDefinitions.Count > desired_num_of_cols)
                {
                    working_grid.ColumnDefinitions.RemoveAt(desired_num_of_cols);
                }

                //will create columns until it's the proper size
                while (working_grid.ColumnDefinitions.Count < desired_num_of_cols)
                {
                    ColumnDefinition new_col_def = new ColumnDefinition
                    {
                        Width = Glo_ProjectData.HeaderWidth
                    };

                    working_grid.ColumnDefinitions.Add(new_col_def);
                }
            }
        }
        private void DummyChangedSize(object sender, SizeChangedEventArgs e)
        {
            //there is a dummy border in the first row of grid_entry_title_blocks and the first column of grid_column_headers
            //these dummy borders expand to fill their entire cell
            //they also have call this method everytime their size changes so we can update the size of the rows/columns in grid_calendar

            //this is the border
            Border sending_dummy = (Border)sender;

            //they each have different tags to differentiate them
            string tag = sending_dummy.Tag.ToString();

            Debug.WriteLine(string.Format("Dummy changed size: {0}", tag));


            //depending on the tag
            switch (tag)
            {
                case "dummy|width":

                    //get the actual width of the dummy, and save it to the glo_CalculatedData class object
                    ColumnWidth = sending_dummy.ActualWidth;
                    Debug.WriteLine("ColumnWidth = " + ColumnWidth);

                    break;

                case "dummy|height":

                    //get the actual height of the dummy, and save it to the glo_CalculatedData class object
                    RowHeight = sending_dummy.ActualHeight;
                    Debug.WriteLine("RowHeight = " + RowHeight);

                    break;
            }
        }
        private void UpdateCalendarColRowSize(string columns_rows)
        {
            //will go through the calendar columns and rows and adjust them to be the correct size, as determined earlier by the dummies

            switch (columns_rows)
            {
                case "columns":
                    foreach (ColumnDefinition cur_columndefinition in grid_calendar.ColumnDefinitions)
                    {
                        cur_columndefinition.Width = new GridLength(ColumnWidth, GridUnitType.Pixel);
                    }
                    break;

                case "rows":
                    foreach (RowDefinition cur_rowdefinition in grid_calendar.RowDefinitions)
                    {
                        cur_rowdefinition.Height = new GridLength(RowHeight, GridUnitType.Pixel);
                    }
                    break;
            }

        }
        private void SynchronizedScrollerOnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            //found at https://stackoverflow.com/questions/15151974/synchronized-scrolling-of-two-scrollviewers-whenever-any-one-is-scrolled-in-wpf

            if (_lastScrollingElement != sender && Environment.TickCount - _lastScrollChange < ScrollLoopbackTimeout) return;

            _lastScrollingElement = sender;
            _lastScrollChange = Environment.TickCount;

            ScrollViewer sourceScrollViewer;
            ScrollViewer targetScrollViewer_hori;
            ScrollViewer targetScrollViewer_vert;


            //scroll_entry_title_blocks -- scrolls vertically
            //scroll_column_headers     -- scrolls horizontally
            //scroll_calendar           -- scrolls vert + hori


            if (sender == scroll_entry_title_blocks)
            {
                sourceScrollViewer = scroll_entry_title_blocks;
                targetScrollViewer_vert = scroll_calendar;


                targetScrollViewer_vert.ChangeView(null, sourceScrollViewer.VerticalOffset, null);
            }
            else if (sender == scroll_column_headers)
            {
                sourceScrollViewer = scroll_column_headers;
                targetScrollViewer_hori = scroll_calendar;


                targetScrollViewer_hori.ChangeView(sourceScrollViewer.HorizontalOffset, null, null);
            }
            else if (sender == scroll_calendar)
            {
                sourceScrollViewer = scroll_calendar;
                targetScrollViewer_vert = scroll_entry_title_blocks;
                targetScrollViewer_hori = scroll_column_headers;


                targetScrollViewer_vert.ChangeView(null, sourceScrollViewer.VerticalOffset, null);
                targetScrollViewer_hori.ChangeView(sourceScrollViewer.HorizontalOffset, null, null);
            }

        }





        //these methods assist in creating titleblocks, calendarblocks, and columnheaders

        public void AddMissingBorders(string title_calendar_columns)
        {
            //this method will go through all stored EntryData and ColumnData, and if one doesn't have a block, it will add it
            //accepts as input a string containing the words  title  calendar  columns
            //at least one of these words must be in the string, and multiples work  (eg.  "title_calendar" is valid)

            Debug.WriteLine("\nAddMisingBorders: " + title_calendar_columns);


            List<string> create_types_to_do = new List<string>();

            if (title_calendar_columns.Contains("title")) { create_types_to_do.Add("title"); }
            if (title_calendar_columns.Contains("calendar")) { create_types_to_do.Add("calendar"); }
            if (title_calendar_columns.Contains("columns")) { create_types_to_do.Add("columns"); }

            if (create_types_to_do.Count == 0) { return; }



            foreach (string cur_type in create_types_to_do)
            {
                List<string> IDs_in_datalists;
                string ID_type;
                string printname;

                switch (cur_type)
                {
                    case "title":
                        IDs_in_datalists = Glo_ProjectData.EntryIDs_lst;
                        ID_type = "entryID";
                        printname = "TitleBlocks";

                        break;

                    case "calendar":
                        IDs_in_datalists = Glo_ProjectData.EntryIDs_lst;
                        ID_type = "entryID";
                        printname = "CalendarBlocks";

                        break;

                    case "columns":
                        IDs_in_datalists = Glo_ProjectData.ColIDs_lst;
                        ID_type = "colID";
                        printname = "ColumnHeaders";

                        break;

                    default:
                        continue;
                }


                List<string> tags_in_borders = CreateBlockTagList_IndexOrder(cur_type);
                List<string> IDs_in_borders = ExtractEntryColIDsFromTagList(tags_in_borders, true, ID_type);
                List<string> IDs_in_datalists_not_in_borders = IDs_in_datalists.Except(IDs_in_borders).ToList();

                Debug.WriteLine(printname + " to add: " + IDs_in_datalists_not_in_borders.Count);

                foreach (string cur_adding_ID in IDs_in_datalists_not_in_borders)
                {
                    Debug.WriteLine("\nAdding: " + cur_adding_ID);

                    EntryData cur_entryData;
                    ColumnData cur_columnData;

                    switch (cur_type)
                    {
                        case "title":
                            cur_entryData = Glo_ProjectData.GetEntryDataFromEntryID(cur_adding_ID);
                            CreateAndPlaceNewTitleBlock(cur_entryData);
                            break;

                        case "calendar":
                            cur_entryData = Glo_ProjectData.GetEntryDataFromEntryID(cur_adding_ID);
                            CreateAndPlaceNewCalendarBlock(cur_entryData);
                            break;

                        case "columns":
                            cur_columnData = Glo_ProjectData.GetColumnDataFromColID(cur_adding_ID);
                            CreateAndPlaceNewColumnHeader(cur_columnData);
                            break;
                    }
                }




            }






        }





        //these methods create the column headers

        public void RemoveDeletedColumnHeaders()
        {

        }
        private void CreateAndPlaceNewColumnHeader(ColumnData creating_columnData)
        {
            Border new_header;

            if (creating_columnData.ColumnHeader == null)
            {
                Debug.WriteLine("  Creating new ColumnHeader");

                new_header = CreateColumnHeader(creating_columnData);
                creating_columnData.ColumnHeader = new_header;
            }
            else
            {
                Debug.WriteLine("  Reusing old ColumnHeader");

                new_header = creating_columnData.ColumnHeader;
            }

            grid_column_headers.Children.Add(new_header);

        }
        private Border CreateColumnHeader(ColumnData cur_columndata)
        {          

            Border outer_border = new Border();

            TextBlock txtblc_title = new TextBlock();




            // Set the DataContext of the border
            outer_border.DataContext = cur_columndata;




            // Create the colID tag binding
            Binding binding_ID_tag = new Binding()
            {
                Path = new PropertyPath("ColID"),
                Mode = BindingMode.OneWay,
                Converter = new Converter_ColIDToBorderTag()
            };
            outer_border.SetBinding(TagProperty, binding_ID_tag);


            // Create the title binding
            Binding binding_title = new Binding()
            {
                Path = new PropertyPath("ColTitle"),
                Mode = BindingMode.OneWay
            };
            txtblc_title.SetBinding(TextBlock.TextProperty, binding_title);


            // Create the column binding
            Binding binding_column = new Binding()
            {
                Path = new PropertyPath("ColPosition"),
                Mode = BindingMode.OneWay
            };
            outer_border.SetBinding(Grid.ColumnProperty, binding_column);








            outer_border.Child = txtblc_title;

            outer_border.Height = 32;

            txtblc_title.HorizontalAlignment = HorizontalAlignment.Center;
            txtblc_title.VerticalAlignment = VerticalAlignment.Center;


            return outer_border;



        }





        //these methods create the entry titleblocks

        public void RemoveDeletedBlocks(string title_calendar_both)
        {
            //this method will go through all stored titleblocks and calendarblocks, 
            //  and if one doesn't have a corresponding EntryData, it will remove it

            Debug.WriteLine("\nRemoveDeletedBlocks: " + title_calendar_both);

            List<string> entryIDs_in_datalists = Glo_ProjectData.EntryIDs_lst;

            //this removes the titleblocks
            if (title_calendar_both == "title" || title_calendar_both == "both")
            {
                //this just gets the tags from all the titleblocks
                List<string> tags_in_titleblocks = CreateBlockTagList_IndexOrder("title");

                //this method will strip out all non-entryID values
                List<string> entryIDs_in_titleblocks = ExtractEntryColIDsFromTagList(tags_in_titleblocks, true, "entryID");

                //this will determine which entryIDs exist in the block list but not in the datalist
                List<string> entryIDs_in_titleblocks_not_in_datalists =
                    entryIDs_in_titleblocks.Except(entryIDs_in_datalists).ToList();

                //PrintList("tags_in_titleblocks", tags_in_titleblocks);
                //PrintList("entryIDs_in_titleblocks", entryIDs_in_titleblocks);
                //PrintList("entryIDs_in_datalists", entryIDs_in_datalists);
                //PrintList("entryIDs_in_titleblocks_not_in_datalists", entryIDs_in_titleblocks_not_in_datalists);

                Debug.WriteLine("Titleblocks to delete: " + entryIDs_in_titleblocks_not_in_datalists.Count);

                //removing from the grid.children is hard
                //solution here  https://codeaddiction.net/articles/7/remove-items-while-iterating-a-collection-in-c

                //now to remove the blocks from the titleblocks pane
                foreach (Border cur_border in grid_entry_title_blocks.Children.OfType<Border>().ToList())
                {
                    //the tag info from the border
                    List<string> tag_info = ExtractTagInfo(cur_border.Tag.ToString());

                    //if the tag starts with  entryID|  and then checks if the  entryID is in the removing list
                    if (tag_info[0] == MasterClass.Glo_Tag_EntryID_Prefix.Trim('|') && 
                        entryIDs_in_titleblocks_not_in_datalists.Contains(tag_info[1]))
                    {
                        grid_entry_title_blocks.Children.Remove(cur_border);
                    }
                }
            }

            //this removes the calendarblocks (works the exact same as above)
            if (title_calendar_both == "calendar" || title_calendar_both == "both")
            {
                List<string> tags_in_calendarblocks = CreateBlockTagList_IndexOrder("calendar");
                List<string> entryIDs_in_calendarblocks = ExtractEntryColIDsFromTagList(tags_in_calendarblocks, true, "entryID");
                List<string> entryIDs_in_calendarblocks_not_in_datalists =
                    entryIDs_in_calendarblocks.Except(entryIDs_in_datalists).ToList();

                Debug.WriteLine("Calendarblocks to delete: " + entryIDs_in_calendarblocks_not_in_datalists.Count);

                foreach (Border cur_border in grid_calendar.Children.OfType<Border>().ToList())
                {
                    List<string> tag_info = ExtractTagInfo(cur_border.Tag.ToString());

                    if (tag_info[0] == MasterClass.Glo_Tag_EntryID_Prefix.Trim('|') && 
                        entryIDs_in_calendarblocks_not_in_datalists.Contains(tag_info[1]))
                    {
                        grid_calendar.Children.Remove(cur_border);
                    }
                }
            }
        }
        private void CreateAndPlaceNewTitleBlock(EntryData creating_entryData)
        {
            Border new_titleblock;

            if (creating_entryData.TitleBlock == null)
            {
                Debug.WriteLine("  Creating new TitleBlock");

                new_titleblock = CreateEntryTitleBlock(creating_entryData, false);
                creating_entryData.TitleBlock = new_titleblock;
            }
            else
            {
                Debug.WriteLine("  Reusing old TitleBlock");

                new_titleblock = creating_entryData.TitleBlock;
            }

            grid_entry_title_blocks.Children.Add(new_titleblock);
        }
        private Border CreateEntryTitleBlock(EntryData cur_entryData, bool for_preview)
        {

            Border outer_border = new Border();

            StackPanel name_stack = new StackPanel
            {
                Style = Application.Current.Resources["stack_EntryTitleBlockTitleStack_Hori"] as Style
            };

            TextBlock txtblc_title = new TextBlock
            {
                Style = Application.Current.Resources["txtblc_EntryTitleBlockTitle"] as Style
            };

            TextBlock txtblc_separator = new TextBlock
            {
                Style = Application.Current.Resources["txtblc_EntryTitleBlockSeparator_Hori"] as Style
            };

            TextBlock txtblc_subtitle = new TextBlock
            {
                Style = Application.Current.Resources["txtblc_EntryTitleBlockSubtitle_Hori"] as Style
            };



            // Set the DataContext of the border
            if (!for_preview)
            {
                outer_border.DataContext = cur_entryData;
            }
            else
            {
                outer_border.DataContext = Glo_ProjectData.Glo_AddNewEntry;
            }


            //the data bindings



            //these bindings are only applicable to the real titleblocks
            if (!for_preview)
            {
                // Create the entryID tag binding
                Binding binding_ID_tag = new Binding()
                {
                    Path = new PropertyPath("EntryID"),
                    Mode = BindingMode.OneWay,
                    Converter = new Converter_EntryIDToBorderTag()
                };
                outer_border.SetBinding(TagProperty, binding_ID_tag);


                // Create the row binding
                Binding binding_row = new Binding()
                {
                    Path = new PropertyPath("RowPosition"),
                    Mode = BindingMode.OneWay
                };
                outer_border.SetBinding(Grid.RowProperty, binding_row);
            }
            else
            {
                outer_border.Tag = "preview";
            }


            //these bindings are for both types, but with different parameter names
            string path_style;
            string path_colourhex;
            string path_title;
            string path_subtitle;

            if (!for_preview)
            {
                path_style = "StyleName_Title";
                path_colourhex = "ColourHex";
                path_title = "Title";
                path_subtitle = "Subtitle";
            }
            else
            {
                path_style = "StyleName_Preview";
                path_colourhex = "ColourHex_Add";
                path_title = "Title_Add";
                path_subtitle = "Subtitle_Add";
            }



            // Create the style binding
            Binding binding_style = new Binding()
            {
                Path = new PropertyPath(path_style),
                Mode = BindingMode.OneWay,
                Converter = new Converter_StyleNameToStyle()
            };
            outer_border.SetBinding(StyleProperty, binding_style);


            // Create the colour binding
            Binding binding_colour = new Binding()
            {
                Path = new PropertyPath(path_colourhex),
                Mode = BindingMode.OneWay,
                Converter = new Converter_ColourHexToSolidColorBrush()
            };
            outer_border.SetBinding(BackgroundProperty, binding_colour);


            // Create the title binding
            Binding binding_title = new Binding()
            {
                Path = new PropertyPath(path_title),
                Mode = BindingMode.OneWay
            };
            txtblc_title.SetBinding(TextBlock.TextProperty, binding_title);


            // Create the subtitle binding
            Binding binding_subtitle = new Binding()
            {
                Path = new PropertyPath(path_subtitle),
                Mode = BindingMode.OneWay
            };
            txtblc_subtitle.SetBinding(TextBlock.TextProperty, binding_subtitle);







            if (!for_preview)
            {
                outer_border.RightTapped += TitleCalendarBlockRightTapped;
                outer_border.DoubleTapped += TitleCalendarBlockDoubleTapped;

                outer_border.ContextFlyout = CreateTitleCalendarBlockFlyout();

                outer_border.CanDrag = true;
                outer_border.AllowDrop = true;

                outer_border.Drop += Drop_OnTitleBlock;
            }
            outer_border.Tapped += TitleCalendarBlockTapped;


            bool is_title = (cur_entryData.Title.Length > 0);
            bool is_subtitle = (cur_entryData.Subtitle.Length > 0);

            if (!is_title | !is_subtitle)
            {
                txtblc_separator.Text = "";
            }

            name_stack.Children.Add(txtblc_title);
            name_stack.Children.Add(txtblc_separator);
            name_stack.Children.Add(txtblc_subtitle);

            outer_border.Child = name_stack;

            return outer_border;

        }
        private MenuFlyout CreateTitleCalendarBlockFlyout()
        {
            MenuFlyout context_menu_border = new MenuFlyout();



            MenuFlyoutItem menu_item_moveup = new MenuFlyoutItem
            {
                Text = "Move up"
            };
            FontIcon moveup_icon = new FontIcon
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = "\xE110"
            };
            menu_item_moveup.Icon = moveup_icon;
            context_menu_border.Items.Add(menu_item_moveup);



            MenuFlyoutItem menu_item_movedown = new MenuFlyoutItem
            {
                Text = "Move down"
            };
            FontIcon movedown_icon = new FontIcon
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = "\xE74B"
            };
            menu_item_movedown.Icon = movedown_icon;
            context_menu_border.Items.Add(menu_item_movedown);



            MenuFlyoutSeparator menu_sep = new MenuFlyoutSeparator();
            context_menu_border.Items.Add(menu_sep);



            MenuFlyoutItem menu_item_delete = new MenuFlyoutItem
            {
                Text = "Delete",
                Icon = new SymbolIcon(Symbol.Delete)
            };
            menu_item_delete.Tapped += PropertiesButtonTapped;
            context_menu_border.Items.Add(menu_item_delete);



            MenuFlyoutItem menu_item_properties = new MenuFlyoutItem
            {
                //menu_item_properties.Click += PropertiesButtonTapped;
                Text = "Properties"
            };
            FontIcon prop_icon = new FontIcon
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = "\xE946"
            };
            menu_item_properties.Icon = prop_icon;
            menu_item_properties.Tapped += PropertiesButtonTapped;
            context_menu_border.Items.Add(menu_item_properties);



            return context_menu_border;
        }





        //these methods create the entry calendarblocks

        private void CreateAndPlaceNewCalendarBlock(EntryData creating_entryData)
        {
            Border new_calendar_block;

            if (creating_entryData.CalendarBlock == null)
            {
                Debug.WriteLine("  Creating new CalendarBlock");

                new_calendar_block = CreateCalendarBlock(creating_entryData);
                creating_entryData.CalendarBlock = new_calendar_block;
            }
            else
            {
                Debug.WriteLine("  Reusing old CalendarBlock");

                new_calendar_block = creating_entryData.CalendarBlock;
            }
            
            grid_calendar.Children.Add(new_calendar_block);
        }
        private Border CreateCalendarBlock(EntryData cur_entryData)
        {
            Border outer_border = new Border();

            StackPanel name_stack = new StackPanel
            {
                Style = Application.Current.Resources["stack_EntryTitleBlockTitleStack_Hori"] as Style
            };

            TextBlock txtblc_title = new TextBlock
            {
                Style = Application.Current.Resources["txtblc_CalendarBlockTitle"] as Style
            };







            // Set the DataContext of the border
            outer_border.DataContext = cur_entryData;




            // Create the entryID tag binding
            Binding binding_ID_tag = new Binding()
            {
                Path = new PropertyPath("EntryID"),
                Mode = BindingMode.OneWay,
                Converter = new Converter_EntryIDToBorderTag()
            };
            outer_border.SetBinding(TagProperty, binding_ID_tag);


            // Create the style binding
            Binding binding_style = new Binding()
            {
                Path = new PropertyPath("StyleName_Calendar"),
                Mode = BindingMode.OneWay,
                Converter = new Converter_StyleNameToStyle()
            };
            outer_border.SetBinding(StyleProperty, binding_style);


            // Create the colour binding
            Binding binding_colour = new Binding()
            {
                Path = new PropertyPath("ColourHex"),
                Mode = BindingMode.OneWay,
                Converter = new Converter_ColourHexToSolidColorBrush()
            };
            outer_border.SetBinding(BackgroundProperty, binding_colour);


            // Create the title binding
            Binding binding_title = new Binding()
            {
                Path = new PropertyPath("Title"),
                Mode = BindingMode.OneWay
            };
            txtblc_title.SetBinding(TextBlock.TextProperty, binding_title);


            // Create the row binding
            Binding binding_row = new Binding()
            {
                Path = new PropertyPath("RowPosition"),
                Mode = BindingMode.OneWay
            };
            outer_border.SetBinding(Grid.RowProperty, binding_row);

            // Create the column binding
            Binding binding_column = new Binding()
            {
                Path = new PropertyPath("ColPosition"),
                Mode = BindingMode.OneWay
            };
            outer_border.SetBinding(Grid.ColumnProperty, binding_column);





            outer_border.RightTapped += TitleCalendarBlockRightTapped;
            outer_border.Tapped += TitleCalendarBlockTapped;
            outer_border.DoubleTapped += TitleCalendarBlockDoubleTapped;

            outer_border.ContextFlyout = CreateTitleCalendarBlockFlyout();


            name_stack.Children.Add(txtblc_title);
            outer_border.Child = name_stack;


            return outer_border;
        }





        //these are utilities for the titleblocks and others

        public List<string> CreateBlockTagList_IndexOrder(string title_calendar)
        {
            //this is a little function that will go through either the titleblocks or calendar blocks
            //  and return a list of its tags (ideally entryID) in the correct index

            List<string> return_list = new List<string>();

            Grid working_grid;

            switch (title_calendar)
            {
                case "title":
                    working_grid = grid_entry_title_blocks;
                    break;

                case "calendar":
                    working_grid = grid_calendar;
                    break;

                case "columns":
                    working_grid = grid_column_headers;
                    break;

                default:
                    return return_list;
            }



            foreach (Border cur_border in working_grid.Children.OfType<Border>())
            {
                return_list.Add(cur_border.Tag.ToString());
            }


            return return_list;
        }
        public List<string> ExtractEntryColIDsFromTagList(List<string> tag_list, bool strip_non_entryIDs, string entryID_or_colID)
        {
            //this method will go through the list of tags and only include the entryID values
            //if strip_non_entryIDs it will delete that indice completely, otherwise that indice will be ""
            //  in order to keep the correct index order


            string cur_global_tag;
            if (entryID_or_colID == "entryID")
            {
                cur_global_tag = MasterClass.Glo_Tag_EntryID_Prefix.Trim('|');
            }
            else if (entryID_or_colID == "colID")
            {
                cur_global_tag = MasterClass.Glo_Tag_ColID_Prefix.Trim('|');
            }
            else
            {
                return new List<string>();
            }



            List<string> return_list = new List<string>();


            foreach (string cur_tag in tag_list)
            {
                List<string> tag_split = ExtractTagInfo(cur_tag);

                bool is_entryID = (tag_split[0] == cur_global_tag);

                if (is_entryID)
                {
                    return_list.Add(tag_split[1]);
                }
                else if (strip_non_entryIDs)
                {
                    //do not put it in
                }
                else
                {
                    return_list.Add("");
                }
            }

            return return_list;
        }
        public void FixBlockInfo(bool reset_names, bool reset_titles, bool reset_subtitles, bool reset_row_pos, bool reset_colours)
        {
            //this function will go through the title and calendar blocks and fix whatever data is specified

            Debug.WriteLine("FixBlockInfo");

            List<string> properties_to_update = new List<string>();

            if (reset_names) { properties_to_update.Add("Names"); }
            if (reset_titles) { properties_to_update.Add("Titles"); }
            if (reset_subtitles) { properties_to_update.Add("Subtitles"); }
            if (reset_row_pos) { properties_to_update.Add("row_pos"); }
            if (reset_colours) { properties_to_update.Add("Colours"); }

            foreach (string cur_prop_to_update in properties_to_update)
            {
                Debug.WriteLine("    Updating: " + cur_prop_to_update);
            }
            Debug.WriteLine("");


            //begin with the titleblocks
            foreach (Border cur_titleblock in grid_entry_title_blocks.Children.OfType<Border>())
            {

                //skips any null
                if (cur_titleblock == null) { continue; }


                //the tag information
                List<string> tag_extracted = ExtractTagInfo(cur_titleblock.Tag.ToString());

                //skipping the dummy and other, only wanting entryID
                if (tag_extracted[0] != "entryID") { continue; }

                //the entryID
                string cur_title_entryID = tag_extracted[1];

                //grabbing the EntryData using the built-in method
                EntryData cur_title_EntryData = Glo_ProjectData.GetEntryDataFromEntryID(cur_title_entryID);


                //this is the structure of the titleblock
                /*
                
                <Border>  ------------------------  this has the entryID as its tag, and the Grid.Row property, the background colour, and the border name
                    <Grid>  ----------------------
                        <StackPanel>  ------------
                            <TextBlock/>  --------  this is the title
                            <TextBlock/>  --------  this is the separator
                            <TextBlock/>  --------  this is the subtitle
                        </StackPanel>  -----------
                    </Grid>  ---------------------
                </Border>  -----------------------

                */


                if (reset_names)
                {
                    cur_titleblock.Name = "bor_titleblock_dataID_" + cur_title_EntryData.EntryID;
                }
                if (reset_row_pos)
                {
                    Grid.SetRow(cur_titleblock, Convert.ToInt32(cur_title_EntryData.RowPosition));
                }
                if (reset_colours)
                {
                    cur_titleblock.Background = MasterClass.GetSolidColorBrushFromHex(cur_title_EntryData.ColourHex);
                }
                if (reset_titles || reset_subtitles)
                {
                    Grid cur_inner_grid = cur_titleblock.Child as Grid;
                    StackPanel cur_inner_stack = cur_inner_grid.Children[0] as StackPanel;

                    if (reset_titles)
                    {
                        TextBlock cur_txtblc_title = cur_inner_stack.Children[0] as TextBlock;
                        cur_txtblc_title.Text = cur_title_EntryData.Title;
                    }
                    if (reset_subtitles)
                    {
                        TextBlock cur_txtblc_subtitle = cur_inner_stack.Children[2] as TextBlock;
                        cur_txtblc_subtitle.Text = cur_title_EntryData.Subtitle;
                    }
                }
            }

            //now do the calendarblocks
            foreach (Border cur_calendarblock in grid_calendar.Children.OfType<Border>())
            {

                //skips any null
                if (cur_calendarblock == null) { continue; }


                //the tag information
                List<string> tag_extracted = ExtractTagInfo(cur_calendarblock.Tag.ToString());

                //skipping the dummy and other, only wanting entryID
                if (tag_extracted[0] != "entryID") { continue; }

                //the entryID
                string cur_calendar_entryID = tag_extracted[1];

                //grabbing the EntryData using the built-in method
                EntryData cur_calendar_EntryData = Glo_ProjectData.GetEntryDataFromEntryID(cur_calendar_entryID);


                //this is the structure of the calendarblock
                /*
                
                <Border>  ------------------------  this has the entryID as its tag, and the Grid.Row property, the background colour, and the border name
                    <TextBlock/>  ----------------  this is the title
                </Border>  -----------------------
                
                */


                if (reset_names)
                {
                    cur_calendarblock.Name = "bor_titleblock_dataID_" + cur_calendar_EntryData.EntryID;
                }
                if (reset_row_pos)
                {
                    Grid.SetRow(cur_calendarblock, cur_calendar_EntryData.RowPosition);
                }
                if (reset_colours)
                {
                    cur_calendarblock.Background = MasterClass.GetSolidColorBrushFromHex(cur_calendar_EntryData.ColourHex);
                }
                if (reset_titles)
                {
                    TextBlock cur_txtblc_title = cur_calendarblock.Child as TextBlock;
                    cur_txtblc_title.Text = cur_calendar_EntryData.Title;
                }
            }

        }
        public void FixSingleTitleCalendarBlock(string fixing_entryID, string title_calendar)
        {
            //this method will find the titleblock and/or calendarblock of the given entryID and update its fields according to its EntryData
            //accepts as input a string containing the words  title  calendar
            //at least one of these words must be in the string, and multiples work  (eg.  "title_calendar" is valid)

            Debug.WriteLine("\nFixSingleTitleCalendarBlock:     fixing_entryID = '" + fixing_entryID + "'    type = " + title_calendar);


            if (fixing_entryID == null || fixing_entryID == "" || 
                !Glo_ProjectData.EntryIDs_lst.Contains(fixing_entryID))
            {
                Debug.WriteLine("ERROR: invalid entryID. Exiting.");
                return;
            }


            List<string> create_types_to_do = new List<string>();

            if (title_calendar.Contains("title")) { create_types_to_do.Add("title"); }
            if (title_calendar.Contains("calendar")) { create_types_to_do.Add("calendar"); }

            if (create_types_to_do.Count == 0) { return; }


            //the entrydata being used to fix
            EntryData fixing_entryData = Glo_ProjectData.GetEntryDataFromEntryID(fixing_entryID);


            //will go through each type requested, one by one
            foreach (string cur_type in create_types_to_do)
            {

                //the border and the textblocks
                Border fixing_border = new Border();
                TextBlock txtblc_title = new TextBlock();
                TextBlock txtblc_subtitle = new TextBlock();    //will not be assigned if calendarblock


                //the switch will choose the correct controls depending on  title_calendar
                switch (cur_type)
                {
                    case "title":
                        fixing_border = fixing_entryData.TitleBlock;


                        //this is the structure of the titleblock
                        /*

                        <Border>  ------------------------  this has the entryID as its tag, and the Grid.Row property, the background colour, and the border name
                            <Grid>  ----------------------
                                <StackPanel>  ------------
                                    <TextBlock/>  --------  this is the title
                                    <TextBlock/>  --------  this is the separator
                                    <TextBlock/>  --------  this is the subtitle
                                </StackPanel>  -----------
                            </Grid>  ---------------------
                        </Border>  -----------------------

                        */

                        Grid inner_grid = fixing_border.Child as Grid;
                        StackPanel inner_stack = inner_grid.Children[0] as StackPanel;
                        txtblc_title = inner_stack.Children[0] as TextBlock;
                        txtblc_subtitle = inner_stack.Children[2] as TextBlock;

                        break;

                    case "calendar":
                        fixing_border = fixing_entryData.CalendarBlock;


                        //this is the structure of the calendarblock
                        /*

                        <Border>  ------------  this has the entryID as its tag, and the Grid.Row and Grid.Column properties, and the background colour
                            <TextBlock/>  ----  this is the title
                        </Border>  -----------

                        */

                        txtblc_title = fixing_border.Child as TextBlock;

                        break;

                    default:
                        continue;
                }


                //the data that will be put into the border
                string data_title = fixing_entryData.Title;
                string data_subtitle = fixing_entryData.Subtitle;
                string data_colour_hex = fixing_entryData.ColourHex;

                int data_entry_row = fixing_entryData.RowPosition;
                int data_entry_col;



                //now to apply the data

                //the entryID
                fixing_border.Tag = MasterClass.Glo_Tag_EntryID_Prefix + fixing_entryID;

                //the colour
                if (data_colour_hex != null && data_colour_hex.Length == 9)
                    fixing_border.Background = MasterClass.GetSolidColorBrushFromHex(data_colour_hex);

                //the title
                if (data_title != null && data_title.Length > 0)
                    txtblc_title.Text = data_title;

                //the row
                if (data_entry_row != -1)
                    Grid.SetRow(fixing_border, data_entry_row);


                if (cur_type == "title")
                {
                    //the subtitle
                    if (cur_type == "title" && data_subtitle != null && data_subtitle.Length > 0)
                        txtblc_subtitle.Text = data_subtitle;
                }

                if (cur_type == "calendar")
                {
                    //the corresponding ColumnData
                    string fixing_colID = fixing_entryData.ActualColID;
                    ColumnData cur_columnData = Glo_ProjectData.GetColumnDataFromColID(fixing_colID);

                    data_entry_col = cur_columnData.ColPosition;

                    //the column
                    if (data_entry_col != -1)
                        Grid.SetRow(fixing_border, fixing_entryData.RowPosition);
                }


            }

        }


        private bool CheckIfAllTitleBlocksValid()
        {
            Debug.WriteLine("CheckIfAllTitleblocksPresent");

            //this method will go through the titleblocks and determine if each has a valid entryID, and that all EntryData's are represented

            //a list of "false" that is a parallel to the EntryData list
            bool[] saw_entry = new bool[Glo_ProjectData.EntryData_lst_param.Count()];

            //will go through every border and match the entryID to the EntryData
            foreach (Border cur_border1 in grid_entry_title_blocks.Children.OfType<Border>())
            {
                //Debug.WriteLine("cur_border1.Tag = '" + cur_border1.Tag + "'");


                //the tag information
                List<string> tag_extracted = ExtractTagInfo(cur_border1.Tag.ToString());

                //skipping the dummy and other, only wanting entryID
                if (tag_extracted[0] != "entryID") { continue; }

                //the entryID
                string cur_entryID1 = tag_extracted[1];



                //checking to see if the entryID is in the indexing list
                if (Glo_ProjectData.EntryIDs_lst.Contains(cur_entryID1))
                {
                    //if it is, then we find this index
                    int cur_index1 = Glo_ProjectData.EntryIDs_lst.IndexOf(cur_entryID1);

                    //and get the corresponding EntryData
                    EntryData cur_entryData1 = Glo_ProjectData.EntryData_lst_param[cur_index1];

                    //now to check just to make sure that the border's entryID, matched with the index list, and matched with the corresponding EntryData
                    if (cur_entryID1 == cur_entryData1.EntryID)
                    {
                        saw_entry[cur_index1] = true;
                    }
                    else
                    {
                        //TODO
                        Debug.WriteLine("ERROR: There an inconsistency between  Glo_ProjectData.EntryData_lst_param  and  Glo_ProjectData.EntryIDs_lst  with entryID '" + cur_entryID1 + "'");
                        return false;
                    }
                }

                //if the entryID is NOT in the indexing list...
                else
                {
                    //TODO
                    Grid inner_grid = cur_border1.Child as Grid;
                    StackPanel inner_stack = inner_grid.Children[0] as StackPanel;
                    TextBlock title_txtblc = inner_stack.Children[0] as TextBlock;

                    Debug.WriteLine(string.Format("ERROR: The titleblock '{0}' (entryID: {1}) does not exist in  Glo_ProjectData.EntryIDs_lst", title_txtblc.Text, cur_entryID1));
                    return false;
                }
            }


            //now if all went well, then saw_entry should be the same length as  Glo_ProjectData.EntryData_lst_param  and be filled with true's
            //if it has any false's left then those are EntryData without any corresponding border

            if (saw_entry.Contains(false))
            {
                //TODO
                Debug.WriteLine("ERROR: There are EntryData's in  Glo_ProjectData.EntryData_lst_param  that do not have a corresponding titleblock");
                return false;
            }

            //if here, then all titleblocks are good :)
            Debug.WriteLine("CheckIfAllTitleBlocksValid = true");

            return true;
        }
        private List<string> ExtractTagInfo(string given_tag)
        {
            List<string> split_tag = given_tag.Split('|').ToList();

            return split_tag;
        }





        //these methods calculate the logic in the calendar display





        //these methods deal with tapping on titleblocks and calendarblocks

        private void TitleCalendarBlockTapped(object sender, TappedRoutedEventArgs e)
        {
            //this is the border if it was the tap_grid
            //Grid sending_grid = (Grid)sender;
            //Grid parent_grid = sending_grid.Parent as Grid;
            //Border sending_border = parent_grid.Parent as Border;



            //if it's just the border itself
            Border sending_border = (Border)sender;
            string sending_tag = sending_border.Tag.ToString();


            if (sending_tag == "preview")
            {
                Glo_ProjectData.SelectAddNewEntry();
                return;
            }


            //the tag information
            List<string> tag_extracted = ExtractTagInfo(sending_tag.ToString());

            //skipping the dummy and other, only wanting entryID
            if (tag_extracted[0] != "entryID") { return; }

            //the entryID of the border that was tapped
            string tapped_entryID = tag_extracted[1];


            //this will determine if the selection needs to be changed, and to change it if it is
            Glo_ProjectData.NewTappedSelection(tapped_entryID);
            //Glo_ProjectData.Glo_Selected_EntryID = tapped_entryID;

        }
        private void TitleCalendarBlockRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            //this is the border
            Border sending_border = (Border)sender;
            string sending_tag = sending_border.Tag.ToString();


            if (sending_tag == "preview")
            {
                Glo_ProjectData.SelectAddNewEntry();
                return;
            }


            //the tag information
            List<string> tag_extracted = ExtractTagInfo(sending_tag.ToString());

            //skipping the dummy and other, only wanting entryID
            if (tag_extracted[0] != "entryID") { return; }

            //the entryID of the border that was tapped
            string tapped_entryID = tag_extracted[1];


            //this will determine if the selection needs to be changed, and to change it if it is
            Glo_ProjectData.NewTappedSelection(tapped_entryID);
            //Glo_ProjectData.Glo_Selected_EntryID = tapped_entryID;
        }
        private void TitleCalendarBlockDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Debug.WriteLine("TitleBlockDoubleTapped");
            //await OpenPropertiesWindow();

            Glo_ProjectData.Glo_PropertyPaneOpen = true;

        }
        private void TitleBlockScrollViewerTapped(object sender, TappedRoutedEventArgs e)
        {
            //UnselectEntryData();
        }


        public async Task<string> ChangeSelectionVerified()
        {
            //this method will check if the properties pane is open with changed values
            //if so, it will ask the user if he wishes to change selection, and either save or discard changes

            //TODO make this method ask the user if they wish to change selection if the properties open


            //if the properties pane isn't open we don't need verification
            if (!Glo_ProjectData.Glo_PropertyPaneOpen) { return "change_not-open"; }


            //if there are no changes, we don't need verification
            if (!Glo_ProjectData.Glo_ChangedProperties.AnyProperties_Changed) { return "change_no-changes"; }



            ContentDialog contentdialog_selectionchange_properties = new ContentDialog
            {
                Title = "Save Changes and Change Selection?",
                Content = "Do you wish to save your unsaved property changes and change selection?",
                PrimaryButtonText = "Save and Change",
                SecondaryButtonText = "Discard and Change",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await contentdialog_selectionchange_properties.ShowAsync();


            if (result == ContentDialogResult.Primary)
            {


                return "change_save";
            }
            else if (result == ContentDialogResult.Secondary)
            {


                return "change_discard";
            }
            else
            {
                return "cancel";
            }


        }

        public void Drop_OnTitleBlock(object sender, DragEventArgs e)
        {
            Border dropped_on_border = sender as Border;
            Debug.WriteLine("Drop_OnTitleBlock     tag = '" + dropped_on_border.Tag.ToString() + "'");
        }




        //these methods deal with the tapping of the titleblock pane buttons

        public void EnableTitleBlockButtons()
        {
            bool blank_selection = false;

            if (Glo_ProjectData.Glo_Selected_EntryID == "")
            {
                blank_selection = true;
            }



            if (blank_selection)
            {
                Debug.WriteLine("Disabling buttons");

                btn_delete.IsEnabled = false;
                btn_properties.IsEnabled = false;

                btn_clear_selection.IsEnabled = false;

                btn_move_up.IsEnabled = false;
                btn_move_down.IsEnabled = false;

                return;
            }


            //will continue if we want enabled
            Debug.WriteLine("Enabling buttons");

            btn_delete.IsEnabled = true;
            btn_properties.IsEnabled = true;
            btn_clear_selection.IsEnabled = true;

            //if there's only 1 item in list (max index of 0), don't want either move

            int row_pos = Glo_ProjectData.Glo_Selected_EntryData.RowPosition;

            int row_max_index = Glo_ProjectData.EntryData_lst_param.Count() - 1;

            Debug.WriteLine("row_pos = " + row_pos + "    row_max_index = " + row_max_index);

            if (row_max_index == 0)
            {
                Debug.WriteLine("Disabling move buttons as single-entry list");

                btn_move_up.IsEnabled = false;
                btn_move_down.IsEnabled = false;

                return;
            }


            //if at the start of list, don't want move up

            if (row_pos == 0)
            {
                Debug.WriteLine("Disabling move up as row_pos = " + row_pos);

                btn_move_up.IsEnabled = false;
            }
            else
            {
                Debug.WriteLine("Enabling move up button");

                btn_move_up.IsEnabled = true;
            }


            //if at the end of list, don't want move down

            if (row_pos == row_max_index)
            {
                Debug.WriteLine("Disabling move down as row_pos = " + row_pos);

                btn_move_down.IsEnabled = false;
            }
            else
            {
                Debug.WriteLine("Enabling move down button");

                btn_move_down.IsEnabled = true;
            }


        }
        private void PropertiesButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            Debug.WriteLine("PropertiesButtonTapped");
            //await OpenPropertiesWindow();

            Glo_ProjectData.Glo_PropertyPaneOpen = true;

        }
        private void DeleteButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            Glo_ProjectData.DeleteEntryMethod();
        }
        public void ClearSelectionButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            Glo_ProjectData.UnselectEntryData();
        }
        private void MoveButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            Button sending_button = (Button)sender;

            //string direction = "";
            int move_relative_int = 0;
            switch (sending_button.Name)
            {
                case "btn_move_up":
                    move_relative_int = -1; //moving up is decreasing index
                    break;

                case "btn_move_down":
                    move_relative_int = +1; //moving down is increasing index
                    break;
            }

            //this is the old method
            //MoveEntryUpDown(direction, glo_selected_EntryData.EntryID);

            //this is the new method
            Glo_ProjectData.MoveEntryByIndex(move_relative_int);
        }
        private void AddButtonTapped(object sender, RoutedEventArgs e)
        {
            OpenAddEntryWindow();
        }







        private async void OpenAddEntryWindow()
        {
            contentdialog_add_new_entry.Visibility = Visibility.Visible;
            Glo_ProjectData.Glo_AddNewWindowOpen = true;

            Glo_ProjectData.CreateNewAddNewEntry();

            contentdialog_add_new_entry.DataContext = Glo_ProjectData.Glo_AddNewEntry;

            AddNewEntryUpdatePreview();

            ContentDialogResult result = await contentdialog_add_new_entry.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                //we close and save the new entry
                Glo_ProjectData.AddEntryMethod();
            }
            else
            {
                //we close and discard the new entry
            }

            Glo_ProjectData.Glo_AddNewWindowOpen = false;

        }
        public void AddNewEntryUpdatePreview()
        {
            //this method will update the preview border when adding a new entry


            Debug.WriteLine("AddNewEntryUpdatePreview");

            Border preview_border = CreateEntryTitleBlock(
                Glo_ProjectData.Glo_AddNewEntry.GetEntryDataForPreview(), true);

            //removes the border that's already there
            if (stack_add_new_entry_preview.Children.Count() > 1)
            {
                stack_add_new_entry_preview.Children.RemoveAt(1);
            }

            //and puts in the new one
            stack_add_new_entry_preview.Children.Add(preview_border);
        }
        public void InsertNewEntryBorders()
        {



            //now to create the titleblock using the known index of the new entryData
            AddMissingBorders("title_calendar");
            //CreateAndPlaceNewTitleBlock(Glo_ProjectData.EntryData_lst_param[inserted_index]);


            //will update the layout of the titleblocks
            CreateCorrectNumberOfGridRowsColumns("titleblock_rows");


            //will update the used colour list
            Glo_ProjectData.CreateUsedColourList();





            //Border new_titleblock = CreateEntryTitleBlock(cur_entryData) as Border;
            //grid_entry_title_blocks.Children.Add(new_titleblock);


            UpdateCalendarLayout();
            UpdateCalendarColRowSize("rows");
        }
        public void ShowAddNewEntryTitleInvalidTeachingTip()
        {
            teach_addnewentry_title_invalid.IsOpen = true;
        }






        //these methods deal with deleting of entries

        public async Task<bool> DeleteConfirmationDialogue(EntryData delete_entryData)
        {

            //with bindings, the title and subtitle should fill
            contentdialog_confirm_delete.DataContext = delete_entryData;
            chkbox_delete_no_ask.DataContext = Glo_ProjectData;

            contentdialog_confirm_delete.Visibility = Visibility.Visible;

            ContentDialogResult result = await contentdialog_confirm_delete.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                //we delete the selected entry
                return true;
            }
            else
            {
                //we do nothing
                return false;
            }
        }
        public void RemoveEntryBorders()
        {


            //remove it from grids
            RemoveDeletedBlocks("both");

            //and update all the other titleblocks and calendarblocks to update their rows
            //FixBlockInfo(false, false, false, true, false);


            UpdateCalendarLayout();
        }




        //these methods deal with the property pane. much of its functionality is data binding with ChangedProperties.cs

        public void OpenPropertiesMethod()
        {
            //glo_property_pane_open = true;

            //this prepares the ChangedProperties class which will keep track of all property editing
            Glo_ProjectData.PrepareChangedProperties();

            //will make the properties pane visible
            grid_properties_pane.Visibility = Visibility.Visible;
        }
        public void SetDataContextPropertiesPane()
        {
            //applies the datacontext of the pane
            grid_properties_pane.DataContext = Glo_ProjectData.Glo_ChangedProperties;
        }
        private async void ClosePropertiesPane_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //if don't NOT want to be asked (so we do want to be asked)
            if (!Glo_ProjectData.No_ApplyProperties_Verif_Wanted)
            {
                //will determine if there are any changed properties
                bool changed_properties = Glo_ProjectData.Glo_ChangedProperties.AnyProperties_Changed;

                //if they confirm they want to close without saving
                if (changed_properties && !await ExitNoApplyPropertiesConfirmationDialogue())
                {
                    return;
                }
            }


            Glo_ProjectData.Glo_PropertyPaneOpen = false;
            //this property value will toggle the visibility of the properties pane 
        }
        public void ClosePropertiesMethod()
        {
            //closes the pane
            grid_properties_pane.Visibility = Visibility.Collapsed;
        }
        private async void ApplyPropertiesButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //this will apply the properties, and then refresh the pane to edit more properties

            bool apply_success = await Glo_ProjectData.ApplyPropertiesMethod(false);

            if (apply_success)
            {
                Glo_ProjectData.Glo_PropertyPaneOpen = false;
                //Glo_ProjectData.PrepareChangedProperties();
            }
        }
        private async void OkPropertiesButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //this will apply the properties, and then close the pane

            bool apply_success = await Glo_ProjectData.ApplyPropertiesMethod(false);

            if (apply_success)
            {
                Glo_ProjectData.Glo_PropertyPaneOpen = false;
            }
        }
        public void ShowPropertiesTitleInvalidTeachingTip()
        {
            teach_properties_title_invalid.IsOpen = true;
        }
        public async Task<bool> ApplyPropertiesConfirmationDialogue()
        {

            chkbox_applyproperties_no_ask.DataContext = Glo_ProjectData;

            contentdialog_confirm_applyproperties.Visibility = Visibility.Visible;

            ContentDialogResult result = await contentdialog_confirm_applyproperties.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                //we apply the properties
                return true;
            }
            else
            {
                //we do nothing
                return false;
            }
        }
        public async Task<bool> ExitNoApplyPropertiesConfirmationDialogue()
        {

            //with bindings, the title and subtitle should fill
            chkbox_exit_properties_no_ask.DataContext = Glo_ProjectData;

            contentdialog_confirm_exitnoapplyproperties.Visibility = Visibility.Visible;

            ContentDialogResult result = await contentdialog_confirm_exitnoapplyproperties.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                //we exit without applying
                return true;
            }
            else
            {
                //we do nothing
                return false;
            }
        }





        //these methods deal with the colour picker

        private void ColourPickerFlyout_Opened(object sender, object e)
        {
            //for some reason
            if (!Glo_ProjectData.Glo_AddNewWindowOpen && !Glo_ProjectData.Glo_PropertyPaneOpen)
            {
                return;
            }

            //puts the correct colour into the colour picker

            myColorPicker.DataContext = Glo_ProjectData;

            Glo_ProjectData.InitiateColourPicker();

            //myColorPicker.Color = Glo_ProjectData.GetColourForColourPicker();


            //clears the used colours variable grid
            vargrid_used_colours.Children.Clear();

            //now to create the new colours in it, if it exists
            if (Glo_ProjectData.UsedColoursHex == null) { return; }
            foreach (string cur_used_colours_hex in Glo_ProjectData.UsedColoursHex)
            {
                //will make a colour grid for each used colour

                //  <Grid Style="{StaticResource grid_UsedColour}" Tapped="InUseColourTapped" Background="Turquoise"/>

                Grid used_colour_grid = new Grid
                {
                    Style = Application.Current.Resources["grid_UsedColour"] as Style,
                    Background = MasterClass.GetSolidColorBrushFromHex(cur_used_colours_hex)
                };

                used_colour_grid.Tapped += InUseColourTapped;

                vargrid_used_colours.Children.Add(used_colour_grid);

            }
        }
        private void InUseColourTapped(object sender, TappedRoutedEventArgs e)
        {
            Grid sending_colour_grid = sender as Grid;
            SolidColorBrush background_brush = (SolidColorBrush)sending_colour_grid.Background;

            //Windows.UI.Color background_colour = background_brush.Color;

            Glo_ProjectData.ColourHex_Picker = MasterClass.GetHexFromSolidColorBrush(background_brush);

            //myColorPicker.Color = background_colour;

        }
        private void ColourPicker_ColourChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            Windows.UI.Color picker_colour = myColorPicker.Color;

            foreach (Grid cur_colour_grid in vargrid_used_colours.Children)
            {
                SolidColorBrush cur_background_brush = (SolidColorBrush)cur_colour_grid.Background;
                Windows.UI.Color cur_background_colour = cur_background_brush.Color;

                if (picker_colour == cur_background_colour)
                { cur_colour_grid.Style = Application.Current.Resources["grid_UsedColour_Selected"] as Style; }

                else 
                { cur_colour_grid.Style = Application.Current.Resources["grid_UsedColour"] as Style; }
            }
        }
        private void ConfirmColour_Click(object sender, RoutedEventArgs e)
        {
            //Glo_ProjectData.ConfirmColour(MasterClass.GetHexFromColorPicker(myColorPicker.Color));
            Glo_ProjectData.ConfirmColour();

            CloseColourFlyouts();
        }
        private void CancelColour_Click(object sender, RoutedEventArgs e)
        {
            //glo_CalculatedData.ColourPickerChosenHex = glo_CalculatedData.DefaultColourChosenHex;

            CloseColourFlyouts();
        }
        public void CloseColourFlyouts()
        {
            // Close the Flyouts
            btn_properties_change_colour.Flyout.Hide();
            btn_add_new_entry_colour.Flyout.Hide();
        }







        //these are general utilities






        //these methods are the tap handlers for the temporary commandbar buttons, for testing purposes
        private void LoadTestDataButton(object sender, RoutedEventArgs e)
        {
            //LoadProjectData(@"ms-appx:///Assets/TestData/", "Test Semesters 1");
            MasterClass.LoadProject(@"ms-appx:///Assets/TestData/", "Test Semesters 2");
        }
        private void ComputeCalendarButton(object sender, RoutedEventArgs e)
        {
            Glo_ProjectData.UpdateCalendarLogicDisplay(false);
        }
        private void PrintGlobalData(object sender, RoutedEventArgs e)
        {
            Glo_ProjectData.PrintProjectDataValues(true, true, true);
        }
        private void EditButton(object sender, RoutedEventArgs e)
        {
            //test_button_datatemplate.ContentTemplate = this.Resources["control_template_test"] as DataTemplate;
            //test_button_datatemplate.DataContext = Glo_ProjectData.EntryData_lst_param[2];

            //relpan_custom_window.Visibility = Visibility.Visible;
            //relpan_custom_window.Translation += new Vector3(0, 0, 32);
            //rec_frame_background.Visibility = Visibility.Visible;

            Glo_ProjectData.PrintPropertiesEntryDatas();

        }
        private void ContactButton(object sender, RoutedEventArgs e)
        {
            //this is a misc button that I'll use for testing commands


            //List<string> save_list = FormatCurrentSaveFile();

            //foreach (string cur_line in save_list)
            //{
            //    Debug.WriteLine(cur_line);
            //}


            //Debug.WriteLine(txtbox_prop_title.Style.ToString());

            Debug.WriteLine("MainPage_Instance_Count = " + MainPage_Instance_Count);
        }
        private void GlobeButton(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("grid_entry_title_blocks.Children.Count = " + grid_entry_title_blocks.Children.Count);
            Debug.WriteLine("grid_calendar.Children.Count = " + grid_calendar.Children.Count);

            //string test_entryID = "geog_240";

            //Border test_border = Glo_ProjectData.GetEntryDataFromEntryID(test_entryID).TitleBlock;

            Debug.WriteLine("Refreshing gridlines");
            CreateGridlines();
        }

        private  void PrintAddNewEntryClass(object sender, TappedRoutedEventArgs e)
        {
            Glo_ProjectData.Glo_AddNewEntry.PrintProperties();
        }
    }
}

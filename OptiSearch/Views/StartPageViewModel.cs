using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace OptiSearch.Views
{
 
    public class StartPageViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ComboBoxItem> ComboBoxOptions;
        private ComboBoxItem _SelectedComboBoxOption;

        public StartPageViewModel()
        {
            ComboBoxOptions = new ObservableCollection<ComboBoxItem>();
            ComboBoxOptionsManager.GetComboBoxList(ComboBoxOptions);
            _SelectedComboBoxOption = ComboBoxOptions[0];

        }
     
        public class ComboBoxOptionsManager
        {
            public static void GetComboBoxList(ObservableCollection<ComboBoxItem> ComboBoxItems)
            {
                var allItems = getComboBoxItems();
                ComboBoxItems.Clear();
                allItems.ForEach(p => ComboBoxItems.Add(p));
            }

            private static List<ComboBoxItem> getComboBoxItems()
            {
                var items = new List<ComboBoxItem>();

                items.Add(new ComboBoxItem() { ComboBoxOption = GetInstalledLanguageCode(), ComboBoxHumanReadableOption = GetInstalledLanguageName() });
               // items.Add(new ComboBoxItem() { ComboBoxOption = "fr", ComboBoxHumanReadableOption = "French"});
               // items.Add(new ComboBoxItem() { ComboBoxOption = "es", ComboBoxHumanReadableOption = "Spanish" });

                return items;
            }
            public static string GetInstalledLanguageName()
            {
                CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                return cultureInfo.DisplayName;          
            }
        }

        public static string GetInstalledLanguageCode()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            return cultureInfo.TwoLetterISOLanguageName;

        }

        public ComboBoxItem SelectedComboBoxOption
        {
            get
            {
                return _SelectedComboBoxOption;
            }
            set
            {
                if (_SelectedComboBoxOption != value)
                {
                    _SelectedComboBoxOption = value;
                    RaisePropertyChanged("SelectedComboBoxOption");
                }
            }
        }
       

        void RaisePropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;

       
    }

    public class ComboBoxItem
    {
        public string ComboBoxOption { get; set; }
        public string ComboBoxHumanReadableOption { get; set; }
    }

    public class ComboBoxItemConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value as ComboBoxItem;
        }
    }

}

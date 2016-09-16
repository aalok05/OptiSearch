using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace OptiSearch
{
    class NotifyService
    {
        public static ContentDialog NotifyUser(string title, string message)
        {
            return new ContentDialog()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "OK",
                IsPrimaryButtonEnabled = true,
          
        };
   
        }
    }
}

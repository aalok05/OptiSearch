using OptiSearch.Constants;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace OptiSearch.Views
{
    public sealed partial class ExtendedSplashScreen : Page
    {
        private Rect _splashImageBounds;

        public ExtendedSplashScreen(SplashScreen splashScreen)
        {
            InitializeComponent();

            if (splashScreen != null)
            {
                _splashImageBounds = splashScreen.ImageLocation;
            }
            Loaded += ExtendedSplashScreen_Loaded;
        }

        private void ExtendedSplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the main page
            StartPage page = new StartPage();

            var rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                Window.Current.Content = rootFrame = new Frame();
            }

            rootFrame.Content = page;
        }

      
    }
}

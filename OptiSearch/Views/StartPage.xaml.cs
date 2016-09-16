using System;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using OptiSearch.Services;
using System.Numerics;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using OptiSearch.Constants;

namespace OptiSearch.Views
{
    public sealed partial class StartPage : Page
    {
        private DeviceInformationCollection _allVideoDevices;

        public StartPage()
        {
            this.InitializeComponent();

            var view = ApplicationView.GetForCurrentView();

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            titleBar.BackgroundColor = GetSolidColorBrush("#535254FF").Color;
            titleBar.ButtonBackgroundColor = GetSolidColorBrush("#535254FF").Color;
            titleBar.ButtonForegroundColor = Colors.Black;

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            // Initialize the surface loader
            SurfaceLoader.Initialize(ElementCompositionPreview.GetElementVisual(this).Compositor);

            if(FirstLaunch.animate)
            {
                // Show the custome splash screen
                Rect imageBounds;
                ShowCustomSplashScreen(imageBounds);
            }
           
            Loaded += StartPage_Loaded;
        }

        private void StartPage_Loaded(object sender, RoutedEventArgs e)
        {
            if(FirstLaunch.animate)
            {
                FirstLaunch.animate = false;
                HideCustomSplashScreen();
            }
          
        }

        #region non UI code
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            OnNavStoryboard.Begin();
          
        }
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            if (secFrame.CurrentSourcePageType == typeof(CamView) ||
                secFrame.CurrentSourcePageType == typeof(PicView) ||
                secFrame.CurrentSourcePageType == typeof(AboutView))
            {
                secFrame.Navigate(typeof(StartPage));
            }
            else
            if (settingsGrid.Visibility == Visibility.Visible)
            {
                settingsClosedStoryboard.Begin();
            }
        }
        #endregion

        #region UI code
        private void StackPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var s = sender as StackPanel;
            s.Opacity = 0.6;

        }
        private void About_Tapped(object sender, TappedRoutedEventArgs e)
        {
            secFrame.Navigate(typeof(AboutView));
        }
        private void Settings_Tapped(object sender, TappedRoutedEventArgs e)
        {
            settingsGrid.Visibility = Visibility.Visible;
            SettingStoryboard.Begin();
        }
        private async void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get available devices for capturing media and list them 
                _allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                if (_allVideoDevices == null || !_allVideoDevices.Any())
                {
                    var dialog = NotifyService.NotifyUser("No camera found", "");
                    dialog.MaxWidth = this.ActualWidth;
                    var result = await dialog.ShowAsync();
                }
                else
                {
                    secFrame.Navigate(typeof(CamView));
                }

            }
            catch (UnauthorizedAccessException)
            {
                var dialog = NotifyService.NotifyUser("App cannot access camera", "");
                dialog.MaxWidth = this.ActualWidth;
                await dialog.ShowAsync();

            }
            catch (Exception ex)
            {
                var dialog = NotifyService.NotifyUser("Something went wrong.", "Exception : " + ex.Message);
                dialog.MaxWidth = this.ActualWidth;
                await dialog.ShowAsync();
            }
        }

        private async void LibraryButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                OCRservice.imageFile = file;
                secFrame.Navigate(typeof(PicView));

            }
        }
        private void StackPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var s = sender as StackPanel;
            s.Opacity = 0.8;
        }

        private async void ShowCustomSplashScreen(Rect imageBounds)
        {
            Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            Vector2 windowSize = new Vector2((float)Window.Current.Bounds.Width, (float)Window.Current.Bounds.Height);


            //
            // Create a container visual to hold the color fill background and image visuals.
            // Configure this visual to scale from the center.
            //

            ContainerVisual container = compositor.CreateContainerVisual();
            container.Size = windowSize;
            container.CenterPoint = new Vector3(windowSize.X, windowSize.Y, 0) * .5f;
            ElementCompositionPreview.SetElementChildVisual(this, container);


            //
            // Create the colorfill sprite for the background, set the color to the same as app theme
            //

            SpriteVisual backgroundSprite = compositor.CreateSpriteVisual();
            backgroundSprite.Size = windowSize;
            backgroundSprite.Brush = compositor.CreateColorBrush(Color.FromArgb(1, 0, 178, 240));
            container.Children.InsertAtBottom(backgroundSprite);


            //
            // Create the image sprite containing the splash screen image.  Size and position this to
            // exactly cover the Splash screen image so it will be a seamless transition between the two
            //

            CompositionDrawingSurface surface = await SurfaceLoader.LoadFromUri(new Uri("ms-appx:///Assets/SplashScreen.png"));
            SpriteVisual imageSprite = compositor.CreateSpriteVisual();
            imageSprite.Brush = compositor.CreateSurfaceBrush(surface);
            imageSprite.Offset = new Vector3((float)imageBounds.X, (float)imageBounds.Y, 0f);
            imageSprite.Size = new Vector2((float)imageBounds.Width, (float)imageBounds.Height);
            container.Children.InsertAtTop(imageSprite);
        }

        private void HideCustomSplashScreen()
        {
            ContainerVisual container = (ContainerVisual)ElementCompositionPreview.GetElementChildVisual(this);
            Compositor compositor = container.Compositor;

            // Setup some constants for scaling and animating
            const float ScaleFactor = 20f;
            TimeSpan duration = TimeSpan.FromMilliseconds(1200);
            LinearEasingFunction linearEase = compositor.CreateLinearEasingFunction();
            CubicBezierEasingFunction easeInOut = compositor.CreateCubicBezierEasingFunction(new Vector2(.38f, 0f), new Vector2(.45f, 1f));

            // Create the fade animation which will target the opacity of the outgoing splash screen
            ScalarKeyFrameAnimation fadeOutAnimation = compositor.CreateScalarKeyFrameAnimation();
            fadeOutAnimation.InsertKeyFrame(1, 0);
            fadeOutAnimation.Duration = duration;

            // Create the scale up animation for the grid
            Vector2KeyFrameAnimation scaleUpGridAnimation = compositor.CreateVector2KeyFrameAnimation();
            scaleUpGridAnimation.InsertKeyFrame(0.1f, new Vector2(1 / ScaleFactor, 1 / ScaleFactor));
            scaleUpGridAnimation.InsertKeyFrame(1, new Vector2(1, 1));
            scaleUpGridAnimation.Duration = duration;

            // Create the scale up animation for the Splash screen visuals
            Vector2KeyFrameAnimation scaleUpSplashAnimation = compositor.CreateVector2KeyFrameAnimation();
            scaleUpSplashAnimation.InsertKeyFrame(0, new Vector2(1, 1));
            scaleUpSplashAnimation.InsertKeyFrame(1, new Vector2(ScaleFactor, ScaleFactor));
            scaleUpSplashAnimation.Duration = duration;

            // Configure the grid visual to scale from the center
            Visual gridVisual = ElementCompositionPreview.GetElementVisual(secFrame);
            gridVisual.Size = new Vector2((float)secFrame.ActualWidth, (float)secFrame.ActualHeight);
            gridVisual.CenterPoint = new Vector3(gridVisual.Size.X, gridVisual.Size.Y, 0) * .5f;


            //
            // Create a scoped batch for the animations.  When the batch completes, we can dispose of the
            // splash screen visuals which will no longer be visible.
            //

            CompositionScopedBatch batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            container.StartAnimation("Opacity", fadeOutAnimation);
            container.StartAnimation("Scale.XY", scaleUpSplashAnimation);
            gridVisual.StartAnimation("Scale.XY", scaleUpGridAnimation);

            batch.Completed += Batch_Completed;
            batch.End();
        }

        private void Batch_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            // Now that the animations are complete, dispose of the custom Splash Screen visuals
            ElementCompositionPreview.SetElementChildVisual(this, null);
        }

        public SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte r = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte a = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            return myBrush;
        }
        #endregion
    }

}

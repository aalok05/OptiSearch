using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using OptiSearch.Services;
using Windows.Graphics.Display;
using Windows.Media.Ocr;
using Windows.Globalization;


namespace OptiSearch.Views
{

    public sealed partial class PicView : Page
    {
        private SoftwareBitmap bitmap;
        private Language ocrLanguage = new Language(StartPageViewModel.GetInstalledLanguageCode());
        private List<WordOverlay> keywordBoxes = new List<WordOverlay>();
        private List<WordOverlay> wordBoxes = new List<WordOverlay>();

        private readonly DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();

        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");


        public PicView()
        {
            this.InitializeComponent();
        }

       
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            if (!OcrEngine.IsLanguageSupported(ocrLanguage))
            {
                Frame.Navigate(typeof(StartPage));
                var dialog = NotifyService.NotifyUser(ocrLanguage.DisplayName + " is not supported.", "");
                dialog.MaxWidth = this.ActualWidth;
                var result = await dialog.ShowAsync();

            }
            else
            {
                await LoadImage(OCRservice.imageFile);
                await extractWords();
            }
           
        }

        private async Task extractWords()
        {

            OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(ocrLanguage);

            if (ocrEngine == null)
            {
                //rootPage.NotifyUser(ocrLanguage.DisplayName + " is not supported.", NotifyType.ErrorMessage);
                return;
            }

            //  SoftwareBitmap bitmap = PreviewImage.SoftwareBitmap;
            var ocrResult = await ocrEngine.RecognizeAsync(bitmap);

            // Used for text overlay.
            // Prepare scale transform for words since image is not displayed in original format.
            var scaleTrasform = new ScaleTransform
            {
                CenterX = 0,
                CenterY = 0,
                ScaleX = PreviewImage.ActualWidth / bitmap.PixelWidth,
                ScaleY = PreviewImage.ActualHeight / bitmap.PixelHeight
            };

            if (ocrResult.TextAngle != null)
            {
                // If text is detected under some angle in this sample scenario we want to
                // overlay word boxes over original image, so we rotate overlay boxes.
                TextOverlay.RenderTransform = new RotateTransform
                {
                    Angle = (double)ocrResult.TextAngle,
                    CenterX = PreviewImage.ActualWidth / 2,
                    CenterY = PreviewImage.ActualHeight / 2
                };
                TextOverlay2.RenderTransform = new RotateTransform
                {
                    Angle = (double)ocrResult.TextAngle,
                    CenterX = PreviewImage.ActualWidth / 2,
                    CenterY = PreviewImage.ActualHeight / 2
                };
            }
            // Iterate over recognized lines of text.
            foreach (var line in ocrResult.Lines)
            {
                // Iterate over words in line.
                foreach (var word in line.Words)
                {
                    // Define the TextBlock.
                    var wordTextBlock = new TextBlock()
                    {
                        Text = word.Text,
                        Style = (Style)this.Resources["ExtractedWordTextStyle"]
                    };

                    WordOverlay wordBoxOverlay = new WordOverlay(word);

                    // Keep references to word boxes.
                    wordBoxes.Add(wordBoxOverlay);

                    // Define position, background, etc.
                    var overlay = new Border()
                    {
                        Child = wordTextBlock,
                        Style = (Style)this.Resources["HighlightedWordBoxHorizontalLine"]
                    };

                    // Bind word boxes to UI.
                    overlay.SetBinding(Border.MarginProperty, wordBoxOverlay.CreateWordPositionBinding());
                    overlay.SetBinding(Border.WidthProperty, wordBoxOverlay.CreateWordWidthBinding());
                    overlay.SetBinding(Border.HeightProperty, wordBoxOverlay.CreateWordHeightBinding());

                    // Put the filled textblock in the results grid.
                    TextOverlay.Children.Add(overlay);

                }
                // rootPage.NotifyUser("Image processed using " + ocrEngine.RecognizerLanguage.DisplayName + " language.", NotifyType.StatusMessage);
            }

            UpdateWordBoxTransform();
        }

        private async void ChooseButton_Click(object sender, RoutedEventArgs e)
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
                TextOverlay.Children.Clear();
                wordBoxes.Clear();
                TextOverlay2.Children.Clear();
                keywordBoxes.Clear();
                await LoadImage(file);
                await extractWords();
            }
        }

        private async Task LoadImage(StorageFile file)
        {
            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(stream);

                bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                var imgSource = new WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight);
                bitmap.CopyToBuffer(imgSource.PixelBuffer);
                PreviewImage.Source = imgSource;
            }
        }

        private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var s = sender as TextBox;

            if (String.IsNullOrEmpty(s.Text))
            {
                return;
            }
            else
            {
                TextOverlay2.Children.Clear();
                keywordBoxes.Clear();
                FindWord(s.Text);
            }
        }

        private void PreviewImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWordBoxTransform();

            // Update image rotation center.
            var rotate = TextOverlay.RenderTransform as RotateTransform;
            var rotate2 = TextOverlay2.RenderTransform as RotateTransform;

            if (rotate2 != null)
            {
                rotate2.CenterX = PreviewImage.ActualWidth / 2;
                rotate2.CenterY = PreviewImage.ActualHeight / 2;
            }
            if (rotate != null)
            {
                rotate.CenterX = PreviewImage.ActualWidth / 2;
                rotate.CenterY = PreviewImage.ActualHeight / 2;
            }
        }

        private void UpdateWordBoxTransform()
        {
            WriteableBitmap bitmap = PreviewImage.Source as WriteableBitmap;

            if (bitmap != null)
            {
                // Used for text overlay.
                // Prepare scale transform for words since image is not displayed in original size.
                ScaleTransform scaleTrasform = new ScaleTransform
                {
                    CenterX = 0,
                    CenterY = 0,
                    ScaleX = PreviewImage.ActualWidth / bitmap.PixelWidth,
                    ScaleY = PreviewImage.ActualHeight / bitmap.PixelHeight
                };

                foreach (var item in wordBoxes)
                {
                    item.Transform(scaleTrasform);
                }
            }
        }
        private void FindWord(string keyword)
        {
            keywordBoxes = wordBoxes
             .Where(p => (p.word.Text).ToLower() == (keyword).ToLower())
             .Select(p => p)
             .ToList();

            if (keywordBoxes.Count > 0)
            {

                OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(ocrLanguage);

                if (ocrEngine == null)
                {
                    return;
                }
                foreach (var w in keywordBoxes)
                {

                    var wordTextBlock = new TextBlock()
                    {
                        Text = w.word.Text,
                        Style = (Style)this.Resources["FoundWordTextStyle"]
                    };

                    WordOverlay wordBoxOverlay = new WordOverlay(w.word);

                    wordBoxes.Add(wordBoxOverlay);

                    var overlay = new Border()
                    {
                        Child = wordTextBlock,
                        Style = (Style)this.Resources["HighlightedWordBoxHorizontalLine"]
                    };

                    // Bind word boxes to UI.
                    overlay.SetBinding(Border.MarginProperty, wordBoxOverlay.CreateWordPositionBinding());
                    overlay.SetBinding(Border.WidthProperty, wordBoxOverlay.CreateWordWidthBinding());
                    overlay.SetBinding(Border.HeightProperty, wordBoxOverlay.CreateWordHeightBinding());

                    // Put the filled textblock in the results grid.
                    TextOverlay2.Children.Add(overlay);
                }

                UpdateWordBoxTransform();
            }
            else
            {
                return;
            }

        }
    }
}

using AForge.Video;
using AForge.Video.DirectShow;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using CandidateForm.Helpers;
using System.Windows.Input;
using System.Threading.Tasks;

namespace CandidateForm.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Бланк кандидата";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private BitmapImage image;
        private BitmapImage screenshot;

        private VideoCaptureDevice videoSource;

        public ICommand TakeScreenshot { get; private set; }

        public BitmapImage Image
        {
            get { return image; }
            set { SetProperty(ref image, value); }
        }

        public BitmapImage Screenshot
        {
            get { return screenshot; }
            set { SetProperty(ref screenshot, value); }
        }

        private void StartCapture()
        {
            var videoDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevice != null)
            {
                videoSource = new VideoCaptureDevice(videoDevice[0].MonikerString);                
                videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                videoSource.Start();

            }
            else
            {
                MessageBox.Show("Не могу получить доступ к видеокамере. Может камера не подключена?");
            }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    Image = bitmap.ToBitmapImage();
                }
                Image.Freeze(); // avoid cross thread operations and prevents leaks
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не могу получить данные с видеокамеры:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopCamera();
            }
        }

        private void StopCamera()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.NewFrame -= new NewFrameEventHandler(video_NewFrame);
            }
            //Image = null; // snapshot should be held in frame
        }

        private void OnTakeScreenshot()
        {
            try
            {
                Screenshot = (BitmapImage)Image.GetCurrentValueAsFrozen();
                Screenshot.Freeze();
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("Не могу сделать снимок.");
                StartCapture();
            }
        }

        public MainWindowViewModel()
        {
            TakeScreenshot = new DelegateCommand(OnTakeScreenshot);

            StartCapture();
        }

    }
}

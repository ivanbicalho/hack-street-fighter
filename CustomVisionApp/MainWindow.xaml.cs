﻿using CustomVisionApp.Models;
using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CustomVisionApp {
    public partial class MainWindow : System.Windows.Window {

        private MyCamera _camera;
        private VisionClient _visionClient;
        private CustomVisionClient _customVisionClient;
        private BitmapImage _currentImage;

        public MainWindow() {
            InitializeComponent();
            var url = "https://brazilsouth.api.cognitive.microsoft.com";
            var token = Environment.GetEnvironmentVariable("AZURE-COGNITIVESERVICES-TOKEN", EnvironmentVariableTarget.User);
            var predictionToken = Environment.GetEnvironmentVariable("AZURE-CUSTOMVISION-PREDICTION-TOKEN", EnvironmentVariableTarget.User);
            _visionClient = new VisionClient(token, url);
            _customVisionClient = new CustomVisionClient(predictionToken);
        }

        private void BStart_Click(object sender, RoutedEventArgs e) {
            if(bStart.Content.Equals("Start")) {
                _camera = new MyCamera();
                _camera.NewImage = NewImage;
                bStart.Content = "Stop";
                bLoad.IsEnabled = false;
                Task.Factory.StartNew(() => _camera.StartCamera(0));
            }
            else {
                _camera.Dispose();
                bStart.Content = "Start";
                bLoad.IsEnabled = true;
            }
        }

        private void BLoad_Click(object sender, RoutedEventArgs e) {
            var op = new OpenFileDialog {
                Title = "Select a picture",
                Filter = @"All supported graphics|*.jpg;*.jpeg;*.png|
                         JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|
                         Portable Network Graphic (*.png)|*.png"
            };
            if (op.ShowDialog() == true) {
                _currentImage = new BitmapImage(new Uri(op.FileName));
                TheImage.Source = _currentImage;
            }
        }

        private async void BDescribe_Click(object sender, RoutedEventArgs e) {
            if(_currentImage == null) {
                return;
            }
            var image = ToImage(_currentImage);

            string description;
            if(ServiceGeneric.IsChecked ?? false) {
                description = await _visionClient.Analyze(image);
            }
            else if(ServiceCustom.IsChecked ?? false) {
                description = await _customVisionClient.Analyze(image, editProjectId.Text);
            }
            else {
                description = "";
            }
            Description.Text = description;
        }

        private byte[] ToImage(BitmapImage bitmapImage) {
            byte[] data;
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
            using (MemoryStream ms = new MemoryStream()) {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }

        private void NewImage(Mat frame) {
            var stream = frame.ToMemoryStream(".png");
            ChangeUI(() => {
                _currentImage = ToImage(stream);
                TheImage.Source = _currentImage;
                TheWindow.Title = "Azure Vision: " + _camera.FramesPerSecond.ToString();
            });
        }

        private void ChangeUI(Action action) {
            Dispatcher.BeginInvoke(action);
        }

        public BitmapImage ToImage(Stream stream) {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }

    }
}

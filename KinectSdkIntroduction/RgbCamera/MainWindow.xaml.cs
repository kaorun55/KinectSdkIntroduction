using System;
using System.Windows;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;

namespace RgbCamera
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            try {
                if ( KinectSensor.KinectSensors.Count == 0 ) {
                    throw new Exception( "Kinectが接続されていません" );
                }

                // Kinectインスタンスを取得する
                KinectSensor kinect = KinectSensor.KinectSensors[0];

                // Colorを有効にする
                kinect.ColorFrameReady +=
                    new EventHandler<ColorImageFrameReadyEventArgs>( kinect_ColorFrameReady );
                kinect.ColorStream.Enable();

                // Kinectの動作を開始する
                kinect.Start();
            }
            catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
                Close();
            }
        }

        void kinect_ColorFrameReady( object sender, ColorImageFrameReadyEventArgs e )
        {
            imageRgbCamera.Source = e.OpenColorImageFrame().ToBitmapSource();
        }
    }
}

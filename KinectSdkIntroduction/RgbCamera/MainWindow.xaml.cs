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
        KinectSensor kinect;

        public MainWindow()
        {
            InitializeComponent();

            try {
                // 利用可能なKinectを探す
                foreach ( var k in KinectSensor.KinectSensors ) {
                    if ( k.Status == KinectStatus.Connected ) {
                        kinect = k;
                        break;
                    }
                }
                if ( kinect == null ) {
                    throw new Exception( "利用可能なKinectがありません" );
                }

                // Colorを有効にする
                kinect.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>( kinect_ColorFrameReady );
                kinect.ColorStream.Enable( ColorImageFormat.RgbResolution640x480Fps30 );

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
            // Disposableなのでusingでくくる
            using ( ColorImageFrame colorFrame = e.OpenColorImageFrame() ) {
                if ( colorFrame != null ) {
                    imageRgbCamera.Source = colorFrame.ToBitmapSource();
                }
            }
        }
    }
}

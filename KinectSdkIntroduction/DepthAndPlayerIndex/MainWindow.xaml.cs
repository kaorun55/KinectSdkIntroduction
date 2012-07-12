using System;
using System.Windows;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;
using System.Diagnostics;

namespace DepthAndPlayerIndex
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

                // すべてのフレーム更新通知をもらう
                kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>( kinect_AllFramesReady );

                // Color,Depth,Skeletonを有効にする(Skeletonを有効にしないと、プレーヤーもとれない)
                kinect.ColorStream.Enable( ColorImageFormat.RgbResolution640x480Fps30 );
                kinect.DepthStream.Enable( DepthImageFormat.Resolution640x480Fps30 );
                kinect.SkeletonStream.Enable();

                // Kinectの動作を開始する
                kinect.Start();
            }
            catch ( Exception ex ) {
                MessageBox.Show( ex.Message );
                Close();
            }
        }

        // すべてのデータの更新通知を受け取る
        void kinect_AllFramesReady( object sender, AllFramesReadyEventArgs e )
        {
            // Disposableなのでusingでくくる
            using ( ColorImageFrame colorFrame = e.OpenColorImageFrame() ) {
                if ( colorFrame != null ) {
                    imageRgbCamera.Source = colorFrame.ToBitmapSource();
                }
            }

            // Disposableなのでusingでくくる
            using ( DepthImageFrame depthFrame = e.OpenDepthImageFrame() ) {
                if ( depthFrame != null ) {
                    imageDepthCamera.Source = depthFrame.ToBitmapSource();
                }
            }
        }
    }
}

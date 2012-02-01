using System;
using System.Windows;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;

namespace DepthAndPlayerIndex
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
            imageRgbCamera.Source = e.OpenColorImageFrame().ToBitmapSource();
            imageDepthCamera.Source = e.OpenDepthImageFrame().ToBitmapSource();
        }
    }
}

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;

namespace SkeletonSample
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
                if ( KinectSensor.KinectSensors.Count == 0 ) {
                    throw new Exception( "Kinectが接続されていません" );
                }

                // Kinectインスタンスを取得する
                kinect = KinectSensor.KinectSensors[0];

                // すべてのフレーム更新通知をもらう
                kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>( kinect_AllFramesReady );

                // Color,Depth,Skeletonを有効にする
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

            // 骨格位置の表示
            ShowSkeleton( e );
        }

        private void ShowSkeleton( AllFramesReadyEventArgs e )
        {
            // キャンバスをクリアする
            canvas1.Children.Clear();

            // スケルトンフレームを取得する
            SkeletonFrame skeletonFrame = e.OpenSkeletonFrame();
            if ( skeletonFrame != null ) {
                // スケルトンデータを取得する
                Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo( skeletonData );

                // プレーヤーごとのスケルトンを描画する
                foreach ( var skeleton in skeletonData ) {
                    if ( skeleton.TrackingState == SkeletonTrackingState.Tracked ) {
                        // 骨格を描画する
                        foreach ( Joint joint in skeleton.Joints ) {
                            // 骨格の座標をカラー座標に変換する
                            ColorImagePoint point = kinect.MapSkeletonPointToColor( joint.Position, kinect.ColorStream.Format );

                            // 円を書く
                            canvas1.Children.Add( new Ellipse()
                            {
                                Margin = new Thickness( point.X, point.Y, 0, 0 ),
                                Fill = new SolidColorBrush( Colors.Black ),
                                Width = 20,
                                Height = 20,
                            } );
                        }
                    }
                }
            }
        }
    }
}

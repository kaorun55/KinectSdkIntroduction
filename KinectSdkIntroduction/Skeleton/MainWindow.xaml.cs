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

            // Disposableなのでusingでくくる
            using ( SkeletonFrame skeletonFrame = e.OpenSkeletonFrame() ) {
                if ( skeletonFrame != null ) {
                    // 骨格位置の表示
                    ShowSkeleton( skeletonFrame );
                }
            }
        }

        private void ShowSkeleton( SkeletonFrame skeletonFrame )
        {
            // キャンバスをクリアする
            canvasSkeleton.Children.Clear();

            // スケルトンデータを取得する
            Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
            skeletonFrame.CopySkeletonDataTo( skeletonData );

            // プレーヤーごとのスケルトンを描画する
            foreach ( var skeleton in skeletonData ) {
                // 追跡されているプレイヤー
                if ( skeleton.TrackingState == SkeletonTrackingState.Tracked ) {
                    // 骨格を描画する
                    foreach ( Joint joint in skeleton.Joints ) {
                        // 追跡されている骨格
                        if ( joint.TrackingState != JointTrackingState.NotTracked ) {
                            // 骨格の座標をカラー座標に変換する
                            ColorImagePoint point = kinect.MapSkeletonPointToColor( joint.Position, kinect.ColorStream.Format );

                            // 円を書く
                            canvasSkeleton.Children.Add( new Ellipse()
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

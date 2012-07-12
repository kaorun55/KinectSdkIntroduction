using System;
using System.Linq;
using System.Windows;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Collections.Generic;
using System.Diagnostics;

namespace FaceTracking
{
    static class KinectExtensions
    {
        public static byte[] ToPixelData( this ColorImageFrame colorFrame )
        {
            byte[] pixels = new byte[colorFrame.PixelDataLength];
            colorFrame.CopyPixelDataTo( pixels );
            return pixels;
        }

        public static short[] ToPixelData( this DepthImageFrame depthFrame )
        {
            short[] depth = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo( depth );
            return depth;
        }

        public static Skeleton[] ToSkeletonData( this SkeletonFrame frame )
        {
            Skeleton[] skeletons = new Skeleton[frame.SkeletonArrayLength];
            frame.CopySkeletonDataTo( skeletons );
            return skeletons;
        }

        public static IEnumerable<Skeleton> GetTrackedSkeletons( this SkeletonFrame skeletonFrame )
        {
            return from s in skeletonFrame.ToSkeletonData()
                   where s.TrackingState == SkeletonTrackingState.Tracked
                   select s;
        }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinect;
        FaceTracker faceTracker;

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

                kinect.DepthStream.Range = DepthRange.Near;
                kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                kinect.SkeletonStream.EnableTrackingInNearRange = true;

                // Kinectの動作を開始する
                kinect.Start();

                // 顔追跡用のインスタンスを生成する
                faceTracker = new FaceTracker( kinect );
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
                using ( DepthImageFrame depthFrame = e.OpenDepthImageFrame() ) {
                    using ( SkeletonFrame skeletonFrame = e.OpenSkeletonFrame() ) {
                        if ( colorFrame != null ) {
                            imageRgbCamera.Source = colorFrame.ToBitmapSource();
                        }

                        // RGB、Depth、Skeletonのフレームが取得できたら、追跡されているSkeletonに対して顔を認識させる
                        if ( (colorFrame != null) && (depthFrame != null) && (skeletonFrame != null) ) {
                            foreach ( var skeleton in skeletonFrame.GetTrackedSkeletons() ) {
                                FaceTracking( colorFrame, depthFrame, skeleton );
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 顔を追跡する
        /// </summary>
        /// <param name="colorFrame"></param>
        /// <param name="depthFrame"></param>
        /// <param name="skeleton"></param>
        private void FaceTracking( ColorImageFrame colorFrame, DepthImageFrame depthFrame, Skeleton skeleton )
        {
            var faceFrame = faceTracker.Track( colorFrame.Format, colorFrame.ToPixelData(),
                depthFrame.Format, depthFrame.ToPixelData(), skeleton );
            if ( faceFrame.TrackSuccessful ) {
                // 四角を移動させる
                rectFace.Margin = new Thickness( faceFrame.FaceRect.Left, faceFrame.FaceRect.Top, 0, 0 );
                rectFace.Width = faceFrame.FaceRect.Width;
                rectFace.Height = faceFrame.FaceRect.Height;

                rectFace.Visibility = System.Windows.Visibility.Visible;
            }
            else {
                rectFace.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}

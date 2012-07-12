using System;
using System.Linq;
using System.Windows;
using Coding4Fun.Kinect.Wpf;
using Microsoft.Kinect;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;

namespace SkeletonBoneMatrix
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
            using ( SkeletonFrame skeletonFrame = e.OpenSkeletonFrame() ) {
                if ( skeletonFrame != null ) {
                    foreach ( var skeleton in skeletonFrame.GetTrackedSkeletons() ) {
                        RotateRectangle( skeleton );
                    }
                }
            }
        }

        /// <summary>
        /// 顔の位置で四角形を回転させる
        /// </summary>
        /// <param name="skeleton"></param>
        private void RotateRectangle( Skeleton skeleton )
        {
            var head = skeleton.Joints[JointType.Head];
            if ( head.TrackingState != JointTrackingState.NotTracked ) {
                // 四角を移動させる
                var point = kinect.MapSkeletonPointToColor( head.Position, kinect.ColorStream.Format );
                rectFace.Margin = new Thickness( point.X - (rectFace.Width / 2),
                                                 point.Y - (rectFace.Height / 2), 0, 0 );

                // 四角を回転させる
                var headMatrix = skeleton.BoneOrientations[JointType.Head].AbsoluteRotation.Matrix;

                Matrix matrix = new Matrix( -headMatrix.M11, headMatrix.M12,
                                            -headMatrix.M21, headMatrix.M22,
                                            0, 0 );

                rectFace.RenderTransform = new MatrixTransform( matrix );
            }
        }
    }
}

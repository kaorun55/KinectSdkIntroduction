using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;

namespace SimapleXnaSample
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        KinectSensor kinect;

        Texture2D imageRgb;
        byte[] rgbBuffer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager( this );
            Content.RootDirectory = "Content";


            // 画面の解像度を変更する
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            kinect = (from k in KinectSensor.KinectSensors
                      where k.Status == KinectStatus.Connected
                      select k).FirstOrDefault();
            if ( kinect == null ) {
                throw new Exception( "利用可能なKinectがありません" );
            }

            kinect.ColorStream.Enable( ColorImageFormat.RgbResolution640x480Fps30 );

            kinect.Start();

            rgbBuffer = new byte[kinect.ColorStream.FramePixelDataLength];


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch( GraphicsDevice );

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update( GameTime gameTime )
        {
            // Allows the game to exit
            if ( GamePad.GetState( PlayerIndex.One ).Buttons.Back == ButtonState.Pressed )
                this.Exit();

            // TODO: Add your update logic here
            //ColorImageFrameを取得する
            using ( ColorImageFrame colorImageFrame = kinect.ColorStream.OpenNextFrame(0) ) {
                if ( colorImageFrame != null ) {
                    colorImageFrame.CopyPixelDataTo( rgbBuffer );
                }
            }

            //XNA上に表示するために、もう一つ同じ長さのbyte配列を用意する
            byte[] xnaColorArray = new byte[rgbBuffer.Length];

            //RGBAの順序を入れ替える
            for ( int i = 0; i + 3 < rgbBuffer.Length; i += 4 ) {
                xnaColorArray[i + 0] = rgbBuffer[i + 2];    // R
                xnaColorArray[i + 1] = rgbBuffer[i + 1];    // G
                xnaColorArray[i + 2] = rgbBuffer[i + 0];    // B
                xnaColorArray[i + 3] = 255;//A
            }

            //テクスチャに画像データをセットする
            imageRgb = new Texture2D( GraphicsDevice, kinect.ColorStream.FrameWidth, kinect.ColorStream.FrameHeight );
            imageRgb.SetData<byte>( xnaColorArray );

            base.Update( gameTime );
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw( GameTime gameTime )
        {
            GraphicsDevice.Clear( Color.CornflowerBlue );

            spriteBatch.Begin();

            //カメラを描画
            spriteBatch.Draw( imageRgb, Vector2.Zero, Color.White );
            spriteBatch.End();

            base.Draw( gameTime );
        }

        static void Swap<T>( ref T lhs, ref T rhs )
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}

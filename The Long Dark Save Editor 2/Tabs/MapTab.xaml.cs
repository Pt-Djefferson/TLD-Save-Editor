using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using The_Long_Dark_Save_Editor_2.Helpers;

namespace The_Long_Dark_Save_Editor_2.Tabs
{

    public partial class MapTab : UserControl
    {

        private MapInfo mapInfo;
        private bool mouseDown;
        private Point clickPosition;
        private Point lastMousePosition;

        private Point playerPosition;

        private string region;

        public MapTab()
        {
            InitializeComponent();

            MainWindow.Instance.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(MainWindow.Instance.CurrentSave))
                {
                    Debug.WriteLine("Currentsave changed");
                    if (MainWindow.Instance.CurrentSave == null)
                    {
                        region = null;
                        UpdateMap();
                        return;
                    }
                    region = MainWindow.Instance.CurrentSave.Boot.m_SceneName.Value;
                    playerPosition = new Point(MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[0], MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[2]);
                    UpdateMap();
                    var saveGamePosition = MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition;
                    saveGamePosition.CollectionChanged += (sender2, e2) =>
                    {

                        if ((e2.NewStartingIndex == 0 && saveGamePosition[0] != (float)playerPosition.X) || (e2.NewStartingIndex == 2 && saveGamePosition[2] != (float)playerPosition.Y))
                        {
                            playerPosition.X = saveGamePosition[0];
                            playerPosition.Y = saveGamePosition[2];
                            UpdatePlayerPosition();
                        }
                    };
                    MainWindow.Instance.CurrentSave.Boot.m_SceneName.PropertyChanged += (sender2, e2) =>
                    {
                        if (e2.PropertyName == "Value")
                        {
                            region = MainWindow.Instance.CurrentSave.Boot.m_SceneName.Value;
                            Debug.WriteLine("New region: " + region);
                            UpdateMap();
                        }
                    };
                }
            };

        }

        private void UpdateMap()
        {
            if (!IsLoaded)
                return;
            if (region == null)
            {
                mapImage.Source = null;
                mapInfo = null;
                player.Visibility = Visibility.Hidden;
                canvasLabel.Text = "";
                canvasLabel.Visibility = Visibility.Visible;
                return;
            }
            if (!MapDictionary.MapExists(region))
            {
                mapImage.Source = null;
                mapInfo = null;
                player.Visibility = Visibility.Hidden;
                canvasLabel.Text = "No map found for current region";
                canvasLabel.Visibility = Visibility.Visible;
                return;
            }
            player.Visibility = Visibility.Visible;
            canvasLabel.Visibility = Visibility.Hidden;

            mapInfo = MapDictionary.GetMapInfo(region);
            mapImage.Source = ((Image)Resources[region]).Source;
            mapImage.Width = mapInfo.width;
            mapImage.Height = mapInfo.height;

            double wScale = canvas.ActualWidth / mapInfo.width;
            double hScale = canvas.ActualHeight / mapInfo.height;
            scaleMap.ScaleX = Math.Max(Math.Min(wScale, hScale), 0.5);
            scaleMap.ScaleY = Math.Max(Math.Min(wScale, hScale), 0.5);

            //SetPosition(0, 0);
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            /*
            if (mapInfo == null) return;

            mouseDown = true;
            clickPosition = e.GetPosition(canvas);
            lastMousePosition = clickPosition;
            */
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            /*
            if (mapInfo == null) return;

            mouseDown = false;
            canvas.ReleaseMouseCapture();
            if (e.GetPosition(canvas) == clickPosition)
            {
                var x = e.GetPosition(mapImage).X;
                var y = e.GetPosition(mapImage).Y;

                playerPosition.X = (x - mapInfo.origo.X) / mapInfo.pixelsPerCoordinate;
                playerPosition.Y = (y - mapInfo.origo.Y) / -mapInfo.pixelsPerCoordinate;
                UpdatePlayerPosition();
                MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[0] = (float)playerPosition.X;
                MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[2] = (float)playerPosition.Y;
            }
            */
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {

            var x = e.GetPosition(mapLayer).X / mapLayer.ActualWidth;
            var y = e.GetPosition(mapLayer).Y / mapLayer.ActualHeight;
            x = x > 0 ? x : 0;
            x = x < 1 ? x : 1;
            y = y > 0 ? y : 0;
            y = y < 1 ? y : 1;
            var x0 = mapLayer.RenderTransformOrigin.X;
            var y0 = mapLayer.RenderTransformOrigin.Y;
            var dX = (-1) * (x - x0) * mapLayer.ActualWidth * scaleMap.ScaleX;
            var dY = (-1) * (y - y0) * mapLayer.ActualHeight * scaleMap.ScaleY;

            label1.Content = @"X: " + x.ToString() + @" Y: " + y.ToString();
            label2.Content = @"X: " + mapLayer.RenderTransformOrigin.X.ToString() + @" Y: " + mapLayer.RenderTransformOrigin.Y.ToString();

            //translateMap.X += dX;
            //translateMap.Y += dY;
            mapLayer.RenderTransformOrigin = new Point(x, y);
            /*
            if (mapInfo == null) return;

            if (mouseDown)
            {
                canvas.CaptureMouse();
                var mousePos = e.GetPosition(canvas);

                var x = Canvas.GetLeft(mapImage) - (lastMousePosition.X - mousePos.X);
                var y = Canvas.GetTop(mapImage) - (lastMousePosition.Y - mousePos.Y);
                SetPosition(x, y);
                lastMousePosition = mousePos;
            }
            */
        }

        private void canvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (mapInfo == null) return;

            double zoom = e.Delta > 0 ? .1 * scaleMap.ScaleX : -.1 * scaleMap.ScaleX;

            //mapLayer.RenderTransformOrigin.X. = e.GetPosition(mapLayer).X;
            //mapLayer.RenderTransformOrigin.Y = e.GetPosition(mapLayer).X;
            var x0 = mapLayer.RenderTransformOrigin.X;
            var y0 = mapLayer.RenderTransformOrigin.Y;
            var x = e.GetPosition(mapLayer).X / mapLayer.ActualWidth;
            var y = e.GetPosition(mapLayer).Y / mapLayer.ActualHeight;
            x = x > 0 ? x : 0;
            x = x < 1 ? x : 1;
            y = y > 0 ? y : 0;
            y = y < 1 ? y : 1;
            var dX = (x - x0) * mapLayer.ActualWidth * (scaleMap.ScaleX);
            var dY = (y - y0) * mapLayer.ActualHeight * (scaleMap.ScaleY);
            label1.Content = @"X: " + x.ToString() + @" Y: " + y.ToString();
            //translateMap.X -= dX;
            ////translateMap.X = mapLayer.ActualWidth * dX / 2;
            //translateMap.Y -= dY;
            ////translateMap.Y = mapLayer.ActualHeight * dY / 2;
            //var poi = new Point(translateMap.X, translateMap.Y);
            
            var poi0 = mapLayer.PointToScreen(e.GetPosition(mapLayer));

            //var canvasX = Canvas.GetLeft(mapLayer);
            //var canvasY = Canvas.GetTop(mapLayer);
            mapLayer.RenderTransformOrigin = new Point(x, y);

            var poi1 = mapLayer.PointToScreen(e.GetPosition(mapLayer));
            translateMap.X += poi1.X - poi0.X;
            translateMap.Y += poi1.Y - poi0.Y;
            //Canvas.SetLeft(mapLayer, canvasX);
            //Canvas.SetTop(mapLayer, canvasY);
            scaleMap.ScaleX += zoom;
            scaleMap.ScaleY += zoom;

            /*
            var centerX = (-Canvas.GetLeft(mapImage) + canvas.ActualWidth / 2) / scale.ScaleX;
            var centerY = (-Canvas.GetTop(mapImage) + canvas.ActualHeight / 2) / scale.ScaleY;

            scale.ScaleX += zoom;
            scale.ScaleY += zoom;

            var x = -centerX * scale.ScaleX + canvas.ActualWidth / 2;
            var y = -centerY * scale.ScaleY + canvas.ActualHeight / 2;
            SetPosition(x, y);
            */
        }
        /*
        private void SetPosition(double x, double y)
        {
            Canvas.SetLeft(mapImage, x);
            Canvas.SetTop(mapImage, y);

            UpdatePlayerPosition(x, y);

            if (MainWindow.Instance.CurrentSave == null)
                return;
            MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[0] = (float)playerPosition.X;
            MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[2] = (float)playerPosition.Y;
        }
        */
        private void UpdatePlayerPosition()
        {
            //UpdatePlayerPosition(Canvas.GetLeft(mapImage), Canvas.GetTop(mapImage));
        }

        private void UpdatePlayerPosition(double canvasXOffset, double canvasYOffset)
        {
            /*
            var playerCanvasX = (playerPosition.X * mapInfo.pixelsPerCoordinate + mapInfo.origo.X) * scale.ScaleX + canvasXOffset;
            var playerCanvasY = (playerPosition.Y * -mapInfo.pixelsPerCoordinate + mapInfo.origo.Y) * scale.ScaleY + canvasYOffset;

            Canvas.SetLeft(player, playerCanvasX);
            Canvas.SetTop(player, playerCanvasY);
            */
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMap();
        }
    }
}

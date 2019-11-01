using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        private ScaleTransform rbScaleTransform;
        private TranslateTransform rbTranslateTransform;

        private RadioButton rbButton;
        private Label rbLabel;

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
                    //playerPosition = new Point(MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[0], MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[2]);
                    playerPosition = new Point(-1197.492, 998.6943);
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
            return;
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
            scale.ScaleX = Math.Max(Math.Min(wScale, hScale), 0.5);
            scale.ScaleY = Math.Max(Math.Min(wScale, hScale), 0.5);

            scaleVB.ScaleX = scale.ScaleX;
            scaleVB.ScaleY = scale.ScaleY;

            //rbLayer.Width = mapImage.Width;
            //rbLayer.Height = mapImage.Height;

            rbScaleTransform = new ScaleTransform();
            rbTranslateTransform = new TranslateTransform();
            TransformGroup rbTransformGroup = new TransformGroup();
            rbTransformGroup.Children.Add(rbScaleTransform);
            rbTransformGroup.Children.Add(rbTranslateTransform);

            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (canvas.Children[i] is RadioButton)
                {
                    canvas.Children.RemoveAt(i);
                }
            }

            rbButton = new RadioButton();
            canvas.Children.Add(rbButton);
            //combo.Visibility = Visibility.Visible;
            //Canvas.SetZIndex(combo, (int)99);
            //combo.Content = "";
            rbButton.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            rbButton.RenderTransformOrigin = new Point(0.5, 0.5);
            rbButton.RenderTransform = rbTransformGroup;

            

            SetPosition(0, 0);
            rbScaleTransform.ScaleX = scale.ScaleX;
            rbScaleTransform.ScaleY = scale.ScaleY;
            //rbTranslateTransform.X = - combo.ActualWidth;
            //rbTranslateTransform.Y = - combo.ActualHeight;
            //Canvas.SetLeft(rbButton, 10);
            //Canvas.SetTop(rbButton, 10);
            Canvas.SetLeft(rbButton, ((-1197.492 * mapInfo.pixelsPerCoordinate + mapInfo.origo.X) * scale.ScaleX + Canvas.GetLeft(mapImage)) - 18 / 2);
            Canvas.SetTop(rbButton, ((998.6943 * -mapInfo.pixelsPerCoordinate + mapInfo.origo.Y) * scale.ScaleY + Canvas.GetTop(mapImage)) - 18 / 2);
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (mapInfo == null) return;

            mouseDown = true;
            clickPosition = e.GetPosition(canvas);
            lastMousePosition = clickPosition;
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
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
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
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
        }

        private void canvas_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (mapInfo == null) return;

            double zoom = e.Delta > 0 ? .1 * scale.ScaleX : -.1 * scale.ScaleX;

            var centerX = (-Canvas.GetLeft(mapImage) + canvas.ActualWidth / 2) / scale.ScaleX;
            var centerY = (-Canvas.GetTop(mapImage) + canvas.ActualHeight / 2) / scale.ScaleY;

            scale.ScaleX += zoom;
            scale.ScaleY += zoom;

            //scaleR.ScaleX += zoom;
            //scaleR.ScaleY += zoom;

            //scaleRBLayer.ScaleX += zoom;
            //scaleRBLayer.ScaleY += zoom;

            scaleVB.ScaleX += zoom;
            scaleVB.ScaleY += zoom;

            rbScaleTransform.ScaleX += zoom;
            rbScaleTransform.ScaleY += zoom;

            var x = -centerX * scale.ScaleX + canvas.ActualWidth / 2;
            var y = -centerY * scale.ScaleY + canvas.ActualHeight / 2;
            SetPosition(x, y);
        }

        private void SetPosition(double x, double y)
        {
            rbTranslateTransform.X = (-1197.492 * mapInfo.pixelsPerCoordinate + mapInfo.origo.X) * (scale.ScaleX - 0.5) + x;
            rbTranslateTransform.Y = (998.6943 * -mapInfo.pixelsPerCoordinate + mapInfo.origo.Y) * (scale.ScaleY - 0.5) + y;

            Canvas.SetLeft(mapImage, x);
            Canvas.SetTop(mapImage, y);
            Canvas.SetLeft(rbLayer, x);
            Canvas.SetTop(rbLayer, y);

            UpdatePlayerPosition(x, y);

            //rbTranslateTransform.X = x;// + 18 * scale.ScaleX;
            //rbTranslateTransform.Y = y;// + 18 * scale.ScaleY;
            scaleLabel.Content = "X:" + rbTranslateTransform.X.ToString() +
                " Y:" + rbTranslateTransform.Y.ToString() + " S:" + scale.ScaleY.ToString();

            if (MainWindow.Instance.CurrentSave == null)
                return;
            MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[0] = (float)playerPosition.X;
            MainWindow.Instance.CurrentSave.Global.PlayerManager.m_SaveGamePosition[2] = (float)playerPosition.Y;
        }

        private void UpdatePlayerPosition()
        {
            UpdatePlayerPosition(Canvas.GetLeft(mapImage), Canvas.GetTop(mapImage));
        }

        private void UpdatePlayerPosition(double canvasXOffset, double canvasYOffset)
        {
            var playerCanvasX = (playerPosition.X * mapInfo.pixelsPerCoordinate + mapInfo.origo.X) * scale.ScaleX + canvasXOffset;
            var playerCanvasY = (playerPosition.Y * -mapInfo.pixelsPerCoordinate + mapInfo.origo.Y) * scale.ScaleY + canvasYOffset;

            playerLabel.Content = "X:" + (playerCanvasX - Canvas.GetLeft(rbButton) - 9).ToString() +
                " Y:" + (playerCanvasY - Canvas.GetTop(rbButton) - 9).ToString() + " S:" + scale.ScaleX.ToString();

            Canvas.SetLeft(player, playerCanvasX);
            Canvas.SetTop(player, playerCanvasY);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateMap();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls;
using PPI;

namespace Testbed_Demo
{
    public sealed partial class MainPage : Page
    {
        private PPITouchDisplay _ppi;

        public MainPage()
        {
            this.InitializeComponent();

            _ppi = new PPITouchDisplay(this);
            _ppi.PPITouchDown += new PPITouchHandler(MyTouchDownFunc);
            _ppi.PPITouchMove += new PPITouchHandler(MyTouchMoveFunc);
            _ppi.PPITouchUp += new PPITouchHandler(MyTouchUpFunc);
        }

        private void MyTouchDownFunc(PPITouchOval oval)
        {
            DrawEllipse(oval);
            CreateInfoPop(oval);
        }

        private void MyTouchMoveFunc(PPITouchOval oval)
        {
            DrawEllipse(oval);
            UpdateInfoPop(oval);
        }

        private void MyTouchUpFunc(PPITouchOval oval)
        {
            DrawEllipse(oval);
            DestroyInfoPop(oval);
        }
        

        private void DrawEllipse(PPITouchOval oval)
        {
            if (oval == null) { return; }
            var ellipse = new Ellipse();
            ellipse.Stroke = new SolidColorBrush(Windows.UI.Colors.SteelBlue);
            ellipse.StrokeThickness = 1;
            ellipse.Width = oval.MajorAxis;
            ellipse.Height = oval.MinorAxis;
            ellipse.Rotation = (float)(-oval.Angle);
            ellipse.Translation = new Vector3(
                (float)(oval.CenterX - oval.MajorAxis / 2),
                (float)(oval.CenterY - oval.MinorAxis / 2), 0);
            Container.Children.Add(ellipse);
        }

        private void CreateInfoPop(PPITouchOval oval)
        {
            TextBlock pointerDetails = new TextBlock();
            pointerDetails.Name = oval.Id.ToString();
            pointerDetails.Foreground = new SolidColorBrush(Windows.UI.Colors.Blue);
            pointerDetails.Text = oval.DebugInfo;
            TranslateTransform x = new TranslateTransform();
            x.X = oval.CenterX + 20;
            x.Y = oval.CenterY + 20;
            pointerDetails.RenderTransform = x;
            Container.Children.Add(pointerDetails);
        }

        private void UpdateInfoPop(PPITouchOval oval)
        {
            foreach (var pointerDetails in Container.Children)
            {
                if (pointerDetails.GetType().ToString() == "Windows.UI.Xaml.Controls.TextBlock")
                {
                    TextBlock textBlock = (TextBlock)pointerDetails;
                    if (textBlock.Name == oval.Id.ToString())
                    {
                        TranslateTransform x = new TranslateTransform();
                        x.X = oval.CenterX + 20;
                        x.Y = oval.CenterY + 20;
                        pointerDetails.RenderTransform = x;
                        textBlock.Text = oval.DebugInfo;
                    }
                }
            }
        }

        private void DestroyInfoPop(PPITouchOval oval)
        {
            foreach (var pointerDetails in Container.Children)
            {
                if (pointerDetails.GetType().ToString() == "Windows.UI.Xaml.Controls.TextBlock")
                {
                    TextBlock textBlock = (TextBlock)pointerDetails;
                    if (textBlock.Name ==oval.Id.ToString())
                    {
                        Container.Children.Remove(pointerDetails);
                    }
                }
            }
        }
    }
}

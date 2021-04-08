using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using PPI;
using System.Numerics;

namespace Testbed_Demo
{
    public sealed partial class MainPage : Page
    {
        PPIHandler handler;

        public MainPage()
        {
            this.InitializeComponent();
            this.handler = new PPIHandler(this);

            /*
            PointerPressed += new PointerEventHandler(TouchDown);
            PointerMoved += new PointerEventHandler(TouchMove);
            PointerReleased += new PointerEventHandler(TouchRelease);
            PointerCaptureLost += new PointerEventHandler(TouchLost);
            */
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            handler.onTouchDown(e);
            // DEBUG
            PointerPoint ptrPt = e.GetCurrentPoint(this);
            DrawEllipse(handler.GetTouchOval(ptrPt, "Down"));
            CreateInfoPop(handler.GetTouchOval(ptrPt, "Down"), ptrPt);
        }


        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            handler.onTouchMove(e);
            // DEBUG
            PointerPoint ptrPt = e.GetCurrentPoint(this);
            DrawEllipse(handler.GetTouchOval(ptrPt, "Move"));
            UpdateInfoPop(handler.GetTouchOval(ptrPt, "Move"), ptrPt);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            handler.onTouchUp(e);
            // DEBUG
            PointerPoint ptrPt = e.GetCurrentPoint(this);
            DestroyInfoPop(ptrPt);
        }

        protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
        {
            // DEBUG
            PointerPoint ptrPt = e.GetCurrentPoint(this);
            DestroyInfoPop(ptrPt);
        }

        void DrawEllipse(PPITouchPointOval oval)
        {
            if (oval == null) { return; }
            var ellipse = new Ellipse();
            ellipse.Stroke = new SolidColorBrush(Windows.UI.Colors.SteelBlue);
            ellipse.StrokeThickness = 1;
            ellipse.Width = oval.MajorAxis;
            ellipse.Height = oval.MinorAxis;
            ellipse.Rotation = (float)(- oval.Angle);
            // Canvas.SetLeft(ellipse, oval.CenterX); // - oval.MajorAxis / 2.0);
            // Canvas.SetTop(ellipse, oval.CenterY); // - oval.MinorAxis / 2.0);
            ellipse.Translation = new Vector3(
                (float)(oval.CenterX - oval.MajorAxis / 2),
                (float)(oval.CenterY - oval.MinorAxis / 2), 0);
            // ellipse.RotationAxis = new Vector3((float)0, (float)0, (float)1); ;
            Container.Children.Add(ellipse);
        }

        void CreateInfoPop(PPITouchPointOval oval, PointerPoint ptrPt)
        {
            TextBlock pointerDetails = new TextBlock();
            pointerDetails.Name = ptrPt.PointerId.ToString();
            pointerDetails.Foreground = new SolidColorBrush(Windows.UI.Colors.Blue);
            pointerDetails.Text = handler.GetTouchDataDisplay(oval, ptrPt);
            TranslateTransform x = new TranslateTransform();
            x.X = ptrPt.Position.X + 20;
            x.Y = ptrPt.Position.Y + 20;
            pointerDetails.RenderTransform = x;
            Container.Children.Add(pointerDetails);
        }

        void UpdateInfoPop(PPITouchPointOval oval, PointerPoint ptrPt)
        {
            foreach (var pointerDetails in Container.Children)
            {
                if (pointerDetails.GetType().ToString() == "Windows.UI.Xaml.Controls.TextBlock")
                {
                    TextBlock textBlock = (TextBlock)pointerDetails;
                    if (textBlock.Name == ptrPt.PointerId.ToString())
                    {
                        // To get pointer location details, we need extended pointer info.
                        // We get the pointer info through the getCurrentPoint method
                        // of the event argument. 
                        TranslateTransform x = new TranslateTransform();
                        x.X = ptrPt.Position.X + 20;
                        x.Y = ptrPt.Position.Y + 20;
                        pointerDetails.RenderTransform = x;
                        textBlock.Text = handler.GetTouchDataDisplay(oval, ptrPt);
                    }
                }
            }
        }

        void DestroyInfoPop(PointerPoint ptrPt)
        {
            foreach (var pointerDetails in Container.Children)
            {
                if (pointerDetails.GetType().ToString() == "Windows.UI.Xaml.Controls.TextBlock")
                {
                    TextBlock textBlock = (TextBlock)pointerDetails;
                    if (textBlock.Name == ptrPt.PointerId.ToString())
                    {
                        Container.Children.Remove(pointerDetails);
                    }
                }
            }
        }
    }
}

using System;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace PPI
{
    public delegate void PPITouchHandler(PPITouchOval oval);

    public class PPITouchOval
    {
        public uint Id; 
        public double CenterX;
        public double CenterY;
        public double Angle;
        public double MajorAxis;
        public double MinorAxis;
        public double ContactArea; // Calculated from major and minor axes
        public long Time;
        public String Type;
        public String DebugInfo;

        public PPITouchOval(uint id, double x, double y, double angle, double majorAxis, double minorAxis, long time, String type)
        {
            this.Id = id;
            this.CenterX = x;
            this.CenterY = y;
            this.Angle = angle;
            this.MajorAxis = majorAxis;
            this.MinorAxis = minorAxis;
            this.ContactArea = Math.PI * majorAxis * minorAxis / 4.0;
            this.Time = time;
            this.Type = type;
            this.DebugInfo = "";
        }
    }

    public class PPITouchDisplay
    {
        Page _page = null;

        public event PPITouchHandler PPITouchDown;
        public event PPITouchHandler PPITouchMove;
        public event PPITouchHandler PPITouchUp;

        private int SCREENHEIGHT_MM = 742;
        private int SCREENWIDTH_MM = 1319;
        private int SCREENHEIGHT_PX = 1080;
        private int SCREENWIDTH_PX = 1920;

        private int MAJORAXIS_USAGEID = 73;
        private int MINORAXIS_USAGEID = 72;

        public PPITouchDisplay(Page page)
        {
            page.PointerPressed += new PointerEventHandler(onTouchDown);
            page.PointerMoved += new PointerEventHandler(onTouchMove);
            page.PointerReleased += new PointerEventHandler(onTouchUp);
            page.PointerCaptureLost += new PointerEventHandler(onTouchUp);
            _page = page;
        }

        private void onTouchDown(Object sender, PointerRoutedEventArgs e)
        {
            PointerPoint ptrPt = e.GetCurrentPoint(_page);
            PPITouchOval oval = GetTouchOval(ptrPt, "Down");
            // Invoke custom touch down function written by end user
            PPITouchDown?.Invoke(oval);
        }

        private void onTouchMove(Object sender, PointerRoutedEventArgs e)
        {
            PointerPoint ptrPt = e.GetCurrentPoint(_page);
            PPITouchOval oval = GetTouchOval(ptrPt, "Move");
            // Invoke custom touch move function written by end user
            PPITouchMove?.Invoke(oval);
        }

        private void onTouchUp(Object sender, PointerRoutedEventArgs e)
        {
            PointerPoint ptrPt = e.GetCurrentPoint(_page);
            PPITouchOval oval = GetTouchOval(ptrPt, "Up");
            // Invoke custom touch up function written by end user
            PPITouchUp?.Invoke(oval);
        }

        // This seems right, but again I didn't find any useful documentation on it
        // and the "5" was a random value of my guess based on pattern matching
        private double UnitToPixel(double val)
        {
            return val / 5 * SCREENWIDTH_PX / SCREENWIDTH_MM;
        }

        private double PixelToMM(double val)
        {
            return val * SCREENWIDTH_MM / SCREENWIDTH_PX;
        }

        private PPITouchOval GetTouchOval(PointerPoint ptrPt, String type)
        {
            if (ptrPt == null || ptrPt.PointerDevice.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                return null;
            }
            uint id = ptrPt.PointerId;
            double x = ptrPt.Position.X;
            double y = ptrPt.Position.Y;
            double angle = ptrPt.Properties.Orientation;
            long time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            double majorAxis = 0;
            double minorAxis = 0;
            int count = ptrPt.PointerDevice.SupportedUsages.Count;
            for (int i = 0; i < count; i++)
            {
                if (ptrPt.PointerDevice.SupportedUsages[i].Usage == MAJORAXIS_USAGEID)
                {
                    majorAxis = UnitToPixel(ptrPt.Properties.GetUsageValue(
                        ptrPt.PointerDevice.SupportedUsages[i].UsagePage,
                        ptrPt.PointerDevice.SupportedUsages[i].Usage
                        ) * ptrPt.PointerDevice.SupportedUsages[i].PhysicalMultiplier);
                }
                else if (ptrPt.PointerDevice.SupportedUsages[i].Usage == MINORAXIS_USAGEID)
                {
                    minorAxis = UnitToPixel(ptrPt.Properties.GetUsageValue(
                        ptrPt.PointerDevice.SupportedUsages[i].UsagePage,
                        ptrPt.PointerDevice.SupportedUsages[i].Usage
                        ) * ptrPt.PointerDevice.SupportedUsages[i].PhysicalMultiplier);
                }
            }
            // This shouldn't happen, but just in case
            if (minorAxis > majorAxis)
            {
                double tmp = majorAxis;
                majorAxis = minorAxis;
                minorAxis = tmp;
            }
            PPITouchOval oval = new PPITouchOval(id, x, y, angle, majorAxis, minorAxis, time, type);
            String detail = GetDebugInfo(oval, ptrPt);
            oval.DebugInfo = detail;
            return oval;
        }

        private String GetDebugInfo(PPITouchOval oval, PointerPoint ptrPt)
        {
            if (oval == null) { return ""; }
            String details = "";
            switch (ptrPt.PointerDevice.PointerDeviceType)
            {
                case Windows.Devices.Input.PointerDeviceType.Touch:
                    details += "\nPointer type: touch";
                    if (oval != null)
                    {
                        details += "\nCenterX: " + oval.CenterX;
                        details += "\nCenterY: " + oval.CenterX;
                        details += "\nAngle: " + oval.Angle;
                        details += "\nMajorAxis: " + PixelToMM(oval.MajorAxis);
                        details += "\nMinorAxis: " + PixelToMM(oval.MinorAxis);
                        details += "\nContactArea: " + oval.ContactArea;
                        details += "\nTime: " + oval.Time;
                    }
                    for (int i = 0; i < ptrPt.PointerDevice.SupportedUsages.Count; i++)
                    {

                        details += "\nTouch Data: " + ptrPt.Properties.GetUsageValue(ptrPt.PointerDevice.SupportedUsages[i].UsagePage,
                            ptrPt.PointerDevice.SupportedUsages[i].Usage) * ptrPt.PointerDevice.SupportedUsages[i].PhysicalMultiplier;
                        // Information regarding the HID configuration
                        details += ", UsagePage = " + ptrPt.PointerDevice.SupportedUsages[i].UsagePage;
                        details += ", UsageID = " + ptrPt.PointerDevice.SupportedUsages[i].Usage;
                        // Provided by the HID to translate raw sensor data to Unit
                        details += ", PhysicalMultiplier = " + ptrPt.PointerDevice.SupportedUsages[i].PhysicalMultiplier;
                        details += ", Unit = " + ptrPt.PointerDevice.SupportedUsages[i].Unit;
                    }
                    break;
                default:
                    details += "\nPointer type: other";
                    break;
            }
            return details;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace PPI
{

    public class PPITouchPointOval
    {
        public double CenterX;
        public double CenterY;
        public double Angle;
        public double MajorAxis;
        public double MinorAxis;
        public double ContactArea; // Calculated from major and minor axes
        public long Time;
        public String Type;

        public PPITouchPointOval(double x, double y, double angle, double majorAxis, double minorAxis, long time)
        {
            this.CenterX = x;
            this.CenterY = y;
            this.Angle = angle;
            this.MajorAxis = majorAxis;
            this.MinorAxis = minorAxis;
            this.ContactArea = Math.PI * majorAxis * minorAxis;
            this.Time = time;
        }
    }

    public class PPITouchDisplay
    {

    }

    public class PPIHandler
    {
        Dictionary<uint, Windows.UI.Xaml.Input.Pointer> pointers;
        List<PPITouchPointOval> ovals = new List<PPITouchPointOval>();

        int SCREENHEIGHT_MM = 742;
        int SCREENWIDTH_MM = 1319;
        int SCREENHEIGHT_PX = 1080;
        int SCREENWIDTH_PX = 1920;

        int MAJORAXIS_USAGEID = 73;
        int MINORAXIS_USAGEID = 72;

        public PPIHandler()
        {
            pointers = new Dictionary<uint, Windows.UI.Xaml.Input.Pointer>();
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

        public void onTouchDown(PointerRoutedEventArgs e, PointerPoint ptrPt)
        {
            if (!pointers.ContainsKey(ptrPt.PointerId))
            {
                pointers[ptrPt.PointerId] = e.Pointer;
            }
            PPITouchPointOval oval = GetTouchOval(ptrPt);
            oval.Type = "Down";
            ovals.Add(oval);
        }

        public void onTouchMove(PointerRoutedEventArgs e, PointerPoint ptrPt) {
            PPITouchPointOval oval = GetTouchOval(ptrPt);
            oval.Type = "Move";
            ovals.Add(oval);
        }

        public void onTouchUp(PointerRoutedEventArgs e, PointerPoint ptrPt)
        {
            PPITouchPointOval oval = GetTouchOval(ptrPt);
            oval.Type = "Up";
            ovals.Add(oval);
            if (pointers.ContainsKey(ptrPt.PointerId))
            {
                pointers[ptrPt.PointerId] = null;
                pointers.Remove(ptrPt.PointerId);
            }
        }

        public PPITouchPointOval GetTouchOval(PointerPoint ptrPt)
        {
            if (ptrPt.PointerDevice.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Touch)
            {
                return null;
            }
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
            PPITouchPointOval oval = new PPITouchPointOval(x, y, angle, majorAxis, minorAxis, time);
            return oval;
        }

        public String GetTouchDataDisplay(PPITouchPointOval oval, PointerPoint ptrPt)
        {
            String details = "";
            switch (ptrPt.PointerDevice.PointerDeviceType)
            {
                case Windows.Devices.Input.PointerDeviceType.Touch:
                    details += "\nPointer type: touch";
                    if (oval != null) { 
                        details += "\nCenterX: " + oval.CenterX;
                        details += "\nCenterY: " + oval.CenterX;
                        details += "\nAngle: " + oval.Angle;
                        details += "\nMajorAxis (mm): " + PixelToMM(oval.MajorAxis);
                        details += "\nMinorAxis (mm): " + PixelToMM(oval.MinorAxis);
                        details += "\nContactArea: " + oval.ContactArea;
                        details += "\nTime: " + oval.Time;
                    }
                    for (int i = 0; i < ptrPt.PointerDevice.SupportedUsages.Count; i++)
                    {

                        details += "\nTouch Data: " + ptrPt.Properties.GetUsageValue(ptrPt.PointerDevice.SupportedUsages[i].UsagePage,
                            ptrPt.PointerDevice.SupportedUsages[i].Usage) * ptrPt.PointerDevice.SupportedUsages[i].PhysicalMultiplier;
                        details += " UsagePage = " + ptrPt.PointerDevice.SupportedUsages[i].UsagePage + ", UsageID = " + ptrPt.PointerDevice.SupportedUsages[i].Usage;
                        details += " PhysicalMultiplier = " + ptrPt.PointerDevice.SupportedUsages[i].PhysicalMultiplier;
                        details += " Unit = " + ptrPt.PointerDevice.SupportedUsages[i].Unit;
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

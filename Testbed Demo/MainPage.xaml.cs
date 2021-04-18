using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
// The above left for convenience in future use
// Only these two need to be imported
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

            _ppi = new PPITouchDisplay(this, Container, true);
            _ppi.PPITouchDown += new PPITouchHandler(MyTouchDownFunc);
            _ppi.PPITouchMove += new PPITouchHandler(MyTouchMoveFunc);
            _ppi.PPITouchUp += new PPITouchHandler(MyTouchUpFunc);
        }

        // Empty custom functions
        private void MyTouchDownFunc(PPITouchOval oval)
        {
            return;
        }

        private void MyTouchMoveFunc(PPITouchOval oval)
        {
            return;
        }

        private void MyTouchUpFunc(PPITouchOval oval)
        {
            return;
        }
    }
}

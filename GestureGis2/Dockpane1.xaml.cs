using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace GestureGis2
{
    /// <summary>
    /// Interaction logic for Dockpane1View.xaml
    /// </summary>
    public partial class Dockpane1View : UserControl
    {
        Point currentPoint = new Point();
        

        List<Point> gesture = new List<Point>();

        public Dockpane1View()
        {
            InitializeComponent();
        }

        private void sketchPad_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Line line = new Line();
                gesture.Add(e.GetPosition(this));
                line.Stroke = SystemColors.ControlDarkDarkBrush;
                line.X1 = currentPoint.X;
                line.Y1 = currentPoint.Y;
                line.X2 = e.GetPosition(this).X;
                line.Y2 = e.GetPosition(this).Y;

                currentPoint = e.GetPosition(this);

                sketchPad.Children.Add(line);
            }
        }

        private void sketchPad_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                gesture.Add(e.GetPosition(this));
                currentPoint = e.GetPosition(this);
            }
                
        }

        private void sketchPad_MouseUp(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gesture = new List<Point>();
            sketchPad.Children.Clear();
        }
    }
}

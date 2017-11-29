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
    /// Interaction logic for TrainingDockView.xaml
    /// </summary>
    public partial class TrainingDockView : UserControl
    {
        Point currentPoint = new Point();
        List<List<List<Point>>> gestureSet = new List<List<List<Point>>>();
        List<List<Point>> gesturePoints = new List<List<Point>>();
        List<Point> gesture = new List<Point>();
        public TrainingDockView()
        {
            InitializeComponent();
        }

        private void trainPad_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                gesture.Add(e.GetPosition(this));
                currentPoint = e.GetPosition(this);
            }
               
        }

        private void trainPad_MouseMove(object sender, MouseEventArgs e)
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

                trainPad.Children.Add(line);
            }
        }

        private void Button_AddNewExample(object sender, RoutedEventArgs e)
        {
            //ToDo: Add error check if no sketch is drawn as an example
            if (gesture.Count > 0)
            {
                gesturePoints.Add(gesture);
                gesture = new List<Point>();
                trainPad.Children.Clear();
            }
            
        }

        private void Button_AddNewGesture(object sender, RoutedEventArgs e)
        {
            //ToDo: Add error check if no examples have been added to the new gesture
            if (gesturePoints.Count > 0)
            {
                gestureSet.Add(gesturePoints);
                gesturePoints = new List<List<Point>>();
                trainPad.Children.Clear();
            }
        }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArchPlaneGenerator
{
    public partial class MainWindow : Window
    {

        #region Filds
        Point StartPoint;
        Point EndPoint;
        Point CenterPoint;
        Line line ;
        Path HorizontalDoor;
        Path VerticalDoor;
        double LogoAreaHieght;
        double OptionsAreaWidth;
        int DoorCounters =0;
        int WindowCounters=0;
        int LinesCounter = 0;
        #endregion
        public MainWindow()
        {
            //Application Icon
            this.Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/Logo.jpg"));
            InitializeComponent();
            StartPoint = new Point();
            EndPoint = new Point();
            line = new Line();//declared here to be seen in both events to identify whether the mouse is over the line or not 
            LogoAreaHieght = LogoArea.ActualHeight;
            OptionsAreaWidth = OptionsArea.ActualWidth;
            //this event handeler is intiated so that we can delete any element when we hover on this element and press delete
            this.AddHandler(KeyDownEvent, new KeyEventHandler(InkCanvas_KeyDown), true);
        }
        //the actions that will be taken when this view is oaded 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //to not draw an ink 
                InkCanvas.EditingMode = InkCanvasEditingMode.None;
                Line L = new Line();
               
                L.StrokeDashArray.Add(1.5) ;
                for (int i = 0; i < 100; i++)
                {
                    L = new Line();
                    L.X1 = i;
                    L.Y1 = i;
                    L.X2 = 100;
                    L.Y2 = i;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// this Event Draws all the elements
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Vector VStartPoint;
                Vector VEndPoint;
                Vector VLineVector;
                double VAngel=-1;
                bool IsOver = false;
                Line L = InkCanvas.Children.OfType<Line>().FirstOrDefault(l => l.IsMouseOver);
                //Drawing Doors
                if (L!=null)
                {
                    VStartPoint = new Vector(L.X1, L.Y1);
                    VEndPoint = new Vector(L.X2, L.Y2);
                    VLineVector = VEndPoint - VStartPoint;
                    VAngel = Vector.AngleBetween(new Vector(1, 0), new Vector(VLineVector.X, VLineVector.Y));
                    if (VAngel == 0 || VAngel == 180)//in case of HL Line the following Code Will be executed 
                    {
                        InkCanvas.SetLeft(CreateHoriZontalDoors(e.GetPosition(this)), e.GetPosition(this).X - OptionsAreaWidth);
                        InkCanvas.SetTop(CreateHoriZontalDoors(e.GetPosition(this)), e.GetPosition(this).Y - LogoAreaHieght);
                        InkCanvas.Children.Add(CreateHoriZontalDoors(e.GetPosition(this)));
                        InkCanvas.UpdateDefaultStyle();
                        InkCanvas.UpdateLayout();
                        IsOver = true;
                    }
                    if (VAngel == 90 || VAngel == -90)//in case of VL Line the following Code Will be executed 
                    {
                        InkCanvas.SetLeft(CreateVerticalDoors(e.GetPosition(this)), e.GetPosition(this).X - OptionsAreaWidth);
                        InkCanvas.SetTop(CreateVerticalDoors(e.GetPosition(this)), e.GetPosition(this).Y - LogoAreaHieght);
                        InkCanvas.Children.Add(CreateVerticalDoors(e.GetPosition(this)));
                        InkCanvas.UpdateLayout();
                        IsOver = true;
                    }
                }
                Path P = InkCanvas.Children.OfType<Path>().FirstOrDefault(p => p.IsMouseOver);

                //Drawing Windows
                if (P!=null)
                {
                    if (P.Uid.Contains("HorizontalDoor"))
                    {
                        InkCanvas.Children.Remove(P);//replacing HLDoor With HLWindow
                        InkCanvas.SetLeft(CreateHorizontalWindows(CenterPoint), CenterPoint .X+ OptionsAreaWidth);
                        InkCanvas.SetTop(CreateHorizontalWindows(CenterPoint), CenterPoint.Y + LogoAreaHieght);
                        InkCanvas.Children.Add(CreateHorizontalWindows(CenterPoint));
                        InkCanvas.UpdateDefaultStyle();
                        InkCanvas.UpdateLayout();
                        IsOver = true;
                    }
                    if (P.Uid.Contains("VerticalDoor"))
                    {
                        InkCanvas.Children.Remove(P);//replacing VLDoor With VLWindow
                        InkCanvas.SetLeft(CreateVerticalWindows(CenterPoint), CenterPoint.X + OptionsAreaWidth);
                        InkCanvas.SetTop(CreateVerticalWindows(CenterPoint), CenterPoint.Y + LogoAreaHieght);
                        InkCanvas.Children.Add(CreateVerticalWindows(CenterPoint));
                        InkCanvas.UpdateDefaultStyle();
                        InkCanvas.UpdateLayout();
                        IsOver = true;
                    }
                }
                    #region Determine StartPoint For each line
                    //using the initial Condition to get the position of the start point
                    if (StartPoint.X == 0 && StartPoint.Y == 0 && IsOver == false)
                    {
                        StartPoint = e.GetPosition(this);
                        return;
                    }
                    if (EndPoint.X != 0 && EndPoint.Y != 0)
                    {
                        foreach (Line line in InkCanvas.Children.OfType<Line>().ToList()) //Delete the final guide line when clicking left button
                        {
                            if (line.Uid.Contains("GL"))
                            {
                                InkCanvas.Children.Remove(line);
                            }
                        }
                        InkCanvas.UpdateLayout();
                    //Creating the Actual Line
                    LogoAreaHieght = LogoArea.ActualHeight;
                    OptionsAreaWidth = OptionsArea.ActualWidth;
                    Point DrawingPoint = DetermineDrawingDirection(StartPoint, EndPoint);
                    line = new Line();
                    SolidColorBrush blackBrush = new SolidColorBrush();
                    blackBrush.Color = Colors.Black;
                    line.Stroke = blackBrush;
                    line.X1 = StartPoint.X - OptionsAreaWidth;
                    line.Y1 = StartPoint.Y - LogoAreaHieght;
                    line.X2 = DrawingPoint.X - OptionsAreaWidth;
                    line.Y2 = DrawingPoint.Y - LogoAreaHieght;
                    line.StrokeThickness = 4; //thickness is increased wo that the cursor can point in right way iwht the line 
                    line.Uid = $"Line{LinesCounter++}";
                    InkCanvas.Children.Add(line);
                    InkCanvas.UpdateLayout();
                    //Resetting to the intial Condirions 
                    StartPoint.X = 0;
                    StartPoint.Y = 0;
                    EndPoint.X = 0;
                    EndPoint.Y = 0;
                    }
                    #endregion
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// this Event is made to Draw Guid Lines When the Curser Moves
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_MouseMove_1(object sender, MouseEventArgs e)
        {
            try
            {
                //If Statement is made for Drawing Lines as the Curser Moves
                if (StartPoint.X != 0 && StartPoint.Y != 0)
                {
                    //getting the Position of the canged Positions
                    EndPoint = e.GetPosition(this);
                    //For Making a relative Position (need to be enhanced)
                    LogoAreaHieght = LogoArea.ActualHeight;
                    OptionsAreaWidth = OptionsArea.ActualWidth;
                    //this method is Created to make the lines draw in orthographic way 
                    Point DrawingPoint = DetermineDrawingDirection(StartPoint, EndPoint);
                    //Creating Line
                    line = new Line();
                    SolidColorBrush blackBrush = new SolidColorBrush();
                    blackBrush.Color = Colors.Black;
                    line.Stroke = blackBrush;
                    line.X1 = StartPoint.X - OptionsAreaWidth;
                    line.Y1 = StartPoint.Y - LogoAreaHieght;
                    line.X2 = DrawingPoint.X - OptionsAreaWidth;
                    line.Y2 = DrawingPoint.Y - LogoAreaHieght;
                    line.StrokeThickness = 4; //thickness is increased wo that the cursor can point in right way iwht the line 
                    line.Uid = $"GL{LinesCounter++}";
                    //Adding Line To OurCanvas
                    InkCanvas.BeginInit();
                    InkCanvas.Children.Add(line);
                    InkCanvas.EndInit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Dark Mode 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkBxDarkMode_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ChkBxDarkMode.IsChecked == true)
                {

                    InkCanvas.Background = Brushes.Black;
                    foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                    {
                        item.Stroke = new SolidColorBrush(Colors.White);
                    }
                    foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                    {
                        item.Stroke = new SolidColorBrush(Colors.White);
                    }


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        /// <summary>
        /// Light Mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkBxDarkMode_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                InkCanvas.Background = Brushes.White;
                foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                {
                    item.Stroke = new SolidColorBrush(Colors.Black);
                }
                foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                {
                    item.Stroke = new SolidColorBrush(Colors.Black);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        /// <summary>
        /// increasing strock thickness
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbobxStrockThickness_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                var SelectedItmes = ((ComboBoxItem)cmbobxStrockThickness.SelectedItem);
                if (SelectedItmes == null)
                    return;
                string Slection = (string)(SelectedItmes.Content);

                switch (Slection)
                {
                    case "Reset":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                        {
                            item.StrokeThickness = 4;
                        }
                        foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                        {
                            item.StrokeThickness = 4;
                        }
                        InkCanvas.UpdateLayout();
                        break;
                    case "10%":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                        {
                            item.StrokeThickness = 4 * 2;
                        }
                        foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                        {
                            item.StrokeThickness = 4 * 2;
                        }
                        break;
                    case "20%":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                        {
                            item.StrokeThickness = 4 * 3;
                        }
                        foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                        {
                            item.StrokeThickness = 4 * 3;
                        }
                        break;
                    case "30%":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                        {
                            item.StrokeThickness = 4 * 4;
                        }
                        foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                        {
                            item.StrokeThickness = 4 * 4;
                        }
                        break;
                    case "40%":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                        {
                            item.StrokeThickness = 4 * 5;
                        }
                        foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                        {
                            item.StrokeThickness = 4 * 5;
                        }
                        break;
                    case "50%":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                        {
                            item.StrokeThickness = 4 * 6;
                        }
                        foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                        {
                            item.StrokeThickness = 4 * 6;
                        }
                        break;
                    case "60%":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                        {
                            item.StrokeThickness = 4 * 7;
                        }
                        foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                        {
                            item.StrokeThickness = 4 * 7;
                        }
                        break;
                    case "70%":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                        {
                            item.StrokeThickness = 4 * 8;
                        }
                        foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                        {
                            item.StrokeThickness = 4 * 8;
                        }
                        break;
                    case "80%":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                        {
                            item.StrokeThickness = 4 * 9;
                        }
                        foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                        {
                            item.StrokeThickness = 4 * 9;
                        }
                        break;
                    case "90%":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                        {
                            item.StrokeThickness = 4 * 10;
                        }
                        foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                        {
                            item.StrokeThickness = 4 * 10;
                        }
                        break;
                    case "100%":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().ToList())
                        {
                            item.StrokeThickness = 4 * 11;
                        }
                        foreach (Path item in InkCanvas.Children.OfType<Path>().ToList())
                        {
                            item.StrokeThickness = 4 * 11;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        /// <summary>
        /// this method is made to Controle the Drawing direction to be in orthographic Mode only 
        /// </summary>
        /// <param name="StartPoint"> </param>
        /// <param name="EndPoint"></param>
        /// <returns></returns>
        private Point DetermineDrawingDirection(Point StartPoint,Point EndPoint) 
        {
            try
            {
                Vector V1 = new Vector(StartPoint.X, StartPoint.Y);// get the Vectpr of the start point 
                Vector V2 = new Vector(EndPoint.X, EndPoint.Y);//get the vector of the end point 
                Vector LineVector = V2 - V1;//get the Line Vector between these two points 
                double angel = Vector.AngleBetween(new Vector(1, 0), LineVector);// get the angel between the Line and X-Axis 
                                                                                 //1st Quartor
                if (angel > 0 && angel <= 45)
                {
                    EndPoint.Y = StartPoint.Y;
                }
                if (angel > 45 && angel <= 90)
                {
                    EndPoint.X = StartPoint.X;
                }
                //---------------
                //2nd Quarter
                if (angel > 90 && angel <= 135)
                {
                    EndPoint.X = StartPoint.X;
                }
                if (angel > 135 && angel <= 180)
                {
                    EndPoint.Y = StartPoint.Y;
                }
                //---------------

                if (angel < 0)
                {
                    //3rd Quarter
                    if (angel * -1 > 0 && angel * -1 <= 45)
                    {
                        EndPoint.Y = StartPoint.Y;
                    }
                    if (angel * -1 > 45 && angel * -1 <= 90)
                    {
                        EndPoint.X = StartPoint.X;
                    }
                    //---------------
                    //4th Quarter

                    if (angel * -1 > 90 && angel * -1 <= 135)
                    {
                        EndPoint.X = StartPoint.X;
                    }
                    if (angel * -1 > 135 && angel * -1 <= 180)
                    {
                        EndPoint.Y = StartPoint.Y;
                    }
                    //---------------
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
          
            return EndPoint;
        }

        /// <summary>
        /// this method is made to Generate Horizonatl Doors Symbol Graphically on the InkCanvas
        /// </summary>
        /// <param name="CLickPoint">this is the Click Points Coordinates which is used to place the Door in the Given Position</param>
        /// <returns></returns>
        public Path CreateHoriZontalDoors(Point CLickPoint)
        {
            HorizontalDoor = new Path();// a new HLDoor object is created each time this method is Called 
            try
            {
                //the nest two lines are for the relative position
                CLickPoint.Y = CLickPoint.Y - LogoAreaHieght;
                CLickPoint.X = CLickPoint.X - OptionsAreaWidth;
                CenterPoint = CLickPoint;
                // Create a white and a black Brush  
                SolidColorBrush WhiteBrush = new SolidColorBrush();
                WhiteBrush.Color = Colors.White;
                SolidColorBrush blackBrush = new SolidColorBrush();
                blackBrush.Color = Colors.Black;
                // Create a Path with black brush and white fill  
                HorizontalDoor.Stroke = blackBrush;
                HorizontalDoor.StrokeThickness = 3;
                HorizontalDoor.Fill = WhiteBrush;


                // Create a lines geometry  
                LineGeometry blackLineGeometry1 = new LineGeometry();
                blackLineGeometry1.StartPoint = new Point(CLickPoint.X, CLickPoint.Y);
                blackLineGeometry1.EndPoint = new Point(CLickPoint.X + 40, CLickPoint.Y);
                LineGeometry blackLineGeometry2 = new LineGeometry();
                blackLineGeometry2.StartPoint = new Point(CLickPoint.X, CLickPoint.Y);
                blackLineGeometry2.EndPoint = new Point(CLickPoint.X - 40, CLickPoint.Y);
                LineGeometry blackLineGeometry3 = new LineGeometry();
                blackLineGeometry3.StartPoint = new Point(CLickPoint.X - 30, CLickPoint.Y);
                blackLineGeometry3.EndPoint = new Point(CLickPoint.X - 30, CLickPoint.Y + 70);


                //// Create a rectangle geometry  
                RectangleGeometry blackRectGeometry1 = new RectangleGeometry();
                Rect rct1 = new Rect();
                rct1.X = blackLineGeometry1.EndPoint.X;
                rct1.Y = blackLineGeometry1.EndPoint.Y;
                rct1.Width = 7.5;
                rct1.Height = 12;
                blackRectGeometry1.Rect = rct1;
                RectangleGeometry blackRectGeometry2 = new RectangleGeometry();
                Rect rct2 = new Rect();
                rct2.X = blackLineGeometry2.EndPoint.X;
                rct2.Y = blackLineGeometry2.EndPoint.Y;
                rct2.Width = 7.5;
                rct2.Height = 12;
                blackRectGeometry2.Rect = rct2;

                //creating Door Curve
                PathGeometry CurvePath = new PathGeometry();
                PathFigureCollection PFCollection = new PathFigureCollection();
                PathFigure PF = new PathFigure();
                PF.StartPoint = new Point(CLickPoint.X - 30, CLickPoint.Y + 70);
                PathSegmentCollection PSCollection = new PathSegmentCollection();
                BezierSegment Curve = new BezierSegment();
                Curve.Point1 = new Point(CLickPoint.X + 20, CLickPoint.Y + 60);
                Curve.Point2 = new Point(CLickPoint.X + 30, CLickPoint.Y + 20);
                Curve.Point3 = new Point(CLickPoint.X + 40, CLickPoint.Y + 10);
                Curve.IsSmoothJoin = true;
                PSCollection.Add(Curve);
                PF.Segments = PSCollection;
                PFCollection.Add(PF);
                CurvePath.Figures = PFCollection;

                //// Add all the geometries to a GeometryGroup.  
                GeometryGroup DoorGeometryGroup = new GeometryGroup();
                DoorGeometryGroup.Children.Add(blackLineGeometry1);
                DoorGeometryGroup.Children.Add(blackLineGeometry2);
                DoorGeometryGroup.Children.Add(blackLineGeometry3);
                DoorGeometryGroup.Children.Add(blackRectGeometry1);
                DoorGeometryGroup.Children.Add(blackRectGeometry2);
                DoorGeometryGroup.Children.Add(CurvePath);

                // Set Path.Data  
                HorizontalDoor.Data = DoorGeometryGroup;
                HorizontalDoor.Uid = $"HorizontalDoor{DoorCounters++}";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return HorizontalDoor;
        }

        /// <summary>
        /// this method is made to Generate Verticals Doors Symbol Graphically on the InkCanvas
        /// </summary>
        /// <param name="CLickPoint">this is the Click Points Coordinates which is used to place the Door in the Given Position</param>
        /// <returns></returns>
        public Path CreateVerticalDoors(Point CLickPoint)
        {
            VerticalDoor = new Path();// a new VLDoor object is created each time this method is Called 
            try
            {
                //the nest two lines are for the relative position
                CLickPoint.Y = CLickPoint.Y - LogoAreaHieght;
                CLickPoint.X = CLickPoint.X - OptionsAreaWidth;
                CenterPoint = CLickPoint;
                // Create a white and a black Brush  
                SolidColorBrush WhiteBrush = new SolidColorBrush();
                WhiteBrush.Color = Colors.White;
                SolidColorBrush blackBrush = new SolidColorBrush();
                blackBrush.Color = Colors.Black;
                // Create a Path with black brush and white fill  
                VerticalDoor.Stroke = blackBrush;
                VerticalDoor.StrokeThickness = 3;
                VerticalDoor.Fill = WhiteBrush;


                // Create a lines geometry  
                LineGeometry blackLineGeometry1 = new LineGeometry();
                blackLineGeometry1.StartPoint = new Point(CLickPoint.X, CLickPoint.Y);
                blackLineGeometry1.EndPoint = new Point(CLickPoint.X , CLickPoint.Y + 40);
                LineGeometry blackLineGeometry2 = new LineGeometry();
                blackLineGeometry2.StartPoint = new Point(CLickPoint.X, CLickPoint.Y);
                blackLineGeometry2.EndPoint = new Point(CLickPoint.X , CLickPoint.Y - 40);
                LineGeometry blackLineGeometry3 = new LineGeometry();
                blackLineGeometry3.StartPoint = new Point(CLickPoint.X , CLickPoint.Y - 30);
                blackLineGeometry3.EndPoint = new Point(CLickPoint.X +70 , CLickPoint.Y - 30);


                //// Create a rectangle geometry  
                RectangleGeometry blackRectGeometry1 = new RectangleGeometry();
                Rect rct1 = new Rect();
                rct1.X = blackLineGeometry1.EndPoint.X;
                rct1.Y = blackLineGeometry1.EndPoint.Y;
                rct1.Width = 7.5;
                rct1.Height = 12;
                blackRectGeometry1.Rect = rct1;
                RectangleGeometry blackRectGeometry2 = new RectangleGeometry();
                Rect rct2 = new Rect();
                rct2.X = blackLineGeometry2.EndPoint.X;
                rct2.Y = blackLineGeometry2.EndPoint.Y;
                rct2.Width = 7.5;
                rct2.Height = 12;
                blackRectGeometry2.Rect = rct2;

                //creating Door Curve
                PathGeometry CurvePath = new PathGeometry();
                PathFigureCollection PFCollection = new PathFigureCollection();
                PathFigure PF = new PathFigure();
                PF.StartPoint = new Point(CLickPoint.X +70, CLickPoint.Y -30);
                PathSegmentCollection PSCollection = new PathSegmentCollection();
                BezierSegment Curve = new BezierSegment();
                Curve.Point1 = new Point(CLickPoint.X + 60, CLickPoint.Y + 20);
                Curve.Point2 = new Point(CLickPoint.X + 20, CLickPoint.Y + 30);
                Curve.Point3 = new Point(CLickPoint.X + 10, CLickPoint.Y + 40);
                PSCollection.Add(Curve);
                PF.Segments = PSCollection;
                PFCollection.Add(PF);
                CurvePath.Figures = PFCollection;

                //// Add all the geometries to a GeometryGroup.  
                GeometryGroup DoorGeometryGroup = new GeometryGroup();
                DoorGeometryGroup.Children.Add(blackLineGeometry1);
                DoorGeometryGroup.Children.Add(blackLineGeometry2);
                DoorGeometryGroup.Children.Add(blackLineGeometry3);
                DoorGeometryGroup.Children.Add(blackRectGeometry1);
                DoorGeometryGroup.Children.Add(blackRectGeometry2);
                DoorGeometryGroup.Children.Add(CurvePath);
                // Set Path.Data  
                VerticalDoor.Data = DoorGeometryGroup;

                VerticalDoor.Uid = $"VerticalDoor{DoorCounters++}";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return VerticalDoor;
        }
        /// <summary>
        /// this method is used to Create the Horizonatl Windows
        /// </summary>
        /// <param name="CLickPoint">the Point Position of the Click </param>
        /// <returns></returns>
        public Path CreateHorizontalWindows(Point CLickPoint)
        {
            Path HorizontalWindow = new Path();// a new VlWindow object is created each time this method is Called 
            try
            {

                // Create a white and a black Brush  
                SolidColorBrush WhiteBrush = new SolidColorBrush();
                WhiteBrush.Color = Colors.White;
                SolidColorBrush blackBrush = new SolidColorBrush();
                blackBrush.Color = Colors.Black;
                // Create a Path with black brush and white fill  
                HorizontalWindow.Stroke = blackBrush;
                HorizontalWindow.StrokeThickness = 3;
                HorizontalWindow.Fill = WhiteBrush;
                RectangleGeometry blackRectGeometry1 = new RectangleGeometry();
                Rect rct1 = new Rect();
                rct1.X = CLickPoint.X-50;
                rct1.Y = CLickPoint.Y-9;
                rct1.Width = 100;
                rct1.Height = 12;
                blackRectGeometry1.Rect = rct1;
                //// Add all the geometries to a GeometryGroup.  
                GeometryGroup DoorGeometryGroup = new GeometryGroup();
                DoorGeometryGroup.Children.Add(blackRectGeometry1);
                HorizontalWindow.Data = DoorGeometryGroup;
                HorizontalWindow.Uid = $"HorizontalWindow{WindowCounters++}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return HorizontalWindow;
        }
        /// <summary>
        /// this method is used to Create the Vertical Windows
        /// </summary>
        /// <param name="CLickPoint">the Point Position of the Click </param>
        /// <returns></returns>
        public Path CreateVerticalWindows(Point CLickPoint)
        {
            Path VerticalWindows = new Path();// a new VlWindow object is created each time this method is Called 
            try
            {
                // Create a white and a black Brush  
                SolidColorBrush WhiteBrush = new SolidColorBrush();
                WhiteBrush.Color = Colors.White;
                SolidColorBrush blackBrush = new SolidColorBrush();
                blackBrush.Color = Colors.Black;
                // Create a Path with black brush and white fill  
                VerticalWindows.Stroke = blackBrush;
                VerticalWindows.StrokeThickness = 3;
                VerticalWindows.Fill = WhiteBrush;
                RectangleGeometry blackRectGeometry1 = new RectangleGeometry();
                Rect rct1 = new Rect();
                rct1.X = CLickPoint.X-9;
                rct1.Y = CLickPoint.Y - 50;
                rct1.Width = 12;
                rct1.Height = 100;
                blackRectGeometry1.Rect = rct1;
                //// Add all the geometries to a GeometryGroup.  
                GeometryGroup DoorGeometryGroup = new GeometryGroup();
                DoorGeometryGroup.Children.Add(blackRectGeometry1);
                VerticalWindows.Data = DoorGeometryGroup;
                VerticalWindows.Uid = $"VerticalWindow{WindowCounters++}";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return VerticalWindows;
        }

        /// <summary>
        /// this Event is used to Delete the Canvas Element 
        /// knowing that he Delete Button Woudln`t be activated unless we Instantiate the event handeler in our Constructor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                Line L = InkCanvas.Children.OfType<Line>().FirstOrDefault(l => l.IsMouseOver);
                if (L != null)
                {
                    if (e.Key == Key.Delete)
                    {
                        InkCanvas.Children.Remove(L);
                    }
                }
                Path P = InkCanvas.Children.OfType<Path>().FirstOrDefault(l => l.IsMouseOver);
                if (P != null)
                {
                    if (e.Key == Key.Delete)
                    {
                        InkCanvas.Children.Remove(P);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// this Event is used to update the Element Counters in our Geometrical Data Panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_LayoutUpdated(object sender, EventArgs e)
        {
            try
            {
                //getting the Count For Each Wall, Door, and Window Generated
                lblNoOfWalls.Content = InkCanvas.Children.OfType<Line>().Where(l => l.Uid.Contains("Line")).ToList().Count;
                lblNoOfDoors.Content = InkCanvas.Children.OfType<Path>().Where(p => p.Uid.Contains("Door")).ToList().Count;
                lblNoOfWindows.Content = InkCanvas.Children.OfType<Path>().Where(p => p.Uid.Contains("Window")).ToList().Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmboBxWallsColor_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                var SelectedItmes = ((ComboBoxItem)cmbobxStrockThickness.SelectedItem);
                if (SelectedItmes == null)
                    return;
                string Slection = (string)(SelectedItmes.Content);

                SolidColorBrush YellowBrush = new SolidColorBrush();
                YellowBrush.Color = Colors.Yellow;
                SolidColorBrush BlackBrush = new SolidColorBrush();
                BlackBrush.Color = Colors.Black;
                SolidColorBrush BlueBrush = new SolidColorBrush();
                BlueBrush.Color = Colors.Blue;
                SolidColorBrush RedBrush = new SolidColorBrush();
                RedBrush.Color = Colors.Red;

                switch (Slection)
                {
                    case "Reset":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().Where(l => l.Uid.Contains("Line")).ToList())
                        {
                            item.Stroke = BlackBrush;
                        }
                        break;
                    case "Red":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().Where(l => l.Uid.Contains("Line")).ToList())
                        {
                            item.Stroke = RedBrush;
                        }
                        break;
                    case "Yellow":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().Where(l => l.Uid.Contains("Line")).ToList())
                        {
                            item.Stroke = YellowBrush;
                        }
                        break;
                    case "Blue":
                        foreach (Line item in InkCanvas.Children.OfType<Line>().Where(l => l.Uid.Contains("Line")).ToList())
                        {
                            item.Stroke = BlueBrush;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
           
        }

        private void cmboBxDoorsColor_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                var SelectedItmes = ((ComboBoxItem)cmbobxStrockThickness.SelectedItem);
                if (SelectedItmes == null)
                    return;
                string Slection = (string)(SelectedItmes.Content);

                SolidColorBrush YellowBrush = new SolidColorBrush();
                YellowBrush.Color = Colors.Yellow;
                SolidColorBrush BlackBrush = new SolidColorBrush();
                BlackBrush.Color = Colors.Black;
                SolidColorBrush BlueBrush = new SolidColorBrush();
                BlueBrush.Color = Colors.Blue;
                SolidColorBrush RedBrush = new SolidColorBrush();
                RedBrush.Color = Colors.Red;

                switch (Slection)
                {
                    case "Reset":
                        foreach (Path item in InkCanvas.Children.OfType<Path>().Where(l => l.Uid.Contains("Door")).ToList())
                        {
                            item.Stroke = BlackBrush;
                        }
                        break;
                    case "Red":
                        foreach (Path item in InkCanvas.Children.OfType<Path>().Where(l => l.Uid.Contains("Door")).ToList())
                        {
                            item.Stroke = RedBrush;
                        }
                        break;
                    case "Yellow":
                        foreach (Path item in InkCanvas.Children.OfType<Path>().Where(l => l.Uid.Contains("Door")).ToList())
                        {
                            item.Stroke = YellowBrush;
                        }
                        break;
                    case "Blue":
                        foreach (Path item in InkCanvas.Children.OfType<Path>().Where(l => l.Uid.Contains("Door")).ToList())
                        {
                            item.Stroke = BlueBrush;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmboBxWindowsColor_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                var SelectedItmes = ((ComboBoxItem)cmbobxStrockThickness.SelectedItem);
                if (SelectedItmes == null)
                    return;
                string Slection = (string)(SelectedItmes.Content);

                SolidColorBrush YellowBrush = new SolidColorBrush();
                YellowBrush.Color = Colors.Yellow;
                SolidColorBrush BlackBrush = new SolidColorBrush();
                BlackBrush.Color = Colors.Black;
                SolidColorBrush BlueBrush = new SolidColorBrush();
                BlueBrush.Color = Colors.Blue;
                SolidColorBrush RedBrush = new SolidColorBrush();
                RedBrush.Color = Colors.Red;

                switch (Slection)
                {
                    case "Reset":
                        foreach (Path item in InkCanvas.Children.OfType<Path>().Where(l => l.Uid.Contains("Window")).ToList())
                        {
                            item.Stroke = BlackBrush;
                        }
                        break;
                    case "Red":
                        foreach (Path item in InkCanvas.Children.OfType<Path>().Where(l => l.Uid.Contains("Window")).ToList())
                        {
                            item.Stroke = RedBrush;
                        }
                        break;
                    case "Yellow":
                        foreach (Path item in InkCanvas.Children.OfType<Path>().Where(l => l.Uid.Contains("Window")).ToList())
                        {
                            item.Stroke = YellowBrush;
                        }
                        break;
                    case "Blue":
                        foreach (Path item in InkCanvas.Children.OfType<Path>().Where(l => l.Uid.Contains("Window")).ToList())
                        {
                            item.Stroke = BlueBrush;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// this event is made to Clear all the elements in the Canvas 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InkCanvas.Children.Clear();
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ShapeConnectors
{
    public partial class Window1 : Window
    {
        // flag for enabling "New thumb" mode
        bool isAddNewAction = false;
        // flag for enabling "New link" mode
        bool isAddNewLink = false;
        // flag that indicates that the link drawing with a mouse started
        bool isLinkStarted = false;
        // variable to hold the thumb drawing started from
        MyThumb linkedThumb;
        // Line drawn by the mouse before connection established
        LineGeometry link;

        public Window1()
        {
            InitializeComponent();            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Setup connections for predefined thumbs            
            connectors.Children.Add(myThumb1.LinkTo(myThumb2));
            connectors.Children.Add(myThumb2.LinkTo(myThumb3));
            connectors.Children.Add(myThumb3.LinkTo(myThumb4));
            connectors.Children.Add(myThumb4.LinkTo(myThumb1));

            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Window1_PreviewMouseLeftButtonDown);
            this.PreviewMouseMove += new MouseEventHandler(Window1_PreviewMouseMove);
            this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(Window1_PreviewMouseLeftButtonUp);

            this.Title = "Links established: " + connectors.Children.Count.ToString();
        }

        // Event hanlder for dragging functionality support
        private void onDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            // Exit dragging operation during adding new link
            if (isAddNewLink) return;

            MyThumb thumb = e.Source as MyThumb;

            Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) + e.HorizontalChange);
            Canvas.SetTop(thumb, Canvas.GetTop(thumb) + e.VerticalChange);
             
            // Update links' layouts for active thumb
            thumb.UpdateLinks();
        }

        // Event handler for creating new thumb element by left mouse click
        // and visually connecting it to the myThumb2 element
        void Window1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If adding new action...
            if (isAddNewAction)
            {
                // Create new thumb object based on a static template "BasicShape1"
                // specifying thumb text as "action" and icon as "/Images/gear_connection.png"
                MyThumb newThumb = new MyThumb(
                    Application.Current.Resources["BasicShape1"] as ControlTemplate,
                    "action",
                    "/Images/gear_connection.png",     
                    e.GetPosition(this),
                    onDragDelta);                

                // Put newly created thumb on the canvas
                myCanvas.Children.Add(newThumb);
                
                // resume common layout for application
                isAddNewAction = false;                
                Mouse.OverrideCursor = null;
                btnNewAction.IsEnabled = btnNewLink.IsEnabled = true;
                e.Handled = true;
            }

            // Is adding new link and a thumb object is clicked...
            if (isAddNewLink && e.Source.GetType() == typeof(MyThumb))
            {                
                if (!isLinkStarted)
                {
                    if (link == null || link.EndPoint != link.StartPoint)
                    {
                        Point position = e.GetPosition(this);
                        link = new LineGeometry(position, position);
                        connectors.Children.Add(link);
                        isLinkStarted = true;
                        linkedThumb = e.Source as MyThumb;
                        e.Handled = true;
                    }
                }
            }
        }

        // Handles the mouse move event when dragging/drawing the new connection link
        void Window1_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isAddNewLink && isLinkStarted)
            {
                // Set the new link end point to current mouse position
                link.EndPoint = e.GetPosition(this);
                e.Handled = true;
            }
        }

        // Handles the mouse up event applying the new connection link or resetting it
        void Window1_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // If "Add link" mode enabled and line drawing started (line placed to canvas)
            if (isAddNewLink && isLinkStarted)
            {
                // declare the linking state
                bool linked = false;
                // We released the button on MyThumb object
                if (e.Source.GetType() == typeof(MyThumb))
                {
                    MyThumb targetThumb = e.Source as MyThumb;
                    // define the final endpoint of line
                    link.EndPoint = e.GetPosition(this);
                    // if any line was drawn (avoid just clicking on the thumb)
                    if (link.EndPoint != link.StartPoint && linkedThumb != targetThumb)
                    {
                        // establish connection
                        linkedThumb.LinkTo(targetThumb, link);
                        // set linked state to true
                        linked = true;
                    }
                }
                // if we didn't manage to approve the linking state
                // button is not released on MyThumb object or double-clicking was performed
                if (!linked)
                {
                    // remove line from the canvas
                    connectors.Children.Remove(link);
                    // clear the link variable
                    link = null;
                }
                
                // exit link drawing mode
                isLinkStarted = isAddNewLink = false;
                // configure GUI
                btnNewAction.IsEnabled = btnNewLink.IsEnabled = true;
                Mouse.OverrideCursor = null;
                e.Handled = true;
            }
            this.Title = "Links established: " + connectors.Children.Count.ToString();
        }
        
        // Event handler for enabling new thumb creation by left mouse button click
        private void btnNewAction_Click(object sender, RoutedEventArgs e)
        {            
            isAddNewAction = true;
            Mouse.OverrideCursor = Cursors.SizeAll;
            btnNewAction.IsEnabled = btnNewLink.IsEnabled = false;
        }

        private void btnNewLink_Click(object sender, RoutedEventArgs e)
        {
            isAddNewLink = true;
            Mouse.OverrideCursor = Cursors.Cross;
            btnNewAction.IsEnabled = btnNewLink.IsEnabled = false;
        }
    }
}

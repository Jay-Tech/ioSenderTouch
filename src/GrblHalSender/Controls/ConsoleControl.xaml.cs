
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for ConsoleControl.xaml
    /// </summary>
    public partial class ConsoleControl : UserControl
    {
        public ConsoleControl()
        {
            InitializeComponent();
            Loaded += ConsoleControl_Loaded;
        }

        private void ConsoleControl_Loaded(object sender, RoutedEventArgs re)
        {
            if (DataContext is GrblViewModel)
            {
                ((INotifyCollectionChanged)ListViewControl.ItemsSource).CollectionChanged += CollectionChange;
               
            }
        }

        private void CollectionChange(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            try
            {
                if (VisualTreeHelper.GetChildrenCount(ListViewControl) > 0)
                {
                    Border border = (Border)VisualTreeHelper.GetChild(ListViewControl, 0);
                    ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                    scrollViewer.ScrollToBottom();
                }

               // var item = ListViewControl.Items.Count - 1;
                // ListView.SelectedItem = ListView.Items[item];

                //ListViewControl.SelectedIndex = item;
                //ListViewControl.ScrollIntoView(ListViewControl.SelectedItem);
                
                
                //ListView.ScrollIntoView(ListView.Items[item]);
                // ListView.UpdateLayout();
            }
            catch (System.Exception)
            {

                throw;
            }
        
        }

        private void btn_Clear(object sender, RoutedEventArgs e)
        {
            (DataContext as GrblViewModel)?.ResponseLog.Clear();
        }
    }
}

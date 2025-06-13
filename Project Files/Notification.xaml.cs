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
using System.Windows.Shapes;

namespace NetScanner
{
    /// <summary>
    /// Логика взаимодействия для Notification.xaml
    /// </summary>
    public partial class Notification : Window
    {

        public Notification(string header, string text)
        {
            InitializeComponent();
            tb_header.Text = header;
            tb_text.Text = text;
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dragging(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}

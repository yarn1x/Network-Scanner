using System.Windows;
using System.Windows.Input;

namespace Анализ_Сети
{
    /// <summary>
    /// Логика взаимодействия для NotificationWindow.xaml
    /// </summary>
    public partial class InformationWindow : Window
    {
        public InformationWindow()
        {
            InitializeComponent();
        }

        private void Btn_OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_github_link_btn_Click(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/yarn1x/Network-Scanner");
        }

        private void drag(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}

using System.Windows;

namespace Player.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("🎉 WPF is working in Rider!", "Success");
        }
    }
}
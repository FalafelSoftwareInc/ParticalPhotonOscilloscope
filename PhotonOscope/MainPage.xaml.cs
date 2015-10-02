using PhotonOscope.Classes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PhotonOscope
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel viewModel
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.viewModel.Dispatcher = this.Dispatcher;
            this.Unloaded += MainPage_Unloaded;
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (this.viewModel.Disconnect.CanExecute(null))
            {
                this.viewModel.Disconnect.Execute(null);
            }
        }
    }
}

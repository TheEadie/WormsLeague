using System.Windows;
using Autofac;
using GifTool.ViewModel;

namespace GifTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var container = SetupDI();
            DataContext = container.Resolve<MainWindowViewModel>();
        }

        private IContainer SetupDI()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<GifToolModule>();
            return builder.Build();
        }
    }
}

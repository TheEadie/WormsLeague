using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace GifTool
{
    /// <summary>
    /// Interaction logic for CreateGifControl.xaml
    /// </summary>
    public partial class CreateGifControl : UserControl
    {
        public CreateGifControl()
        {
            InitializeComponent();
        }

        private void ValidateNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = int.TryParse(e.Text, out _);
        }
    }
}

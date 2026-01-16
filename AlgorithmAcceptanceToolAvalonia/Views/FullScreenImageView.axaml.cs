using AlgorithmAcceptanceToolAvalonia.ViewModels;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using SukiUI.Controls;

namespace AlgorithmAcceptanceToolAvalonia.Views
{
    public partial class FullScreenImageView : SukiWindow
    {
        private readonly FullScreenImageViewModel _viewModel;
        public FullScreenImageView(Bitmap bitmap)
        {
            _viewModel = new FullScreenImageViewModel(bitmap);
            DataContext = _viewModel;
            InitializeComponent();
        }

        private void OnImageDoubleTapped(object? sender, TappedEventArgs e)
        {
            this.Close();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z)
            {
                this.Close();
            }
        }
    }
}
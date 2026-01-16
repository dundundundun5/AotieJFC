using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AlgorithmAcceptanceToolAvalonia.ViewModels;

public partial class FullScreenImageViewModel : ViewModelBase
{
    [ObservableProperty]
    private Bitmap _fullScreenImage;

    public FullScreenImageViewModel(Bitmap img)
    {
        
        FullScreenImage = img;
    }
}
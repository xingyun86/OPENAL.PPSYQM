using Avalonia.Controls;

namespace OpenAL.Sample.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        PlayButton.Click+=(sender, e) =>
        {
            AudioPlayer.Play("test.wav");
        };
    }
}

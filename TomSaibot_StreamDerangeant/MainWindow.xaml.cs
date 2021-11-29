using System.Windows;
using System.Windows.Controls;
using TomSaibot_StreamDerangeant.Models;
using TomSaibot_StreamDerangeant.ViewModels;

namespace TomSaibot_StreamDerangeant;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
    private void Answer_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if ((sender as ListBox).SelectedItem != null)
            (DataContext as MainWindowViewModel).StreamMode.SelectCmd.Execute((sender as ListBox).SelectedItem as Answer);
    }
}

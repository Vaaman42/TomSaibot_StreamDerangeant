namespace TomSaibot_StreamDerangeant.ViewModels;

public class MainWindowViewModel
{
    public StreamModeViewModel StreamMode { get; set; } = new StreamModeViewModel();
    public SeeQuestionsViewModel SeeQuestions { get; set; } = new SeeQuestionsViewModel();
}

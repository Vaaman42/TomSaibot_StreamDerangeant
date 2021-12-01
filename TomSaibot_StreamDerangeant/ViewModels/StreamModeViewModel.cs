using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using TomSaibot_StreamDerangeant.Models;
using TomSaibot_StreamDerangeant.Services;

namespace TomSaibot_StreamDerangeant.ViewModels;

public class StreamModeViewModel : INotifyPropertyChanged
{
    #region OnPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region Constructor
    public StreamModeViewModel()
    {
        State = StreamModeState.NotStarted;
        random = new Random();
    }
    #endregion

    #region Public Properties
    public Question Question
    {
        get => question;
        set
        {
            question = value;
            OnPropertyChanged();
        }
    }
    public ObservableCollection<Answer> Answers { get; set; } = new ObservableCollection<Answer>();
    public Visibility NotStartedVisibility
    {
        get => notStartedVisibility;
        set
        {
            notStartedVisibility = value;
            OnPropertyChanged();
        }
    }
    public Visibility FreeQuestionVisibility
    {
        get => freeQuestionVisibility;
        set
        {
            freeQuestionVisibility = value;
            OnPropertyChanged();
        }
    }
    public Visibility MultipleAnswerQuestionVisibility
    {
        get => multipleAnswerQuestionVisibility;
        set
        {
            multipleAnswerQuestionVisibility = value;
            OnPropertyChanged();
        }
    }
    public Visibility FinishedVisibility
    {
        get => finishedVisibility;
        set
        {
            finishedVisibility = value;
            OnPropertyChanged();
        }
    }
    public StreamModeState State
    {
        get => state;
        set
        {
            state = value;
            switch (state)
            {
                case StreamModeState.NotStarted:
                    NotStartedVisibility = Visibility.Visible;
                    FreeQuestionVisibility = Visibility.Hidden;
                    MultipleAnswerQuestionVisibility = Visibility.Hidden;
                    FinishedVisibility = Visibility.Hidden;
                    break;
                case StreamModeState.FreeQuestion:
                    NotStartedVisibility = Visibility.Hidden;
                    FreeQuestionVisibility = Visibility.Visible;
                    MultipleAnswerQuestionVisibility = Visibility.Hidden;
                    FinishedVisibility = Visibility.Hidden;
                    break;
                case StreamModeState.MultipleAnswerQuestion:
                    NotStartedVisibility = Visibility.Hidden;
                    FreeQuestionVisibility = Visibility.Hidden;
                    MultipleAnswerQuestionVisibility = Visibility.Visible;
                    FinishedVisibility = Visibility.Hidden;
                    break;
                case StreamModeState.Finished:
                    NotStartedVisibility = Visibility.Hidden;
                    FreeQuestionVisibility = Visibility.Hidden;
                    MultipleAnswerQuestionVisibility = Visibility.Hidden;
                    FinishedVisibility = Visibility.Visible;
                    break;
            }
            OnPropertyChanged();
        }
    }
    public Command StartCmd
    {
        get
        {
            if (startCmd == null)
                startCmd = new Command { ExecuteAction = _ => NextQuestion() };
            return startCmd;
        }
    }
    public Command NextCmd
    {
        get
        {
            if (nextCmd == null)
                nextCmd = new Command { ExecuteAction = _ => NextQuestion() };
            return nextCmd;
        }
    }
    public Command RestartCmd
    {
        get
        {
            if (restartCmd == null)
                restartCmd = new Command { ExecuteAction = _ => Restart() };
            return restartCmd;
        }
    }
    public Command SelectCmd
    {
        get
        {
            if (selectCmd == null)
                selectCmd = new Command { ExecuteAction = ans => SelectAnswer(ans as Answer) };
            return selectCmd;
        }
    }
    public Command ReturnCmd
    {
        get
        {
            if (returnCmd == null)
                returnCmd = new Command { ExecuteAction = _ => Return() };
            return returnCmd;
        }
    }
    public bool ShowReturn
    {
        get => showReturn;
        set
        {
            showReturn = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Fields
    private List<Question> QuestionsShowed { get; set; } = new List<Question>();
    private List<Guid> AnswersSelected { get; set; } = new List<Guid>();
    private Question question;
    private Command startCmd;
    private Command nextCmd;
    private Command restartCmd;
    private Command selectCmd;
    private StreamModeState state;
    private Visibility notStartedVisibility;
    private Visibility freeQuestionVisibility;
    private Visibility multipleAnswerQuestionVisibility;
    private Visibility finishedVisibility;
    private Random random;
    private bool showReturn;
    private Command returnCmd;
    private Question previousQuestion;
    private Question nextQuestion;
    private Answer previousAnswer;
    #endregion

    #region Methods
    private Question GetRandomQuestion()
    {
        using var database = new LocalDatabase().Database;
        var applicableQuestions = database.Table<Question>().ToList();
        applicableQuestions = applicableQuestions.Where(q => !q.ParentAnswer.HasValue || AnswersSelected.Contains(q.ParentAnswer.Value)).ToList();
        applicableQuestions = applicableQuestions.Where(q => !QuestionsShowed.Contains(q)).ToList();
        if (!applicableQuestions.Any())
            throw new NoMoreQuestionsException();
        var question = applicableQuestions.ElementAt(random.Next(applicableQuestions.Count() - 1));
        return question;
    }
    private List<Answer> GetAnswers()
    {
        using var database = new LocalDatabase().Database;
        return database.Table<Answer>().Where(q => q.QuestionId == Question.Id).ToList();
    }
    private void NextQuestion()
    {
        using var database = new LocalDatabase().Database;
        ShowReturn = false;
        if (Question != null)
        {
            previousQuestion = Question;
            if (previousAnswer != null && previousAnswer.QuestionId != Question.Id) 
                previousAnswer = null;
            ShowReturn = true;
        }

        try
        {
            Answers.Clear();

            Question = nextQuestion != null && (!nextQuestion.ParentAnswer.HasValue || (nextQuestion.ParentAnswer.HasValue && AnswersSelected.Contains(nextQuestion.ParentAnswer.Value)))
                ? nextQuestion
                : GetRandomQuestion();
            nextQuestion = null;
            QuestionsShowed.Add(Question);
            GetAnswers().ForEach(a => Answers.Add(a));
            State = Answers.Any() ? StreamModeState.MultipleAnswerQuestion : StreamModeState.FreeQuestion;
        }
        catch (NoMoreQuestionsException)
        {
            State = StreamModeState.Finished;
        }
    }
    private void SelectAnswer(Answer answer)
    {
        AnswersSelected.Add(answer.Id);
        previousAnswer = answer;
        NextQuestion();
    }
    private void Restart()
    {
        QuestionsShowed.Clear();
        AnswersSelected.Clear();
        State = StreamModeState.NotStarted;
    }
    private void Return()
    {
        if (previousQuestion != null)
        {
            QuestionsShowed.Remove(Question);
            nextQuestion = Question;
            Question = previousQuestion;
            GetAnswers().ForEach(a => Answers.Add(a));
            State = Answers.Any() ? StreamModeState.MultipleAnswerQuestion : StreamModeState.FreeQuestion;
        }
        if (previousAnswer != null)
        {
            AnswersSelected.Remove(previousAnswer.Id);
        }
        ShowReturn = false;
    }
    #endregion
}

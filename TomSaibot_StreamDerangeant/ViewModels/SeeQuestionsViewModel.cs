using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TomSaibot_StreamDerangeant.Models;
using TomSaibot_StreamDerangeant.Services;

namespace TomSaibot_StreamDerangeant.ViewModels;

public class SeeQuestionsViewModel : INotifyPropertyChanged
{
    #region OnPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region Constructor
    public SeeQuestionsViewModel()
    {
        Questions = new ObservableCollection<Question>();
        SelectedAnswers = new ObservableCollection<Answer>();
        ParentableQuestions = new ObservableCollection<Question>();
        ParentableAnswers = new ObservableCollection<Answer>();
        FilteredQuestions = new ObservableCollection<Question>();
        Refresh();
    }
    #endregion

    #region Methods
    private void Refresh()
    {
        using var db = new LocalDatabase().Database;

        Questions.Clear();
        db.Table<Question>()
          .ToList()
          .ForEach(q => Questions.Add(q));

        FilteredQuestions.Clear();
        Questions.Where(q => q.Statement.ToLowerInvariant().Contains(Filter.ToLowerInvariant()))
                 .ToList()
                 .ForEach(q => FilteredQuestions.Add(q));
    }
    private void Save()
    {
        using var db = new LocalDatabase().Database;
        db.InsertOrReplace(SelectedQuestion);
        foreach (var answer in SelectedAnswers)
        {
            db.InsertOrReplace(answer);
        }
    }
    private void AddAnswer()
    {
        SelectedAnswers.Add(new Answer
        {
            Id = Guid.NewGuid(),
            QuestionId = SelectedQuestion.Id
        });
        Save();
    }
    private void RemoveAnswer()
    {
        if (SelectedAnswer != null)
        {
            using var database = new LocalDatabase().Database;
            database.Table<Answer>().Where(a => a.Id == SelectedAnswer.Id).Delete();
            database.Table<Question>().Where(q => q.ParentAnswer == SelectedAnswer.Id).ToList().ForEach(q => q.ParentAnswer = null);
            SelectedAnswers.Remove(SelectedAnswer);
        }
        Save();
    }
    private void AddQuestion()
    {
        using var database = new LocalDatabase().Database;
        var question = new Question
        {
            Id = Guid.NewGuid(),
            Statement = "Nouvelle Question"
        };
        Questions.Add(question);
        FilteredQuestions.Add(question);
        SelectedQuestion = question;
        Save();
    }
    private void RemoveQuestion()
    {
        using var database = new LocalDatabase().Database;
        if (SelectedQuestion != null)
        {
            var deletedrows = database.Table<Answer>().Where(a => a.QuestionId == SelectedQuestion.Id).Delete();
            SelectedAnswers.Clear();
            deletedrows = database.Table<Question>().Where(q => q.Id == SelectedQuestion.Id).Delete();
        }
        Questions.Remove(SelectedQuestion);
        Refresh();
        Save();
    }
    #endregion

    #region Properties
    public ObservableCollection<Question> Questions { get; set; }
    public ObservableCollection<Question> FilteredQuestions { get; set; }
    public ObservableCollection<Answer> SelectedAnswers { get; set; }
    public ObservableCollection<Question> ParentableQuestions { get; set; }
    public ObservableCollection<Answer> ParentableAnswers { get; set; }
    public Question SelectedQuestion
    {
        get => selectedQuestion;
        set
        {
            if (value != null && selectedQuestion != null)
                Save();
            selectedQuestion = value;
            IsDetailEnabled = selectedQuestion != null;
            using var db = new LocalDatabase().Database;

            SelectedAnswers.Clear();
            if (selectedQuestion != null)
            {
                db.Table<Answer>()
                  .Where(a => a.QuestionId == SelectedQuestion.Id)
                  .ToList()
                  .ForEach(a => SelectedAnswers.Add(a));
                if (selectedQuestion.ParentAnswer != null)
                {
                    LinkedToPrevious = true;
                    var questionId = db.Table<Answer>().FirstOrDefault(a => a.Id == selectedQuestion.ParentAnswer)?.QuestionId;
                    if (questionId == null)
                    {
                        selectedQuestion.ParentAnswer = null;
                        Save();
                        LinkedToPrevious = false;
                    }
                    else
                    {
                        ParentQuestion = db.Table<Question>().FirstOrDefault(q => q.Id == questionId);
                        ParentAnswer = ParentableAnswers.FirstOrDefault(a => a.Id == selectedQuestion.ParentAnswer);
                    }
                    OnPropertyChanged("ParentAnswer");
                }
            }
            OnPropertyChanged();
        }
    }
    public Answer SelectedAnswer
    {
        get => selectedAnswer;
        set
        {
            selectedAnswer = value;
            OnPropertyChanged();
        }
    }
    public Question ParentQuestion
    {
        get => parentQuestion;
        set
        {
            parentQuestion = value;
            ParentableAnswers.Clear();
            if (parentQuestion != null)
            {
                using var db = new LocalDatabase().Database;
                db.Table<Answer>().Where(a => a.QuestionId == parentQuestion.Id).ToList().ForEach(a => ParentableAnswers.Add(a));
            }
            OnPropertyChanged();
        }
    }
    public Answer ParentAnswer
    {
        get => parentAnswer;
        set
        {
            parentAnswer = value;
            if (parentAnswer != null)
                SelectedQuestion.ParentAnswer = parentAnswer?.Id;
            OnPropertyChanged();
            Save();
        }
    }
    public Command RefreshCmd
    {
        get
        {
            if (refreshCmd == null)
                refreshCmd = new Command { ExecuteAction = _ => Refresh() };
            return refreshCmd;
        }
    }
    public Command SaveCmd
    {
        get
        {
            if (saveCmd == null)
                saveCmd = new Command { ExecuteAction = _ => Save() };
            return saveCmd;
        }
    }
    public Command AddAnswerCmd
    {
        get
        {
            if (addAnswerCmd == null)
                addAnswerCmd = new Command { ExecuteAction = _ => AddAnswer() };
            return addAnswerCmd;
        }
    }
    public Command RemoveAnswerCmd
    {
        get
        {
            if (removeAnswerCmd == null)
                removeAnswerCmd = new Command { ExecuteAction = _ => RemoveAnswer() };
            return removeAnswerCmd;
        }
    }
    public Command AddQuestionCmd
    {
        get
        {
            if (addQuestionCmd == null)
                addQuestionCmd = new Command { ExecuteAction = _ => AddQuestion() };
            return addQuestionCmd;
        }
    }
    public Command RemoveQuestionCmd
    {
        get
        {
            if (removeQuestionCmd == null)
                removeQuestionCmd = new Command { ExecuteAction = _ => RemoveQuestion() };
            return removeQuestionCmd;
        }
    }
    public string Filter
    {
        get => filter;
        set
        {
            filter = value;
            OnPropertyChanged();
            Refresh();
        }
    }
    public bool IsDetailEnabled
    {
        get => isDetailEnabled; 
        set
        {
            isDetailEnabled = value;
            LinkedToPrevious = false;
            SelectedAnswers.Clear();
            OnPropertyChanged();
        }
    }
    public bool LinkedToPrevious
    {
        get => linkedToPrevious;
        set
        {
            linkedToPrevious = value;
            ParentableQuestions.Clear();
            if (linkedToPrevious)
            {
                using var db = new LocalDatabase().Database;
                var multipleChoiceQuestions = db.Table<Answer>().GroupBy(a => a.QuestionId).Select(a => a.Key).ToList();
                Questions.Where(q => multipleChoiceQuestions.Contains(q.Id)).ToList().ForEach(q => ParentableQuestions.Add(q));
            }
            else
            {
                ParentQuestion = null;
                ParentableQuestions.Clear();
                ParentAnswer = null;
                ParentableAnswers.Clear();
            }
            OnPropertyChanged();
        }
    }
    #endregion

    #region Fields
    private Question selectedQuestion;
    private Answer selectedAnswer;
    private Question parentQuestion;
    private Answer parentAnswer;
    private Command refreshCmd;
    private Command saveCmd;
    private Command addAnswerCmd;
    private Command removeAnswerCmd;
    private Command addQuestionCmd;
    private Command removeQuestionCmd;
    private string filter = string.Empty;
    private bool isDetailEnabled;
    private bool linkedToPrevious;
    #endregion
}


using SQLite;

namespace TomSaibot_StreamDerangeant.Models;

public class Answer
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public string Value { get; set; }

    //Linked question
    public Guid QuestionId { get; set; }
}

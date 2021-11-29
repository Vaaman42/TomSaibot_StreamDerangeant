using SQLite;

namespace TomSaibot_StreamDerangeant.Models;

public class Question
{
    [PrimaryKey]
    public Guid Id { get; set; }
    public string Statement { get; set; }

    //Parent answer => If specified, this question will be proposed only if this was answered 
    public Guid? ParentAnswer { get; set; }

    public override bool Equals(object obj)
    {
        return (obj is Question) && (obj as Question).Id == Id;
    }
}

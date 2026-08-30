namespace DotnetBase.Data.Model.Interface;

public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }
}

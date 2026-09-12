namespace DotnetBase.Contract.User.Response;

public sealed class UserInformationResponse
{
    public long UserId { get; set; }

    public long UserProfileId { get; set; }

    public required IEnumerable<string> Roles { get; set; }

    public required IEnumerable<string> Permissions { get; set; }
}

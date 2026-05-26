namespace SportConnect.Domain.Entities;

public class Sport : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public bool IsOutdoor { get; set; }
    public string? IconName { get; set; }

    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}

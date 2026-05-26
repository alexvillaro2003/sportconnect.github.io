namespace SportConnect.Domain.Entities;

public class Language : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // ISO 639-1, ej. "es", "en"

    public ICollection<UserLanguage> Users { get; set; } = new List<UserLanguage>();
}

public class UserLanguage
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int LanguageId { get; set; }
    public Language Language { get; set; } = null!;
}

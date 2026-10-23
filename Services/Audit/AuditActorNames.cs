using AuratechBackend.Data.Entities;

namespace AuratechBackend.Services.Audit;

public static class AuditActorNames
{
    public static string Format(ApplicationUser? user)
    {
        if (user is null)
        {
            return "Unknown user";
        }

        var name = $"{user.FirstName} {user.LastName}".Trim();
        return string.IsNullOrWhiteSpace(name) ? (user.Email ?? "Unknown user") : name;
    }

    public const string System = "System";
}

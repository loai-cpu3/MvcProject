namespace MvcProject.Helpers;

internal static class AvatarHelper
{
    internal static string GetInitials(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return "?";
        }

        var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return nameParts.Length switch
        {
            0 => "?",
            1 => nameParts[0][0].ToString().ToUpper(),
            _ => $"{char.ToUpper(nameParts[0][0])}{char.ToUpper(nameParts[^1][0])}"
        };
    }
}

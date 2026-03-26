namespace auth_service.Data.Models;

public static class UserRole
{
    public const string Admin = "admin";
    public const string Teacher = "teacher";
    public const string Student = "student";

    public static readonly string[] AllRoles = [Admin, Teacher, Student];

    public static bool IsValid(string role)
    {
        return AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}

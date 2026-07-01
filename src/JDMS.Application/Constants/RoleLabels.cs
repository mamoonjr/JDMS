namespace JDMS.Application.Constants;

public static class RoleLabels
{
    public static string ToArabic(string role) => role switch
    {
        Roles.Administrator => "مدير النظام (كامل الصلاحيات)",
        Roles.Manager => "مدير (تقارير + إدارة)",
        Roles.Employee => "موظف (عمليات يومية)",
        _ => role
    };

    public static bool IsValid(string? role) =>
        !string.IsNullOrEmpty(role) && Roles.All.Contains(role);
}

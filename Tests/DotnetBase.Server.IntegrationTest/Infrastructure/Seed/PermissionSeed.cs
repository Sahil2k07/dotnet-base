using System.Reflection;
using DotnetBase.Data.Context;
using DotnetBase.Data.Model;
using DotnetBase.Shared.Constant;

namespace DotnetBase.Server.IntegrationTest.Infrastructure.Seed;

public static class PermissionSeed
{
    public static async Task SeedPermissions(DotnetBaseContext context)
    {
        var permissions = typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && !field.IsInitOnly)
            .Where(field => field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!);

        context.Permissions.AddRange(
            permissions.Select(permission => new Permission
            {
                Name = permission,
                Description = permission,
            })
        );

        await context.SaveChangesAsync();
    }
}

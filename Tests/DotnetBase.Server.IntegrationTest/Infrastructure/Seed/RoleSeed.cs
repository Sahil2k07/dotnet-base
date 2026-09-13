using System.Reflection;
using DotnetBase.Data.Context;
using DotnetBase.Data.Model;
using DotnetBase.Shared.Constant;

namespace DotnetBase.Server.IntegrationTest.Infrastructure.Seed;

public static class RoleSeed
{
    public static async Task SeedRoles(DotnetBaseContext context)
    {
        var roles = typeof(Roles)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && !field.IsInitOnly)
            .Where(field => field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!);

        context.Roles.AddRange(roles.Select(role => new Role { Name = role, Description = role }));

        await context.SaveChangesAsync();
    }
}

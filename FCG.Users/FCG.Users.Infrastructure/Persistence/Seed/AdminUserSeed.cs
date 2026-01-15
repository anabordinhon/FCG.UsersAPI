using FCG.Users.Application.Common.Ports;
using FCG.Users.Domain.Common.Enuns;
using FCG.Users.Domain.Users.Entities;
using FCG.Users.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FCG.Users.Infrastructure.Persistence.Seed;
public class AdminUserSeed
{
    public static async Task EnsureAdminUserAsync(
    AppDbContext db,
    IHashHelper hashHelper,
    IConfiguration configuration,
    CancellationToken ct = default)
    {
        const string adminEmail = "admin@admin.com";
        const string adminName = "Admin Teste";
        const string adminNick = "admin";

        var adminPass = configuration["DefaultAdmin:Password"];
        if (string.IsNullOrWhiteSpace(adminPass))
            throw new InvalidOperationException("Default admin password not configured.");

        var exists = await db.Users.AnyAsync(u => u.Email.Email == adminEmail, ct);
        if (exists) return;

        var rawPassword = RawPassword.Create(adminPass);
        var (hash, salt) = hashHelper.GenerateHash(rawPassword);

        var user = User.Create(
            FullName.Create(adminName),
            EmailAddress.Create(adminEmail),
            NickName.Create(adminNick),
            hash,
            salt,
            EUserRole.Admin
        );

        user.CreatedBy = 0;

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
    }

}

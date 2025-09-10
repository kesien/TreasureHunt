using Microsoft.EntityFrameworkCore;
using TreasureHuntApp.Infrastructure.Data;

namespace TreasureHuntApp.Infrastructure.Services;
public class TeamCodeGenerator(TreasureHuntDbContext context) : ITeamCodeGenerator
{
    public async Task<string> GenerateUniqueTeamCodeAsync()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        string code;

        do
        {
            code = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        while (await context.Teams.AnyAsync(t => t.AccessCode == code));

        return code;
    }
}

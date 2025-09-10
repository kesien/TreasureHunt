namespace TreasureHuntApp.Infrastructure.Services;
public interface ITeamCodeGenerator
{
    Task<string> GenerateUniqueTeamCodeAsync();
}
namespace LiiteriStatisticsCore.Repositories
{
    /* this supplementary interface defines methods that exist for
     * SqlReadRepositories that return StatisticsResults */

    public interface IStatisticsRepository : IReadRepository<Models.StatisticsResult>
    {
        bool MaySkipPrivacyLimits { get; }
    }
}
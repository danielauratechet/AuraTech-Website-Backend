using AuratechBackend.Core.Enums;
using AuratechBackend.Data.Entities;

namespace AuratechBackend.Services.Campaigns;

public static class CampaignFunding
{
    public const decimal DaysPerMonthEstimate = 30.5m;

    public static decimal RequiredBalance(Campaign campaign, DateOnly today)
    {
        if (!campaign.DailyBudget.HasValue || campaign.DailyBudget <= 0 || !campaign.ScheduleType.HasValue)
        {
            return 0m;
        }

        decimal days;
        if (campaign.ScheduleType == CampaignScheduleType.EndDate)
        {
            if (!campaign.EndDate.HasValue || campaign.EndDate.Value < today)
            {
                return 0m;
            }

            days = campaign.EndDate.Value.DayNumber - today.DayNumber + 1;
        }
        else
        {
            days = DaysPerMonthEstimate;
        }

        return decimal.Round(campaign.DailyBudget.Value * days, 2, MidpointRounding.AwayFromZero);
    }
}

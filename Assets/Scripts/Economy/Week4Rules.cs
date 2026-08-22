namespace BankruptVtuber
{
    public static class Week4Rules
    {
        public static int AgencySurcharge(GameRunState run, Week4Balance w4)
        {
            if (run == null || w4 == null || !run.agencyFounded)
                return 0;
            return w4.agencyDailyCost;
        }

        public static bool CanFoundAgency(GameRunState run, Week4Balance w4)
        {
            if (run == null || w4 == null || run.agencyFounded)
                return false;
            if (run.day < w4.firstDay || !run.goodsUnlocked)
                return false;
            return run.cash >= w4.agencyUnlockCash && run.debt <= w4.agencyUnlockDebtMax;
        }

        public static bool FoundAgency(GameRunState run, Week4Balance w4)
        {
            if (!CanFoundAgency(run, w4))
                return false;
            run.cash -= w4.agencyFoundCost;
            run.agencyFounded = true;
            run.lastAgencyFoundCost = w4.agencyFoundCost;
            EconomyRules.ConvertNegativeCashToDebt(run);
            return true;
        }

        public static bool CanScoutJunior(GameRunState run, Week4Balance w4)
        {
            if (run == null || w4 == null || !run.agencyFounded || run.juniorScouted)
                return false;
            return run.cash >= w4.juniorScoutCost;
        }

        public static bool ScoutJunior(GameRunState run, Week4Balance w4)
        {
            if (!CanScoutJunior(run, w4))
                return false;
            run.cash -= w4.juniorScoutCost;
            run.juniorScouted = true;
            run.lastJuniorScoutCost = w4.juniorScoutCost;
            EconomyRules.ConvertNegativeCashToDebt(run);
            return true;
        }

        public static bool CanOfferSponsor(GameRunState run, Week4Balance w4)
        {
            if (run == null || w4 == null || !run.agencyFounded || run.sponsorEverSigned)
                return false;
            return run.peakViewersEver >= w4.sponsorPeakViewers;
        }

        public static bool SignSponsor(GameRunState run, Week4Balance w4)
        {
            if (!CanOfferSponsor(run, w4))
                return false;
            run.sponsorEverSigned = true;
            run.sponsorActive = true;
            run.sponsorDaysLeft = w4.sponsorDays;
            ApplySponsorDaily(run, w4);
            return true;
        }

        public static int ApplyJuniorDaily(GameRunState run, Week4Balance w4)
        {
            if (run == null || w4 == null || run.juniorAppliedThisDay)
                return 0;
            run.juniorAppliedThisDay = true;
            run.lastJuniorPay = 0;
            run.lastJuniorTrainFail = false;
            if (!run.juniorScouted)
                return 0;

            bool success = run.streamDoneThisDay && !run.lastStreamForceEnded;
            if (success)
            {
                run.lastJuniorPay = w4.juniorDailySuccess;
                run.cash += w4.juniorDailySuccess;
            }

            bool trainFail = run.lastStreamForceEnded || run.lastMisses >= w4.juniorTrainFailMisses;
            if (trainFail)
            {
                run.lastJuniorTrainFail = true;
                run.mental -= w4.juniorTrainFailMental;
                if (run.mental < 0)
                    run.mental = 0;
            }

            EconomyRules.ConvertNegativeCashToDebt(run);
            return run.lastJuniorPay;
        }

        public static int ApplySponsorDaily(GameRunState run, Week4Balance w4)
        {
            if (run == null || w4 == null || !run.sponsorActive || run.sponsorDailyAppliedThisDay)
                return 0;
            run.sponsorDailyAppliedThisDay = true;
            run.lastSponsorDaily = 0;

            run.lastSponsorDaily = w4.sponsorDaily;
            run.cash += w4.sponsorDaily;
            run.sponsorDaysLeft -= 1;
            if (run.sponsorDaysLeft <= 0)
            {
                run.sponsorDaysLeft = 0;
                run.sponsorActive = false;
            }

            EconomyRules.ConvertNegativeCashToDebt(run);
            return run.lastSponsorDaily;
        }

        public static void ApplySponsorLine(GameRunState run, Week4Balance w4, bool success)
        {
            if (run == null || w4 == null || run.sponsorLineAppliedThisDay)
                return;
            run.sponsorLineAppliedThisDay = true;
            run.lastSponsorLineSuccess = success;
            run.lastSponsorLineBonus = 0;
            run.lastSponsorBroke = false;
            if (!run.sponsorEverSigned)
                return;

            if (success)
            {
                run.lastSponsorLineBonus = w4.sponsorLineBonus;
                run.cash += w4.sponsorLineBonus;
                EconomyRules.ConvertNegativeCashToDebt(run);
                return;
            }

            run.lastSponsorBroke = true;
            run.sponsorActive = false;
            run.sponsorDaysLeft = 0;
            run.cash -= w4.sponsorFailCash;
            run.mental -= w4.sponsorFailMental;
            if (run.mental < 0)
                run.mental = 0;
            EconomyRules.ConvertNegativeCashToDebt(run);
        }
    }
}

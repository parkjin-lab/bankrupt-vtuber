using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    public class SettlementDirector : MonoBehaviour
    {
        Text _body;
        Text _result;
        Button _next;
        Button _repay;
        Button _restart;
        Button _clipYes;
        Button _clipNo;
        Text _clipNote;
        Button _produce;
        Button _foundAgency;
        Button _scout;
        Button _signSponsor;
        Button _bookConcert;
        Button _concertLive;
        Button _letter;
        Button _auto;
        Button _soothe;
        Button _style;
        Button _retire;
        RectTransform _rankBox;
        Text _rankPanel;
        GameObject _endingRoot;
        Text _endingTitle;
        Text _endingBody;

        void Awake()
        {
            UiKit.EnsureCamera(Palette.Studio);
            UiKit.EnsureEventSystem();
            Build();
        }

        void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                Week2Rules.ApplyMembershipPassive(gm.Run, gm.Week2);
                Week3Rules.TryUnlockGoods(gm.Run, gm.Week3);
                Week3Rules.ApplyGoodsSales(gm.Run, gm.Week3);
                Week4Rules.ApplyJuniorDaily(gm.Run, gm.Week4);
                Week4Rules.ApplySponsorDaily(gm.Run, gm.Week4);
                Week5Rules.ApplyRanking(gm.Run, gm.Week5);
                Week5Rules.ApplyConcertResult(gm.Run, gm.Balance, gm.Week5);
                Week5Rules.NoteZeroMentalDay(gm.Run);
            }
            Render();
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null)
                return;
            if (CanAdvance(gm.Run) && StreamBindings.Confirm)
                gm.NextMorning();
        }

        static bool CanAdvance(GameRunState run) =>
            !FandomRules.MustResolveConflict(run) &&
            (run.lastOutcome == WeekOutcome.Continue ||
            WeekSchedule.CanEnterWeek2(run) ||
            WeekSchedule.CanEnterWeek3(run) ||
            WeekSchedule.CanEnterWeek4(run) ||
            WeekSchedule.CanEnterWeek5(run));

        void Build()
        {
            var canvas = UiKit.CreateCanvas("SettlementCanvas", transform);
            var root = canvas.transform;
            UiKit.Image(root, "Wash", Palette.Studio);
            UiKit.Stretch(root.Find("Wash") as RectTransform);

            var title = UiKit.Label(root, "Title", "정산", 54, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(48, -28), new Vector2(400, 70));

            var panel = UiKit.Panel(root, "Sheet", new Color(1, 1, 1, 0.07f));
            UiKit.Layout(panel, new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.56f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920, 500));
            _body = UiKit.Label(panel, "Body", "", 24, Palette.Pastel, TextAnchor.UpperLeft);
            UiKit.Stretch(_body.rectTransform, 36, 36, 28, 28);
            _body.horizontalOverflow = HorizontalWrapMode.Wrap;
            _body.verticalOverflow = VerticalWrapMode.Overflow;
            _body.lineSpacing = 1.12f;

            _result = UiKit.Label(root, "Result", "", 30, Palette.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_result.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 168), new Vector2(1200, 50));

            _clipYes = UiKit.Button(root, "ClipYes", "클립 업로드", OnClipYes, Palette.Gold, Palette.Ink);
            UiKit.Layout(_clipYes.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-190, 118), new Vector2(320, 56));
            _clipNo = UiKit.Button(root, "ClipNo", "올리지 않기", OnClipNo, Palette.StudioHi, Palette.Pastel);
            UiKit.Layout(_clipNo.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(190, 118), new Vector2(320, 56));
            _clipNote = UiKit.Label(root, "ClipNote", "", 18, Palette.Gold, TextAnchor.MiddleCenter);
            UiKit.Layout(_clipNote.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 118), new Vector2(720, 28));

            _produce = UiKit.Button(root, "Produce", "아크릴 1개 생산  ₩2,500", OnProduce, Palette.Gold, Palette.Ink);
            UiKit.Layout(_produce.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 118), new Vector2(360, 56));
            _produce.gameObject.SetActive(false);

            _foundAgency = UiKit.Button(root, "FoundAgency", "에이전시 설립  ₩40,000", OnFoundAgency, Palette.Gold, Palette.Ink);
            UiKit.Layout(_foundAgency.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-360, 118), new Vector2(300, 56));
            _foundAgency.gameObject.SetActive(false);
            _scout = UiKit.Button(root, "Scout", "주니어 스카우트  ₩25,000", OnScout, Palette.PinkDeep, Color.white);
            UiKit.Layout(_scout.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 118), new Vector2(300, 56));
            _scout.gameObject.SetActive(false);
            _signSponsor = UiKit.Button(root, "Sponsor", "스폰서 계약", OnSignSponsor, Palette.Gold, Palette.Ink);
            UiKit.Layout(_signSponsor.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(360, 118), new Vector2(300, 56));
            _signSponsor.gameObject.SetActive(false);

            _bookConcert = UiKit.Button(root, "BookConcert", "콘서트 개최  ₩80,000", OnBookConcert, Palette.Gold, Palette.Ink);
            UiKit.Layout(_bookConcert.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-210, 184), new Vector2(360, 56));
            _bookConcert.gameObject.SetActive(false);
            _concertLive = UiKit.Button(root, "ConcertLive", "콘서트 방송", OnConcertLive, Palette.PinkDeep, Color.white);
            UiKit.Layout(_concertLive.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(210, 184), new Vector2(360, 56));
            _concertLive.gameObject.SetActive(false);

            _letter = UiKit.Button(root, "FanLetter", "팬레터 답장", OnLetter, Palette.PinkDeep, Color.white);
            UiKit.Layout(_letter.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(48, 52), new Vector2(240, 56));
            _auto = UiKit.Button(root, "AutoReply", "기본 자동응답", OnToggleAuto, Palette.Gold, Palette.Ink);
            UiKit.Layout(_auto.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-48, 52), new Vector2(240, 56));
            _soothe = UiKit.Button(root, "Soothe", "특별방송으로 달래기", OnSootheConflict, Palette.PinkDeep, Color.white);
            UiKit.Layout(_soothe.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-190, 184), new Vector2(320, 56));
            _style = UiKit.Button(root, "Style", "내 스타일대로", OnStyleConflict, Palette.Troll, Color.white);
            UiKit.Layout(_style.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(190, 184), new Vector2(320, 56));

            _rankBox = UiKit.Panel(root, "RankPanel", new Color(0.10f, 0.05f, 0.12f, 0.92f));
            UiKit.Layout(_rankBox, new Vector2(1, 0.55f), new Vector2(1, 0.55f), new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(280, 220));
            _rankPanel = UiKit.Label(_rankBox, "RankBody", "", 18, Palette.Pastel, TextAnchor.UpperLeft);
            UiKit.Stretch(_rankPanel.rectTransform, 16, 16, 14, 14);
            _rankPanel.lineSpacing = 1.15f;
            _rankBox.gameObject.SetActive(false);

            _repay = UiKit.Button(root, "Repay", "남은 현금으로 빚 갚기", OnRepay, Palette.Gold, Palette.Ink);
            UiKit.Layout(_repay.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-210, 52), new Vector2(360, 60));

            _next = UiKit.Button(root, "Next", "다음날  (Space)", () => GameManager.Instance.NextMorning(), Palette.PinkDeep, Color.white);
            UiKit.Layout(_next.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(210, 52), new Vector2(360, 60));

            _restart = UiKit.Button(root, "Restart", "처음부터", () => GameManager.Instance.RestartRun(), Palette.Troll, Color.white);
            UiKit.Layout(_restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 52), new Vector2(360, 60));

            _endingRoot = new GameObject("EndingRoot", typeof(RectTransform));
            _endingRoot.transform.SetParent(root, false);
            UiKit.Stretch(_endingRoot.GetComponent<RectTransform>());
            var endingWash = UiKit.Image(_endingRoot.transform, "EndingWash", new Color(0.06f, 0.03f, 0.08f, 0.94f));
            UiKit.Stretch(endingWash.rectTransform);
            var endingCard = UiKit.Panel(_endingRoot.transform, "EndingCard", new Color(0.14f, 0.07f, 0.12f, 0.98f));
            UiKit.Layout(endingCard, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 360));
            _endingTitle = UiKit.Label(endingCard, "ETitle", "", 48, Palette.Gold, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(_endingTitle.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(-40, 64));
            _endingBody = UiKit.Label(endingCard, "EBody", "", 24, Palette.Pastel, TextAnchor.MiddleCenter);
            UiKit.Layout(_endingBody.rectTransform, new Vector2(0, 0.28f), new Vector2(1, 0.72f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-48, 0));
            _endingBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _endingBody.lineSpacing = 1.2f;
            _retire = UiKit.Button(endingCard, "Retire", "후배에게 메인 양도", OnRetire, Palette.Gold, Palette.Ink);
            UiKit.Layout(_retire.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-190, 28), new Vector2(320, 56));
            var endingRestart = UiKit.Button(endingCard, "EndingRestart", "처음부터", () => GameManager.Instance.RestartRun(), Palette.PinkDeep, Color.white);
            UiKit.Layout(endingRestart.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(190, 28), new Vector2(320, 56));
            _endingRoot.SetActive(false);
        }

        void Render()
        {
            var gm = GameManager.Instance;
            var run = gm.Run;
            var b = gm.Balance;
            var w2 = gm.Week2;
            var w3 = gm.Week3;
            var w4 = gm.Week4;
            var w5 = gm.Week5;

            string extras = "";
            if (run.extraRolls.Count > 0)
            {
                for (int i = 0; i < run.extraRolls.Count; i++)
                    extras += $"위협 {run.extraRolls[i].DisplayName,-10} -{EconomyRules.FormatWon(run.extraRolls[i].Amount)}\n";
            }
            else if (run.extraThreatAmount > 0)
                extras = $"위협 {run.extraThreatName,-10} -{EconomyRules.FormatWon(run.extraThreatAmount)}\n";

            string memberDelta = "";
            if (run.lastMembershipFromHype > 0)
                memberDelta += $"   (+{run.lastMembershipFromHype} 하이프)";
            if (run.lastMembershipFromMiss > 0)
                memberDelta += $"   (-{run.lastMembershipFromMiss} 미스)";

            string force = run.lastStreamForceEnded ? "멘탈 붕괴로 강제 종료 · 수입 50%\n" : "";
            string weekTag = WeekSchedule.WeekNumber(run) + "주차";
            string rivalLine = "";
            if (run.lastRivalMatch)
                rivalLine = run.lastRivalWon
                    ? $"라이벌전 승리       {EconomyRules.FormatWon(run.lastRivalCash)} · 시작 시청자 +6\n"
                    : "라이벌전 패배       시작 시청자 −5 · 멘탈 −12\n";
            string goodsLine = "";
            if (run.goodsUnlocked)
            {
                goodsLine =
                    (run.lastGoodsSold > 0
                        ? $"아크릴 판매         {run.lastGoodsSold}개  {EconomyRules.FormatWon(run.lastGoodsRevenue)}" +
                          (run.lastGoodsPromoSuccess ? "  · 홍보 1.5x\n" : "\n")
                        : "") +
                    $"아크릴 재고         {run.goodsStock}개\n";
            }
            _body.text =
                $"{weekTag}  {run.day}일차 정산\n\n" +
                force +
                $"방송 수익(초당)     {EconomyRules.FormatWon(run.lastTickIncome)}\n" +
                $"슈퍼챗              {EconomyRules.FormatWon(run.lastSuperchatIncome)}\n" +
                $"실지급              {EconomyRules.FormatWon(run.lastStreamIncome)}\n" +
                $"오늘 고정비         -{EconomyRules.FormatWon(run.lastBills)}\n" +
                extras +
                (run.lastMembershipPassive > 0
                    ? $"멤버십 수익         {EconomyRules.FormatWon(run.lastMembershipPassive)}\n"
                    : "") +
                (run.lastClipCash > 0
                    ? $"클립 성공           {EconomyRules.FormatWon(run.lastClipCash)}\n"
                    : "") +
                rivalLine +
                goodsLine +
                (run.lastAgencyFoundCost > 0
                    ? $"에이전시 설립      -{EconomyRules.FormatWon(run.lastAgencyFoundCost)}\n"
                    : "") +
                (run.lastJuniorScoutCost > 0
                    ? $"주니어 스카우트    -{EconomyRules.FormatWon(run.lastJuniorScoutCost)}\n"
                    : "") +
                (run.lastJuniorPay > 0
                    ? $"주니어 수입         {EconomyRules.FormatWon(run.lastJuniorPay)}\n"
                    : "") +
                (run.lastJuniorTrainFail ? "주니어 훈련 실패   멘탈 −8\n" : "") +
                (run.lastSponsorDaily > 0
                    ? $"스폰서 일급         {EconomyRules.FormatWon(run.lastSponsorDaily)}\n"
                    : "") +
                (run.lastSponsorLineBonus > 0
                    ? $"스폰서 멘트         {EconomyRules.FormatWon(run.lastSponsorLineBonus)}\n"
                    : "") +
                (run.lastSponsorBroke ? "스폰서 계약 종료   −₩15,000 · 멘탈 −12\n" : "") +
                (run.lastRankingFirstPay > 0
                    ? $"랭킹 1위            {EconomyRules.FormatWon(run.lastRankingFirstPay)}\n"
                    : "") +
                (run.lastConcertCost > 0 && run.concertBooked
                    ? $"콘서트 개최        -{EconomyRules.FormatWon(run.lastConcertCost)}\n"
                    : "") +
                (run.lastConcertFailed ? "콘서트 실패         개최비 소멸 · 멘탈 −25 · 시작 시청자 −10\n" : "") +
                (run.lastConcertPayout > 0
                    ? $"콘서트 정산         {EconomyRules.FormatWon(run.lastConcertPayout)}" +
                      (run.lastConcertPerformanceSuccess ? "  · 퍼포먼스 1.3x\n" : "\n")
                    : "") +
                (run.lastRepaid > 0 ? $"부채 상환           -{EconomyRules.FormatWon(run.lastRepaid)}\n" : "") +
                (run.lastAutoCost > 0
                    ? $"자동응답           -{EconomyRules.FormatWon(run.lastAutoCost)}\n"
                    : "") +
                (run.lastConflictSurcharge > 0
                    ? $"갈등 할증           -{EconomyRules.FormatWon(run.lastConflictSurcharge)}\n"
                    : "") +
                (run.lastFanSupport > 0
                    ? $"팬 지원금           {EconomyRules.FormatWon(run.lastFanSupport)}\n"
                    : "") +
                (run.lastFanLetter ? "팬레터 답장         충성 +4 · 멘탈 +8\n" : "") +
                (run.lastMinjunLeft ? "민준이 떠났습니다   충성 −12\n" : "") +
                (run.lastHaeunLeft ? "하은이 떠났습니다   충성 −15\n" : "") +
                (run.lostSuperchatBonusDay ? "슈퍼챗 보너스 1일 소멸\n" : "") +
                $"\n판정  P {run.lastPerfects}  G {run.lastGreats}  Good {run.lastGoods}  Miss {run.lastMisses}" +
                (run.lastHadHype ? "   · 하이프 달성" : "") +
                (run.lastStreamEventHappened
                    ? $"\n이벤트 {run.lastStreamEventName}   {(run.lastStreamEventSuccess ? "성공" : "실패")}"
                    : "") +
                (run.membershipUnlocked
                    ? $"\n멤버십 {run.membershipCount}{memberDelta}"
                    : "") +
                (run.agencyFounded ? "\n에이전시 설립됨" : "") +
                (run.juniorScouted ? "   ·   주니어 1" : "") +
                (run.sponsorActive ? $"   ·   스폰서 남은 {run.sponsorDaysLeft}일" : "") +
                $"\n{FandomRules.HudLine(run)}" +
                (string.IsNullOrEmpty(FandomRules.SuperfanLine(run, gm.Fandom))
                    ? ""
                    : $"\n{FandomRules.SuperfanLine(run, gm.Fandom)}") +
                (FandomRules.MustResolveConflict(run) ? "\n콘텐츠 편중 갈등 — 오늘 안에 고르세요." : "") +
                $"\n\n현금 {EconomyRules.FormatWon(run.cash)}     부채 {EconomyRules.FormatWon(run.debt)}     멘탈 {run.mental}";

            run.lastOutcome = EconomyRules.Evaluate(run, b, w2, w3, w4, w5);
            bool offerClip = Week2Rules.CanOfferClip(run, w2);
            bool offerFound = Week4Rules.CanFoundAgency(run, w4);
            bool offerScout = Week4Rules.CanScoutJunior(run, w4);
            bool offerSponsor = Week4Rules.CanOfferSponsor(run, w4);
            bool offerConcert = Week5Rules.CanBookConcert(run, w5);
            bool concertReady = Week5Rules.ConcertStreamReady(run);
            bool week4Offer = offerFound || offerScout || offerSponsor;
            bool week5Offer = offerConcert || concertReady;
            _clipYes.gameObject.SetActive(offerClip);
            _clipNo.gameObject.SetActive(offerClip);
            _foundAgency.gameObject.SetActive(offerFound);
            _scout.gameObject.SetActive(offerScout);
            _signSponsor.gameObject.SetActive(offerSponsor);
            _bookConcert.gameObject.SetActive(offerConcert);
            _concertLive.gameObject.SetActive(concertReady);
            _produce.gameObject.SetActive(run.goodsUnlocked && !offerClip && !week4Offer && !week5Offer && run.cash >= (w3 != null ? w3.goodsProduceCost : 2500));
            bool rankOn = Week5Rules.RankingUnlocked(run, w5);
            _rankBox.gameObject.SetActive(rankOn);
            if (rankOn)
            {
                string daily = run.lastDailyRank > 0
                    ? $"오늘 {run.lastDailyRank}위  {run.lastRankingScore}\n누적 {run.finalRank}위\n\n"
                    : "누적\n\n";
                _rankPanel.text = "챌린지 랭킹\n" + daily + Week5Rules.RankingBoard(run);
            }
            if (run.lastClipAttempted)
                _clipNote.text = run.lastClipSuccess
                    ? "클립 성공 — ₩30,000 · 시작 시청자 +10"
                    : "클립 없음";
            else
                _clipNote.text = "";
            _clipNote.gameObject.SetActive(run.lastClipAttempted && !offerClip);

            bool ending = ShouldShowEnding(run, w5);
            bool conflict = FandomRules.MustResolveConflict(run);
            _letter.gameObject.SetActive(!ending);
            _letter.interactable = FandomRules.CanSendLetter(run);
            _letter.GetComponentInChildren<Text>().text = run.fanLetterSentThisDay ? "팬레터 완료" : "팬레터 답장";
            _auto.gameObject.SetActive(!ending && FandomRules.CanToggleAuto(run));
            if (_auto.gameObject.activeSelf)
                _auto.GetComponentInChildren<Text>().text = run.autoReplyOn ? "기본 자동응답 끄기" : "기본 자동응답 켜기";
            _soothe.gameObject.SetActive(!ending && conflict);
            _style.gameObject.SetActive(!ending && conflict);
            if (conflict)
            {
                _clipYes.gameObject.SetActive(false);
                _clipNo.gameObject.SetActive(false);
                _produce.gameObject.SetActive(false);
                _foundAgency.gameObject.SetActive(false);
                _scout.gameObject.SetActive(false);
                _signSponsor.gameObject.SetActive(false);
                _bookConcert.gameObject.SetActive(false);
                _concertLive.gameObject.SetActive(false);
            }

            switch (run.lastOutcome)
            {
                case WeekOutcome.Bankrupt:
                    _result.text = WeekSchedule.InWeek5(run)
                        ? "파산. 부채가 ₩350,000을 넘었습니다."
                        : WeekSchedule.InWeek4(run)
                        ? "파산. 부채가 ₩300,000을 넘었습니다."
                        : WeekSchedule.InWeek3(run)
                        ? "파산. 부채가 ₩260,000을 넘었습니다."
                        : WeekSchedule.InWeek2(run)
                        ? "파산. 부채가 ₩220,000을 넘었습니다."
                        : "파산. 부채가 ₩180,000을 넘었습니다.";
                    _result.color = Palette.MoneyRed;
                    _next.gameObject.SetActive(false);
                    _repay.gameObject.SetActive(false);
                    _restart.gameObject.SetActive(true);
                    _letter.gameObject.SetActive(false);
                    _auto.gameObject.SetActive(false);
                    _soothe.gameObject.SetActive(false);
                    _style.gameObject.SetActive(false);
                    break;
                case WeekOutcome.Win:
                    _result.text = "1주차 생존 성공. 2주차를 이어갈 수 있습니다.";
                    _result.color = Palette.CashGreen;
                    _next.GetComponentInChildren<Text>().text = "2주차 시작  (Space)";
                    _next.gameObject.SetActive(true);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    PlaceTripleButtons();
                    break;
                case WeekOutcome.Week2Win:
                    _result.text = "2주차 클리어. 빚 ≤ 2만 또는 현금 ≥ 11만, 그리고 멤버십 해금.";
                    _result.color = Palette.CashGreen;
                    _next.GetComponentInChildren<Text>().text = "3주차 시작  (Space)";
                    _next.gameObject.SetActive(true);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    PlaceTripleButtons();
                    break;
                case WeekOutcome.Week3Win:
                    _result.text = "3주차 클리어. 빚 ≤ 1.5만 또는 현금 ≥ 14만, 그리고 아크릴 스탠드 해금.";
                    _result.color = Palette.CashGreen;
                    _next.GetComponentInChildren<Text>().text = "4주차 시작  (Space)";
                    _next.gameObject.SetActive(true);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    PlaceTripleButtons();
                    break;
                case WeekOutcome.Week4Win:
                    _result.text = "4주차 클리어. 에이전시 설립, 그리고 빚 ≤ 1만 또는 현금 ≥ 18만.";
                    _result.color = Palette.CashGreen;
                    _next.GetComponentInChildren<Text>().text = "5주차 시작  (Space)";
                    _next.gameObject.SetActive(true);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    PlaceTripleButtons();
                    break;
                case WeekOutcome.Ending:
                    _result.text = "5주차 정산. 엔딩이 열립니다.";
                    _result.color = Palette.Gold;
                    _next.gameObject.SetActive(false);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0 && !ShouldShowEnding(run, w5));
                    _restart.gameObject.SetActive(!ShouldShowEnding(run, w5));
                    break;
                case WeekOutcome.WeekFailed:
                    _result.text = WeekSchedule.InWeek4(run)
                        ? "4주차 목표 미달 (에이전시 설립, 그리고 부채 1만 이하 또는 현금 18만)."
                        : WeekSchedule.InWeek3(run)
                        ? "3주차 목표 미달 (부채 1.5만 이하 또는 현금 14만, 그리고 아크릴 스탠드 해금)."
                        : WeekSchedule.InWeek2(run)
                        ? "2주차 목표 미달 (부채 2만 이하 또는 현금 11만, 그리고 멤버십 해금)."
                        : "5일은 버텼지만 목표 미달 (부채 3만 이하 또는 현금 7만). 남은 현금으로 빚을 갚을 수 있습니다.";
                    _result.color = Palette.Gold;
                    _next.gameObject.SetActive(false);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(true);
                    break;
                default:
                    _result.text = $"{run.day}일차 종료. 남은 날 {WeekSchedule.DaysLeftInWeek(run)}일.";
                    _result.color = Palette.Pastel;
                    _next.GetComponentInChildren<Text>().text = "다음날  (Space)";
                    _next.gameObject.SetActive(true);
                    _repay.gameObject.SetActive(run.cash > 0 && run.debt > 0);
                    _restart.gameObject.SetActive(false);
                    break;
            }

            if (conflict)
                _next.gameObject.SetActive(false);

            ApplyEndingOverlay(run, w5);
        }

        void PlaceTripleButtons()
        {
            var run = GameManager.Instance.Run;
            if (run.cash > 0 && run.debt > 0)
            {
                UiKit.Layout(_repay.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-360, 52), new Vector2(300, 60));
                UiKit.Layout(_next.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 52), new Vector2(300, 60));
                UiKit.Layout(_restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(360, 52), new Vector2(300, 60));
            }
            else
            {
                UiKit.Layout(_next.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-210, 52), new Vector2(360, 60));
                UiKit.Layout(_restart.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(210, 52), new Vector2(360, 60));
            }
        }

        void OnLetter()
        {
            var gm = GameManager.Instance;
            FandomRules.SendLetter(gm.Run, gm.Balance, gm.Fandom);
            Render();
        }

        void OnToggleAuto()
        {
            var gm = GameManager.Instance;
            FandomRules.SetAutoReply(gm.Run, !gm.Run.autoReplyOn);
            Render();
        }

        void OnSootheConflict()
        {
            var gm = GameManager.Instance;
            FandomRules.SootheConflict(gm.Run, gm.Fandom);
            Render();
        }

        void OnStyleConflict()
        {
            var gm = GameManager.Instance;
            FandomRules.StyleConflict(gm.Run, gm.Fandom);
            Render();
        }

        void OnRepay()
        {
            var run = GameManager.Instance.Run;
            EconomyRules.RepayDebt(run, run.cash);
            Render();
        }

        void OnClipYes()
        {
            var gm = GameManager.Instance;
            Week2Rules.AttemptClip(gm.Run, gm.Week2);
            Render();
        }

        void OnClipNo()
        {
            Week2Rules.DeclineClip(GameManager.Instance.Run);
            Render();
        }

        void OnProduce()
        {
            var gm = GameManager.Instance;
            Week3Rules.ProduceGoods(gm.Run, gm.Week3);
            Render();
        }

        void OnFoundAgency()
        {
            var gm = GameManager.Instance;
            Week4Rules.FoundAgency(gm.Run, gm.Week4);
            Render();
        }

        void OnScout()
        {
            var gm = GameManager.Instance;
            Week4Rules.ScoutJunior(gm.Run, gm.Week4);
            Render();
        }

        void OnSignSponsor()
        {
            var gm = GameManager.Instance;
            Week4Rules.SignSponsor(gm.Run, gm.Week4);
            Render();
        }

        void OnBookConcert()
        {
            var gm = GameManager.Instance;
            Week5Rules.BookConcert(gm.Run, gm.Week5);
            Render();
        }

        void OnConcertLive()
        {
            GameManager.Instance.GoLive();
        }

        void OnRetire()
        {
            var gm = GameManager.Instance;
            gm.Run.retirePicked = true;
            gm.Run.lastEnding = Week5Rules.ResolveEnding(gm.Run, gm.Week5, true);
            gm.Run.lastOutcome = WeekOutcome.Ending;
            Render();
        }

        static bool ShouldShowEnding(GameRunState run, Week5Balance w5)
        {
            if (run == null || !WeekSchedule.InWeek5(run))
                return false;
            bool fatal = run.lastOutcome == WeekOutcome.Bankrupt
                || run.lastEnding == EndingKind.Bankrupt
                || run.lastEnding == EndingKind.Burnout;
            if (fatal)
                return true;
            if (run.lastOutcome != WeekOutcome.Ending)
                return false;
            return !Week5Rules.CanBookConcert(run, w5) && !Week5Rules.ConcertStreamReady(run);
        }

        void ApplyEndingOverlay(GameRunState run, Week5Balance w5)
        {
            bool show = ShouldShowEnding(run, w5);
            _endingRoot.SetActive(show);
            if (!show)
            {
                _retire.gameObject.SetActive(false);
                return;
            }

            var kind = run.lastEnding == EndingKind.None
                ? Week5Rules.ResolveEnding(run, w5, run.retirePicked)
                : run.lastEnding;
            _endingTitle.text = Week5Rules.EndingTitle(kind);
            _endingBody.text = Week5Rules.EndingBody(kind);
            bool offerRetire = Week5Rules.CanOfferRetire(run, w5) && !run.retirePicked;
            _retire.gameObject.SetActive(offerRetire);
            _next.gameObject.SetActive(false);
            _repay.gameObject.SetActive(false);
            _restart.gameObject.SetActive(false);
            _bookConcert.gameObject.SetActive(false);
            _concertLive.gameObject.SetActive(false);
            _foundAgency.gameObject.SetActive(false);
            _scout.gameObject.SetActive(false);
            _signSponsor.gameObject.SetActive(false);
            _produce.gameObject.SetActive(false);
            _clipYes.gameObject.SetActive(false);
            _clipNo.gameObject.SetActive(false);
            _letter.gameObject.SetActive(false);
            _auto.gameObject.SetActive(false);
            _soothe.gameObject.SetActive(false);
            _style.gameObject.SetActive(false);
        }
    }
}

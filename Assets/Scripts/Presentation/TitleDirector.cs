using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BankruptVtuber
{
    /// <summary>
    /// Start screen + optional first-boot prologue. Runtime UI only.
    /// </summary>
    public class TitleDirector : MonoBehaviour
    {
        GameObject _titleRoot;
        GameObject _howToRoot;
        GameObject _wipeRoot;
        GameObject _prologueRoot;
        RectTransform _billStack;
        Button _start;
        RectTransform _startRt;
        RectTransform _startChip;
        Button _continue;
        RectTransform _continueRt;
        RectTransform _continueChip;
        Text _continueDay;
        Text _continueMoney;
        Text _continueDebt;
        Text _continueHead;
        Image _continueClip;
        Image _continueCashSlip;
        Image _continueShortStamp;
        Text _continueShort;
        Image _continueDebtNotice;
        Image _continueMentalNote;
        Text _continueMental;
        Text _wordmark;
        Button _how;
        Text _hint;
        Text _wipeBody;
        StudioPortrait _portrait;
        bool _busy;
        bool _howToOpen;
        bool _wipeOpen;
        bool _prologuePlaying;
        bool _canSkipPrologue;
        bool _hasSave;
        AudioSource _titleBgm;
        AudioSource _titleSfx;
        AudioClip _titleCue;
        bool _leavingTitle;

        void Awake()
        {
            UiKit.EnsureCamera(Palette.Studio);
            UiKit.EnsureEventSystem();
            UiKit.UnlockUiInputForStream();
            Build();
            RefreshContinue();
            StartTitleBgm();
        }

        void OnDestroy()
        {
            if (_titleBgm != null)
                _titleBgm.Stop();
        }

        void Update()
        {
            if (_wordmark != null)
            {
                float u = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.4f);
                _wordmark.rectTransform.localScale = Vector3.one * (1f + 0.04f * u);
            }
            TickStartPulse();
            TickContinuePulse();
            if (_busy && !_prologuePlaying)
                return;

            if (_wipeOpen && StreamBindings.Confirm)
            {
                CloseWipe();
                return;
            }

            if (_howToOpen && StreamBindings.Confirm)
            {
                CloseHowTo();
                return;
            }

            if (_prologuePlaying && _canSkipPrologue && StreamBindings.Confirm)
            {
                FinishPrologue();
                return;
            }

            if (_portrait != null && !_prologuePlaying)
                _portrait.Tick(Time.deltaTime);

            if (!_howToOpen && !_prologuePlaying && !_busy && StreamBindings.Confirm)
            {
                if (_hasSave)
                    OnContinue();
                else
                    OnStartBroadcast();
            }
        }

        void Build()
        {
            var canvas = UiKit.CreateCanvas("TitleCanvas", transform);
            StudioChrome.Wash(canvas.transform);
            var backdrop = UiKit.Image(canvas.transform, "TitleBackdrop", Color.white);
            UiKit.Stretch(backdrop.rectTransform);
            ArtSprites.Apply(backdrop, ArtSprites.TitleStudio, Palette.Studio, Color.white);
            backdrop.preserveAspect = false;
            backdrop.raycastTarget = false;
            var root = StreamSafeArea.Attach(canvas.transform);

            _titleRoot = new GameObject("TitleRoot", typeof(RectTransform));
            _titleRoot.transform.SetParent(root, false);
            UiKit.Stretch(_titleRoot.GetComponent<RectTransform>());

            var titleParent = _titleRoot.transform;
            _portrait = new StudioPortrait(titleParent, new Vector2(0.78f, 0.48f), new Vector2(440, 560), true);

            var lockup = UiKit.Panel(titleParent, "Lockup", Color.white);
            UiKit.Layout(lockup, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(56, -48), new Vector2(760, 200));
            ArtSprites.ApplySliced(lockup.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 1f, 1f, 0.92f));
            _wordmark = UiKit.Label(lockup, "GameTitle", "「파산 버튜버」", 64, Palette.Pastel, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(_wordmark.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(28, -18), new Vector2(-40, 86));
            var line = UiKit.Label(lockup, "Tagline", "빚더미에서 최고의 버튜버가 되어라.", 26, Palette.Pink, TextAnchor.UpperLeft);
            UiKit.Layout(line.rectTransform, new Vector2(0, 0), new Vector2(1, 0.48f), new Vector2(0, 0), new Vector2(28, 18), new Vector2(-40, 0));
            line.horizontalOverflow = HorizontalWrapMode.Wrap;

            _start = UiKit.Button(titleParent, "Start", "방송 시작", OnStartBroadcast, Palette.PinkDeep, Color.white);
            StyleMenuButton(_start, new Vector2(56, -40), new Vector2(420, 78), Palette.PinkDeep);
            _startRt = _start.GetComponent<RectTransform>();
            var startImg = _start.GetComponent<Image>();
            if (startImg != null)
            {
                ArtSprites.ApplySliced(startImg, ArtSprites.TitleStart, Color.white, new Vector4(48f, 36f, 48f, 36f));
                startImg.raycastTarget = true;
            }
            _startChip = UiKit.Panel(_start.transform, "StartChip", Palette.Gold);
            UiKit.Layout(_startChip, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(28f, 0f), new Vector2(52f, 26f));
            var startChipImg = _startChip.GetComponent<Image>();
            if (startChipImg != null)
                startChipImg.raycastTarget = false;
            var startChipT = UiKit.Label(_startChip, "T", "시작", 14, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(startChipT.rectTransform);
            var startCap = _start.transform.Find("Caption") as RectTransform;
            if (startCap != null)
                startCap.offsetMin = new Vector2(56f, 0f);
            _continue = UiKit.Button(titleParent, "Continue", "이어서 하기", OnContinue, Palette.Gold, Palette.Ink);
            StyleMenuButton(_continue, new Vector2(56, -154), new Vector2(560, 128), Palette.Gold);
            _continueRt = _continue.GetComponent<RectTransform>();
            var continueImg = _continue.GetComponent<Image>();
            if (continueImg != null)
            {
                ArtSprites.ApplySliced(continueImg, ArtSprites.TitleContinue, Color.white, new Vector4(48f, 36f, 48f, 36f));
                continueImg.raycastTarget = true;
            }
            _continueDay = _continue.transform.Find("Caption") != null
                ? _continue.transform.Find("Caption").GetComponent<Text>()
                : null;
            if (_continueDay != null)
            {
                _continueDay.alignment = TextAnchor.UpperLeft;
                _continueDay.fontSize = 26;
                UiKit.Layout(_continueDay.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(72, -10), new Vector2(-28, 34));
            }
            _continueChip = UiKit.Panel(_continue.transform, "ContinueChip", Palette.PinkDeep);
            UiKit.Layout(_continueChip, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(36f, -22f), new Vector2(52f, 26f));
            var contChipImg = _continueChip.GetComponent<Image>();
            if (contChipImg != null)
                contChipImg.raycastTarget = false;
            var contChipT = UiKit.Label(_continueChip, "T", "이어", 14, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Stretch(contChipT.rectTransform);
            var moneyPlate = UiKit.Panel(_continue.transform, "MoneyPlate", new Color(0, 0, 0, 0));
            UiKit.Layout(moneyPlate, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(-16f, 32f));
            _continueCashSlip = UiKit.Image(moneyPlate, "ContinueCashSlip", Color.white);
            UiKit.Layout(_continueCashSlip.rectTransform, new Vector2(0f, 0f), new Vector2(0.34f, 1f), new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(-2f, 0f));
            ArtSprites.Apply(_continueCashSlip, ArtSprites.CashSlip, new Color(0.98f, 0.94f, 0.86f, 0.98f), Color.white);
            _continueCashSlip.preserveAspect = false;
            _continueCashSlip.raycastTarget = false;
            _continueMoney = UiKit.Label(_continueCashSlip.transform, "SaveMoney", "", 15, Palette.MoneyRed, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_continueMoney.rectTransform, new Vector2(0.08f, 0.10f), new Vector2(0.94f, 0.90f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            _continueShortStamp = UiKit.Image(moneyPlate, "ContinueShortStamp", Color.white);
            UiKit.Layout(_continueShortStamp.rectTransform, new Vector2(0f, 0f), new Vector2(0.34f, 0f), new Vector2(0.5f, 1f), new Vector2(4f, -2f), new Vector2(-2f, 28f));
            ArtSprites.Apply(_continueShortStamp, ArtSprites.BillShort, Palette.MoneyRed, Color.white);
            _continueShortStamp.preserveAspect = false;
            _continueShortStamp.raycastTarget = false;
            _continueShortStamp.gameObject.SetActive(false);
            _continueShort = UiKit.Label(_continueShortStamp.transform, "ContinueShort", "청구보다 부족", 13, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_continueShort.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -1f), new Vector2(-8f, -4f));
            _continueShort.gameObject.SetActive(false);
            _continueDebtNotice = UiKit.Image(moneyPlate, "ContinueDebtNotice", Color.white);
            UiKit.Layout(_continueDebtNotice.rectTransform, new Vector2(0.33f, 0f), new Vector2(0.67f, 1f), new Vector2(0f, 0.5f), new Vector2(2f, 0f), new Vector2(-2f, 0f));
            ArtSprites.ApplySliced(_continueDebtNotice, ArtSprites.BillNotice, Color.white, new Vector4(28f, 16f, 28f, 16f));
            _continueDebtNotice.raycastTarget = false;
            _continueDebt = UiKit.Label(_continueDebtNotice.transform, "SaveDebt", "", 15, Palette.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_continueDebt.rectTransform, new Vector2(0.08f, 0.10f), new Vector2(0.94f, 0.90f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            _continueMentalNote = UiKit.Image(moneyPlate, "ContinueMentalNote", Color.white);
            UiKit.Layout(_continueMentalNote.rectTransform, new Vector2(0.66f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(2f, 0f), new Vector2(-4f, 0f));
            ArtSprites.Apply(_continueMentalNote, ArtSprites.MentalNote, new Color(1f, 0.95f, 0.72f, 0.98f), Color.white);
            _continueMentalNote.preserveAspect = false;
            _continueMentalNote.raycastTarget = false;
            _continueMental = UiKit.Label(_continueMentalNote.transform, "SaveMental", "", 15, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_continueMental.rectTransform, new Vector2(0.08f, 0.10f), new Vector2(0.94f, 0.90f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            _continueCashSlip.gameObject.SetActive(false);
            _continueDebtNotice.gameObject.SetActive(false);
            _continueMentalNote.gameObject.SetActive(false);
            _continueClip = UiKit.Image(titleParent, "ContinueClip", Color.white);
            UiKit.Layout(_continueClip.rectTransform, new Vector2(0, 0.52f), new Vector2(0, 0.52f), new Vector2(0, 0.5f), new Vector2(56, -286), new Vector2(420, 72));
            ArtSprites.Apply(_continueClip, ArtSprites.HeadlineClip, new Color(0.93f, 0.88f, 0.74f, 0.98f), Color.white);
            _continueClip.preserveAspect = false;
            _continueClip.raycastTarget = false;
            _continueHead = UiKit.Label(_continueClip.transform, "SaveHead", "", 16, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_continueHead.rectTransform, new Vector2(0.07f, 0.14f), new Vector2(0.93f, 0.86f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UiKit.Wrap(_continueHead);
            _continueClip.gameObject.SetActive(false);
            _continue.gameObject.SetActive(false);
            _how = UiKit.Button(titleParent, "HowTo", "조작 설명", OpenHowTo, Palette.StudioHi, Palette.Pastel);
            StyleMenuButton(_how, new Vector2(56, -224), new Vector2(420, 70), Palette.StudioHi);

            _hint = UiKit.Label(titleParent, "Hint", "Space / Enter  방송 시작", 18, Palette.Muted, TextAnchor.LowerLeft);
            UiKit.Layout(_hint.rectTransform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(64, 28), new Vector2(520, 28));

            BuildHowTo(root);
            BuildWipe(root);
            BuildPrologue(root);
        }

        void BuildHowTo(Transform root)
        {
            var panel = UiKit.Panel(root, "HowTo", Color.white);
            UiKit.Layout(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 540));
            ArtSprites.ApplySliced(panel.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 1f, 1f, 0.98f));
            SafeFitCard.Bind(panel, 760f, 540f);
            _howToRoot = panel.gameObject;
            _howToRoot.SetActive(false);

            var heading = UiKit.Label(panel, "H", "조작 설명", 36, Palette.Pastel, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(heading.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(0, 44));

            const string body =
                "←     긍정\n" +
                "↓     공감\n" +
                "→     웃음\n" +
                "↑     감사\n" +
                "Space / Enter  슈퍼챗 (떼면 한 번 판정)\n" +
                "1–4    방송 중 이벤트\n" +
                "A/S/D/F · WASD  같은 판정\n" +
                "화면 버튼   긍정/공감/웃음/감사";
            var keysView = UiKit.Panel(panel, "KeysView", new Color(0, 0, 0, 0));
            UiKit.Stretch(keysView, 32f, 32f, 72f, 100f);
            var keysImg = keysView.GetComponent<Image>();
            if (keysImg != null)
                keysImg.raycastTarget = true;
            var keys = UiKit.Label(keysView, "Keys", body, 28, Palette.PastelDim, TextAnchor.UpperLeft);
            UiKit.Stretch(keys.rectTransform);
            keys.lineSpacing = 1.25f;
            UiKit.MakeScrollBody(keys);

            var close = UiKit.Button(panel, "Close", "닫기  (Space)", CloseHowTo, Palette.PinkDeep, Color.white);
            UiKit.Layout(close.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(260, 56));
        }

        void BuildWipe(Transform root)
        {
            var wash = UiKit.Image(root, "WipeWash", new Color(0.06f, 0.02f, 0.06f, 0.82f));
            UiKit.Stretch(wash.rectTransform);
            wash.raycastTarget = true;
            _wipeRoot = wash.gameObject;
            var card = UiKit.Panel(wash.transform, "WipeCard", Color.white);
            UiKit.Layout(card, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 360));
            var wipeImg = card.GetComponent<Image>();
            ArtSprites.Apply(wipeImg, ArtSprites.NewGameCard, new Color(1f, 0.90f, 0.92f, 0.98f), Color.white);
            wipeImg.preserveAspect = false;
            SafeFitCard.Bind(card, 720f, 360f);
            var title = UiKit.Label(card, "WipeTitle", "새 방송 시작", 36, Palette.MoneyRed, TextAnchor.UpperCenter, FontStyle.Bold);
            UiKit.Layout(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(-24, 48));
            _wipeBody = UiKit.Label(card, "WipeBody", "진행 중인 1일차를 지울까?", 26, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(_wipeBody.rectTransform, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.78f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            UiKit.Wrap(_wipeBody);
            var pair = UiKit.Panel(card, "WipePair", new Color(0, 0, 0, 0));
            UiKit.Layout(pair, new Vector2(0.06f, 0), new Vector2(0.94f, 0.32f), new Vector2(0.5f, 0), new Vector2(0, 16), Vector2.zero);
            var wipe = UiKit.Button(pair, "WipeYes", "지우고 시작", ConfirmWipe, Palette.Troll, Color.white);
            UiKit.Layout(wipe.GetComponent<RectTransform>(), new Vector2(0, 0.1f), new Vector2(0.48f, 0.9f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var cancel = UiKit.Button(pair, "WipeNo", "취소", CloseWipe, Palette.StudioHi, Palette.Pastel);
            UiKit.Layout(cancel.GetComponent<RectTransform>(), new Vector2(0.52f, 0.1f), new Vector2(1f, 0.9f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            SafePairLayout.Bind(pair, wipe.GetComponent<RectTransform>(), cancel.GetComponent<RectTransform>(), true, false);
            _wipeRoot.SetActive(false);
        }

        void BuildPrologue(Transform root)
        {
            var panel = UiKit.Panel(root, "Prologue", new Color(0.12f, 0.04f, 0.08f, 1f));
            UiKit.Stretch(panel);
            _prologueRoot = panel.gameObject;
            _prologueRoot.SetActive(false);

            var bust = UiKit.Image(panel, "Pasan", Color.white);
            UiKit.Layout(bust.rectTransform, new Vector2(0.28f, 0.46f), new Vector2(0.28f, 0.46f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420, 520));
            ArtSprites.Apply(bust, ArtSprites.Avatar, new Color(1f, 0.83f, 0.9f, 1f));
            var cam = UiKit.Image(panel, "CamFrame", new Color(0.92f, 0.28f, 0.48f, 0.95f));
            UiKit.Layout(cam.rectTransform, new Vector2(0.28f, 0.46f), new Vector2(0.28f, 0.46f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(448, 548));
            cam.transform.SetSiblingIndex(bust.transform.GetSiblingIndex());

            _billStack = UiKit.Panel(panel, "BillStack", new Color(0, 0, 0, 0));
            UiKit.Layout(_billStack, new Vector2(0.68f, 0.48f), new Vector2(0.68f, 0.48f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 340));

            var copy = UiKit.Label(panel, "Copy", "빚더미.", 40, Palette.MoneyRed, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Layout(copy.rectTransform, new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(720, 48));

            var skip = UiKit.Label(panel, "Skip", "Space 로 건너뛰기", 18, Palette.Muted, TextAnchor.LowerCenter);
            UiKit.Layout(skip.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(360, 28));
        }

        void TickStartPulse()
        {
            if (_startRt == null)
                return;
            if (_start == null || !_start.gameObject.activeInHierarchy || _wipeOpen || _howToOpen || _prologuePlaying)
            {
                _startRt.localScale = Vector3.one;
                return;
            }
            float u = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.2f);
            _startRt.localScale = Vector3.one * (1f + 0.03f * u);
            if (_startChip != null)
                _startChip.localScale = Vector3.one * (1f + 0.08f * u);
        }

        void TickContinuePulse()
        {
            if (_continueRt == null)
                return;
            if (_continue == null || !_continue.gameObject.activeInHierarchy || _wipeOpen || _howToOpen || _prologuePlaying)
            {
                _continueRt.localScale = Vector3.one;
                return;
            }
            float u = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.2f);
            _continueRt.localScale = Vector3.one * (1f + 0.03f * u);
            if (_continueChip != null)
                _continueChip.localScale = Vector3.one * (1f + 0.08f * u);
        }

        void OpenHowTo()
        {
            if (_busy || _prologuePlaying || _wipeOpen)
                return;
            _howToOpen = true;
            _howToRoot.SetActive(true);
        }

        void CloseHowTo()
        {
            _howToOpen = false;
            _howToRoot.SetActive(false);
        }

        void RefreshContinue()
        {
            var peek = new GameRunState();
            _hasSave = RunSave.HasValidSave() && RunSave.TryLoad(peek);
            _continue.gameObject.SetActive(_hasSave);
            bool hasHead = _hasSave && peek.lastHeadline != null && peek.lastHeadline.Length > 0;
            if (_hasSave)
                FillContinue(peek);
            if (_continueCashSlip != null)
                _continueCashSlip.gameObject.SetActive(_hasSave);
            if (!_hasSave)
            {
                if (_continueShortStamp != null)
                    _continueShortStamp.gameObject.SetActive(false);
                if (_continueShort != null)
                    _continueShort.gameObject.SetActive(false);
            }
            if (_continueDebtNotice != null)
                _continueDebtNotice.gameObject.SetActive(_hasSave);
            if (_continueMentalNote != null)
                _continueMentalNote.gameObject.SetActive(_hasSave);
            if (_continueClip != null)
                _continueClip.gameObject.SetActive(hasHead);
            var caption = _start.transform.Find("Caption") != null
                ? _start.transform.Find("Caption").GetComponent<Text>()
                : null;
            if (caption != null)
                caption.text = _hasSave ? "새 방송 시작" : "방송 시작";
            if (_hint != null)
                _hint.text = _hasSave ? "Space / Enter  이어서 하기" : "Space / Enter  방송 시작";
            if (_how != null)
                StyleMenuButton(_how, new Vector2(56, !_hasSave ? -132 : hasHead ? -340 : -252), new Vector2(420, 70), Palette.StudioHi);
        }

        void FillContinue(GameRunState peek)
        {
            if (_continueDay != null)
                _continueDay.text = "이어하기 " + peek.day + "일차";
            int bills = EconomyRules.TonightBills(peek);
            bool shortfall = bills > 0 && peek.cash < bills;
            if (_continueMoney != null)
            {
                _continueMoney.text = "현금 " + EconomyRules.FormatWon(peek.cash);
                _continueMoney.color = shortfall ? Palette.MoneyRed : Palette.Pastel;
            }
            if (_continueShortStamp != null)
            {
                if (shortfall)
                {
                    ArtSprites.Apply(_continueShortStamp, ArtSprites.BillShort, Palette.MoneyRed, Color.white);
                    _continueShortStamp.preserveAspect = false;
                }
                _continueShortStamp.gameObject.SetActive(shortfall);
            }
            if (_continueShort != null)
            {
                if (shortfall)
                {
                    _continueShort.text = "청구보다 부족";
                    _continueShort.color = Palette.MoneyRed;
                }
                _continueShort.gameObject.SetActive(shortfall);
            }
            if (_continueDebt != null)
            {
                _continueDebt.text = "부채 " + EconomyRules.FormatWon(peek.debt);
                _continueDebt.color = Palette.Gold;
            }
            if (_continueMental != null)
            {
                _continueMental.text = "멘탈 " + peek.mental;
                _continueMental.color = peek.mental <= 20 ? Palette.MoneyRed : Palette.Ink;
            }
            bool hasHead = peek.lastHeadline != null && peek.lastHeadline.Length > 0;
            if (_continueHead != null)
            {
                _continueHead.text = hasHead ? "어제: " + peek.lastHeadline : "";
                _continueHead.gameObject.SetActive(hasHead);
            }
        }

        void OpenWipe()
        {
            if (_wipeRoot == null)
                return;
            int day = 1;
            var peek = new GameRunState();
            if (RunSave.TryLoad(peek) && peek.day > 0)
                day = peek.day;
            if (_wipeBody != null)
                _wipeBody.text = "진행 중인 " + day + "일차를 지울까?";
            _wipeOpen = true;
            _wipeRoot.SetActive(true);
            _wipeRoot.transform.SetAsLastSibling();
        }

        void CloseWipe()
        {
            _wipeOpen = false;
            if (_wipeRoot != null)
                _wipeRoot.SetActive(false);
        }

        void ConfirmWipe()
        {
            CloseWipe();
            LeaveTitle(BeginNewRun);
        }

        void OnContinue()
        {
            if (_busy || _prologuePlaying || _leavingTitle)
                return;
            if (_wipeOpen)
            {
                CloseWipe();
                return;
            }
            if (_howToOpen)
            {
                CloseHowTo();
                return;
            }

            LeaveTitle(() =>
            {
                var gm = GameManager.Instance;
                if (gm == null)
                    return;
                if (gm.ContinueRun())
                    return;
                RefreshContinue();
                BeginNewRun();
            });
        }

        void OnStartBroadcast()
        {
            if (_busy || _prologuePlaying || _leavingTitle)
                return;
            if (_wipeOpen)
                return;
            if (_howToOpen)
            {
                CloseHowTo();
                return;
            }

            if (_hasSave)
            {
                OpenWipe();
                return;
            }

            LeaveTitle(BeginNewRun);
        }

        void BeginNewRun()
        {
            var gm = GameManager.Instance;
            if (gm == null)
                return;

            gm.StartNewRun();
            if (gm.ShouldPlayPrologue())
                StartCoroutine(PlayPrologue(gm));
            else
                gm.GoWeekStart();
        }

        IEnumerator PlayPrologue(GameManager gm)
        {
            _busy = true;
            _prologuePlaying = true;
            _canSkipPrologue = false;
            _titleRoot.SetActive(false);
            _howToRoot.SetActive(false);
            _prologueRoot.SetActive(true);
            gm.MarkPrologueSeen();

            SpawnBillStack();
            yield return new WaitForSeconds(0.85f);
            _canSkipPrologue = true;
            yield return new WaitForSeconds(5.65f);
            FinishPrologue();
        }

        void SpawnBillStack()
        {
            for (int i = 0; i < _billStack.childCount; i++)
                Destroy(_billStack.GetChild(i).gameObject);

            // One red bill stack — not the six-card WeekStart wave.
            for (int i = 0; i < 4; i++)
            {
                var card = UiKit.Panel(_billStack, "Bill" + i, new Color(1f, 0.88f, 0.9f, 0.98f));
                float y = -18f + i * 22f;
                float rot = (i - 1.5f) * 6f;
                UiKit.Layout(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(i * 4f, y + 80f), new Vector2(168, 210));
                card.localRotation = Quaternion.Euler(0, 0, rot);
                var icon = UiKit.Image(card, "Icon", Color.white);
                UiKit.Layout(icon.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(100, 100));
                ArtSprites.Apply(icon, ArtSprites.BillRent, Palette.MoneyRed, Palette.MoneyRed);
                var amt = UiKit.Label(card, "A", "청구서", 20, Palette.MoneyRed, TextAnchor.LowerCenter, FontStyle.Bold);
                UiKit.Layout(amt.rectTransform, new Vector2(0, 0), new Vector2(1, 0.28f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                StartCoroutine(SlamCard(card, i * 4f, y, i * 0.08f));
            }
        }

        IEnumerator SlamCard(RectTransform card, float x, float y, float delay)
        {
            yield return new WaitForSeconds(delay);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2.8f;
                float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
                card.anchoredPosition = new Vector2(x, Mathf.Lerp(y + 80f, y, e));
                card.localScale = Vector3.one * Mathf.Lerp(0.72f, 1f, e);
                yield return null;
            }

            card.anchoredPosition = new Vector2(x, y);
        }

        void FinishPrologue()
        {
            if (!_prologuePlaying)
                return;
            _prologuePlaying = false;
            _canSkipPrologue = false;
            StopAllCoroutines();
            GameManager.Instance.GoWeekStart();
        }

        static void StyleMenuButton(Button btn, Vector2 pos, Vector2 size, Color tint)
        {
            var rt = btn.GetComponent<RectTransform>();
            UiKit.Layout(rt, new Vector2(0, 0.52f), new Vector2(0, 0.52f), new Vector2(0, 0.5f), pos, size);
            var img = btn.GetComponent<Image>();
            ArtSprites.ApplySliced(img, ArtSprites.BubblePill, tint);
            img.raycastTarget = true;
            var cap = btn.GetComponentInChildren<Text>();
            if (cap != null)
                cap.fontSize = 30;
        }

        void StartTitleBgm()
        {
            var clip = Resources.Load<AudioClip>("Audio/bgm_title");
            if (clip == null)
                return;
            _titleBgm = gameObject.AddComponent<AudioSource>();
            _titleBgm.clip = clip;
            _titleBgm.loop = true;
            _titleBgm.playOnAwake = false;
            _titleBgm.volume = 0.28f;
            _titleBgm.Play();
            _titleSfx = gameObject.AddComponent<AudioSource>();
            _titleSfx.playOnAwake = false;
            _titleCue = Resources.Load<AudioClip>("Audio/sfx_title");
        }

        void PlayTitleSfx()
        {
            if (_titleSfx != null && _titleCue != null)
                _titleSfx.PlayOneShot(_titleCue, 0.46f);
        }

        void LeaveTitle(System.Action next)
        {
            if (_leavingTitle)
                return;
            _leavingTitle = true;
            _busy = true;
            PlayTitleSfx();
            StartCoroutine(FadeTitleBgmThen(next));
        }

        IEnumerator FadeTitleBgmThen(System.Action next)
        {
            if (_titleBgm != null && _titleBgm.isPlaying)
            {
                float start = _titleBgm.volume;
                float t = 0f;
                const float fade = 0.2f;
                while (t < fade)
                {
                    t += Time.deltaTime;
                    _titleBgm.volume = Mathf.Lerp(start, 0f, t / fade);
                    yield return null;
                }
                _titleBgm.Stop();
            }
            next?.Invoke();
        }
    }
}

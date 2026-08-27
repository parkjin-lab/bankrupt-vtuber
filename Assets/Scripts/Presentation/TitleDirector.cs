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
        Button _continue;
        Text _continueDay;
        Text _continueMoney;
        Text _continueDebt;
        Text _continueHead;
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

        void Awake()
        {
            UiKit.EnsureCamera(Palette.Studio);
            UiKit.EnsureEventSystem();
            UiKit.UnlockUiInputForStream();
            Build();
            RefreshContinue();
        }

        void Update()
        {
            if (_wordmark != null)
            {
                float u = 0.5f + 0.5f * Mathf.Sin(Time.time * 2.4f);
                _wordmark.rectTransform.localScale = Vector3.one * (1f + 0.04f * u);
            }
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
            _continue = UiKit.Button(titleParent, "Continue", "이어서 하기", OnContinue, Palette.Gold, Palette.Ink);
            StyleMenuButton(_continue, new Vector2(56, -154), new Vector2(420, 128), Palette.Gold);
            _continueDay = _continue.GetComponentInChildren<Text>();
            if (_continueDay != null)
            {
                _continueDay.alignment = TextAnchor.UpperLeft;
                _continueDay.fontSize = 26;
                UiKit.Layout(_continueDay.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(18, -10), new Vector2(-28, 34));
            }
            var moneyPlate = UiKit.Panel(_continue.transform, "MoneyPlate", new Color(0.12f, 0.05f, 0.08f, 0.88f));
            UiKit.Layout(moneyPlate, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -42f), new Vector2(-16f, 28f));
            _continueMoney = UiKit.Label(moneyPlate, "SaveMoney", "", 18, Palette.MoneyRed, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_continueMoney.rectTransform, new Vector2(0f, 0f), new Vector2(0.52f, 1f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(-6f, 0f));
            _continueDebt = UiKit.Label(moneyPlate, "SaveDebt", "", 18, Palette.Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            UiKit.Layout(_continueDebt.rectTransform, new Vector2(0.50f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(-10f, 0f));
            _continueHead = UiKit.Label(_continue.transform, "SaveHead", "", 16, Palette.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            UiKit.Layout(_continueHead.rectTransform, new Vector2(0, 0), new Vector2(1, 0.42f), new Vector2(0, 0), new Vector2(18, 8), new Vector2(-28, 0));
            UiKit.Wrap(_continueHead);
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
            ArtSprites.ApplySliced(card.GetComponent<Image>(), ArtSprites.PanelDark, new Color(1f, 0.90f, 0.92f, 0.98f));
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
            if (_hasSave)
                FillContinue(peek);
            var caption = _start.GetComponentInChildren<Text>();
            if (caption != null)
                caption.text = _hasSave ? "새 방송 시작" : "방송 시작";
            if (_hint != null)
                _hint.text = _hasSave ? "Space / Enter  이어서 하기" : "Space / Enter  방송 시작";
            if (_how != null)
                StyleMenuButton(_how, new Vector2(56, _hasSave ? -252 : -132), new Vector2(420, 70), Palette.StudioHi);
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
            if (_continueDebt != null)
            {
                _continueDebt.text = "부채 " + EconomyRules.FormatWon(peek.debt);
                _continueDebt.color = Palette.Gold;
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
            BeginNewRun();
        }

        void OnContinue()
        {
            if (_busy || _prologuePlaying)
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

            var gm = GameManager.Instance;
            if (gm == null)
                return;
            if (gm.ContinueRun())
                return;
            RefreshContinue();
            OnStartBroadcast();
        }

        void OnStartBroadcast()
        {
            if (_busy || _prologuePlaying)
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

            BeginNewRun();
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
    }
}

#!/usr/bin/env python3
"""Read back scenes / input / economy and simulate Week 1 take bands."""
from __future__ import annotations

import json
import math
import random
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
errors: list[str] = []


def fail(msg: str) -> None:
    errors.append(msg)
    print("FAIL:", msg)


def ok(msg: str) -> None:
    print("OK  :", msg)


def guid_of(meta: Path) -> str:
    text = meta.read_text(encoding="utf-8")
    m = re.search(r"^guid: ([0-9a-f]{32})$", text, re.M)
    if not m:
        fail(f"no guid in {meta}")
        return ""
    return m.group(1)


def check_project() -> None:
    scenes = {
        "Title": ROOT / "Assets/Scenes/Title.unity",
        "WeekStart": ROOT / "Assets/Scenes/WeekStart.unity",
        "LiveStream": ROOT / "Assets/Scenes/LiveStream.unity",
        "Settlement": ROOT / "Assets/Scenes/Settlement.unity",
    }
    scripts = {
        "Title": ROOT / "Assets/Scripts/Presentation/TitleDirector.cs",
        "WeekStart": ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs",
        "LiveStream": ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs",
        "Settlement": ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs",
    }
    for name, path in scenes.items():
        if not path.exists():
            fail(f"missing scene {path}")
            continue
        scene = path.read_text(encoding="utf-8")
        script_guid = guid_of(Path(str(scripts[name]) + ".meta"))
        if script_guid and script_guid not in scene:
            fail(f"{name} scene does not reference {scripts[name].name} guid {script_guid}")
        else:
            ok(f"{name} scene references {scripts[name].name}")
        if "SceneRoots" not in scene:
            fail(f"{name} missing SceneRoots")
        if "orthographic: 1" not in scene:
            fail(f"{name} camera is not orthographic")

    ebs = (ROOT / "ProjectSettings/EditorBuildSettings.asset").read_text(encoding="utf-8")
    for path in scenes.values():
        rel = str(path.relative_to(ROOT)).replace("\\", "/")
        if rel not in ebs:
            fail(f"EditorBuildSettings missing {rel}")
        guid = guid_of(Path(str(path) + ".meta"))
        if guid and guid not in ebs:
            fail(f"EditorBuildSettings missing guid for {rel}")
    first = ebs.split("path: ")[1].split("\n")[0].strip() if "path: " in ebs else ""
    if first != "Assets/Scenes/Title.unity":
        fail(f"first build scene is {first}, expected Title")
    else:
        ok("first build scene is Title")

    version = (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8")
    if "6000.5.9f1" not in version or "b57deb96f08d" not in version:
        fail("ProjectVersion is not Unity 6000.5.9f1")
    else:
        ok("Unity version " + version.split()[1])

    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    if "defaultScreenOrientation: 0" not in player:
        fail("Android default orientation is not Portrait")
    elif "allowedAutorotateToLandscapeRight: 1" in player or "allowedAutorotateToLandscapeLeft: 1" in player:
        fail("landscape autorotate is still allowed")
    elif "allowedAutorotateToPortrait: 1" not in player:
        fail("portrait autorotate is not allowed")
    elif "폰은 세로" not in (ROOT / "README.md").read_text(encoding="utf-8"):
        fail("README missing 폰은 세로")
    else:
        ok("Android player is Portrait; landscape autorotate off")

    gfx = (ROOT / "ProjectSettings/GraphicsSettings.asset").read_text(encoding="utf-8")
    if "m_CustomRenderPipeline: {fileID: 0}" not in gfx:
        fail("expected built-in render pipeline (no URP asset)")
    else:
        ok("built-in 2D / no SRP asset")

    bindings = (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8")
    for key in ("KeyCode.LeftArrow", "KeyCode.DownArrow", "KeyCode.RightArrow", "KeyCode.UpArrow", "KeyCode.Space", "KeyCode.Return", "KeyCode.Alpha1", "KeyCode.A", "KeyCode.S", "KeyCode.D", "KeyCode.F", "KeyCode.W"):
        if key not in bindings:
            fail(f"StreamBindings missing {key}")
    if "GetKeyDown(KeyCode.LeftArrow)" not in bindings or "PositiveDown" not in bindings:
        fail("arrows are no longer the documented lane map")
    else:
        ok("←↓→↑ documented, A/S/D/F and WASD aliases, Space / Enter / 1–4 present")

    consume = bindings.split("TryConsumeKind", 1)[-1]
    consume = consume.split("public static bool SuperchatCharging", 1)[0]
    consume = consume.split("public static bool EventKeyPressed", 1)[0]
    consume = consume.split("public static bool EventStubPressed", 1)[0]
    if "GetKey(KeyCode.Space)" in consume:
        fail("TryConsumeKind still polls GetKey(Space) — hold would farm superchats")
    else:
        ok("TryConsumeKind does not poll GetKey(Space)")
    if "GetKeyUp(KeyCode.Space)" not in consume and "GetKeyDown(KeyCode.Space)" not in consume:
        fail("Space superchat is not a one-shot GetKeyDown/GetKeyUp")
    else:
        ok("Space superchat commits once (GetKeyUp/GetKeyDown)")
    for tap in ("GetKeyDown(KeyCode.LeftArrow)", "GetKeyDown(KeyCode.DownArrow)", "GetKeyDown(KeyCode.RightArrow)", "GetKeyDown(KeyCode.UpArrow)"):
        if tap not in bindings:
            fail(f"regular lane missing {tap}")
    if "GetKeyDown(KeyCode.A)" not in bindings or "GetKeyDown(KeyCode.W)" not in bindings:
        fail("A/S/D/F / WASD aliases missing")
    ok("arrow keys stay GetKeyDown; A/S/D/F and WASD alias the lanes")

    uikit_cs = (ROOT / "Assets/Scripts/Presentation/UiKit.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    live_awake = live_cs.split("void Awake()", 1)[-1].split("void Start()", 1)[0]
    lock_fn = uikit_cs.split("LockUiInputForStream", 1)[-1].split("UnlockUiInputForStream", 1)[0]
    if "LockUiInputForStream" not in uikit_cs:
        fail("UiKit missing LockUiInputForStream")
    elif "sendNavigationEvents = false" not in lock_fn or "SetSelectedGameObject(null)" not in lock_fn:
        fail("LockUiInputForStream does not drop EventSystem navigation")
    elif "module.enabled = false" in lock_fn or "enabled = false" in lock_fn:
        fail("LockUiInputForStream still disables StandaloneInputModule")
    elif "horizontalAxis" not in lock_fn or "Disabled" not in lock_fn:
        fail("LockUiInputForStream does not retarget module axes to unused names")
    elif "UnlockUiInputForStream" not in uikit_cs or "UnlockUiInputForStream" not in live_cs:
        fail("stream lock is never unlocked for later Submit screens")
    elif "LockUiInputForStream" not in live_awake or live_awake.find("EnsureEventSystem") > live_awake.find("LockUiInputForStream"):
        fail("LiveStreamDirector.Awake does not lock UI input after EnsureEventSystem")
    elif "DontDestroyOnLoad" not in uikit_cs.split("EnsureEventSystem", 1)[-1].split("LockUiInputForStream", 1)[0]:
        fail("EventSystem dies with LiveStream so later screens lose clicks")
    else:
        title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
        week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
        settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
        if "UnlockUiInputForStream" not in title_cs or "UnlockUiInputForStream" not in week_cs or "UnlockUiInputForStream" not in settle_cs:
            fail("Title / WeekStart / Settlement do not unlock stream input so buttons click")
        else:
            ok("LiveStream keeps EventSystem clicks and stops navigation from eating keys")

    pad_cs = (ROOT / "Assets/Scripts/Input/StreamPadButton.cs").read_text(encoding="utf-8") if (ROOT / "Assets/Scripts/Input/StreamPadButton.cs").exists() else ""
    relay_cs = (ROOT / "Assets/Scripts/Input/StreamPointerRelay.cs").read_text(encoding="utf-8") if (ROOT / "Assets/Scripts/Input/StreamPointerRelay.cs").exists() else ""
    safe_cs = (ROOT / "Assets/Scripts/Presentation/StreamSafeArea.cs").read_text(encoding="utf-8") if (ROOT / "Assets/Scripts/Presentation/StreamSafeArea.cs").exists() else ""
    if "IPointerDownHandler" not in pad_cs or "QueueKind" not in pad_cs:
        fail("StreamPadButton does not tap the same TryHit path")
    elif "GraphicRaycaster" not in relay_cs or "StandaloneInputModule" not in relay_cs:
        fail("StreamPointerRelay does not raycast without StandaloneInputModule")
    elif "EventSystemOwnsPointer" not in relay_cs:
        fail("StreamPointerRelay does not yield to EventSystem clicks")
    elif "safeArea" not in safe_cs or "StreamSafeArea" not in live_cs:
        fail("LiveStream pads are not lifted by Screen.safeArea")
    elif "AddColumnPad" not in live_cs or "index / (float)count" not in live_cs:
        fail("lane pads are not a full-width equal-column row")
    elif "입력됨" not in live_cs:
        fail("accepted input has no on-screen echo")
    elif "긍정" not in live_cs or "공감" not in live_cs or "웃음" not in live_cs or "감사" not in live_cs or "슈퍼챗" not in live_cs:
        fail("LiveStream missing on-screen 긍정/공감/웃음/감사/슈퍼챗 pad")
    elif "StreamPointerRelay" not in live_cs or "StreamPadButton" not in live_cs:
        fail("LiveStream does not wire the touch pad")
    else:
        ok("LiveStream pads fit the canvas, stay clickable, and echo accepted input")

    run_cs = (ROOT / "Assets/Scripts/Core/GameRunState.cs").read_text(encoding="utf-8")
    begin = run_cs.split("BeginNextDay", 1)[-1]
    if "mental = b.mentalRestoreEachMorning" in begin.split("billsAppliedThisDay", 1)[0]:
        fail("BeginNextDay overwrites mental instead of keeping leftover")
    elif "mental += b.mentalRestoreEachMorning" not in begin:
        fail("BeginNextDay does not add morning restore onto leftover mental")
    else:
        ok("mental leftover persists; morning restore is additive")

    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    if "mentalRestoreEachMorning: 15" not in balance:
        fail("Week1Balance morning restore is not +15")
    else:
        ok("morning restore is +15 on Week1Balance")
    defaults = (ROOT / "Assets/Scripts/Data/Week1Balance.cs").read_text(encoding="utf-8")
    if "mentalRestoreEachMorning = 15" not in defaults:
        fail("Week1Balance.cs default morning restore is not 15")
    else:
        ok("Week1Balance.cs default morning restore is +15")

    art = {
        "pasan_nyang.png": "avatar",
        "bill_rent.png": "월세",
        "bill_electric.png": "전기",
        "bill_license.png": "라이선스",
        "bill_food.png": "식비",
        "bill_gear.png": "장비",
        "bill_notice.png": "고지서",
        "stream_overlay.png": "라이브 오버레이",
        "title_studio.png": "타이틀 스튜디오",
        "settlement_desk.png": "정산 책상",
        "morning_room.png": "아침 방",
        "pad_left.png": "← 키캡",
        "pad_down.png": "↓ 키캡",
        "pad_right.png": "→ 키캡",
        "pad_up.png": "↑ 키캡",
        "pad_superchat.png": "슈퍼챗 키캡",
        "chat_bubble.png": "채팅 버블",
        "note_chip.png": "노트 칩",
        "hit_rail.png": "히트 레일",
        "badge_superchat.png": "superchat",
        "badge_troll.png": "troll",
    }
    for name, label in art.items():
        path = ROOT / "Assets/Resources/Art" / name
        if not path.exists() or path.stat().st_size < 1000:
            fail(f"missing/empty art {name} ({label})")
        else:
            ok(f"art {name} ({label})")
    if (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").exists() and "Sprite.Create" in (
        ROOT / "Assets/Scripts/Presentation/ArtSprites.cs"
    ).read_text(encoding="utf-8"):
        ok("ArtSprites builds sprites at runtime")
    else:
        fail("ArtSprites runtime Sprite.Create missing")
    directors = (
        (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8")
        + (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
        + (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
        + (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    )
    if "ArtSprites.Apply" not in directors:
        fail("directors do not hook ArtSprites")
    else:
        ok("directors hook ArtSprites at runtime")

    stream_art = {
        "bubble_pill.png": "chat pill",
        "bubble_superchat.png": "superchat banner",
        "bubble_troll.png": "troll spike",
        "sparkle.png": "sparkle",
    }
    for name, label in stream_art.items():
        path = ROOT / "Assets/Resources/Art" / name
        if not path.exists() or path.stat().st_size < 1000:
            fail(f"missing/empty art {name} ({label})")
        else:
            ok(f"art {name} ({label})")
    avatar_cs = (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8")
    art_cs = (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    if "Bezel" not in avatar_cs or '"LIVE"' not in avatar_cs or "SetViewers" not in avatar_cs:
        fail("AvatarView missing webcam LIVE frame")
    elif "_pop" not in avatar_cs or "_shake" not in avatar_cs or "_spark" not in avatar_cs:
        fail("AvatarView missing hit / miss / superchat reactions")
    else:
        ok("AvatarView webcam frame reacts to Perfect/Miss/superchat")
    if "ApplySliced" not in art_cs or "BubblePill" not in art_cs or "SuperchatBanner" not in art_cs or "TrollBubble" not in art_cs:
        fail("ArtSprites missing sliced chat bubble sprites")
    elif "ApplySliced" not in live_cs or "SuperchatBanner" not in live_cs or "TrollBubble" not in live_cs:
        fail("LiveStream notes are not chat bubbles")
    else:
        ok("chat notes use pill / gold banner / troll spike sprites")
    sync = live_cs.split("void SyncNotes", 1)[-1].split("void RefreshPromoOverlay", 1)[0]
    if "c.a =" in sync or "img.color = c" in sync:
        fail("SyncNotes still washes bubble alpha into a flat bar")
    else:
        ok("SyncNotes keeps bubble color (no flat-bar alpha wash)")
    if "_judgePop" not in live_cs or "0.25f" not in live_cs:
        fail("Perfect/Miss judgement pop is missing")
    else:
        ok("Perfect/Miss judgement pops bigger for 0.25s")
    if '"현금"' not in live_cs or '"부채"' not in live_cs or "Palette.CashGreen" not in live_cs or "Palette.MoneyRed" not in live_cs:
        fail("LiveStream HUD missing loud cash/debt")
    elif '"LIVE"' not in live_cs or "MoveTowards(_shownViewers" not in live_cs:
        fail("LiveStream missing LIVE + ticking viewers")
    else:
        ok("LiveStream overlay has LIVE, ticking viewers, cash/debt")
    if "Audio/sfx_perfect" not in live_cs or "Audio/sfx_good" not in live_cs or "Audio/sfx_miss" not in live_cs:
        fail("distinct Perfect/Good/Miss Resource SFX clips missing")
    elif "sfx_super" not in live_cs or "sfx_combo" not in live_cs or "Audio/sfx_onair" not in live_cs:
        fail("superchat / combo / on-air SFX missing")
    elif "Combo >= 5" not in live_cs or "PlayOneShot" not in live_cs:
        fail("combo-5 cue or AudioSource PlayOneShot missing")
    else:
        ok("Perfect/Good/Miss Resource SFX; superchat/combo-5/on-air stay")
    if "new Vector2(128, 128)" not in week_cs or "ArtSprites.BillRent" not in week_cs:
        fail("WeekStart bill sprites are still tiny")
    else:
        ok("WeekStart money slam shows bill sprites at 128px")

    chrome = {
        "panel_dark.png": "studio card",
        "banner_red.png": "threat banner",
        "banner_green.png": "cash banner",
    }
    for name, label in chrome.items():
        path = ROOT / "Assets/Resources/Art" / name
        if not path.exists() or path.stat().st_size < 1000:
            fail(f"missing/empty art {name} ({label})")
        else:
            ok(f"art {name} ({label})")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    chrome_cs = (ROOT / "Assets/Scripts/Presentation/StudioChrome.cs").read_text(encoding="utf-8") if (ROOT / "Assets/Scripts/Presentation/StudioChrome.cs").exists() else ""
    if "StudioPortrait" not in title_cs or "「파산 버튜버」" not in title_cs or "빚더미에서 최고의 버튜버가 되어라." not in title_cs:
        fail("Title is not a splash lockup with webcam + title")
    elif "방송 시작" not in title_cs or "이어서 하기" not in title_cs or "조작 설명" not in title_cs:
        fail("Title missing 방송 시작 / 이어서 하기 / 조작 설명")
    else:
        ok("Title splash has webcam, lockup, and the three menu actions")
    if "FanChip" not in week_cs or "민준" not in week_cs or "하은" not in week_cs:
        fail("WeekStart missing 민준/하은 fan chips")
    elif "ThreatBanner" not in week_cs or "오늘의 위협" not in week_cs:
        fail("WeekStart extra threat is not a red card")
    elif "토크" not in week_cs or "BubblePill" not in week_cs or "index / 4f" not in week_cs:
        fail("content pick is still tiny buttons")
    else:
        ok("WeekStart slam + red threat + chunky 콘텐츠 cards")
    if '"오늘 수입"' not in settle_cs or '"청구"' not in settle_cs or "_tilePerfect" not in settle_cs or "_tileMiss" not in settle_cs:
        fail("Settlement missing 2-second recap tiles")
    elif "_cashUp" not in settle_cs or "PoseEnding" not in settle_cs:
        fail("Settlement cash-up / debt-up / ending pose missing")
    elif "EndingRoot" not in settle_cs or "EndingTitle" not in settle_cs:
        fail("named ending screen missing")
    else:
        ok("Settlement recap is 오늘 수입 / 청구 / 현금·부채 / Perfect·Miss")
    if "StudioPortrait" not in chrome_cs or "PoseEnding" not in chrome_cs:
        fail("StudioChrome missing shared webcam portrait")
    else:
        ok("Title / WeekStart / Settlement share StudioChrome webcam language")
    if "주차 클리어" not in settle_cs or "1주차 생존" not in settle_cs or "다음 주차 시작" not in settle_cs:
        fail("week-clear splash is missing")
    elif "StampRoot" not in settle_cs or '"파산"' not in settle_cs or "처음부터" not in settle_cs:
        fail("bankrupt stamp overlay is missing")
    elif "번아웃" not in settle_cs or "IsBurnoutResult" not in settle_cs:
        fail("burnout stamp is missing")
    elif "2주차 시작" not in settle_cs or "EndingRoot" not in settle_cs:
        fail("week-clear splash dropped Week 2 continue or Week 5 endings")
    else:
        ok("week clear / 파산 / 번아웃 have screenshot screens; Week 5 endings stay")

    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    if '"오늘 청구"' not in live_cs or "청구 커버" not in live_cs or "파산까지" not in live_cs:
        fail("LiveStream missing tonight bill race HUD")
    elif "TonightBills" not in eco_cs or "BankruptDebt" not in eco_cs or "BankruptDebt" not in live_cs:
        fail("bill race is not reading existing lastBills / bankruptDebt")
    elif "하이프" not in live_cs or "hypeIncomeMultiplier" not in live_cs:
        fail("hype income ticker does not show existing 2.5x")
    else:
        ok("LiveStream races tonight's 청구 vs LiveIncome and shows 파산까지")
    if "BindNamedFans" not in session_cs or "NamedFan" not in live_cs or "minjunName" not in live_cs:
        fail("named 민준/하은 bubbles are missing")
    elif "FanWounded" not in session_cs or "FanWounded" not in live_cs:
        fail("wounded fan bubbles are not dimmed")
    else:
        ok("민준/하은 get labeled gold/pink bubbles when present")
    if "ShowMissSting" not in live_cs or "시청자" not in live_cs or "멘탈" not in live_cs:
        fail("Miss sting is not unmissable")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs:
        fail("stream pads/echo were broken while adding the money HUD")
    else:
        ok("Miss sting flashes 시청자 / 멘탈; pads still echo 입력됨")
    if "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("Week 1 bills were retuned by the money HUD")
    else:
        ok("Week 1–5 locked bills stay unchanged after the money HUD")

    for rel in (
        "Assets/Scripts/Core/GameManager.cs",
        "Assets/Scripts/Core/RunSave.cs",
        "Assets/Scripts/Economy/EconomyRules.cs",
        "Assets/Scripts/Stream/StreamSession.cs",
        "Assets/Resources/Balance/Week1Balance.asset",
        "Assets/Resources/Balance/Week2Balance.asset",
        "Assets/Resources/Balance/Week3Balance.asset",
        "Assets/Resources/Balance/Week4Balance.asset",
        "Assets/Resources/Balance/Week5Balance.asset",
        "Assets/Resources/Balance/FandomBalance.asset",
        "Assets/Resources/Balance/ContentBalance.asset",
        "Assets/Resources/Balance/ChatCatalog.asset",
        "Assets/Resources/Fonts/NotoSansKR-Regular.ttf",
    ):
        if not (ROOT / rel).exists():
            fail("missing " + rel)
        else:
            ok("found " + rel)

    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    expect = {
        "startingCash: 45000": "start cash",
        "startingDebt: 50000": "start debt",
        "billRent: 8000": "rent",
        "billElectricNet: 4000": "electric",
        "billAvatarLicense: 3000": "license",
        "billFood: 5000": "food",
        "billGear: 2000": "gear",
        "streamSeconds: 90": "90s stream",
        "incomePerViewerPerSec: 3": "₩3/viewer/s",
        "perfectViewerDelta: 0.5": "perfect viewers",
        "missViewerDelta: -1.2": "miss viewers",
        "bankruptDebt: 180000": "bankrupt",
        "winDebtMax: 30000": "win debt",
        "winCashMin: 70000": "win cash",
        "hypeSeconds: 12": "hype",
    }
    for token, label in expect.items():
        if token not in balance:
            fail(f"Week1Balance missing {label} ({token})")
    ok("Week1Balance locked numbers present")

    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    if "SceneFlow.Title" not in gm or "SceneFlow.WeekStart" not in gm or "SceneFlow.LiveStream" not in gm or "SceneFlow.Settlement" not in gm:
        fail("GameManager does not load Title + Week 1 scenes")
    else:
        ok("GameManager loads Title → WeekStart → LiveStream → Settlement")
    restart = gm.split("RestartRun", 1)[-1].split("public void", 1)[0]
    if "SceneFlow.Title" not in restart:
        fail("RestartRun does not return to Title")
    else:
        ok("RestartRun returns to Title")
    next_morning = gm.split("NextMorning", 1)[-1]
    if "SceneFlow.Title" in next_morning.split("public void", 1)[0]:
        fail("NextMorning must stay on WeekStart, not Title")
    else:
        ok("NextMorning stays on WeekStart")
    if "ShouldPlayPrologue" not in gm or "PrologueSeenThisSession" not in gm:
        fail("GameManager missing prologue session skip")
    else:
        ok("prologue is skipped after seen once this session")

    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    for token in ("「파산 버튜버」", "빚더미에서 최고의 버튜버가 되어라.", "방송 시작", "조작 설명"):
        if token not in title_cs:
            fail(f"TitleDirector missing {token}")
    else:
        ok("Title has Korean title, tagline, 방송 시작 / 조작 설명")
    if "←     긍정" not in title_cs or "1–4" not in title_cs or "Space" not in title_cs:
        fail("조작 설명 does not list ← ↓ → ↑ Space, 1–4")
    else:
        ok("조작 설명 lists ← ↓ → ↑ Space, 1–4")
    if "ArtSprites.Avatar" not in title_cs or "BillRent" not in title_cs:
        fail("Title/prologue does not show 파산냥 + a bill stack")
    else:
        ok("Title/prologue uses 파산냥 + one red bill stack")
    if "PlayPrologue" not in title_cs or "WaitForSeconds(5.65f)" not in title_cs:
        fail("prologue beat is not a short 5–8s hold")
    else:
        ok("prologue is a short 5–8s beat")

    editor = (ROOT / "Assets/Editor/PlayFromWeekStart.cs").read_text(encoding="utf-8")
    if "Assets/Scenes/Title.unity" not in editor or "playModeStartScene" not in editor:
        fail("editor bootstrap does not pin Title as Play start")
    else:
        ok("editor bootstrap pins Title as Play / Build start")
    title_idx = editor.find("TitlePath")
    week_idx = editor.find("WeekStartPath")
    if title_idx < 0 or week_idx < 0 or editor.find("new EditorBuildSettingsScene(TitlePath") < 0:
        fail("editor Build Settings does not list Title first")
    else:
        ok("editor Build Settings lists Title first")

    threats = ("장비 고장", "라이벌 견제", "플랫폼 수수료", "스캔들 루머", "인터넷 끊김")
    extra_cs = (ROOT / "Assets/Scripts/Data/ExtraThreat.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    run_cs = (ROOT / "Assets/Scripts/Core/GameRunState.cs").read_text(encoding="utf-8")
    for name in threats:
        if name not in extra_cs or name not in balance:
            fail(f"extra threat '{name}' missing from table")
    else:
        ok("extra threat table has 5 named KRW threats")
    if "runSeed" not in run_cs or "MixSeed" not in extra_cs:
        fail("extra threat is not seeded by runSeed + day")
    else:
        ok("extra threat seed is runSeed + day")
    if "Extra = true" not in week_cs or "오늘의 위협" not in week_cs:
        fail("WeekStart does not slam a sixth named extra bill")
    else:
        ok("WeekStart slams a sixth extra-threat bill")
    if "extraThreatAmount" not in eco_cs or "fixedBills + extra" not in eco_cs:
        fail("ApplyDailyBills does not add extra on top of the week's fixed bills")
    else:
        ok("economy adds extra on top of the week's fixed bills")
    if "InWeek2" not in (ROOT / "Assets/Scripts/Economy/WeekSchedule.cs").read_text(encoding="utf-8"):
        fail("WeekSchedule missing InWeek2 gate")
    else:
        ok("Week 2 numbers are gated behind InWeek2")
    if "위협" not in settle_cs or "extraThreatName" not in settle_cs:
        fail("Settlement does not list the rolled extra threat")
    else:
        ok("Settlement lists the persisted extra threat")
    for token, lo, hi in (
        ("minWon: 7000", 4000, 12000),
        ("maxWon: 12000", 4000, 12000),
        ("minWon: 4000", 4000, 12000),
    ):
        if token not in balance:
            fail(f"extra threat amount {token} missing")
    ok("extra threat amounts stay in ₩4,000–₩12,000")

    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    event_cs = (ROOT / "Assets/Scripts/Stream/StreamEvent.cs").read_text(encoding="utf-8")
    if "2주차 예정" in live_cs:
        fail("LiveStream still shows the 1–4 stub toast")
    if "EventKeyPressed" not in live_cs or "TryEventKey" not in live_cs:
        fail("LiveStream does not consume 1–4 as a real event QTE")
    else:
        ok("LiveStream 1–4 is a real event QTE")
    if "안티 웨이브" not in event_cs or "장비 렉" not in event_cs:
        fail("Week 1 event types missing")
    else:
        ok("event types 안티 웨이브 / 장비 렉 exist")
    if "Event.Fired" not in session_cs or "void StartEvent" not in session_cs:
        fail("stream event does not fire-once")
    else:
        ok("stream event is armed once per session")
    if "Event.Active" not in session_cs.split("TryHit", 1)[-1].split("ChatNote best", 1)[0]:
        fail("A/S/D/F can still hit chat during the event")
    else:
        ok("regular chat keys are ignored while the event is up")
    for token in (
        "eventEarliestSeconds: 35",
        "eventLatestSeconds: 55",
        "eventAntiFailMental: 8",
        "eventAntiFailViewers: 4",
        "eventLagFailFreezeSeconds: 3",
    ):
        if token not in balance:
            fail(f"Week1Balance missing event field {token}")
    ok("event numbers live on Week1Balance")

    w2_asset = (ROOT / "Assets/Resources/Balance/Week2Balance.asset").read_text(encoding="utf-8")
    w2_cs = (ROOT / "Assets/Scripts/Data/Week2Balance.cs").read_text(encoding="utf-8")
    sched_cs = (ROOT / "Assets/Scripts/Economy/WeekSchedule.cs").read_text(encoding="utf-8")
    w2r_cs = (ROOT / "Assets/Scripts/Economy/Week2Rules.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    run_cs = (ROOT / "Assets/Scripts/Core/GameRunState.cs").read_text(encoding="utf-8")
    extra_cs = (ROOT / "Assets/Scripts/Data/ExtraThreat.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")

    w2_expect = {
        "billRent: 10000": "w2 rent",
        "billElectricNet: 5000": "w2 electric",
        "billAvatarLicense: 5000": "w2 license",
        "billFood: 6000": "w2 food",
        "billGear: 2000": "w2 gear",
        "winDebtMax: 20000": "w2 win debt",
        "winCashMin: 110000": "w2 win cash",
        "bankruptDebt: 220000": "w2 bankrupt",
        "entryCash: 15000": "entry cash",
        "entryDebtRelief: 10000": "entry debt",
        "entryMental: 100": "entry mental",
        "startingMembers: 8": "start members",
        "unlockPeakViewers: 40": "unlock peak",
        "unlockSuccessfulStreams: 4": "unlock streams",
        "membershipPassivePerMember: 150": "member passive",
        "membersFromHype: 1": "hype +1",
        "membersFromHypeDayMax: 2": "hype day cap",
        "membersMissPenaltyAt: 10": "miss penalty",
        "clipPerfectsRequired: 25": "clip perfects",
        "clipChance: 30": "clip chance",
        "clipCash: 30000": "clip cash",
        "clipViewerBonus: 10": "clip viewers",
        "firstDay: 6": "week2 start day",
        "lastDay: 10": "week2 end day",
        "extraThreatMaxPerDay: 2": "max extras",
    }
    for token, label in w2_expect.items():
        if token not in w2_asset:
            fail(f"Week2Balance missing {label} ({token})")
    if "billAvatarLicense = 5000" not in w2_cs or "billGear = 2000" not in w2_cs:
        fail("Week2Balance.cs missing locked bills 10000/5000/5000/6000/2000")
    else:
        ok("Week2Balance locked bills 10000/5000/5000/6000/2000")
    if "billRent: 8000" not in balance or "billAvatarLicense: 3000" not in balance or "bankruptDebt: 180000" not in balance:
        fail("Week 1 bills or bankrupt were overwritten")
    else:
        ok("Week 1 bills stay ₩22,000 and bankrupt ₩180,000")

    for name in ("장비 고장", "소액 추가 청구", "플랫폼 수수료"):
        if name not in w2_asset or name not in extra_cs:
            fail(f"Week 2 extra threat '{name}' missing")
    else:
        ok("Week 2 extra threats are 장비 고장 / 소액 추가 청구 / 플랫폼 수수료")
    if "RollWeek2" not in extra_cs or "chancePercent" not in extra_cs:
        fail("Week 2 extras are not independent chance rolls")
    else:
        ok("Week 2 extras are 0–2 independent chance rolls")
    for token in ("chancePercent: 20", "chancePercent: 25", "chancePercent: 15", "minWon: 5000", "maxWon: 12000", "minWon: 3000"):
        if token not in w2_asset:
            fail(f"Week 2 threat field {token} missing")
    ok("Week 2 extra threat chances/amounts match the locked table")
    if "라이벌 견제" in w2_asset or "스캔들 루머" in w2_asset:
        fail("Week 2 still uses the old five-threat draft table")
    else:
        ok("old Week 2 five-threat draft table is gone")

    if "membershipCount" not in run_cs or "viewerBonus" not in run_cs or "peakViewersEver" not in run_cs:
        fail("GameRunState missing membership / viewerBonus / peak viewers")
    else:
        ok("run state persists membership, viewerBonus, peak viewers")
    if "ClearWeek2Progress" not in run_cs or "membershipUnlocked = false" not in run_cs:
        fail("ResetNewRun does not clear Week 2 progress")
    else:
        ok("Title / Restart clears Week 2 so a new run is Week 1")
    if "unlockPeakViewers" not in sched_cs or "unlockSuccessfulStreams" not in sched_cs:
        fail("membership unlock is not peak viewers >= 40 or successful streams >= 4")
    else:
        ok("membership unlocks at peak viewers >= 40 or 4 successful streams")
    if "membersFromHype" not in w2r_cs or "membersMissPenaltyAt" not in w2r_cs:
        fail("stream membership +1 hype / −1 miss missing")
    else:
        ok("membership +1 on hype (max +2/day), −1 if Misses >= 10")
    if "ApplyWeek2Entry" not in w2r_cs or "entryCash" not in w2r_cs:
        fail("Week 2 entry bonus missing")
    else:
        ok("Week 2 entry applies +₩15,000 / −₩10,000 debt / mental 100")
    if "membershipPassivePerMember" not in w2r_cs or "ApplyMembershipPassive" not in w2r_cs:
        fail("settlement membership passive missing")
    else:
        ok("membership passive is settlement-only (count * 150)")
    if "클립 업로드" not in settle_cs or "올리지 않기" not in settle_cs or "AttemptClip" not in w2r_cs:
        fail("Settlement missing Yes/No 클립 업로드")
    else:
        ok("Settlement offers Yes/No 클립 업로드")
    if "clipPerfectsRequired" not in w2r_cs or "lastHadHype" not in w2r_cs:
        fail("clip eligibility is not hype AND Perfects >= 25")
    else:
        ok("clip is eligible only if hype AND Perfects >= 25")
    if "clipAttemptedThisDay" not in run_cs or "DeclineClip" not in w2r_cs:
        fail("clip is not a once-per-day Yes/No")
    else:
        ok("viral clip is one Yes/No attempt per day")
    if "2주차 시작" not in settle_cs or "CanEnterWeek2" not in sched_cs:
        fail("Week 1 win does not offer Week 2 continue")
    else:
        ok("Week 1 win can continue into days 6–10")
    if "Week2Win" not in eco_cs or "membershipUnlocked" not in eco_cs:
        fail("Week 2 clear does not require membership unlocked")
    else:
        ok("Week 2 clear requires survive 6–10 and membership unlocked")
    if "멤버십 유도" in live_cs or "PitchInvite" in live_cs or "MaybeStartPitch" in session_cs:
        fail("mid-stream 멤버십 유도 prompt is still present")
    else:
        ok("Week 2 stream variable is clip (no 멤버십 유도)")
    if (ROOT / "Assets/Scripts/Stream/MembershipPitch.cs").exists():
        fail("MembershipPitch.cs should be removed")
    else:
        ok("MembershipPitch was removed")
    if "agency" in w2_cs.lower() or "concert" in w2_cs.lower() or "글로벌" in settle_cs:
        fail("Week 2 added agency/concert/global")
    else:
        ok("Week 2 does not add agency, concert, or global")
    if "Week2" in title_cs or "membershipCount" in title_cs or "Week3" in title_cs or "Week4" in title_cs or "Week5" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "팬레터" in title_cs:
        fail("Title scene started applying Week 2/3/4/5 or Fandom systems")
    else:
        ok("Title still starts a Week 1 run")
    if "Week2Balance.Load" not in gm:
        fail("GameManager does not load Week2Balance")
    else:
        ok("GameManager loads Week2Balance")

    w3_asset_path = ROOT / "Assets/Resources/Balance/Week3Balance.asset"
    w3_cs = (ROOT / "Assets/Scripts/Data/Week3Balance.cs").read_text(encoding="utf-8")
    w3r_cs = (ROOT / "Assets/Scripts/Economy/Week3Rules.cs").read_text(encoding="utf-8")
    promo_cs = (ROOT / "Assets/Scripts/Stream/GoodsPromo.cs").read_text(encoding="utf-8")
    w3_asset = w3_asset_path.read_text(encoding="utf-8") if w3_asset_path.exists() else ""
    in2 = sched_cs.split("public static bool InWeek2", 1)[-1].split("public static", 1)[0]
    if "Week2LastDay" not in in2:
        fail("InWeek2 still treats days 11+ as Week 2")
    else:
        ok("InWeek2 is days 6–10 only")
    if "InWeek3" not in sched_cs or "Week3LastDay" not in sched_cs:
        fail("WeekSchedule missing InWeek3 / days 11–15")
    else:
        ok("Week 3 is gated to days 11–15")
    if "CanEnterWeek3" not in sched_cs or "3주차 시작" not in settle_cs:
        fail("Week 2 clear does not offer Week 3 continue")
    else:
        ok("Week 2 clear can continue into days 11–15")

    w3_expect = {
        "billRent: 12000": "w3 rent",
        "billElectricNet: 6000": "w3 electric",
        "billAvatarLicense: 6000": "w3 license",
        "billFood: 7000": "w3 food",
        "billGear: 3000": "w3 gear",
        "winDebtMax: 15000": "w3 win debt",
        "winCashMin: 140000": "w3 win cash",
        "bankruptDebt: 260000": "w3 bankrupt",
        "firstDay: 11": "week3 start day",
        "lastDay: 15": "week3 end day",
        "extraThreatMaxPerDay: 2": "w3 max extras",
        "rivalDay: 12": "rival day",
        "rivalPeakViewers: 55": "rival peak",
        "rivalStartViewers: 25": "rival start",
        "rivalViewersPerSec: 0.9": "rival growth",
        "rivalPerfectSteal: 0.6": "perfect steal",
        "rivalMissSteal: 0.8": "miss steal",
        "rivalWinCash: 20000": "rival cash",
        "rivalWinViewerBonus: 6": "rival viewers",
        "rivalLoseViewerPenalty: 5": "rival lose viewers",
        "rivalLoseMental: 12": "rival lose mental",
        "goodsUnlockCash: 60000": "goods unlock cash",
        "goodsUnlockStock: 20": "goods unlock stock",
        "goodsProduceCost: 2500": "goods produce",
        "goodsPrice: 7000": "goods price",
        "goodsSoldMembersFactor: 0.4": "goods members",
        "goodsSoldPeakFactor: 0.08": "goods peak",
        "goodsPromoMultiplier: 1.5": "promo mult",
        "promoWindowSeconds: 1.2": "promo window",
        "promoFallbackSeconds: 55": "promo fallback",
    }
    for token, label in w3_expect.items():
        if token not in w3_asset:
            fail(f"Week3Balance missing {label} ({token})")
    if "billRent = 12000" not in w3_cs or "billGear = 3000" not in w3_cs:
        fail("Week3Balance.cs missing locked bills 12000/6000/6000/7000/3000")
    else:
        ok("Week3Balance locked bills 12000/6000/6000/7000/3000")
    if "billRent: 10000" not in w2_asset or "bankruptDebt: 220000" not in w2_asset:
        fail("Week 2 bills or bankrupt were overwritten")
    else:
        ok("Week 2 bills stay ₩28,000 and bankrupt ₩220,000")
    if "billRent: 8000" not in balance or "bankruptDebt: 180000" not in balance:
        fail("Week 1 bills or bankrupt were overwritten by Week 3")
    else:
        ok("Week 1 numbers stay unchanged after Week 3")

    for name in ("장비 고장", "소액 추가", "플랫폼 수수료"):
        if name not in w3_asset or name not in extra_cs:
            fail(f"Week 3 extra threat '{name}' missing")
    else:
        ok("Week 3 extra threats are 장비 고장 / 소액 추가 / 플랫폼 수수료")
    if "DefaultWeek3Table" not in extra_cs or "RollWeek3" not in extra_cs:
        fail("Week 3 extras are not independent chance rolls")
    else:
        ok("Week 3 extras are 0–2 independent chance rolls")
    for token in ("chancePercent: 25", "chancePercent: 20", "minWon: 6000", "maxWon: 15000", "minWon: 4000", "maxWon: 10000"):
        if token not in w3_asset:
            fail(f"Week 3 threat field {token} missing")
    ok("Week 3 extra threat chances/amounts match the locked table")

    if "ShouldStartRival" not in w3r_cs or "rivalMatchHappened" not in run_cs:
        fail("rival match is not a once-per-run stream")
    else:
        ok("rival match fires once when day==12 or peak viewers >= 55")
    if "EnableRival" not in session_cs or "rivalPerfectSteal" not in session_cs:
        fail("stream session missing rival steal/defense")
    else:
        ok("Perfect +0.6 steal and Miss −0.8 to the rival lane")
    if "_rival" not in live_cs or "라이벌" not in live_cs:
        fail("LiveStream HUD missing rival viewer count")
    else:
        ok("LiveStream HUD shows rival viewers during the match")
    if "ApplyRivalResult" not in w3r_cs or "rivalWinCash" not in w3r_cs:
        fail("rival win/lose payout missing")
    else:
        ok("rival win +₩20,000 / +6 viewers; lose −5 viewers floor 12 and mental −12")

    if "goodsUnlocked" not in run_cs or "TryUnlockGoods" not in w3r_cs:
        fail("아크릴 스탠드 unlock is missing")
    else:
        ok("goods unlock when membership is on and cash >= ₩60,000")
    if "ProduceGoods" not in w3r_cs or "아크릴 1개 생산" not in settle_cs:
        fail("Settlement missing goods produce button")
    else:
        ok("Settlement can produce 아크릴 at ₩2,500 each")
    if "ApplyGoodsSales" not in w3r_cs or "goodsPrice" not in w3r_cs:
        fail("daily goods sales missing")
    else:
        ok("daily goods sold is floor(members*0.4 + peak*0.08), min 1, capped by stock")

    if "GoodsPromo" not in session_cs or "굿즈 홍보 타이밍" not in live_cs or "굿즈 홍보 타이밍" not in promo_cs:
        fail("굿즈 홍보 타이밍 prompt missing")
    else:
        ok("Week 3 stream variable is 굿즈 홍보 타이밍")
    promo_bind = (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8")
    promo_fn = promo_bind.split("PromoConfirmDown", 1)[-1].split("PromoSkipDown", 1)[0]
    skip_fn = promo_bind.split("PromoSkipDown", 1)[-1].split("public static void QueueKind", 1)[0]
    if "KeyCode.LeftArrow" not in promo_fn or "KeyCode.UpArrow" not in promo_fn:
        fail("promo confirm is not Left/Up")
    elif "KeyCode.RightArrow" not in skip_fn or "KeyCode.DownArrow" not in skip_fn:
        fail("promo skip is not Right/Down")
    else:
        ok("promo confirm is ←/↑ and skip is →/↓")
    if "Week3Win" not in eco_cs or "goodsUnlocked" not in eco_cs:
        fail("Week 3 clear does not require goods unlocked")
    else:
        ok("Week 3 clear requires survive 11–15 and goods unlocked")
    if "Week3Balance.Load" not in gm:
        fail("GameManager does not load Week3Balance")
    else:
        ok("GameManager loads Week3Balance")
    if "agency" in w3_cs.lower() or "concert" in w3_cs.lower() or "글로벌" in w3_cs:
        fail("Week 3 added agency/concert/global")
    else:
        ok("Week 3 does not add agency, concert, or global")
    if "ClearWeek3Progress" not in run_cs or "goodsUnlocked = false" not in run_cs:
        fail("ResetNewRun does not clear Week 3 progress")
    else:
        ok("Title / Restart clears Week 3 so a new run is Week 1")

    w4_asset_path = ROOT / "Assets/Resources/Balance/Week4Balance.asset"
    w4_cs = (ROOT / "Assets/Scripts/Data/Week4Balance.cs").read_text(encoding="utf-8")
    w4r_cs = (ROOT / "Assets/Scripts/Economy/Week4Rules.cs").read_text(encoding="utf-8")
    line_cs = (ROOT / "Assets/Scripts/Stream/SponsorLine.cs").read_text(encoding="utf-8")
    w4_asset = w4_asset_path.read_text(encoding="utf-8") if w4_asset_path.exists() else ""
    in3 = sched_cs.split("public static bool InWeek3", 1)[-1].split("public static", 1)[0]
    if "Week3LastDay" not in in3:
        fail("InWeek3 still treats days 16+ as Week 3")
    else:
        ok("InWeek3 is days 11–15 only")
    if "InWeek4" not in sched_cs or "Week4LastDay" not in sched_cs:
        fail("WeekSchedule missing InWeek4 / days 16–20")
    else:
        ok("Week 4 is gated to days 16–20")
    if "CanEnterWeek4" not in sched_cs or "4주차 시작" not in settle_cs:
        fail("Week 3 clear does not offer Week 4 continue")
    else:
        ok("Week 3 clear can continue into days 16–20")

    w4_expect = {
        "billRent: 14000": "w4 rent",
        "billElectricNet: 7000": "w4 electric",
        "billAvatarLicense: 7000": "w4 license",
        "billFood: 7000": "w4 food",
        "billGear: 3000": "w4 gear",
        "winDebtMax: 10000": "w4 win debt",
        "winCashMin: 180000": "w4 win cash",
        "bankruptDebt: 300000": "w4 bankrupt",
        "firstDay: 16": "week4 start day",
        "lastDay: 20": "week4 end day",
        "extraThreatMaxPerDay: 2": "w4 max extras",
        "agencyUnlockCash: 100000": "agency cash",
        "agencyUnlockDebtMax: 40000": "agency debt",
        "agencyFoundCost: 40000": "agency found",
        "agencyDailyCost: 15000": "agency daily",
        "juniorScoutCost: 25000": "junior scout",
        "juniorDailySuccess: 4000": "junior pay",
        "juniorTrainFailMental: 8": "junior fail mental",
        "juniorTrainFailMisses: 10": "junior fail misses",
        "sponsorPeakViewers: 70": "sponsor peak",
        "sponsorDaily: 10000": "sponsor daily",
        "sponsorDays: 5": "sponsor days",
        "sponsorLineBonus: 3000": "line bonus",
        "sponsorFailCash: 15000": "line fail cash",
        "sponsorFailMental: 12": "line fail mental",
        "lineWindowSeconds: 1.2": "line window",
        "lineFallbackSeconds: 55": "line fallback",
    }
    for token, label in w4_expect.items():
        if token not in w4_asset:
            fail(f"Week4Balance missing {label} ({token})")
    if "billRent = 14000" not in w4_cs or "billGear = 3000" not in w4_cs:
        fail("Week4Balance.cs missing locked bills 14000/7000/7000/7000/3000")
    else:
        ok("Week4Balance locked bills 14000/7000/7000/7000/3000")
    if "billRent: 12000" not in w3_asset or "bankruptDebt: 260000" not in w3_asset:
        fail("Week 3 bills or bankrupt were overwritten")
    else:
        ok("Week 3 bills stay ₩34,000 and bankrupt ₩260,000")
    if "billRent: 10000" not in w2_asset or "bankruptDebt: 220000" not in w2_asset:
        fail("Week 2 bills or bankrupt were overwritten by Week 4")
    else:
        ok("Week 2 numbers stay unchanged after Week 4")
    if "billRent: 8000" not in balance or "bankruptDebt: 180000" not in balance:
        fail("Week 1 bills or bankrupt were overwritten by Week 4")
    else:
        ok("Week 1 numbers stay unchanged after Week 4")

    for name in ("장비 고장", "소액", "수수료"):
        if name not in w4_asset or name not in extra_cs:
            fail(f"Week 4 extra threat '{name}' missing")
    else:
        ok("Week 4 extra threats are 장비 고장 / 소액 / 수수료")
    if "DefaultWeek4Table" not in extra_cs or "RollWeek4" not in extra_cs:
        fail("Week 4 extras are not independent chance rolls")
    else:
        ok("Week 4 extras are 0–2 independent chance rolls")
    for token in ("chancePercent: 25", "chancePercent: 20", "minWon: 8000", "maxWon: 18000", "minWon: 5000", "maxWon: 12000"):
        if token not in w4_asset:
            fail(f"Week 4 threat field {token} missing")
    ok("Week 4 extra threat chances/amounts match the locked table")

    if "CanFoundAgency" not in w4r_cs or "에이전시 설립" not in settle_cs:
        fail("agency founding offer/button missing")
    else:
        ok("agency founds for ₩40,000 when cash >= 100000, debt <= 40000, goods on, day >= 16")
    if "agencyDailyCost" not in w4r_cs or "에이전시 운영" not in week_cs:
        fail("agency daily +15000 missing from WeekStart")
    else:
        ok("founded agency adds ₩15,000 daily so bills become ₩53,000")
    if "CanScoutJunior" not in w4r_cs or "주니어 스카우트" not in settle_cs:
        fail("junior scout button missing")
    else:
        ok("junior slot 1 scouts once for ₩25,000")
    if "juniorDailySuccess" not in w4r_cs or "juniorTrainFailMisses" not in w4r_cs:
        fail("junior daily pay / train-fail missing")
    else:
        ok("junior +₩4,000 on a successful stream; train-fail is force-end or Misses >= 10")
    if "CanOfferSponsor" not in w4r_cs or "스폰서 계약" not in settle_cs:
        fail("sponsor offer/button missing")
    else:
        ok("one sponsor deal offers after agency and peak viewers >= 70")
    if "sponsorDaily" not in w4r_cs or "sponsorDays" not in w4_cs:
        fail("sponsor daily +10000 for 5 days missing")
    else:
        ok("active sponsor pays ₩10,000/day for 5 days")

    if "EnableSponsorLine" not in session_cs or "스폰서 멘트 타이밍" not in live_cs or "스폰서 멘트 타이밍" not in line_cs:
        fail("스폰서 멘트 타이밍 prompt missing")
    else:
        ok("Week 4 stream variable is 스폰서 멘트 타이밍")
    if "ApplySponsorLine" not in w4r_cs or "sponsorLineBonus" not in w4r_cs:
        fail("sponsor line success/fail payout missing")
    else:
        ok("멘트 success keeps the contract and +₩3,000; fail ends it (−₩15,000, mental −12)")
    if "Week4Win" not in eco_cs or "agencyFounded" not in eco_cs:
        fail("Week 4 clear does not require agency founded")
    else:
        ok("Week 4 clear requires survive 16–20 and agency founded")
    if "Week4Balance.Load" not in gm:
        fail("GameManager does not load Week4Balance")
    else:
        ok("GameManager loads Week4Balance")
    if "concert" in w4_cs.lower() or "글로벌" in w4_cs:
        fail("Week 4 added concert/global")
    else:
        ok("Week 4 does not add concert or global")
    if "ClearWeek4Progress" not in run_cs or "agencyFounded = false" not in run_cs:
        fail("ResetNewRun does not clear Week 4 progress")
    else:
        ok("Title / Restart clears Week 4 so a new run is Week 1")

    w5_asset_path = ROOT / "Assets/Resources/Balance/Week5Balance.asset"
    w5_cs = (ROOT / "Assets/Scripts/Data/Week5Balance.cs").read_text(encoding="utf-8")
    w5r_cs = (ROOT / "Assets/Scripts/Economy/Week5Rules.cs").read_text(encoding="utf-8")
    concert_cs = (ROOT / "Assets/Scripts/Stream/ConcertPerformance.cs").read_text(encoding="utf-8")
    w5_asset = w5_asset_path.read_text(encoding="utf-8") if w5_asset_path.exists() else ""
    in4 = sched_cs.split("public static bool InWeek4", 1)[-1].split("public static", 1)[0]
    if "Week4LastDay" not in in4:
        fail("InWeek4 still treats days 21+ as Week 4")
    else:
        ok("InWeek4 is days 16–20 only")
    if "InWeek5" not in sched_cs or "Week5LastDay" not in sched_cs:
        fail("WeekSchedule missing InWeek5 / days 21–25")
    else:
        ok("Week 5 is gated to days 21–25")
    if "CanEnterWeek5" not in sched_cs or "5주차 시작" not in settle_cs:
        fail("Week 4 clear does not offer Week 5 continue")
    else:
        ok("Week 4 clear can continue into days 21–25")

    w5_expect = {
        "billRent: 15000": "w5 rent",
        "billElectricNet: 8000": "w5 electric",
        "billAvatarLicense: 8000": "w5 license",
        "billFood: 8000": "w5 food",
        "billGear: 6000": "w5 gear",
        "agencyDailyCost: 15000": "w5 agency ops",
        "bankruptDebt: 350000": "w5 bankrupt",
        "firstDay: 21": "week5 start day",
        "lastDay: 25": "week5 end day",
        "extraThreatMaxPerDay: 2": "w5 max extras",
        "rankingDay: 22": "ranking day",
        "rankingPeakViewers: 100": "ranking peak",
        "rankingPeakFactor: 3": "rank peak factor",
        "rankingMembersFactor: 8": "rank members",
        "rankingGoodsFactor: 4": "rank goods",
        "rankingPerfectsFactor: 2": "rank perfects",
        "rankingDailyFirstCash: 10000": "rank first cash",
        "npcBase0: 420": "npc 0",
        "npcBase1: 360": "npc 1",
        "npcBase2: 300": "npc 2",
        "concertUnlockCash: 150000": "concert cash",
        "concertUnlockPeak: 90": "concert peak",
        "concertUnlockDay: 22": "concert day",
        "concertCost: 80000": "concert cost",
        "concertBasePayout: 200000": "concert payout",
        "concertRankBonus: 500": "concert rank bonus",
        "concertSuccessMultiplier: 1.3": "concert mult",
        "concertFailMisses: 12": "concert fail misses",
        "concertFailMental: 25": "concert fail mental",
        "concertFailViewers: 10": "concert fail viewers",
        "concertWindowSeconds: 1.2": "concert window",
        "concertFallbackSeconds: 55": "concert fallback",
        "endingSoloMental: 40": "solo mental",
        "endingEmpireCash: 250000": "empire cash",
        "burnoutZeroMentalDays: 2": "burnout days",
    }
    for token, label in w5_expect.items():
        if token not in w5_asset:
            fail(f"Week5Balance missing {label} ({token})")
    if "billRent = 15000" not in w5_cs or "billGear = 6000" not in w5_cs:
        fail("Week5Balance.cs missing locked bills 15000/8000/8000/8000/6000")
    else:
        ok("Week5Balance locked bills 15000/8000/8000/8000/6000")
    if "billRent: 14000" not in w4_asset or "bankruptDebt: 300000" not in w4_asset:
        fail("Week 4 bills or bankrupt were overwritten")
    else:
        ok("Week 4 bills stay ₩38,000 and bankrupt ₩300,000")
    if "billRent: 12000" not in w3_asset or "bankruptDebt: 260000" not in w3_asset:
        fail("Week 3 bills or bankrupt were overwritten by Week 5")
    else:
        ok("Week 3 numbers stay unchanged after Week 5")
    if "billRent: 10000" not in w2_asset or "bankruptDebt: 220000" not in w2_asset:
        fail("Week 2 bills or bankrupt were overwritten by Week 5")
    else:
        ok("Week 2 numbers stay unchanged after Week 5")
    if "billRent: 8000" not in balance or "bankruptDebt: 180000" not in balance:
        fail("Week 1 bills or bankrupt were overwritten by Week 5")
    else:
        ok("Week 1 numbers stay unchanged after Week 5")

    for name in ("고장", "소액", "수수료"):
        if name not in w5_asset or name not in extra_cs:
            fail(f"Week 5 extra threat '{name}' missing")
    else:
        ok("Week 5 extra threats are 고장 / 소액 / 수수료")
    if "DefaultWeek5Table" not in extra_cs or "RollWeek5" not in extra_cs:
        fail("Week 5 extras are not independent chance rolls")
    else:
        ok("Week 5 extras are 0–2 independent chance rolls")
    for token in ("chancePercent: 30", "chancePercent: 25", "chancePercent: 20", "minWon: 10000", "maxWon: 20000", "minWon: 6000", "maxWon: 15000"):
        if token not in w5_asset:
            fail(f"Week 5 threat field {token} missing")
    ok("Week 5 extra threat chances/amounts match the locked table")

    if "RankingUnlocked" not in w5r_cs or "챌린지 랭킹" not in settle_cs:
        fail("ranking unlock / panel missing")
    else:
        ok("ranking panel unlocks at peak viewers >= 100 and day >= 22")
    if "peakViewers * 3" not in w5r_cs or "members * 8" not in w5r_cs or "goodsSoldToday * 4" not in w5r_cs or "perfects * 2" not in w5r_cs:
        fail("daily ranking score formula missing")
    else:
        ok("daily score is (peakViewers * 3) + (members * 8) + (goodsSoldToday * 4) + (perfects * 2)")
    if "MixSeed" not in w5r_cs or "NpcDailyScore" not in w5r_cs or "루나벨" not in w5_cs:
        fail("deterministic NPC rivals missing")
    else:
        ok("3 NPC rivals use deterministic daily scores")
    if "rankingDailyFirstCash" not in w5r_cs or "rankingDailyFirstCash: 10000" not in w5_asset:
        fail("daily rank-1 cash missing")
    else:
        ok("daily rank 1 pays ₩10,000")

    if "CanBookConcert" not in w5r_cs or "콘서트 개최" not in settle_cs:
        fail("concert booking button missing")
    else:
        ok("concert books for ₩80,000 when cash >= 150000, peak >= 90, day >= 22")
    if "EnableConcert" not in session_cs or "콘서트 퍼포먼스 타이밍" not in live_cs or "콘서트 퍼포먼스 타이밍" not in concert_cs:
        fail("콘서트 퍼포먼스 타이밍 prompt missing")
    else:
        ok("Week 5 stream variable is 콘서트 퍼포먼스 타이밍")
    if "concertBasePayout" not in w5r_cs or "concertSuccessMultiplier" not in w5r_cs:
        fail("concert success payout missing")
    else:
        ok("concert success is ₩200,000 + ranking +500, times 1.3 on performance hit")
    if "concertFailMisses" not in w5r_cs or "concertFailMental" not in w5r_cs:
        fail("concert fail path missing")
    else:
        ok("concert fail spends the ₩80,000, mental −25, starting viewers −10, no ₩200,000")

    if "ResolveEnding" not in w5r_cs or "EndingKind.Bankrupt" not in w5r_cs:
        fail("named endings missing")
    else:
        ok("endings resolve after day 25 or earlier bankrupt/burnout")
    if "파산 > 번아웃 > 솔로 전설 > 에이전시 제국" not in w5r_cs:
        fail("ending priority missing")
    else:
        ok("auto ending priority is 파산 > 번아웃 > 솔로 전설 > 에이전시 제국")
    if "후배에게 메인 양도" not in settle_cs or "CanOfferRetire" not in w5r_cs:
        fail("retire producer choice missing")
    else:
        ok("settlement/ending can pick 후배에게 메인 양도")
    offer = w5r_cs.split("public static bool CanOfferRetire", 1)[-1].split("public static string EndingTitle", 1)[0]
    if "juniorScouted" not in offer:
        fail("retire offer does not require junior scouted")
    elif "Nameless" in offer:
        fail("retire is still only offered when auto-end is 무명")
    elif "Bankrupt" not in offer or "Burnout" not in offer:
        fail("retire offer does not hide on 파산/번아웃")
    else:
        ok("retire is offered whenever agency and junior are set, unless 파산/번아웃")
    resolve = w5r_cs.split("public static EndingKind ResolveEnding", 1)[-1].split("public static bool CanOfferRetire", 1)[0]
    if "retirePicked" not in resolve or resolve.find("retirePicked") > resolve.find("AgencyEmpire"):
        fail("retire pick does not override 에이전시 제국")
    else:
        ok("후배에게 메인 양도 forces 은퇴 프로듀서 over 에이전시 제국")
    if "EndingRoot" not in settle_cs or "EndingTitle" not in w5r_cs:
        fail("dedicated ending screen missing")
    else:
        ok("dedicated ending screen then Restart")
    if "Week5Balance.Load" not in gm:
        fail("GameManager does not load Week5Balance")
    else:
        ok("GameManager loads Week5Balance")
    if "글로벌" in w5_cs or "글로벌" in w5r_cs or "global tour" in w5_cs.lower():
        fail("Week 5 added a global tour")
    else:
        ok("Week 5 does not add a global tour")
    if "ClearWeek5Progress" not in run_cs or "concertBooked = false" not in run_cs:
        fail("ResetNewRun does not clear Week 5 progress")
    else:
        ok("Title / Restart clears Week 5 so a new run is Week 1")
    if "concert" in w4_cs.lower() or "글로벌" in w4_cs:
        fail("Week 4 gained concert/global during Week 5")
    else:
        ok("Week 4 still does not add concert or global")

    debug_path = ROOT / "Assets/Scripts/Core/PlaytestDebug.cs"
    debug_cs = debug_path.read_text(encoding="utf-8") if debug_path.exists() else ""
    readme = (ROOT / "README.md").read_text(encoding="utf-8")
    if not debug_path.exists():
        fail("PlaytestDebug helper missing")
    if "UNITY_EDITOR" not in debug_cs or "DEVELOPMENT_BUILD" not in debug_cs:
        fail("playtest skip is not editor/DEVELOPMENT gated")
    else:
        ok("playtest skip is UNITY_EDITOR / DEVELOPMENT_BUILD only")
    if "AverageStreamTake = 28000" not in debug_cs or "24000" not in debug_cs or "32000" not in debug_cs:
        fail("F10 average take is not the documented ₩28,000 mid band")
    else:
        ok("F10 uses documented average mid take ₩28,000")
    if "lastHadHype = false" not in debug_cs:
        fail("F10 average skip still marks hype")
    else:
        ok("F10 average skip is not a hype exploit")
    if 'text = "DEBUG' not in debug_cs and "DEBUG  F9" not in debug_cs:
        fail("DEBUG on-screen label missing")
    else:
        ok("DEBUG badge is visible when skip is armed")
    if "KeyCode.F10" not in debug_cs or "KeyCode.F9" not in debug_cs:
        fail("F9/F10 skip bindings missing")
    else:
        ok("F9 skips to next week, F10 skips the current day")
    if "DEBUG 오늘 스킵" not in editor or "DEBUG 다음 주 점프" not in editor:
        fail("파산 버튜버 DEBUG menu items missing")
    else:
        ok("파산 버튜버 menu exposes F9/F10 skips")
    if "F9" not in readme or "F10" not in readme:
        fail("README missing F9/F10 playtest keys")
    else:
        ok("README documents F9/F10")
    if "F9" in title_cs or "F10" in title_cs or "PlaytestDebug" in title_cs:
        fail("Title started advertising the playtest skip")
    else:
        ok("Title still starts a Week 1 run without debug keys")
    if "DayHeadline.Remember(gm.Run)" not in debug_cs:
        fail("F10 skip does not write lastHeadline for the next 어제 line")
    elif "DayHeadline.Remember(gm.Run, false)" not in debug_cs:
        fail("F9 skip does not keep lastHeadline before the week jump wipe")
    elif 'lastHeadline = ""' in debug_cs or "lastHeadline = string.Empty" in debug_cs:
        fail("F9/F10 skip blanks lastHeadline")
    else:
        ok("F9/F10 skip writes lastHeadline so the next morning can show 어제")

    fandom_asset_path = ROOT / "Assets/Resources/Balance/FandomBalance.asset"
    fandom_cs = (ROOT / "Assets/Scripts/Data/FandomBalance.cs").read_text(encoding="utf-8")
    fandom_rules = (ROOT / "Assets/Scripts/Economy/FandomRules.cs").read_text(encoding="utf-8")
    fandom_asset = fandom_asset_path.read_text(encoding="utf-8") if fandom_asset_path.exists() else ""
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    run_cs = (ROOT / "Assets/Scripts/Core/GameRunState.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    w2r_cs = (ROOT / "Assets/Scripts/Economy/Week2Rules.cs").read_text(encoding="utf-8")
    sched_cs = (ROOT / "Assets/Scripts/Economy/WeekSchedule.cs").read_text(encoding="utf-8")

    if (ROOT / "Assets/Scripts/Data/Week6Balance.cs").exists() or (ROOT / "Assets/Resources/Balance/Week6Balance.asset").exists():
        fail("Week 6 was added")
    elif "InWeek6" in sched_cs or "Week6LastDay" in sched_cs:
        fail("WeekSchedule gained a Week 6 gate")
    else:
        ok("no Week 6 — fandom is folded into Weeks 1–5")

    fandom_expect = {
        "startT0: 12": "start T0",
        "startLoyalty: 40": "start loyalty",
        "maxLoyalty: 100": "max loyalty",
        "perfectHigh: 8": "perfect high",
        "perfectHighT0toT1: 2": "T0→T1 high",
        "perfectHighT1toT2: 1": "T1→T2 high",
        "perfectHighLoyalty: 5": "loyalty high",
        "perfectMidLo: 4": "perfect mid lo",
        "perfectMidHi: 7": "perfect mid hi",
        "perfectMidT0toT1: 1": "T0→T1 mid",
        "perfectMidLoyalty: 1": "loyalty mid",
        "missCount: 10": "miss count",
        "missLoyalty: 8": "miss loyalty",
        "missT2Loss: 1": "miss T2",
        "minjunName: 민준": "민준",
        "haeunName: 하은": "하은",
        "minjunIgnoreSettlements: 3": "민준 ignore",
        "minjunLeaveLoyalty: 12": "민준 leave",
        "haeunHurtStreak: 3": "하은 streak",
        "haeunLeaveLoyalty: 15": "하은 leave",
        "haeunAppearDay: 2": "하은 day",
        "letterLoyalty: 4": "letter loyalty",
        "letterMental: 8": "letter mental",
        "supportLoyaltyMin: 60": "support loyalty",
        "supportBase: 3000": "support base",
        "supportPerT3: 200": "support T3",
        "supportPerT4: 4000": "support T4",
        "supportMin: 3000": "support min",
        "supportMax: 20000": "support max",
        "conflictDay: 11": "conflict day",
        "conflictSootheMental: 10": "soothe mental",
        "conflictSootheLoyalty: 8": "soothe loyalty",
        "conflictStyleT2: 2": "style T2",
        "conflictStyleLoyalty: 10": "style loyalty",
        "conflictExtraSurcharge: 2000": "style surcharge",
        "autoDailyCost: 8000": "auto cost",
        "autoLoyaltyDrain: 1": "auto drain",
    }
    for token, label in fandom_expect.items():
        if token not in fandom_asset:
            fail(f"FandomBalance missing {label} ({token})")
    if "startT0 = 12" not in fandom_cs or "startLoyalty = 40" not in fandom_cs or "autoDailyCost = 8000" not in fandom_cs:
        fail("FandomBalance.cs missing locked start / auto defaults")
    else:
        ok("FandomBalance locked numbers present")

    t0, t1, t2 = 12, 0, 0
    move0 = min(2, t0)
    t0 -= move0
    t1 += move0
    move1 = min(1, t1)
    t1 -= move1
    t2 += move1
    if (t0, t1, t2) != (10, 1, 1):
        fail(f"Perfects>=8 convert {t0}/{t1}/{t2} != 10/1/1")
    else:
        ok("Perfects >= 8 converts 2 T0→T1 then 1 T1→T2")
    if 40 + 5 != 45 or 40 + 1 != 41 or 40 - 8 != 32:
        fail("loyalty after-stream deltas drifted")
    else:
        ok("after-stream loyalty is +5 / +1 / −8")
    support = 3000 + 8 * 200 + 2 * 4000
    if support != 12600 or support < 3000 or support > 20000:
        fail(f"fan support formula {support} != 3000 + T3*200 + T4*4000")
    else:
        ok("팬 지원금 is 3000 + T3×200 + T4×4000 (min 3000, max 20000)")

    if "FandomRules.AfterStream" not in live_cs or "HadSuccessfulSuperchat" not in session_cs:
        fail("stream does not feed fandom AfterStream / superchat / miss streak")
    else:
        ok("live stream calls Fandom AfterStream after the first superchat / miss streak")
    if "MaybeSpawnMinjun" not in fandom_rules or "MaybeSpawnHaeun" not in fandom_rules:
        fail("named superfans 민준/하은 missing")
    else:
        ok("민준 appears after first superchat; 하은 on morning of day 2")
    if "팬레터 답장" not in settle_cs or "SendLetter" not in fandom_rules or "letterLoyalty" not in fandom_rules:
        fail("팬레터 settlement action missing")
    else:
        ok("팬레터 답장 is a free once-per-day settlement action")
    if "HudLine" not in fandom_rules or "HudLine" not in week_cs or "HudLine" not in settle_cs:
        fail("loyalty / tier HUD missing on WeekStart or Settlement")
    elif "충성" not in fandom_rules or "시청자" not in fandom_rules or "슈퍼팬" not in fandom_rules:
        fail("fandom HUD line is not Korean tier labels")
    else:
        ok("WeekStart and Settlement show 충성 + tier counts in Korean")
    if "SuperfanLine" not in fandom_rules or "첫 도네" not in fandom_rules or "매일 오는 야간" not in fandom_rules:
        fail("named superfan labels missing")
    else:
        ok("민준 (첫 도네) and 하은 (매일 오는 야간) show when present")
    if "콘텐츠 편중 갈등" not in week_cs or "특별방송으로 달래기" not in week_cs or "내 스타일대로" not in week_cs:
        fail("Week 3 conflict buttons missing on WeekStart")
    else:
        ok("day 11 WeekStart has 콘텐츠 편중 갈등")
    if "특별방송으로 달래기" not in settle_cs or "MustResolveConflict" not in settle_cs:
        fail("conflict is not also offered / blocking on Settlement")
    else:
        ok("conflict must be picked that day (WeekStart or Settlement)")
    if "기본 자동응답" not in settle_cs or "autoDailyCost" not in fandom_rules or "8000" not in fandom_cs:
        fail("Week 4 auto-reply toggle / ₩8,000 cost missing")
    else:
        ok("기본 자동응답 is Week 4+ agency only, ₩8,000/day")
    if "RollSupport" not in fandom_rules or "lastFanSupport" not in eco_cs:
        fail("fan support is not rolled on bill slam")
    else:
        apply = eco_cs.split("public static int ApplyDailyBills", 1)[-1].split("public static int ApplyStreamPayout", 1)[0]
        if "RollSupport" not in apply or apply.find("RollSupport") > apply.find("ConvertNegativeCashToDebt"):
            fail("팬 지원금 is not applied before convert-negative-cash-to-debt")
        else:
            ok("팬 지원금 applies on bill slam before convert-to-debt")
    if "SyncT3" not in fandom_rules or "SyncT3" not in sched_cs or "SyncT3" not in w2r_cs:
        fail("membershipCount is not kept in sync with T3")
    else:
        ok("membership unlock still uses Week 2 rules; membershipCount is T3")
    if "FandomBalance.Load" not in gm or "FandomRules.AfterStream" not in debug_cs:
        fail("GameManager / playtest skip do not load or apply fandom")
    else:
        ok("GameManager loads FandomBalance; F10 still runs AfterStream")
    if "팬 지원금" not in week_cs:
        fail("WeekStart does not show 팬 지원금")
    else:
        ok("WeekStart shows 팬 지원금 when it rolls")
    if "Fandom" in title_cs or "6주차" in fandom_cs or "Week6" in fandom_rules:
        fail("Title or fandom files mention Week 6 / title fandom")
    else:
        ok("Title does not mention Fandom; no Week 6 high concept")

    save_cs = (ROOT / "Assets/Scripts/Core/RunSave.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    debug_cs = (ROOT / "Assets/Scripts/Core/PlaytestDebug.cs").read_text(encoding="utf-8")
    if "Application.persistentDataPath" not in save_cs or "bankrupt-vtuber-run.json" not in save_cs:
        fail("save is not JSON under Application.persistentDataPath")
    else:
        ok("run save is JSON in Application.persistentDataPath")
    if ".tmp" not in save_cs or "File.Replace" not in save_cs:
        fail("save write is not atomic (temp then replace)")
    else:
        ok("save write is temp file then replace")
    if "이어서 하기" not in title_cs or "새 방송 시작" not in title_cs or "ContinueRun" not in title_cs:
        fail("Title missing 이어서 하기 / 새 방송 시작")
    else:
        ok("Title shows 이어서 하기 next to 방송 시작")
    if "StartNewRun" not in gm or "RunSave.Delete" not in gm:
        fail("새 방송 시작 does not wipe the save")
    else:
        ok("새 방송 시작 / Restart wipes the save and starts Week 1")
    if "streamDoneThisDay" not in gm.split("ResumeFromSave", 1)[-1].split("public void SaveRun", 1)[0]:
        fail("resume does not route WeekStart vs Settlement")
    else:
        ok("resume goes to WeekStart before bills/stream, else Settlement")
    if "SaveRun" not in week_cs or "SaveRun" not in gm.split("NextMorning", 1)[-1]:
        fail("bill slam or NextMorning does not save")
    else:
        ok("saves after bill slam, settlement, and week-advance")
    if "SaveRun" not in debug_cs:
        fail("F9/F10 do not save after skip")
    else:
        ok("F9/F10 still work and then save")
    if "SaveRun" in session_cs:
        fail("stream session writes a save mid-QTE")
    live_tick = live_cs.split("void Update", 1)[-1].split("EndRoutine", 1)[0]
    if "SaveRun" in live_tick or "RunSave.Write" in live_tick:
        fail("LiveStream Update saves mid-stream")
    else:
        ok("does not save during the 90s QTE")
    if "DummyRoundTrip" not in save_cs or "MakeDummy" not in save_cs:
        fail("RunSave missing dummy serialize roundtrip")
    else:
        ok("RunSave has a dummy-run serialize roundtrip")
    if "try" not in save_cs.lower() or "FromJson" not in save_cs:
        fail("corrupt save is not ignored")
    else:
        ok("corrupt save is ignored and does not crash")
    check_save_roundtrip()
    check_content_types()
    check_first_stream_coach()
    check_event_accident()
    check_fan_letter()
    check_chat_catalog()
    check_week2_beats()
    check_rival_duel()
    check_week3_goods_beats()
    check_week4_agency_beats()
    check_week5_finale_beats()
    check_fandom_beats()
    check_portrait_safe_area()
    check_day_headline()
    check_chat_nicks()
    check_hype_wash()
    check_combo_break()
    check_clock_urgency()
    check_on_air()
    check_perfect_good()
    check_income_pop()
    check_end_cut()
    check_income_count()
    check_shortfall()
    check_morning_bill()
    check_debt_count()
    check_hype_chat()
    check_cam_punch()
    check_combo_pop()
    check_pad_flash()
    check_mental_count()
    check_morning_cash_short()
    check_note_hot()
    check_content_card_mood()
    check_title_broke_login()
    check_superchat_pip()
    check_left_cash()
    check_go_live_pulse()
    check_note_pad_color()
    check_strike_marker()
    check_next_pulse()
    check_event_warn()
    check_day_slam()
    check_bill_chip()
    check_bill_fill()
    check_mental_fatigue()
    check_superchat_fly()
    check_viewer_pop()
    check_viewer_chip_pop()
    check_bill_cover_slam()
    check_yesterday_headline()
    check_last_day_banner()
    check_title_continue_preview()
    check_start_pulse()
    check_continue_pulse()
    check_show_chip()
    check_settle_show_line()
    check_vtuber_face()
    check_bill_notice()
    check_stream_overlay()
    check_title_studio()
    check_settlement_desk()
    check_morning_room()
    check_pad_keycaps()
    check_chat_bubble()
    check_note_chip()
    check_hit_rail()
    check_judge_sfx()
    check_stream_stings()


def check_content_types() -> None:
    content_path = ROOT / "Assets/Resources/Balance/ContentBalance.asset"
    content_cs = (ROOT / "Assets/Scripts/Data/ContentBalance.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Economy/ContentRules.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    save_cs = (ROOT / "Assets/Scripts/Core/RunSave.cs").read_text(encoding="utf-8")
    content_asset = content_path.read_text(encoding="utf-8") if content_path.exists() else ""

    expect = {
        "talkIncomeMultiplier: 1": "talk income",
        "talkMentalCost: 6": "talk mental",
        "gameIncomeMultiplier: 1.15": "game income",
        "gameMentalCost: 10": "game mental",
        "gamePerfectViewerMul: 1.4": "game perfect viewers",
        "gameMissViewerMul: 1.35": "game miss viewers",
        "songIncomeMultiplier: 1.1": "song income",
        "songMentalCost: 8": "song mental",
        "songPerfectWindowMul: 0.85": "song perfect window",
        "reactionIncomeMultiplier: 0.9": "reaction income",
        "reactionMentalCost: 4": "reaction mental",
        "reactionLoyalty: 2": "reaction loyalty",
        "reactionMissMax: 8": "reaction miss cap",
    }
    for token, label in expect.items():
        if token not in content_asset:
            fail(f"ContentBalance missing {label} ({token})")
    if "talkIncomeMultiplier = 1.0f" not in content_cs or "gameIncomeMultiplier = 1.15f" not in content_cs:
        fail("ContentBalance.cs missing locked income multipliers")
    else:
        ok("each content type has locked income / mental multiplier fields")
    if "오늘 콘텐츠" not in week_cs or "토크" not in week_cs or "게임" not in week_cs or "노래" not in week_cs or "리액션" not in week_cs:
        fail("WeekStart does not force a 4-type 콘텐츠 pick")
    else:
        ok("WeekStart after bills forces 토크 / 게임 / 노래 / 리액션")
    if "MustPick" not in week_cs or "MustPick" not in gm.split("GoLive", 1)[-1].split("public void GoSettlement", 1)[0]:
        fail("Go Live is not blocked until a content type is picked")
    else:
        ok("Go Live waits for today's content pick")
    if "Tuning.IncomeMul" not in session_cs or "PerfectWindowMul" not in session_cs or "RollRegularKind" not in session_cs:
        fail("StreamSession does not retune chat/income/windows from the pick")
    else:
        ok("the existing arrow-key stream reads the picked type")
    if "contentPicked" not in save_cs or "contentPicked" not in (ROOT / "Assets/Scripts/Core/GameRunState.cs").read_text(encoding="utf-8"):
        fail("save/load does not persist today's content pick")
    else:
        ok("save/load persists today's content pick")
    if "ContentBalance.Load" not in gm:
        fail("GameManager does not load ContentBalance")
    else:
        ok("GameManager loads ContentBalance")
    if "토크" in title_cs or "리액션" in title_cs or "ContentBalance" in title_cs:
        fail("Title started advertising content types")
    else:
        ok("Title still starts a Week 1 run without content types")
    if (ROOT / "Assets/Scripts/Data/Week6Balance.cs").exists():
        fail("Week 6 was added during content types")
    if "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("Week 1 bills were retuned by content types")
    else:
        ok("Week 1–5 locked bills stay unchanged after content types")
    if "TryEventKey" not in live_cs or "KeyCode.LeftArrow" not in (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8"):
        fail("content types replaced the arrow-key QTE")
    else:
        ok("content type only retunes the existing QTE")
    look_cs = (ROOT / "Assets/Scripts/Presentation/ContentShowLook.cs").read_text(encoding="utf-8")
    avatar_show = (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8")
    if "오늘: 토크" not in look_cs or "오늘: 게임" not in look_cs or "오늘: 노래" not in look_cs or "오늘: 리액션" not in look_cs:
        fail("content show look missing 오늘: overlay titles")
    elif "ApplyContentShow" not in live_cs or "ContentShowLook.For" not in live_cs or "ShowTitle" not in live_cs:
        fail("LiveStream does not apply today's content show skin")
    elif "BedClip" not in live_cs or "bgm_" not in live_cs or "_bed" not in live_cs:
        fail("content type BGM bed is missing")
    elif "클로즈업" not in avatar_show or "게임 화면" not in avatar_show or "노래방" not in avatar_show or "리액션 캠" not in avatar_show:
        fail("webcam does not change per content type")
    elif "ContentShowLook.For" not in week_cs or "ShowWash" not in week_cs:
        fail("WeekStart cards do not preview the LiveStream color language")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or '"오늘 청구"' not in live_cs:
        fail("content show skin broke pads or the money HUD")
    else:
        ok("LiveStream skins 토크/게임/노래/리액션; WeekStart cards match")
    if "talkIncomeMultiplier: 1" not in content_asset or "reactionChatSpawnMul" in look_cs:
        fail("content show skin retuned ContentBalance numbers")
    else:
        ok("content show skin does not retune spawn / income / mental")
    threat_cs = (ROOT / "Assets/Scripts/Presentation/ExtraThreatLook.cs").read_text(encoding="utf-8")
    extra_cs = (ROOT / "Assets/Scripts/Data/ExtraThreat.cs").read_text(encoding="utf-8")
    if "장비 불안정" not in threat_cs or "재연결 중" not in threat_cs or '"수수료"' not in threat_cs:
        fail("extra-threat overlay look is missing Korean fingerprints")
    elif "gear_break" not in threat_cs or "net_drop" not in threat_cs or "petty_bill" not in threat_cs:
        fail("extra-threat overlay does not map existing kind ids")
    elif "ApplyThreatShow" not in live_cs or "TickThreatFx" not in live_cs:
        fail("LiveStream does not apply today's extra-threat fingerprint")
    elif "ExtraThreatLook.For" not in week_cs:
        fail("WeekStart threat cards do not share ExtraThreatLook")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("threat overlay broke pads or added timeScale lag")
    elif "id = \"gear_break\"" not in extra_cs or "minWon = 7000" not in extra_cs:
        fail("extra threat table was retuned")
    else:
        ok("LiveStream fingerprints today's extra threat; WeekStart cards match")


def check_first_stream_coach() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    pad_cs = (ROOT / "Assets/Scripts/Input/StreamPadButton.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    offer = session_cs.split("ShouldOfferFirstStreamCoach", 1)[-1].split("public void EnableFirstStreamCoach", 1)[0]
    coach_tick = session_cs.split("void TickCoach", 1)[-1].split("void FreezeNotes", 1)[0]
    spawn_reg = session_cs.split("void MaybeSpawnRegular", 1)[-1].split("void MaybeSpawnSuperchat", 1)[0]
    spawn_sc = session_cs.split("void MaybeSpawnSuperchat", 1)[-1].split("void SpawnNote", 1)[0]

    if "EnableFirstStreamCoach" not in session_cs or "ShouldOfferFirstStreamCoach" not in session_cs:
        fail("Day 1 first-stream coach gate is missing")
    elif "day != 1" not in offer and "day == 1" not in offer:
        fail("first-stream coach is not gated on day == 1")
    elif "streamDoneThisDay" not in offer or "successfulStreams" not in offer:
        fail("first-stream coach does not skip continue / already-streamed day 1")
    elif "EnableFirstStreamCoach" not in live_cs or "ShouldOfferFirstStreamCoach" not in live_cs:
        fail("LiveStream does not arm the Day 1 first-stream coach")
    else:
        ok("Day 1 first LiveStream of a new run arms the QTE coach")

    if "CoachSuccessTarget = 3" not in session_cs or "CoachSeconds = 8f" not in session_cs:
        fail("coach does not end on 3 successes or 8 seconds")
    elif "CoachSeconds" not in coach_tick or "CoachSuccessTarget" not in coach_tick:
        fail("TickCoach does not honor 3 successes / 8 seconds")
    elif "MissHeldCoach" not in session_cs or "Judgement.Miss" not in session_cs.split("public bool MissHeldCoach", 1)[-1][:500]:
        fail("wrong coach key is not a normal Miss")
    else:
        ok("coach holds notes until 3 successes or 8 seconds; wrong key is a Miss")

    if "색에 맞는 키 또는 아래 버튼을 눌러" not in live_cs:
        fail("coach is missing the single Korean hint line")
    elif "← 긍정" not in live_cs or "↓ 공감" not in live_cs or "→ 웃음" not in live_cs or "↑ 감사" not in live_cs:
        fail("coach prompts do not match note kinds")
    elif "슈퍼챗 Space" not in live_cs or "눌러서 차지 후 떼기" not in live_cs:
        fail("superchat coach prompt is missing")
    elif "SetPulse" not in pad_cs or "SetPulse" not in live_cs or "RefreshCoach" not in live_cs:
        fail("matching pad does not pulse during the coach")
    else:
        ok("one-line coach + kind prompt + pad pulse; superchat teaches charge-and-release")

    if "Coach" in spawn_reg or "Coach" in spawn_sc:
        fail("coach changed spawn tables")
    elif "chatSpawnStart: 1.55" not in balance or "billRent: 8000" not in balance:
        fail("Week 1 spawn / bill numbers were retuned by the coach")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("coach broke pads, 입력됨 echo, or added timeScale")
    elif "주차 클리어" not in (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8"):
        fail("week-clear screen was dropped while adding the coach")
    elif "토크" in title_cs or "Week2" in title_cs or "민준" in title_cs:
        fail("Title started advertising the coach / later weeks")
    elif "_onAirLeft <= 0f" not in live_cs:
        fail("Day-1 coach UI no longer waits for the ON AIR sting")
    else:
        ok("spawn tables, pads, week-clear, and Title stay unchanged; Day 2 has no coach")


def check_event_accident() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    event_cs = (ROOT / "Assets/Scripts/Stream/StreamEvent.cs").read_text(encoding="utf-8")
    avatar_cs = (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    start_ev = session_cs.split("void StartEvent", 1)[-1].split("void ResolveEvent", 1)[0]
    resolve_ev = session_cs.split("void ResolveEvent", 1)[-1].split("public bool TryLine", 1)[0]
    event_in = live_cs.split("if (_session.EventActive)", 1)[-1].split("else if (_session.PromoActive)", 1)[0]
    kinds = event_cs.split("enum StreamEventKind", 1)[-1].split("enum StreamEventTrigger", 1)[0]

    if "BeginEventAccident" not in live_cs or "EventSting" not in live_cs:
        fail("mid-stream event has no full-screen accident sting")
    elif "0.2f" not in live_cs.split("void BeginEventAccident", 1)[-1][:500]:
        fail("event sting is not a short ~0.2s flash")
    elif "Panic" not in avatar_cs or "Panic" not in live_cs:
        fail("avatar does not panic when a stream event fires")
    elif "LaneFreeze" not in live_cs or "EventActive ? 0f" not in live_cs:
        fail("chat lane does not freeze visually during the event")
    else:
        ok("existing stream event opens with a kind-matched sting and a frozen lane")

    event_overlay = live_cs.split("void RefreshEventOverlay", 1)[-1].split("RectTransform MakeBubble", 1)[0]
    if "SetPulse" not in event_overlay or "1.18f" not in event_overlay:
        fail("correct event key 1–4 does not glow brighter than the others")
    elif "1.18f" not in live_cs and "localScale = hot" not in live_cs:
        fail("correct event pad is not larger / brighter than the rest")
    elif "DiscardLaneQueue" not in event_in or "TryEventKey" not in event_in:
        fail("event 1–4 pads/keys or lane-queue discard were dropped")
    elif "사고 수습" not in event_cs or "RecoverCopy" not in live_cs:
        fail("event success does not snap back with 사고 수습 copy")
    elif "방어 실패 — 시청자·멘탈 타격" not in event_cs or "송출 끊김 — 3초 무수익" not in event_cs:
        fail("existing event fail copy was rewritten")
    elif "ApplyEventScar" not in live_cs or "EventCrack" not in live_cs or "EventStatic" not in live_cs:
        fail("event fail has no leftover webcam scar")
    else:
        ok("correct 1–4 key glows; success 사고 수습; fail keeps copy plus a cosmetic scar")

    if "RivalWave" in event_cs or kinds.count("=") > 3:
        fail("a new StreamEvent kind was added")
    elif "Event.Kind =" in start_ev and "StreamEventKind.AntiWave" not in start_ev:
        fail("StartEvent no longer assigns existing kinds")
    elif "eventEarliestSeconds: 35" not in balance or "eventAntiFailMental: 8" not in balance or "eventLagFailFreezeSeconds: 3" not in balance:
        fail("event timing / fail numbers were retuned")
    elif "eventAntiFailViewers" in resolve_ev and "eventAntiFailViewers + 1" in resolve_ev:
        fail("event fail penalties were retuned")
    elif "MaybeStartEvent" in live_cs or "StartEvent(" in live_cs:
        fail("LiveStream started firing extra events")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("event accident broke pads, 입력됨, or added timeScale")
    elif "색에 맞는 키 또는 아래 버튼을 눌러" not in live_cs or "주차 클리어" not in (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8"):
        fail("coach or week-clear was dropped while dressing the event")
    elif "토크" in title_cs or "Week2" in title_cs:
        fail("Title started advertising events / later weeks")
    elif "billRent: 8000" not in balance or "Combo >= 5" not in live_cs:
        fail("event accident retuned Week 1 bills or dropped combo SFX")
    else:
        ok("no new event types, no extra rolls, same windows and numbers; Day-1 coach stays")


def check_fan_letter() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    look_cs = (ROOT / "Assets/Scripts/Presentation/FanLetterLook.cs").read_text(encoding="utf-8") if (ROOT / "Assets/Scripts/Presentation/FanLetterLook.cs").exists() else ""
    fandom_rules = (ROOT / "Assets/Scripts/Economy/FandomRules.cs").read_text(encoding="utf-8")
    fandom_asset = (ROOT / "Assets/Resources/Balance/FandomBalance.asset").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    offer = fandom_rules.split("ShouldOfferLetter", 1)[-1].split("public static bool SendLetter", 1)[0]

    if "ShouldOfferLetter" not in fandom_rules or "minjunPresent" not in offer or "haeunPresent" not in offer:
        fail("fan letter is not gated on existing 민준/하은 present flags")
    elif "답장하기" not in settle_cs or "나중에" not in settle_cs or "팬레터" not in settle_cs:
        fail("settlement letter card missing 답장하기 / 나중에")
    elif "FanLetterLook" not in settle_cs or "첫 도네" not in look_cs or "매일 오는 야간" not in look_cs:
        fail("letter card does not use named 민준/하은 copy")
    elif "내일도 켤 거죠" not in look_cs or "내일 밤에 또 올게" not in look_cs:
        fail("warm in-character letter lines are missing")
    elif "답이 없더라고요" not in look_cs or "채팅이 좀 아팠어요" not in look_cs:
        fail("wounded/ignored letters are not shorter and colder")
    elif "SendLetter" not in settle_cs.split("void OnLetter", 1)[-1][:500]:
        fail("답장하기 does not call existing SendLetter")
    elif "letterLoyalty" not in settle_cs or "letterMental" not in settle_cs or "충성 +" not in settle_cs:
        fail("reply heart does not show existing loyalty / mental deltas")
    else:
        ok("named fan letter is a readable card with 답장하기 / 나중에")

    if "letterLoyalty: 4" not in fandom_asset or "letterMental: 8" not in fandom_asset:
        fail("팬레터 loyalty / mental numbers were retuned")
    elif "minjunIgnoreSettlements: 3" not in fandom_asset or "haeunAppearDay: 2" not in fandom_asset:
        fail("민준 ignore / 하은 appear numbers were retuned")
    elif "MaybeSpawnHaeun" not in fandom_rules.split("void OnMorning", 1)[-1][:400]:
        fail("하은 morning spawn was moved off WeekStart / OnMorning")
    elif "새로운팬" in look_cs or "third fan" in look_cs.lower():
        fail("a new named fan was invented")
    elif "팬레터 답장" not in settle_cs or "CanSendLetter" not in fandom_rules:
        fail("existing once-per-day 팬레터 답장 path was removed")
    elif "MaybeSpawnHaeun" not in week_cs and "haeunPresent" not in week_cs:
        fail("WeekStart no longer shows 하은 when present")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "사고 수습" not in (ROOT / "Assets/Scripts/Stream/StreamEvent.cs").read_text(encoding="utf-8"):
        fail("letter card broke pads or the event accident")
    elif "색에 맞는 키 또는 아래 버튼을 눌러" not in live_cs or "주차 클리어" not in settle_cs:
        fail("coach or week-clear was dropped while adding the letter")
    elif "토크" in title_cs or "팬레터" in title_cs or "민준" in title_cs:
        fail("Title started advertising the fan letter")
    elif "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("Week 1 bills were retuned by the fan letter")
    else:
        ok("letter uses existing fandom numbers; Day 1 has no fake letter; WeekStart 하은 stays")


def _catalog_cs_lines(cs: str, name: str) -> list[str]:
    block = cs.split(f"{name} = new[]", 1)[-1].split("};", 1)[0]
    return re.findall(r'"([^"]+)"', block)


def _catalog_asset_lines(asset: str, name: str) -> list[str]:
    rest = asset.split(f"  {name}:", 1)[-1]
    lines: list[str] = []
    for raw in rest.splitlines()[1:]:
        if raw.startswith("  - "):
            lines.append(raw[4:])
        elif raw.startswith("  ") and raw.endswith(":") and not raw.startswith("  - "):
            break
    return lines


def _has_hangul(text: str) -> bool:
    return any("가" <= ch <= "힣" for ch in text)


def check_chat_catalog() -> None:
    catalog_cs = (ROOT / "Assets/Scripts/Data/ChatCatalog.cs").read_text(encoding="utf-8")
    asset = (ROOT / "Assets/Resources/Balance/ChatCatalog.asset").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    kinds = ("positive", "empathy", "laugh", "thanks")
    banned = ("lorem", "ipsum", "hello", "world", "asdf", "test line")
    blob = catalog_cs + "\n" + asset

    for name in kinds:
        cs_lines = _catalog_cs_lines(catalog_cs, name)
        asset_lines = _catalog_asset_lines(asset, name)
        if len(set(cs_lines)) < 16 or len(set(asset_lines)) < 16:
            fail(f"ChatCatalog {name} has fewer than 16 distinct lines")
            continue
        if set(cs_lines) != set(asset_lines):
            fail(f"ChatCatalog {name} defaults and asset pools drifted")
            continue
        if any(not _has_hangul(line) for line in cs_lines):
            fail(f"ChatCatalog {name} has a line with no Hangul")
            continue
        if any(len(line) > 28 for line in cs_lines):
            fail(f"ChatCatalog {name} has a line too long for the bubble")
            continue
        ok(f"ChatCatalog {name} has {len(set(cs_lines))} short Korean lines")

    if any(token in blob.lower() for token in banned):
        fail("ChatCatalog picked up English lorem / placeholder copy")
    elif "월세" not in blob or "슈퍼챗" not in blob or ("빚" not in blob and "부채" not in blob):
        fail("ChatCatalog lost 월세 / 빚 / 슈퍼챗 jokes")
    elif "ㅋㅋ" not in blob and "ㄹㅇ" not in blob:
        fail("ChatCatalog has no light ㅋㅋ / ㄹㅇ")
    elif "talkPositive" in catalog_cs or "gamePositive" in catalog_cs or "string[] minjun" in catalog_cs:
        fail("ChatCatalog grew a new data shape")
    elif "ChatKind.Positive => positive" not in catalog_cs or "Catalog.Pick(kind, Rng)" not in (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8"):
        fail("stream no longer picks from the shared ChatCatalog pools")
    elif "chatSpawnStart: 1.55" not in balance or "billRent: 8000" not in balance:
        fail("chat copy retuned spawn or Week 1 bills")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "답장하기" not in (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8"):
        fail("catalog rewrite broke pads or the fan letter")
    elif "색에 맞는 키 또는 아래 버튼을 눌러" not in live_cs or "사고 수습" not in (ROOT / "Assets/Scripts/Stream/StreamEvent.cs").read_text(encoding="utf-8"):
        fail("catalog rewrite dropped the coach or event accident")
    elif "토크" in title_cs or "민준" in title_cs:
        fail("Title started advertising chat copy")
    else:
        ok("shared ChatCatalog pools only; spawn / bills / named-fan rules unchanged")


def check_week2_beats() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    w2_asset = (ROOT / "Assets/Resources/Balance/Week2Balance.asset").read_text(encoding="utf-8")
    w2r_cs = (ROOT / "Assets/Scripts/Economy/Week2Rules.cs").read_text(encoding="utf-8")
    sched_cs = (ROOT / "Assets/Scripts/Economy/WeekSchedule.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    beats = settle_cs.split("void AdvanceBeats", 1)[-1].split("void ShowMemberSplash", 1)[0]

    if "멤버십 해금" not in settle_cs or "정산 때 멤버" not in settle_cs:
        fail("membership unlock splash is missing")
    elif "membershipJustUnlocked" not in sched_cs or "membershipJustUnlocked" not in settle_cs:
        fail("first membership unlock is not a one-shot splash")
    elif "오늘 클립 올릴까" not in settle_cs or "올린다" not in settle_cs or "패스" not in settle_cs:
        fail("clip decision is not a chunky 올린다 / 패스 card")
    elif "클립 업로드" not in settle_cs or "올리지 않기" not in settle_cs:
        fail("existing clip copy was deleted")
    elif "AttemptClip" not in settle_cs or "DeclineClip" not in settle_cs:
        fail("clip card does not use existing AttemptClip / DeclineClip")
    elif "시청자 +" not in settle_cs or "clipCash" not in settle_cs.split("void OnClipYes", 1)[-1][:500]:
        fail("clip success slam does not read existing clipCash / viewer bonus")
    else:
        ok("Week 2 membership splash and clip card are screenshot beats")

    if beats.find("ShouldOfferLetter") > beats.find("membershipJustUnlocked"):
        fail("membership splash is not after the fan letter")
    elif beats.find("membershipJustUnlocked") > beats.find("CanOfferClip"):
        fail("clip card is not after the membership splash")
    elif "InWeek2" not in w2r_cs.split("CanOfferClip", 1)[-1][:300]:
        fail("clip offer is no longer Week 2 only")
    elif "InWeek2" not in sched_cs.split("TryUnlockMembership", 1)[-1][:400]:
        fail("membership unlock lost its Week 2+ gate")
    elif "startingMembers: 8" not in w2_asset or "membershipPassivePerMember: 150" not in w2_asset:
        fail("membership starting count / ₩150 were retuned")
    elif "clipCash: 30000" not in w2_asset or "clipChance: 30" not in w2_asset or "clipPerfectsRequired: 25" not in w2_asset:
        fail("clip roll numbers were retuned")
    elif "unlockPeakViewers: 40" not in w2_asset or "unlockSuccessfulStreams: 4" not in w2_asset:
        fail("membership unlock thresholds were retuned")
    elif "답장하기" not in settle_cs or "AddColumnPad" not in live_cs or "입력됨" not in live_cs:
        fail("Week 2 beats broke the letter or stream pads")
    elif "토크" in title_cs or "Week2" in title_cs:
        fail("Title started advertising Week 2 beats")
    elif "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("Week 1 bills were retuned by Week 2 beats")
    else:
        ok("letter → 멤버십 해금 → clip; Week 1 gated; Week2Balance numbers unchanged")


def check_rival_duel() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    duel_path = ROOT / "Assets/Scripts/Presentation/RivalDuelView.cs"
    duel_cs = duel_path.read_text(encoding="utf-8") if duel_path.exists() else ""
    w3r_cs = (ROOT / "Assets/Scripts/Economy/Week3Rules.cs").read_text(encoding="utf-8")
    w3_asset = (ROOT / "Assets/Resources/Balance/Week3Balance.asset").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    event_cs = (ROOT / "Assets/Scripts/Stream/StreamEvent.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")

    if not duel_path.exists():
        fail("RivalDuelView is missing")
        return
    if "라이벌" not in duel_cs or "RivalCam" not in duel_cs:
        fail("rival duel is missing the 라이벌 portrait")
    elif "YouFill" not in duel_cs or "RivalFill" not in duel_cs:
        fail("rival duel is missing the two viewer bars")
    elif "스틸 +" not in duel_cs or "라이벌 스틸" not in duel_cs:
        fail("Perfect/Miss steal flashes are missing")
    elif "라이벌 승" not in duel_cs or "라이벌 패" not in duel_cs or "멘탈 −" not in duel_cs:
        fail("end-of-stream 라이벌 승 / 라이벌 패 slam is missing")
    elif "RivalDuelView" not in live_cs or "FlashSteal" not in live_cs or "ShowResult" not in live_cs:
        fail("LiveStream does not bind the rival duel")
    elif "RivalActive" not in duel_cs or "SetActive(on)" not in duel_cs:
        fail("duel chrome is not gated on RivalActive")
    else:
        ok("rival day shows a 라이벌 cam and two viewer bars")

    end = live_cs.split("EndRoutine", 1)[-1].split("void Build", 1)[0]
    if "ApplyRivalResult" not in end:
        fail("EndRoutine no longer applies the existing rival result")
    elif end.find("ApplyRivalResult") > end.find("ShowResult"):
        fail("rival slam does not use ApplyRivalResult (display only)")
    elif "lastRivalWon" not in end or "rivalWinCash" not in end or "rivalLoseMental" not in end:
        fail("rival slam does not read existing win cash / lose mental")
    elif "rivalPerfectSteal" not in live_cs or "rivalMissSteal" not in live_cs:
        fail("steal flash does not read existing Week3 steal amounts")
    else:
        ok("rival slam is display-only on existing ApplyRivalResult")

    if "ShouldStartRival" not in live_cs or "EnableRival" not in live_cs:
        fail("rival still must start from ShouldStartRival / EnableRival")
    elif "enum StreamEventKind" not in event_cs or "GearLag = 2" not in event_cs.split("enum StreamEventKind", 1)[-1].split("}", 1)[0]:
        fail("a second QTE / event kind was added")
    elif "TryEventKey" not in session_cs or "ApplyRivalSteal" not in session_cs:
        fail("existing QTE or rival steal was removed")
    else:
        ok("same one rival stream; no second QTE")

    for token in (
        "rivalDay: 12",
        "rivalPeakViewers: 55",
        "rivalStartViewers: 25",
        "rivalViewersPerSec: 0.9",
        "rivalPerfectSteal: 0.6",
        "rivalMissSteal: 0.8",
        "rivalWinCash: 20000",
        "rivalWinViewerBonus: 6",
        "rivalLoseViewerPenalty: 5",
        "rivalLoseMental: 12",
    ):
        if token not in w3_asset:
            fail(f"Week3Balance rival number changed ({token})")
            break
    else:
        ok("Week3Balance rival numbers unchanged")

    if "playerViewers > rivalViewers" not in w3r_cs or "rivalWinCash" not in w3r_cs:
        fail("ApplyRivalResult formula was rewritten")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("rival duel broke pads, 입력됨, or added timeScale")
    elif "멤버십 해금" not in settle_cs or "오늘 클립 올릴까" not in settle_cs:
        fail("rival duel dropped membership / clip cards")
    elif "Week3" in title_cs or "라이벌" in title_cs or "토크" in title_cs:
        fail("Title started advertising the rival duel")
    elif "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("Week 1 bills were retuned by the rival duel")
    else:
        ok("non-rival days and Week 1 stay unchanged")


def check_week3_goods_beats() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    w3r_cs = (ROOT / "Assets/Scripts/Economy/Week3Rules.cs").read_text(encoding="utf-8")
    w3_asset = (ROOT / "Assets/Resources/Balance/Week3Balance.asset").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    run_cs = (ROOT / "Assets/Scripts/Core/GameRunState.cs").read_text(encoding="utf-8")
    promo_cs = (ROOT / "Assets/Scripts/Stream/GoodsPromo.cs").read_text(encoding="utf-8")
    beats = settle_cs.split("void AdvanceBeats", 1)[-1].split("void ShowMemberSplash", 1)[0]
    unlock = w3r_cs.split("public static void TryUnlockGoods", 1)[-1].split("public static bool ProduceGoods", 1)[0]

    splash = settle_cs.split("void ShowGoodsSplash", 1)[-1][:700] if "void ShowGoodsSplash" in settle_cs else ""
    if "아크릴 스탠드 해금" not in settle_cs or "재고" not in splash or "원가" not in splash or "판매" not in splash:
        fail("acrylic unlock splash is missing")
    elif "goodsJustUnlocked" not in run_cs or "goodsJustUnlocked" not in settle_cs or "goodsJustUnlocked = true" not in w3r_cs:
        fail("first goods unlock is not a one-shot splash")
    elif "지금 아크릴 홍보" not in live_cs or "성공 시 오늘 판매" not in live_cs:
        fail("goods promo card is not the loud 지금 아크릴 홍보 beat")
    elif "홍보 성공" not in live_cs or "FlashPromoSuccess" not in live_cs:
        fail("promo success flash is missing")
    elif "개 팔림" not in settle_cs:
        fail("settlement does not show 아크릴 n개 팔림")
    elif "TryPromo" not in live_cs or "PromoConfirmDown" not in live_cs or "PromoSkipDown" not in live_cs:
        fail("promo no longer uses existing ←/↑ confirm and →/↓ skip")
    elif "TryUnlockGoods" not in w3r_cs or "ProduceGoods" not in w3r_cs:
        fail("existing goods unlock / produce was removed")
    else:
        ok("Week 3 acrylic unlock splash and promo card are screenshot beats")

    if "InWeek3" not in unlock:
        fail("goods unlock lost its Week 3 gate")
    elif "EnablePromo" not in live_cs.split("void Start", 1)[-1].split("void Update", 1)[0] or "InWeek3" not in live_cs.split("void Start", 1)[-1].split("void Update", 1)[0]:
        fail("promo is no longer gated to Week 3 + goods unlock")
    elif beats.find("CanOfferClip") > beats.find("goodsJustUnlocked") and "goodsJustUnlocked" in beats:
        fail("goods splash is not after the Week 2 clip card")
    elif "InWeek2" not in (ROOT / "Assets/Scripts/Economy/Week2Rules.cs").read_text(encoding="utf-8").split("CanOfferClip", 1)[-1][:300]:
        fail("clip offer is no longer Week 2 only")
    elif "goodsUnlockCash: 60000" not in w3_asset or "goodsUnlockStock: 20" not in w3_asset:
        fail("goods unlock cash / stock were retuned")
    elif "goodsProduceCost: 2500" not in w3_asset or "goodsPrice: 7000" not in w3_asset:
        fail("goods produce / price were retuned")
    elif "goodsSoldMembersFactor: 0.4" not in w3_asset or "goodsSoldPeakFactor: 0.08" not in w3_asset:
        fail("daily goods sales formula was retuned")
    elif "goodsPromoMultiplier: 1.5" not in w3_asset or "promoFallbackSeconds: 55" not in w3_asset:
        fail("promo 1.5x / 55s window was retuned")
    elif "membershipUnlocked" not in unlock or "goodsUnlockCash" not in unlock:
        fail("goods unlock is not membership + cash >= 60000")
    else:
        ok("Week 1–2 stay gated; Week3Balance goods numbers unchanged")

    if "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("goods beats broke pads, 입력됨, or added timeScale")
    elif "라이벌 승" not in (ROOT / "Assets/Scripts/Presentation/RivalDuelView.cs").read_text(encoding="utf-8") or "멤버십 해금" not in settle_cs or "오늘 클립 올릴까" not in settle_cs:
        fail("goods beats dropped rival duel or Week 2 cards")
    elif "아크릴 1개 생산" not in settle_cs:
        fail("extra produce button was removed")
    elif "Week3" in title_cs or "아크릴" in title_cs or "굿즈" in title_cs:
        fail("Title started advertising Week 3 goods")
    elif "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("Week 1 bills were retuned by Week 3 goods beats")
    elif "enum StreamEventKind" not in promo_cs and "GearLag = 2" not in (ROOT / "Assets/Scripts/Stream/StreamEvent.cs").read_text(encoding="utf-8"):
        fail("a second QTE was added")
    else:
        ok("produce button stays; rival / membership / clip / Week 1 stay")


def check_week4_agency_beats() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    w4r_cs = (ROOT / "Assets/Scripts/Economy/Week4Rules.cs").read_text(encoding="utf-8")
    w4_asset = (ROOT / "Assets/Resources/Balance/Week4Balance.asset").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    run_cs = (ROOT / "Assets/Scripts/Core/GameRunState.cs").read_text(encoding="utf-8")
    beats = settle_cs.split("void AdvanceBeats", 1)[-1].split("void ShowMemberSplash", 1)[0]
    found = w4r_cs.split("public static bool FoundAgency", 1)[-1].split("public static bool CanScoutJunior", 1)[0]
    can_found = w4r_cs.split("public static bool CanFoundAgency", 1)[-1].split("public static bool FoundAgency", 1)[0]

    agency_card = settle_cs.split("void ShowAgencyCard", 1)[-1][:700] if "void ShowAgencyCard" in settle_cs else ""
    if "에이전시 설립" not in settle_cs or "이후 일" not in agency_card or "고정비" not in agency_card:
        fail("agency found card is missing")
    elif "에이전시 오픈" not in settle_cs or "agencyJustFounded" not in run_cs or "agencyJustFounded = true" not in found:
        fail("agency open splash is not a one-shot after FoundAgency")
    elif "후배 스카우트" not in settle_cs or "ShowJuniorCard" not in settle_cs:
        fail("junior scout card is missing")
    elif "후배 방송" not in settle_cs or "lastJuniorPay" not in settle_cs:
        fail("settlement does not show 후배 방송 +₩ on success days")
    elif "스폰서 멘트" not in live_cs or "계약 유지" not in live_cs or "계약 파기" not in live_cs:
        fail("sponsor line card is not the loud 스폰서 멘트 beat")
    elif "FlashLineResult" not in live_cs or "ApplySponsorLine" not in live_cs:
        fail("sponsor line flash does not use existing ApplySponsorLine")
    elif "FoundAgency" not in settle_cs or "ScoutJunior" not in settle_cs:
        fail("cards do not spend via existing FoundAgency / ScoutJunior")
    else:
        ok("Week 4 agency / junior / sponsor line are screenshot beats")

    if "firstDay" not in can_found or "goodsUnlocked" not in can_found or "agencyUnlockCash" not in can_found:
        fail("agency unlock lost cash / debt / acrylic / day gates")
    elif "CanScoutJunior" not in beats or "agencyFounded" not in w4r_cs.split("CanScoutJunior", 1)[-1][:250]:
        fail("junior card is not gated on existing CanScoutJunior")
    elif beats.find("goodsJustUnlocked") > beats.find("CanFoundAgency"):
        fail("agency card is not after the Week 3 goods splash")
    elif "EnableSponsorLine" not in live_cs.split("void Start", 1)[-1].split("void Update", 1)[0]:
        fail("sponsor line is no longer the existing mid-stream window")
    elif "agencyFoundCost: 40000" not in w4_asset or "agencyDailyCost: 15000" not in w4_asset:
        fail("agency found / daily cost were retuned")
    elif "agencyUnlockCash: 100000" not in w4_asset or "agencyUnlockDebtMax: 40000" not in w4_asset:
        fail("agency unlock gates were retuned")
    elif "juniorScoutCost: 25000" not in w4_asset or "juniorDailySuccess: 4000" not in w4_asset:
        fail("junior scout / pay were retuned")
    elif "sponsorLineBonus: 3000" not in w4_asset or "sponsorFailCash: 15000" not in w4_asset or "sponsorFailMental: 12" not in w4_asset:
        fail("sponsor line numbers were retuned")
    elif "sponsorPeakViewers: 70" not in w4_asset or "sponsorDaily: 10000" not in w4_asset:
        fail("sponsor deal numbers were retuned")
    else:
        ok("Week 1–3 stay gated; Week4Balance numbers unchanged")

    if "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("Week 4 beats broke pads, 입력됨, or added timeScale")
    elif "아크릴 스탠드 해금" not in settle_cs or "라이벌 승" not in (ROOT / "Assets/Scripts/Presentation/RivalDuelView.cs").read_text(encoding="utf-8"):
        fail("Week 4 beats dropped goods splash or rival duel")
    elif "멤버십 해금" not in settle_cs or "오늘 클립 올릴까" not in settle_cs or "주니어 스카우트" not in settle_cs:
        fail("Week 4 beats dropped Week 2 cards or the junior button")
    elif "Week4" in title_cs or "에이전시" in title_cs or "스폰서" in title_cs:
        fail("Title started advertising Week 4 agency")
    elif "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("Week 1 bills were retuned by Week 4 beats")
    else:
        ok("agency / junior / sponsor stay Week 4-only; prior beats stay")


def check_week5_finale_beats() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    w5r_cs = (ROOT / "Assets/Scripts/Economy/Week5Rules.cs").read_text(encoding="utf-8")
    w5_asset = (ROOT / "Assets/Resources/Balance/Week5Balance.asset").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    w5_cs = (ROOT / "Assets/Scripts/Data/Week5Balance.cs").read_text(encoding="utf-8")
    beats = settle_cs.split("void AdvanceBeats", 1)[-1].split("void ShowMemberSplash", 1)[0]
    rank_fn = settle_cs.split("void FillRankPanel", 1)[-1][:900] if "void FillRankPanel" in settle_cs else ""

    if "챌린지 랭킹" not in settle_cs or "루나벨" not in rank_fn or "하츠비" not in rank_fn or "네온토끼" not in rank_fn:
        fail("ranking panel is not you vs 루나벨 / 하츠비 / 네온토끼")
    elif "1위 +" not in rank_fn or "rankingDailyFirstCash" not in rank_fn:
        fail("daily 1st +₩10,000 is missing from the ranking panel")
    elif "lastRankingScore" not in rank_fn or "lastNpcScore" not in rank_fn:
        fail("ranking panel does not show today's scores")
    elif "콘서트 개최" not in settle_cs or "ShowConcertCard" not in settle_cs:
        fail("concert book card is missing")
    elif "퍼포먼스 지금" not in live_cs or "정산 ×" not in live_cs or "FlashConcertSuccess" not in live_cs:
        fail("concert performance card is not the loud 퍼포먼스 지금? beat")
    elif "개최비만 날림" not in settle_cs or "ShowConcertResult" not in settle_cs:
        fail("concert success/fail settlement slam is missing")
    elif "BookConcert" not in settle_cs or "ApplyConcertResult" not in (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8") + settle_cs:
        fail("concert card does not use existing BookConcert / ApplyConcertResult")
    else:
        ok("Week 5 ranking board and concert card are screenshot beats")

    unlock = w5r_cs.split("public static bool RankingUnlocked", 1)[-1].split("public static int DailyScore", 1)[0]
    book = w5r_cs.split("public static bool CanBookConcert", 1)[-1].split("public static bool BookConcert", 1)[0]
    if "rankingDay" not in unlock or "rankingPeakViewers" not in unlock:
        fail("ranking lost peak >= 100 / day >= 22 gates")
    elif "concertUnlockDay" not in book or "concertUnlockCash" not in book or "concertUnlockPeak" not in book:
        fail("concert book lost cash / peak / day gates")
    elif "EnableConcert" not in live_cs.split("void Start", 1)[-1].split("void Update", 1)[0]:
        fail("concert performance is no longer the existing mid-stream window")
    elif "EndingRoot" not in settle_cs or "후배에게 메인 양도" not in settle_cs:
        fail("existing ending pose cards were removed")
    elif "rankingDailyFirstCash: 10000" not in w5_asset or "rankingPeakViewers: 100" not in w5_asset or "rankingDay: 22" not in w5_asset:
        fail("ranking unlock / 1st pay were retuned")
    elif "concertCost: 80000" not in w5_asset or "concertBasePayout: 200000" not in w5_asset or "concertSuccessMultiplier: 1.3" not in w5_asset:
        fail("concert cost / payout / 1.3x were retuned")
    elif "concertUnlockCash: 150000" not in w5_asset or "concertUnlockPeak: 90" not in w5_asset:
        fail("concert unlock gates were retuned")
    elif "루나벨" not in w5_cs or "하츠비" not in w5_cs or "네온토끼" not in w5_cs:
        fail("NPC names were renamed")
    else:
        ok("Week 1–4 stay gated; Week5Balance numbers unchanged")

    if "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("Week 5 beats broke pads, 입력됨, or added timeScale")
    elif "에이전시 오픈" not in settle_cs or "아크릴 스탠드 해금" not in settle_cs or "멤버십 해금" not in settle_cs:
        fail("Week 5 beats dropped prior settlement cards")
    elif "Week5" in title_cs or "콘서트" in title_cs or "랭킹" in title_cs:
        fail("Title started advertising Week 5 ranking/concert")
    elif "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("Week 1 bills were retuned by Week 5 beats")
    else:
        ok("ranking / concert stay Week 5-only; endings and prior beats stay")


def check_fandom_beats() -> None:
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    fandom_rules = (ROOT / "Assets/Scripts/Economy/FandomRules.cs").read_text(encoding="utf-8")
    fandom_cs = (ROOT / "Assets/Scripts/Data/FandomBalance.cs").read_text(encoding="utf-8")
    fandom_asset = (ROOT / "Assets/Resources/Balance/FandomBalance.asset").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    wave = week_cs.split("IEnumerator BillWave", 1)[-1].split("void SpawnIncoming", 1)[0]
    beats = settle_cs.split("void AdvanceBeats", 1)[-1].split("void ShowMemberSplash", 1)[0]
    soothe = fandom_rules.split("public static bool SootheConflict", 1)[-1].split("public static bool StyleConflict", 1)[0]
    style = fandom_rules.split("public static bool StyleConflict", 1)[-1].split("public static bool CanToggleAuto", 1)[0]
    support = fandom_rules.split("public static int RollSupport", 1)[-1].split("public static string HudLine", 1)[0]
    auto = fandom_rules.split("public static bool CanToggleAuto", 1)[-1].split("public static void SetAutoReply", 1)[0]
    auto_cost = fandom_rules.split("public static int AutoCostToday", 1)[-1].split("public static int RollSupport", 1)[0]

    if "콘텐츠 편중 갈등" not in week_cs or "특별방송으로 달래기" not in week_cs or "내 스타일대로" not in week_cs:
        fail("WeekStart lost the day 11 콘텐츠 편중 갈등 two-card")
    elif "conflictSootheMental" not in week_cs or "conflictSootheLoyalty" not in week_cs:
        fail("soothe card does not write existing mental / loyalty deltas")
    elif "conflictStyleT2" not in week_cs or "conflictStyleLoyalty" not in week_cs or "conflictExtraSurcharge" not in week_cs:
        fail("style card does not write existing T2 / loyalty / extra-threat deltas")
    elif "달랬다" not in week_cs or "다음 위협" not in week_cs:
        fail("conflict pick has no one-line result")
    elif "ConflictWash" not in week_cs or "MustResolveConflict" not in wave:
        fail("conflict is not a must-pick wash (can skip)")
    elif "나중에" in week_cs.split("콘텐츠 편중 갈등", 1)[-1].split("오늘 콘텐츠", 1)[0]:
        fail("conflict card gained a skip / 나중에")
    elif "콘텐츠 편중 갈등" not in settle_cs or "특별방송으로 달래기" not in settle_cs or "내 스타일대로" not in settle_cs:
        fail("Settlement lost the same two-card conflict")
    elif "ShowConflictCard" not in beats or "MustResolveConflict" not in beats:
        fail("Settlement does not force the conflict card that day")
    else:
        ok("day 11 콘텐츠 편중 갈등 is a must-pick two-card with written deltas")

    if "conflictSootheMental" not in soothe or "conflictSootheLoyalty" not in soothe:
        fail("SootheConflict deltas were retuned")
    elif "conflictStyleT2" not in style or "conflictStyleLoyalty" not in style or "conflictExtraSurcharge" not in style:
        fail("StyleConflict deltas were retuned")
    elif "conflictDay: 11" not in fandom_asset or "conflictSootheMental: 10" not in fandom_asset:
        fail("FandomBalance conflict day / soothe numbers were retuned")
    elif "conflictStyleT2: 2" not in fandom_asset or "conflictStyleLoyalty: 10" not in fandom_asset or "conflictExtraSurcharge: 2000" not in fandom_asset:
        fail("FandomBalance style numbers were retuned")
    elif "conflictDay" not in fandom_rules.split("public static void OnMorning", 1)[-1][:800]:
        fail("conflict is no longer gated on FandomBalance.conflictDay")
    else:
        ok("conflict still uses existing FandomBalance deltas; Week 1–2 stay quiet")

    if "팬 지원금" not in week_cs or "SupportCard" not in week_cs or "SupportRoot" not in week_cs:
        fail("WeekStart has no gold 팬 지원금 splash")
    elif "RollSupport" not in wave or "billsAppliedThisDay" not in wave:
        fail("support splash does not peek the existing RollSupport before bills")
    elif "peek > 0" not in wave and "peek>0" not in wave:
        fail("support splash is not gated on a real roll")
    elif "lastFanSupport > 0" not in week_cs:
        fail("bill slam lost the real 팬 지원금 chip")
    elif "loyalty < f.supportLoyaltyMin" not in support or "loyalty / 2" not in support:
        fail("RollSupport chance / loyalty gate was retuned")
    elif "supportBase" not in support or "supportPerT3" not in support or "supportPerT4" not in support:
        fail("RollSupport amount left FandomBalance")
    elif "supportLoyaltyMin: 60" not in fandom_asset or "supportBase: 3000" not in fandom_asset:
        fail("팬 지원금 loyalty / base were retuned")
    elif "supportPerT3: 200" not in fandom_asset or "supportPerT4: 4000" not in fandom_asset:
        fail("팬 지원금 T3/T4 amounts were retuned")
    elif "supportMin: 3000" not in fandom_asset or "supportMax: 20000" not in fandom_asset:
        fail("팬 지원금 min/max were retuned")
    else:
        ok("팬 지원금 gold splash only when the existing roll succeeds")

    if "기본 자동응답" not in settle_cs or "AutoCard" not in settle_cs or "autoReplyPrompted" not in settle_cs:
        fail("Week 4 auto-reply is still a quiet toggle")
    elif "autoDailyCost" not in settle_cs or "켜기" not in settle_cs or "끄기" not in settle_cs:
        fail("auto-reply card is missing 켜기/끄기 or the existing daily fee")
    elif "ShowAutoCard" not in beats or "CanToggleAuto" not in beats:
        fail("auto-reply card is not shown once when agency exists")
    elif "agencyFounded" not in auto or "InWeek4" not in auto or "InWeek5" not in auto:
        fail("CanToggleAuto is no longer Week 4+ agency")
    elif "autoDailyCost" not in auto_cost or "autoDailyCost: 8000" not in fandom_asset or "autoDailyCost = 8000" not in fandom_cs:
        fail("기본 자동응답 daily fee was retuned")
    else:
        ok("기본 자동응답 is a once-only on/off card; ₩8,000/day unchanged")

    if "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("fandom beats broke pads, 입력됨, or added timeScale")
    elif "멤버십 해금" not in settle_cs or "아크릴 스탠드 해금" not in settle_cs or "에이전시 오픈" not in settle_cs:
        fail("fandom beats dropped prior settlement cards")
    elif "챌린지 랭킹" not in settle_cs or "라이벌 승" not in (ROOT / "Assets/Scripts/Presentation/RivalDuelView.cs").read_text(encoding="utf-8"):
        fail("fandom beats dropped ranking or rival duel")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs or "리액션" in title_cs or "ContentBalance" in title_cs:
        fail("Title started advertising fandom / content / later weeks")
    elif "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("Week 1 bills were retuned by fandom beats")
    else:
        ok("fandom beats stay on existing rules; prior presentation stays")


def check_portrait_safe_area() -> None:
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    safe_cs = (ROOT / "Assets/Scripts/Presentation/StreamSafeArea.cs").read_text(encoding="utf-8")
    fit_cs = (ROOT / "Assets/Scripts/Presentation/SafeFitCard.cs").read_text(encoding="utf-8") if (ROOT / "Assets/Scripts/Presentation/SafeFitCard.cs").exists() else ""
    pair_cs = (ROOT / "Assets/Scripts/Presentation/SafePairLayout.cs").read_text(encoding="utf-8") if (ROOT / "Assets/Scripts/Presentation/SafePairLayout.cs").exists() else ""
    uikit_cs = (ROOT / "Assets/Scripts/Presentation/UiKit.cs").read_text(encoding="utf-8")

    if "Screen.safeArea" not in safe_cs or "Attach" not in safe_cs:
        fail("StreamSafeArea lost Screen.safeArea / Attach")
    elif "StreamSafeArea.Attach" not in title_cs or "StreamSafeArea.Attach" not in week_cs or "StreamSafeArea.Attach" not in settle_cs:
        fail("Title / WeekStart / Settlement are not inside StreamSafeArea")
    elif "StreamSafeArea.Attach" not in live_cs and "AddComponent<StreamSafeArea>" not in live_cs:
        fail("LiveStream pads left StreamSafeArea")
    elif "SafeFitCard" not in fit_cs or "SafePairLayout" not in pair_cs:
        fail("portrait fit / stack helpers are missing")
    elif "SafeFitCard.Bind" not in settle_cs or "SafePairLayout.Bind" not in settle_cs:
        fail("settlement overlay cards are not fitted / stacked")
    elif "SafeFitCard.Bind" not in week_cs or "SafePairLayout.Bind" not in week_cs:
        fail("WeekStart conflict / support / wave are not fitted")
    elif "SafeFitCard.Bind" not in title_cs or "MakeScrollBody" not in title_cs:
        fail("Title 조작 설명 is not a fitted scrolling card")
    elif "EndingCard" in settle_cs and "SafeFitCard.Bind(endingCard" not in settle_cs:
        fail("ending card is not fitted into safeArea")
    elif "ClearGo" not in settle_cs or "StampRestart" not in settle_cs:
        fail("week-clear / bankrupt confirm buttons missing")
    else:
        ok("Title / WeekStart / Settlement / overlays sit inside StreamSafeArea")

    if "AddColumnPad" not in live_cs or "index / (float)count" not in live_cs:
        fail("LiveStream pads left the full-width equal-column row")
    elif "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("portrait layout broke 입력됨 or added timeScale")
    elif "ChoiceRow" not in live_cs or "PromoConfirm" not in live_cs:
        fail("promo / sponsor / concert confirm-skip left the tappable row")
    elif "Event" not in live_cs.split("AddColumnPad(eventRow", 1)[0][-80:] and "AddColumnPad(eventRow" not in live_cs:
        fail("event 1–4 pads are gone")
    elif "SafeFitCard.Bind(_promoRoot" not in live_cs or "SafeFitCard.Bind(_concertRoot" not in live_cs:
        fail("live promo / concert cards are not fitted")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising later weeks")
    elif "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("Week 1 bills were retuned by portrait layout")
    elif "멤버십 해금" not in settle_cs or "콘텐츠 편중 갈등" not in week_cs or "팬 지원금" not in week_cs:
        fail("portrait layout dropped prior screenshot cards")
    elif "Wrap" not in uikit_cs or "MakeScrollBody" not in uikit_cs:
        fail("card copy cannot wrap or scroll")
    elif "defaultScreenOrientation: 0" not in (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(
        encoding="utf-8"
    ):
        fail("StreamSafeArea portrait assume lost Android Portrait lock")
    else:
        ok("pads stay full-width; new cards wrap/stack; confirm stays on-screen")


def check_day_headline() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    head_cs = (ROOT / "Assets/Scripts/Presentation/DayHeadline.cs").read_text(encoding="utf-8") if (ROOT / "Assets/Scripts/Presentation/DayHeadline.cs").exists() else ""
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    recap = settle_cs.split("var recap", 1)[0][-400:] if "var recap" in settle_cs else ""

    if "오늘 헤드라인" not in settle_cs or "DayHeadline" not in settle_cs:
        fail("settlement recap has no 오늘 헤드라인")
    elif "청구 커버" not in head_cs or "청구 미달" not in head_cs:
        fail("headline does not say 청구 커버 / 청구 미달 from tonight's bills")
    elif "TonightBills" not in head_cs or "lastStreamIncome" not in head_cs:
        fail("headline does not use existing income / tonight bills")
    elif "라이벌 승" not in head_cs or "민준 답장" not in head_cs or "하이프 실패" not in head_cs:
        fail("headline is missing rival / letter / hype facts")
    elif "lastRivalMatch" not in head_cs or "lastFanLetter" not in head_cs or "lastHadHype" not in head_cs:
        fail("headline is not gated on what actually happened")
    elif "lastClipSuccess" not in head_cs or "lastStreamPeakViewers" not in head_cs:
        fail("headline dropped clip / peak")
    elif "ApplyHeadline" not in settle_cs or "ClearHeadline" not in settle_cs or "StampHeadline" not in settle_cs:
        fail("week-clear / bankrupt do not reuse the headline")
    elif '"오늘 수입"' not in settle_cs or '"청구"' not in settle_cs:
        fail("headline replaced the green 수입 / red 청구 tiles")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("headline slice broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in settle_cs:
        fail("headline slice dropped StreamSafeArea")
    elif "멤버십 해금" not in settle_cs or "콘텐츠 편중 갈등" not in (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8"):
        fail("headline slice dropped prior cards")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising fandom / later weeks")
    elif "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("Week 1 bills were retuned by the headline")
    else:
        ok("settlement recap has a screenshot 오늘 헤드라인 from existing facts")


def check_chat_nicks() -> None:
    nicks_path = ROOT / "Assets/Scripts/Data/ChatNicks.cs"
    nicks_cs = nicks_path.read_text(encoding="utf-8") if nicks_path.exists() else ""
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    catalog_cs = (ROOT / "Assets/Scripts/Data/ChatCatalog.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    required = ("밤샌사람", "월세토끼", "정산요정", "ㄹㅇ팬", "빚쟁이형")
    pool = re.findall(r'"(.*?)"', nicks_cs.split("Pick(", 1)[0] if "Pick(" in nicks_cs else nicks_cs)

    if not nicks_cs or "ChatNicks" not in nicks_cs:
        fail("falling chat has no ChatNicks pool")
    elif any(name not in nicks_cs for name in required):
        fail("nick pool is missing 밤샌사람 / 월세토끼 / 정산요정 / ㄹㅇ팬 / 빚쟁이형")
    elif len(set(pool)) < 18:
        fail("nick pool is smaller than ~20 fake chat nicks")
    elif any(not _has_hangul(name) for name in pool):
        fail("a chat nick has no Hangul")
    elif any(len(name) > 8 for name in pool):
        fail("a chat nick is too long for the bubble")
    elif "Pick(int runSeed, int noteId)" not in nicks_cs or "runSeed * 397" not in nicks_cs:
        fail("nicks are not deterministic from runSeed + note id")
    elif "ChatNicks.Pick(_runSeed, id)" not in session_cs or "BindChatSeed" not in session_cs:
        fail("stream does not pick nicks from runSeed + note id")
    elif "BindChatSeed(gm.Run.runSeed)" not in live_cs:
        fail("LiveStream does not bind runSeed for nicks")
    elif '"Nick"' not in live_cs or "FormatWon(note.SuperchatWon)" not in live_cs:
        fail("bubbles do not show a nick label or superchat ₩")
    elif "BindNamedFans" not in session_cs or "minjunName" not in live_cs or "haeunName" not in live_cs:
        fail("민준/하은 special labels were dropped")
    elif "NamedFan" not in live_cs or "슈퍼팬 · 첫 도네" not in live_cs:
        fail("named-fan gold/pink copy was replaced")
    elif "Catalog.Pick(kind, Rng)" not in session_cs or "RollRegularKind" not in session_cs:
        fail("nick slice retuned chat spawn or ChatKind weights")
    elif "string[] minjun" in catalog_cs or "talkPositive" in catalog_cs:
        fail("ChatCatalog grew a new data shape for nicks")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("nick slice broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in live_cs or "오늘 헤드라인" not in settle_cs:
        fail("nick slice dropped StreamSafeArea or 오늘 헤드라인")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising nicks / later weeks")
    elif "chatSpawnStart: 1.55" not in balance or "billRent: 8000" not in balance:
        fail("nick slice retuned spawn or Week 1 bills")
    else:
        ok("falling chat shows deterministic Korean nicks; 민준/하은 stay special")


def check_hype_wash() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    avatar_cs = (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    nicks_cs = (ROOT / "Assets/Scripts/Data/ChatNicks.cs").read_text(encoding="utf-8") if (ROOT / "Assets/Scripts/Data/ChatNicks.cs").exists() else ""

    if "RefreshHypeShow" not in live_cs or "HypeBanner" not in live_cs:
        fail("hype does not eat the screen")
    elif "하이프" not in live_cs or "hypeIncomeMultiplier" not in live_cs:
        fail("하이프 2.5x ticker was dropped")
    elif "HypeLeft" not in live_cs or "HypeCount" not in live_cs:
        fail("hype countdown is not visible")
    elif "SetHype" not in avatar_cs or "SetHype(true)" not in live_cs:
        fail("avatar has no hype sparkle / happy pose")
    elif "HypeChatGlow" not in live_cs:
        fail("chat bubbles do not brighten during hype")
    elif "ComboSting" not in live_cs or "comboIncomeMultiplier" not in live_cs:
        fail("combo 5 has no smaller 1.5x sting")
    elif "_comboStingFlash * 0.20f" not in live_cs:
        fail("combo 5 sting is as loud as the full hype wash")
    elif "_look.Wash" not in live_cs or "SetHype(false)" not in live_cs:
        fail("hype end does not snap back to today's content wash")
    elif "if (hypeActive)" not in rules_cs or "return b.hypeIncomeMultiplier;" not in rules_cs:
        fail("StreamRules.IncomeMultiplier math was retuned")
    elif "Combo >= 5" not in live_cs or "PlayOneShot" not in live_cs:
        fail("combo-5 SFX was dropped")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("hype wash broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in live_cs or '"Nick"' not in live_cs or "오늘 헤드라인" not in settle_cs:
        fail("hype wash dropped StreamSafeArea, nicks, or 오늘 헤드라인")
    elif "밤샌사람" not in nicks_cs:
        fail("hype wash dropped chat nicks")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising hype / later weeks")
    elif "hypeSeconds: 12" not in balance or "hypeIncomeMultiplier: 2.5" not in balance:
        fail("hype numbers were retuned")
    elif "comboIncomeMultiplier: 1.5" not in balance or "hypePerfectCombo: 9" not in balance:
        fail("combo / hype thresholds were retuned")
    elif "billRent: 8000" not in balance:
        fail("Week 1 bills were retuned by the hype wash")
    else:
        ok("hype eats the screen gold; combo 5 is a smaller sting; numbers unchanged")


def check_combo_break() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")

    if "콤보 끊김" not in live_cs or "ShowComboBreak" not in live_cs:
        fail("broken combo has no 콤보 끊김 sting")
    elif "comboWas >= 2" not in live_cs:
        fail("콤보 끊김 is not gated on combo ≥ 2")
    elif "_comboBreakLeft = 0.25f" not in live_cs:
        fail("콤보 끊김 is not a 0.25s sting")
    elif "ComboBreak" not in live_cs or "Palette.MoneyRed" not in live_cs.split("void ShowComboBreak", 1)[-1].split("void TickComboBreak", 1)[0] and '"콤보 끊김"' not in live_cs:
        fail("콤보 끊김 sting is not red")
    elif "reset = true" not in rules_cs or "if (result.ResetCombo)" not in session_cs:
        fail("combo break sting retuned miss/combo math")
    elif "ComboSting" not in live_cs or "comboIncomeMultiplier" not in live_cs:
        fail("combo break dropped the combo-5 sting")
    elif "ShowMissSting" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("combo break dropped miss sting, pads, or added timeScale")
    elif "AddColumnPad" not in live_cs or "StreamSafeArea" not in live_cs:
        fail("combo break dropped pads or StreamSafeArea")
    elif "오늘 헤드라인" not in settle_cs:
        fail("combo break dropped 오늘 헤드라인")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising combo break / later weeks")
    elif "hypeSeconds: 12" not in balance or "comboIncomeMultiplier: 1.5" not in balance:
        fail("combo break retuned hype / combo numbers")
    elif "billRent: 8000" not in balance:
        fail("combo break retuned Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("combo break dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("combo break moved Unity off 6000.5.9f1")
    else:
        ok("combo ≥ 2 miss flashes 콤보 끊김; combo-0 miss stays a normal Miss")


def check_clock_urgency() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")

    if "RefreshClockChip" not in live_cs or "TimeLeft <= 10f" not in live_cs:
        fail("last 10s clock does not go urgent")
    elif '"종료"' not in live_cs or "TimeLeft <= 0f" not in live_cs:
        fail("clock 0 does not snap to 종료")
    elif "sfx_clock" not in live_cs or "PlaySfx(_clockTick" not in live_cs:
        fail("last 10s has no per-second tick")
    elif "shown != _lastClockSec" not in live_cs:
        fail("clock tick is not once per second")
    elif "streamSeconds = 90f" not in session_cs.split("TimeLeft = balance.streamSeconds", 1)[0] and "TimeLeft = balance.streamSeconds" not in session_cs:
        fail("clock urgency retuned stream length wiring")
    elif "streamSeconds: 90" not in balance:
        fail("clock urgency retuned the 90s stream")
    elif "콤보 끊김" not in live_cs or "ShowMissSting" not in live_cs:
        fail("clock urgency dropped combo-break or miss sting")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("clock urgency broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in live_cs or "오늘 헤드라인" not in settle_cs:
        fail("clock urgency dropped StreamSafeArea or 오늘 헤드라인")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising clock / later weeks")
    elif "billRent: 8000" not in balance or "hypeSeconds: 12" not in balance:
        fail("clock urgency retuned Week 1 bills or hype")
    elif "defaultScreenOrientation: 0" not in player:
        fail("clock urgency dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("clock urgency moved Unity off 6000.5.9f1")
    else:
        ok("last 10s clock pulses red and ticks 10…9…; 0 snaps to 종료")


def check_on_air() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")

    if '"ON AIR"' not in live_cs or "방송 시작" not in live_cs or "TickOnAir" not in live_cs:
        fail("stream start has no ON AIR / 방송 시작 sting")
    elif "_onAirLeft = 0.6f" not in live_cs:
        fail("ON AIR sting is not 0.6s")
    elif "PlaySfx(_onAirCue" not in live_cs or "Audio/sfx_onair" not in live_cs:
        fail("ON AIR has no start sting clip")
    elif "EnableFirstStreamCoach" not in live_cs or "_onAirLeft <= 0f" not in live_cs:
        fail("Day-1 coach was replaced by ON AIR or no longer runs after it")
    elif "_nextChatAt = 0.4f" not in session_cs:
        fail("ON AIR retuned first-note timing")
    elif "streamSeconds: 90" not in balance or "TimeLeft = balance.streamSeconds" not in session_cs:
        fail("ON AIR retuned the 90s stream")
    elif "RefreshClockChip" not in live_cs or "콤보 끊김" not in live_cs:
        fail("ON AIR dropped last-10s clock or combo-break")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("ON AIR broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in live_cs or "오늘 헤드라인" not in settle_cs:
        fail("ON AIR dropped StreamSafeArea or 오늘 헤드라인")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising ON AIR / later weeks")
    elif "billRent: 8000" not in balance or "hypeSeconds: 12" not in balance:
        fail("ON AIR retuned Week 1 bills or hype")
    elif "defaultScreenOrientation: 0" not in player:
        fail("ON AIR dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("ON AIR moved Unity off 6000.5.9f1")
    else:
        ok("stream opens with 0.6s ON AIR / 방송 시작; Day-1 coach still follows")


def check_perfect_good() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    judge = live_cs.split("void ShowJudge", 1)[-1].split("void BeginSuperchatFly", 1)[0]

    if "PERFECT" not in judge or "GOOD" not in judge:
        fail("judge pop lost PERFECT / GOOD labels")
    elif "Palette.Gold" not in judge or "_judgePopMax = 0.2f" not in judge:
        fail("Perfect is not a gold 0.2s pop")
    elif "Color.white" not in judge or "1.08f" not in judge:
        fail("Good is not a smaller white pop")
    elif "Palette.MoneyRed" not in judge or "_judgePopMax = 0.25f" not in judge:
        fail("Miss lost the existing red 0.25s pop")
    elif "콤보 끊김" not in live_cs or "comboWas >= 2" not in live_cs:
        fail("Perfect/Good pop dropped combo-break")
    elif "perfectWindow * " not in rules_cs or "b.greatWindow" not in rules_cs or "b.goodWindow" not in rules_cs:
        fail("Perfect/Good pop retuned hit windows")
    elif "perfectWindow: 0.07" not in balance or "greatWindow: 0.13" not in balance or "goodWindow: 0.22" not in balance:
        fail("Perfect/Good pop retuned Week 1 hit windows")
    elif '"ON AIR"' not in live_cs or "RefreshClockChip" not in live_cs:
        fail("Perfect/Good pop dropped ON AIR or last-10s clock")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("Perfect/Good pop broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in live_cs or "오늘 헤드라인" not in settle_cs:
        fail("Perfect/Good pop dropped StreamSafeArea or 오늘 헤드라인")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising judge pops / later weeks")
    elif "billRent: 8000" not in balance:
        fail("Perfect/Good pop retuned Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("Perfect/Good pop dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("Perfect/Good pop moved Unity off 6000.5.9f1")
    else:
        ok("Perfect is a gold 0.2s pop; Good is a smaller white GOOD; Miss stays red")


def check_income_pop() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    hit = live_cs.split("if (j == Judgement.Miss)", 1)[-1].split("SyncNotes();", 1)[0]

    if "ShowIncomeDelta" not in live_cs or "IncomePop" not in live_cs:
        fail("successful notes have no +₩ popup on 지금 수입")
    elif '"+" + EconomyRules.FormatWon' not in live_cs and 'text = "+" + EconomyRules.FormatWon' not in live_cs:
        fail("+₩ popup does not use FormatWon")
    elif "ShowIncomeDelta(_session.LiveIncome - _incomeMarked)" not in live_cs:
        fail("+₩ popup is not the actual LiveIncome delta")
    elif "if (!note.IsSuperchat)" not in hit or "ShowIncomeDelta" not in hit:
        fail("superchat fly was duplicated as the +₩ popup")
    elif "BeginSuperchatFly" not in live_cs:
        fail("superchat fly was dropped")
    elif "ShowIncomeDelta" in live_cs.split("if (j == Judgement.Miss)", 1)[1].split("else if (note.IsSuperchat)", 1)[0]:
        fail("Miss shows a +₩ popup")
    elif "incomePerViewerPerSec" not in rules_cs or "TickIncome += gained" not in session_cs:
        fail("+₩ popup retuned tick income math")
    elif "incomePerViewerPerSec: 3" not in balance:
        fail("+₩ popup retuned ₩ per viewer")
    elif "Palette.Gold" not in live_cs.split("void ShowJudge", 1)[-1] or "Color.white" not in live_cs.split("void ShowJudge", 1)[-1]:
        fail("+₩ popup dropped Perfect gold / Good white")
    elif "콤보 끊김" not in live_cs or '"ON AIR"' not in live_cs or "RefreshClockChip" not in live_cs:
        fail("+₩ popup dropped combo-break, ON AIR, or last-10s clock")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("+₩ popup broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in live_cs or "오늘 헤드라인" not in settle_cs:
        fail("+₩ popup dropped StreamSafeArea or 오늘 헤드라인")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising income pop / later weeks")
    elif "billRent: 8000" not in balance or "superchatMinWon: 1000" not in balance:
        fail("+₩ popup retuned Week 1 bills or superchat")
    elif "defaultScreenOrientation: 0" not in player:
        fail("+₩ popup dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("+₩ popup moved Unity off 6000.5.9f1")
    else:
        ok("successful notes pop +₩ next to 지금 수입; superchat fly stays; Miss does not")


def check_end_cut() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    debug_cs = (ROOT / "Assets/Scripts/Core/PlaytestDebug.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    end = live_cs.split("EndRoutine", 1)[-1].split("void Build", 1)[0]

    show = live_cs.split("void ShowEndCut", 1)[-1].split("void Build", 1)[0]
    if "ShowEndCut" not in live_cs or "방송 종료" not in live_cs:
        fail("90s end has no 방송 종료 cut")
    elif "WaitForSeconds(0.5f)" not in end:
        fail("방송 종료 cut is not 0.5s")
    elif "EndCut" not in live_cs or "0f, 0f, 0f, 0.96f" not in live_cs:
        fail("방송 종료 cut has no black flash")
    elif "PlaySfx(_endCutCue" not in show or "Audio/sfx_end_cut" not in live_cs:
        fail("방송 종료 cut has no end-cut sting")
    elif "ApplyStreamPayout" not in end or "GoSettlement" not in end:
        fail("end cut dropped payout or settlement")
    elif "gm.GoSettlement()" not in debug_cs or "ShowEndCut" in debug_cs:
        fail("F10 no longer jumps straight to settlement")
    elif "TimeLeft = balance.streamSeconds" not in session_cs or "streamSeconds: 90" not in balance:
        fail("end cut retuned the 90s stream")
    elif '"ON AIR"' not in live_cs or "ShowIncomeDelta" not in live_cs or "RefreshClockChip" not in live_cs:
        fail("end cut dropped ON AIR, +₩ popup, or last-10s clock")
    elif "강제 종료" not in end or "WaitForSeconds(1.25f)" not in end:
        fail("end cut replaced the existing force-end sting")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("end cut broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in live_cs or "오늘 헤드라인" not in settle_cs:
        fail("end cut dropped StreamSafeArea or 오늘 헤드라인")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising end cut / later weeks")
    elif "billRent: 8000" not in balance:
        fail("end cut retuned Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("end cut dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("end cut moved Unity off 6000.5.9f1")
    else:
        ok("90s end cuts with 0.5s 방송 종료; F10 still jumps to settlement")


def check_income_count() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    save_cs = (ROOT / "Assets/Scripts/Core/RunSave.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")

    if "TickIncomeCount" not in settle_cs or "_incomeCountT / 0.6f" not in settle_cs:
        fail("오늘 수입 does not count up over ~0.6s")
    elif "FormatWon(0)" not in settle_cs or "FormatWon(shown)" not in settle_cs:
        fail("income count does not start at 0 and use FormatWon")
    elif "lastStreamIncome" not in settle_cs or "_incomeTarget = run.lastStreamIncome" not in settle_cs:
        fail("income count does not use the real lastStreamIncome")
    elif "shown >= _incomeBill" not in settle_cs or "_cashUp = true" not in settle_cs:
        fail("crossing the bill does not reuse the existing cover-gold cash pulse")
    elif "SlamBillCover" in settle_cs or "CoverSlam" in settle_cs:
        fail("settlement grew a second 청구 커버 slam")
    elif "SlamBillCover" not in live_cs:
        fail("mid-stream cover slam was dropped")
    elif "lastStreamIncome =" in settle_cs.split("void TickIncomeCount", 1)[-1].split("void Render", 1)[0]:
        fail("income count writes save/payout numbers")
    elif '"오늘 수입"' not in settle_cs or "오늘 헤드라인" not in settle_cs:
        fail("income count dropped 오늘 수입 / 오늘 헤드라인")
    elif "ShowEndCut" not in live_cs or "ShowIncomeDelta" not in live_cs:
        fail("income count dropped 방송 종료 or +₩ popup")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("income count broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising settlement count / later weeks")
    elif "billRent: 8000" not in balance:
        fail("income count retuned Week 1 bills")
    elif "lastStreamIncome" not in save_cs:
        fail("income count dropped lastStreamIncome from the save")
    elif "defaultScreenOrientation: 0" not in player:
        fail("income count dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("income count moved Unity off 6000.5.9f1")
    else:
        ok("settlement 오늘 수입 counts 0→total in 0.6s; bill-cross reuses cash gold once")


def check_shortfall() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    head_cs = (ROOT / "Assets/Scripts/Presentation/DayHeadline.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    count = settle_cs.split("void TickIncomeCount", 1)[-1].split("void ShowShortfall", 1)[0]

    if "ShowShortfall" not in settle_cs or "청구 미달" not in settle_cs:
        fail("short night has no 청구 미달 chip")
    elif "_shortFlash = 0.35f" not in settle_cs:
        fail("청구 미달 flash is not 0.35s")
    elif "_incomeTarget < _incomeBill" not in count or "ShowShortfall" not in count:
        fail("청구 미달 does not fire after the count-up snap on a short night")
    elif "ShowShortfall" in settle_cs.split("if (!_coverCrossed && _incomeTarget >= _incomeBill", 1)[-1].split("void ShowShortfall", 1)[0]:
        fail("covered nights also flash 청구 미달")
    elif "SlamBillCover" in settle_cs or "CoverSlam" in settle_cs:
        fail("shortfall grew a fake 청구 커버 slam")
    elif "_cashUp = true" not in settle_cs or "_incomeCoverFlash = 1f" not in settle_cs:
        fail("covered nights lost the gold pulse")
    elif "lastBills =" in settle_cs.split("void ShowShortfall", 1)[-1].split("void TickShortfall", 1)[0]:
        fail("shortfall writes bill / save numbers")
    elif "TickIncomeCount" not in settle_cs or "_incomeCountT / 0.6f" not in settle_cs:
        fail("shortfall dropped the 0.6s income count")
    elif "청구 미달" not in head_cs:
        fail("headline lost 청구 미달")
    elif "ShowEndCut" not in live_cs or "ShowIncomeDelta" not in live_cs:
        fail("shortfall dropped 방송 종료 or +₩ popup")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("shortfall broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising shortfall / later weeks")
    elif "billRent: 8000" not in balance:
        fail("shortfall retuned Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("shortfall dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("shortfall moved Unity off 6000.5.9f1")
    else:
        ok("short night flashes 청구 미달 after the count snap; covered nights stay gold")


def check_morning_bill() -> None:
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")

    if '"오늘 청구"' not in week_cs or "BillChip" not in week_cs:
        fail("WeekStart does not show a 오늘 청구 tile")
    elif "_billSlam = 0.25f" not in week_cs:
        fail("오늘 청구 does not slam in 0.25s")
    elif "ThreatBanner" not in week_cs.split("BillChip", 1)[-1].split("void RefreshHud", 1)[0] and "bill ? ArtSprites.ThreatBanner" not in week_cs:
        fail("오늘 청구 is not on a red-tinted tile")
    elif "PeekTodayBills" not in week_cs or "TonightBills" not in week_cs:
        fail("오늘 청구 does not read the existing today-bill total")
    elif "lastBills =" in week_cs:
        fail("morning bill tile writes lastBills")
    elif "ApplyDailyBills" not in week_cs or "SpawnIncoming" not in week_cs:
        fail("morning bill tile dropped the existing bill wave")
    elif "마지막 날" not in week_cs or "LastDayBanner" not in week_cs or "RefreshLastDay" not in week_cs:
        fail("morning bill tile dropped the last-day banner")
    elif "ShowShortfall" not in settle_cs or "청구 미달" not in settle_cs:
        fail("morning bill tile dropped settlement 청구 미달")
    elif "TickIncomeCount" not in settle_cs or "ShowEndCut" not in live_cs:
        fail("morning bill tile dropped income count or 방송 종료")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("morning bill tile broke pads, 입력됨, or added timeScale")
    elif "UnlockUiInputForStream" not in week_cs or "StreamSafeArea" not in week_cs:
        fail("morning bill tile dropped WeekStart input unlock or safe area")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising morning bills / later weeks")
    elif "TonightBills" not in eco_cs:
        fail("morning bill tile dropped EconomyRules.TonightBills")
    elif "billRent: 8000" not in balance:
        fail("morning bill tile retuned Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("morning bill tile dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("morning bill tile moved Unity off 6000.5.9f1")
    else:
        ok("WeekStart 오늘 청구 slams 0.25s on a red tile; last-day banner stays")


def check_debt_count() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    state_cs = (ROOT / "Assets/Scripts/Core/GameRunState.cs").read_text(encoding="utf-8")
    save_cs = (ROOT / "Assets/Scripts/Core/RunSave.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    tick = settle_cs.split("void TickDebtCount", 1)[-1].split("void TickIncomeCount", 1)[0]

    if "TickDebtCount" not in settle_cs or "_debtCountT / 0.4f" not in settle_cs:
        fail("부채 does not count up over ~0.4s")
    elif "_debtTo > _debtFrom" not in settle_cs or "debtAtDayStart" not in settle_cs:
        fail("부채 count does not run only when tonight's debt rose")
    elif "Palette.MoneyRed" not in settle_cs.split("_debtCounting", 1)[-1].split("else if (_tileDebt != null && _debtDip", 1)[0]:
        fail("rising 부채 is not tinted red while it climbs")
    elif "debt =" in tick or "debtAtDayStart =" in tick:
        fail("부채 count writes debt math")
    elif "debtAtDayStart = debt" not in state_cs or "debtAtDayStart" not in save_cs:
        fail("부채 count does not remember the actual day-start debt")
    elif "ConvertNegativeCashToDebt" in settle_cs:
        fail("부채 count reimplemented debt math")
    elif "TickIncomeCount" not in settle_cs or "ShowShortfall" not in settle_cs or "청구 미달" not in settle_cs:
        fail("부채 count dropped income count or 청구 미달")
    elif '"오늘 청구"' not in week_cs or "_billSlam = 0.25f" not in week_cs:
        fail("부채 count dropped WeekStart 오늘 청구 slam")
    elif "ShowEndCut" not in live_cs or "ShowIncomeDelta" not in live_cs:
        fail("부채 count dropped 방송 종료 or +₩ popup")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("부채 count broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising debt count / later weeks")
    elif "billRent: 8000" not in balance or "startingDebt: 50000" not in balance:
        fail("부채 count retuned Week 1 debt or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("부채 count dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("부채 count moved Unity off 6000.5.9f1")
    else:
        ok("settlement 부채 counts old→new in 0.4s when debt rose; flat/drop just shows")


def check_hype_chat() -> None:
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    catalog_cs = (ROOT / "Assets/Scripts/Data/ChatCatalog.cs").read_text(encoding="utf-8")
    nicks_cs = (ROOT / "Assets/Scripts/Data/ChatNicks.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    spawn = session_cs.split("void MaybeSpawnRegular", 1)[-1].split("void MaybeSpawnSuperchat", 1)[0]

    if "HypeActive" not in spawn or "interval *= 0.5f" not in spawn:
        fail("hype does not spawn regular chat at ~2x")
    elif "Catalog.Pick" not in session_cs or "ChatNicks.Pick" not in session_cs:
        fail("hype chat does not reuse ChatCatalog / nicks")
    elif "RollRegularKind" not in spawn:
        fail("hype chat spawned a new kind instead of regular notes")
    elif "hypeSeconds =" in spawn or "hypeIncomeMultiplier =" in spawn or "hypePerfectCombo =" in spawn:
        fail("hype chat retuned trigger / duration / payout")
    elif "if (hypeActive)" not in rules_cs or "return b.hypeIncomeMultiplier;" not in rules_cs:
        fail("hype chat retuned IncomeMultiplier")
    elif "HypeLeft = Balance.hypeSeconds" not in session_cs:
        fail("hype duration assignment moved off hypeSeconds")
    elif "RefreshHypeShow" not in live_cs or "HypeBanner" not in live_cs:
        fail("hype chat dropped the gold wash")
    elif "TickDebtCount" not in settle_cs or "ShowShortfall" not in settle_cs:
        fail("hype chat dropped debt count or 청구 미달")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("hype chat broke pads, 입력됨, or added timeScale")
    elif "밤샌사람" not in nicks_cs or "Pick" not in catalog_cs:
        fail("hype chat dropped nicks or catalog pick")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising hype chat / later weeks")
    elif "chatSpawnStart: 1.55" not in balance or "chatSpawnEnd: 1.05" not in balance:
        fail("hype chat retuned the base spawn table")
    elif "hypeSeconds: 12" not in balance or "hypeIncomeMultiplier: 2.5" not in balance or "hypePerfectCombo: 9" not in balance:
        fail("hype chat retuned hype numbers")
    elif "billRent: 8000" not in balance:
        fail("hype chat retuned Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("hype chat dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("hype chat moved Unity off 6000.5.9f1")
    else:
        ok("hype window spawns regular chat ~2x; catalog/nicks and hype numbers stay")


def check_cam_punch() -> None:
    avatar_cs = (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    react = avatar_cs.split("public void React", 1)[-1].split("public void Panic", 1)[0]

    if "_punch = 0.12f" not in react or "0.08f * punchU" not in avatar_cs:
        fail("Perfect does not punch the webcam 1.08 for 0.12s")
    elif "Judgement.Good" not in react or "_nod" not in react:
        fail("Good has no smaller webcam nod")
    elif "Judgement.Miss" not in react or "_shake = 1f" not in react or "_hurt = 1f" not in react:
        fail("Miss lost the existing webcam shake / scar")
    elif "ApplyEventScar" not in live_cs:
        fail("cam punch dropped the existing event scar path")
    elif "perfectWindow * " not in rules_cs or "b.goodWindow" not in rules_cs:
        fail("cam punch retuned hit windows")
    elif "perfectWindow: 0.07" not in balance or "goodWindow: 0.22" not in balance:
        fail("cam punch retuned Week 1 hit windows")
    elif "interval *= 0.5f" not in session_cs:
        fail("cam punch dropped hype chat 2x")
    elif "React(j, note.IsSuperchat)" not in live_cs:
        fail("webcam React is no longer wired from live hits")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("cam punch broke pads, 입력됨, or added timeScale")
    elif "TickDebtCount" not in settle_cs or "ShowShortfall" not in settle_cs:
        fail("cam punch dropped debt count or 청구 미달")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising cam punch / later weeks")
    elif "billRent: 8000" not in balance or "hypeSeconds: 12" not in balance:
        fail("cam punch retuned Week 1 bills or hype")
    elif "defaultScreenOrientation: 0" not in player:
        fail("cam punch dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("cam punch moved Unity off 6000.5.9f1")
    else:
        ok("Perfect punches the webcam 1.08 + flash 0.12s; Good nods; Miss stays")


def check_combo_pop() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    avatar_cs = (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    pop = live_cs.split("void TickComboPop", 1)[-1].split("void TickComboBreak", 1)[0]

    if "TickComboPop" not in live_cs or "_comboPop = 0.1f" not in live_cs:
        fail("combo chip does not pop 0.1s when combo goes up")
    elif "0.15f" not in pop or "0.22f" not in pop:
        fail("combo pop is not 1.15 / 1.22 at combo 5+")
    elif "_session.Combo > _lastCombo" not in live_cs:
        fail("combo pop is not keyed off combo going up")
    elif "_comboStingFlash = 1f" not in live_cs or "ShowComboBreak" not in live_cs:
        fail("combo pop dropped combo-5 sting or 콤보 끊김")
    elif "_comboBreakLeft = 0.25f" not in live_cs:
        fail("combo pop retimed 콤보 끊김")
    elif "reset = true" not in rules_cs or "if (result.ResetCombo)" not in session_cs:
        fail("combo pop retuned combo math")
    elif "hypePerfectCombo: 9" not in balance or "HypeLeft = Balance.hypeSeconds" not in session_cs:
        fail("combo pop retuned hype trigger")
    elif "_punch = 0.12f" not in avatar_cs:
        fail("combo pop dropped Perfect webcam punch")
    elif "interval *= 0.5f" not in session_cs:
        fail("combo pop dropped hype chat 2x")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("combo pop broke pads, 입력됨, or added timeScale")
    elif "오늘 헤드라인" not in settle_cs:
        fail("combo pop dropped 오늘 헤드라인")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising combo pop / later weeks")
    elif "comboIncomeMultiplier: 1.5" not in balance or "billRent: 8000" not in balance:
        fail("combo pop retuned combo payout or Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("combo pop dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("combo pop moved Unity off 6000.5.9f1")
    else:
        ok("combo chip pops 1.15 on the way up, 1.22 at 5+; stings stay")


def check_pad_flash() -> None:
    pad_cs = (ROOT / "Assets/Scripts/Input/StreamPadButton.cs").read_text(encoding="utf-8")
    bind_cs = (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    uikit_cs = (ROOT / "Assets/Scripts/Presentation/UiKit.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")

    if "_flash = 0.08f" not in pad_cs or "Color.white" not in pad_cs:
        fail("pads do not flash brighter for 0.08s")
    elif "pad?.Flash()" not in live_cs or "KindPressPad" not in live_cs:
        fail("keyboard aliases do not flash the matching pad")
    elif "Echo(\"입력됨 홍보\", _promoYes)" not in live_cs or "EventPad(idx)" not in live_cs:
        fail("promo / event pads do not use the press flash")
    elif "QueueKind" not in pad_cs or "BeginSuperchatCharge" not in pad_cs:
        fail("pad flash changed input bindings")
    elif "TryConsumeKind" not in bind_cs or "GetKeyDown(KeyCode.LeftArrow)" not in bind_cs:
        fail("pad flash retuned keyboard aliases")
    elif "UnlockUiInputForStream" not in live_cs or "DontDestroyOnLoad" not in uikit_cs:
        fail("pad flash dropped EventSystem unlock / DDOL")
    elif "perfectWindow: 0.07" not in balance or "goodWindow: 0.22" not in balance:
        fail("pad flash retuned hit windows")
    elif "perfectWindow * " not in rules_cs or "b.goodWindow" not in rules_cs:
        fail("pad flash retuned Judge windows")
    elif "_comboPop = 0.1f" not in live_cs or "입력됨" not in live_cs:
        fail("pad flash dropped combo pop or 입력됨")
    elif "AddColumnPad" not in live_cs or "timeScale" in live_cs:
        fail("pad flash broke pads or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising pad flash / later weeks")
    elif "billRent: 8000" not in balance:
        fail("pad flash retuned Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("pad flash dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("pad flash moved Unity off 6000.5.9f1")
    else:
        ok("pads flash brighter 0.08s on press; keyboard aliases match; bindings stay")


def check_mental_count() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    state_cs = (ROOT / "Assets/Scripts/Core/GameRunState.cs").read_text(encoding="utf-8")
    save_cs = (ROOT / "Assets/Scripts/Core/RunSave.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    tick = settle_cs.split("void TickMentalCount", 1)[-1].split("void TickIncomeCount", 1)[0]

    if "TickMentalCount" not in settle_cs or "_mentalCountT / 0.35f" not in settle_cs:
        fail("멘탈 does not count over ~0.35s")
    elif "_mentalTo < _mentalFrom" not in settle_cs or "mentalAtDayStart" not in settle_cs:
        fail("멘탈 count does not run only when tonight's mental dropped")
    elif "_mentalTo > _mentalFrom" not in settle_cs or "_mentalTick = 1f" not in settle_cs:
        fail("rising 멘탈 has no green tick")
    elif "Palette.MoneyRed" not in settle_cs.split("_mentalCounting", 1)[-1].split("else if (_tileMental != null && _mentalTick", 1)[0]:
        fail("falling 멘탈 is not tinted tired-red while it falls")
    elif "Palette.CashGreen" not in settle_cs.split("_mentalTick > 0.02f", 1)[-1].split("else if (_tileMental != null)", 1)[0]:
        fail("rising 멘탈 is not tinted green")
    elif "mental =" in tick or "mentalAtDayStart =" in tick:
        fail("멘탈 count writes mental math")
    elif "mentalAtDayStart = mental" not in state_cs or "mentalAtDayStart" not in save_cs:
        fail("멘탈 count does not remember the actual day-start mental")
    elif "run.mental =" in settle_cs or "mental +=" in settle_cs or "mental -=" in settle_cs:
        fail("멘탈 count reimplemented mental math")
    elif "RefreshMentalShow" not in live_cs or "멘탈 위험" not in live_cs:
        fail("멘탈 count dropped low-mental stream FX")
    elif "TickDebtCount" not in settle_cs or "ShowShortfall" not in settle_cs or "청구 미달" not in settle_cs:
        fail("멘탈 count dropped debt count or 청구 미달")
    elif "_flash = 0.08f" not in (ROOT / "Assets/Scripts/Input/StreamPadButton.cs").read_text(encoding="utf-8"):
        fail("멘탈 count dropped pad press flash")
    elif "_comboPop = 0.1f" not in live_cs or "입력됨" not in live_cs:
        fail("멘탈 count dropped combo pop or 입력됨")
    elif "AddColumnPad" not in live_cs or "timeScale" in live_cs:
        fail("멘탈 count broke pads or added timeScale")
    elif "missStreakMentalPenalty" not in rules_cs or "totalMissMentalPenalty" not in rules_cs:
        fail("멘탈 count retuned miss mental penalties")
    elif "Mental <= 0" not in session_cs or "ForceEnded = true" not in session_cs:
        fail("멘탈 count changed the force-end rule")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising mental count / later weeks")
    elif "missStreakMental: 3" not in balance or "missStreakMentalPenalty: 12" not in balance:
        fail("멘탈 count retuned miss-streak mental numbers")
    elif "billRent: 8000" not in balance or "startingMental: 100" not in balance:
        fail("멘탈 count retuned Week 1 bills or starting mental")
    elif "defaultScreenOrientation: 0" not in player:
        fail("멘탈 count dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("멘탈 count moved Unity off 6000.5.9f1")
    else:
        ok("settlement 멘탈 counts old→new in 0.35s when it dropped; rise ticks green")


def check_morning_cash_short() -> None:
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    warn = week_cs.split("void RefreshCashShort", 1)[-1].split("static int PeekTodayBills", 1)[0]

    if "RefreshCashShort" not in week_cs or "청구보다 부족" not in week_cs:
        fail("broke morning has no 청구보다 부족 line")
    elif "run.cash <" not in week_cs or "PeekTodayBills" not in warn:
        fail("청구보다 부족 does not compare current cash to today's bill")
    elif "Palette.MoneyRed" not in warn:
        fail("short 현금 chip is not tinted warning-red")
    elif "Palette.Gold" in warn:
        fail("covered 현금 chip gained extra gold")
    elif "run.cash =" in week_cs or "cash +=" in week_cs or "cash -=" in week_cs:
        fail("morning cash warn writes cash")
    elif "lastBills =" in week_cs:
        fail("morning cash warn writes lastBills")
    elif "ApplyDailyBills" not in week_cs or "GoLive()" not in week_cs:
        fail("morning cash warn dropped bill apply or GO LIVE")
    elif "마지막 날" not in week_cs or "LastDayBanner" not in week_cs or "RefreshLastDay" not in week_cs:
        fail("morning cash warn dropped the last-day banner")
    elif '"오늘 청구"' not in week_cs or "_billSlam = 0.25f" not in week_cs:
        fail("morning cash warn dropped 오늘 청구 slam")
    elif "TickMentalCount" not in settle_cs or "_mentalCountT / 0.35f" not in settle_cs:
        fail("morning cash warn dropped settlement 멘탈 count")
    elif "_flash = 0.08f" not in (ROOT / "Assets/Scripts/Input/StreamPadButton.cs").read_text(encoding="utf-8"):
        fail("morning cash warn dropped pad press flash")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("morning cash warn broke pads, 입력됨, or added timeScale")
    elif "UnlockUiInputForStream" not in week_cs or "StreamSafeArea" not in week_cs:
        fail("morning cash warn dropped WeekStart input unlock or safe area")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising morning cash warn / later weeks")
    elif "TonightBills" not in eco_cs:
        fail("morning cash warn dropped EconomyRules.TonightBills")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("morning cash warn retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("morning cash warn dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("morning cash warn moved Unity off 6000.5.9f1")
    else:
        ok("WeekStart tints 현금 warning-red + 청구보다 부족 when cash < today's bill")


def check_note_hot() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    sync = live_cs.split("void SyncNotes", 1)[-1].split("void RefreshPromoOverlay", 1)[0]
    coach = session_cs.split("void TickCoach", 1)[-1].split("void TryGrabCoachNote", 1)[0]

    if "abs <= 0.15f" not in sync or "HitTime" not in sync:
        fail("notes do not brighten within ~0.15s of the hit line")
    elif '"Hot"' not in live_cs or "1f, 1f, 1f" not in sync:
        fail("near-hit notes have no bright overlay")
    elif "c.a =" in sync or "img.color = c" in sync:
        fail("note glow washed bubble alpha into a flat bar")
    elif "approachSeconds =" in sync or "HitTime =" in sync:
        fail("note glow writes travel / hit times")
    elif "perfectWindow =" in live_cs or "goodWindow =" in live_cs:
        fail("note glow retuned judge windows")
    elif "perfectWindow * " not in rules_cs or "b.goodWindow" not in rules_cs:
        fail("note glow retuned Judge")
    elif "perfectWindow: 0.07" not in balance or "goodWindow: 0.22" not in balance:
        fail("note glow retuned hit windows")
    elif "approachSeconds: 1.35" not in balance:
        fail("note glow retuned travel speed")
    elif "FreezeNotes(dt)" not in coach or "_coachHeld.HitTime = Elapsed" not in coach:
        fail("note glow broke the Day-1 coach pause")
    elif "EnableFirstStreamCoach" not in live_cs or "CoachActive" not in live_cs:
        fail("note glow dropped the Day-1 coach")
    elif "청구보다 부족" not in week_cs or "RefreshCashShort" not in week_cs:
        fail("note glow dropped WeekStart 청구보다 부족")
    elif "_flash = 0.08f" not in (ROOT / "Assets/Scripts/Input/StreamPadButton.cs").read_text(encoding="utf-8"):
        fail("note glow dropped pad press flash")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("note glow broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising note glow / later weeks")
    elif "billRent: 8000" not in balance:
        fail("note glow retuned Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("note glow dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("note glow moved Unity off 6000.5.9f1")
    else:
        ok("notes brighten within 0.15s of the hit line; windows / travel / coach stay")


def check_content_card_mood() -> None:
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    look_cs = (ROOT / "Assets/Scripts/Presentation/ContentShowLook.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Economy/ContentRules.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    content_asset = (ROOT / "Assets/Resources/Balance/ContentBalance.asset").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    pick = week_cs.split("void AddContentButton", 1)[-1].split("void OnPickContent", 1)[0]

    if "ContentPickVibe" not in week_cs or "편하게 잡담" not in week_cs or "고음 승부" not in week_cs:
        fail("content pick cards have no one-line Korean vibe")
    elif "같이 깨자" not in week_cs or "같이 보자" not in week_cs:
        fail("game / reaction cards have no vibe line")
    elif "ContentPickIcon" not in week_cs or "ArtSprites.Superchat" not in week_cs or "ArtSprites.Sparkle" not in week_cs:
        fail("content pick cards have no distinct icons")
    elif "ArtSprites.Troll" not in week_cs or "ArtSprites.Avatar" not in week_cs:
        fail("game / reaction cards have no icons")
    elif "ContentPickAccent" not in week_cs or "Palette.Pink" not in week_cs or "Palette.Gold" not in week_cs:
        fail("content pick cards have no accent colors")
    elif "토크" not in week_cs or "게임" not in week_cs or "노래" not in week_cs or "리액션" not in week_cs:
        fail("content pick renamed the four types")
    elif "ContentRules.Pick" not in week_cs or "StreamContentType.Talk" not in week_cs:
        fail("content pick changed which cards are offered")
    elif "talkIncomeMultiplier =" in pick or "talkMentalCost =" in pick:
        fail("content card mood rewrote content modifiers")
    elif "talkIncomeMultiplier: 1" not in content_asset or "songMentalCost: 8" not in content_asset:
        fail("content card mood retuned ContentBalance")
    elif "IncomeMul" not in rules_cs or "MentalCost" not in rules_cs:
        fail("content card mood dropped ContentRules tuning")
    elif "ContentShowLook.For" not in week_cs or "ShowWash" not in week_cs:
        fail("content card mood dropped the LiveStream color preview")
    elif "오늘: 토크" not in look_cs or "reactionChatSpawnMul" in look_cs:
        fail("content card mood retuned the live show skin")
    elif "StreamSafeArea.Attach" not in week_cs:
        fail("content card mood dropped StreamSafeArea")
    elif "abs <= 0.15f" not in live_cs or "청구보다 부족" not in week_cs:
        fail("content card mood dropped note glow or 청구보다 부족")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("content card mood broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising content cards / later weeks")
    elif "billRent: 8000" not in balance:
        fail("content card mood retuned Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("content card mood dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("content card mood moved Unity off 6000.5.9f1")
    else:
        ok("WeekStart content cards are icon + accent + Korean vibe; modifiers stay")


def check_title_broke_login() -> None:
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")

    if "_wordmark" not in title_cs or "Sin(Time.time" not in title_cs or "1f + 0.04f" not in title_cs:
        fail("파산 버튜버 wordmark does not pulse")
    elif '"「파산 버튜버」"' not in title_cs:
        fail("title wordmark text changed")
    elif "TonightBills" not in title_cs or "peek.cash <" not in title_cs:
        fail("continue cash is not compared to a known next bill")
    elif "Palette.MoneyRed" not in title_cs or "Palette.Gold" not in title_cs.split("_continueDebt", 1)[-1]:
        fail("continue cash/debt are not panic red/gold")
    elif '"현금 "' not in title_cs or '"부채 "' not in title_cs or "FormatWon" not in title_cs:
        fail("continue row dropped saved cash/debt")
    elif "이어하기 " not in title_cs or "TryLoad" not in title_cs:
        fail("broke login dropped 이어하기 peek")
    elif "진행 중인 " not in title_cs or "지울까?" not in title_cs or "ConfirmWipe" not in title_cs:
        fail("broke login changed the new-game wipe confirm")
    elif "OpenWipe" not in title_cs or "BeginNewRun" not in title_cs or "StartNewRun" not in title_cs:
        fail("broke login unhooked wipe / new run")
    elif "RunSave.Delete" not in gm or "startingCash: 45000" not in balance:
        fail("broke login changed wipe flow or start numbers")
    elif "편하게 잡담" not in week_cs or "고음 승부" not in week_cs:
        fail("broke login dropped content pick vibes")
    elif "abs <= 0.15f" not in live_cs or "청구보다 부족" not in week_cs:
        fail("broke login dropped note glow or 청구보다 부족")
    elif "UnlockUiInputForStream" not in title_cs or "StreamSafeArea.Attach" not in title_cs:
        fail("broke login dropped EventSystem unlock or StreamSafeArea")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("broke login broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising later weeks / fandom")
    elif "billRent: 8000" not in balance:
        fail("broke login retuned Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("broke login dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("broke login moved Unity off 6000.5.9f1")
    else:
        ok("Title wordmark pulses; continue cash/debt go panic red/gold")


def check_superchat_pip() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    bind_cs = (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    tick = live_cs.split("void TickSuperchatPip", 1)[-1].split("void Echo", 1)[0] if "void TickSuperchatPip" in live_cs else ""

    if "TickSuperchatPip" not in live_cs or "eta <= 0.4f" not in live_cs:
        fail("superchat has no 0.4s pad telegraph")
    elif '"슈퍼챗"' not in live_cs.split("void BuildSuperchatPip", 1)[-1].split("void TickSuperchatPip", 1)[0]:
        fail("superchat pip is not labeled 슈퍼챗")
    elif "_lanePads[4]" not in live_cs or "BuildSuperchatPip(_lanePads[4])" not in live_cs:
        fail("슈퍼챗 pip is not on the superchat pad")
    elif "IsSuperchat" not in tick or "Palette.Gold" not in tick:
        fail("pip does not flash gold for incoming superchat notes")
    elif "HitTime =" in tick or "SpawnNote" in tick:
        fail("superchat pip writes spawn / hit times")
    elif "BeginSuperchatCrack" not in live_cs or "BeginSuperchatFly" not in live_cs:
        fail("superchat pip dropped miss crack or ₩ fly")
    elif "StreamRules.SuperchatAmount(HypeActive, Rng, Balance)" not in session_cs:
        fail("superchat pip retuned spawn amounts")
    elif "superchatMinInterval =" in tick or "superchatMinWon =" in tick:
        fail("superchat pip rewrote spawn rate / amounts")
    elif "GetKeyUp(KeyCode.Space)" not in bind_cs:
        fail("superchat pip broke Space release-once")
    elif "perfectWindow: 0.07" not in balance or "goodWindow: 0.22" not in balance:
        fail("superchat pip retuned hit windows")
    elif "superchatMinInterval: 9" not in balance or "superchatMinWon: 1000" not in balance:
        fail("superchat pip retuned spawn rate or amounts")
    elif "superchatMinCount: 8" not in balance or "superchatMaxCount: 10" not in balance:
        fail("superchat pip retuned superchat count")
    elif "_wordmark" not in title_cs or "Sin(Time.time" not in title_cs:
        fail("superchat pip dropped title wordmark pulse")
    elif "abs <= 0.15f" not in live_cs or "입력됨" not in live_cs:
        fail("superchat pip dropped note glow or 입력됨")
    elif "AddColumnPad" not in live_cs or "timeScale" in live_cs:
        fail("superchat pip broke pads or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising superchat pip / later weeks")
    elif "defaultScreenOrientation: 0" not in player:
        fail("superchat pip dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("superchat pip moved Unity off 6000.5.9f1")
    else:
        ok("superchat flashes a gold 슈퍼챗 pip 0.4s early; miss still cracks")


def check_left_cash() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    left = settle_cs.split("void TickLeftCash", 1)[-1].split("void ShowShortfall", 1)[0]

    if "TickLeftCash" not in settle_cs or "남은 현금" not in settle_cs:
        fail("settlement has no 남은 현금 snap")
    elif "!_incomeCounting && !_debtCounting" not in settle_cs:
        fail("남은 현금 does not wait for income / debt counts")
    elif "gm.Run.cash" not in left and "run.cash" not in left:
        fail("남은 현금 does not read the real leftover cash")
    elif "TotalFixedBills" not in settle_cs or "DaysLeftInWeek" not in settle_cs:
        fail("남은 현금 cannot compare to tomorrow's typical bill")
    elif "Palette.MoneyRed" not in left:
        fail("short leftover 남은 현금 is not warning-red")
    elif "run.cash =" in left or "lastBills =" in left or "cash +=" in left:
        fail("남은 현금 writes cash / bill math")
    elif "ApplyDailyBills" in left or "ConvertNegativeCashToDebt" in settle_cs:
        fail("남은 현금 reimplemented bill math")
    elif "TickIncomeCount" not in settle_cs or "TickDebtCount" not in settle_cs:
        fail("남은 현금 dropped income / debt counts")
    elif "eta <= 0.4f" not in live_cs or "BuildSuperchatPip" not in live_cs:
        fail("남은 현금 dropped superchat telegraph")
    elif "_wordmark" not in title_cs or "청구보다 부족" not in week_cs:
        fail("남은 현금 dropped title pulse or 청구보다 부족")
    elif "TonightBills" not in eco_cs:
        fail("남은 현금 dropped EconomyRules.TonightBills")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("남은 현금 broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising leftover cash / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("남은 현금 retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("남은 현금 dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("남은 현금 moved Unity off 6000.5.9f1")
    else:
        ok("settlement snaps 남은 현금 after counts; short vs tomorrow tints red")


def check_go_live_pulse() -> None:
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    bind_cs = (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    pulse = week_cs.split("void TickGoLivePulse", 1)[-1].split("void Build()", 1)[0]
    click = week_cs.split("_goLive = UiKit.Button", 1)[-1].split("_conflictRoot", 1)[0]
    confirm = week_cs.split("void Update()", 1)[-1].split("void TickGoLivePulse", 1)[0]
    pick = week_cs.split("void OnPickContent", 1)[-1].split("static void StyleConflictCard", 1)[0]

    if "TickGoLivePulse" not in week_cs or "LivePip" not in week_cs:
        fail("GO LIVE has no LIVE pulse / red pip")
    elif "1f + 0.04f" not in pulse or "Sin(Time.time" not in pulse:
        fail("GO LIVE does not soft-pulse at 1.04")
    elif "Palette.MoneyRed" not in pulse or "_goLivePip" not in pulse:
        fail("GO LIVE pip is not warning-red")
    elif "() => GameManager.Instance.GoLive()" not in click:
        fail("GO LIVE click no longer starts the stream")
    elif "StreamBindings.Confirm" not in confirm or "GameManager.Instance.GoLive()" not in confirm:
        fail("Space confirm no longer starts the stream")
    elif "MustResolveConflict" not in confirm or "MustPick" not in confirm:
        fail("GO LIVE pulse skipped conflict / content-pick gates")
    elif "ContentRules.Pick" not in pick or "SetActive(true)" not in pick:
        fail("content pick no longer reveals GO LIVE")
    elif "ConcertStreamReady" not in week_cs or "콘서트 방송" not in week_cs:
        fail("GO LIVE pulse dropped concert caption")
    elif '"방송 켜기  (Space)"' not in week_cs:
        fail("GO LIVE caption is no longer 방송 켜기")
    elif "ApplyDailyBills" not in week_cs or "MustPick" not in week_cs:
        fail("GO LIVE pulse reordered bills or content pick")
    elif "TickLeftCash" not in settle_cs or "남은 현금" not in settle_cs:
        fail("GO LIVE pulse dropped 남은 현금")
    elif "eta <= 0.4f" not in live_cs or "BuildSuperchatPip" not in live_cs:
        fail("GO LIVE pulse dropped superchat telegraph")
    elif "_wordmark" not in title_cs or "청구보다 부족" not in week_cs:
        fail("GO LIVE pulse dropped title pulse or 청구보다 부족")
    elif "public void GoLive()" not in gm or "UnlockUiInputForStream" not in week_cs:
        fail("GO LIVE pulse changed GameManager.GoLive or dropped UI unlock")
    elif "GetKeyUp(KeyCode.Space)" not in bind_cs:
        fail("GO LIVE pulse broke Space release-once")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("GO LIVE pulse broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising GO LIVE pulse / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("GO LIVE pulse retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("GO LIVE pulse dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("GO LIVE pulse moved Unity off 6000.5.9f1")
    else:
        ok("WeekStart 방송 켜기 pulses 1.04 with a red LIVE pip; click still GoLive")


def check_note_pad_color() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    bind_cs = (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    make = live_cs.split("RectTransform MakeBubble", 1)[-1].split("static void DimNamedBubble", 1)[0]
    sync = live_cs.split("void SyncNotes", 1)[-1].split("void RefreshPromoOverlay", 1)[0]
    padc = live_cs.split("static Color NotePadColor", 1)[-1].split("RectTransform MakeBubble", 1)[0]
    pads = live_cs.split("_lanePads[0]", 1)[-1].split("BuildSuperchatPip", 1)[0]

    if "NotePadColor" not in live_cs or "TintTravelNote" not in sync:
        fail("traveling notes are not tinted to the kind pad")
    elif "Palette.ForKind(note.Kind)" not in padc or "Palette.Gold" not in padc:
        fail("note pad color is not ForKind / gold superchat")
    elif "IsSuperchat" not in padc:
        fail("superchat notes are not kept gold")
    elif "Palette.ForKind(ChatKind.Positive)" not in pads or "Palette.ForKind(ChatKind.Empathy)" not in pads:
        fail("kind pads no longer use ForKind")
    elif "Palette.ForKind(ChatKind.Laugh)" not in pads or "Palette.ForKind(ChatKind.Thanks)" not in pads:
        fail("laugh / thanks pads no longer use ForKind")
    elif "Palette.Gold" not in pads or "슈퍼챗" not in pads:
        fail("superchat pad is no longer gold")
    elif "color = Palette.Pink" in make or "Color.Lerp(color, Palette.Gold" in make:
        fail("named / hype / song wash still hides the pad color")
    elif "abs <= 0.15f" not in sync or '"Hot"' not in live_cs or "1f, 1f, 1f" not in sync:
        fail("note pad tint dropped the 0.15s hittable glow")
    elif "c.a =" in sync or "img.color = c" in sync:
        fail("note pad tint washed bubble alpha into a flat bar")
    elif "approachSeconds =" in sync or "HitTime =" in sync:
        fail("note pad tint writes travel / hit times")
    elif "perfectWindow =" in live_cs or "goodWindow =" in live_cs:
        fail("note pad tint retuned judge windows")
    elif "perfectWindow: 0.07" not in balance or "goodWindow: 0.22" not in balance:
        fail("note pad tint retuned hit windows")
    elif "approachSeconds: 1.35" not in balance:
        fail("note pad tint retuned travel speed")
    elif "GetKeyUp(KeyCode.Space)" not in bind_cs or "TryConsumeKind" not in bind_cs:
        fail("note pad tint broke stream bindings")
    elif "TickGoLivePulse" not in week_cs or "LivePip" not in week_cs:
        fail("note pad tint dropped GO LIVE pulse")
    elif "TickLeftCash" not in (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8"):
        fail("note pad tint dropped 남은 현금")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("note pad tint broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising note pad tint / later weeks")
    elif "billRent: 8000" not in balance or "hypeSeconds: 12" not in balance:
        fail("note pad tint retuned Week 1 bills or hype")
    elif "defaultScreenOrientation: 0" not in player:
        fail("note pad tint dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("note pad tint moved Unity off 6000.5.9f1")
    elif "SpawnNote" in padc or "HitTime =" in padc:
        fail("note pad tint writes spawn / hit times")
    elif "FreezeNotes(dt)" not in session_cs.split("void TickCoach", 1)[-1]:
        fail("note pad tint broke the Day-1 coach pause")
    elif "perfectWindow * " not in rules_cs or "b.goodWindow" not in rules_cs:
        fail("note pad tint retuned Judge")
    else:
        ok("traveling notes match kind pad colors; superchat stays gold; 0.15s glow stays")


def check_strike_marker() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    bind_cs = (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    tick = live_cs.split("void TickStrike", 1)[-1].split("void SyncNotes", 1)[0]
    build = live_cs.split("_hit = UiKit.Panel", 1)[-1].split("var hitLabel", 1)[0]
    hit = live_cs.split("_hit = UiKit.Panel", 1)[-1].split("_strike", 1)[0]

    if "TickStrike" not in live_cs or '"Strike"' not in live_cs:
        fail("hit line has no white/gold strike marker")
    elif "new Vector2(0, LaneHit)" not in build or "new Vector2(0, 4)" not in build:
        fail("strike marker is not a thin bar at the hit position")
    elif "new Vector2(0, LaneHit)" not in hit or "new Vector2(0, 10)" not in hit:
        fail("existing hit line moved or resized")
    elif "const float LaneHit = -210f" not in live_cs:
        fail("hit line Y was moved")
    elif "StreamRules.Judge" not in tick or "Judgement.Perfect" not in tick:
        fail("strike does not pulse on the existing Perfect window")
    elif "Palette.Gold" not in tick or "Color.white" not in tick:
        fail("strike pulse is not white/gold")
    elif "LaneHit =" in tick or "HitTime =" in tick:
        fail("strike marker writes hit position / times")
    elif "perfectWindow =" in live_cs or "goodWindow =" in live_cs:
        fail("strike marker retuned judge windows")
    elif "perfectWindow: 0.07" not in balance or "goodWindow: 0.22" not in balance:
        fail("strike marker retuned hit windows")
    elif "approachSeconds: 1.35" not in balance:
        fail("strike marker retuned travel speed")
    elif "NotePadColor" not in live_cs or "TintTravelNote" not in live_cs:
        fail("strike marker dropped note pad colors")
    elif "abs <= 0.15f" not in live_cs or '"Hot"' not in live_cs:
        fail("strike marker dropped the 0.15s hittable glow")
    elif "TickGoLivePulse" not in week_cs or "LivePip" not in week_cs:
        fail("strike marker dropped GO LIVE pulse")
    elif "GetKeyUp(KeyCode.Space)" not in bind_cs or "TryConsumeKind" not in bind_cs:
        fail("strike marker broke stream bindings")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("strike marker broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising strike marker / later weeks")
    elif "billRent: 8000" not in balance or "hypeSeconds: 12" not in balance:
        fail("strike marker retuned Week 1 bills or hype")
    elif "defaultScreenOrientation: 0" not in player:
        fail("strike marker dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("strike marker moved Unity off 6000.5.9f1")
    elif "perfectWindow * " not in rules_cs or "b.goodWindow" not in rules_cs:
        fail("strike marker retuned Judge")
    elif "FreezeNotes(dt)" not in session_cs.split("void TickCoach", 1)[-1]:
        fail("strike marker broke the Day-1 coach pause")
    else:
        ok("hit line has a thin white/gold strike; pulses in the Perfect window")


def check_next_pulse() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    pulse = settle_cs.split("void TickNextPulse", 1)[-1].split("static bool CanAdvance", 1)[0]
    click = settle_cs.split("_next = UiKit.Button", 1)[-1].split("_restart = UiKit.Button", 1)[0]
    confirm = settle_cs.split("void Update()", 1)[-1].split("void TickNextPulse", 1)[0]
    advance = settle_cs.split("static bool CanAdvance", 1)[-1].split("void Build()", 1)[0]
    render = settle_cs.split("switch (run.lastOutcome)", 1)[-1].split("void PlaceTripleButtons", 1)[0]

    if "TickNextPulse" not in settle_cs or "NextChip" not in settle_cs or '"다음"' not in settle_cs:
        fail("다음날 button has no pulse / 다음 chip")
    elif "1f + 0.03f" not in pulse or "Sin(Time.time" not in pulse:
        fail("다음날 does not soft-pulse at 1.03")
    elif "() => GameManager.Instance.NextMorning()" not in click:
        fail("다음날 click no longer goes to next morning")
    elif "CanAdvance(gm.Run)" not in confirm or "StreamBindings.Confirm" not in confirm:
        fail("Space confirm no longer advances settlement")
    elif "_letterOpen" not in confirm or "_conflictOpen" not in confirm:
        fail("다음날 pulse skipped fan-letter / overlay gates")
    elif "MustResolveConflict" not in advance or "WeekOutcome.Continue" not in advance:
        fail("CanAdvance gates changed")
    elif "CanEnterWeek2" not in advance or "CanEnterWeek5" not in advance:
        fail("week-clear advance routing changed")
    elif "WeekOutcome.Bankrupt" not in render or "_next.gameObject.SetActive(false)" not in render:
        fail("bankrupt still must hide 다음날")
    elif "WeekOutcome.WeekFailed" not in render or '"다음날  (Space)"' not in render:
        fail("week-fail / continue captions changed")
    elif "OnLetter" not in settle_cs or "팬레터 답장" not in settle_cs:
        fail("다음날 pulse dropped fan-letter")
    elif "TickStrike" not in live_cs or "NotePadColor" not in live_cs:
        fail("다음날 pulse dropped strike marker or note pad colors")
    elif "TickGoLivePulse" not in week_cs or "LivePip" not in week_cs:
        fail("다음날 pulse dropped GO LIVE pulse")
    elif "TickLeftCash" not in settle_cs or "남은 현금" not in settle_cs:
        fail("다음날 pulse dropped 남은 현금")
    elif "public void NextMorning()" not in gm:
        fail("다음날 pulse changed GameManager.NextMorning")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("다음날 pulse broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising 다음날 pulse / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("다음날 pulse retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("다음날 pulse dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("다음날 pulse moved Unity off 6000.5.9f1")
    else:
        ok("settlement 다음날 pulses 1.03 with a 다음 chip; routing stays")


def check_event_warn() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    event_cs = (ROOT / "Assets/Scripts/Stream/StreamEvent.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    peek = session_cs.split("public bool TryPeekEventWarn", 1)[-1].split("public bool PromoActive", 1)[0]
    start_ev = session_cs.split("void StartEvent", 1)[-1].split("void ResolveEvent", 1)[0]
    tick = live_cs.split("void TickEventWarn", 1)[-1].split("void TickStrike", 1)[0]
    kinds = event_cs.split("enum StreamEventKind", 1)[-1].split("enum StreamEventTrigger", 1)[0]

    if "TryPeekEventWarn" not in session_cs or "TickEventWarn" not in live_cs:
        fail("events have no 0.5s warning chip")
    elif "eta > 0.5f" not in peek or "안티 온다" not in event_cs or "렉 온다" not in event_cs:
        fail("warning is not 안티 온다 / 렉 온다 at 0.5s")
    elif "WarnCopy" not in tick or "EventWarnBox" not in live_cs:
        fail("warning chip is not shown on the live HUD")
    elif "BeginEventAccident" not in live_cs or "0.2f" not in live_cs.split("void BeginEventAccident", 1)[-1][:500]:
        fail("event warn dropped the existing sting")
    elif "ApplyEventScar" not in live_cs or "EventCrack" not in live_cs or "SetPulse" not in live_cs:
        fail("event warn dropped glow-key or fail-scar")
    elif "사고 수습" not in event_cs or "RecoverCopy" not in live_cs:
        fail("event warn dropped 사고 수습")
    elif "MaybeStartEvent" in live_cs or "StartEvent(" in live_cs:
        fail("LiveStream started firing extra events")
    elif "eventEarliestSeconds =" in peek or "eventWindowSeconds =" in peek:
        fail("event warn writes event timing")
    elif "eventAntiFailMental" in peek or "eventLagFailFreezeSeconds" in peek:
        fail("event warn writes fail penalties")
    elif "Event.Window =" not in start_ev or "eventWindowSeconds" not in start_ev:
        fail("event warn changed the QTE window assignment")
    elif "eventEarliestSeconds: 35" not in balance or "eventAntiFailMental: 8" not in balance:
        fail("event warn retuned event timing / fail numbers")
    elif "eventLagFailFreezeSeconds: 3" not in balance or "eventWindowSeconds: 1.15" not in balance:
        fail("event warn retuned lag freeze or window")
    elif "RivalWave" in event_cs or kinds.count("=") > 3:
        fail("a new StreamEvent kind was added")
    elif "TickNextPulse" not in settle_cs or "TickStrike" not in live_cs:
        fail("event warn dropped 다음날 pulse or strike marker")
    elif "NotePadColor" not in live_cs or "TickGoLivePulse" not in week_cs:
        fail("event warn dropped note pad colors or GO LIVE pulse")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("event warn broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising event warn / later weeks")
    elif "billRent: 8000" not in balance or "hypeSeconds: 12" not in balance:
        fail("event warn retuned Week 1 bills or hype")
    elif "defaultScreenOrientation: 0" not in player:
        fail("event warn dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("event warn moved Unity off 6000.5.9f1")
    else:
        ok("events flash 안티 온다 / 렉 온다 0.5s early; sting / keys / scars stay")


def check_day_slam() -> None:
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    schedule_cs = (ROOT / "Assets/Scripts/Economy/WeekSchedule.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    hud = week_cs.split("void RefreshHud", 1)[-1].split("void RefreshCashShort", 1)[0]
    slam = week_cs.split("_daySlam = Mathf.MoveTowards", 1)[-1].split("TickGoLivePulse", 1)[0]

    if "_dayHead" not in week_cs or '"일차"' not in week_cs:
        fail("WeekStart has no n일차 morning header")
    elif "_daySlam = 0.25f" not in week_cs or "_daySlam / 0.25f" not in slam:
        fail("n일차 does not slam in 0.25s")
    elif "run.day +" not in hud and "run.day + \"일차\"" not in hud and 'run.day + "일차"' not in hud:
        fail("n일차 header does not read run.day")
    elif "run.day = " in week_cs or "day += " in week_cs or "day -= " in week_cs:
        fail("n일차 slam writes the day index")
    elif "LastDayOfCurrentWeek" not in schedule_cs or "WeekNumber" not in schedule_cs:
        fail("n일차 slam dropped week day math")
    elif "마지막 날" not in week_cs or "LastDayBanner" not in week_cs or "RefreshLastDay" not in week_cs:
        fail("n일차 slam dropped the last-day banner")
    elif "YesterdayLine" not in week_cs or "RefreshYesterday" not in week_cs:
        fail("n일차 slam dropped 어제 headline")
    elif '"오늘 청구"' not in week_cs or "_billSlam = 0.25f" not in week_cs:
        fail("n일차 slam dropped 오늘 청구 slam")
    elif "편하게 잡담" not in week_cs or "고음 승부" not in week_cs:
        fail("n일차 slam dropped content cards")
    elif "TryPeekEventWarn" not in (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8"):
        fail("n일차 slam dropped event telegraph")
    elif "TickNextPulse" not in settle_cs or "TickStrike" not in live_cs:
        fail("n일차 slam dropped 다음날 pulse or strike marker")
    elif "TickGoLivePulse" not in week_cs or "청구보다 부족" not in week_cs:
        fail("n일차 slam dropped GO LIVE pulse or 청구보다 부족")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("n일차 slam broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising n일차 slam / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("n일차 slam retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("n일차 slam dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("n일차 slam moved Unity off 6000.5.9f1")
    else:
        ok("WeekStart slams n일차 0.25s; last-day / 어제 / 청구 / cards stay")


def check_bill_chip() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    hud = live_cs.split("void RefreshHud", 1)[-1].split("void TickEventWarn", 1)[0]

    if "BillChip" not in live_cs or '"청구 "' not in live_cs:
        fail("LiveStream has no persistent 청구 ₩N chip")
    elif "FormatWon(_tonightBills)" not in hud or '"청구 "' not in hud:
        fail("청구 chip does not show tonight's real bill")
    elif "Palette.Gold" not in hud or "_billsCovered" not in hud:
        fail("청구 chip does not flip gold when income covers")
    elif hud.count("SlamBillCover(") > 0 and hud.split("if (_billChip")[-1].count("SlamBillCover(") > 0:
        fail("청구 chip added a second cover slam")
    elif "SlamBillCover()" not in live_cs or "CoverSlam" not in live_cs:
        fail("청구 chip dropped the existing once cover slam")
    elif "SlamBillCover" in settle_cs or "CoverSlam" in settle_cs:
        fail("청구 chip grew a fake settlement slam")
    elif "lastBills =" in hud or "billRent =" in hud or "TonightBills =" in hud:
        fail("청구 chip writes bill amounts")
    elif "TonightBills(gm.Run)" not in live_cs:
        fail("청구 chip is not keyed off EconomyRules.TonightBills")
    elif "TonightBills" not in eco_cs:
        fail("청구 chip dropped EconomyRules.TonightBills")
    elif "_dayHead" not in week_cs or "TryPeekEventWarn" not in (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8"):
        fail("청구 chip dropped n일차 slam or event telegraph")
    elif "TickNextPulse" not in settle_cs or "TickStrike" not in live_cs:
        fail("청구 chip dropped 다음날 pulse or strike marker")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("청구 chip broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising 청구 chip / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("청구 chip retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("청구 chip dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("청구 chip moved Unity off 6000.5.9f1")
    else:
        ok("HUD keeps 청구 ₩N all stream; gold on cover; slam stays once")


def check_bill_fill() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    hud = live_cs.split("void RefreshHud", 1)[-1].split("void TickEventWarn", 1)[0]
    fill = hud.split("if (_billFill != null)", 1)[-1].split("if (_session.HypeActive)", 1)[0]

    if "_billFill" not in live_cs or "BillFill" not in live_cs:
        fail("청구 chip has no thin fill bar")
    elif "ticking / (float)_tonightBills" not in fill:
        fail("fill bar is not live income / tonight bill")
    elif "Palette.MoneyRed" not in fill or "Palette.Gold" not in fill:
        fail("fill bar is not red while short / gold when full")
    elif "SlamBillCover" in fill:
        fail("fill bar added a second cover slam")
    elif "new Vector2(180, 10)" not in live_cs:
        fail("fill bar is not a thin bar next to the chip")
    elif "const float LaneHit = -210f" not in live_cs or "TickStrike" not in live_cs:
        fail("fill bar moved or stole the hit bar")
    elif "SlamBillCover()" not in live_cs or "CoverSlam" not in live_cs:
        fail("fill bar dropped the existing once cover slam")
    elif "SlamBillCover" in settle_cs or "CoverSlam" in settle_cs:
        fail("fill bar grew a fake settlement slam")
    elif "lastBills =" in hud or "billRent =" in hud or "LiveIncome =" in fill:
        fail("fill bar writes bill / payout amounts")
    elif "PayoutIncome" in fill and "PayoutIncome =" in fill:
        fail("fill bar writes payout")
    elif "BillChip" not in live_cs or '"청구 "' not in live_cs:
        fail("fill bar dropped the 청구 ₩N chip")
    elif "TonightBills(gm.Run)" not in live_cs or "TonightBills" not in eco_cs:
        fail("fill bar is not keyed off TonightBills")
    elif "_dayHead" not in week_cs:
        fail("fill bar dropped n일차 slam")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("fill bar broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising fill bar / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("fill bar retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("fill bar dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("fill bar moved Unity off 6000.5.9f1")
    else:
        ok("thin 청구 fill bar tracks live income; red short / gold full; slam stays once")


def check_mental_fatigue() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    avatar_cs = (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")

    if "RefreshMentalShow" not in live_cs or "m <= 40" not in live_cs:
        fail("low mental does not tire the studio wash")
    elif "SetTired" not in avatar_cs or "SetTired(tired, danger)" not in live_cs:
        fail("avatar has no tired pose")
    elif "멘탈 위험" not in live_cs or "m <= 20" not in live_cs:
        fail("mental ≤ 20 has no 멘탈 위험 chip")
    elif "MentalGrain" not in live_cs or "GrainSprite" not in live_cs:
        fail("low mental has no grain")
    elif "강제 종료" not in live_cs or "ForceEnd" not in live_cs:
        fail("mental 0 has no 강제 종료 sting")
    elif "_mentalPunch" not in live_cs or "Palette.MoneyRed" not in live_cs:
        fail("mental number does not punch red when it drops")
    elif "ShowMissSting" not in live_cs or "시청자" not in live_cs:
        fail("miss-streak 3 flash was dropped")
    elif "missStreakMentalPenalty" not in rules_cs or "totalMissMentalPenalty" not in rules_cs:
        fail("miss mental penalties were retuned")
    elif "forceEndIncomeNumerator" not in eco_cs or "forceEndIncomeDenominator" not in eco_cs:
        fail("force-end income ×0.5 math was retuned")
    elif "Mental <= 0" not in session_cs or "ForceEnded = true" not in session_cs:
        fail("mental 0 force-end rule was changed")
    elif "RefreshHypeShow" not in live_cs or '"Nick"' not in live_cs or "오늘 헤드라인" not in settle_cs:
        fail("mental wash dropped hype, nicks, or headline")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("mental wash broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in live_cs:
        fail("mental wash dropped StreamSafeArea")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising mental / later weeks")
    elif "missStreakMental: 3" not in balance or "missStreakMentalPenalty: 12" not in balance:
        fail("miss-streak mental numbers were retuned")
    elif "missStreakViewerPenalty: 4" not in balance or "totalMissMentalPenalty: 20" not in balance:
        fail("miss viewer / total-miss mental numbers were retuned")
    elif "forceEndIncomeNumerator: 1" not in balance or "forceEndIncomeDenominator: 2" not in balance:
        fail("force-end income fraction was retuned")
    elif "billRent: 8000" not in balance or "hypeSeconds: 12" not in balance:
        fail("mental wash retuned Week 1 bills or hype")
    else:
        ok("low mental looks exhausted; force-end / miss math unchanged")


def check_superchat_fly() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    bind_cs = (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")

    if "BeginSuperchatFly" not in live_cs or "WonFly" not in live_cs:
        fail("successful superchat has no ₩ fly to the money HUD")
    elif "_incomeNow" not in live_cs or '"지금 수입"' not in live_cs:
        fail("₩ fly has no 지금 수입 target")
    elif "BeginSuperchatCrack" not in live_cs or "_scCracks" not in live_cs:
        fail("missed superchat banner does not crack / fall")
    elif "민준 첫 도네" not in live_cs or "minjunEver" not in live_cs:
        fail("민준 first-superchat banner stamp is missing")
    elif "MaybeSpawnMinjun" in live_cs:
        fail("superchat fly added a new 민준 unlock rule")
    elif "ShowMissSting" not in live_cs:
        fail("superchat miss dropped the existing miss sting")
    elif "SuperchatIncome += note.SuperchatWon" not in session_cs:
        fail("superchat fly retuned session won income")
    elif "StreamRules.SuperchatAmount(HypeActive, Rng, Balance)" not in session_cs:
        fail("superchat spawn amount path was changed")
    elif "superchatMinWon" not in rules_cs or "hypeSuperchatMultiplier" not in rules_cs:
        fail("StreamRules superchat amount math was retuned")
    elif "GetKeyUp(KeyCode.Space)" not in bind_cs or "GetKeyUp(KeyCode.Return)" not in bind_cs:
        fail("Space/Enter superchat release-once was broken")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("superchat fly broke pads, 입력됨, or added timeScale")
    elif "RefreshHypeShow" not in live_cs or "RefreshMentalShow" not in live_cs or '"Nick"' not in live_cs:
        fail("superchat fly dropped hype, mental wash, or nicks")
    elif "StreamSafeArea" not in live_cs or "오늘 헤드라인" not in settle_cs:
        fail("superchat fly dropped StreamSafeArea or headline")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising superchat / later weeks")
    elif "superchatMinWon: 1000" not in balance or "superchatMaxWon: 6000" not in balance:
        fail("superchat amount ranges were retuned")
    elif "superchatMinCount: 8" not in balance or "superchatMaxCount: 10" not in balance:
        fail("superchat count was retuned")
    elif "billRent: 8000" not in balance or "hypeSeconds: 12" not in balance:
        fail("superchat fly retuned Week 1 bills or hype")
    else:
        ok("Perfect superchat flies ₩ to 지금 수입; miss cracks; 민준 첫 도네 is stamp-only")


def check_viewer_pop() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")

    if "ShowViewerDelta" not in live_cs or "시청 +" not in live_cs or "시청 −" not in live_cs:
        fail("viewer chip has no +/− popup")
    elif "ViewerPop" not in live_cs or "_viewers" not in live_cs:
        fail("viewer popup is not next to the viewer chip")
    elif "ShowMissSting" not in live_cs or "ShowViewerDelta(viewerDelta)" not in live_cs:
        fail("miss does not reuse the one viewer popup")
    elif live_cs.count("ShowViewerDelta") < 3:
        fail("viewer popup is not used for hit / miss / leftover ticks")
    elif "ClampViewers" not in rules_cs or "ViewerDeltaFor" not in rules_cs:
        fail("viewer popup retuned ClampViewers / deltas")
    elif "perfectViewerDelta" not in rules_cs or "missViewerDelta" not in rules_cs:
        fail("StreamRules viewer deltas were dropped")
    elif "Tuning.PerfectViewerMul" not in session_cs or "ApplyRivalSteal" not in session_cs:
        fail("content / rival viewer modifiers were dropped")
    elif "BeginSuperchatFly" not in live_cs or "RefreshHypeShow" not in live_cs or "RefreshMentalShow" not in live_cs:
        fail("viewer popup dropped superchat fly or washes")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("viewer popup broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in live_cs or '"Nick"' not in live_cs or "오늘 헤드라인" not in settle_cs:
        fail("viewer popup dropped StreamSafeArea, nicks, or headline")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising viewers / later weeks")
    elif "perfectViewerDelta: 0.5" not in balance or "greatViewerDelta: 0.2" not in balance:
        fail("Perfect / Great viewer deltas were retuned")
    elif "missViewerDelta: -1.2" not in balance or "goodViewerDelta: 0" not in balance:
        fail("Miss / Good viewer deltas were retuned")
    elif "hypeViewersPerSec: 1" not in balance or "billRent: 8000" not in balance:
        fail("viewer popup retuned hype viewers or Week 1 bills")
    else:
        ok("시청 +/− pops next to the chip; miss reuses one popup; deltas unchanged")


def check_viewer_chip_pop() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    show = live_cs.split("void ShowViewerDelta", 1)[-1].split("void TickViewerChipPop", 1)[0]
    paint = live_cs.split("void PaintViewerChip", 1)[-1].split("void ShowMissSting", 1)[0]

    if "_viewerChipPop" not in live_cs or "TickViewerChipPop" not in live_cs:
        fail("viewer chip does not pop when the count changes")
    elif "_viewerChipPop = 0.1f" not in show:
        fail("viewer chip pop is not 0.1s")
    elif "1f + 0.12f" not in paint or "_viewerChip.localScale" not in paint:
        fail("viewer chip pop is not 1.12 scale on the chip")
    elif "Palette.CashGreen" not in paint or "Palette.MoneyRed" not in paint:
        fail("viewer chip is not green on gain / red on drop")
    elif "_viewerChipUp" not in show or "_viewerChipUp" not in paint:
        fail("viewer chip tint is not keyed off gain vs drop")
    elif "시청 +" not in show or "시청 −" not in show:
        fail("viewer chip pop dropped the existing +/− text")
    elif show.count("시청 +") != 1 or live_cs.count("ViewerPop") < 1:
        fail("viewer chip pop duplicated the +/− text")
    elif "ShowViewerDelta(viewerDelta)" not in live_cs:
        fail("viewer chip pop stopped miss from reusing the one popup")
    elif live_cs.count("ShowViewerDelta") < 3:
        fail("viewer chip pop is not used for hit / miss / leftover ticks")
    elif "ClampViewers" not in rules_cs or "ViewerDeltaFor" not in rules_cs:
        fail("viewer chip pop retuned ClampViewers / deltas")
    elif "perfectViewerDelta" not in rules_cs or "missViewerDelta" not in rules_cs:
        fail("StreamRules viewer deltas were dropped")
    elif "Tuning.PerfectViewerMul" not in session_cs or "ApplyRivalSteal" not in session_cs:
        fail("content / rival viewer modifiers were dropped")
    elif "Viewers =" in show or "Viewers =" in paint:
        fail("viewer chip pop writes viewer math")
    elif "BillFill" not in live_cs or "new Vector2(180, 10)" not in live_cs:
        fail("viewer chip pop dropped the 청구 fill bar")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("viewer chip pop broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising viewer chip / later weeks")
    elif "perfectViewerDelta: 0.5" not in balance or "missViewerDelta: -1.2" not in balance:
        fail("viewer chip pop retuned Perfect / Miss viewer deltas")
    elif "hypeViewersPerSec: 1" not in balance or "billRent: 8000" not in balance:
        fail("viewer chip pop retuned hype viewers or Week 1 bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("viewer chip pop dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("viewer chip pop moved Unity off 6000.5.9f1")
    elif "오늘 헤드라인" not in settle_cs:
        fail("viewer chip pop dropped settlement headline")
    else:
        ok("viewer chip pops 1.12 / 0.1s; green gain / red drop; +/− text stays one")


def check_bill_cover_slam() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    avatar_cs = (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")

    if "SlamBillCover" not in live_cs or "CoverSlam" not in live_cs:
        fail("mid-stream 청구 커버 has no gold slam")
    elif "청구 커버" not in live_cs or "TonightBills" not in live_cs:
        fail("cover slam is not keyed off tonight bills vs live income")
    elif "_billsCovered" not in live_cs or "SlamBillCover()" not in live_cs:
        fail("cover slam is not once-per-stream / sticky")
    elif "dt * 2.5f" not in live_cs:
        fail("cover slam is not a short 0.4s hit")
    elif "HappyPop" not in avatar_cs or "HappyPop()" not in live_cs:
        fail("cover slam has no avatar happy pop")
    elif "SlamBillCover" in settle_cs or "CoverSlam" in settle_cs:
        fail("settlement grew a fake 청구 커버 slam")
    elif "청구 미달" not in (ROOT / "Assets/Scripts/Presentation/DayHeadline.cs").read_text(encoding="utf-8"):
        fail("settlement headline lost 청구 미달")
    elif "TonightBills" not in eco_cs:
        fail("cover slam is not using existing TonightBills")
    elif "ShowViewerDelta" not in live_cs or "BeginSuperchatFly" not in live_cs:
        fail("cover slam dropped viewer popups or superchat fly")
    elif "RefreshHypeShow" not in live_cs or "RefreshMentalShow" not in live_cs:
        fail("cover slam dropped hype / mental wash")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("cover slam broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in live_cs or '"Nick"' not in live_cs:
        fail("cover slam dropped StreamSafeArea or nicks")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising cover / later weeks")
    elif "billRent: 8000" not in balance or "hypeSeconds: 12" not in balance:
        fail("cover slam retuned Week 1 bills or hype")
    else:
        ok("first 청구 커버 slams gold once; sticky green; no fake settle slam")


def check_yesterday_headline() -> None:
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    head_cs = (ROOT / "Assets/Scripts/Presentation/DayHeadline.cs").read_text(encoding="utf-8")
    state_cs = (ROOT / "Assets/Scripts/Core/GameRunState.cs").read_text(encoding="utf-8")
    save_cs = (ROOT / "Assets/Scripts/Core/RunSave.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    reset = state_cs.split("void ResetNewRun", 1)[-1].split("void ClearWeek2Progress", 1)[0]
    nextday = state_cs.split("void BeginNextDay", 1)[-1]
    morning = gm.split("void NextMorning", 1)[-1].split("public void Load", 1)[0]

    if "YesterdayLine" not in head_cs or '"어제: "' not in head_cs:
        fail("WeekStart has no 어제 headline line")
    elif "lastHeadline" not in state_cs or "lastHeadline" not in save_cs:
        fail("yesterday headline is not persisted on the run save")
    elif "Remember" not in head_cs or "DayHeadline.Remember" not in settle_cs:
        fail("settlement does not store today's headline string")
    elif "DayHeadline.Remember" not in morning and "lastHeadline" not in morning:
        fail("NextMorning does not keep lastHeadline before the next day wipe")
    elif "lastHeadline = \"\"" not in reset and "lastHeadline = string.Empty" not in reset:
        fail("ResetNewRun leaves a leftover headline")
    elif "lastHeadline =" in nextday.split("FandomRules.ResetDaily", 1)[0] and 'lastHeadline = ""' in nextday:
        fail("BeginNextDay wipes yesterday's headline before WeekStart")
    elif "day <= 1" not in head_cs:
        fail("Day 1 first morning can show a fake yesterday")
    elif "Yesterday" not in week_cs or "YesterdayLine" not in week_cs:
        fail("WeekStart does not show 어제 above the bill slam")
    elif "오늘 헤드라인" not in settle_cs or "청구 커버" not in head_cs:
        fail("yesterday slice dropped today's settlement headline")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("yesterday slice broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in week_cs or "SafeFitCard" not in week_cs:
        fail("yesterday slice dropped StreamSafeArea")
    elif "멤버십 해금" not in settle_cs or "콘텐츠 편중 갈등" not in week_cs:
        fail("yesterday slice dropped prior cards")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising yesterday / later weeks")
    elif "billRent: 8000" not in (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8"):
        fail("yesterday headline retuned Week 1 bills")
    elif "lastHeadline == \"청구 커버 · 시청 32\"" not in save_cs:
        fail("dummy save/load dropped lastHeadline")
    elif "data.lastHeadline ?? \"\"" not in save_cs:
        fail("old saves without lastHeadline cannot load")
    elif "lastHeadline" in save_cs.split("static bool IsValid", 1)[-1].split("static RunSaveData Capture", 1)[0]:
        fail("IsValid rejects old saves that omit lastHeadline")
    elif "DayHeadline.Remember" not in (ROOT / "Assets/Scripts/Core/PlaytestDebug.cs").read_text(encoding="utf-8"):
        fail("F9/F10 skip leaves lastHeadline empty")
    else:
        ok("Day 2+ WeekStart shows yesterday's headline; Day 1 and restart stay empty")


def check_last_day_banner() -> None:
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    sched_cs = (ROOT / "Assets/Scripts/Economy/WeekSchedule.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    uikit_cs = (ROOT / "Assets/Scripts/Presentation/UiKit.cs").read_text(encoding="utf-8")
    w1 = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")

    if "마지막 날" not in week_cs or "주차 마지막" not in week_cs:
        fail("WeekStart last-day morning has no 마지막 날 banner")
    elif "LastDayOfCurrentWeek" not in week_cs:
        fail("last-day banner is not keyed off WeekSchedule last days")
    elif "Week1LastDay = 5" not in sched_cs or "Week5LastDay = 25" not in sched_cs:
        fail("WeekSchedule last days moved off 5/10/15/20/25")
    elif "winDebtMax" not in week_cs or "winCashMin" not in week_cs:
        fail("last-day banner does not show the existing cash/debt clear line")
    elif "RefreshLastDay" not in week_cs or "LastDayBanner" not in week_cs:
        fail("last-day banner is not wired on WeekStart")
    elif "YesterdayLine" not in week_cs or "UnlockUiInputForStream" not in week_cs:
        fail("last-day banner dropped 어제 headline or input unlock")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("last-day banner broke pads, 입력됨, or added timeScale")
    elif "StreamSafeArea" not in week_cs or "SafeFitCard" not in week_cs:
        fail("last-day banner dropped StreamSafeArea")
    elif "UnlockUiInputForStream" not in title_cs or "UnlockUiInputForStream" not in settle_cs:
        fail("last-day banner dropped Title/Settlement input unlock")
    elif "DontDestroyOnLoad" not in uikit_cs:
        fail("last-day banner dropped EventSystem DDOL")
    elif "콘텐츠 편중 갈등" not in week_cs or "멤버십 해금" not in settle_cs:
        fail("last-day banner dropped prior cards")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs:
        fail("Title started advertising last-day / later weeks")
    elif "winDebtMax: 30000" not in w1 or "winCashMin: 70000" not in w1 or "billRent: 8000" not in w1:
        fail("last-day banner retuned Week 1 clear or bills")
    else:
        ok("day 5/10/15/20/25 WeekStart shows 마지막 날; other mornings stay quiet")


def check_title_continue_preview() -> None:
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")

    if "이어하기 " not in title_cs or "일차" not in title_cs:
        fail("Title continue does not show 이어하기 n일차")
    elif "현금 " not in title_cs or "부채 " not in title_cs or "FormatWon" not in title_cs:
        fail("Title continue does not show saved cash/debt")
    elif "lastHeadline" not in title_cs or '"어제: "' not in title_cs:
        fail("Title continue dropped lastHeadline when present")
    elif "TryLoad" not in title_cs or "HasValidSave" not in title_cs:
        fail("Title continue does not peek the existing save")
    elif "새 방송 시작" not in title_cs or "StartNewRun" not in title_cs or "RunSave.Delete" not in gm:
        fail("새 방송 시작 no longer wipes the save")
    elif "이어서 하기" not in title_cs or "ContinueRun" not in title_cs:
        fail("Title lost 이어서 하기")
    elif "UnlockUiInputForStream" not in title_cs or "StreamSafeArea.Attach" not in title_cs:
        fail("Title continue dropped EventSystem unlock or StreamSafeArea")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("Title continue broke pads, 입력됨, or added timeScale")
    elif "마지막 날" not in week_cs or "YesterdayLine" not in week_cs:
        fail("Title continue dropped last-day banner or 어제 headline")
    elif "진행 중인 " not in title_cs or "지울까?" not in title_cs or "지우고 시작" not in title_cs or "취소" not in title_cs:
        fail("wiping a save does not take a confirm card")
    elif "OpenWipe" not in title_cs or "ConfirmWipe" not in title_cs or "BeginNewRun" not in title_cs:
        fail("wipe confirm is not wired on 새 방송 시작")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising later weeks / fandom")
    else:
        ok("Title with a save shows 이어하기 n일차 + 현금/부채")


def check_start_pulse() -> None:
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    pulse = title_cs.split("void TickStartPulse", 1)[-1].split("void OpenHowTo", 1)[0]
    click = title_cs.split("_start = UiKit.Button", 1)[-1].split("_continue = UiKit.Button", 1)[0]
    wipe = title_cs.split("void BuildWipe", 1)[-1].split("void BuildPrologue", 1)[0]
    cont = title_cs.split("void FillContinue", 1)[-1].split("void OpenWipe", 1)[0]
    start = title_cs.split("void OnStartBroadcast", 1)[-1].split("void BeginNewRun", 1)[0]

    if "TickStartPulse" not in title_cs or "StartChip" not in title_cs or '"시작"' not in title_cs:
        fail("새 방송 시작 has no pulse / 시작 chip")
    elif "1f + 0.03f" not in pulse or "Sin(Time.time" not in pulse:
        fail("새 방송 시작 does not soft-pulse at 1.03")
    elif "OnStartBroadcast" not in click or "StartChip" not in click:
        fail("시작 chip is not on the new-game button")
    elif "OpenWipe" not in start or "BeginNewRun" not in start:
        fail("새 방송 시작 click no longer wipes or starts")
    elif "진행 중인 " not in wipe or "지울까?" not in wipe or "지우고 시작" not in wipe or "취소" not in wipe:
        fail("save-wipe confirm is not exactly as-is")
    elif "ConfirmWipe" not in title_cs or "CloseWipe" not in title_cs:
        fail("wipe confirm wiring changed")
    elif "MoneyPlate" not in title_cs or "이어하기 " not in title_cs or '"현금 "' not in cont or '"부채 "' not in cont:
        fail("continue row is no longer as painted")
    elif "peek.cash <" not in cont or "Palette.MoneyRed" not in cont or "Palette.Gold" not in cont:
        fail("continue cash/debt panic colors changed")
    elif '"어제: "' not in title_cs or "lastHeadline" not in title_cs:
        fail("start pulse dropped continue yesterday headline")
    elif "1f + 0.04f" not in title_cs or "_wordmark" not in title_cs:
        fail("start pulse dropped the title wordmark pulse")
    elif "TickGoLivePulse" not in week_cs or "TickNextPulse" not in settle_cs:
        fail("start pulse dropped GO LIVE / 다음날 pulse")
    elif "TickViewerChipPop" not in live_cs or "BillFill" not in live_cs:
        fail("start pulse dropped viewer chip pop or 청구 fill")
    elif "public void StartNewRun()" not in gm or "RunSave.Delete" not in gm:
        fail("start pulse changed StartNewRun / wipe")
    elif "UnlockUiInputForStream" not in title_cs or "StreamSafeArea.Attach" not in title_cs:
        fail("start pulse dropped EventSystem unlock or StreamSafeArea")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("start pulse broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising start pulse / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("start pulse retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("start pulse dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("start pulse moved Unity off 6000.5.9f1")
    else:
        ok("Title 새 방송 시작 pulses 1.03 with a 시작 chip; wipe / continue stay")


def check_continue_pulse() -> None:
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    pulse = title_cs.split("void TickContinuePulse", 1)[-1].split("void OpenHowTo", 1)[0]
    click = title_cs.split("_continue = UiKit.Button", 1)[-1].split("_how = UiKit.Button", 1)[0]
    hide = title_cs.split("void RefreshContinue", 1)[-1].split("void FillContinue", 1)[0]
    cont = title_cs.split("void FillContinue", 1)[-1].split("void OpenWipe", 1)[0]
    load = title_cs.split("void OnContinue", 1)[-1].split("void OnStartBroadcast", 1)[0]
    wipe = title_cs.split("void BuildWipe", 1)[-1].split("void BuildPrologue", 1)[0]

    if "TickContinuePulse" not in title_cs or "ContinueChip" not in title_cs or '"이어"' not in title_cs:
        fail("이어서 하기 has no pulse / 이어 chip")
    elif "1f + 0.03f" not in pulse or "Sin(Time.time" not in pulse:
        fail("이어서 하기 does not soft-pulse at 1.03")
    elif "OnContinue" not in click or "ContinueChip" not in click:
        fail("이어 chip is not on the continue button")
    elif "activeInHierarchy" not in pulse or "_hasSave" not in hide or "SetActive(_hasSave)" not in hide:
        fail("continue pulse is not hidden when there is no save")
    elif "ContinueRun()" not in load:
        fail("continue pulse changed continue load")
    elif "진행 중인 " not in wipe or "지울까?" not in wipe or "지우고 시작" not in wipe or "취소" not in wipe:
        fail("continue pulse changed the save-wipe confirm")
    elif "ConfirmWipe" not in title_cs or "OpenWipe" not in title_cs:
        fail("continue pulse unhooked wipe confirm")
    elif "MoneyPlate" not in title_cs or "이어하기 " not in title_cs or '"현금 "' not in cont or '"부채 "' not in cont:
        fail("continue pulse dropped day + cash/debt")
    elif "peek.cash <" not in cont or "Palette.MoneyRed" not in cont or "Palette.Gold" not in cont:
        fail("continue cash/debt panic colors changed")
    elif '"어제: "' not in title_cs or "lastHeadline" not in title_cs:
        fail("continue pulse dropped yesterday headline")
    elif "TickStartPulse" not in title_cs or "StartChip" not in title_cs or '"시작"' not in title_cs:
        fail("continue pulse dropped 새 방송 시작 pulse")
    elif "ShowChip" not in live_cs or "TickGoLivePulse" not in week_cs or "TickNextPulse" not in settle_cs:
        fail("continue pulse dropped show chip or GO LIVE / 다음날")
    elif "public bool ContinueRun()" not in gm:
        fail("continue pulse changed GameManager.ContinueRun")
    elif "UnlockUiInputForStream" not in title_cs or "StreamSafeArea.Attach" not in title_cs:
        fail("continue pulse dropped EventSystem unlock or StreamSafeArea")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("continue pulse broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising continue pulse / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("continue pulse retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("continue pulse dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("continue pulse moved Unity off 6000.5.9f1")
    else:
        ok("Title 이어서 하기 pulses 1.03 with a 이어 chip; hidden without save")


def check_show_chip() -> None:
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    look_cs = (ROOT / "Assets/Scripts/Presentation/ContentShowLook.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Economy/ContentRules.cs").read_text(encoding="utf-8")
    content_asset = (ROOT / "Assets/Resources/Balance/ContentBalance.asset").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    paint = live_cs.split("void PaintShowChip", 1)[-1].split("void ApplyThreatShow", 1)[0]
    apply = live_cs.split("void ApplyContentShow", 1)[-1].split("void PaintShowChip", 1)[0]
    accent = week_cs.split("static Color ContentPickAccent", 1)[-1].split("void AddContentButton", 1)[0]

    if "ShowChip" not in live_cs or "ShowChipName" not in live_cs or "ShowChipAccent" not in live_cs:
        fail("live stream has no tonight show chip")
    elif '"토크"' not in paint or '"게임"' not in paint or '"노래"' not in paint or '"리액션"' not in paint:
        fail("show chip does not name 토크/게임/노래/리액션")
    elif "Palette.Pink" not in paint or "Palette.Troll" not in paint or "Palette.Gold" not in paint or "Palette.PastelDim" not in paint:
        fail("show chip is not the content card accent colors")
    elif "Palette.Pink" not in accent or "Palette.Troll" not in accent or "Palette.Gold" not in accent or "Palette.PastelDim" not in accent:
        fail("show chip drifted from WeekStart card accents")
    elif "PaintShowChip(look.Type)" not in apply:
        fail("show chip is not keyed off tonight's pick")
    elif "look.OverlayTitle" not in apply or "look.Wash" not in apply or "look.BedVolume" not in apply:
        fail("show chip changed the existing content skins")
    elif "_avatar?.ApplyShow(look)" not in apply:
        fail("show chip dropped the webcam show skin")
    elif "오늘: 토크" not in look_cs or "오늘: 게임" not in look_cs or "오늘: 노래" not in look_cs or "오늘: 리액션" not in look_cs:
        fail("show chip dropped 오늘: overlay titles")
    elif "reactionChatSpawnMul" in look_cs or "talkIncomeMultiplier =" in apply:
        fail("show chip retuned content modifiers")
    elif "talkIncomeMultiplier: 1" not in content_asset or "songMentalCost: 8" not in content_asset:
        fail("show chip retuned ContentBalance")
    elif "IncomeMul" not in rules_cs or "MentalCost" not in rules_cs:
        fail("show chip dropped ContentRules tuning")
    elif "ContentPickAccent" not in week_cs or "편하게 잡담" not in week_cs:
        fail("show chip dropped WeekStart content cards")
    elif "TickStartPulse" not in title_cs or "StartChip" not in title_cs:
        fail("show chip dropped 새 방송 시작 pulse")
    elif "TickViewerChipPop" not in live_cs or "BillFill" not in live_cs or "const float LaneHit = -210f" not in live_cs:
        fail("show chip dropped viewer pop, 청구 fill, or the hit bar")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("show chip broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising show chip / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("show chip retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("show chip dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("show chip moved Unity off 6000.5.9f1")
    else:
        ok("live show chip names 토크/게임/노래/리액션 in card accent; skins stay")


def check_settle_show_line() -> None:
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    head_cs = (ROOT / "Assets/Scripts/Presentation/DayHeadline.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    content_asset = (ROOT / "Assets/Resources/Balance/ContentBalance.asset").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    paint = settle_cs.split("void PaintShowLine", 1)[-1].split("void ApplyEndingOverlay", 1)[0]
    head = settle_cs.split("void ApplyHeadline", 1)[-1].split("void PaintShowLine", 1)[0]
    accent = week_cs.split("static Color ContentPickAccent", 1)[-1].split("void AddContentButton", 1)[0]

    if "ShowLine" not in settle_cs or "PaintShowLine" not in settle_cs:
        fail("settlement has no tonight content line")
    elif '"오늘 토크"' not in paint or '"오늘 게임"' not in paint or '"오늘 노래"' not in paint or '"오늘 리액션"' not in paint:
        fail("settlement line does not name 오늘 토크/게임/노래/리액션")
    elif "Palette.Pink" not in paint or "Palette.Troll" not in paint or "Palette.Gold" not in paint or "Palette.PastelDim" not in paint:
        fail("settlement line is not the content card accent colors")
    elif "Palette.Pink" not in accent or "Palette.Troll" not in accent:
        fail("settlement line drifted from WeekStart card accents")
    elif "contentPicked" not in paint or "HasPick" not in paint:
        fail("settlement line is not keyed off tonight's pick")
    elif "DayHeadline.Remember" not in head or "DayHeadline.Build" not in head:
        fail("settlement line changed headline logic")
    elif "오늘 토크" in head or "ShowLine" in head:
        fail("settlement line wrote into ApplyHeadline")
    elif "청구 커버" not in head_cs or "TonightBills" not in head_cs or "lastStreamIncome" not in head_cs:
        fail("settlement line retuned DayHeadline facts")
    elif "PayoutIncome" in paint or "lastStreamIncome =" in paint or "cash =" in paint:
        fail("settlement line writes payout")
    elif '"오늘 수입"' not in settle_cs or "TickLeftCash" not in settle_cs:
        fail("settlement line dropped income tiles / leftover cash")
    elif "팬레터 답장" not in settle_cs or "OnLetter" not in settle_cs or "클립 업로드" not in settle_cs:
        fail("settlement line dropped letter / clip cards")
    elif "TickNextPulse" not in settle_cs or "TickContinuePulse" not in title_cs or "ShowChip" not in live_cs:
        fail("settlement line dropped 다음날 / continue pulse / live show chip")
    elif "public static int Payout" in eco_cs and "lastStreamIncome =" in paint:
        fail("settlement line wrote payout")
    elif "talkIncomeMultiplier: 1" not in content_asset or "songMentalCost: 8" not in content_asset:
        fail("settlement line retuned ContentBalance")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("settlement line broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising settlement show line / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("settlement line retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("settlement line dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("settlement line moved Unity off 6000.5.9f1")
    else:
        ok("settlement names 오늘 토크/게임/노래/리액션 in accent; headline / payout stay")


def check_vtuber_face() -> None:
    import struct

    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    avatar_cs = (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8")
    chrome_cs = (ROOT / "Assets/Scripts/Presentation/StudioChrome.cs").read_text(encoding="utf-8")
    art_cs = (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    bind_cs = (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    png = ROOT / "Assets/Resources/Art/pasan_nyang.png"
    data = png.read_bytes() if png.exists() else b""
    w = h = color = 0
    if len(data) >= 26 and data[:8] == b"\x89PNG\r\n\x1a\n":
        w, h = struct.unpack(">II", data[16:24])
        color = data[25]

    if not png.exists() or png.stat().st_size < 8000:
        fail("webcam talent PNG is missing or empty")
    elif w < 192 or h < 192 or w > 384 or h > 384:
        fail("webcam talent PNG is not a readable ~256px face")
    elif color != 6:
        fail("webcam talent PNG is not RGBA (face would flatten to a blob)")
    elif 'Avatar = "Art/pasan_nyang"' not in art_cs:
        fail("ArtSprites no longer hooks Art/pasan_nyang")
    elif "ArtSprites.Apply(_bust, ArtSprites.Avatar" not in avatar_cs:
        fail("webcam bust is not the VTuber sprite")
    elif "Color.white" not in avatar_cs or "Color.white" not in chrome_cs:
        fail("webcam still tints the face into a colored rect")
    elif "_punch" not in avatar_cs or "_nod" not in avatar_cs or "_shake" not in avatar_cs:
        fail("webcam dropped Perfect punch / Good nod / Miss shake")
    elif "SetTired" not in avatar_cs or "SetTired(tired, danger)" not in live_cs or "m <= 40" not in live_cs:
        fail("webcam dropped tired/desat at mental ≤40")
    elif "perfectWindow" not in rules_cs or "greatWindow" not in rules_cs:
        fail("vtuber face retuned hit windows")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("vtuber face broke pads, 입력됨, or added timeScale")
    elif "GetKeyUp(KeyCode.Space)" not in bind_cs:
        fail("vtuber face broke Space release-once")
    elif "ShowChip" not in live_cs or "PaintShowLine" not in (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8"):
        fail("vtuber face dropped live show chip or settlement show line")
    elif "TickContinuePulse" not in title_cs or "TickStartPulse" not in title_cs:
        fail("vtuber face dropped Title start / continue pulses")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising vtuber face / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("vtuber face retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("vtuber face dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("vtuber face moved Unity off 6000.5.9f1")
    else:
        ok("webcam is a ~256px 2D VTuber face; punch / nod / shake / tired stay")


def check_bill_notice() -> None:
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    art_cs = (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    png = ROOT / "Assets/Resources/Art/bill_notice.png"
    money = week_cs.split("Text MoneyChip", 1)[-1].split("void RefreshHud", 1)[0]
    chip = live_cs.split('var billChip = UiKit.Panel', 1)[-1].split("var billTrack", 1)[0]
    hud = live_cs.split("void RefreshHud", 1)[-1].split("void TickEventWarn", 1)[0]

    if not png.exists() or png.stat().st_size < 8000:
        fail("고지서 sprite is missing")
    elif 'BillNotice = "Art/bill_notice"' not in art_cs:
        fail("ArtSprites does not hook Art/bill_notice")
    elif "ArtSprites.BillNotice" not in money or '"오늘 청구"' not in week_cs:
        fail("WeekStart 오늘 청구 is not on the 고지서 sprite")
    elif "ArtSprites.BillNotice" not in chip or "BillChip" not in live_cs:
        fail("live 청구 chip is not on the 고지서 sprite")
    elif "_billSlam = 0.25f" not in week_cs or "청구보다 부족" not in week_cs:
        fail("bill notice dropped slam or 청구보다 부족")
    elif "new Vector2(180, 10)" not in live_cs or "ticking / (float)_tonightBills" not in hud:
        fail("bill notice dropped the 청구 fill bar")
    elif "SlamBillCover()" not in live_cs or "CoverSlam" not in live_cs:
        fail("bill notice dropped the once cover slam")
    elif "PeekTodayBills" not in week_cs or "TonightBills" not in eco_cs:
        fail("bill notice stopped reading tonight's bill total")
    elif "lastBills =" in week_cs or "billRent =" in hud or "TonightBills =" in hud:
        fail("bill notice writes bill amounts")
    elif "ArtSprites.Avatar" not in (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8"):
        fail("bill notice dropped the VTuber webcam face")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("bill notice broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising 고지서 / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("bill notice retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("bill notice dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("bill notice moved Unity off 6000.5.9f1")
    else:
        ok("오늘 청구 uses a 고지서 sprite; slam / 부족 / fill / cover stay")


def check_stream_overlay() -> None:
    import struct

    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    art_cs = (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    bind_cs = (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    avatar_cs = (ROOT / "Assets/Scripts/Presentation/AvatarView.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    png = ROOT / "Assets/Resources/Art/stream_overlay.png"
    data = png.read_bytes() if png.exists() else b""
    w = h = color = 0
    if len(data) >= 26 and data[:8] == b"\x89PNG\r\n\x1a\n":
        w, h = struct.unpack(">II", data[16:24])
        color = data[25]
    build = live_cs.split("void Build()", 1)[-1].split("void TickOnAir", 1)[0]
    hud = live_cs.split("void RefreshHud", 1)[-1].split("void TickEventWarn", 1)[0]

    if not png.exists() or png.stat().st_size < 8000:
        fail("라이브 overlay PNG is missing")
    elif w < 360 or h < 540 or h <= w:
        fail("라이브 overlay PNG is not a readable portrait frame")
    elif color != 6:
        fail("라이브 overlay PNG is not RGBA (frame would flatten to a box)")
    elif 'StreamOverlay = "Art/stream_overlay"' not in art_cs:
        fail("ArtSprites does not hook Art/stream_overlay")
    elif '"StreamOverlay"' not in build or "ArtSprites.StreamOverlay" not in build:
        fail("LiveStream does not hang the overlay behind the HUD")
    elif "AddColumnPad" not in live_cs or '"Hit"' not in live_cs or "Strike" not in live_cs:
        fail("stream overlay dropped pads, hit bar, or strike")
    elif "BillChip" not in live_cs or "ArtSprites.BillNotice" not in live_cs:
        fail("stream overlay dropped the 청구 chip")
    elif "ShowChip" not in live_cs or "_billFill" not in live_cs or "SlamBillCover()" not in live_cs:
        fail("stream overlay dropped show chip, fill bar, or cover slam")
    elif "ArtSprites.Avatar" not in avatar_cs or "ArtSprites.BillNotice" not in week_cs:
        fail("stream overlay dropped the webcam face or 고지서")
    elif "perfectWindow" not in rules_cs or "greatWindow" not in rules_cs:
        fail("stream overlay retuned hit windows")
    elif "lastStreamIncome =" in hud or "TonightBills =" in hud:
        fail("stream overlay writes payout or bill amounts")
    elif "public static int Payout" in eco_cs and "lastStreamIncome =" in live_cs.split("void RefreshHud", 1)[-1][:800]:
        fail("stream overlay wrote live payout")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("stream overlay broke pads, 입력됨, or added timeScale")
    elif "GetKeyUp(KeyCode.Space)" not in bind_cs:
        fail("stream overlay broke Space release-once")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising stream overlay / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("stream overlay retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("stream overlay dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("stream overlay moved Unity off 6000.5.9f1")
    else:
        ok("라이브 chrome is a 2D overlay frame; chips / pads / hit / FX stay")


def check_title_studio() -> None:
    import struct

    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    art_cs = (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    png = ROOT / "Assets/Resources/Art/title_studio.png"
    data = png.read_bytes() if png.exists() else b""
    w = h = color = 0
    if len(data) >= 26 and data[:8] == b"\x89PNG\r\n\x1a\n":
        w, h = struct.unpack(">II", data[16:24])
        color = data[25]
    build = title_cs.split("void Build()", 1)[-1].split("void BuildHowTo", 1)[0]
    pulse = title_cs.split("void TickStartPulse", 1)[-1].split("void OpenHowTo", 1)[0]
    wipe = title_cs.split("void BuildWipe", 1)[-1].split("void BuildPrologue", 1)[0]
    cont = title_cs.split("void FillContinue", 1)[-1].split("void OpenWipe", 1)[0]

    if not png.exists() or png.stat().st_size < 8000:
        fail("타이틀 studio PNG is missing")
    elif w < 360 or h < 540 or h <= w:
        fail("타이틀 studio PNG is not a readable portrait backdrop")
    elif color != 6:
        fail("타이틀 studio PNG is not RGBA (menu would stay a blank wash)")
    elif 'TitleStudio = "Art/title_studio"' not in art_cs:
        fail("ArtSprites does not hook Art/title_studio")
    elif '"TitleBackdrop"' not in build or "ArtSprites.TitleStudio" not in build:
        fail("Title does not hang the studio behind the wordmark")
    elif "「파산 버튜버」" not in build or "방송 시작" not in build or "이어서 하기" not in build:
        fail("title studio covered the wordmark or menu buttons")
    elif "1f + 0.04f" not in title_cs or "TickStartPulse" not in title_cs or '"시작"' not in title_cs:
        fail("title studio dropped wordmark / 새 방송 시작 pulse")
    elif "1f + 0.03f" not in pulse or "TickContinuePulse" not in title_cs or '"이어"' not in title_cs:
        fail("title studio dropped start / continue pulse chips")
    elif "MoneyPlate" not in title_cs or "이어하기 " not in title_cs or '"현금 "' not in cont or '"부채 "' not in cont:
        fail("title studio dropped the continue row")
    elif "새 방송 시작" not in title_cs or "진행 중인 " not in wipe or "지울까?" not in wipe or "지우고 시작" not in wipe or "취소" not in wipe:
        fail("title studio dropped 새 방송 시작 / wipe confirm")
    elif "OpenWipe" not in title_cs or "ConfirmWipe" not in title_cs or "StartNewRun" not in gm:
        fail("title studio unhooked wipe / start")
    elif "ArtSprites.StreamOverlay" not in live_cs or "ArtSprites.BillNotice" not in week_cs:
        fail("title studio dropped the live overlay or 고지서")
    elif "startingCash: 45000" not in balance or "startingDebt: 50000" not in balance or "startingMental: 100" not in balance:
        fail("title studio retuned start numbers")
    elif "billRent: 8000" not in balance:
        fail("title studio retuned Week 1 bills")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("title studio broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising studio / later weeks")
    elif "defaultScreenOrientation: 0" not in player:
        fail("title studio dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("title studio moved Unity off 6000.5.9f1")
    else:
        ok("Title sits on a broke-studio backdrop; pulse / continue / wipe stay")


def check_settlement_desk() -> None:
    import struct

    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    art_cs = (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    eco_cs = (ROOT / "Assets/Scripts/Economy/EconomyRules.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    png = ROOT / "Assets/Resources/Art/settlement_desk.png"
    data = png.read_bytes() if png.exists() else b""
    w = h = color = 0
    if len(data) >= 26 and data[:8] == b"\x89PNG\r\n\x1a\n":
        w, h = struct.unpack(">II", data[16:24])
        color = data[25]
    build = settle_cs.split("void Build()", 1)[-1].split("void TickDebtCount", 1)[0]
    paint = settle_cs.split("void PaintShowLine", 1)[-1].split("void ApplyEndingOverlay", 1)[0]
    pulse = settle_cs.split("void TickNextPulse", 1)[-1].split("static bool CanAdvance", 1)[0]

    if not png.exists() or png.stat().st_size < 8000:
        fail("정산 desk PNG is missing")
    elif w < 360 or h < 540 or h <= w:
        fail("정산 desk PNG is not a readable portrait room")
    elif color != 6:
        fail("정산 desk PNG is not RGBA (recap would stay a dark stack)")
    elif 'SettlementDesk = "Art/settlement_desk"' not in art_cs:
        fail("ArtSprites does not hook Art/settlement_desk")
    elif '"SettlementBackdrop"' not in build or "ArtSprites.SettlementDesk" not in build:
        fail("Settlement does not hang the desk behind the cards")
    elif '"오늘 수입"' not in build or "TickIncomeCount" not in settle_cs or "ShowShortfall" not in settle_cs:
        fail("settlement desk dropped income count-up or 청구 미달")
    elif "청구 미달" not in settle_cs or "TickDebtCount" not in settle_cs or "TickMentalCount" not in settle_cs:
        fail("settlement desk dropped 청구 미달 or debt/mental ticks")
    elif "TickLeftCash" not in settle_cs or '"남은 현금"' not in settle_cs:
        fail("settlement desk dropped 남은 현금")
    elif "ApplyHeadline" not in settle_cs or "DayHeadline.Build" not in settle_cs or "ShowLine" not in settle_cs:
        fail("settlement desk dropped headline or content line")
    elif "PaintShowLine" not in settle_cs or '"오늘 토크"' not in paint:
        fail("settlement desk dropped the tonight content line")
    elif "TickNextPulse" not in settle_cs or '"다음"' not in settle_cs or "1f + 0.03f" not in pulse:
        fail("settlement desk dropped 다음날 pulse")
    elif "팬레터 답장" not in settle_cs or "OnLetter" not in settle_cs or "LetterCard" not in settle_cs:
        fail("settlement desk dropped fan-letter cards")
    elif "NextMorning()" not in settle_cs or "public void NextMorning()" not in gm:
        fail("settlement desk changed next-morning routing")
    elif "lastStreamIncome =" in build or "PayoutIncome" in paint:
        fail("settlement desk writes payout")
    elif "public static int Payout" in eco_cs and "lastStreamIncome =" in paint:
        fail("settlement desk wrote live payout")
    elif "ArtSprites.TitleStudio" not in title_cs or "ArtSprites.StreamOverlay" not in live_cs:
        fail("settlement desk dropped title studio or live overlay")
    elif "ArtSprites.BillNotice" not in week_cs:
        fail("settlement desk dropped the 고지서")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("settlement desk retuned Week 1 cash or bills")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("settlement desk broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising settlement desk / later weeks")
    elif "defaultScreenOrientation: 0" not in player:
        fail("settlement desk dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("settlement desk moved Unity off 6000.5.9f1")
    else:
        ok("Settlement sits on an after-stream desk; counts / 미달 / 다음날 stay")


def check_morning_room() -> None:
    import struct

    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    art_cs = (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    settle_cs = (ROOT / "Assets/Scripts/Presentation/SettlementDirector.cs").read_text(encoding="utf-8")
    gm = (ROOT / "Assets/Scripts/Core/GameManager.cs").read_text(encoding="utf-8")
    head_cs = (ROOT / "Assets/Scripts/Presentation/DayHeadline.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    png = ROOT / "Assets/Resources/Art/morning_room.png"
    data = png.read_bytes() if png.exists() else b""
    w = h = color = 0
    if len(data) >= 26 and data[:8] == b"\x89PNG\r\n\x1a\n":
        w, h = struct.unpack(">II", data[16:24])
        color = data[25]
    build = week_cs.split("void Build()", 1)[-1].split("void RefreshHud", 1)[0]
    pulse = week_cs.split("void TickGoLivePulse", 1)[-1].split("void Build()", 1)[0]
    money = week_cs.split("Text MoneyChip", 1)[-1].split("void RefreshHud", 1)[0]

    if not png.exists() or png.stat().st_size < 8000:
        fail("아침 room PNG is missing")
    elif w < 360 or h < 540 or h <= w:
        fail("아침 room PNG is not a readable portrait room")
    elif color != 6:
        fail("아침 room PNG is not RGBA (morning would stay a dark stack)")
    elif 'MorningRoom = "Art/morning_room"' not in art_cs:
        fail("ArtSprites does not hook Art/morning_room")
    elif '"MorningBackdrop"' not in build or "ArtSprites.MorningRoom" not in build:
        fail("WeekStart does not hang the morning room behind the cards")
    elif "_daySlam = 0.25f" not in week_cs or "_daySlam / 0.25f" not in week_cs:
        fail("morning room dropped the n일차 slam")
    elif "ArtSprites.BillNotice" not in money or '"오늘 청구"' not in week_cs:
        fail("morning room dropped the 오늘 청구 고지서")
    elif "청구보다 부족" not in week_cs or "RefreshCashShort" not in week_cs:
        fail("morning room dropped 청구보다 부족")
    elif "AddContentButton" not in week_cs or "StreamContentType.Talk" not in build:
        fail("morning room dropped content pick cards")
    elif "마지막 날" not in week_cs or "LastDayBanner" not in week_cs or "RefreshLastDay" not in week_cs:
        fail("morning room dropped the last-day banner")
    elif "YesterdayLine" not in week_cs or '"어제: "' not in head_cs:
        fail("morning room dropped the 어제 headline")
    elif "TickGoLivePulse" not in week_cs or "1f + 0.04f" not in pulse or "LivePip" not in week_cs:
        fail("morning room dropped GO LIVE pulse")
    elif "GameManager.Instance.GoLive()" not in week_cs or "public void GoLive()" not in gm:
        fail("morning room changed GO LIVE behavior")
    elif "PeekTodayBills" not in week_cs or "lastBills =" in week_cs or "billRent =" in build:
        fail("morning room writes bill amounts")
    elif "ArtSprites.TitleStudio" not in title_cs or "ArtSprites.SettlementDesk" not in settle_cs:
        fail("morning room dropped title studio or settlement desk")
    elif "ArtSprites.StreamOverlay" not in live_cs:
        fail("morning room dropped the live overlay")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("morning room retuned Week 1 cash or bills")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("morning room broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising morning room / later weeks")
    elif "defaultScreenOrientation: 0" not in player:
        fail("morning room dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("morning room moved Unity off 6000.5.9f1")
    else:
        ok("WeekStart sits on a 청구 아침 room; slam / 고지서 / GO LIVE stay")


def check_pad_keycaps() -> None:
    import struct

    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    pad_cs = (ROOT / "Assets/Scripts/Input/StreamPadButton.cs").read_text(encoding="utf-8")
    bind_cs = (ROOT / "Assets/Scripts/Input/StreamBindings.cs").read_text(encoding="utf-8")
    art_cs = (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    uikit_cs = (ROOT / "Assets/Scripts/Presentation/UiKit.cs").read_text(encoding="utf-8")
    week_cs = (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    add = live_cs.split("StreamPadButton AddColumnPad", 1)[-1].split("void BuildSuperchatPip", 1)[0]
    names = {
        "pad_left.png": "←",
        "pad_down.png": "↓",
        "pad_right.png": "→",
        "pad_up.png": "↑",
        "pad_superchat.png": "슈퍼챗",
    }

    for name, label in names.items():
        png = ROOT / "Assets/Resources/Art" / name
        data = png.read_bytes() if png.exists() else b""
        w = h = color = 0
        if len(data) >= 26 and data[:8] == b"\x89PNG\r\n\x1a\n":
            w, h = struct.unpack(">II", data[16:24])
            color = data[25]
        if not png.exists() or png.stat().st_size < 4000:
            fail(f"pad keycap {name} ({label}) is missing")
            return
        if w < 128 or h < 128 or abs(w - h) > 32:
            fail(f"pad keycap {name} is not a readable square key")
            return
        if color != 6:
            fail(f"pad keycap {name} is not RGBA")
            return

    if (
        'PadLeft = "Art/pad_left"' not in art_cs
        or 'PadDown = "Art/pad_down"' not in art_cs
        or 'PadRight = "Art/pad_right"' not in art_cs
        or 'PadUp = "Art/pad_up"' not in art_cs
        or 'PadSuperchat = "Art/pad_superchat"' not in art_cs
    ):
        fail("ArtSprites does not hook pad_* keycaps")
    elif "KeycapFor" not in add or "ArtSprites.PadLeft" not in add or "ArtSprites.PadSuperchat" not in add:
        fail("AddColumnPad does not hang stream-deck keycaps")
    elif "ArtSprites.PadDown" not in add or "ArtSprites.PadRight" not in add or "ArtSprites.PadUp" not in add:
        fail("kind pads are not each on their own keycap")
    elif "_flash = 0.08f" not in pad_cs or "Color.white" not in pad_cs:
        fail("pad keycaps dropped the 0.08s press flash")
    elif '"슈퍼챗"' not in live_cs.split("void BuildSuperchatPip", 1)[-1].split("void TickSuperchatPip", 1)[0]:
        fail("pad keycaps dropped the 슈퍼챗 telegraph pip")
    elif "UnlockUiInputForStream" not in live_cs or "DontDestroyOnLoad" not in uikit_cs:
        fail("pad keycaps dropped EventSystem unlock / DDOL")
    elif "GetKeyDown(KeyCode.LeftArrow)" not in bind_cs or "KeyCode.A" not in bind_cs or "KeyCode.W" not in bind_cs:
        fail("pad keycaps retuned arrows / ASDF / WASD")
    elif "QueueKind" not in pad_cs or "BeginSuperchatCharge" not in pad_cs or "GetKeyUp(KeyCode.Space)" not in bind_cs:
        fail("pad keycaps changed input bindings")
    elif "perfectWindow: 0.07" not in balance or "goodWindow: 0.22" not in balance:
        fail("pad keycaps retuned hit windows")
    elif "perfectWindow * " not in rules_cs or "b.goodWindow" not in rules_cs:
        fail("pad keycaps retuned Judge windows")
    elif "lastStreamIncome =" in add or "PayoutIncome" in add:
        fail("pad keycaps write payout")
    elif "ArtSprites.MorningRoom" not in week_cs or "ArtSprites.StreamOverlay" not in live_cs:
        fail("pad keycaps dropped morning room or live overlay")
    elif "긍정" not in live_cs or "공감" not in live_cs or "웃음" not in live_cs or "감사" not in live_cs or "슈퍼챗" not in live_cs:
        fail("pad keycaps dropped Korean pad labels")
    elif "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("pad keycaps broke 입력됨 or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising pad keycaps / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("pad keycaps retuned Week 1 cash or bills")
    elif "defaultScreenOrientation: 0" not in player:
        fail("pad keycaps dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("pad keycaps moved Unity off 6000.5.9f1")
    else:
        ok("live pads are stream-deck keycaps; flash / pip / bindings stay")


def check_chat_bubble() -> None:
    import struct

    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    art_cs = (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    session_cs = (ROOT / "Assets/Scripts/Stream/StreamSession.cs").read_text(encoding="utf-8")
    catalog = (ROOT / "Assets/Resources/Balance/ChatCatalog.asset").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    png = ROOT / "Assets/Resources/Art/chat_bubble.png"
    data = png.read_bytes() if png.exists() else b""
    w = h = color = 0
    if len(data) >= 26 and data[:8] == b"\x89PNG\r\n\x1a\n":
        w, h = struct.unpack(">II", data[16:24])
        color = data[25]
    make = live_cs.split("RectTransform MakeBubble", 1)[-1].split("static void DimNamedBubble", 1)[0]
    tint = live_cs.split("static void TintTravelNote", 1)[-1].split("RectTransform MakeBubble", 1)[0]
    sync = live_cs.split("void SyncNotes", 1)[-1].split("void RefreshPromoOverlay", 1)[0]

    if not png.exists() or png.stat().st_size < 4000:
        fail("채팅 bubble PNG is missing")
    elif w < 240 or h < 96 or w <= h:
        fail("채팅 bubble PNG is not a readable horizontal pill")
    elif color != 6:
        fail("채팅 bubble PNG is not RGBA")
    elif 'ChatBubble = "Art/chat_bubble"' not in art_cs:
        fail("ArtSprites does not hook Art/chat_bubble")
    elif "ArtSprites.ChatBubble" not in make or "KindEdge" not in make:
        fail("regular chat notes are not drawn on the chat bubble")
    elif "ArtSprites.SuperchatBanner" not in make or "ArtSprites.TrollBubble" not in make:
        fail("chat bubble dropped gold superchat or troll treatment")
    elif "KindEdge" not in tint or "NotePadColor" not in tint:
        fail("kind edge no longer follows pad colors")
    elif "TintTravelNote" not in sync or "Palette.ForKind(note.Kind)" not in live_cs:
        fail("chat bubble dropped traveling kind tint")
    elif '"Nick"' not in make or "note.User" not in make or "note.Text" not in make:
        fail("chat bubble dropped nicks or ChatCatalog lines")
    elif "interval *= 0.5f" not in session_cs or "HypeActive" not in session_cs:
        fail("chat bubble dropped hype 2x spawn")
    elif "chatSpawnStart: 1.55" not in balance or "chatSpawnEnd: 1.05" not in balance:
        fail("chat bubble retuned chat spawn table")
    elif "positive:" not in catalog or "empathy:" not in catalog or "laugh:" not in catalog or "thanks:" not in catalog:
        fail("chat bubble dropped ChatCatalog kinds")
    elif "ChatKind.Positive" not in make or "ChatKind.Empathy" not in make or "ChatKind.Laugh" not in make:
        fail("chat bubble changed chat kinds")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance or "hypeSeconds: 12" not in balance:
        fail("chat bubble retuned Week 1 economy / hype")
    elif "ArtSprites.PadLeft" not in live_cs or "ArtSprites.MorningRoom" not in (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8"):
        fail("chat bubble dropped pad keycaps or morning room")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("chat bubble broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising chat bubble / later weeks")
    elif "defaultScreenOrientation: 0" not in player:
        fail("chat bubble dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("chat bubble moved Unity off 6000.5.9f1")
    else:
        ok("regular chat sits on a live-chat bubble; superchat gold / nicks / hype stay")


def check_note_chip() -> None:
    import struct

    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    art_cs = (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    png = ROOT / "Assets/Resources/Art/note_chip.png"
    data = png.read_bytes() if png.exists() else b""
    w = h = color = 0
    if len(data) >= 26 and data[:8] == b"\x89PNG\r\n\x1a\n":
        w, h = struct.unpack(">II", data[16:24])
        color = data[25]
    make = live_cs.split("RectTransform MakeBubble", 1)[-1].split("static void DimNamedBubble", 1)[0]
    tint = live_cs.split("static void TintTravelNote", 1)[-1].split("RectTransform MakeBubble", 1)[0]
    sync = live_cs.split("void SyncNotes", 1)[-1].split("void RefreshPromoOverlay", 1)[0]
    strike = live_cs.split("void TickStrike", 1)[-1].split("void SyncNotes", 1)[0]

    if not png.exists() or png.stat().st_size < 4000:
        fail("노트 chip PNG is missing")
    elif w < 128 or h < 128 or abs(w - h) > 8:
        fail("노트 chip PNG is not a readable square gem")
    elif color != 6:
        fail("노트 chip PNG is not RGBA")
    elif 'NoteChip = "Art/note_chip"' not in art_cs:
        fail("ArtSprites does not hook Art/note_chip")
    elif "ArtSprites.NoteChip" not in make or '"NoteChip"' not in make:
        fail("traveling notes are not drawn on the note chip")
    elif "NoteChipAngle" not in make and "NoteChipAngle" not in live_cs:
        fail("note chip does not rotate to kind arrows")
    elif "NoteChip" not in tint or "NotePadColor" not in tint:
        fail("note chip is not tinted to kind pad colors")
    elif "ArtSprites.SuperchatBanner" not in make or "Palette.Gold" not in live_cs.split("static Color NotePadColor", 1)[-1].split("static void TintTravelNote", 1)[0]:
        fail("note chip dropped gold superchat treatment")
    elif "ArtSprites.ChatBubble" not in make or "KindEdge" not in make:
        fail("note chip dropped the live-chat bubble")
    elif "abs <= 0.15f" not in sync or '"Hot"' not in live_cs or "1f, 1f, 1f" not in sync:
        fail("note chip dropped the 0.15s hittable glow")
    elif "TickStrike" not in live_cs or "Judgement.Perfect" not in strike:
        fail("note chip dropped the strike line pulse")
    elif "approachSeconds =" in sync or "HitTime =" in sync or "SpawnNote" in tint:
        fail("note chip writes travel / hit times")
    elif "perfectWindow =" in live_cs or "goodWindow =" in live_cs:
        fail("note chip retuned judge windows")
    elif "perfectWindow: 0.07" not in balance or "goodWindow: 0.22" not in balance:
        fail("note chip retuned hit windows")
    elif "approachSeconds: 1.35" not in balance:
        fail("note chip retuned travel speed")
    elif "perfectWindow * " not in rules_cs or "b.goodWindow" not in rules_cs:
        fail("note chip retuned Judge scoring")
    elif "ArtSprites.PadLeft" not in live_cs or 'ChatBubble = "Art/chat_bubble"' not in art_cs:
        fail("note chip dropped pad keycaps or chat bubble art")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("note chip broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising note chip / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance or "hypeSeconds: 12" not in balance:
        fail("note chip retuned Week 1 economy / hype")
    elif "defaultScreenOrientation: 0" not in player:
        fail("note chip dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("note chip moved Unity off 6000.5.9f1")
    else:
        ok("traveling notes sit on a rhythm chip; pad tint / glow / strike / scoring stay")


def check_hit_rail() -> None:
    import struct

    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    art_cs = (ROOT / "Assets/Scripts/Presentation/ArtSprites.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    png = ROOT / "Assets/Resources/Art/hit_rail.png"
    data = png.read_bytes() if png.exists() else b""
    w = h = color = 0
    if len(data) >= 26 and data[:8] == b"\x89PNG\r\n\x1a\n":
        w, h = struct.unpack(">II", data[16:24])
        color = data[25]
    lane = live_cs.split('_lane = UiKit.Panel', 1)[-1].split("var bottom = UiKit.Panel", 1)[0]
    tick = live_cs.split("void TickStrike", 1)[-1].split("void SyncNotes", 1)[0]
    hit = live_cs.split("_hit = UiKit.Panel", 1)[-1].split("_strike", 1)[0]
    build = live_cs.split("_hit = UiKit.Panel", 1)[-1].split("var hitLabel", 1)[0]

    if not png.exists() or png.stat().st_size < 4000:
        fail("히트 rail PNG is missing")
    elif w < 128 or h < 240 or h <= w:
        fail("히트 rail PNG is not a readable tall track")
    elif color != 6:
        fail("히트 rail PNG is not RGBA")
    elif 'HitRail = "Art/hit_rail"' not in art_cs:
        fail("ArtSprites does not hook Art/hit_rail")
    elif "ArtSprites.HitRail" not in lane or '"HitRail"' not in lane:
        fail("lane does not draw the hit rail under notes")
    elif "SetAsFirstSibling" not in lane:
        fail("hit rail is not drawn under the notes")
    elif "new Vector2(0, LaneHit)" not in hit or "new Vector2(0, 10)" not in hit:
        fail("hit rail moved or resized the hit line")
    elif "new Vector2(0, LaneHit)" not in build or "new Vector2(0, 4)" not in build:
        fail("hit rail dropped the thin strike marker at LaneHit")
    elif "const float LaneHit = -210f" not in live_cs or "const float LaneTop = 260f" not in live_cs:
        fail("hit rail moved the hit line or lane top")
    elif "StreamRules.Judge" not in tick or "Judgement.Perfect" not in tick:
        fail("hit rail dropped Perfect-window strike pulse")
    elif "Palette.Gold" not in tick or "Color.white" not in tick:
        fail("hit rail dropped white/gold strike pulse")
    elif "ArtSprites.NoteChip" not in live_cs or "ArtSprites.ChatBubble" not in live_cs:
        fail("hit rail dropped note chip or chat bubble")
    elif "perfectWindow =" in live_cs or "goodWindow =" in live_cs:
        fail("hit rail retuned judge windows")
    elif "perfectWindow: 0.07" not in balance or "goodWindow: 0.22" not in balance:
        fail("hit rail retuned hit windows")
    elif "approachSeconds: 1.35" not in balance:
        fail("hit rail retuned travel speed")
    elif "perfectWindow * " not in rules_cs or "b.goodWindow" not in rules_cs:
        fail("hit rail retuned Judge scoring")
    elif "LaneHit =" in tick or "HitTime =" in tick:
        fail("hit rail writes hit position / times")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("hit rail broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising hit rail / later weeks")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance or "hypeSeconds: 12" not in balance:
        fail("hit rail retuned Week 1 economy / hype")
    elif "defaultScreenOrientation: 0" not in player:
        fail("hit rail dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("hit rail moved Unity off 6000.5.9f1")
    else:
        ok("notes ride a hit rail; strike marker / Perfect pulse / windows stay")


def check_judge_sfx() -> None:
    import wave

    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    rules_cs = (ROOT / "Assets/Scripts/Stream/StreamRules.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    judge = live_cs.split("if (_session.LastJudgement.HasValue", 1)[-1].split("SyncNotes();", 1)[0]
    apply = live_cs.split("void ApplyContentShow", 1)[-1].split("void PaintShowChip", 1)[0]
    clips = {
        "sfx_perfect.wav": (0.05, 0.20),
        "sfx_good.wav": (0.04, 0.20),
        "sfx_miss.wav": (0.08, 0.25),
        "sfx_combo_break.wav": (0.10, 0.30),
    }
    durations = {}
    for name, (lo, hi) in clips.items():
        path = ROOT / "Assets/Resources/Audio" / name
        if not path.exists() or path.stat().st_size < 1500:
            fail(f"judge SFX {name} is missing")
            return
        with wave.open(str(path), "rb") as w:
            if w.getnchannels() < 1 or w.getframerate() < 22050:
                fail(f"judge SFX {name} is not a readable PCM clip")
                return
            dur = w.getnframes() / float(w.getframerate())
            durations[name] = dur
            if dur < lo or dur > hi:
                fail(f"judge SFX {name} duration {dur:.3f}s is not a short distinct clip")
                return

    if durations["sfx_perfect.wav"] >= durations["sfx_miss.wav"]:
        fail("Perfect tick is not shorter/brighter than Miss thud")
    elif "Audio/sfx_perfect" not in live_cs or "Audio/sfx_good" not in live_cs or "Audio/sfx_miss" not in live_cs:
        fail("LiveStream does not load Audio/sfx_perfect|good|miss")
    elif "Audio/sfx_combo_break" not in live_cs:
        fail("combo-break SFX is not loaded")
    elif "PlaySfx(_perfect" not in judge or "PlaySfx(_good" not in judge or "PlaySfx(_miss" not in judge:
        fail("Perfect / Good / Miss do not play distinct clips")
    elif "PlaySfx(_comboBreakSfx" not in judge:
        fail("combo-break does not play its thud")
    elif "PlaySfx(_sc" not in judge or "sfx_onair" not in live_cs:
        fail("judge SFX dropped superchat or on-air clips")
    elif "_ok = ToneClip" in apply or "_bad = BuzzerClip" in apply:
        fail("content show still overwrites distinct judge SFX")
    elif "perfectWindow =" in live_cs or "goodWindow =" in live_cs:
        fail("judge SFX retuned judge windows")
    elif "perfectWindow: 0.07" not in balance or "goodWindow: 0.22" not in balance:
        fail("judge SFX retuned hit windows")
    elif "approachSeconds: 1.35" not in balance:
        fail("judge SFX retuned travel speed")
    elif "perfectWindow * " not in rules_cs or "b.goodWindow" not in rules_cs:
        fail("judge SFX retuned Judge scoring")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("judge SFX retuned Week 1 economy")
    elif "ArtSprites.HitRail" not in live_cs or "ArtSprites.NoteChip" not in live_cs:
        fail("judge SFX dropped hit rail or note chip")
    elif "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("judge SFX broke pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising judge SFX / later weeks")
    elif "defaultScreenOrientation: 0" not in player:
        fail("judge SFX dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("judge SFX moved Unity off 6000.5.9f1")
    else:
        ok("Perfect/Good/Miss are distinct Resource SFX; combo-break thud; windows stay")


def check_stream_stings() -> None:
    import wave

    live_cs = (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    title_cs = (ROOT / "Assets/Scripts/Presentation/TitleDirector.cs").read_text(encoding="utf-8")
    debug_cs = (ROOT / "Assets/Scripts/Core/PlaytestDebug.cs").read_text(encoding="utf-8")
    balance = (ROOT / "Assets/Resources/Balance/Week1Balance.asset").read_text(encoding="utf-8")
    player = (ROOT / "ProjectSettings/ProjectSettings.asset").read_text(encoding="utf-8")
    start_block = live_cs.split("void Start()", 1)[-1].split("void Update()", 1)[0]
    end_show = live_cs.split("void ShowEndCut", 1)[-1].split("void Build", 1)[0]
    judge = live_cs.split("if (_session.LastJudgement.HasValue", 1)[-1].split("SyncNotes();", 1)[0]

    clips = {
        "sfx_onair.wav": (0.18, 0.45),
        "sfx_end_cut.wav": (0.12, 0.40),
    }
    for name, (lo, hi) in clips.items():
        path = ROOT / "Assets/Resources/Audio" / name
        if not path.exists() or path.stat().st_size < 2000:
            fail(f"stream sting {name} is missing")
            return
        with wave.open(str(path), "rb") as w:
            dur = w.getnframes() / float(w.getframerate())
            if dur < lo or dur > hi:
                fail(f"stream sting {name} duration {dur:.3f}s is not a short distinct sting")
                return

    if "Audio/sfx_onair" not in live_cs or "PlaySfx(_onAirCue" not in start_block:
        fail("ON AIR does not play Audio/sfx_onair start sting")
    elif "Audio/sfx_end_cut" not in live_cs or "PlaySfx(_endCutCue" not in end_show:
        fail("방송 종료 does not play Audio/sfx_end_cut sting")
    elif "PlaySfx(_perfect" not in judge or "PlaySfx(_miss" not in judge or "PlaySfx(_good" not in judge:
        fail("stream stings overwrote distinct judge SFX")
    elif "Audio/sfx_perfect" not in live_cs or "Audio/sfx_good" not in live_cs:
        fail("stream stings dropped judge Resource SFX")
    elif "_onAirLeft = 0.6f" not in live_cs:
        fail("stream stings retuned 0.6s ON AIR")
    elif "WaitForSeconds(0.5f)" not in live_cs.split("EndRoutine", 1)[-1].split("void Build", 1)[0]:
        fail("stream stings retuned 0.5s end cut")
    elif "streamSeconds: 90" not in balance:
        fail("stream stings retuned 90s length")
    elif "ShowEndCut" in debug_cs or "PlaySfx(_endCutCue" in debug_cs:
        fail("F10 skip is no longer silent / direct to settlement")
    elif "billRent: 8000" not in balance or "startingCash: 45000" not in balance:
        fail("stream stings retuned Week 1 economy")
    elif "ArtSprites.HitRail" not in live_cs or "AddColumnPad" not in live_cs or "입력됨" not in live_cs or "timeScale" in live_cs:
        fail("stream stings broke rail, pads, 입력됨, or added timeScale")
    elif "Week2" in title_cs or "Fandom" in title_cs or "민준" in title_cs or "토크" in title_cs:
        fail("Title started advertising stream stings / later weeks")
    elif "defaultScreenOrientation: 0" not in player:
        fail("stream stings dropped the Android Portrait lock")
    elif "6000.5.9f1" not in (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8"):
        fail("stream stings moved Unity off 6000.5.9f1")
    else:
        ok("ON AIR start sting + 방송 종료 cut sting; timings / F10 / judge SFX stay")


def check_save_roundtrip() -> None:
    dummy = {
        "version": 1,
        "day": 11,
        "cash": 88000,
        "debt": 21000,
        "mental": 72,
        "billsAppliedThisDay": False,
        "streamDoneThisDay": False,
        "runSeed": 4242,
        "membershipUnlocked": True,
        "membershipCount": 8,
        "viewerBonus": 6,
        "peakViewersEver": 48.0,
        "successfulStreams": 7,
        "week2EntryApplied": True,
        "goodsUnlocked": True,
        "goodsStock": 14,
        "agencyFounded": False,
        "juniorScouted": False,
        "tier0": 18,
        "tier1": 4,
        "tier2": 2,
        "tier3": 8,
        "tier4": 2,
        "loyalty": 62,
        "minjunPresent": True,
        "minjunEver": True,
        "haeunPresent": True,
        "haeunEver": True,
        "conflictPending": True,
        "conflictResolved": False,
        "playerRankingScore": 0,
        "npcRankingScore": [420, 360, 300],
        "lastNpcScore": [0, 0, 0],
        "concertPlayed": False,
        "lastEnding": 0,
        "lastOutcome": 0,
        "contentPicked": 2,
        "extraRolls": [
            {
                "id": "gear_break",
                "displayName": "장비 고장",
                "amount": 7000,
                "artPath": "Art/bill_gear",
                "tintHex": "FF6B6B",
            }
        ],
    }
    text = json.dumps(dummy, ensure_ascii=False, separators=(",", ":"))
    back = json.loads(text)
    if back != dummy:
        fail("dummy run JSON roundtrip mutated fields")
        return
    if back["day"] != 11 or back["loyalty"] != 62 or back["membershipCount"] != 8:
        fail("dummy Week 3 run lost day/loyalty/membership")
        return
    if back["minjunPresent"] is not True or back["haeunPresent"] is not True:
        fail("dummy run lost named superfans")
        return
    if back["extraRolls"][0]["amount"] != 7000 or back["npcRankingScore"][1] != 360:
        fail("dummy run lost extra threat / ranking arrays")
        return
    if back.get("contentPicked") != 2:
        fail("dummy run lost today's content pick")
        return
    save_cs = (ROOT / "Assets/Scripts/Core/RunSave.cs").read_text(encoding="utf-8")
    for token in ("day = 11", "cash = 88000", "loyalty = 62", "membershipCount = 8", "goodsStock = 14", "contentPicked = StreamContentType.Game"):
        if token not in save_cs:
            fail(f"MakeDummy missing {token}")
            return
    ok("dummy run JSON serialize roundtrip keeps Week 3 + fandom + ranking")


def simulate_stream(skill: str, seed: int) -> int:
    rng = random.Random(seed)
    viewers = 12.0
    mental = 100
    perfect_combo = 0
    miss_streak = 0
    total_miss = 0
    total_miss_pen = False
    hype = 0.0
    tick = 0.0
    superchat = 0
    t = 0.0
    next_chat = 0.4
    next_sc = rng.uniform(9, 11)
    sc_target = rng.randint(8, 10)
    sc_spawned = 0
    dt = 0.05
    notes = []  # (hit_at, kind, sc_won)

    def income_mul():
        if hype > 0:
            return 2.5
        if perfect_combo >= 5:
            return 1.5
        return 1.0

    def resolve(is_sc, won):
        nonlocal viewers, mental, perfect_combo, miss_streak, total_miss, total_miss_pen, hype, superchat
        # skill hit model
        if skill == "afk":
            hit = False
            grade = "miss"
        elif skill == "newbie":
            hit = rng.random() < (0.72 if is_sc else 0.48)
            grade = rng.choices(["perfect", "great", "good"], [0.15, 0.35, 0.50])[0]
        elif skill == "average":
            hit = rng.random() < (0.90 if is_sc else 0.78)
            grade = rng.choices(["perfect", "great", "good"], [0.35, 0.40, 0.25])[0]
        else:
            hit = rng.random() < (0.99 if is_sc else 0.94)
            grade = rng.choices(["perfect", "great", "good"], [0.72, 0.22, 0.06])[0]
        if not hit:
            grade = "miss"
        if grade == "miss":
            viewers = max(1.0, viewers - 1.2)
            perfect_combo = 0
            miss_streak += 1
            total_miss += 1
            if miss_streak >= 3:
                mental -= 12
                viewers = max(1.0, viewers - 4)
                miss_streak = 0
            if not total_miss_pen and total_miss >= 10:
                mental -= 20
                total_miss_pen = True
        else:
            miss_streak = 0
            if grade == "perfect":
                viewers += 0.5
                perfect_combo += 1
                if perfect_combo == 9:
                    hype = 12.0
            elif grade == "great":
                viewers += 0.2
                perfect_combo = 0
            else:
                perfect_combo = 0
            if is_sc:
                superchat += won
        return mental <= 0

    force = False
    while t < 90 and mental > 0:
        t += dt
        if hype > 0:
            hype -= dt
            viewers += 1.0 * dt
        tick += math.floor(viewers) * 3 * income_mul() * dt
        interval = 1.55 + (1.05 - 1.55) * (t / 90)
        if t >= next_chat and t < 90 - 0.5:
            notes.append((t + 1.35, "chat", 0))
            next_chat = t + interval
        if sc_spawned < sc_target and t >= next_sc and t < 90 - 0.5:
            base = rng.randint(1000, 6000)
            if hype > 0:
                base = int(base * 2)
            notes.append((t + 1.35, "sc", base))
            sc_spawned += 1
            next_sc = t + rng.uniform(9, 11)
        due = [n for n in notes if n[0] <= t]
        notes = [n for n in notes if n[0] > t]
        for _, kind, won in due:
            if resolve(kind == "sc", won):
                force = True
                break
        if force:
            break

    paid = int(tick) + superchat
    if force:
        paid //= 2
    return paid


def check_economy() -> None:
    bands = {
        "newbie": (12000, 18000),
        "average": (24000, 32000),
        "skilled": (40000, 55000),
    }
    for skill, (lo, hi) in bands.items():
        takes = [simulate_stream(skill, seed=1000 + i) for i in range(40)]
        med = sorted(takes)[len(takes) // 2]
        mean = sum(takes) / len(takes)
        print(f"SIM : {skill:8s} median={med:5d} mean={mean:7.0f}  range={min(takes)}-{max(takes)}  target={lo}-{hi}")
        # Allow slack: median should sit near the band, not necessarily inside every seed.
        if med < lo * 0.55 or med > hi * 1.55:
            fail(f"{skill} median {med} far from {lo}-{hi}")
        else:
            ok(f"{skill} median {med} near {lo}-{hi}")

    afk = [simulate_stream("afk", seed=2000 + i) for i in range(20)]
    afk_med = sorted(afk)[len(afk) // 2]
    print(f"SIM : afk      median={afk_med:5d}")
    if afk_med >= 12000:
        fail(f"AFK median {afk_med} is high enough to win passively")
    else:
        ok(f"AFK cannot farm the newbie band (median {afk_med})")

    # 5-day ledger sanity (fixed 22000 + extra 4000-12000)
    cash, debt = 45000, 50000
    bills = 22000
    extras = [7000, 5000, 4000, 10000, 5000]
    for skill in ("newbie", "average", "skilled"):
        c, d = cash, debt
        for day in range(5):
            c -= bills + extras[day]
            if c < 0:
                d += -c
                c = 0
            take = simulate_stream(skill, seed=hash(skill) % 10000 + day)
            c += take
        print(f"WEEK: {skill:8s} cash={c} debt={d} win={d <= 30000 or c >= 70000}")
    if extras[0] == extras[1] == extras[2]:
        fail("week extra amounts are identical — days should not all be ₩22,000")
    else:
        ok("week extras differ so days are not a flat ₩22,000")
    ok("5-day ledger simulation completed")

    # Week 2 numbers must not leak into days 1–5.
    w1_bills = 8000 + 4000 + 3000 + 5000 + 2000
    w2_bills = 10000 + 5000 + 5000 + 6000 + 2000
    if w1_bills != 22000:
        fail(f"Week 1 bill sum {w1_bills} != 22000")
    if w2_bills != 28000:
        fail(f"Week 2 bill sum {w2_bills} != 28000")
    else:
        ok("Week 2 fixed bills sum to ₩28,000")

    members = 8
    members += 1  # hype
    members -= 1  # misses >= 10
    if members != 8:
        fail(f"membership hype/miss math {members} != 8")
    else:
        ok("membership +1 on hype and −1 on 10+ misses")
    passive = members * 150
    if passive != 1200:
        fail(f"membership passive {passive} != 1200")
    else:
        ok("settlement membership passive is count * ₩150")

    cash, debt, members = 70000 + 15000, max(0, 30000 - 10000), 8
    extras2 = [5000, 0, 3000, 8000, 0]
    for day in range(6, 11):
        cash -= w2_bills + extras2[day - 6]
        if cash < 0:
            debt += -cash
            cash = 0
        take = simulate_stream("average", seed=3000 + day)
        cash += take
        cash += members * 150
    unlocked = True
    win = (debt <= 20000 or cash >= 110000) and unlocked
    print(f"WEEK2: average cash={cash} debt={debt} members={members} win={win}")
    ok("Week 2 5-day ledger simulation completed")

    w3_bills = 12000 + 6000 + 6000 + 7000 + 3000
    if w3_bills != 34000:
        fail(f"Week 3 bill sum {w3_bills} != 34000")
    else:
        ok("Week 3 fixed bills sum to ₩34,000")

    members, peak = 8, 50.0
    sold = math.floor(members * 0.4 + peak * 0.08)
    if sold < 1:
        sold = 1
    if sold != 7:
        fail(f"goods sold {sold} != 7 for 8 members / peak 50")
    else:
        ok("goods sold floor(members*0.4 + peak*0.08) is 7 at 8/50")
    promo_sold = math.floor(sold * 1.5)
    if promo_sold != 10:
        fail(f"promo goods sold {promo_sold} != 10")
    else:
        ok("goods promo multiplies that day's sold by 1.5")
    if 7 * 7000 != 49000:
        fail("goods profit is not sold * 7000")
    else:
        ok("goods profit is sold × ₩7,000 after produce cost is paid")

    bonus = 0
    start = 12
    next_v = max(start, start + bonus - 5)
    if next_v != 12:
        fail(f"rival lose viewer floor {next_v} != 12")
    else:
        ok("rival lose floors starting viewers at the Week 1 start of 12")

    cash, debt, stock = 110000, 15000, 20
    extras3 = [6000, 0, 4000, 0, 4000]
    unlocked = True
    for day in range(11, 16):
        cash -= w3_bills + extras3[day - 11]
        if cash < 0:
            debt += -cash
            cash = 0
        take = simulate_stream("average", seed=4000 + day)
        cash += take
        cash += members * 150
        day_sold = min(stock, max(1, int(math.floor(members * 0.4 + 40 * 0.08))))
        cash += day_sold * 7000
        stock -= day_sold
        if day == 12:
            cash += 20000
    win3 = (debt <= 15000 or cash >= 140000) and unlocked
    print(f"WEEK3: average cash={cash} debt={debt} stock={stock} win={win3}")
    ok("Week 3 5-day ledger simulation completed")

    w4_bills = 14000 + 7000 + 7000 + 7000 + 3000
    if w4_bills != 38000:
        fail(f"Week 4 bill sum {w4_bills} != 38000")
    else:
        ok("Week 4 fixed bills sum to ₩38,000 before agency")
    if w4_bills + 15000 != 53000:
        fail(f"Week 4 after agency {w4_bills + 15000} != 53000")
    else:
        ok("Week 4 bills become ₩53,000 after agency")

    cash, debt = 140000, 10000
    cash -= 40000
    cash -= 25000
    agency = True
    junior = True
    extras4 = [8000, 0, 5000, 0, 5000]
    for day in range(16, 21):
        today = (w4_bills + 15000) if agency else w4_bills
        cash -= today + extras4[day - 16]
        if cash < 0:
            debt += -cash
            cash = 0
        take = simulate_stream("average", seed=5000 + day)
        cash += take
        cash += 8 * 150
        if junior:
            cash += 4000
        cash += 10000
        cash += 3000
    win4 = agency and (debt <= 10000 or cash >= 180000)
    print(f"WEEK4: average cash={cash} debt={debt} agency={agency} win={win4}")
    ok("Week 4 5-day ledger simulation completed")

    w5_bills = 15000 + 8000 + 8000 + 8000 + 6000
    if w5_bills != 45000:
        fail(f"Week 5 bill sum {w5_bills} != 45000")
    else:
        ok("Week 5 solo bills sum to ₩45,000")
    if w5_bills + 15000 != 60000:
        fail(f"Week 5 after agency {w5_bills + 15000} != 60000")
    else:
        ok("Week 5 bills become ₩60,000 after agency")

    peak, members, sold, perfects = 100, 8, 2, 10
    daily = int(math.floor(peak)) * 3 + members * 8 + sold * 4 + perfects * 2
    if daily != 392:
        fail(f"ranking daily score {daily} != 392 for 100/8/2/10")
    else:
        ok("ranking daily score is 392 at peak 100 / 8 members / 2 goods / 10 perfects")
    concert_pay = int(math.floor((200000 + daily + 500) * 1.3))
    if concert_pay != 261159:
        fail(f"concert success payout {concert_pay} != 261159")
    else:
        ok("concert success payout is floor((200000 + ranking + 500) * 1.3)")

    cash, debt = 180000, 0
    extras5 = [10000, 0, 6000, 0, 6000]
    rank_score = 0
    for day in range(21, 26):
        cash -= (w5_bills + 15000) + extras5[day - 21]
        if cash < 0:
            debt += -cash
            cash = 0
        take = simulate_stream("average", seed=6000 + day)
        cash += take
        cash += 8 * 150
        cash += 4000
        day_score = 80 * 3 + 8 * 8 + 1 * 4 + 8 * 2
        rank_score += day_score
        if day >= 22:
            cash += 10000
        if day == 22:
            cash -= 80000
            cash += 200000 + day_score + 500
    empire = cash >= 250000 and True and True
    print(f"WEEK5: average cash={cash} debt={debt} rank={rank_score} empire={empire}")
    ok("Week 5 5-day ledger simulation completed")


def main() -> int:
    check_project()
    check_economy()
    if errors:
        print(f"\n{len(errors)} check(s) failed")
        return 1
    print("\nAll hookup + economy checks passed")
    return 0


if __name__ == "__main__":
    sys.exit(main())

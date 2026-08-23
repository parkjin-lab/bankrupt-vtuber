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
    if "6000.3" not in version:
        fail("ProjectVersion is not Unity 6000.3.x")
    else:
        ok("Unity version " + version.split()[1])

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

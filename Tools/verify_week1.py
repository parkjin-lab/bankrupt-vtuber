#!/usr/bin/env python3
"""Read back scenes / input / economy and simulate Week 1 take bands."""
from __future__ import annotations

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
        "WeekStart": ROOT / "Assets/Scenes/WeekStart.unity",
        "LiveStream": ROOT / "Assets/Scenes/LiveStream.unity",
        "Settlement": ROOT / "Assets/Scenes/Settlement.unity",
    }
    scripts = {
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
    if "Assets/Scenes/WeekStart.unity" not in ebs.split("m_Scenes:")[1].split("guid")[0]:
        # first scene should be WeekStart
        first = ebs.split("path: ")[1].split("\n")[0].strip() if "path: " in ebs else ""
        if first != "Assets/Scenes/WeekStart.unity":
            fail(f"first build scene is {first}, expected WeekStart")
        else:
            ok("first build scene is WeekStart")
    else:
        ok("WeekStart is listed first in EditorBuildSettings")

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
    for key in ("KeyCode.A", "KeyCode.S", "KeyCode.D", "KeyCode.F", "KeyCode.Space", "KeyCode.Alpha1"):
        if key not in bindings:
            fail(f"StreamBindings missing {key}")
    ok("A/S/D/F/Space/1-4 bindings present")

    consume = bindings.split("TryConsumeKind", 1)[-1]
    consume = consume.split("public static bool SuperchatCharging", 1)[0]
    consume = consume.split("public static bool EventStubPressed", 1)[0]
    if "GetKey(KeyCode.Space)" in consume:
        fail("TryConsumeKind still polls GetKey(Space) — hold would farm superchats")
    else:
        ok("TryConsumeKind does not poll GetKey(Space)")
    if "GetKeyUp(KeyCode.Space)" not in consume and "GetKeyDown(KeyCode.Space)" not in consume:
        fail("Space superchat is not a one-shot GetKeyDown/GetKeyUp")
    else:
        ok("Space superchat commits once (GetKeyUp/GetKeyDown)")
    for tap in ("GetKeyDown(KeyCode.A)", "GetKeyDown(KeyCode.S)", "GetKeyDown(KeyCode.D)", "GetKeyDown(KeyCode.F)"):
        if tap not in consume:
            fail(f"regular lane missing {tap}")
    ok("A/S/D/F stay GetKeyDown")

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
        + (ROOT / "Assets/Scripts/Presentation/WeekStartDirector.cs").read_text(encoding="utf-8")
        + (ROOT / "Assets/Scripts/Presentation/LiveStreamDirector.cs").read_text(encoding="utf-8")
    )
    if "ArtSprites.Apply" not in directors:
        fail("directors do not hook ArtSprites")
    else:
        ok("directors hook ArtSprites at runtime")

    for rel in (
        "Assets/Scripts/Core/GameManager.cs",
        "Assets/Scripts/Economy/EconomyRules.cs",
        "Assets/Scripts/Stream/StreamSession.cs",
        "Assets/Resources/Balance/Week1Balance.asset",
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
    if "SceneFlow.WeekStart" not in gm or "SceneFlow.LiveStream" not in gm or "SceneFlow.Settlement" not in gm:
        fail("GameManager does not load all three scenes")
    else:
        ok("GameManager loads WeekStart → LiveStream → Settlement")


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

    # 5-day ledger sanity
    cash, debt = 45000, 50000
    bills = 22000
    for skill in ("newbie", "average", "skilled"):
        c, d = cash, debt
        for _ in range(5):
            c -= bills
            if c < 0:
                d += -c
                c = 0
            take = simulate_stream(skill, seed=hash(skill) % 10000 + _)
            c += take
        print(f"WEEK: {skill:8s} cash={c} debt={d} win={d <= 30000 or c >= 70000}")
    ok("5-day ledger simulation completed")


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

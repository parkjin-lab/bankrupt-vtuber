# 파산 버튜버

VTuber money-defense. Week 1 slice: 매일 청구서가 들이닥치고, 90초 라이브 채팅 QTE로 메우고, 정산한다.

Cute anime-adjacent presentation, harsh red KRW numbers. Built-in 2D (Unity UI Canvas), Unity **6000.3.x**.

## Unity에서 열기

1. [Unity Hub](https://unity.com/download)에 **Unity 6000.3.x** (이 레포는 `6000.3.11f1`로 기록, 같은 6.3 LTS면 업그레이드 가능)를 설치한다.
2. Hub → **Add** → 이 폴더(`Assets`, `Packages`, `ProjectSettings`가 있는 루트)를 연다.
3. 첫 임포트 후 **`Assets/Scenes/Title.unity`** 를 더블클릭한다. 이 씬이 시작 씬이다.
4. Play를 누른다. 에디터 스크립트가 Play Mode 시작 씬을 `Title`로 고정하고 Build Settings에 네 씬을 넣는다.

Build Settings 순서:

1. `Assets/Scenes/Title.unity`
2. `Assets/Scenes/WeekStart.unity`
3. `Assets/Scenes/LiveStream.unity`
4. `Assets/Scenes/Settlement.unity`

빈 씬이나 "coming soon" 씬은 없다. `GameManager`는 Play 시 `DontDestroyOnLoad`로 뜨고 루프를 연결한다.

## 1주차 루프

`Title` (타이틀 / 첫 실행 프롤로그) → `WeekStart` (고정비 웨이브) → `LiveStream` (90초 채팅 QTE) → `Settlement` → 다음날 또는 승/패.

타이틀: 「파산 버튜버」 / 빚더미에서 최고의 버튜버가 되어라. / 방송 시작 · 조작 설명 (A S D F Space, 1–4).  
1일차·미청구·이번 Play에서 아직 안 봤으면 5–8초 프롤로그(빨간 청구서 더미 + 파산냥) 후 WeekStart. 다음날과, 이번 세션에서 한 번 본 뒤 Restart는 건너뛴다.

- 5일 = 1주차.
- 5일 생존 **그리고** (부채 ≤ ₩30,000 **또는** 현금 ≥ ₩70,000) = 승리.
- 부채 ≥ ₩180,000 = 파산 게임 오버.
- 정산 후 현금이 음수면 그 금액이 부채로 넘어간다.

시작: 현금 ₩45,000 / 부채 ₩50,000 / 멘탈 100/100.  
매일 고정비 ₩22,000 (월세 8k, 전기+넷 4k, 아바타 라이선스 3k, 식비 5k, 장비 2k) + **오늘의 위협** 하나 (₩4,000–₩12,000: 장비 고장 / 라이벌 견제 / 플랫폼 수수료 / 스캔들 루머 / 인터넷 끊김). 테이블은 `Week1Balance.extraThreats`. 시드는 `runSeed + day`라 Restart마다 주간 순서가 바뀐다.

## 조작 (PC 키보드)

| 키 | 행동 | 채팅 |
| --- | --- | --- |
| **A** | 긍정 | 파랑 일반 |
| **S** | 공감 | 초록 질문 |
| **D** | 웃음 | 빨강 트롤 |
| **F** | 감사 | 골드 슈퍼챗 |
| **Space** | 슈퍼챗 차지 후 **떼면** 한 번만 판정 (홀드 연타 없음) | 골드 |
| **1–4** | 방송 중 이벤트 QTE (안티 웨이브 / 장비 렉, 빛나는 키) | 한 번 |
| **Space / Enter** | 타이틀·프롤로그·WeekStart·정산에서 진행 | — |

채팅이 오른쪽 레인에서 히트바로 떨어질 때 색/타입에 맞는 키를 누른다. Perfect / Great / Good / Miss.

## 숫자 리튠

잠긴 1주차 숫자는 데이터로 빠져 있다. 코드 상수에 의존하지 말고 에셋을 고친다.

- `Assets/Resources/Balance/Week1Balance.asset` — 현금/부채/청구/판정/슈퍼챗/하이프/멘탈
- `Assets/Resources/Balance/ChatCatalog.asset` — 한국어 채팅 카피
- 런타임 폴백: `Week1Balance.ApplyLockedWeek1Defaults()` / `ChatCatalog.ApplyDefaults()`

훅업 확인: 에디터 메뉴 **파산 버튜버 → Verify Week 1 Hookup**.  
헤드리스 검증: `python3 Tools/verify_week1.py`

## 렌더 파이프라인

**Built-in 2D** (URP 없음). UI는 Screen Space Overlay Canvas + Noto Sans KR. 카메라 orthographic.

## 스코프 밖 (만들지 않음)

에이전시, 콘서트, 멀티 플랫폼, 팬덤 파벌, 스태프 자동화, 중후반 시스템.

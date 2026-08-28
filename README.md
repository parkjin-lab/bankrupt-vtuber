# 파산 버튜버

90초 라이브 한 판으로 이번 주 청구를 막거나, 못 막으면 끝나는 세로 2D 프로토타입이다.

**Unity 6000.5.9f1** (`b57deb96f08d`) · **Android Portrait** · **`Assets/Scenes/Title.unity`** · 브랜치 **`cursor/week1-playable-loop-133f`**

숫자·연출은 이 브랜치에 박혀 있다. 노트북 클론에서는 에디터만 맞추면 바로 플레이할 수 있다.

## 클론 후 플레이

1. 이 저장소를 클론한다.
2. Unity Hub에서 **6000.5.9f1** (`b57deb96f08d`)을 설치한다.
3. **Add** → 저장소 루트를 연다. Built-in 2D, URP/HDRP 없음.
4. `Assets/Scenes/Title.unity`를 연 뒤 Play. 폰은 세로. 안드로이드 빌드는 Portrait로 열린다.

씬 네 장: `Title` → `WeekStart` → `LiveStream` → `Settlement`.

## 1주차 숫자 (잠김)

리튠은 `Assets/Resources/Balance/` 에셋. 여기서 숫자를 새로 만들지 않는다.

| | |
|---|---|
| 시작 | 현금 ₩45,000 / 부채 ₩50,000 / 멘탈 100 |
| 매일 고정비 | ₩22,000 × 5일 (월세 8천 + 전기 4천 + 라이선스 3천 + 식비 5천 + 장비 2천). 그날 추가 위협이 붙을 수 있다 |
| 라이브 | 90초 |
| 파산 | 부채 ≥ ₩180,000 |
| 클리어 | 5일 생존 + (부채 ≤ ₩30,000 또는 현금 ≥ ₩70,000) |

## 조작

라이브에서 화살표 · A/S/D/F · WASD · 화면 패드가 같은 노트에 들어간다. 패드/키를 누르면 그 패드가 0.08초 밝아지고 `Audio/sfx_pad` 키캡 클릭이 한 번 나며 **입력됨**이 뜬다(판정·슈퍼챗 성공 차임은 그대로 히트 때).

| 입력 | 동작 |
|---|---|
| ← / A / 왼쪽 패드 | 긍정 |
| ↓ / S / 아래 패드 | 공감 |
| → / D / 오른쪽 패드 | 웃음 |
| ↑ / F / W / 감사 패드 | 감사 |
| Space / Enter **떼면** 한 번 | 슈퍼챗 놓기 (홀드 연타 없음) |
| 1 / 2 / 3 / 4 | 이벤트 QTE (안티 웨이브 / 장비 렉, 빛나는 키) |
| F9 | 다음 주차 첫날 아침으로 점프 (에디터 / DEVELOPMENT) |
| F10 | 오늘 방송을 건너뛰고 정산 (에디터 / DEVELOPMENT) |
| 1일차 코치 | 새 런 첫 라이브만. `Art/coach_card` 책상 스티키 카드 위에 노트가 히트바에서 멈추고 `Art/pad_*` 키캡 + **← 긍정** / Space 같은 한 줄이 뜬다(레전드에 ←↓→↑ Space). 성공 3회 또는 8초면 끝. |

## 한 판 루프

**Title**은 `Art/title_studio` 파산 스튜디오 바탕 위에 `Art/title_wordmark` 네온 로고 플레이트(**「파산 버튜버」** 골드, 1.04 펄스)·버튼이 그대로 올라간다. `Audio/bgm_title` 불안한 네온 루프(로비만. 떠나면 0.2초 페이드). **새 방송 시작**은 `Art/title_start` 스트림덱 키캡 + 작은 **시작** 칩과 함께 1.03으로 숨쉰다. 세이브가 있으면 **이어서 하기**도 `Art/title_continue` 키캡 + 작은 **이어** 칩과 함께 1.03으로 숨쉬고(세이브 없으면 키캡 숨김), **n일차**가 아침과 같은 `Art/day_tab` 달력 탭(세이브 없으면 탭 숨김) + 현금(`Art/cash_slip` 영수증. 청구를 알면 부족은 빨강 + 아침·정산과 같은 `Art/bill_short` 스탬프 **청구보다 부족**. 세이브 없거나 충분하면 스탬프 숨김)/부채(`Art/bill_notice` 고지서. 골드)/멘탈(`Art/mental_note` 스티키. `멘탈 N`. ≤20이면 빨강. 세이브 없으면 영수증·고지서·메모 숨김). 어제 헤드라인이 있으면 그 줄이 `Art/headline_clip` 신문 스크랩으로 이어하기 옆에 붙는다(세이브·헤드라인 없으면 숨김). **새 방송 시작**은 `Art/newgame_card` 지우기 고지 카드에 **진행 중인 n일차를 지울까?** 확인(**지우고 시작** / **취소**) 후 지운다. 로비를 떠나면(`방송 시작` / `이어서 하기` / `지우고 시작`) `Audio/sfx_title` 확인이 한 번 난다.

**Title** → **WeekStart**는 `Art/morning_room` 청구 아침 방 위에 `Audio/bgm_morning` 불안한 청구 루프(타이틀보다 작고 날카로움. **방송 켜기**/콘서트 시작으로 떠나면 0.2초 페이드)와 **n일차**가 `Art/day_tab` 찢긴 달력 탭 위에서 크게 0.25초 팝 → **오늘 청구**가 `Art/bill_notice` 고지서 칸에서 0.25초 슬램(**현금**은 정산·타이틀과 같은 `Art/cash_slip` 영수증. 청구보다 적으면 경고 빨강 + 정산과 같은 `Art/bill_short` 스탬프 **청구보다 부족**. 막히면 그대로. **멘탈**은 정산·라이브·타이틀과 같은 `Art/mental_note` 스티키 메모에 `N/100`) → 청구 카드 → 콘텐츠 픽(토크/게임/노래/리액션, 네 장 모두 `Art/content_plate` 스트림 카드 플레이트 + `Art/content_*` 아이콘 + 액센트 + 편하게 잡담 / 같이 깨자 / 고음 승부 / 같이 보자. 고르면 `Audio/sfx_pick` 확인이 한 번) → **방송 켜기**(콘서트면 **콘서트 방송**)가 `Art/golive_key` 스트림덱 키캡 + 빨간 LIVE 핍과 함께 1.04로 숨쉬고, 누르면 `Audio/sfx_golive` 확인 후쉬가 한 번 난 뒤 아침 베드가 0.2초 페이드 → 라이브 화면의 0.6초 **ON AIR**(`Audio/sfx_onair`)는 그대로 → **90초 라이브** → **Settlement**.

웹캠 파산냥은 `Art/pasan_nyang` 256px 2D 얼굴이고, 바깥은 `Art/webcam_bezel` 스트림 캠 베젤이다. 3주차 라이벌 캠(`Art/rival_nyang`)도 같은 `Art/webcam_bezel`을 쓴다. Perfect 펀치 · Good 끄덕 · Miss 흔들 · 멘탈 ≤40 지침 · 시청 틱·승/패 SFX는 그대로다.

라이브는 `Art/onair_led` LED 배지 **ON AIR** / **방송 시작**(0.6초, `Audio/sfx_onair` 시작 스팅)과 함께 `Audio/bgm_stream`이 타이틀보다 작게 루프한다. 웹캠 코너의 작은 `Art/onair_led` **ON AIR**는 90초 내내 켜져 있고, 마지막 10초는 시계와 같이 깜빡이다가 **방송 종료**에 꺼진다. 5주차 콘서트 라이브만 `Audio/bgm_concert`가 스트림보다 크고 밝게 루프하고, 판정·슈퍼챗·하이프 SFX는 그대로 위에 뜬다. HUD가 이어진다. 바탕은 `Art/stream_overlay` 2D 오버레이(LIVE 핍 · 웹캠 베젤 · 채팅 테두리). 하단 키는 `Art/pad_*` 스트림덱 키캡(←파랑 / ↓초록 / →트롤 / ↑골드 / 슈퍼챗 골드). 칩 · 히트바 · FX · 0.08초 프레스 플래시는 그대로다. 1일차 코치는 `Art/coach_card` 스티키 카드로 그 다음. 90초가 끝나면 `Art/end_cut` 컷 카드 **방송 종료**(검정 플래시 · LIVE 점 꺼짐, 0.5초, `Audio/sfx_end_cut` 컷 스팅 · 베드 0.2초 페이드) 뒤 정산. F10 스킵은 조용히 정산으로. 정산은 `Art/settlement_desk` 방송 끝난 책상 바탕 위에 기존 카드가 그대로 올라간다.

라이브 HUD 스택(책상 종이 + 배지 + 스탬프 + 피크/사고 오버레이). 자세한 목록은 아래 **라이브 HUD 스택** / **책상 종이** / **돈 스탬프 · 팝 슬립**.

- 지금 수입 · 오늘 청구 · 파산까지 — **지금 수입**은 책상·아침·타이틀과 같은 `Art/cash_slip` 영수증 칩. HUD에 **청구 ₩N**이 아침 **오늘 청구**·정산 **부채**와 같은 `Art/bill_notice` 고지서 칩으로 내내 붙어 있고, 옆에 `Art/bill_bar` 찢긴 영수증 스트립이 지금 수입/오늘 청구를 채운다(모자라면 빨강, 꽉 차면 골드. 슬램은 기존 한 번). 오늘 픽은 **토크 / 게임 / 노래 / 리액션** 칩(아침과 같은 `Art/content_plate` + `Art/content_*` 아이콘 + 카드 액센트 색)으로 아침과 같다. 슈퍼챗이 히트 0.4초 전에 패드에 `Art/superchat_pip` 금빛 **슈퍼챗** 핍. 성공하면 `Art/superchat_fly` 금 봉투에 ₩이 수입 칸으로 날아가며 `Audio/sfx_superchat` 골드 코인 차임. 빗나가면 크랙(+미스 쿵). 성공 노트는 지금 수입 옆에 `Art/won_pop` 작은 현금 슬립 **+₩**. 마지막 10초는 `Art/clock_plate` 방송 시계 배지가 빨강으로 뛰며 **10…9…** 초마다 `Audio/sfx_clock_tick` 틱(0은 틱 없이 **종료**)이고, 웹캠 `Art/onair_led` **ON AIR**도 같이 깜빡인다(이어 `Art/end_cut` **방송 종료** 스팅).
- 떨어지는 채팅은 `Art/chat_bubble` 다크 채팅 필(흰 글칸)에 한국어 닉(`Art/chat_nick` 작은 네임플레이트) + 대사. 트롤/안티 채팅 닉은 `Art/chat_troll` 빨간 네임플레이트(팬 닉과 구분). 슈퍼챗 닉은 `Art/chat_super` 금 네임플레이트(유료 채팅 구분). 왼쪽에 `Art/note_chip` 화살 젬이 패드 색으로 칠해진다(←파랑 / ↓초록 / →트롤 / ↑골드, 회전에 맞춤). 슈퍼챗은 골드 배너 + `Art/superchat_chip` 금 봉투 칩(텔레그래프·₩ 플라이·SFX·패드 그대로). 노트 아래에는 `Art/hit_rail` 다크 레인+밝은 스트라이크 포켓이 깔리고, 그 위에 기존 흰/골드 **타이밍** 스트라이크가 퍼펙트 창에서 펄스한다. 히트라인 0.15초 안(퍼펙트 구간)에 들어온 노트는 밝아진다. 민준 첫 도네는 스탬프.
- 하이프 시작 시 `Audio/sfx_hype` 상승 치어(한 번) · 금빛 워시 · `Art/hype_frame` 골드 스페셜 오버레이 프레임 · `Art/hype_chip` 골드 칩 **하이프 N**(남은 초. 12초·콤보 9 Perfect 그대로) · 채팅이 기존 카탈로그/닉으로 약 2배 떨어짐(끝나면 원래 속도) · 멘탈 위험(`Art/mental_note` 같은 스티키 메모. `Audio/sfx_mental` 불안 스팅 한 번, 칩이 처음 뜰 때만)/강제 종료 워시 · 시청 ± 팝업(`Art/viewer_pop` 작은 팔로워 칩. `Art/viewer_badge` 배지가 1.12로 0.1초 팝. 오르면 초록, 떨어지면 빨강. 값·수식 그대로). Perfect는 `Art/judge_perfect` 금색 스탬프 **PERFECT**(0.2초, 큼) + 웹캠 1.08 펀치/흰 플래시 0.12초 + `Audio/sfx_perfect` 밝은 틱. Good은 `Art/judge_good` 흰 스탬프 작은 **GOOD** + 작은 끄덕 + `Audio/sfx_good` 부드러운 탭. Miss는 `Art/judge_miss` 빨간 X 스탬프 **MISS** + 기존 흔들/스카 + `Audio/sfx_miss` 둔탁한 쿵. 콤보가 오르면 `Art/combo_plate` 스트림 배지 칩이 0.1초 팝(1.15, 5+면 1.22). 콤보 2 이상에서 미스면 `Art/combo_break` 빨간 스탬프 **콤보 끊김**(빨강 0.25초, `Audio/sfx_combo_break`) 뒤 COMBO 0. 첫 노트 미스는 그냥 Miss. 슈퍼챗·온에어(`Art/onair_led`) SFX는 그대로.
- 위협 오버레이 · 이벤트 카드. 안티 웨이브 / 장비 렉은 0.5초 전에 `Art/event_warn` 경고 플레이트 **안티 온다** / **렉 온다** 가 뜬 뒤, 실제 발화 때 `Audio/sfx_anti` 야유 + `Art/anti_sting` 빨간 야유 오버레이 / `Audio/sfx_lag` 글리치 + `Art/lag_sting` 정적 오버레이. 청구를 넘기는 순간 `Art/bill_cover` 금색 PAID 스탬프 **청구 커버** 슬램(한 판 한 번, 이후 초록 고정) + `Audio/sfx_bill_cover` 캐시 레지스터 스팅.

정산: `Audio/bgm_settlement` 조용한 책상 루프(타이틀보다 작고 지친 톤. **다음날** / 클리어 / 파산·처음부터로 떠나면 0.2초 페이드. 수입 카운트·청구 미달·다음날 펄스는 그대로) 위에 **n일차**가 아침·타이틀과 같은 `Art/day_tab` 달력 탭 + **오늘 헤드라인**(`Art/headline_clip` 같은 신문 스크랩. 카피·카운트·미달·라우팅은 그대로) + 오늘 픽 한 줄(**오늘 토크 / 오늘 게임 / 오늘 노래 / 오늘 리액션**, 아침·라이브와 같은 `Art/content_plate` + `Art/content_*` 아이콘 + 카드 액센트 색) → **오늘 수입**이 `Art/cash_slip` 영수증 칸에서 0에서 실지급까지 0.6초 카운트(청구를 넘기면 기존 커버 골드 한 번. 못 막으면 스냅 뒤 `Art/bill_short` 빨간 스탬프 **청구 미달** 0.35초) → **청구**가 아침 **오늘 청구**·라이브 **청구 ₩N**·정산 **부채**와 같은 `Art/bill_notice` 고지서 칸에서 오늘 청구 금액을 보여 준다 → 부채가 늘었으면 **부채**가 아침과 같은 `Art/bill_notice` 고지서 칸에서 전날 금액에서 오늘 금액까지 0.4초 빨강 카운트(그대로거나 줄면 숫자만, 줄면 약한 초록) → 카운트가 끝나면 **남은 현금**이 `Art/cash_slip` 영수증 스크랩 위에 실제 잔액으로 스냅(내일 고정비가 보이는데 모자라면 경고 빨강 + 아침·타이틀과 같은 `Art/bill_short` 스탬프 **청구보다 부족**. 충분하면 스탬프 숨김) → **멘탈**이 `Art/mental_note` 책상 메모 칸에서 빠졌으면 아침 값에서 오늘 값까지 0.35초 지친 빨강 카운트(오르면 작은 초록 틱, 그대로면 숫자만) → 편지(`Art/letter_card` 분홍 봉투·크림 편지지. **답장하기**는 `Art/letter_reply` 스트림덱 키캡 + `Audio/sfx_letter` 스탬프가 한 번. **나중에**는 `Art/letter_ignore` 키캡·조용. 카피·라우팅·종이·정산 베드는 그대로) → 2주차면 멤버십 해금(`Art/membership_card` 배지·패스) · 클립 업로드(`Art/clip_card` 폰 썸네일. 카피·해금/클립 숫자·라우팅은 그대로) → 그 주 시스템 카드. **다음날**은 `Art/nextday_key` 스트림덱 키캡 + 작은 **다음** 칩과 함께 1.03으로 숨쉬고, 누르면 `Audio/sfx_nextday` 페이지 넘김이 한 번 난다(펄스·라우팅·편지·클리어/파산 스팅·정산 베드 0.2초 페이드는 그대로). 다음날 아침 청구 슬램 위에 **어제: …** 가 `Art/headline_clip` 신문 스크랩으로 붙는다(1일차·재시작은 없음. 카피·세이브·일차는 그대로). 5/10/15/20/25일 아침은 **마지막 날** / **n주차 마지막** 배너. 주 클리어/파산은 전용 화면(`Art/ending_clear` 골드·네온 승리 스튜디오 / `Art/ending_bankrupt` 빨강·어두운 퇴거 방. 카피·버튼·스팅은 그대로). 스플래시가 뜨면 `Audio/sfx_clear` 승리 팡파르 / `Audio/sfx_bankrupt` 붕괴 슬램이 한 번 나고, 정산 베드는 0.2초 페이드.

## 지금 보이는 것 / 들리는 것

방·책상 종이·라이브 HUD 스택·돈 스탬프·팝 슬립·스트림덱 키캡·카드/탭·코치 카드·채팅 네임플레이트·웹캠 베젤·청구 영수증 바·스테이지 아트, 패드, 채팅 버블, 노트, 화면별 BGM(일반 라이브 `bgm_stream` / 콘서트 라이브 `bgm_concert`), 판정·이벤트·엔딩·중반 SFX, 콘텐츠 아이콘, 라이벌 얼굴, 굿즈/에이전시/랭킹/콘서트 아트가 이미 붙어 있다. 아래는 저장소에 있는 아트만 적는다(새로 만들지 않음).

- **방** — `Art/title_studio` · `Art/title_wordmark`(타이틀 **「파산 버튜버」** 네온 로고) · `Art/morning_room` · `Art/settlement_desk` · `Art/stream_overlay` · `Art/ending_clear` / `Art/ending_bankrupt`
- **스트림덱 키캡** — 메뉴·아침·정산 확인 버튼이 같은 키캡 패밀리로 읽힌다(펄스·칩·SFX·라우팅은 그대로).
  - `Art/title_start` — 타이틀 **새 방송 시작** (`sfx_title`, **시작** 칩)
  - `Art/title_continue` — 타이틀 **이어서 하기** (`sfx_title`, **이어** 칩. 세이브 없으면 숨김)
  - `Art/golive_key` — 아침 **방송 켜기** / **콘서트 방송** (`sfx_golive`, LIVE 핍)
  - `Art/nextday_key` — 정산 **다음날** (`sfx_nextday`, **다음** 칩)
  - `Art/letter_reply` / `Art/letter_ignore` — 팬레터 **답장하기** / **나중에** (`sfx_letter`는 답장만)
- **카드 / 탭** — 타이틀·아침·라이브·정산 카드 플레이트가 이미 붙어 있다(숫자·라우팅·SFX는 그대로).
  - `Art/title_wordmark` — 타이틀 **「파산 버튜버」** 네온 로고 플레이트 (골드·1.04 펄스)
  - `Art/content_plate` — 같은 스트림 카드 플레이트 세 곳: 아침 콘텐츠 픽 네 장 · 라이브 **오늘 픽** 칩 · 정산 **오늘 토크 / 오늘 게임 / 오늘 노래 / 오늘 리액션** 줄 (`content_*` 아이콘·액센트·`sfx_pick`)
  - `Art/letter_reply` / `Art/letter_ignore` — 팬레터 **답장하기** / **나중에** 키캡 (`letter_card` 종이 위. `sfx_letter`는 답장만)
  - `Art/newgame_card` — 타이틀 새 방송 지우기 고지 (**진행 중인 n일차를 지울까?** / **지우고 시작** / **취소**)
  - `Art/day_tab` — 같은 달력 탭 세 곳: 아침 **n일차**(골드·0.25초 슬램) · 타이틀 **이어서 하기** **n일차**(세이브 없으면 숨김) · 정산 **n일차**
  - `Art/coach_card` — **코치 카드**(1일차 책상 스티키. `pad_*` 키캡·←↓→↑ Space 바인딩. Day-1만)
- **책상 종이** — 같은 영수증·고지서·스티키·스크랩이 타이틀 / 아침 / 라이브 / 정산 / 클리어·파산을 오간다(숫자·라우팅은 그대로).
  - **영수증 `Art/cash_slip`** — 타이틀 **이어서 하기** 현금 · 아침 **현금** · 라이브 **지금 수입** · 정산 **오늘 수입** / **남은 현금** · 클리어/파산 **현금** (모자라면 경고 빨강. 세이브 없으면 로비 영수증 숨김. 카운트 그대로). 타이틀·아침 부족 줄·정산 **청구 미달**·정산 **남은 현금** 부족은 같은 `Art/bill_short` 빨간 스탬프(**청구보다 부족** / **청구 미달**. 세이브 없거나 충분하면 스탬프 숨김). 히트 **+₩** 팝은 작은 `Art/won_pop` 슬립(수입 칩·슈퍼챗 플라이·값은 그대로)
  - **고지서 `Art/bill_notice`** — 같은 고지서 여섯 곳: 아침 **오늘 청구** · 타이틀 **이어서 하기** **부채** · 라이브 **청구 ₩N** 칩 · 정산 **부채** · 정산 **오늘 청구** · 클리어/파산 **부채** (세이브 없으면 로비 고지서 숨김. 늘면 빨강 카운트. 라이브 필/PAID는 `bill_bar` / `bill_cover`)
  - **멘탈 메모 `Art/mental_note`** — 정산 **멘탈** · 라이브 **멘탈 위험** · 아침 **멘탈** · 타이틀 **이어서 하기** 멘탈 · 클리어/파산 **멘탈** (세이브·위험 없으면 숨김. 카운트·빨강/초록·워시·스팅 그대로)
  - **헤드라인 `Art/headline_clip`** — 정산 **오늘 헤드라인** · 아침 **어제:** · 타이틀 **이어서 하기** (세이브·헤드라인 없으면 로비 스크랩 숨김)
- **돈 스탬프 · 팝 슬립** — 이미 붙은 머니 피드백만 모은다(숫자·경제·청구 수식은 그대로).
  - `Art/bill_bar` — 라이브 **청구 영수증 바**(찢긴 영수증 필 스트립. 빨강 부족 / 골드 풀. `bill_notice` 칩·`bill_cover` PAID·`sfx_bill_cover` 그대로)
  - `Art/bill_cover` — 라이브 **청구 커버** 금색 PAID 슬램(`sfx_bill_cover`, 한 판 한 번)
  - `Art/won_pop` — 히트 **+₩** 작은 현금 슬립(지금 수입 `cash_slip` 옆)
  - `Art/superchat_fly` — 슈퍼챗 성공 ₩ 플라이 금 봉투(`superchat_chip` 노트·`superchat_pip`·`sfx_superchat` 그대로)
  - `Art/viewer_pop` — 시청 ± 작은 팔로워 칩(`viewer_badge` 1.12 팝·초록/빨강 그대로)
  - `Art/bill_short` — **부족 스탬프** 공유 재사용: 타이틀 **이어서 하기** · 아침 **현금** · 정산 **청구 미달** · 정산 **남은 현금** (**청구보다 부족** / **청구 미달**. 같은 PNG. 충분하면 숨김)
- **라이브 HUD 스택** — 라이브 한 판에서 보이는 오버레이·스탬프·칩(히트창·콤보·하이프·이벤트·경제 숫자는 그대로).
  - **칩 / 배지** — `Art/combo_plate`(**COMBO**) · `Art/viewer_badge`(**시청자**) · `Art/viewer_pop`(시청 ± 칩) · `Art/hype_chip`(**하이프 N** 남은 초) · `Art/clock_plate`(**남은 시간**, 마지막 10초 · `sfx_clock_tick`) · `Art/onair_led` — **ON AIR** 네 박: 시작 0.6초 스팅(`sfx_onair` · **방송 시작**) · 웹캠 코너 90초 점등 · 마지막 10초 시계와 같이 깜빡임 · **방송 종료**에 꺼짐
  - **판정 스탬프** — `Art/judge_perfect`(**PERFECT**) · `Art/judge_good`(**GOOD**) · `Art/judge_miss`(**MISS**) · `Art/combo_break`(**콤보 끊김**, `sfx_combo_break`)
  - **노트 / 컷** — `Art/superchat_chip`(슈퍼챗 금 봉투) · `Art/superchat_pip`(0.4초 금 핍, `sfx_superchat`) · `Art/superchat_fly`(성공 ₩ 플라이 금 봉투) · `Art/won_pop`(히트 **+₩** 현금 슬립) · `Art/bill_cover`(**청구 커버** PAID 스탬프, `sfx_bill_cover`) · `Art/bill_short`(타이틀·아침·정산 **남은 현금** **청구보다 부족** / 정산 **청구 미달**) · `Art/end_cut`(**방송 종료**, `sfx_end_cut`)
  - **피크 / 사고** — `Art/hype_frame`(하이프 골드 프레임, `sfx_hype`) · `Art/hype_chip`(**하이프 N**) · `Art/event_warn`(**안티 온다** / **렉 온다**) · `Art/anti_sting`(`sfx_anti`) · `Art/lag_sting`(`sfx_lag`)
- **패드 / 채팅 / 노트** — 라이브 키캡·버블·닉 네임플레이트가 이미 붙어 있다(스폰·카탈로그·경제 숫자는 그대로).
  - `Art/pad_*` — 라이브 키캡 (←파랑 / ↓초록 / →트롤 / ↑골드 / 슈퍼챗 골드). 위 **스트림덱 키캡**(`title_start` / `title_continue` / `golive_key` / `nextday_key`)
  - `Art/chat_bubble` — 일반 채팅 다크 필(흰 글칸)
  - `Art/chat_nick` — 일반 채팅 닉 분홍 네임플레이트
  - `Art/chat_troll` — 트롤/안티 닉 빨간 네임플레이트
  - `Art/chat_super` — 슈퍼챗 닉 금 네임플레이트 (봉투·플라이·핍·`sfx_superchat` 그대로)
  - `Art/note_chip` · `Art/superchat_chip` 금 봉투 · `Art/hit_rail` · leftover HUD `Art/hype_chip`(**하이프 N**) · `Art/superchat_fly`(성공 ₩ 플라이 금 봉투)
- **얼굴 / 아이콘** — 웹캠 얼굴·베젤·쇼 아이콘이 이미 붙어 있다(펀치·지침·라이벌 규칙·경제 숫자는 그대로).
  - `Art/pasan_nyang` — 플레이어 웹캠 얼굴
  - `Art/webcam_bezel` — **웹캠 베젤**(플레이어 라이브 캠 + 3주차 라이벌 캠 공유)
  - `Art/rival_nyang` — 라이벌 웹캠 얼굴
  - `Art/content_*` — 토크/게임/노래/리액션 아이콘 (픽 카드는 위 **카드 / 탭** `content_plate`)
- **주차 카드** — `Art/goods_stand` · `Art/agency_card` · `Art/sponsor_card` · `Art/ranking_board` · `Art/concert_stage` · `Art/letter_card`(팬레터 종이. 답장/나중이는 위 **카드 / 탭** `letter_reply` / `letter_ignore`) · `Art/membership_card` · `Art/clip_card`
- **BGM** — Title `Audio/bgm_title` · 아침 `Audio/bgm_morning` · 라이브 `Audio/bgm_stream` · 콘서트 라이브만 `Audio/bgm_concert`(일반 라이브는 `bgm_stream` 유지) · 정산 `Audio/bgm_settlement` (각 화면에서 떠나면 0.2초 페이드)
- **SFX** — 판정 `sfx_perfect` / `sfx_good` / `sfx_miss` · 이벤트 `sfx_anti` / `sfx_lag` · 엔딩 `sfx_clear` / `sfx_bankrupt` · 로비/아침/정산 확인 `sfx_title` / `sfx_pick` / `sfx_golive` / `sfx_nextday` · 답장하기 `sfx_letter` · 라이벌 승/패 `sfx_rival_win` / `sfx_rival_lose` · 멤버십 `sfx_membership` · 클립 `sfx_clip` · 굿즈 `sfx_goods` · 에이전시 `sfx_agency` · 스폰서 `sfx_sponsor` · 랭킹 `sfx_ranking` · 콘서트 개최 `sfx_concert_book`

## 2~5주차 (있는 그대로)

1주차는 청구·멘탈·콤보·이벤트만. 2주차부터 아래가 정산/라이브에 붙는다. 숫자는 코드에 잠겨 있다.

- **2주차** — 멤버십 스플래시(`Art/membership_card` 뱃지. 해금 카드가 뜨면 `Audio/sfx_membership` 한 번. 해금 숫자·카피·라우팅은 그대로) · 클립 카드(`Art/clip_card` 폰/썸네일. 카드가 뜨면 `Audio/sfx_clip` 셔터 한 번. 업로드 숫자·카피·라우팅은 그대로).
- **3주차** — 라이벌 듀얼(`Art/rival_nyang` 웹캠 얼굴 + 플레이어와 같은 `Art/webcam_bezel` + 기존 시청 틱·승/패. 결과는 `Audio/sfx_rival_win` 스틸/치어 / `Audio/sfx_rival_lose` 디플레이트. 플레이어는 `Art/pasan_nyang` 그대로) · 아크릴 해금/`굿즈 홍보`(`Art/goods_stand` 스탠드 제품 그림. 해금 카드·라이브 홍보 카드가 뜨면 `Audio/sfx_goods` 한 번. 해금·홍보 숫자·확인은 그대로) · 홍보 카드.
- **4주차** — 에이전시/후배(`Art/agency_card` 사무실 레터헤드. 설립·스카우트 카드가 뜨면 `Audio/sfx_agency` 한 번. 설립 숫자·카피·확인은 그대로) · 스폰서 멘트(`Art/sponsor_card` 브랜드 계약 타일. 라이브 카드가 뜨면 `Audio/sfx_sponsor` 한 번. 카피·해금·숫자는 그대로).
- **5주차** — 랭킹(`Art/ranking_board` 리더보드 패널. 보드가 뜨면 `Audio/sfx_ranking` 한 번. 순위·숫자는 그대로) · 콘서트(`Art/concert_stage` 나이트 스테이지. 개최 카드가 뜨면 `Audio/sfx_concert_book` 한 번. 라이브만 `Audio/bgm_concert`. 순위·숫자·라우팅은 그대로) · 엔딩 루트(`EndingRoot`).

이름 팬은 **민준** / **하은**. 일반 채팅 닉은 `밤샌사람` 같은 풀. 콘텐츠 네 장(토크/게임/노래/리액션)은 청구 슬램 뒤 고른다.

## 플레이테스트 키

에디터 / DEVELOPMENT 빌드만. 켜지면 오른쪽 위에 **DEBUG  F9 다음 주  F10 오늘 스킵**.

- **F9** — 다음 주차(2/3/4/5) 첫날 아침으로 점프한 뒤 세이브. 5주차면 무시. 이미 적힌 **어제** 헤드라인은 그대로 두고, 비어 있으면 오늘 사실로 한 번 적는다.
- **F10** — 남은 방송을 평균 성공 **₩28,000**으로 건너뛰고 정산. 정산 화면에서는 다음날 아침. 하이프 익스플로잇 아님. 스킵도 `lastHeadline`을 남겨 다음날 **어제:** 가 뜬다.

## 검증

```bash
python3 Tools/verify_week1.py
```

에디터 메뉴 **파산 버튜버 → Verify Week 1 Hookup** 도 같은 스크립트를 돌린다.

## 범위 밖

6주차·글로벌 투어 없음. 타이틀은 1주차 루프만 말한다.

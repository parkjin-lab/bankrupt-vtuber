using System.Collections.Generic;
using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Runtime-loads PNGs from Resources/Art and builds sprites.
    /// Unity generates the texture .meta on first import.
    /// </summary>
    public static class ArtSprites
    {
        public const string Avatar = "Art/pasan_nyang";
        public const string RivalAvatar = "Art/rival_nyang";
        public const string GoodsStand = "Art/goods_stand";
        public const string AgencyCard = "Art/agency_card";
        public const string SponsorCard = "Art/sponsor_card";
        public const string RankingBoard = "Art/ranking_board";
        public const string ConcertStage = "Art/concert_stage";
        public const string BillRent = "Art/bill_rent";
        public const string BillElectric = "Art/bill_electric";
        public const string BillLicense = "Art/bill_license";
        public const string BillFood = "Art/bill_food";
        public const string BillGear = "Art/bill_gear";
        public const string BillNotice = "Art/bill_notice";
        public const string StreamOverlay = "Art/stream_overlay";
        public const string TitleStudio = "Art/title_studio";
        public const string SettlementDesk = "Art/settlement_desk";
        public const string MorningRoom = "Art/morning_room";
        public const string EndingClear = "Art/ending_clear";
        public const string EndingBankrupt = "Art/ending_bankrupt";
        public const string LetterCard = "Art/letter_card";
        public const string HeadlineClip = "Art/headline_clip";
        public const string CashSlip = "Art/cash_slip";
        public const string BillCover = "Art/bill_cover";
        public const string MentalNote = "Art/mental_note";
        public const string ComboPlate = "Art/combo_plate";
        public const string ComboBreak = "Art/combo_break";
        public const string HypeFrame = "Art/hype_frame";
        public const string EventWarn = "Art/event_warn";
        public const string AntiSting = "Art/anti_sting";
        public const string LagSting = "Art/lag_sting";
        public const string ViewerBadge = "Art/viewer_badge";
        public const string ClockPlate = "Art/clock_plate";
        public const string OnAirLed = "Art/onair_led";
        public const string EndCut = "Art/end_cut";
        public const string JudgePerfect = "Art/judge_perfect";
        public const string JudgeGood = "Art/judge_good";
        public const string JudgeMiss = "Art/judge_miss";
        public const string MembershipCard = "Art/membership_card";
        public const string ClipCard = "Art/clip_card";
        public const string PadLeft = "Art/pad_left";
        public const string PadDown = "Art/pad_down";
        public const string PadRight = "Art/pad_right";
        public const string PadUp = "Art/pad_up";
        public const string PadSuperchat = "Art/pad_superchat";
        public const string Superchat = "Art/badge_superchat";
        public const string Troll = "Art/badge_troll";
        public const string BubblePill = "Art/bubble_pill";
        public const string ChatBubble = "Art/chat_bubble";
        public const string NoteChip = "Art/note_chip";
        public const string SuperchatChip = "Art/superchat_chip";
        public const string SuperchatPip = "Art/superchat_pip";
        public const string HitRail = "Art/hit_rail";
        public const string ContentTalk = "Art/content_talk";
        public const string ContentGame = "Art/content_game";
        public const string ContentSong = "Art/content_song";
        public const string ContentReaction = "Art/content_reaction";

        public static string ForContent(StreamContentType type) => type switch
        {
            StreamContentType.Talk => ContentTalk,
            StreamContentType.Game => ContentGame,
            StreamContentType.Song => ContentSong,
            StreamContentType.Reaction => ContentReaction,
            _ => null
        };

        public const string SuperchatBanner = "Art/bubble_superchat";
        public const string TrollBubble = "Art/bubble_troll";
        public const string Sparkle = "Art/sparkle";
        public const string PanelDark = "Art/panel_dark";
        public const string ThreatBanner = "Art/banner_red";
        public const string CashBanner = "Art/banner_green";

        static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
                return null;
            if (Cache.TryGetValue(resourcePath, out var cached) && cached != null)
                return cached;

            var tex = Resources.Load<Texture2D>(resourcePath);
            if (tex == null)
            {
                Debug.LogWarning("[파산 버튜버] missing art " + resourcePath);
                return null;
            }

            tex.filterMode = FilterMode.Bilinear;
            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.name = tex.name;
            Cache[resourcePath] = sprite;
            return sprite;
        }

        public static void Apply(UnityEngine.UI.Image image, string resourcePath, Color? fallbackTint = null, Color? multiply = null)
        {
            if (image == null)
                return;
            var sprite = Get(resourcePath);
            if (sprite == null)
            {
                if (fallbackTint.HasValue)
                    image.color = fallbackTint.Value;
                return;
            }

            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = multiply ?? Color.white;
            image.type = UnityEngine.UI.Image.Type.Simple;
        }

        public static void ApplySliced(UnityEngine.UI.Image image, string resourcePath, Color color, Vector4? border = null)
        {
            if (image == null)
                return;
            var sprite = GetSliced(resourcePath, border ?? new Vector4(48f, 36f, 48f, 36f));
            if (sprite == null)
            {
                Apply(image, resourcePath, color, color);
                return;
            }

            image.sprite = sprite;
            image.type = UnityEngine.UI.Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;
        }

        static Sprite GetSliced(string resourcePath, Vector4 border)
        {
            string key = resourcePath + "|slice";
            if (Cache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            var tex = Resources.Load<Texture2D>(resourcePath);
            if (tex == null)
                return Get(resourcePath);

            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                border);
            sprite.name = tex.name + "_slice";
            Cache[key] = sprite;
            return sprite;
        }
    }
}

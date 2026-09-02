using UnityEngine;

namespace BankruptVtuber
{
    /// <summary>
    /// Presentation-only skin for today's content pick.
    /// Does not touch spawn rates, income, or mental costs.
    /// </summary>
    public readonly struct ContentShowLook
    {
        public readonly StreamContentType Type;
        public readonly string OverlayTitle;
        public readonly Color Wash;
        public readonly Color WashVeil;
        public readonly Color Card;
        public readonly Color CardInk;
        public readonly Color Lane;
        public readonly Color CamFrame;
        public readonly Color CamWindow;
        public readonly float BubbleScale;
        public readonly float LaneJitter;
        public readonly float BedVolume;
        public readonly bool WarmChat;
        public readonly bool LoudTroll;
        public readonly bool GoldSparkle;
        public readonly bool DimWash;

        public ContentShowLook(
            StreamContentType type,
            string overlayTitle,
            Color wash,
            Color washVeil,
            Color card,
            Color cardInk,
            Color lane,
            Color camFrame,
            Color camWindow,
            float bubbleScale,
            float laneJitter,
            float bedVolume,
            bool warmChat,
            bool loudTroll,
            bool goldSparkle,
            bool dimWash)
        {
            Type = type;
            OverlayTitle = overlayTitle;
            Wash = wash;
            WashVeil = washVeil;
            Card = card;
            CardInk = cardInk;
            Lane = lane;
            CamFrame = camFrame;
            CamWindow = camWindow;
            BubbleScale = bubbleScale;
            LaneJitter = laneJitter;
            BedVolume = bedVolume;
            WarmChat = warmChat;
            LoudTroll = loudTroll;
            GoldSparkle = goldSparkle;
            DimWash = dimWash;
        }

        public static ContentShowLook For(StreamContentType type)
        {
            switch (type)
            {
                case StreamContentType.Talk:
                    return new ContentShowLook(
                        type,
                        "오늘: 토크",
                        Palette.Hex("3A2430"),
                        new Color(0.92f, 0.55f, 0.38f, 0.18f),
                        Palette.Hex("F2B48A"),
                        Palette.Ink,
                        new Color(0.18f, 0.22f, 0.32f, 0.90f),
                        Palette.Hex("E8A07C"),
                        Palette.Hex("3A241C"),
                        1f, 0f, 0.16f,
                        true, false, false, false);
                case StreamContentType.Game:
                    return new ContentShowLook(
                        type,
                        "오늘: 게임",
                        Palette.Hex("2A1218"),
                        new Color(0.95f, 0.18f, 0.28f, 0.20f),
                        Palette.Troll,
                        Palette.Ink,
                        new Color(0.22f, 0.06f, 0.10f, 0.92f),
                        Palette.Hex("1E2A22"),
                        Palette.Hex("101610"),
                        1f, 1f, 0.14f,
                        false, true, false, false);
                case StreamContentType.Song:
                    return new ContentShowLook(
                        type,
                        "오늘: 노래",
                        Palette.Hex("2A2210"),
                        new Color(1f, 0.82f, 0.28f, 0.16f),
                        Palette.Gold,
                        Palette.Ink,
                        new Color(0.22f, 0.16f, 0.06f, 0.90f),
                        Palette.Gold,
                        Palette.Hex("2A1E0C"),
                        0.78f, 0f, 0.15f,
                        false, false, true, false);
                case StreamContentType.Reaction:
                    return new ContentShowLook(
                        type,
                        "오늘: 리액션",
                        Palette.Hex("16181A"),
                        new Color(0.04f, 0.05f, 0.06f, 0.38f),
                        Palette.Hex("6A7A72"),
                        Palette.Pastel,
                        new Color(0.10f, 0.12f, 0.12f, 0.94f),
                        Palette.Hex("4A5550"),
                        Palette.Hex("121614"),
                        1.28f, 0f, 0.11f,
                        false, false, false, true);
                default:
                    return new ContentShowLook(
                        StreamContentType.None,
                        "",
                        Palette.Studio,
                        new Color(0, 0, 0, 0),
                        Palette.Blue,
                        Palette.Ink,
                        new Color(0.07f, 0.05f, 0.1f, 0.88f),
                        Palette.PinkDeep,
                        Palette.Hex("1C1228"),
                        1f, 0f, 0.12f,
                        false, false, false, false);
            }
        }

        public static string OverlayLine(StreamContentType type) => For(type).OverlayTitle;
    }
}

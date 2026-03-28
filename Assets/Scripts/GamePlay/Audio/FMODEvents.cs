namespace GamePlay
{
    public static class FMODEvents
    {
        public static class Music
        {
            public const string Lobby    = "event:/Music/Lobby";
            public const string Gameplay = "event:/Music/Gameplay";
        }

        public static class SFX
        {
            public static class Player
            {
                public const string Shoot   = "event:/SFX/Player/Shoot";
                public const string Dash    = "event:/SFX/Player/Dash";
                public const string Graze   = "event:/SFX/Player/Graze";
                public const string Bomb    = "event:/SFX/Player/Bomb";
                public const string Damaged = "event:/SFX/Player/Damaged";
                public const string Death   = "event:/SFX/Player/Death";
            }

            public static class Bullet
            {
                public const string PlayerLoop = "event:/SFX/Bullet/PlayerLoop";
                public const string PlayerHit  = "event:/SFX/Bullet/PlayerHit";
            }

            public static class Enemy
            {
                public const string Death  = "event:/SFX/Enemy/Death";
                public const string Attack = "event:/SFX/Enemy/Attack";
            }

            public static class UI
            {
                public const string Hover        = "event:/SFX/UI/Hover";
                public const string Click        = "event:/SFX/UI/Click";
                public const string DialogueTick = "event:/SFX/UI/DialogueTick";
            }
        }
    }
}
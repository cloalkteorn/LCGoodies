using BepInEx.Configuration;

namespace LCGoodies
{
    class Configuration
    {
        public static ConfigEntry<int> walletRarity;
        public static ConfigEntry<int> sockRarity;
        public static ConfigEntry<int> buttonRarity;
        public static ConfigEntry<int> massagerRarity;
        public static ConfigEntry<int> beerRarity;
        public static ConfigEntry<int> whiskeyRarity;
        public static ConfigEntry<bool> alcoholEffects;
        public static ConfigEntry<float> beerDrunkess;
        public static ConfigEntry<float> beerDrunkessDissipationSpeed;
        public static ConfigEntry<float> whiskeyDrunkess;
        public static ConfigEntry<float> whiskeyDrunkessDissipationSpeed;
        public static ConfigEntry<float> shotgunTrapAmtMax;
        public static ConfigEntry<float> gooTrapSpeedDecreaseMultiplier;
        public static ConfigEntry<float> gooTrapJumpDecreaseMultiplier;
        public static ConfigEntry<float> gooTrapEnemySpeed;
        public static ConfigEntry<float> gooTrapAmtMax;
        public static ConfigEntry<float> wOffX;
        public static ConfigEntry<float> wOffY;
        public static ConfigEntry<float> wOffZ;


        public static void LoadConfig()
        {
            walletRarity = Plugin.config.Bind("Scrap", "WalletRarity", 15, "Chance for wallet to spawn. Higher is more likely to spawn.");
            sockRarity = Plugin.config.Bind("Scrap", "SockRarity", 15, "Chance for sock to spawn. Higher is more likely to spawn.");
            buttonRarity = Plugin.config.Bind("Scrap", "ButtonRarity", 15, "Chance for button to spawn. Higher is more likely to spawn.");
            massagerRarity = Plugin.config.Bind("Scrap", "MassagerRarity", 5, "Chance for massager to spawn. Higher is more likely to spawn.");
            beerRarity = Plugin.config.Bind("Scrap.Alcohol.Beer", "BeerRarity", 15, "Chance for beer to spawn. Higher is more likely to spawn.");
            whiskeyRarity = Plugin.config.Bind("Scrap.Alcohol.Whiskey", "WhiskeyRarity", 3, "Chance for whiskey to spawn. Higher is more likely to spawn.");
            shotgunTrapAmtMax = Plugin.config.Bind("Traps.ShotgunTrap", "TrapRarity", 4f, "Higher adds more max traps to the level");
            beerDrunkessDissipationSpeed = Plugin.config.Bind("Scrap.Alcohol.Beer", "BeerDrunknessDissipation", 0.3f, "How quickly the drunk effect dissipates when drinking beer");
            beerDrunkess = Plugin.config.Bind("Scrap.Alcohol.Beer", "BeerDrunkness", 0.2f, "How drunk you get from beer");
            whiskeyDrunkess = Plugin.config.Bind("Scrap.Alcohol.Whiskey", "WhiskeyDrunkness", 2.5f, "How drunk you get from whiskey");
            whiskeyDrunkessDissipationSpeed = Plugin.config.Bind("Scrap.Alcohol.Whiskey", "WhiskeyDrunknessDissipation", 0.1f, "How quickly the drunk effect dissipates when drinking whiskey");
            gooTrapSpeedDecreaseMultiplier = Plugin.config.Bind("Traps.GooTrap", "SpeedDecreaseMultiplier", 0.15f, "How much your speed gets decreased when in the goo trap");

            gooTrapAmtMax = Plugin.config.Bind("Traps.GooTrap", "TrapRarity", 10f, "Higher adds more max traps to the level");
            gooTrapJumpDecreaseMultiplier = Plugin.config.Bind("Traps.GooTrap", "JumpDecreaseMultiplier", 0f, "How much your jump force gets decreased when in the trap.");
            gooTrapEnemySpeed = Plugin.config.Bind("Traps.GooTrap", "EnemySpeed", 0.65f, "What speed to set enemy acceleration to when they touch the goo trap.");
            alcoholEffects = Plugin.config.Bind("Scrap.Alcohol", "Effects", true, "Alcohol effects on or off. True gets you drunk, false has no effect.");
            wOffX = Plugin.config.Bind("Scrap.Alcohol.Whiskey", "OffsetX", 0f, "X Offset of whiskey held in hand.");
            wOffY = Plugin.config.Bind("Scrap.Alcohol.Whiskey", "OffsetY", 0f, "Y Offset of whiskey held in hand.");
            wOffZ = Plugin.config.Bind("Scrap.Alcohol.Whiskey", "OffsetZ", 0f, "Z Offset of whiskey held in hand.");
        }
    }
}

using LCGoodies.MonoBehaviors;
using LethalLib.Modules;

namespace LCGoodies
{
    class CustomScrap
    {
        public static void RegisterScrap()
        {
            //register scrap items
            LoadNewItem("Assets/Wallet/WalletItem.asset", Configuration.walletRarity.Value);
            LoadNewItem("Assets/Dirty Sock/DirtySock.asset", Configuration.sockRarity.Value);
            LoadNewItem("Assets/Button/ButtonItem.asset", Configuration.buttonRarity.Value);
            string[] colors = new string[] { "Red", "Orange", "Blue", "Green", "Purple" };
            foreach (string color in colors)
            {
                LoadNewItem("Assets/Massager/MassagerItem" + color + ".asset", Configuration.massagerRarity.Value);
            }

            //load alcohol
            LoadAlcoholItem("Assets/Alcohol/BeerItem.asset", Configuration.beerRarity.Value, Configuration.beerDrunkess.Value, Configuration.beerDrunkessDissipationSpeed.Value);
            LoadAlcoholItem("Assets/Alcohol/WhiskeyItem.asset", Configuration.whiskeyRarity.Value, Configuration.whiskeyDrunkess.Value, Configuration.whiskeyDrunkessDissipationSpeed.Value);
        }

        //TODO: Figure out if there's a way to make item variants based on material variants that isn't just making individual items in the Unity editor
        //TODO: Figure out why multiple materials on the item are not resulting in items using multiple materials
        static void LoadNewItem(string assetPath, int rarity)
        {
            Item item = Plugin.ab.LoadAsset<Item>(assetPath);
            Items.RegisterScrap(item, rarity, Levels.LevelTypes.All);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
            Plugin.ls.LogInfo("Registered new item '" + item.name + "'");
        }

        static void LoadAlcoholItem(string assetPath, int rarity, float drunkness, float drunkRemovalSpeed)
        {
            Item item = Plugin.ab.LoadAsset<Item>(assetPath);
            Alcohol alc = item.spawnPrefab.GetComponent<Alcohol>();
            alc.drunkenness = drunkness;
            alc.drunkRemovalSpeed = drunkRemovalSpeed;

            if (assetPath == "Assets/Alcohol/WhiskeyItem.asset")
            {
                item.positionOffset.x = Configuration.wOffX.Value;
                item.positionOffset.y = Configuration.wOffY.Value;
                item.positionOffset.z = Configuration.wOffZ.Value;
            }

            Items.RegisterScrap(item, rarity, Levels.LevelTypes.All);
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(item.spawnPrefab);
            Plugin.ls.LogInfo("Registered alcohol item " + assetPath);
        }
    }
}

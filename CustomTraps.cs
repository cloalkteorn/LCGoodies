using LethalLib.Extras;
using LethalLib.Modules;
using System; 
using UnityEngine;

namespace LCGoodies
{
    class CustomTraps
    {
        public static void RegisterTraps()
        {
            //register traps
            LoadNewMapObject("Assets/ShotgunTrap/ShotgunTrap.asset", Configuration.shotgunTrapAmtMax.Value);
            LoadNewMapObject("Assets/GooTrap/GooTrap.asset", Configuration.gooTrapAmtMax.Value);
        }

        static void LoadNewMapObject(string assetPath, float trapAmountMax)
        {
            //load map object definition from asset bundle
            SpawnableMapObjectDef trapDefinition = Plugin.ab.LoadAsset<SpawnableMapObjectDef>(assetPath);

            Keyframe start = new Keyframe(0f, 0f);
            Keyframe end = new Keyframe(1f, trapAmountMax);
            AnimationCurve rarity = new AnimationCurve(new Keyframe[2] { start, end });
            trapDefinition.spawnableMapObject.numberToSpawn = rarity;

            //register the network prefab and register the mapobject
            LethalLib.Modules.NetworkPrefabs.RegisterNetworkPrefab(trapDefinition.spawnableMapObject.prefabToSpawn);

            Func<SelectableLevel, AnimationCurve> rates = (level) => rarity;

            MapObjects.RegisterMapObject(trapDefinition, Levels.LevelTypes.All, rates);
            Plugin.ls.LogInfo("Registered trap " + assetPath);
        }
    }
}

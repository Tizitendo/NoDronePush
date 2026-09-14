using BepInEx;
using Logger;
using RoR2;

[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace NoDronePush;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public sealed class NoDronePush : BaseUnityPlugin
{
    public const string PluginGUID = "com." + PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "Onyx";
    public const string PluginName = "NoDronePush";
    public const string PluginVersion = "1.0.0";

	public void Awake()
    {
		Log.Init(Logger);
	}

	[SystemInitializer(typeof(DroneCatalog))]
	static void Init()
	{
		foreach(DroneDef drone in DroneCatalog.allDroneDefs)
		{
			if (!drone.bodyPrefab)
				continue;
			if (drone.bodyPrefab.TryGetComponent(out CharacterBody body))
			{
				body.doNotReassignToTeamBasedCollisionLayer = true;
				drone.bodyPrefab.layer = LayerIndex.playerFakeActor.intVal;
			}
		}
	}
}

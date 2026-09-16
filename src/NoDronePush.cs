using System;
using BepInEx;
using EntityStates.Merc;
using Logger;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;

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

		IL.EntityStates.Merc.EvisDash.FixedUpdate += EvisDash_FixedUpdate;
	}

    private static void EvisDash_FixedUpdate(ILContext il)
    {
        ILCursor c = new(il);
		ILLabel label = null;
		int hurtboxLoc = 0;

		if (!c.TryGotoNext(
			x => x.MatchCallOrCallvirt(typeof(Component), nameof(Component.GetComponent)),
			x => x.MatchStloc(out hurtboxLoc)
		))
		{
			Log.Error(il.Method.Name + " IL Hook failed!");
		}

		c.Index = 0;

		if (c.TryGotoNext(
			x => x.MatchNewobj(typeof(EntityStates.Merc.Evis))
		) &&
		c.TryGotoPrev(MoveType.After,
			x => x.MatchBrfalse(out label)
		))
		{
			c.Emit(OpCodes.Ldarg_0);
			c.Emit(OpCodes.Ldloc, hurtboxLoc);
			c.EmitDelegate<Func<EvisDash, HurtBox, bool>>(ignoreTeamMates);
			c.Emit(OpCodes.Brfalse, label);
		} else
		{
			Log.Error(il.Method.Name + " IL Hook failed!");
		}

		static bool ignoreTeamMates(EvisDash self, HurtBox hurtBox)
		{
			if (TeamMask.GetUnprotectedTeams(self.GetTeam()).HasTeam(hurtBox.teamIndex))
			{
				return true;
			}
			return false;
		}
    }
}

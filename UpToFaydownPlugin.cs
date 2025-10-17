using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;

namespace Silksong.UpToFaydown;

[BepInAutoPlugin(id: "runhello.silksong.uptofaydown", name: "Up to Faydown")]
[HarmonyPatch]
public partial class UpToFaydown : BaseUnityPlugin {
    private static ManualLogSource logger;
    private static ConfigEntry<bool> enabled;
    private Harmony harmony;

    private void Awake() {
        logger = Logger;
        enabled = Config.Bind("General",
            "Faydown Require Up",
            true,
            "Enable up-to-doublejump requirement");
        harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void OnDestroy() {
        harmony.UnpatchSelf();
    }

    [HarmonyPatch(typeof(HeroController), nameof(HeroController.CanDoubleJump))]
    [HarmonyILManipulator]
    private static void IlCanDoubleJump(ILContext ctx) {
        var ilCursor = new ILCursor(ctx);
        // if (playerData.hasDoubleJump && !doubleJumped && !IsDashLocked() && !cState.wallSliding && !cState.backDashing && !IsAttackLocked() && !cState.bouncing && !cState.shroomBouncing && !cState.onGround && !cState.doubleJumping && Config.CanDoubleJump)
        // ↓
        // if (playerData.hasDoubleJump && AllowDoubleJump() && !doubleJumped...
        if (!ilCursor.TryGotoNext(MoveType.After,
                instruction => instruction.OpCode == OpCodes.Ldfld &&
                               instruction.Operand.ToString().EndsWith(nameof(PlayerData.hasDoubleJump)))) {
            logger.LogWarning("Failed to find instruction in HeroController.CanDoubleJump()");
            return;
        }

        ilCursor.Emit(OpCodes.Ldarg_0).EmitDelegate<Func<bool, HeroController, bool>>(AllowDoubleJump);
    }

    private static bool AllowDoubleJump(bool hasDoubleJump, HeroController heroController) {
        if (!enabled.Value) {
            return hasDoubleJump;
        }

        var inputActions = heroController.inputHandler.inputActions;
        if (!inputActions.Up.IsPressed) {
            return false;
        }

        return hasDoubleJump;
    }
}

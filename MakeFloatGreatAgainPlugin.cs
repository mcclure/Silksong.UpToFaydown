using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;

namespace Silksong.MakeFloatGreatAgain;

[BepInAutoPlugin(id: "io.github.silksong.makefloatgreatagain", name: "MakeFloatGreatAgain")]
[HarmonyPatch]
public partial class MakeFloatGreatAgainPlugin : BaseUnityPlugin {
    private static ManualLogSource logger;
    private Harmony harmony;

    private void Awake() {
        logger = Logger;
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
                               instruction.Operand.ToString().EndsWith("hasDoubleJump"))) {
            logger.LogWarning("Failed to find instruction in HeroController.CanDoubleJump()");
            return;
        }

        ilCursor.Emit(OpCodes.Ldarg_0).EmitDelegate<Func<bool, HeroController, bool>>(AllowDoubleJump);
    }

    private static bool AllowDoubleJump(bool hasDoubleJump, HeroController heroController) {
        if (Constants.GAME_VERSION == "1.0.28324") {
            return hasDoubleJump;
        }

        var inputActions = heroController.inputHandler.inputActions;
        if (inputActions.Down.IsPressed && !inputActions.Right.IsPressed && !inputActions.Left.IsPressed) {
            return false;
        }

        return hasDoubleJump;
    }
}

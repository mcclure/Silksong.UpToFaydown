using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;

namespace Silksong.MakeFloatGreatAgain;

[BepInAutoPlugin(id: "demojameson.silksong.makefloatgreatagain", name: "Make Float Great Again")]
[HarmonyPatch]
public partial class MakeFloatGreatAgainPlugin : BaseUnityPlugin {
    private static ManualLogSource logger;
    private static ConfigEntry<bool> enabled;
    private static ConfigEntry<bool> allowDownwardDiagonal;
    private Harmony harmony;

    private void Awake() {
        logger = Logger;
        enabled = Config.Bind("General",
            "Float Override Input",
            true,
            "Whether to enable float override input");
        allowDownwardDiagonal = Config.Bind("General",
            "Allow Downward Diagonal",
            false,
            "Whether to allow diagonal down + jump to trigger floating");
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
        if (inputActions.Down.IsPressed) {
            if (allowDownwardDiagonal.Value) {
                return false;
            }

            if (!inputActions.Right.IsPressed && !inputActions.Left.IsPressed) {
                return false;
            }
        }

        return hasDoubleJump;
    }
}

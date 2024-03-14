using HarmonyLib;
using Nekoyume.Game;
using Nekoyume.UI;
using UnityEngine;

namespace NineChronicles.Mods.TestMod.Patches
{
    [HarmonyPatch(typeof(ActionCamera))]
    internal class NotifyKeyboardInput
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void PostfixUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NotificationSystem.Push(
                    Nekoyume.Model.Mail.MailType.System,
                    "Space key is pressed!",
                    Nekoyume.UI.Scroller.NotificationCell.NotificationType.Notification);
            }
        }
    }
}

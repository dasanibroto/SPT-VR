using UnityEngine;
using HarmonyLib;
using TarkovVR.Source.Player.VRManager;
using EFT;
using FIKAVRSync; 

namespace TarkovVR.ModSupport.FIKAVRSyncPatch
{
    [HarmonyPatch]
    internal static class FIKAVRSyncSupport
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RaidVRPlayerManager), "Update")]
        private static void SendToFikaVRSync(RaidVRPlayerManager __instance)
        {
            if (FIKAVRSync.Plugin.Instance == null || VRGlobals.player == null) return;

            Vector3 rootPos = VRGlobals.player.Transform.position;
            
            // Head Rotation
            Quaternion headWorldRot = VRGlobals.VRCam.transform.rotation;

            Transform weaponTrans = null;

            // try get melee
            if (VRGlobals.player.HandsController is EFT.Player.KnifeController knifeController)
            {
                weaponTrans = knifeController.WeaponRoot;
            }
            // else try get gun
            else if (VRGlobals.firearmController != null && VRGlobals.firearmController.WeaponRoot != null)
            {
                var wRoot = VRGlobals.firearmController.WeaponRoot;
                var wAnim = wRoot.Find("Weapon_root_anim");
                weaponTrans = wAnim ? wAnim.Find("weapon") : wRoot.Find("weapon");
                if (weaponTrans == null) weaponTrans = wRoot;
            }
            // else try get gun from holder object
            else if (VRGlobals.weaponHolder != null && VRGlobals.weaponHolder.transform.childCount > 0)
            {
                weaponTrans = VRGlobals.weaponHolder.transform.GetChild(0);
            }
            // else just send holder
            else
            {
                weaponTrans = VRGlobals.weaponHolder?.transform;
            }

            Vector3 weaponOffset = Vector3.zero;
            Quaternion weaponWorldRot = Quaternion.identity;

            if (weaponTrans != null)
            {
                // Calculate WORLD offset from the player's feet/root.
                weaponOffset = weaponTrans.position - rootPos;
                weaponWorldRot = weaponTrans.rotation;
            }

            Vector3 lHandOffset = Vector3.zero;
            Quaternion lHandWorldRot = Quaternion.identity;

            if (VRGlobals.vrPlayer != null && VRGlobals.vrPlayer.LeftHand != null)
            {
                lHandOffset = VRGlobals.vrPlayer.LeftHand.transform.position - rootPos;
                lHandWorldRot = VRGlobals.vrPlayer.LeftHand.transform.rotation;
            }

            bool isTwoHanding = VRGlobals.vrPlayer != null && VRGlobals.vrPlayer.isSupporting;

            // Send Data
            FIKAVRSync.Plugin.Instance.UpdateLocalData(
                headWorldRot, 
                weaponOffset, weaponWorldRot,
                lHandOffset, lHandWorldRot,
                VRGlobals.player.ProfileId,
                isTwoHanding
            );
        }
    }
}
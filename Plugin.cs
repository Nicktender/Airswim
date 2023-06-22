using System;
using BepInEx;
using UnityEngine;
using UnityEngine.XR;
using Utilla;

namespace Airswim
{
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool inRoom;
        bool effectsEnabled;
        bool gripPressed;
        GameObject beachObject;
        GameObject waterVolumesObject;
        GameObject alwaysLoadedWaterObject;
        Vector3 originalWaterVolumeSize;

        void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void OnDisable()
        {
            HarmonyPatches.RemoveHarmonyPatches();
            RevertChanges();
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            // Find the necessary game objects
            beachObject = GameObject.Find("Level/beach");
            waterVolumesObject = beachObject?.transform.Find("B_WaterVolumes")?.gameObject;

            // Cache the original water volume size
            if (waterVolumesObject != null)
            {
                originalWaterVolumeSize = waterVolumesObject.transform.localScale;
            }
        }

        void Update()
        {
            if (!inRoom)
            {
                DisableEffects();
                return;
            }

            if (beachObject == null)
            {
                beachObject = GameObject.Find("Level/beach");
            }

            if (beachObject != null)
            {
                beachObject.SetActive(true);
            }

            if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.gripButton, out bool rightGripButton))
            {
                if (rightGripButton && !gripPressed)
                {
                    gripPressed = true;

                    if (effectsEnabled)
                    {
                        RevertChanges();
                        DisableEffects();
                    }
                    else
                    {
                        ApplyChanges();
                        EnableEffects();
                    }
                }
                else if (!rightGripButton && gripPressed)
                {
                    gripPressed = false;
                }
            }

            if (waterVolumesObject != null)
            {
                waterVolumesObject.SetActive(true);
            }
        }

        void EnableEffects()
        {
            effectsEnabled = true;
        }

        void DisableEffects()
        {
            effectsEnabled = false;
            RevertChanges();
        }

        void ApplyChanges()
        {
            // Create AlwaysLoadedWater game object if it doesn't exist
            if (alwaysLoadedWaterObject == null)
            {
                alwaysLoadedWaterObject = new GameObject("AlwaysLoadedWater");
            }

            // Make water volumes a child of AlwaysLoadedWater
            if (waterVolumesObject != null)
            {
                waterVolumesObject.transform.SetParent(alwaysLoadedWaterObject.transform, false);
            }

            // Modify water volume size
            if (waterVolumesObject != null)
            {
                waterVolumesObject.transform.localScale = new Vector3(10000f, 10000f, 10000f);
            }
        }

        void RevertChanges()
        {
            // Revert water volume size to original
            if (waterVolumesObject != null)
            {
                waterVolumesObject.transform.localScale = originalWaterVolumeSize;
            }

            // Remove AlwaysLoadedWater game object and restore water volumes parent
            if (alwaysLoadedWaterObject != null)
            {
                if (waterVolumesObject != null)
                {
                    waterVolumesObject.transform.SetParent(beachObject.transform, false);
                }
                Destroy(alwaysLoadedWaterObject);
                alwaysLoadedWaterObject = null;
            }
        }

        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            inRoom = true;
        }

        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            inRoom = false;
            DisableEffects();
        }
    }
}

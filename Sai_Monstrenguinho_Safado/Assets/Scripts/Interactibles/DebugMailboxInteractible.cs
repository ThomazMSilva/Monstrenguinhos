﻿using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Interactibles
{
    public class DebugMailboxInteractible : Interactible
    {
        [SerializeField] private TextMeshProUGUI debugTMP;

        private Dictionary<string, List<CropAttributes>> totalCropsDict = new();

        public override void Interact(object sender = null)
        {
            if (sender != null && sender is PlayerScripts.PlayerController player)
            {
                switch (player.HeldItem.itemTag)
                {
                    case PlayerScripts.ItemTag.Box:
                        if(player.TryGetFromHeld<BoxHoldable>(out var box))
                        {
                            UpdateTMP(box.storedCrops);
                            box.ClearCrops();
                            box.ReturnToStartingPoint();
                            player.SetHeldEmpty();
                        }
                        break;
                    case PlayerScripts.ItemTag.Crop:
                        if(player.TryGetFromHeld<CropHoldable>(out var singleCrop))
                        {
                            List<CropAttributes> singleCropList = new() { singleCrop.cropAttributes };
                            UpdateTMP(singleCropList);
                            player.SetHeldEmpty();
                            singleCrop.ReturnToStartingPoint();
                        }
                        break;

                    default: break;
                }
            }
        }
        
        private string CropAmounts(List<CropAttributes> cropList)
        {
            foreach (var crop in cropList)
            {
                string name = crop.CropName.ToString();
                if (totalCropsDict.ContainsKey(name))
                    totalCropsDict[name].Add(crop);

                else totalCropsDict.Add(name, new() { crop });
            }
            string debugString = "";
            foreach (var key in totalCropsDict.Keys)
            {
                debugString += $"{key}: {totalCropsDict[key].Count}\n";
            }
            return debugString;
        }

        public void UpdateTMP(List<CropAttributes> cropList) => debugTMP.text = CropAmounts(cropList);
        
    }
}
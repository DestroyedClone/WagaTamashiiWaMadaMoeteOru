﻿using System;
using System.Collections.Generic;
using BepInEx;
using RoR2;
using UnityEngine;

namespace SillyGlasses
{
    public class CharacterSwooceManager : MonoBehaviour {
        
        public bool Engi;

        private Dictionary<ItemIndex, int> _instantiatedGlasAmounts = new Dictionary<ItemIndex, int>();
        private Dictionary<ItemIndex, Transform> _instantiatedGlasParents = new Dictionary<ItemIndex, Transform>();
        private Dictionary<string, Transform> _extraGlasParents = new Dictionary<string, Transform>();

        //private Dictionary<string, Transform> _instantiatedGlasParentsExtra = new Dictionary<string, Transform>();
        //check the parents of the parents:
        //displayruleparent != _instantiatedGlasParents[item].parent
        //displayruleparent != _instantiatedGlasParentsExtra[string.format(item+number)].parent
        //how do i find number?

        private CharacterModel _swoocedModel;
        private Inventory _swoocedCurrentInventory;
        private ChildLocator _swooceChildLocator;

        public void Awake() {

            if (_swoocedModel == null)
            {
                _swoocedModel = GetComponent<CharacterModel>();
            }
        }

        public void HookedUpdateItemDisplay(CharacterModel self, Inventory inventory)
        {
            if (_swoocedModel == self && _swoocedCurrentInventory == null)
            {
                _swoocedCurrentInventory = inventory;
            }
        }

        public void HookedEnableDisableDisplay(CharacterModel self, ItemIndex itemIndex)
        {
            if (self.itemDisplayRuleSet)
            {
                DisplayRuleGroup displayRuleGroup = self.itemDisplayRuleSet.GetItemDisplayRuleGroup(itemIndex);

                PseudoInstantiateDisplayRuleGroup(self, displayRuleGroup, itemIndex);
            }
        }

        public void PseudoInstantiateDisplayRuleGroup(CharacterModel self,
                                                      DisplayRuleGroup displayRuleGroup_,
                                                      ItemIndex itemIndex_)
        {
            if (self != _swoocedModel)
                return;

            if (displayRuleGroup_.rules == null)
                return;

            if (itemIndex_ == ItemIndex.None)
                return;
            //if (itemIndex_ == ItemIndex.BoostHp)
            //    return;
            //if (itemIndex_ == ItemIndex.BoostDamage)
            //    return;

            if (_swoocedCurrentInventory == null)
            {
                return;
            }

            int currentCount = _swoocedCurrentInventory.GetItemCount(itemIndex_);
            if (!_instantiatedGlasAmounts.ContainsKey(itemIndex_))
            {
                _instantiatedGlasAmounts.Add(itemIndex_, 1);
            }
            
            int displayOriginalPrefabsCount = _instantiatedGlasAmounts[itemIndex_];

            int difference = currentCount - displayOriginalPrefabsCount;
            //Utils.Log($"{itemIndex_} diff: {difference} = current: {currentCount} - orig: {displayOriginalPrefabsCount}");

            if (difference < 0)
            {
                //if (_instantiatedGlasParents.ContainsKey(itemIndex_) && _instantiatedGlasParents[itemIndex_] != null)
                //{
                //    Destroy(_instantiatedGlasParents[itemIndex_].gameObject);
                //    _instantiatedGlasAmounts[itemIndex_] = 1;
                //    difference = currentCount - 1;
                //}
                return;
            }

            if (_swooceChildLocator == null)
            {
                _swooceChildLocator = self.GetComponent<ChildLocator>();
                
            }

            GameObject IterInstantiatedItem = null;

            for (int j = 0; j < difference; j++)
            {
                if (SillyGlasse.ItemStackMax.Value != -1 && _instantiatedGlasAmounts[itemIndex_] + 1 >= SillyGlasse.ItemStackMax.Value)
                    return;
            
                int currentCountIterated = displayOriginalPrefabsCount + j;

                //Utils.Log($"swoocing new prefab {itemIndex_}: {j} ");
                _instantiatedGlasAmounts[itemIndex_]++;

                for (int i = 0; i < displayRuleGroup_.rules.Length; i++)
                {
                    ItemDisplayRule swoocedDisplayRule = displayRuleGroup_.rules[i];

                    if (swoocedDisplayRule.ruleType != ItemDisplayRuleType.ParentedPrefab)
                        continue;
                
                    Transform displayParent = _swooceChildLocator.FindChild(swoocedDisplayRule.childName);

                    IterInstantiatedItem = InstantiateExtraItem(self, swoocedDisplayRule, _swooceChildLocator, displayParent, currentCountIterated);
                    IterInstantiatedItem.name += currentCountIterated.ToString();
                    
                    if (!_instantiatedGlasParents.ContainsKey(itemIndex_) || _instantiatedGlasParents[itemIndex_] == null)
                    {
                        Transform parentTransform = new GameObject(IterInstantiatedItem.gameObject.name + "Parent").transform;
                        parentTransform.parent = IterInstantiatedItem.transform.parent;
                        parentTransform.localPosition = Vector3.one;
                        parentTransform.localRotation = Quaternion.identity;
                        parentTransform.localScale = Vector3.one;

                        if (!_instantiatedGlasParents.ContainsKey(itemIndex_))
                        {
                            _instantiatedGlasParents.Add(itemIndex_, parentTransform);
                        }
                        if (_instantiatedGlasParents[itemIndex_] == null)
                        {
                            _instantiatedGlasParents[itemIndex_] = parentTransform;
                        }
                    }
                    else
                    {
                        IterInstantiatedItem.transform.parent = _instantiatedGlasParents[itemIndex_];
                    }
                }
            }
        }

        //copied from ParentedPrefabDisplay.Apply
        private GameObject InstantiateExtraItem(CharacterModel characterModel, ItemDisplayRule displayRule, ChildLocator childLocator, Transform parent, int moveMult)
        {
            GameObject prefab = displayRule.followerPrefab;

            Vector3 displayRuleLocalPosition = displayRule.localPos;
            Quaternion displayRuleLocalRotation = Quaternion.Euler(displayRule.localAngles);
            Vector3 displayRuleLocalScale = displayRule.localScale;

            GameObject instantiatedDisplay = Instantiate<GameObject>(prefab.gameObject, parent);

            instantiatedDisplay.transform.localPosition = displayRuleLocalPosition;
            instantiatedDisplay.transform.localRotation = displayRuleLocalRotation;
            instantiatedDisplay.transform.localScale = displayRuleLocalScale;
            instantiatedDisplay.transform.position += instantiatedDisplay.transform.forward * moveMult * SillyGlasse.ItemDistanceMultiplier.Value * (Engi ? 1.5f : 1);

            LimbMatcher component = instantiatedDisplay.GetComponent<LimbMatcher>();
            if (component && childLocator)
            {
                component.SetChildLocator(childLocator);
            }
            //this.itemDisplay = parentedDisplay.GetComponent<ItemDisplay>();

            return instantiatedDisplay;
        }

        private static void ShowFunnyCube(Transform parent, Vector3 DisplayRuleLocalPos, Quaternion DisplayRuleLocalRotation)
        {
            Type[] cubeComponents = new Type[] { typeof(MeshRenderer), typeof(MeshFilter) };

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            GameObject bruh = new GameObject("bruh", cubeComponents);
            bruh.GetComponent<MeshFilter>().mesh = cube.GetComponent<MeshFilter>().mesh;
            bruh.GetComponent<MeshRenderer>().material = new Material(cube.GetComponent<MeshRenderer>().material);
            bruh.transform.parent = parent;
            bruh.transform.localScale = new Vector3(0.169f, 0.01f, 0.1f);
            bruh.transform.localPosition = DisplayRuleLocalPos;
            bruh.transform.localRotation = DisplayRuleLocalRotation;

            Destroy(cube);
        }
    }
}
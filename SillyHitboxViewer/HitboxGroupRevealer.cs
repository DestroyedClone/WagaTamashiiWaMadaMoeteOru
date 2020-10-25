﻿using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace SillyHitboxViewer {
    public class HitboxGroupRevealer: MonoBehaviour {

        public HitBoxGroup hitboxGroup { get; set; }
        private List<HitboxGroupRevealer> _hitboxesList;
        private bool _isMerc;

        private List<HitboxRevealer> _revealers = new List<HitboxRevealer>();

        private int revealBufferCount;

        public void init(HitBoxGroup hitboxGroup_, List<HitboxGroupRevealer> hitboxesList) {
            hitboxGroup = hitboxGroup_;
            _hitboxesList = hitboxesList;

            var bod = GetComponent<CharacterBody>();
            Debug.Log($"bod: {bod}");
            if (bod) {
                _isMerc = GetComponent<CharacterBody>().bodyIndex == BodyCatalog.FindBodyIndex("Mercenary");
            }
            addVisaulizinators();
        }

        private void addVisaulizinators() {

            for (int i = 0; i < hitboxGroup.hitBoxes.Length; i++) {

                HitboxRevealer rev = Instantiate(_hitboxPrefab, hitboxGroup.hitBoxes[i].transform, false).init(_isMerc);

                //HitboxViewerMod.addPooledRevealer(hitboxGroup.hitBoxes[i].transform);

                _revealers.Add (rev);
            }
        }

        public void reveal(bool active) {
            revealBufferCount = 3;
            for (int i = 0; i < _revealers.Count; i++) {
                _revealers[i].show(active);
            }
        }

        void FixedUpdate() {
            if (revealBufferCount == 0)
                reveal(false);
            revealBufferCount -= 1;
        }

        void OnDestroy() {
            reveal(false);
            revealBufferCount = -1;

            _hitboxesList.Remove(this);
            _revealers.Clear();
        }
    }
}

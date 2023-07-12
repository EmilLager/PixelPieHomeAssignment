using System;
using DG.Tweening;
using UnityEngine;

namespace Collapse.Blocks {
    /**
     * Bomb specific behavior
     */
    public class Bomb : Block {
        [SerializeField]
        private Transform Sprite;

        [SerializeField]
        private Vector3 ShakeStrength;

        [SerializeField]
        private int ShakeVibrato;

        [SerializeField]
        private float ShakeDuration;

        private Vector3 origin;

        private void Awake() {
            origin = Sprite.localPosition;
        }

        protected override void OnMouseUp() {
            Trigger(0.5f); //Note: Magic numbers shouldn't be here
        }
        
        /**
         * Convenience for shake animation with callback in the end
         */
        private void Shake(Action onComplete = null) {
            Sprite.DOKill();
            Sprite.localPosition = origin;
            Sprite.DOShakePosition(ShakeDuration, ShakeStrength, ShakeVibrato, fadeOut: false).onComplete += () => {
                onComplete?.Invoke();
            };
        }

        public override void Trigger(float delay)
        {
            if (IsTriggered) return;
            IsTriggered = true;
            
            // Clear from board
            BoardManager.Instance.ClearBlockFromGrid(this);
            
            Shake(() =>
            {
                BoardManager.Instance.TriggerBomb(this);
                
                transform.DOScale(Vector3.zero, delay)
                    .onComplete += () =>
                {
                    // Kill game object
                    transform.DOKill();
                    Destroy(gameObject);
                };
            });
        }
    }
}
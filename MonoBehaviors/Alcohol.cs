using System.Collections;
using System;
using Unity.Netcode;
using UnityEngine;
using GameNetcodeStuff;

namespace LCGoodies.MonoBehaviors
{
    class Alcohol : GrabbableObject
    {
        public float drunkenness;
        public float drunkRemovalSpeed;

        public AudioSource audioSource;
        public AudioClip[] drinkNoises;

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);

            PlayDrinkNoiseServerRpc();

            if (IsOwner)
            {
                playerHeldBy.activatingItem = buttonDown;
                playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", buttonDown);
            }

            StartCoroutine(FinishDrinkAnimation());
        }

        [ServerRpc(RequireOwnership = false)]
        void PlayDrinkNoiseServerRpc()
        {
            int random = UnityEngine.Random.Range(0, drinkNoises.Length);
            PlayDrinkNoiseClientRpc(random);
        }

        [ClientRpc]
        void PlayDrinkNoiseClientRpc(int random)
        {
            audioSource.PlayOneShot(drinkNoises[random]);

            RoundManager.Instance.PlayAudibleNoise(base.transform.position, 10, 1f);
            WalkieTalkie.TransmitOneShotAudio(audioSource, drinkNoises[random], 1f);
        }

        private IEnumerator FinishDrinkAnimation()
        {
            yield return new WaitForSeconds(1f);
            if (Configuration.alcoholEffects.Value)
            {
                playerHeldBy.drunkness += drunkenness;
                playerHeldBy.drunknessSpeed = drunkRemovalSpeed;
            }
            playerHeldBy.activatingItem = false;
            playerHeldBy.playerBodyAnimator.SetBool("useTZPItem", false);
            playerHeldBy.DespawnHeldObject();
        }
    }
}

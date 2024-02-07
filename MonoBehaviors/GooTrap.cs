using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace LCGoodies.MonoBehaviors
{
    class GooTrap : NetworkBehaviour
    {
        public float playerSpeedDecreaseMultiplier;
        public float playerJumpDecreaseMultiplier;
        public float enemySlowedSpeed;
        public AudioSource stickySoundsAudio;
        public AudioClip[] stuckSounds;
        public AudioClip[] unstuckSounds;

        private float playerDefaultMovementSpeed;
        private float playerDefaultJumpForce;

        void Start()
        {
            Plugin.ls.LogInfo("GooTrap starting!");
            //surely no one will modify these beforehand, right? :^)
            playerDefaultMovementSpeed = GameNetworkManager.Instance.localPlayerController.movementSpeed;
            playerDefaultJumpForce = GameNetworkManager.Instance.localPlayerController.jumpForce;
            playerSpeedDecreaseMultiplier = Configuration.gooTrapSpeedDecreaseMultiplier.Value;
            playerJumpDecreaseMultiplier = Configuration.gooTrapJumpDecreaseMultiplier.Value;
            enemySlowedSpeed = Configuration.gooTrapEnemySpeed.Value;
        }

        [ServerRpc(RequireOwnership = false)]
        void PlayStickySoundServerRpc(bool stuck)
        {
            PlayStickySoundClientRpc(stuck);
        }

        [ClientRpc]
        void PlayStickySoundClientRpc(bool stuck)
        {
            PlayStickySound(stuck);
        }

        void PlayStickySound(bool stuck)
        {
            AudioClip[] sounds = stuck ? stuckSounds : unstuckSounds;
            int random = UnityEngine.Random.Range(0, sounds.Length);
            stickySoundsAudio.PlayOneShot(sounds[random]);

            RoundManager.Instance.PlayAudibleNoise(base.transform.position, 10, 1f);
            WalkieTalkie.TransmitOneShotAudio(stickySoundsAudio, sounds[random], 1f);
        }

        void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                PlayerControllerB collidingPlayer = collider.gameObject.GetComponent<PlayerControllerB>();

                if (collidingPlayer == GameNetworkManager.Instance.localPlayerController)
                {
                    LimitPlayerSpeed lps = collidingPlayer.gameObject.AddComponent<LimitPlayerSpeed>();
                    lps.slowJump = collidingPlayer.jumpForce * playerJumpDecreaseMultiplier;
                    lps.slowSpeed = collidingPlayer.movementSpeed * playerSpeedDecreaseMultiplier;
                    lps.defaultJump = playerDefaultJumpForce;
                    lps.defaultSpeed = playerDefaultMovementSpeed;
                    lps.player = collidingPlayer;
                    PlayStickySoundServerRpc(true);
                }
            }
            else if (collider.CompareTag("Enemy"))
            {
                EnemyAICollisionDetect enemyAICollision = collider.gameObject.GetComponent<EnemyAICollisionDetect>();
                if (enemyAICollision != null)
                {
                    EnemyAI enemy = enemyAICollision.mainScript;

                    //only slow the enemy if it's not the ghost girl or the blob
                    if (!(enemy is DressGirlAI) && !(enemy is BlobAI))
                    {
                        //set enemy velocity one-time to instantly slow enemies that are already moving fast
                        enemy.agent.velocity *= enemySlowedSpeed;

                        //add LimitEnemySpeed to enemy to update their speed every frame so it doesn't get overwritten
                        LimitEnemySpeed les = enemy.gameObject.AddComponent<LimitEnemySpeed>();
                        les.speedLimit = enemySlowedSpeed;
                        les.enemyToSlow = enemy;
                        PlayStickySoundServerRpc(true);
                    }
                }
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                PlayerControllerB collidingPlayer = collider.gameObject.GetComponent<PlayerControllerB>();
                if (collidingPlayer == GameNetworkManager.Instance.localPlayerController)
                {
                    LimitPlayerSpeed lps = collidingPlayer.gameObject.GetComponent<LimitPlayerSpeed>();
                    if (lps != null)
                    {
                        Destroy(lps);
                        PlayStickySoundServerRpc(false);
                    }
                }
            }
            else if (collider.CompareTag("Enemy"))
            {
                EnemyAICollisionDetect enemyAICollision = collider.gameObject.GetComponent<EnemyAICollisionDetect>();
                if (enemyAICollision != null)
                {
                    EnemyAI enemy = enemyAICollision.mainScript;

                    LimitEnemySpeed les = enemy.gameObject.GetComponent<LimitEnemySpeed>();
                    if (les != null)
                    {
                        Destroy(les);
                        PlayStickySoundServerRpc(false);
                    }
                }
            }
        }

        private class LimitEnemySpeed : MonoBehaviour
        {
            public EnemyAI enemyToSlow;
            public float speedLimit;

            void Update()
            {
                if (enemyToSlow != null)
                {
                    enemyToSlow.agent.speed = speedLimit;
                }
            }
        }

        private class LimitPlayerSpeed : MonoBehaviour
        {
            public PlayerControllerB player;
            public float slowSpeed;
            public float slowJump;

            public float defaultSpeed;
            public float defaultJump;

            void Update()
            {
                if (player != null)
                {
                    player.movementSpeed = slowSpeed;
                    player.jumpForce = slowJump;
                }
            }

            void OnDestroy()
            {
                if (player != null)
                {
                    player.movementSpeed = defaultSpeed;
                    player.jumpForce = defaultJump;
                }
            }
        }
    }
}

using BepInEx.Logging;
using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace LCGoodies.MonoBehaviors
{
    public class ShotgunTrap : NetworkBehaviour
    {
        private NetworkVariable<bool> hasTriggered = new NetworkVariable<bool>(false);
        private NetworkVariable<bool> shotgunHasFired = new NetworkVariable<bool>(false);
        private ManualLogSource mls;
        private NetworkVariable<bool> touchingDoor = new NetworkVariable<bool>(false);
        private GameObject objectTouchingTrigger;
        //part of the object
        public Transform targetPosition;
        public Transform aimLocation;
        private LineRenderer lr;
        private LineRenderer[] renderers;
        public ShotgunItem shotgunObject;

        LayerMask lm;

        void Awake()
        {
            if (mls == null) mls = Plugin.GetLogSource();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            this.shotgunObject = null;

            if (IsServer)
            {
                hasTriggered.Value = false;
                shotgunHasFired.Value = false;
            }
            else
            {
                hasTriggered.OnValueChanged += TrapTriggered;
                shotgunHasFired.OnValueChanged += ShotgunFired;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            hasTriggered.OnValueChanged -= TrapTriggered;
            shotgunHasFired.OnValueChanged -= ShotgunFired;
        }

        void TrapTriggered(bool prev, bool current)
        {
            hasTriggered.Value = current;
            if (current)
            {
                DisableLineRenderers();
            }
        }

        void DisableLineRenderers()
        {
            foreach (LineRenderer r in renderers)
            {
                r.enabled = false;
            }
        }

        void ShotgunFired(bool prev, bool current)
        {
            shotgunHasFired.Value = current;
        }

        void Start()
        {
            mls.LogInfo("ShotgunTrap Starting!");

            lm = LayerMask.GetMask(new string[] { "Player", "Props", "PhysicsObject", "Enemies", "PlayerRagdoll", "EnemiesNotRendered" });
            lr = this.gameObject.GetComponent<LineRenderer>();
            renderers = this.gameObject.GetComponentsInChildren<LineRenderer>();
            Transform wireStart = this.gameObject.transform.Find("Tripwire Locations").Find("Wire Start").transform;
            lr.SetPosition(0, wireStart.position);
            //set end position of the tripwire using a raycast
            int layerToUse = StartOfRound.Instance.collidersAndRoomMaskAndDefault;
            int layerToAdd = LayerMask.NameToLayer("InteractableObject");
            //add InteractableObject to the raycast layermask so we can stop on doors

            //make this a coroutine because sometimes the doors aren't yet synced and can cause trip wires to be misaligned in clients (I think)
            layerToUse |= (1 << layerToAdd);

            StartCoroutine(SetupTripwire(wireStart, layerToUse));

            this.targetPosition = this.gameObject.transform.Find("Shotgun Position");

            if (IsHost)
            {
                StartCoroutine(FindAndSpawnShotgun());
            }
        }

        private IEnumerator SetupTripwire(Transform wireStart, int layerToUse)
        {
            yield return new WaitForSeconds(2f);
            RaycastHit hit;
            if (Physics.Raycast(wireStart.position, wireStart.forward, out hit, 5000f, layerToUse, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider)
                {
                    lr.SetPosition(1, hit.point);
                    objectTouchingTrigger = hit.collider.gameObject;

                    if (objectTouchingTrigger.transform.parent != null)
                    {
                        //if we're touching either a lockable door or a big door, set a listener to trigger the trap when they are opened
                        bool touchingNormalDoor = objectTouchingTrigger.transform.parent.gameObject.name == "DoorMesh";
                        bool touchingBigDoor = objectTouchingTrigger.name == "BigDoorRight" || objectTouchingTrigger.name == "BigDoorLeft";
                        if (touchingNormalDoor || touchingBigDoor)
                        {
                            if (IsHost)
                                touchingDoor.Value = true;

                            AnimatedObjectTrigger aot = new();
                            if (touchingNormalDoor)
                            {
                                aot = hit.collider.gameObject.GetComponent<AnimatedObjectTrigger>();
                                aot.onTriggerBool.AddListener(DoorInteract);

                            }
                            else if (touchingBigDoor)
                            {
                                aot = hit.collider.gameObject.transform.parent.gameObject.GetComponent<AnimatedObjectTrigger>();
                                aot.onTriggerBool.AddListener(DoorInteract);

                            }
                        }
                    }
                }
            }
            else
            {
                //so far away, just end the wire somewhere I guess
                lr.SetPosition(1, wireStart.forward * 5000f);
            }
        }

        private void DoorInteract(bool newBool)
        {
            if (touchingDoor.Value && !this.hasTriggered.Value)
            {
                //trigger the trap and unregister the listener
                TriggerTrap();
                objectTouchingTrigger.GetComponent<AnimatedObjectTrigger>().onTriggerBool.RemoveListener(DoorInteract);
                if (IsHost)
                    touchingDoor.Value = false;
            }
        }

        private IEnumerator FindAndSpawnShotgun()
        {
            yield return new WaitForSeconds(1f);
            SelectableLevel level = StartOfRound.Instance.levels.First((level) => level.PlanetName == "8 Titan");
            SpawnableEnemyWithRarity nutcracker = level.Enemies.Find((enemy) => enemy.enemyType.enemyName == "Nutcracker");
            GameObject shotgun = nutcracker.enemyType.enemyPrefab.gameObject.GetComponent<NutcrackerEnemyAI>().gunPrefab;

            if (shotgun != null)
            {
                GameObject shotgunObject = GameObject.Instantiate(shotgun, targetPosition.position, targetPosition.rotation, targetPosition);
                NetworkObject no = shotgunObject.GetComponent<NetworkObject>();
                no.Spawn(false);
                ShotgunItem sg = no.gameObject.GetComponent<ShotgunItem>();
                Item props = sg.itemProperties;
                Vector3 rot = props.rotationOffset;
                Vector3 pos = props.positionOffset;
                SetShotgunServerRpc(no);
            }
        }


        [ServerRpc]
        void SetShotgunServerRpc(NetworkObjectReference gun)
        {
            NetworkObject no;
            if (gun.TryGet(out no, null))
            {
                ShotgunItem si = no.gameObject.GetComponent<ShotgunItem>();
                int scrapVal = 50;
                si.SetScrapValue(scrapVal);
                int safetyChance = UnityEngine.Random.Range(0, 100);
                bool safety = (safetyChance < 94) ? false : true;
                si.safetyOn = safety;
                si.grabbable = true;
                int numShellsToLoad = 0;
                int genShellsChance = UnityEngine.Random.Range(0, 100);
                if (genShellsChance < 84)
                {
                    numShellsToLoad = UnityEngine.Random.Range(1, 3);
                    si.shellsLoaded = numShellsToLoad;
                }
                else
                {
                    si.shellsLoaded = 0;
                }
                si.itemProperties.syncGrabFunction = true;
                si.itemProperties.syncDiscardFunction = true;
                si.itemProperties.syncInteractLRFunction = true;
                si.itemProperties.syncUseFunction = true;

                this.shotgunObject = si;
                SetShotgunClientRpc(no, safety, numShellsToLoad, true, scrapVal);
            }
        }

        [ClientRpc]
        public void SetShotgunClientRpc(NetworkObjectReference gun, bool safety, int numShells, bool syncGrab, int value)
        {
            NetworkObject no;
            if (gun.TryGet(out no, null))
            {
                this.shotgunObject = no.gameObject.GetComponent<ShotgunItem>();
                this.shotgunObject.parentObject = targetPosition;
                this.shotgunObject.transform.parent = targetPosition;
                this.shotgunObject.safetyOn = safety;
                this.shotgunObject.shellsLoaded = numShells;
                this.shotgunObject.itemProperties.syncGrabFunction = syncGrab;
                this.shotgunObject.SetScrapValue(value);
            }
        }

        void Update()
        {
            if (!this.hasTriggered.Value)
            {
                RaycastHit hit;
                if (Physics.Raycast(lr.GetPosition(0), lr.transform.TransformDirection(Vector3.forward), out hit, Vector3.Distance(lr.GetPosition(0), lr.GetPosition(1)), lm))
                {
                    if (hit.collider)
                        TriggerTripped(hit.collider);
                }
            }
            else if (this.hasTriggered.Value && renderers[0].enabled)
            {
                DisableLineRenderers();
            }

            if (this.shotgunObject != null)
            {
                if (this.shotgunObject.heldByPlayerOnServer || this.shotgunObject.isHeld)
                {
                    if (this.shotgunObject.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                    {
                        GrabbedGunServerRpc();
                    }
                }
            }
        } 

        [ServerRpc(RequireOwnership = false)]
        public void GrabbedGunServerRpc()
        {
            if (this.shotgunObject != null)
            {
                this.shotgunObject = null;
            }

            GrabbedGunClientRpc();
        }

        [ClientRpc]
        public void GrabbedGunClientRpc()
        {
            if (this.shotgunObject != null)
            {
                this.shotgunObject = null;
            }
        }

        private void TriggerTripped(Collider other)
        {
            if (this.hasTriggered.Value) return;

            if (other.gameObject != objectTouchingTrigger)
            {
                TriggerTrap();
            }
        }

        private void TriggerTrap()
        {
            if (!this.hasTriggered.Value)
            {
                FireGunServerRpc();
                DisableLineRenderers();   
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void FireGunServerRpc()
        {
            this.hasTriggered.Value = true;
            FireGunClientRpc();
        }

        [ClientRpc]
        public void FireGunClientRpc()
        {
            FireGun();
        }

        private bool FireGun()
        {
            if (this.shotgunObject == null)
            {
                return false;
            }

            if (IsHost && hasTriggered.Value && !shotgunHasFired.Value)
            {
                shotgunHasFired.Value = true;
                if(this.shotgunObject.safetyOn || this.shotgunObject.shellsLoaded == 0)
                {
                    this.shotgunObject.gunAudio.PlayOneShot(this.shotgunObject.gunSafetySFX);

                }
                else
                {
                    this.shotgunObject.ShootGunAndSync(false);
                }
            }

            return this.shotgunObject != null;
        }
    }
}
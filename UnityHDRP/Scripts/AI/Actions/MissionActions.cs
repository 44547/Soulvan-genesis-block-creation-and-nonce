using UnityEngine;
using Soulvan.AI.Decision;

namespace Soulvan.AI.Actions
{
    /// <summary>
    /// GTA-style mission actions: pickup/drop cargo, hacking, stealth interactions.
    /// Integrates with AgentBlackboard for state management.
    /// </summary>
    public class MissionActions : MonoBehaviour
    {
        [Header("Cargo Settings")]
        [SerializeField] private Transform cargoAttachPoint;
        [SerializeField] private float pickupRadius = 3f;
        [SerializeField] private float hackDuration = 2.5f;

        [Header("Audio")]
        [SerializeField] private AudioClip cargoPickupSfx;
        [SerializeField] private AudioClip hackCompleteSfx;
        
        private AudioSource audioSource;
        private float hackTimer;
        private bool isHacking;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (!cargoAttachPoint)
            {
                // Create default attach point if not assigned
                var go = new GameObject("CargoAttachPoint");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(0f, 1.5f, -1f);
                cargoAttachPoint = go.transform;
            }
        }

        /// <summary>
        /// Pickup cargo and attach to agent.
        /// </summary>
        public bool PickupCargo(AgentBlackboard bb, Transform cargo)
        {
            if (!cargo) return false;
            if (bb.hasCargo) return false;
            
            float distance = Vector3.Distance(transform.position, cargo.position);
            if (distance > pickupRadius) return false;

            bb.hasCargo = true;
            bb.cargo = cargo;
            cargo.SetParent(cargoAttachPoint);
            cargo.localPosition = Vector3.zero;
            cargo.localRotation = Quaternion.identity;

            // Disable physics on cargo
            var cargoRb = cargo.GetComponent<Rigidbody>();
            if (cargoRb)
            {
                cargoRb.isKinematic = true;
            }

            PlaySound(cargoPickupSfx);
            Debug.Log($"[MissionActions] {gameObject.name} picked up cargo");
            
            return true;
        }

        /// <summary>
        /// Drop cargo at current location.
        /// </summary>
        public void DropCargo(AgentBlackboard bb)
        {
            if (!bb.hasCargo) return;
            if (!bb.cargo) return;

            bb.hasCargo = false;
            var cargo = bb.cargo;
            bb.cargo = null;

            cargo.SetParent(null);
            
            // Re-enable physics
            var cargoRb = cargo.GetComponent<Rigidbody>();
            if (cargoRb)
            {
                cargoRb.isKinematic = false;
            }

            PlaySound(cargoPickupSfx);
            Debug.Log($"[MissionActions] {gameObject.name} dropped cargo");
        }

        /// <summary>
        /// Start hacking gate/terminal (async operation).
        /// </summary>
        public void HackGate(System.Action onSuccess, System.Action onFail = null)
        {
            if (isHacking) return;

            isHacking = true;
            hackTimer = 0f;
            
            StartCoroutine(HackRoutine(onSuccess, onFail));
        }

        private System.Collections.IEnumerator HackRoutine(System.Action onSuccess, System.Action onFail)
        {
            Debug.Log($"[MissionActions] {gameObject.name} started hacking...");
            
            while (hackTimer < hackDuration)
            {
                hackTimer += Time.deltaTime;
                yield return null;
            }

            // Success
            isHacking = false;
            PlaySound(hackCompleteSfx);
            Debug.Log($"[MissionActions] {gameObject.name} hack complete!");
            onSuccess?.Invoke();
        }

        /// <summary>
        /// Check if agent can interact with target object.
        /// </summary>
        public bool CanInteract(Transform target, float interactRadius = 5f)
        {
            if (!target) return false;
            return Vector3.Distance(transform.position, target.position) <= interactRadius;
        }

        /// <summary>
        /// Trigger mission objective completion.
        /// </summary>
        public void CompleteMissionObjective(string objectiveId)
        {
            Debug.Log($"[MissionActions] {gameObject.name} completed objective: {objectiveId}");
            // Integrate with blockchain/reward system
            Infra.EventBus.EmitMissionCompleted(objectiveId);
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource && clip)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize pickup radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}

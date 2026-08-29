using UnityEngine;

/// <summary>
/// Controls two-handed weapon IK holding for First-Person and Third-Person.
/// Attaches to SK_Military_Survivalist (which has the Animator component).
/// </summary>
public class WeaponHandIK : MonoBehaviour
{
    [Header("Grip Targets (on the gun)")]
    [SerializeField] private Transform rightHandGrip;
    [SerializeField] private Transform leftHandGrip;

    [Header("Elbow Hints (optional, prevents arm twisting)")]
    [SerializeField] private Transform rightElbowHint;
    [SerializeField] private Transform leftElbowHint;

    [Header("IK Weights")]
    [Range(0f, 1f)] [SerializeField] private float rightHandWeight = 1f;
    [Range(0f, 1f)] [SerializeField] private float leftHandWeight = 1f;
    [Range(0f, 1f)] [SerializeField] private float elbowWeight = 0.8f;

    [Header("Hand Rotation Offsets (Fine Tuning)")]
    [SerializeField] private Vector3 rightHandRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 leftHandRotationOffset = Vector3.zero;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_animator == null) return;

        // Right Hand IK (Main Grip)
        if (rightHandGrip != null && rightHandWeight > 0f)
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandWeight);
            _animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandGrip.position);
            Quaternion targetRot = rightHandGrip.rotation * Quaternion.Euler(rightHandRotationOffset);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, targetRot);
        }
        else
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
        }

        // Left Hand IK (Foregrip / Barrel Guard)
        if (leftHandGrip != null && leftHandWeight > 0f)
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandWeight);
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandGrip.position);
            Quaternion targetRot = leftHandGrip.rotation * Quaternion.Euler(leftHandRotationOffset);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, targetRot);
        }
        else
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
        }

        // Elbow Hints
        if (rightElbowHint != null && elbowWeight > 0f)
        {
            _animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, elbowWeight);
            _animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowHint.position);
        }
        else
        {
            _animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0f);
        }

        if (leftElbowHint != null && elbowWeight > 0f)
        {
            _animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, elbowWeight);
            _animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowHint.position);
        }
        else
        {
            _animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0f);
        }
    }
}
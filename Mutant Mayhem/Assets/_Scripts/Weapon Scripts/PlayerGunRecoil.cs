using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;

public class PlayerGunRecoil : MonoBehaviour
{
    public Transform gunLimbTargets;
    public Transform leftHandTarget;
    public Transform rightHandTarget;
    public Transform leftHandTrans;
    public Transform rightHandTrans;
    public Transform leftHandAnchor;
    public Transform rightHandAnchor;
    public LimbSolver2D leftHandSolver;
    public IKManager2D iKManager2D;
    public Transform gunTrans;
    public float recoilDuration = 0.1f;

    void LateUpdate() 
    {
        // Sync the IK targets to follow the gun transform
        leftHandTarget.position = leftHandAnchor.position;
        rightHandTarget.position = rightHandAnchor.position;
        
        //gunTrans.rotation = originalGunRotation;
        leftHandTarget.rotation = gunTrans.rotation;
        rightHandTarget.rotation = gunTrans.rotation;

        leftHandAnchor.position = leftHandTrans.position;
        rightHandAnchor.position = rightHandTrans.position;
    }

    public void TriggerRecoil(Vector2 recoilAmount, bool isOneHanded)
    {
        StopAllCoroutines();
        StartCoroutine(RecoilCoroutine(recoilAmount, isOneHanded));
    }

    private IEnumerator RecoilCoroutine(Vector2 recoilAmount, bool isOneHanded)
    {
        // Setup for one-handed
        if (isOneHanded)
        {
            leftHandSolver.weight = 0;
        }
        else
        {
            leftHandSolver.weight = 1;
        }

        // Capture the current local positions and rotations as the original state
        Vector3 originalLeftHandLocalPos = gunTrans.InverseTransformPoint(leftHandTrans.position);
        Vector3 originalRightHandLocalPos = gunTrans.InverseTransformPoint(rightHandTrans.position);
        Quaternion originalGunRotation = gunTrans.rotation;
        Quaternion originalLeftHandRotation = leftHandTrans.rotation;
        Quaternion originalRightHandRotation = rightHandTrans.rotation;

        // Calculate the recoil direction in local space
        Vector3 recoilDir = new Vector3(recoilAmount.x, recoilAmount.y, 0);

        // Calculate the target positions in local space
        Vector3 targetLeftAnchorLocalPos = originalLeftHandLocalPos + recoilDir;
        Vector3 targetRightAnchorLocalPos = originalRightHandLocalPos + recoilDir;

        iKManager2D.weight = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < recoilDuration)
        {
            // Lerp the positions in local space
            Vector3 currentLeftAnchorLocalPos = Vector3.Lerp(originalLeftHandLocalPos, targetLeftAnchorLocalPos, elapsedTime / recoilDuration);
            Vector3 currentRightAnchorLocalPos = Vector3.Lerp(originalRightHandLocalPos, targetRightAnchorLocalPos, elapsedTime / recoilDuration);

            // Convert the local positions to world space
            leftHandAnchor.position = gunTrans.TransformPoint(currentLeftAnchorLocalPos);
            rightHandAnchor.position = gunTrans.TransformPoint(currentRightAnchorLocalPos);

            // Maintain original rotations
            gunTrans.rotation = originalGunRotation;
            leftHandTarget.rotation = originalLeftHandRotation;
            rightHandTarget.rotation = originalRightHandRotation;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset the IK weight
        iKManager2D.weight = 0;

        // Restore original positions in world space
        leftHandAnchor.position = leftHandTrans.TransformPoint(originalLeftHandLocalPos);
        rightHandAnchor.position = rightHandTrans.TransformPoint(originalRightHandLocalPos);
    }
}

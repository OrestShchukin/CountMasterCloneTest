using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CinemachineCamera[] cinemachineCameras;
    public static CameraSwitcher cameraSwitcherInstance;

    void Awake()
    {
        cameraSwitcherInstance = this;
        cinemachineCameras[0].GameObject().SetActive(true);
        for (int cameraIndex = 1; cameraIndex < cinemachineCameras.Length; cameraIndex++)
                cinemachineCameras[cameraIndex].GameObject().SetActive(false);
    }

    public void ActivateCinemachineCamera(int cameraIndex)
    {
        cinemachineCameras[cameraIndex - 1].GameObject().SetActive(true);
    }

    public void ActivateCinemachineCamera(CinemachineCamera camera)
    {
        camera.GameObject().SetActive(true);
    }

    public void DeactivateCinemachineCamera(int cameraIndex)
    {
        cinemachineCameras[cameraIndex - 1].GameObject().SetActive(false);
    }

    public void SwitchCameraTarget(int cameraIndex, Transform newTarget, float duration)
    {
        
        CinemachineCamera camera = cinemachineCameras[cameraIndex - 1];
        Transform baseTarget = camera.Follow;
        camera.Follow = newTarget;
        StartCoroutine(returnToBaseTargetAfter(duration));

        IEnumerator returnToBaseTargetAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            ActivateCinemachineCamera(3);
        }
    }
    public void SwitchCameraTarget(int cameraIndex, Transform newTarget)
    {
        CinemachineCamera camera = cinemachineCameras[cameraIndex - 1];
        camera.Follow = newTarget;
    }
}

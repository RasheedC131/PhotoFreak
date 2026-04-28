using UnityEngine;
using System.Collections; 

public class CameraFlash : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Light flashLight;
    
    [Header("Settings")]
    [SerializeField] private float flashDuration = 0.5f; 
    [SerializeField] private float flashIntensity = 50.0f; 
    [SerializeField] private Color flashColor = new Color(1f, 0.95f, 0.8f); 

    void Awake()
    {
        // auto create the child flash object under the photo camera object 
        if (flashLight == null)
        {
            flashLight = GetComponentInChildren<Light>(); 
            if (flashLight == null)
            {
                GameObject lightObj = new GameObject("FlashEmitter"); 
                lightObj.transform.SetParent(transform);
                lightObj.transform.localPosition = Vector3.zero; 
                lightObj.transform.localRotation = Quaternion.identity;

                flashLight = lightObj.AddComponent<Light>(); 
                flashLight.type = LightType.Spot; 
                flashLight.range = 40f; 
                flashLight.spotAngle = 60f; 
                flashLight.shadows = LightShadows.Hard;  
            }
        }

        flashLight.intensity = flashIntensity; 
        flashLight.color = flashColor; 
    }

    void OnEnable()
    {
        if (flashLight != null) flashLight.enabled = false; 
    }

    public void TriggerFlash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        flashLight.enabled = true;
        yield return new WaitForSeconds(flashDuration);
        flashLight.enabled = false;
    }
}

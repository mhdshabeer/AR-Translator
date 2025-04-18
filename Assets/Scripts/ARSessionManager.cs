using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Manages the AR session and camera functionality
/// </summary>
public class ARSessionManager : MonoBehaviour
{
    [SerializeField] private ARSession arSession;
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private ARRaycastManager arRaycastManager;
    
    [HideInInspector] public Camera ARCamera;
    
    public static ARSessionManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Get AR Camera reference
        ARCamera = arCameraManager.GetComponent<Camera>();
        
        // Initialize AR Foundation
        CheckARSupport();
    }

    private void CheckARSupport()
    {
        // For webcam mode, skip AR check and just use camera
        if (!Application.isMobilePlatform)
        {
            Debug.Log("Desktop platform detected, initializing webcam...");
            
            // Get available webcam devices
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("No webcam found!");
                UIManager.Instance.ShowErrorMessage("No webcam detected. Please ensure a camera is connected.");
                return;
            }

            // Use the first available webcam
            WebCamTexture webcamTexture = new WebCamTexture(devices[0].name, 1280, 720, 30);
            webcamTexture.Play();

            // Wait for webcam to start
            while (!webcamTexture.isPlaying && !webcamTexture.didUpdateThisFrame)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // Create a Raw Image component to display webcam feed
            var rawImage = GetComponent<UnityEngine.UI.RawImage>();
            if (rawImage == null)
            {
                rawImage = gameObject.AddComponent<UnityEngine.UI.RawImage>();
            }
            rawImage.texture = webcamTexture;
            
            Debug.Log("Webcam initialized successfully");
            StartARSession();
        }
        else if (ARSession.state == ARSessionState.None || 
                 ARSession.state == ARSessionState.CheckingAvailability)
        {
            StartCoroutine(CheckAvailability());
        }
        else
        {
            StartARSession();
        }
    }

    private IEnumerator CheckAvailability()
    {
        yield return ARSession.CheckAvailability();

        if (ARSession.state == ARSessionState.Unsupported)
        {
            Debug.LogWarning("AR is not supported on this device.");
            UIManager.Instance.ShowARNotSupportedMessage();
        }
        else
        {
            StartARSession();
        }
    }

    private void StartARSession()
    {
        if (arSession != null)
        {
            arSession.enabled = true;
            Debug.Log("AR Session started");
        }
    }

    /// <summary>
    /// Performs a raycast to find planes in the AR scene
    /// </summary>
    public bool TryGetRaycastHit(Vector2 screenPoint, out ARRaycastHit hit)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (arRaycastManager.Raycast(screenPoint, hits, TrackableType.PlaneWithinPolygon))
        {
            hit = hits[0];
            return true;
        }
        
        hit = default;
        return false;
    }

    /// <summary>
    /// Pauses the AR session
    /// </summary>
    public void PauseARSession()
    {
        if (arSession != null)
        {
            arSession.enabled = false;
        }
    }

    /// <summary>
    /// Resumes the AR session
    /// </summary>
    public void ResumeARSession()
    {
        if (arSession != null)
        {
            arSession.enabled = true;
        }
    }
}

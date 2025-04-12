using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;

/// <summary>
/// Handles text recognition from the camera feed
/// </summary>
public class TextRecognizer : MonoBehaviour
{
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private float recognitionInterval = 0.5f; // Time between recognition attempts
    
    private bool isProcessing = false;
    private float lastCaptureTime = 0f;
    
    public static TextRecognizer Instance { get; private set; }
    
    // Event that is triggered when text is recognized
    public event Action<string, Rect> OnTextRecognized;

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
        }
    }

    private void OnEnable()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived += OnCameraFrameReceived;
        }
    }

    private void OnDisable()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived -= OnCameraFrameReceived;
        }
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        // Check if enough time has passed since the last recognition
        if (Time.time - lastCaptureTime < recognitionInterval || isProcessing)
        {
            return;
        }
        
        lastCaptureTime = Time.time;
        StartCoroutine(ProcessCameraImage());
    }

    private IEnumerator ProcessCameraImage()
    {
        isProcessing = true;
        
        // Try to get the latest camera image
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            isProcessing = false;
            yield break;
        }
        
        using (image)
        {
            // Convert XRCpuImage to texture
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width / 2, image.height / 2), // Reduce size for processing
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };
            
            int size = image.GetConvertedDataSize(conversionParams);
            var buffer = new NativeArray<byte>(size, Allocator.Temp);
            
            // Convert the image to regular texture format
            image.Convert(conversionParams, buffer.GetSubArray(0, size));
            
            // Create a texture to hold the image
            var texture = new Texture2D(
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                conversionParams.outputFormat,
                false);
            
            // Load the converted data into the texture
            texture.LoadRawTextureData(buffer);
            texture.Apply();
            
            buffer.Dispose();
            
            // Process the texture for text recognition
            yield return RecognizeTextInTexture(texture);
            
            // Clean up
            Destroy(texture);
        }
        
        isProcessing = false;
    }

    private IEnumerator RecognizeTextInTexture(Texture2D texture)
    {
        // In a real implementation, you would use a text recognition library or API here
        // For this example, we'll simulate finding text with a mock implementation
        
        // Simulating text processing delay
        yield return new WaitForSeconds(0.1f);
        
        // Simulate detected text areas
        // In a real implementation, this would be the result from the ML model
        DetectTextAreas(texture);
    }

    private void DetectTextAreas(Texture2D texture)
    {
        // This is a placeholder for an actual ML-based text detection
        // In a real application, this would implement or call an ML model for text detection
        
        // For now, we'll just create a simple pixel luminance analysis to detect potential text regions
        // This is a very rudimentary approach and won't work well in practice
        
        int width = texture.width;
        int height = texture.height;
        Color[] pixels = texture.GetPixels();
        
        // Find areas of high contrast that might indicate text
        List<Rect> potentialTextAreas = new List<Rect>();
        
        // Very simplified detection - looking for high contrast areas
        // In a real app, use Unity ML Agents or TensorFlow Lite
        for (int y = 0; y < height - 10; y += 10)
        {
            for (int x = 0; x < width - 20; x += 20)
            {
                float contrast = CalculateContrastInRegion(pixels, x, y, 20, 10, width);
                if (contrast > 0.5f) // Arbitrary threshold
                {
                    // Create a normalized rect (0-1 range)
                    Rect rect = new Rect(
                        (float)x / width,
                        (float)y / height,
                        20f / width,
                        10f / height
                    );
                    
                    potentialTextAreas.Add(rect);
                    
                    // Simulate OCR by sending a notification that text was found
                    // Here we would normally extract the actual text, but we're simulating
                    OnTextRecognized?.Invoke("Sample Text", rect);
                }
            }
        }
    }
    
    private float CalculateContrastInRegion(Color[] pixels, int x, int y, int width, int height, int textureWidth)
    {
        // Simple contrast calculation for a region
        float minLuminance = 1f;
        float maxLuminance = 0f;
        
        for (int j = y; j < y + height; j++)
        {
            for (int i = x; i < x + width; i++)
            {
                int index = j * textureWidth + i;
                if (index < pixels.Length)
                {
                    Color pixel = pixels[index];
                    float luminance = 0.299f * pixel.r + 0.587f * pixel.g + 0.114f * pixel.b;
                    
                    minLuminance = Mathf.Min(minLuminance, luminance);
                    maxLuminance = Mathf.Max(maxLuminance, luminance);
                }
            }
        }
        
        return maxLuminance - minLuminance;
    }
    
    // Public method to force a text recognition pass
    public void TriggerTextRecognition()
    {
        if (!isProcessing)
        {
            lastCaptureTime = 0; // Reset timer to force processing
        }
    }
}

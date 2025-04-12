# AR Translator App

An augmented reality translation application built with Unity that recognizes and translates text in real-time on both iOS and Android devices.

## Features

- Real-time text recognition within the camera view
- Instant translation of recognized text
- Overlay of translated text on top of original text
- Support for multiple languages
- Simple UI for language selection
- Cross-platform support for iOS and Android

## Requirements

- Unity 2022.3 LTS or newer
- AR Foundation package
- ARCore (for Android)
- ARKit (for iOS)
- iOS 11+ or Android 7.0+
- Device with camera and support for AR

## Setup

1. Clone or download this repository
2. Open the project in Unity 2022.3 LTS or newer
3. Install required packages if not already included:
   - AR Foundation
   - ARCore XR Plugin (for Android builds)
   - ARKit XR Plugin (for iOS builds)
   - TextMeshPro
4. Open the MainScene in Assets/Scenes
5. Configure build settings for your target platform (iOS or Android)
6. For iOS, ensure you have proper signing and capabilities set
7. For Android, ensure your manifest includes camera permissions

## Usage

1. Launch the app on your device
2. Point your camera at text you want to translate
3. The app will recognize text and display translations on screen
4. Use the UI to change source and target languages as needed

## How It Works

The app uses Unity's AR Foundation framework to access the device camera and AR capabilities. Text recognition is performed on camera frames to identify text regions. Recognized text is sent to a translation API, and the translated text is displayed as an overlay on the original text.

## Limitations

- Text recognition works best on clear, well-lit text
- Some languages may have reduced accuracy
- Internet connection is required for translation
- Translation accuracy depends on the free translation API used

## Credits

This app uses LibreTranslate as the default translation service, with a fallback to Google Translate API.

## License

MIT License

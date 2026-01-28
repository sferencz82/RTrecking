# Capacitor + Ionic Setup Guide

## Overview

The Angular app now supports both web and iOS native deployment using Capacitor and Ionic.

## Installation Commands

```bash
# Install Capacitor and Ionic packages
npm install @ionic/angular @capacitor/core @capacitor/cli @capacitor/ios @capacitor/camera

# Initialize Capacitor (already done)
npx cap init "Receipt Tracking" "com.receipttracking.app" --web-dir=dist/frontend
```

## Configuration

### capacitor.config.ts

Located at `frontend/capacitor.config.ts`:

- **appId**: `com.receipttracking.app`
- **appName**: `Receipt Tracking`
- **webDir**: `dist/frontend` (Angular build output)
- **Camera Plugin**: Configured with permission messages

### App Config

`app.config.ts` includes:
- `provideIonicAngular({})` for Ionic integration

## Camera Integration

### CameraService

New service at `src/app/services/camera.service.ts`:

- `isNativePlatform()` - Detects if running on iOS/Android
- `takePhoto()` - Opens device camera
- `pickPhoto()` - Opens device photo gallery
- `convertPhotoToFile()` - Converts Capacitor Photo to File object

### Upload Component Updates

The upload component now supports:
1. **Take Photo** (iOS/Android only) - Uses device camera
2. **Choose from Gallery** (iOS/Android only) - Opens photo picker
3. **Choose File** (Web) - Traditional file input

The component automatically detects the platform and shows appropriate options.

## iOS Setup

### Initial Setup

```bash
# Build the Angular app
ng build

# Add iOS platform
npx cap add ios

# Sync web assets to iOS
npx cap sync ios

# Open in Xcode
npx cap open ios
```

### Build and Sync Scripts

Added to `package.json`:
- `npm run build:ios` - Build and sync to iOS
- `npm run open:ios` - Open Xcode project
- `npm run sync:ios` - Sync web assets only

### iOS Permissions

Camera permissions are configured in `capacitor.config.ts`. iOS will prompt for:
- **Camera**: "This app needs access to your camera to take receipt photos."
- **Photos**: "This app needs access to your photos to select receipt images."

You may also need to configure `Info.plist` in Xcode:
- `NSCameraUsageDescription`
- `NSPhotoLibraryUsageDescription`

## Development Workflow

### Web Development

```bash
# Normal Angular development
ng serve
```

### iOS Development

```bash
# 1. Build Angular app
ng build

# 2. Sync to iOS
npx cap sync ios

# 3. Open in Xcode
npx cap open ios

# 4. Build and run in Xcode or iOS Simulator
```

### After Code Changes

When you make changes to Angular code:

```bash
# Rebuild
ng build

# Sync changes to iOS
npx cap sync ios
```

**Note**: `npx cap sync` copies web assets and updates native dependencies. Use this after:
- Installing new Capacitor plugins
- Changing `capacitor.config.ts`
- Building the Angular app

## Camera Plugin Usage

### Taking Photos

```typescript
import { CameraService } from './services/camera.service';

constructor(private cameraService: CameraService) {}

async takePhoto() {
  const file = await this.cameraService.takePhoto();
  if (file) {
    // Use file like any File object
    this.uploadFile(file);
  }
}
```

### Platform Detection

```typescript
if (this.cameraService.isNativePlatform()) {
  // Show camera/gallery options
} else {
  // Show file input
}
```

## Features

### Web App
- Traditional file input for image selection
- All existing functionality works as before

### iOS App
- Native camera integration
- Photo gallery picker
- Same Angular codebase
- Native performance

## File Upload Flow

Both web and iOS use the same flow:
1. Select image (file input OR camera/gallery)
2. Preview image
3. Upload to backend
4. Process OCR
5. Review and apply draft

The only difference is the image selection method.

## Troubleshooting

### Camera Not Working
- Check permissions in iOS Settings
- Verify `Info.plist` has camera permissions
- Check Xcode console for errors

### Build Issues
- Ensure `ng build` completes successfully
- Run `npx cap sync ios` after each build
- Clean build folder: `rm -rf ios` then `npx cap add ios`

### Sync Issues
- Delete `ios` folder and re-add: `npx cap add ios`
- Check `capacitor.config.ts` webDir path matches build output

## Next Steps

1. **Test on iOS Simulator**:
   ```bash
   ng build
   npx cap sync ios
   npx cap open ios
   # Run in Xcode simulator
   ```

2. **Test on Physical Device**:
   - Connect iPhone via USB
   - Select device in Xcode
   - Build and run

3. **Configure App Icon and Splash**:
   - Update assets in Xcode
   - Or use Capacitor assets plugin

4. **App Store Preparation**:
   - Configure app signing
   - Set up provisioning profiles
   - Prepare screenshots and metadata

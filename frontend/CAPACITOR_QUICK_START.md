# Capacitor + Ionic Quick Start

## Installation (Already Done)

```bash
npm install @ionic/angular @capacitor/core @capacitor/cli @capacitor/ios @capacitor/camera
npx cap init "Receipt Tracking" "com.receipttracking.app" --web-dir=dist/frontend
```

## Configuration Files

### capacitor.config.ts
- App ID: `com.receipttracking.app`
- Web directory: `dist/frontend`
- Camera permissions configured

### app.config.ts
- Added `provideIonicAngular({})` for Ionic support

## Camera Integration

### CameraService
- `takePhoto()` - Opens device camera
- `pickPhoto()` - Opens photo gallery
- `isNativePlatform()` - Detects iOS/Android

### Upload Component
- **Web**: Shows "Choose File" option
- **iOS/Android**: Shows menu with:
  - "Take Photo" (camera)
  - "Choose from Gallery"
  - "Choose File" (fallback)

## iOS Setup Commands

```bash
# 1. Build Angular app
ng build

# 2. Add iOS platform (first time only)
npx cap add ios

# 3. Sync web assets to iOS
npx cap sync ios

# 4. Open in Xcode
npx cap open ios
```

## Development Workflow

### After Code Changes

```bash
# Rebuild and sync
ng build
npx cap sync ios

# Or use script
npm run build:ios
```

### Using Scripts

```bash
npm run build:ios    # Build and sync
npm run open:ios     # Open Xcode
npm run sync:ios     # Sync only
```

## Camera Usage

The upload component automatically:
1. Detects if running on native platform
2. Shows camera/gallery options on iOS
3. Shows file input on web
4. Converts camera photos to File objects
5. Works with existing upload flow

## Testing

### Web
```bash
ng serve
# Navigate to Upload Receipt
# Use "Choose File" option
```

### iOS Simulator
```bash
ng build
npx cap sync ios
npx cap open ios
# Run in Xcode simulator
# Camera won't work in simulator, use gallery
```

### iOS Device
```bash
ng build
npx cap sync ios
npx cap open ios
# Connect iPhone, build and run
# Camera will work on device
```

## Permissions

Camera permissions are configured in `capacitor.config.ts`. iOS will automatically prompt for:
- Camera access
- Photo library access

## Notes

- **Same Codebase**: Works on web and iOS
- **Platform Detection**: Automatic
- **File Upload**: Same API, different source (camera vs file input)
- **No Code Changes**: Upload flow unchanged, just different image source

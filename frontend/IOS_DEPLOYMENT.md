# iOS Deployment Guide

## Prerequisites

- macOS with Xcode installed
- Apple Developer account (for device testing and App Store)
- iOS device or simulator

## Step-by-Step iOS Setup

### 1. Build Angular App

```bash
cd frontend
ng build --configuration production
```

This creates the `dist/frontend` directory with compiled assets.

### 2. Add iOS Platform

```bash
npx cap add ios
```

This creates the `ios` folder with a native iOS project.

### 3. Sync Web Assets

```bash
npx cap sync ios
```

This copies:
- Web assets from `dist/frontend` to iOS project
- Updates native dependencies
- Updates `capacitor.config.ts` changes

### 4. Open in Xcode

```bash
npx cap open ios
```

This opens the iOS project in Xcode.

## Xcode Configuration

### 1. Signing & Capabilities

1. Select the project in Xcode
2. Go to "Signing & Capabilities"
3. Select your development team
4. Xcode will automatically create provisioning profile

### 2. Camera Permissions

The permissions are configured in `capacitor.config.ts`, but you can verify in Xcode:

1. Open `Info.plist`
2. Ensure these keys exist:
   - `NSCameraUsageDescription`: "This app needs access to your camera to take receipt photos."
   - `NSPhotoLibraryUsageDescription`: "This app needs access to your photos to select receipt images."

### 3. App Configuration

- **Bundle Identifier**: `com.receipttracking.app` (from capacitor.config.ts)
- **Display Name**: "Receipt Tracking"
- **Version**: Update as needed
- **Minimum iOS Version**: 13.0+ (Capacitor requirement)

## Running on Simulator

1. Open Xcode
2. Select a simulator (e.g., iPhone 15)
3. Click Run (▶️) or press `Cmd+R`
4. App will build and launch in simulator

## Running on Physical Device

1. Connect iPhone via USB
2. Trust the computer on iPhone
3. In Xcode, select your device from the device list
4. Click Run
5. On first run, you may need to:
   - Trust the developer certificate on iPhone
   - Settings → General → VPN & Device Management → Trust

## Development Workflow

### After Code Changes

```bash
# 1. Rebuild Angular app
ng build

# 2. Sync to iOS
npx cap sync ios

# 3. In Xcode, build and run again
```

### Using Scripts

```bash
# Build and sync in one command
npm run build:ios

# Then open Xcode
npm run open:ios
```

## API Configuration for iOS

### Development

For iOS Simulator/Device, update `environment.ts`:

```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000'  // For simulator
  // For physical device, use your Mac's IP:
  // apiBaseUrl: 'http://192.168.1.xxx:5000'
};
```

### Production

For production builds:

```typescript
// environment.prod.ts
export const environment = {
  production: true,
  apiBaseUrl: 'https://your-api-domain.com'
};
```

## Camera Testing

1. Launch app on device/simulator
2. Navigate to "Upload Receipt"
3. Tap "Select Image"
4. Choose "Take Photo" or "Choose from Gallery"
5. Camera/gallery should open
6. Take/select photo
7. Photo should appear in preview
8. Continue with upload flow

## Building for App Store

### 1. Archive Build

1. In Xcode, select "Any iOS Device" as target
2. Product → Archive
3. Wait for archive to complete

### 2. Distribute

1. Window → Organizer
2. Select your archive
3. Click "Distribute App"
4. Follow App Store Connect workflow

### 3. App Store Connect

1. Create app in App Store Connect
2. Upload build via Xcode or Transporter
3. Fill in app information
4. Submit for review

## Troubleshooting

### Camera Permission Denied

- Check `Info.plist` has permission descriptions
- Reset permissions: Settings → Privacy → Camera → Reset
- Reinstall app

### API Connection Issues

- For simulator: Use `localhost:5000`
- For device: Use Mac's local IP address
- Check firewall settings
- Ensure backend CORS allows device IP

### Build Errors

- Clean build folder: Product → Clean Build Folder
- Delete derived data
- Re-run `npx cap sync ios`

### Sync Issues

- Delete `ios` folder
- Run `npx cap add ios` again
- Verify `webDir` in `capacitor.config.ts` matches build output

## Notes

- **Same Codebase**: The Angular app works identically on web and iOS
- **Platform Detection**: Camera options only show on native platforms
- **File Upload**: Works the same way on both platforms
- **Performance**: Native iOS app has better performance than web view

import { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.receipttracking.app',
  appName: 'Receipt Tracking',
  webDir: 'dist/frontend',
  server: {
    android: {
      allowMixedContent: true
    },
    ios: {
      allowMixedContent: true
    }
  },
  plugins: {
    Camera: {
      permissions: {
        camera: 'This app needs access to your camera to take receipt photos.',
        photos: 'This app needs access to your photos to select receipt images.'
      }
    }
  }
};

export default config;

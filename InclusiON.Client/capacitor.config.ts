import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.inclusion.app',
  appName: 'InclusiON',
  webDir: 'dist/inclusion-client/browser',
  server: {
    androidScheme: 'https',
    cleartext: true
  }
};

export default config;

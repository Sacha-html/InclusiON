export const environment = {
  production: false,

  // URLs del Backend (IP local para conexiones desde teléfono/tablet)
  apiUrl: 'http://192.168.0.12:5000/api',

  // Configuración de la Aplicación
  appName: 'InclusiON',
  version: '1.0.0',

  // Logging
  enableDebugLogs: true,

  // HTTP Client
  httpTimeout: 30000,
  maxRetries: 3,

  // Storage Keys
  storageKeys: {
    accessToken: 'access_token',
    refreshToken: 'refresh_token',
    currentUser: 'current_user',
    theme: 'app_theme',
  },
};

export const environment = {
  production: true,

  // URLs del Backend
  apiUrl: '/api',

  // Configuración de la Aplicación
  appName: 'InclusiON',
  version: '1.0.0',

  // Logging
  enableDebugLogs: false,

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

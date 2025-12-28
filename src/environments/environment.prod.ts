export const environment = {
  production: true,

  // URLs del Backend (CAMBIAR EN PRODUCCIÓN)
  apiUrl: 'https://api.tudominio.com/api',
  signalRUrl: 'https://api.tudominio.com/chathub',

  // Configuración de la Aplicación
  appName: 'Mi Aplicación de Chat',
  version: '1.0.0',

  // Logging (DESACTIVAR EN PRODUCCIÓN)
  enableDebugLogs: false,

  // HTTP Client
  httpTimeout: 30000,
  maxRetries: 3,

  // Features
  features: {
    chat: true,
    notifications: true,
    fileUpload: true,
    voiceMessages: false,
    videoCall: false,
  },

  // Configuración de Chat
  chatConfig: {
    maxMessageLength: 5000,
    reconnectAttempts: 5,
    reconnectInterval: 3000,
    typingIndicatorDelay: 500,
    messageLoadBatchSize: 50,
    autoScrollEnabled: true,
    soundEnabled: true,
    notificationsEnabled: true,
    theme: 'light' as const,
  },

  // SignalR Configuration
  signalR: {
    enableAutoReconnect: true,
    reconnectDelays: [0, 2000, 5000, 10000, 30000],
    serverTimeout: 30000,
    keepAliveInterval: 15000,
    logLevel: 'Error' as const, // Solo errores en producción
  },

  // Storage Keys
  storageKeys: {
    accessToken: 'access_token',
    refreshToken: 'refresh_token',
    currentUser: 'current_user',
    chatConfig: 'chat_config',
    theme: 'app_theme',
  },
};

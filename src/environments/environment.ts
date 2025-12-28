export const environment = {
  production: false,

  // URLs del Backend
  apiUrl: 'https://localhost:5001/api',
  signalRUrl: 'https://localhost:5000/chathub',

  // Configuración de la Aplicación
  appName: 'Mi Aplicación de Chat',
  version: '1.0.0',

  // Logging
  enableDebugLogs: true,

  // HTTP Client
  httpTimeout: 30000, // 30 segundos
  maxRetries: 3,

  // Features (Feature Flags)
  features: {
    chat: true,
    notifications: true,
    fileUpload: true,
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
    logLevel: 'Information' as const,
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

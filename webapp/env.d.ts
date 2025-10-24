/// <reference types="bun-types" />
/* Ambient env vars for the Bun server */
declare namespace NodeJS {
  interface ProcessEnv {
    PORT?: string;
    BASE_URL?: string; // e.g. https://yourdomain.example
    SESSION_SECRET?: string; // any long random string
    STEAM_API_KEY?: string; // Web API key from https://steamcommunity.com/dev/apikey
    NODE_ENV?: 'development' | 'production' | 'test';
  }
}


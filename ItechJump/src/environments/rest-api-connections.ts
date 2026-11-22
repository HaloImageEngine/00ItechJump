/**
 * REST API connection profiles for EmpireOneRestAPIITJ.
 * Set `apiConnection` in the active environment file to choose local or prod.
 */
export type RestApiConnectionKey = 'local' | 'prod';

export interface RestApiConnection {
  key: RestApiConnectionKey;
  label: string;
  /** Origin without trailing slash (absolute URL to the API site). */
  baseUrl: string;
  /**
   * Same-origin relative URLs when using `ng serve` + proxy (local only).
   * Proxy: proxy.conf.local.json → http://localhost:64150
   */
  proxyBaseUrl: string;
}

export const REST_API_LOCAL: RestApiConnection = {
  key: 'local',
  label: 'Local REST API',
  baseUrl: 'http://localhost:64150',
  proxyBaseUrl: ''
};

export const REST_API_PROD: RestApiConnection = {
  key: 'prod',
  label: 'Production REST API',
  baseUrl: 'https://api.itechjump.com',
  proxyBaseUrl: ''
};

export const REST_API_CONNECTIONS: Record<RestApiConnectionKey, RestApiConnection> = {
  local: REST_API_LOCAL,
  prod: REST_API_PROD
};

export interface RestApiEnvironment {
  apiConnection: RestApiConnectionKey;
  restApiLocal: RestApiConnection;
  restApiProd: RestApiConnection;
  /** When true and apiConnection is local, use proxyBaseUrl (dev server proxy). */
  useRestApiProxy: boolean;
  /**
   * Production: call `/api` and `/QandA` on the same host as the UI (IIS reverse proxy).
   * Avoids mixed-content when the UI is HTTPS and the API is separate.
   */
  useSameOriginApi?: boolean;
}

export function resolveRestApiBaseUrl(env: RestApiEnvironment): string {
  const active = REST_API_CONNECTIONS[env.apiConnection];
  if (env.apiConnection === 'local' && env.useRestApiProxy) {
    return active.proxyBaseUrl.replace(/\/$/, '');
  }
  if (env.apiConnection === 'prod' && env.useSameOriginApi) {
    return '';
  }
  let url = active.baseUrl.replace(/\/$/, '');
  if (
    env.apiConnection === 'prod' &&
    typeof globalThis !== 'undefined' &&
    globalThis.location?.protocol === 'https:' &&
    url.startsWith('http://')
  ) {
    url = `https://${url.slice('http://'.length)}`;
  }
  return url;
}

export function getActiveRestApiConnection(env: RestApiEnvironment): RestApiConnection {
  return REST_API_CONNECTIONS[env.apiConnection];
}
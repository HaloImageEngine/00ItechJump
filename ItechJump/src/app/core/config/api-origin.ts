import { environment } from '../../../environments/environment';
import {
  getActiveRestApiConnection,
  resolveRestApiBaseUrl
} from '../../../environments/rest-api-connections';

/** Active REST API origin without a trailing slash (empty = same-origin dev proxy). */
export const API_ORIGIN = resolveRestApiBaseUrl(environment);

/** Which connection profile is active (local or prod). */
export const ACTIVE_REST_API = getActiveRestApiConnection(environment);
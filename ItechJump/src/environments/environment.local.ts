import { versionId } from './version';
import { REST_API_LOCAL, REST_API_PROD } from './rest-api-connections';

export const environment = {
  production: false,
  versionId,
  appVersion: 'L26.11.22.46',
  stripePublishableKey:
    'pk_test_51SehAWDpJWWE5ZOMhafwFHJzEtlqwqncK9b22XTv3zgDq5I5jXbP6YrnJ6NRajkzn9vpGzpYveQ7bdOV3kSWlYuV00jegahgj1',
  debugMode: true,
  apiConnection: 'local' as const,
  restApiLocal: REST_API_LOCAL,
  restApiProd: REST_API_PROD,
  useRestApiProxy: true
};
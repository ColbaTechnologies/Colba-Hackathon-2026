export type Tenant = {
  id: string;
  passwordHash: string;
};

export type TenantsRepository = {
  store: (tenant: Tenant) => Promise<void>;
  getById: (tenantId: string) => Promise<Tenant | undefined>;
  createApiKey: (tenantId: string) => Promise<string>;
  validateApiKey: (apiKey: string) => Promise<Tenant | undefined>;
};
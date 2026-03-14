export type Tenant = {
  id: string;
  passwordHash: string;
};

export type TenantsRepository = {
  store: (tenant: Tenant) => Promise<void>;
  validate: (id: string, password: string) => Promise<boolean>;
  getById: (tenantId: string) => Promise<Tenant | undefined>;
  createApiKey: (tenantId: string) => Promise<string>;
  validateApiKey: (apiKey: string) => Promise<Tenant | undefined>;
};
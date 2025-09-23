import { FunctionDeclaration } from '@google/genai';

export interface ChatbotFunctionDeclaration extends FunctionDeclaration {
  roles: UserRole[];
  handler: (args: Record<string, unknown>) => Promise<unknown>;
}

export type UserRole = 'Admin' | 'Manager' | 'HR' | 'Employee';

export interface FunctionExecutionContext {
  userId: number;
  userRoles: string[];
  userName: string;
  userEmail: string;
}

export interface FunctionExecutionResult {
  success: boolean;
  data?: unknown;
  error?: string;
  message?: string;
}

export interface ChatbotConfig {
  modelName: string;
  maxTokens?: number;
  temperature?: number;
  topP?: number;
  enableFunctionCalling: boolean;
  allowedRoles: UserRole[];
}
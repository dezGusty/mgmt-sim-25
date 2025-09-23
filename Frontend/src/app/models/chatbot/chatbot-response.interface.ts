export interface ChatbotResponse {
  success: boolean;
  message?: string;
  data?: unknown;
  error?: string;
  requiresAction?: boolean;
  suggestedActions?: SuggestedAction[];
}

export interface SuggestedAction {
  label: string;
  action: string;
  parameters?: Record<string, unknown>;
  description?: string;
}

export interface ChatbotState {
  isInitialized: boolean;
  isLoading: boolean;
  error?: string;
  currentSession?: string;
  userContext?: UserContext;
}

export interface UserContext {
  userId: number;
  roles: string[];
  firstName: string;
  lastName: string;
  email: string;
  departmentName?: string;
  jobTitleName?: string;
  isManager: boolean;
  subordinateIds?: number[];
}

export interface QuickAction {
  id: string;
  label: string;
  prompt: string;
  icon?: string;
  roles: string[];
  category: QuickActionCategory;
}

export type QuickActionCategory = 'leave' | 'team' | 'reports' | 'admin' | 'general';
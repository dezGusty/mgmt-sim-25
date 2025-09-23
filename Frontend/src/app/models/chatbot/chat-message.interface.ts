export interface ChatMessage {
  id: string;
  content: string;
  role: 'user' | 'assistant' | 'system';
  timestamp: Date;
  functionCall?: FunctionCallInfo;
  functionResponse?: FunctionResponseInfo;
  isLoading?: boolean;
  error?: string;
}

export interface FunctionCallInfo {
  name: string;
  arguments: Record<string, unknown>;
  id?: string;
}

export interface FunctionResponseInfo {
  name: string;
  result: unknown;
  error?: string;
  id?: string;
}

export interface ChatSession {
  id: string;
  messages: ChatMessage[];
  isActive: boolean;
  createdAt: Date;
  lastMessageAt: Date;
}
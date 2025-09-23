import { Component, OnInit, OnDestroy, ElementRef, ViewChild, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ChatbotService } from '../../services/chatbot/chatbot.service';
import {
  ChatMessage,
  ChatbotState,
  UserContext,
  QuickAction
} from '../../models/chatbot';

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot.component.html',
  styleUrls: ['./chatbot.component.css']
})
export class ChatbotComponent implements OnInit, OnDestroy {
  @ViewChild('messagesContainer', { static: false }) messagesContainer!: ElementRef;
  @ViewChild('messageInput', { static: false }) messageInput!: ElementRef;

  isOpen = false;
  isMinimized = false;
  currentMessage = '';
  messages: ChatMessage[] = [];
  chatbotState: ChatbotState = {
    isInitialized: false,
    isLoading: false
  };
  userContext: UserContext | null = null;
  quickActions: QuickAction[] = [];
  showQuickActions = true;

  private destroy$ = new Subject<void>();

  constructor(
    private chatbotService: ChatbotService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.initializeComponent();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeComponent(): void {
    // Subscribe to chatbot state changes
    this.chatbotService.chatState$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.chatbotState = state;
        this.userContext = state.userContext || null;
        this.cdr.detectChanges();
      });

    // Subscribe to message updates
    this.chatbotService.messages$
      .pipe(takeUntil(this.destroy$))
      .subscribe(messages => {
        this.messages = messages;
        this.scrollToBottom();
        this.cdr.detectChanges();
      });

    // Load quick actions
    this.loadQuickActions();
  }

  private loadQuickActions(): void {
    // Wait for initialization before loading quick actions
    this.chatbotService.chatState$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        if (state.isInitialized) {
          this.quickActions = this.chatbotService.getQuickActions();
          this.cdr.detectChanges();
        }
      });
  }

  toggleChatbot(): void {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.isMinimized = false;
      this.focusMessageInput();
    }
  }

  minimizeChatbot(): void {
    this.isMinimized = true;
  }

  maximizeChatbot(): void {
    this.isMinimized = false;
    this.focusMessageInput();
  }

  closeChatbot(): void {
    this.isOpen = false;
    this.isMinimized = false;
  }

  async sendMessage(): Promise<void> {
    if (!this.currentMessage.trim() || this.chatbotState.isLoading) {
      return;
    }

    const messageToSend = this.currentMessage.trim();
    this.currentMessage = '';
    this.showQuickActions = false;

    try {
      await this.chatbotService.sendMessage(messageToSend);
    } catch (error) {
      console.error('Error sending message:', error);
      // Error handling is done in the service
    }

    this.focusMessageInput();
  }

  async sendQuickAction(action: QuickAction): Promise<void> {
    if (this.chatbotState.isLoading) {
      return;
    }

    this.showQuickActions = false;

    try {
      await this.chatbotService.sendMessage(action.prompt);
    } catch (error) {
      console.error('Error sending quick action:', error);
    }
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  clearChat(): void {
    this.chatbotService.clearChat();
    this.showQuickActions = true;
  }

  getMessageTime(message: ChatMessage): string {
    return message.timestamp.toLocaleTimeString([], {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getMessageClass(message: ChatMessage): string {
    const baseClass = 'message';
    const roleClass = message.role === 'user' ? 'user-message' : 'assistant-message';
    const errorClass = message.error ? 'error-message' : '';
    const loadingClass = message.isLoading ? 'loading-message' : '';

    return [baseClass, roleClass, errorClass, loadingClass]
      .filter(cls => cls)
      .join(' ');
  }

  getQuickActionsByCategory(): { [key: string]: QuickAction[] } {
    return this.quickActions.reduce((acc, action) => {
      const category = action.category;
      if (!acc[category]) {
        acc[category] = [];
      }
      acc[category].push(action);
      return acc;
    }, {} as { [key: string]: QuickAction[] });
  }

  getQuickActionCategoryLabel(category: string): string {
    switch (category) {
      case 'leave': return 'Leave Management';
      case 'team': return 'Team Management';
      case 'reports': return 'Reports';
      case 'admin': return 'Administration';
      case 'general': return 'General';
      default: return category;
    }
  }

  shouldShowQuickActions(): boolean {
    return this.showQuickActions &&
           this.messages.length === 0 &&
           this.quickActions.length > 0 &&
           !this.chatbotState.isLoading;
  }

  getRoleDisplayName(): string {
    if (!this.userContext) return '';
    return this.userContext.roles.join(', ');
  }

  getUserDisplayName(): string {
    if (!this.userContext) return '';
    return `${this.userContext.firstName} ${this.userContext.lastName}`;
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.messagesContainer) {
        const element = this.messagesContainer.nativeElement;
        element.scrollTop = element.scrollHeight;
      }
    }, 100);
  }

  private focusMessageInput(): void {
    setTimeout(() => {
      if (this.messageInput) {
        this.messageInput.nativeElement.focus();
      }
    }, 100);
  }

  // Helper methods for template
  isUserMessage(message: ChatMessage): boolean {
    return message.role === 'user';
  }

  isAssistantMessage(message: ChatMessage): boolean {
    return message.role === 'assistant';
  }

  hasError(message: ChatMessage): boolean {
    return !!message.error;
  }

  isLoading(message: ChatMessage): boolean {
    return !!message.isLoading;
  }

  hasFunctionCall(message: ChatMessage): boolean {
    return !!message.functionCall;
  }

  hasFunctionResponse(message: ChatMessage): boolean {
    return !!message.functionResponse;
  }

  getFunctionCallName(message: ChatMessage): string {
    return message.functionCall?.name || '';
  }

  getFunctionResponseName(message: ChatMessage): string {
    return message.functionResponse?.name || '';
  }

  formatFunctionResult(result: unknown): string {
    if (typeof result === 'string') {
      return result;
    }
    return JSON.stringify(result, null, 2);
  }

  retryLastMessage(): void {
    // Find the last user message and resend it
    for (let i = this.messages.length - 1; i >= 0; i--) {
      if (this.messages[i].role === 'user') {
        this.sendMessage();
        break;
      }
    }
  }

  showRetryButton(): boolean {
    // Show retry button if the last message is an error from assistant
    const lastMessage = this.messages[this.messages.length - 1];
    return lastMessage &&
           lastMessage.role === 'assistant' &&
           !!lastMessage.error &&
           !this.chatbotState.isLoading;
  }

  trackByMessage(index: number, message: ChatMessage): string {
    return message.id;
  }
}
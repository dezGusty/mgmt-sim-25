import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { of } from 'rxjs';

import { ChatbotComponent } from './chatbot.component';
import { ChatbotService } from '../../services/chatbot/chatbot.service';
import { ChatbotState, UserContext, ChatMessage } from '../../models/chatbot';

describe('ChatbotComponent', () => {
  let component: ChatbotComponent;
  let fixture: ComponentFixture<ChatbotComponent>;
  let mockChatbotService: jasmine.SpyObj<ChatbotService>;

  const mockUserContext: UserContext = {
    userId: 1,
    roles: ['Manager'],
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@example.com',
    isManager: true
  };

  const mockChatbotState: ChatbotState = {
    isInitialized: true,
    isLoading: false,
    userContext: mockUserContext
  };

  const mockMessages: ChatMessage[] = [
    {
      id: '1',
      content: 'Hello',
      role: 'user',
      timestamp: new Date()
    },
    {
      id: '2',
      content: 'Hello! How can I help you today?',
      role: 'assistant',
      timestamp: new Date()
    }
  ];

  beforeEach(async () => {
    const chatbotServiceSpy = jasmine.createSpyObj('ChatbotService', [
      'sendMessage',
      'clearChat',
      'getQuickActions',
      'getChatHistory',
      'isInitialized',
      'getUserContext'
    ], {
      chatState$: of(mockChatbotState),
      messages$: of(mockMessages)
    });

    await TestBed.configureTestingModule({
      imports: [ChatbotComponent, CommonModule, FormsModule],
      providers: [
        { provide: ChatbotService, useValue: chatbotServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ChatbotComponent);
    component = fixture.componentInstance;
    mockChatbotService = TestBed.inject(ChatbotService) as jasmine.SpyObj<ChatbotService>;

    // Setup return values
    mockChatbotService.getQuickActions.and.returnValue([]);
    mockChatbotService.getChatHistory.and.returnValue(mockMessages);
    mockChatbotService.isInitialized.and.returnValue(true);
    mockChatbotService.getUserContext.and.returnValue(mockUserContext);

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with correct default values', () => {
    expect(component.isOpen).toBeFalse();
    expect(component.isMinimized).toBeFalse();
    expect(component.currentMessage).toBe('');
    expect(component.showQuickActions).toBeTrue();
  });

  it('should toggle chatbot visibility', () => {
    expect(component.isOpen).toBeFalse();

    component.toggleChatbot();
    expect(component.isOpen).toBeTrue();
    expect(component.isMinimized).toBeFalse();

    component.toggleChatbot();
    expect(component.isOpen).toBeFalse();
  });

  it('should minimize and maximize chatbot', () => {
    component.isOpen = true;

    component.minimizeChatbot();
    expect(component.isMinimized).toBeTrue();

    component.maximizeChatbot();
    expect(component.isMinimized).toBeFalse();
  });

  it('should close chatbot', () => {
    component.isOpen = true;
    component.isMinimized = true;

    component.closeChatbot();
    expect(component.isOpen).toBeFalse();
    expect(component.isMinimized).toBeFalse();
  });

  it('should send message when content is provided', async () => {
    component.currentMessage = 'Hello';
    mockChatbotService.sendMessage.and.returnValue(Promise.resolve());

    await component.sendMessage();

    expect(mockChatbotService.sendMessage).toHaveBeenCalledWith('Hello');
    expect(component.currentMessage).toBe('');
    expect(component.showQuickActions).toBeFalse();
  });

  it('should not send empty message', async () => {
    component.currentMessage = '   ';

    await component.sendMessage();

    expect(mockChatbotService.sendMessage).not.toHaveBeenCalled();
  });

  it('should not send message when loading', async () => {
    component.currentMessage = 'Hello';
    component.chatbotState = { ...mockChatbotState, isLoading: true };

    await component.sendMessage();

    expect(mockChatbotService.sendMessage).not.toHaveBeenCalled();
  });

  it('should handle Enter key press to send message', () => {
    component.currentMessage = 'Hello';
    spyOn(component, 'sendMessage');

    const event = new KeyboardEvent('keypress', { key: 'Enter', shiftKey: false });
    spyOn(event, 'preventDefault');

    component.onKeyPress(event);

    expect(event.preventDefault).toHaveBeenCalled();
    expect(component.sendMessage).toHaveBeenCalled();
  });

  it('should not send message on Shift+Enter', () => {
    component.currentMessage = 'Hello';
    spyOn(component, 'sendMessage');

    const event = new KeyboardEvent('keypress', { key: 'Enter', shiftKey: true });
    spyOn(event, 'preventDefault');

    component.onKeyPress(event);

    expect(event.preventDefault).not.toHaveBeenCalled();
    expect(component.sendMessage).not.toHaveBeenCalled();
  });

  it('should clear chat', () => {
    component.clearChat();

    expect(mockChatbotService.clearChat).toHaveBeenCalled();
    expect(component.showQuickActions).toBeTrue();
  });

  it('should format message time correctly', () => {
    const message: ChatMessage = {
      id: '1',
      content: 'Test',
      role: 'user',
      timestamp: new Date('2023-01-01T12:30:00')
    };

    const timeString = component.getMessageTime(message);
    expect(timeString).toMatch(/\d{1,2}:\d{2}\s?(AM|PM)/i);
  });

  it('should identify user messages correctly', () => {
    const userMessage: ChatMessage = {
      id: '1',
      content: 'Test',
      role: 'user',
      timestamp: new Date()
    };

    const assistantMessage: ChatMessage = {
      id: '2',
      content: 'Test',
      role: 'assistant',
      timestamp: new Date()
    };

    expect(component.isUserMessage(userMessage)).toBeTrue();
    expect(component.isUserMessage(assistantMessage)).toBeFalse();
  });

  it('should identify assistant messages correctly', () => {
    const userMessage: ChatMessage = {
      id: '1',
      content: 'Test',
      role: 'user',
      timestamp: new Date()
    };

    const assistantMessage: ChatMessage = {
      id: '2',
      content: 'Test',
      role: 'assistant',
      timestamp: new Date()
    };

    expect(component.isAssistantMessage(assistantMessage)).toBeTrue();
    expect(component.isAssistantMessage(userMessage)).toBeFalse();
  });

  it('should detect error messages', () => {
    const normalMessage: ChatMessage = {
      id: '1',
      content: 'Test',
      role: 'assistant',
      timestamp: new Date()
    };

    const errorMessage: ChatMessage = {
      id: '2',
      content: 'Error occurred',
      role: 'assistant',
      timestamp: new Date(),
      error: 'Something went wrong'
    };

    expect(component.hasError(normalMessage)).toBeFalse();
    expect(component.hasError(errorMessage)).toBeTrue();
  });

  it('should detect loading messages', () => {
    const normalMessage: ChatMessage = {
      id: '1',
      content: 'Test',
      role: 'assistant',
      timestamp: new Date()
    };

    const loadingMessage: ChatMessage = {
      id: '2',
      content: 'Loading...',
      role: 'assistant',
      timestamp: new Date(),
      isLoading: true
    };

    expect(component.isLoading(normalMessage)).toBeFalse();
    expect(component.isLoading(loadingMessage)).toBeTrue();
  });

  it('should get user display name', () => {
    component.userContext = mockUserContext;

    const displayName = component.getUserDisplayName();
    expect(displayName).toBe('John Doe');
  });

  it('should get role display name', () => {
    component.userContext = mockUserContext;

    const roleDisplay = component.getRoleDisplayName();
    expect(roleDisplay).toBe('Manager');
  });

  it('should track messages by ID', () => {
    const message: ChatMessage = {
      id: 'test-id',
      content: 'Test',
      role: 'user',
      timestamp: new Date()
    };

    const result = component.trackByMessage(0, message);
    expect(result).toBe('test-id');
  });

  it('should show quick actions when appropriate', () => {
    component.showQuickActions = true;
    component.messages = [];
    component.quickActions = [
      {
        id: '1',
        label: 'Test',
        prompt: 'Test prompt',
        roles: ['Manager'],
        category: 'general'
      }
    ];
    component.chatbotState = { ...mockChatbotState, isLoading: false };

    expect(component.shouldShowQuickActions()).toBeTrue();
  });

  it('should not show quick actions when there are messages', () => {
    component.showQuickActions = true;
    component.messages = [mockMessages[0]];
    component.quickActions = [
      {
        id: '1',
        label: 'Test',
        prompt: 'Test prompt',
        roles: ['Manager'],
        category: 'general'
      }
    ];
    component.chatbotState = { ...mockChatbotState, isLoading: false };

    expect(component.shouldShowQuickActions()).toBeFalse();
  });
});
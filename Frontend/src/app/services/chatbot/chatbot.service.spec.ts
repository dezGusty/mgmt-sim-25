import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ChatbotService } from './chatbot.service';
import { UsersService } from '../users/users-service';
import { LeaveRequests } from '../leave-requests/leave-requests';
import { Auth } from '../authService/auth';

describe('ChatbotService', () => {
  let service: ChatbotService;
  let usersService: jasmine.SpyObj<UsersService>;
  let leaveRequestsService: jasmine.SpyObj<LeaveRequests>;
  let authService: jasmine.SpyObj<Auth>;

  beforeEach(() => {
    const usersServiceSpy = jasmine.createSpyObj('UsersService', ['getUserById', 'getTotalManagersCount', 'getTotalAdminsCount', 'getTotalUnassignedUsersCount']);
    const leaveRequestsServiceSpy = jasmine.createSpyObj('LeaveRequests', ['fetchByManagerPaginated', 'patchLeaveRequest', 'addLeaveRequest']);
    const authServiceSpy = jasmine.createSpyObj('Auth', ['getCurrentUser']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        ChatbotService,
        { provide: UsersService, useValue: usersServiceSpy },
        { provide: LeaveRequests, useValue: leaveRequestsServiceSpy },
        { provide: Auth, useValue: authServiceSpy }
      ]
    });

    service = TestBed.inject(ChatbotService);
    usersService = TestBed.inject(UsersService) as jasmine.SpyObj<UsersService>;
    leaveRequestsService = TestBed.inject(LeaveRequests) as jasmine.SpyObj<LeaveRequests>;
    authService = TestBed.inject(Auth) as jasmine.SpyObj<Auth>;
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize service correctly', () => {
    expect(service.isInitialized()).toBeDefined();
  });

  it('should get user context', () => {
    const context = service.getUserContext();
    expect(context).toBeDefined();
  });

  it('should generate quick actions based on user roles', () => {
    const quickActions = service.getQuickActions();
    expect(Array.isArray(quickActions)).toBeTruthy();
  });

  it('should clear chat history', () => {
    service.clearChat();
    const history = service.getChatHistory();
    expect(history).toEqual([]);
  });
});
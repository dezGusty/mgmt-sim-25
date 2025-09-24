import { Injectable } from '@angular/core';
import { GoogleGenAI, Chat, FunctionDeclaration, GenerateContentResponse } from '@google/genai';
import { BehaviorSubject, Observable, from, throwError } from 'rxjs';
import { map, switchMap, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ChatMessage,
  ChatSession,
  ChatbotFunctionDeclaration,
  FunctionExecutionContext,
  FunctionExecutionResult,
  UserContext,
  ChatbotState,
  QuickAction
} from '../../models/chatbot';
import { UsersService } from '../users/users-service';
import { LeaveRequests } from '../leave-requests/leave-requests';
import { LeaveRequestService } from '../leaveRequest/leaveRequest.service';
import { LeaveRequestTypeService } from '../leaveRequestType/leave-request-type-service';
import { HrService } from '../hr/hr.service';
import { Auth } from '../authService/auth';
import { RequestStatus } from '../../models/enums/RequestStatus';

@Injectable({
  providedIn: 'root'
})
export class ChatbotService {
  private genAI: GoogleGenAI;
  private currentChat: Chat | null = null;
  private functionDeclarations: ChatbotFunctionDeclaration[] = [];

  private chatStateSubject = new BehaviorSubject<ChatbotState>({
    isInitialized: false,
    isLoading: false
  });

  private messagesSubject = new BehaviorSubject<ChatMessage[]>([]);

  public chatState$ = this.chatStateSubject.asObservable();
  public messages$ = this.messagesSubject.asObservable();

  private currentSessionId: string = '';
  private userContext: UserContext | null = null;

  constructor(
    private usersService: UsersService,
    private leaveRequestsService: LeaveRequests,
    private leaveRequestService: LeaveRequestService,
    private leaveRequestTypeService: LeaveRequestTypeService,
    private hrService: HrService,
    private authService: Auth
  ) {
    this.genAI = new GoogleGenAI({ apiKey: environment.geminiApiKey });
    this.initializeService();
  }

  private async initializeService(): Promise<void> {
    try {
      await this.loadUserContext();
      this.setupFunctionDeclarations();
      this.initializeChat();

      this.chatStateSubject.next({
        ...this.chatStateSubject.value,
        isInitialized: true,
        userContext: this.userContext || undefined
      });
    } catch (error) {
      console.error('Failed to initialize chatbot service:', error);
      this.chatStateSubject.next({
        ...this.chatStateSubject.value,
        error: 'Failed to initialize chatbot service'
      });
    }
  }

  private async loadUserContext(): Promise<void> {
    try {
      const currentUser = this.authService.getCurrentUser();
      if (!currentUser) {
        throw new Error('No authenticated user found');
      }

      // Get full user details from UsersService
      const userId = parseInt(currentUser.userId);
      const userDetailsResponse = await this.usersService.getUserById(userId).toPromise();

      if (!userDetailsResponse?.success || !userDetailsResponse.data) {
        throw new Error('Failed to load user details');
      }

      const userDetails = userDetailsResponse.data;

      this.userContext = {
        userId: userDetails.id,
        roles: currentUser.roles || [],
        firstName: userDetails.firstName,
        lastName: userDetails.lastName,
        email: userDetails.email,
        departmentName: userDetails.departmentName,
        jobTitleName: userDetails.jobTitleName,
        isManager: currentUser.roles?.includes('Manager') || false,
        subordinateIds: userDetails.subordinatesIds
      };
    } catch (error) {
      console.error('Failed to load user context:', error);
      throw error;
    }
  }

  private setupFunctionDeclarations(): void {
    if (!this.userContext) return;

    this.functionDeclarations = [];

    // Common functions for all users
    this.addCommonFunctions();

    // Role-specific functions
    if (this.userContext.roles.includes('Manager')) {
      this.addManagerFunctions();
    }

    if (this.userContext.roles.includes('Employee')) {
      this.addEmployeeFunctions();
    }

    if (this.userContext.roles.includes('Admin')) {
      this.addAdminFunctions();
    }

    if (this.userContext.roles.includes('HR')) {
      this.addHRFunctions();
    }
  }

  private addCommonFunctions(): void {
    this.functionDeclarations.push({
      name: 'getUserInfo',
      description: 'Get information about a user by name or get current user info',
      parametersJsonSchema: {
        type: 'object',
        properties: {
          userName: {
            type: 'string',
            description: 'Name of the user to look up (optional, if not provided returns current user info)'
          }
        }
      },
      roles: ['Admin', 'Manager', 'HR', 'Employee'],
      handler: this.handleGetUserInfo.bind(this)
    });

    this.functionDeclarations.push({
      name: 'getLeaveRequestTypes',
      description: 'Get all available leave request types',
      parametersJsonSchema: {
        type: 'object',
        properties: {}
      },
      roles: ['Admin', 'Manager', 'HR', 'Employee'],
      handler: this.handleGetLeaveRequestTypes.bind(this)
    });
  }

  private addManagerFunctions(): void {
    this.functionDeclarations.push({
      name: 'getEmployeeVacationDays',
      description: 'Get remaining vacation days for a team member',
      parametersJsonSchema: {
        type: 'object',
        properties: {
          employeeName: {
            type: 'string',
            description: 'Name of the employee (first name, last name, or full name)'
          }
        },
        required: ['employeeName']
      },
      roles: ['Manager'],
      handler: this.handleGetEmployeeVacationDays.bind(this)
    });

    this.functionDeclarations.push({
      name: 'getTeamLeaveRequests',
      description: 'Get leave requests for team members',
      parametersJsonSchema: {
        type: 'object',
        properties: {
          status: {
            type: 'string',
            description: 'Filter by status: ALL, PENDING, APPROVED, DENIED',
            enum: ['ALL', 'PENDING', 'APPROVED', 'DENIED']
          },
          pageSize: {
            type: 'number',
            description: 'Number of requests to return (default: 10)'
          }
        }
      },
      roles: ['Manager'],
      handler: this.handleGetTeamLeaveRequests.bind(this)
    });

    this.functionDeclarations.push({
      name: 'approveLeaveRequest',
      description: 'Approve a leave request',
      parametersJsonSchema: {
        type: 'object',
        properties: {
          requestId: {
            type: 'number',
            description: 'ID of the leave request to approve'
          },
          comment: {
            type: 'string',
            description: 'Optional comment for the approval'
          }
        },
        required: ['requestId']
      },
      roles: ['Manager'],
      handler: this.handleApproveLeaveRequest.bind(this)
    });

    this.functionDeclarations.push({
      name: 'denyLeaveRequest',
      description: 'Deny a leave request',
      parametersJsonSchema: {
        type: 'object',
        properties: {
          requestId: {
            type: 'number',
            description: 'ID of the leave request to deny'
          },
          reason: {
            type: 'string',
            description: 'Reason for denying the request'
          }
        },
        required: ['requestId', 'reason']
      },
      roles: ['Manager'],
      handler: this.handleDenyLeaveRequest.bind(this)
    });
  }

  private addEmployeeFunctions(): void {
    this.functionDeclarations.push({
      name: 'createLeaveRequest',
      description: 'Create a new leave request',
      parametersJsonSchema: {
        type: 'object',
        properties: {
          startDate: {
            type: 'string',
            description: 'Start date of leave in YYYY-MM-DD format'
          },
          endDate: {
            type: 'string',
            description: 'End date of leave in YYYY-MM-DD format'
          },
          leaveType: {
            type: 'string',
            description: 'Type of leave (vacation, sick, personal, etc.)'
          },
          reason: {
            type: 'string',
            description: 'Reason for the leave request'
          }
        },
        required: ['startDate', 'endDate', 'leaveType']
      },
      roles: ['Employee'],
      handler: this.handleCreateLeaveRequest.bind(this)
    });

    this.functionDeclarations.push({
      name: 'getMyVacationDays',
      description: 'Get current user remaining vacation days',
      parametersJsonSchema: {
        type: 'object',
        properties: {}
      },
      roles: ['Employee'],
      handler: this.handleGetMyVacationDays.bind(this)
    });

    this.functionDeclarations.push({
      name: 'getMyLeaveRequests',
      description: 'Get current user leave request history',
      parametersJsonSchema: {
        type: 'object',
        properties: {
          status: {
            type: 'string',
            description: 'Filter by status: ALL, PENDING, APPROVED, DENIED',
            enum: ['ALL', 'PENDING', 'APPROVED', 'DENIED']
          }
        }
      },
      roles: ['Employee'],
      handler: this.handleGetMyLeaveRequests.bind(this)
    });

    this.functionDeclarations.push({
      name: 'cancelMyLeaveRequest',
      description: 'Cancel one of my own pending leave requests',
      parametersJsonSchema: {
        type: 'object',
        properties: {
          requestId: {
            type: 'number',
            description: 'The ID of the leave request to cancel'
          }
        },
        required: ['requestId']
      },
      roles: ['Employee'],
      handler: this.handleCancelMyLeaveRequest.bind(this)
    });
  }

  private addAdminFunctions(): void {
    this.functionDeclarations.push({
      name: 'getUserStatistics',
      description: 'Get user statistics and counts',
      parametersJsonSchema: {
        type: 'object',
        properties: {
          category: {
            type: 'string',
            description: 'Type of statistics: total, managers, admins, unassigned',
            enum: ['total', 'managers', 'admins', 'unassigned']
          }
        }
      },
      roles: ['Admin'],
      handler: this.handleGetUserStatistics.bind(this)
    });
  }

  private addHRFunctions(): void {
    // HR functions can be added here in the future
  }

  private initializeChat(): void {
    if (!this.userContext || this.functionDeclarations.length === 0) return;

    const availableFunctions = this.functionDeclarations
      .filter(func => func.roles.some(role => this.userContext!.roles.includes(role)))
      .map(func => ({
        name: func.name,
        description: func.description,
        parametersJsonSchema: func.parametersJsonSchema
      }));

    this.currentChat = this.genAI.chats.create({
      model: 'gemini-2.0-flash-001',
      config: {
        tools: availableFunctions.length > 0 ? [{ functionDeclarations: availableFunctions }] : undefined,
        systemInstruction: this.getSystemInstruction()
      }
    });

    this.currentSessionId = this.generateSessionId();
  }

  private getSystemInstruction(): string {
    const userName = `${this.userContext!.firstName} ${this.userContext!.lastName}`;
    const roles = this.userContext!.roles.join(', ');
    const currentDate = new Date();
    const currentYear = currentDate.getFullYear();
    const currentMonth = currentDate.getMonth() + 1; // getMonth() returns 0-11
    const currentDay = currentDate.getDate();
    const formattedDate = `${currentDay}/${currentMonth.toString().padStart(2, '0')}/${currentYear}`;

    return `You are an AI assistant for a management simulation application. You are helping ${userName} who has the role(s): ${roles}.

You can help with:
- Leave management (checking vacation days, creating/managing leave requests)
- Team management (for managers - viewing team information, approving requests)
- User information and statistics
- System navigation and assistance

Always be helpful, professional, and accurate. When performing actions, confirm what you've done and provide clear feedback. If you need clarification, ask specific questions.

IMPORTANT: When creating leave requests or dealing with dates:
- The current date is: ${formattedDate}
- The current year is: ${currentYear}
- If a user doesn't specify a year when requesting leave, automatically use the current year (${currentYear})
- If a user mentions "this year" or "current year", use ${currentYear}

Current user context:
- Name: ${userName}
- Email: ${this.userContext!.email}
- Department: ${this.userContext!.departmentName || 'Not assigned'}
- Job Title: ${this.userContext!.jobTitleName || 'Not assigned'}
- Roles: ${roles}`;
  }

  public async sendMessage(message: string): Promise<void> {
    if (!this.currentChat || !this.userContext) {
      throw new Error('Chatbot not initialized');
    }

    this.setLoading(true);

    const userMessage: ChatMessage = {
      id: this.generateMessageId(),
      content: message,
      role: 'user',
      timestamp: new Date()
    };

    this.addMessage(userMessage);

    try {
      const response = await this.currentChat.sendMessage({ message });
      await this.handleResponse(response);
    } catch (error) {
      console.error('Error sending message:', error);
      this.addMessage({
        id: this.generateMessageId(),
        content: 'Sorry, I encountered an error processing your request. Please try again.',
        role: 'assistant',
        timestamp: new Date(),
        error: error instanceof Error ? error.message : 'Unknown error'
      });
    } finally {
      this.setLoading(false);
    }
  }

  private async handleResponse(response: GenerateContentResponse): Promise<void> {
    if (response.functionCalls && response.functionCalls.length > 0) {
      // Handle function calls
      for (const functionCall of response.functionCalls) {
        await this.executeFunctionCall(functionCall);
      }
    } else if (response.text) {
      // Handle regular text response
      this.addMessage({
        id: this.generateMessageId(),
        content: response.text,
        role: 'assistant',
        timestamp: new Date()
      });
    }
  }

  private async executeFunctionCall(functionCall: any): Promise<void> {
    const functionDeclaration = this.functionDeclarations.find(f => f.name === functionCall.name);

    if (!functionDeclaration) {
      this.addMessage({
        id: this.generateMessageId(),
        content: `Unknown function: ${functionCall.name}`,
        role: 'assistant',
        timestamp: new Date(),
        error: 'Function not found'
      });
      return;
    }

    // Check if user has permission to execute this function
    if (!functionDeclaration.roles.some(role => this.userContext!.roles.includes(role))) {
      this.addMessage({
        id: this.generateMessageId(),
        content: 'You do not have permission to perform this action.',
        role: 'assistant',
        timestamp: new Date(),
        error: 'Insufficient permissions'
      });
      return;
    }

    try {
      const result = await functionDeclaration.handler(functionCall.args || {});

      // Send function response back to Gemini for natural language response
      const functionResponse = {
        name: functionCall.name,
        response: result
      };

      // Add loading message while processing function result
      const loadingMessage: ChatMessage = {
        id: this.generateMessageId(),
        content: 'Processing your request...',
        role: 'assistant',
        timestamp: new Date(),
        isLoading: true,
        functionCall: {
          name: functionCall.name,
          arguments: functionCall.args || {}
        }
      };

      this.addMessage(loadingMessage);

      // Send function response back to get natural language response
      const followUpResponse = await this.currentChat!.sendMessage({
        message: `Function ${functionCall.name} executed successfully with result: ${JSON.stringify(result)}`
      });

      // Remove loading message and add final response
      this.removeMessage(loadingMessage.id);

      if (followUpResponse.text) {
        this.addMessage({
          id: this.generateMessageId(),
          content: followUpResponse.text,
          role: 'assistant',
          timestamp: new Date(),
          functionResponse: {
            name: functionCall.name,
            result: result
          }
        });
      }

    } catch (error) {
      console.error(`Error executing function ${functionCall.name}:`, error);
      this.addMessage({
        id: this.generateMessageId(),
        content: `Error executing ${functionCall.name}: ${error instanceof Error ? error.message : 'Unknown error'}`,
        role: 'assistant',
        timestamp: new Date(),
        error: error instanceof Error ? error.message : 'Unknown error'
      });
    }
  }

  // Function handlers
  private async handleGetUserInfo(args: Record<string, unknown>): Promise<FunctionExecutionResult> {
    try {
      const userName = args['userName'] as string;

      if (!userName) {
        // Return current user info
        return {
          success: true,
          data: this.userContext,
          message: 'Current user information retrieved successfully'
        };
      }

      // Search for user by name using HR service
      const currentYear = new Date().getFullYear();
      const usersResponse = await this.hrService.getUsers(currentYear, 1, 100).toPromise();

      if (usersResponse?.data) {
        const matchingUsers = usersResponse.data.filter(user =>
          user.fullName.toLowerCase().includes(userName.toLowerCase()) ||
          user.firstName.toLowerCase().includes(userName.toLowerCase()) ||
          user.lastName.toLowerCase().includes(userName.toLowerCase()) ||
          user.email.toLowerCase().includes(userName.toLowerCase())
        );

        if (matchingUsers.length === 0) {
          return {
            success: false,
            error: `No users found matching "${userName}"`
          };
        }

        if (matchingUsers.length === 1) {
          const user = matchingUsers[0];
          return {
            success: true,
            data: {
              id: user.id,
              fullName: user.fullName,
              firstName: user.firstName,
              lastName: user.lastName,
              email: user.email,
              roles: user.roles,
              jobTitle: user.jobTitleName,
              department: user.departmentName,
              isActive: user.isActive,
              dateOfEmployment: user.dateOfEmployment,
              totalLeaveDays: user.totalLeaveDays,
              usedLeaveDays: user.usedLeaveDays,
              remainingLeaveDays: user.remainingLeaveDays
            },
            message: `Found user: ${user.fullName}`
          };
        }

        // Multiple matches found
        return {
          success: true,
          data: {
            matches: matchingUsers.map(user => ({
              id: user.id,
              fullName: user.fullName,
              email: user.email,
              jobTitle: user.jobTitleName,
              department: user.departmentName
            }))
          },
          message: `Found ${matchingUsers.length} users matching "${userName}"`
        };
      }

      return {
        success: false,
        error: 'Failed to retrieve user data'
      };
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  private async handleGetLeaveRequestTypes(args: Record<string, unknown>): Promise<FunctionExecutionResult> {
    try {
      const response = await this.leaveRequestTypeService.getAllLeaveRequestTypes().toPromise();

      if (response?.success && response.data) {
        const types = response.data.map(type => ({
          id: type.id,
          title: type.title,
          description: type.description,
          maxDays: type.maxDays,
          isPaid: type.isPaid
        }));

        return {
          success: true,
          data: types,
          message: `Found ${types.length} available leave request types`
        };
      } else {
        return {
          success: false,
          error: response?.message || 'Failed to fetch leave request types'
        };
      }
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  private async handleGetEmployeeVacationDays(args: Record<string, unknown>): Promise<FunctionExecutionResult> {
    try {
      const employeeName = args['employeeName'] as string;

      if (!employeeName) {
        return {
          success: false,
          error: 'Employee name is required'
        };
      }

      // First, search for the employee using HR service
      const currentYear = new Date().getFullYear();
      const usersResponse = await this.hrService.getUsers(currentYear, 1, 100).toPromise();

      if (!usersResponse?.data) {
        return {
          success: false,
          error: 'Failed to retrieve user data'
        };
      }

      // Find matching employees
      const matchingUsers = usersResponse.data.filter(user =>
        user.fullName.toLowerCase().includes(employeeName.toLowerCase()) ||
        user.firstName.toLowerCase().includes(employeeName.toLowerCase()) ||
        user.lastName.toLowerCase().includes(employeeName.toLowerCase())
      );

      if (matchingUsers.length === 0) {
        return {
          success: false,
          error: `No employees found matching "${employeeName}"`
        };
      }

      if (matchingUsers.length > 1) {
        return {
          success: false,
          error: `Multiple employees found matching "${employeeName}". Please be more specific.`,
          data: {
            matches: matchingUsers.map(user => ({
              fullName: user.fullName,
              email: user.email,
              department: user.departmentName
            }))
          }
        };
      }

      const employee = matchingUsers[0];

      // Check if the employee has vacation data already available from HR service
      if (employee.totalLeaveDays !== undefined && employee.remainingLeaveDays !== undefined) {
        return {
          success: true,
          data: {
            employee: {
              fullName: employee.fullName,
              email: employee.email,
              department: employee.departmentName,
              jobTitle: employee.jobTitleName
            },
            vacation: {
              totalLeaveDays: employee.totalLeaveDays,
              usedLeaveDays: employee.usedLeaveDays,
              remainingLeaveDays: employee.remainingLeaveDays,
              year: currentYear
            }
          },
          message: `${employee.fullName} has ${employee.remainingLeaveDays} remaining vacation days out of ${employee.totalLeaveDays} total days for ${currentYear}`
        };
      }

      // If vacation data is not available in the HR response, return basic info
      return {
        success: true,
        data: {
          employee: {
            fullName: employee.fullName,
            email: employee.email,
            department: employee.departmentName,
            jobTitle: employee.jobTitleName
          },
          vacation: null
        },
        message: `Found employee ${employee.fullName}, but vacation balance information is not available`
      };
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  private async handleGetTeamLeaveRequests(args: Record<string, unknown>): Promise<FunctionExecutionResult> {
    try {
      const status = (args['status'] as string) || 'ALL';
      const pageSize = (args['pageSize'] as number) || 10;

      const response = await this.leaveRequestsService.fetchByManagerPaginated(status, pageSize, 1).toPromise();

      if (response?.success) {
        return {
          success: true,
          data: response.data,
          message: `Retrieved ${response.data?.items?.length || 0} leave requests`
        };
      } else {
        return {
          success: false,
          error: response?.message || 'Failed to fetch team leave requests'
        };
      }
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  private async handleApproveLeaveRequest(args: Record<string, unknown>): Promise<FunctionExecutionResult> {
    try {
      const requestId = args['requestId'] as number;
      const comment = (args['comment'] as string) || 'Approved by manager via AI assistant';

      const response = await this.leaveRequestsService.patchLeaveRequest({
        id: requestId.toString(),
        requestStatus: 4, // Approved status
        reviewerComment: comment
      }).toPromise();

      if (response?.success) {
        return {
          success: true,
          data: response.data,
          message: `Leave request ${requestId} approved successfully`
        };
      } else {
        return {
          success: false,
          error: response?.message || 'Failed to approve leave request'
        };
      }
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  private async handleDenyLeaveRequest(args: Record<string, unknown>): Promise<FunctionExecutionResult> {
    try {
      const requestId = args['requestId'] as number;
      const reason = args['reason'] as string;

      const response = await this.leaveRequestsService.patchLeaveRequest({
        id: requestId.toString(),
        requestStatus: 3, // Denied status
        reviewerComment: reason
      }).toPromise();

      if (response?.success) {
        return {
          success: true,
          data: response.data,
          message: `Leave request ${requestId} denied successfully`
        };
      } else {
        return {
          success: false,
          error: response?.message || 'Failed to deny leave request'
        };
      }
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  private async handleCreateLeaveRequest(args: Record<string, unknown>): Promise<FunctionExecutionResult> {
    try {
      const startDate = args['startDate'] as string;
      const endDate = args['endDate'] as string;
      const leaveType = args['leaveType'] as string;
      const reason = (args['reason'] as string) || '';

      // Map leave type string to leave type ID
      const typesResponse = await this.leaveRequestTypeService.getAllLeaveRequestTypes().toPromise();

      if (!typesResponse?.success || !typesResponse.data) {
        return {
          success: false,
          error: 'Unable to retrieve leave request types'
        };
      }

      // Find matching leave type (case-insensitive)
      const matchingType = typesResponse.data.find(type =>
        type.title.toLowerCase().includes(leaveType.toLowerCase()) ||
        leaveType.toLowerCase().includes(type.title.toLowerCase())
      );

      if (!matchingType) {
        const availableTypes = typesResponse.data.map(type => type.title).join(', ');
        return {
          success: false,
          error: `Unknown leave type "${leaveType}". Available types: ${availableTypes}`
        };
      }

      const leaveRequestTypeId = matchingType.id;

      // Use employee-specific service to create request as pending
      const requestData = {
        leaveRequestTypeId: leaveRequestTypeId,
        startDate: startDate,
        endDate: endDate,
        reason: reason
      };

      const response = await this.leaveRequestService.addLeaveRequestByEmployee(requestData).toPromise();

      if (response?.success) {
        return {
          success: true,
          data: response.data,
          message: `Leave request created successfully for ${startDate} to ${endDate}. Status: Pending approval.`
        };
      } else {
        return {
          success: false,
          error: response?.message || 'Failed to create leave request'
        };
      }
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  private async handleGetMyVacationDays(args: Record<string, unknown>): Promise<FunctionExecutionResult> {
    try {
      // Get all leave request types first
      const typesResponse = await this.leaveRequestTypeService.getAllLeaveRequestTypes().toPromise();

      if (!typesResponse?.success || !typesResponse.data) {
        return {
          success: false,
          error: 'Unable to retrieve leave request types'
        };
      }

      const currentYear = new Date().getFullYear();
      const vacationBalances = [];

      // Get vacation balance for each leave type
      for (const leaveType of typesResponse.data) {
        try {
          const balanceResponse = await this.leaveRequestService.getCurrentUserRemainingLeaveDays(leaveType.id, currentYear).toPromise();

          if (balanceResponse?.success && balanceResponse.data) {
            vacationBalances.push({
              leaveType: leaveType.title,
              maxDaysAllowed: balanceResponse.data.maxDaysAllowed || leaveType.maxDays,
              usedDays: balanceResponse.data.usedDays || 0,
              remainingDays: balanceResponse.data.remainingDays,
              year: currentYear
            });
          }
        } catch (leaveTypeError) {
          // Continue with other types if one fails
          console.warn(`Failed to get balance for ${leaveType.title}:`, leaveTypeError);
        }
      }

      if (vacationBalances.length === 0) {
        return {
          success: false,
          error: 'No vacation balance information available'
        };
      }

      // Format the response with comprehensive information
      const totalRemaining = vacationBalances.reduce((sum, balance) =>
        sum + (balance.remainingDays || 0), 0);
      const totalUsed = vacationBalances.reduce((sum, balance) =>
        sum + balance.usedDays, 0);

      return {
        success: true,
        data: {
          summary: {
            totalRemainingDays: totalRemaining,
            totalUsedDays: totalUsed,
            year: currentYear
          },
          details: vacationBalances
        },
        message: `You have ${totalRemaining} total remaining vacation days this year (${totalUsed} used)`
      };
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Failed to retrieve vacation balance'
      };
    }
  }

  private async handleGetMyLeaveRequests(args: Record<string, unknown>): Promise<FunctionExecutionResult> {
    try {
      const status = (args['status'] as string) || 'ALL';

      // Use the appropriate method based on status filter
      let response;
      if (status === 'ALL') {
        response = await this.leaveRequestService.getCurrentUserLeaveRequests().toPromise();
      } else {
        response = await this.leaveRequestService.getCurrentUserLeaveRequestsPaginated(status, 50, 1).toPromise();
      }

      if (response?.success) {
        let requests: any[];
        if (Array.isArray(response.data)) {
          requests = response.data;
        } else if (response.data?.items) {
          requests = response.data.items;
        } else {
          requests = [];
        }

        // Get leave request types for better formatting
        const typesResponse = await this.leaveRequestTypeService.getAllLeaveRequestTypes().toPromise();
        const leaveTypes = typesResponse?.success ? typesResponse.data : [];

        // Format the requests with additional information
        const formattedRequests = requests.map((request: any) => {
          const leaveType = leaveTypes.find(type => type.id === request.leaveRequestTypeId);
          return {
            id: request.id,
            leaveType: leaveType?.title || 'Unknown',
            startDate: request.startDate,
            endDate: request.endDate,
            duration: request.duration,
            reason: request.reason,
            status: this.getRequestStatusName(request.requestStatus),
            reviewerComment: request.reviewerComment,
            submitDate: request.submitDate || 'N/A'
          };
        });

        const totalRequests = Array.isArray(response.data) ? requests.length : response.data?.totalCount || requests.length;

        return {
          success: true,
          data: {
            requests: formattedRequests,
            totalCount: totalRequests,
            filter: status
          },
          message: `Found ${formattedRequests.length} leave ${formattedRequests.length === 1 ? 'request' : 'requests'}${status !== 'ALL' ? ` with status: ${status}` : ''}`
        };
      } else {
        return {
          success: false,
          error: response?.message || 'Failed to fetch your leave requests'
        };
      }
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Failed to retrieve leave requests'
      };
    }
  }

  private async handleCancelMyLeaveRequest(args: Record<string, unknown>): Promise<FunctionExecutionResult> {
    try {
      const requestId = args['requestId'] as number;

      if (!requestId) {
        return {
          success: false,
          error: 'Request ID is required'
        };
      }

      // First, get the user's leave requests to verify ownership and status
      const userRequestsResponse = await this.leaveRequestService.getCurrentUserLeaveRequests().toPromise();

      if (!userRequestsResponse?.success) {
        return {
          success: false,
          error: 'Failed to retrieve your leave requests for verification'
        };
      }

      // Find the specific request
      const targetRequest = userRequestsResponse.data?.find((request: any) => request.id === requestId);

      if (!targetRequest) {
        return {
          success: false,
          error: 'Leave request not found. Please check the request ID.'
        };
      }

      // Check if the request is pending (only pending requests can be cancelled)
      if (targetRequest.requestStatus !== RequestStatus.PENDING) {
        const statusName = this.getRequestStatusName(targetRequest.requestStatus);
        return {
          success: false,
          error: `Cannot cancel a ${statusName.toLowerCase()} request. Only pending requests can be cancelled.`
        };
      }

      // Cancel the request
      const cancelResponse = await this.leaveRequestService.cancelLeaveRequestByEmployee(requestId).toPromise();

      return {
        success: true,
        data: {
          cancelledRequestId: requestId,
          startDate: targetRequest.startDate,
          endDate: targetRequest.endDate,
          leaveType: targetRequest.leaveRequestTypeId
        },
        message: `Leave request #${requestId} has been successfully cancelled. The request for ${targetRequest.startDate} to ${targetRequest.endDate} is now cancelled.`
      };

    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Failed to cancel leave request'
      };
    }
  }

  private async handleGetUserStatistics(args: Record<string, unknown>): Promise<FunctionExecutionResult> {
    try {
      const category = args['category'] as string;

      let result;
      switch (category) {
        case 'managers':
          result = await this.usersService.getTotalManagersCount().toPromise();
          break;
        case 'admins':
          result = await this.usersService.getTotalAdminsCount().toPromise();
          break;
        case 'unassigned':
          result = await this.usersService.getTotalUnassignedUsersCount().toPromise();
          break;
        default:
          return {
            success: false,
            error: 'Unknown statistics category'
          };
      }

      if (result?.success) {
        return {
          success: true,
          data: { category, count: result.data },
          message: `${category} count: ${result.data}`
        };
      } else {
        return {
          success: false,
          error: result?.message || 'Failed to fetch statistics'
        };
      }
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  // Utility methods
  private addMessage(message: ChatMessage): void {
    const currentMessages = this.messagesSubject.value;
    this.messagesSubject.next([...currentMessages, message]);
  }

  private removeMessage(messageId: string): void {
    const currentMessages = this.messagesSubject.value;
    this.messagesSubject.next(currentMessages.filter(m => m.id !== messageId));
  }

  private setLoading(isLoading: boolean): void {
    this.chatStateSubject.next({
      ...this.chatStateSubject.value,
      isLoading
    });
  }

  private generateSessionId(): string {
    return `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  private generateMessageId(): string {
    return `msg_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  // Public methods for component interaction
  public clearChat(): void {
    this.messagesSubject.next([]);
    this.initializeChat();
  }

  public getChatHistory(): ChatMessage[] {
    return this.messagesSubject.value;
  }

  public getQuickActions(): QuickAction[] {
    if (!this.userContext) return [];

    const actions: QuickAction[] = [];

    // Common quick actions
    actions.push({
      id: 'check-vacation',
      label: 'Check my vacation days',
      prompt: 'How many vacation days do I have left?',
      category: 'leave',
      roles: ['Employee']
    });

    // Manager quick actions
    if (this.userContext.roles.includes('Manager')) {
      actions.push(
        {
          id: 'team-requests',
          label: 'View team requests',
          prompt: 'Show me pending leave requests from my team',
          category: 'team',
          roles: ['Manager']
        },
        {
          id: 'team-vacation',
          label: 'Check team vacation',
          prompt: 'Show me vacation balances for my team members',
          category: 'team',
          roles: ['Manager']
        }
      );
    }

    // Filter by user roles
    return actions.filter(action =>
      action.roles.some(role => this.userContext!.roles.includes(role))
    );
  }

  public isInitialized(): boolean {
    return this.chatStateSubject.value.isInitialized;
  }

  public getUserContext(): UserContext | null {
    return this.userContext;
  }

  private getRequestStatusName(status: number): string {
    switch (status) {
      case RequestStatus.INVALID_REQUEST_STATUS: return 'Invalid';
      case RequestStatus.ARRIVED: return 'Arrived';
      case RequestStatus.PENDING: return 'Pending';
      case RequestStatus.APPROVED: return 'Approved';
      case RequestStatus.REJECTED: return 'Rejected';
      case RequestStatus.EXPIRED: return 'Expired';
      case RequestStatus.CANCELED: return 'Cancelled';
      default: return 'Unknown';
    }
  }
}
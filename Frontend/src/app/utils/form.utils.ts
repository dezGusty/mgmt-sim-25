export class FormUtils {
  static validateLeaveRequestForm(
    userId: number | null,
    leaveRequestTypeId: number | null,
    startDate: string,
    endDate: string,
    reason: string
  ): boolean {
    return !!(userId && leaveRequestTypeId && startDate && endDate && reason);
  }

  static getTodayDateString(): string {
    return new Date().toISOString().slice(0, 10);
  }
}

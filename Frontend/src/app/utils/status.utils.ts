export class StatusUtils {
  static mapStatus(status: number): 'Pending' | 'Approved' | 'Rejected' {
    switch (status) {
      case 2:
        return 'Pending';
      case 4:
        return 'Approved';
      case 8:
        return 'Rejected';
      default:
        return 'Pending';
    }
  }
}

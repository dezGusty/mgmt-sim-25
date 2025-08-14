export class StatusUtils {
  static mapStatus(
    status: number
  ): 'Pending' | 'Approved' | 'Rejected' | 'Invalid' | 'Expired' | 'Canceled' | 'Arrived' | undefined {
    switch (status) {
      case 0:
        return 'Invalid';
      case 1:
        return 'Arrived';
      case 2:
        return 'Pending';
      case 4:
        return 'Approved';
      case 8:
        return 'Rejected';
      case 16:
        return 'Expired';
      case 32:
        return 'Canceled';
      default:
        return undefined;
    }
  }
}

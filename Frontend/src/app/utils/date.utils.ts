export class DateUtils {
  static formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleString('en-GB', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  static calcDays(start: string, end: string): number {
    const d1 = new Date(start);
    const d2 = new Date(end);
    return Math.max(
      1,
      Math.ceil((d2.getTime() - d1.getTime()) / (1000 * 60 * 60 * 24)) + 1
    );
  }
}

export class FormUtils {
  static getTodayDateString(): string {
    return new Date().toISOString().slice(0, 10);
  }
}

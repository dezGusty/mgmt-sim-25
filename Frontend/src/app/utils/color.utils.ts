export class ColorUtils {
  private static leaveTypeColors: Map<string, string> = new Map();

  private static readonly PREDEFINED_COLORS = [
    '#3B82F6', // Blue
    '#10B981', // Emerald
    '#F59E0B', // Amber
    '#EF4444', // Red
    '#8B5CF6', // Violet
    '#06B6D4', // Cyan
    '#84CC16', // Lime
    '#F97316', // Orange
    '#EC4899', // Pink
    '#6366F1', // Indigo
    '#14B8A6', // Teal
    '#F472B6', // Pink-400
    '#A855F7', // Purple
    '#22C55E', // Green
    '#FB7185', // Rose
    '#38BDF8', // Sky
    '#FACC15', // Yellow
    '#F87171', // Red-400
    '#60A5FA', // Blue-400
  ];

  static generateColorForLeaveType(leaveType: string): string {
    if (this.leaveTypeColors.has(leaveType)) {
      return this.leaveTypeColors.get(leaveType)!;
    }

    const existingColorsCount = Array.from(this.leaveTypeColors.keys()).length;
    let color: string;

    if (existingColorsCount < this.PREDEFINED_COLORS.length) {
      color = this.PREDEFINED_COLORS[existingColorsCount];
    } else {
      color = this.generateRandomColor();
    }

    this.leaveTypeColors.set(leaveType, color);
    return color;
  }

  private static generateRandomColor(): string {
    const hue = Math.floor(Math.random() * 360);
    const saturation = 60 + Math.floor(Math.random() * 30);
    const lightness = 45 + Math.floor(Math.random() * 20);

    return this.hslToHex(hue, saturation, lightness);
  }

  private static hslToHex(h: number, s: number, l: number): string {
    l /= 100;
    const a = (s * Math.min(l, 1 - l)) / 100;
    const f = (n: number) => {
      const k = (n + h / 30) % 12;
      const color = l - a * Math.max(Math.min(k - 3, 9 - k, 1), -1);
      return Math.round(255 * color)
        .toString(16)
        .padStart(2, '0');
    };
    return `#${f(0)}${f(8)}${f(4)}`;
  }

  static getAllLeaveTypeColors(): { type: string; color: string }[] {
    return Array.from(this.leaveTypeColors.entries()).map(([type, color]) => ({
      type,
      color,
    }));
  }

  static resetColors(): void {
    this.leaveTypeColors.clear();
  }
}

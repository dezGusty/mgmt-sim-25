import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ColorGenerator {
  private stringToHash(str: string): number {
    let hash = 0;
    const salt = 31; // Prime number pentru mai multă variație
    
    for (let i = 0; i < str.length; i++) {
      const char = str.charCodeAt(i);
      hash = (hash * salt + char) % 1000000; // Limitează overflow-ul
    }
    
    // Aplică o funcție de mixing pentru mai multă variație
    hash = hash ^ (hash >>> 16);
    hash = hash * 0x85ebca6b;
    hash = hash ^ (hash >>> 13);
    hash = hash * 0xc2b2ae35;
    hash = hash ^ (hash >>> 16);
    
    return Math.abs(hash);
  }

  private hslToHex(h: number, s: number, l: number): string {
    l /= 100;
    const a = s * Math.min(l, 1 - l) / 100;
    const f = (n: number) => {
      const k = (n + h / 30) % 12;
      const color = l - a * Math.max(Math.min(k - 3, 9 - k, 1), -1);
      return Math.round(255 * color).toString(16).padStart(2, '0');
    };
    return `#${f(0)}${f(8)}${f(4)}`;
  }

  generateColorFromId(id: string | number, saturation: number = 65, lightness: number = 55): string {
    const hash = this.stringToHash(id.toString());
    const hue = hash % 360;
    return this.hslToHex(hue, saturation, lightness);
  }
}
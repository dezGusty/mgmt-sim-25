import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Pipe({
  name: 'highlight',
  standalone: true
})
export class HighlightPipe implements PipeTransform {
  constructor(private sanitizer: DomSanitizer) {}

  transform(text: string, searchTerm: string, type: 'name' | 'email' | 'jobTitle' = 'name'): SafeHtml {
    if (!text || !searchTerm) {
      return text;
    }

    // Escape special regex characters in search term
    const escapedSearchTerm = searchTerm.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    
    // Create regex for case-insensitive search
    const regex = new RegExp(`(${escapedSearchTerm})`, 'gi');
    
    // Different styles based on type
    const styleClasses = {
      name: 'bg-blue-100 text-blue-800 font-medium px-1 rounded',
      email: 'bg-green-100 text-green-800 font-medium px-1 rounded',
      jobTitle: 'bg-pink-100 text-pink-700 font-medium px-1 rounded'
    };
    
    // Replace matches with highlighted version
    const highlightedText = text.replace(
      regex, 
      `<mark class="${styleClasses[type]}">$1</mark>`
    );
    
    return this.sanitizer.bypassSecurityTrustHtml(highlightedText);
  }
}

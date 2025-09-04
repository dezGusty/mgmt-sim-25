import { Component, Input } from '@angular/core';

@Component({
	selector: 'animated-background',
	standalone: true,
	templateUrl: './animated-background.html',
	styleUrls: ['./animated-background.css']
})
export class AnimatedBackground {
	@Input() theme: 'default' | 'login' = 'default';
} 
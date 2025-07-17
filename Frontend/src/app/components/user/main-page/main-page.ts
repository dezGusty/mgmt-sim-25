import { Component, signal } from '@angular/core';


@Component({
  selector: 'app-user-main-page',
  imports:[],
  templateUrl: './main-page.html',
  styleUrl: './main-page.css',
})
export class UserMainPage {
  showAddRequestForm = false;
}
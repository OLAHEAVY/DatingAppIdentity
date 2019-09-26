import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  registerMode = false;
  values: any;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.getValues();
  }

  // method to toggle the register button
  registerToggle() {
    this.registerMode = true;
  }
    // method to get values from the database for the parent child communication
  getValues() {
    this.http.get('http://localhost:5000/api/values').subscribe(response => {
      this.values = response;
    }, error => {
      console.log(error);
    });
  }

  // child to parent communication from the register component
  cancelRegisterMode(registerMode: boolean) {
    this.registerMode = registerMode;
  }

}

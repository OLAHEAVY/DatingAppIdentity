import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap';
import { User } from 'src/app/_models/user';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent implements OnInit {
  // passing data out of the component
  @Output() updateSelectedRoles = new EventEmitter();
  // modal starts here
  user: User;
  roles: any[];
 constructor(public bsModalRef: BsModalRef) {}

  ngOnInit() {
  }
// passing data out of the component
  updateRoles() {
    this.updateSelectedRoles.emit(this.roles);
    this.bsModalRef.hide();
  }

}

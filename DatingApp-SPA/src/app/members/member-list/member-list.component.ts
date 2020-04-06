import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/user';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { Pagination, PaginatedResult } from 'src/app/_models/pagination';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  users: User[];
  // pagination
  pagination: Pagination;
  // filtering
  user: User = JSON.parse(localStorage.getItem('user'));
  genderList = [{value: 'male', display: 'Male'}, {value: 'female', display: 'Females'}];
  userParams: any = {};

  constructor(private userService: UserService, private alertify: AlertifyService,  private route: ActivatedRoute) { }

  ngOnInit() {
    // ordinary list of users
    // this.route.data.subscribe(data => {
    //   this.users = data['users'];
    // });

    // Paginated list
    this.route.data.subscribe(data => {
      this.users = data['users'].result;
      this.pagination = data['users'].pagination;
    });

    // filtering
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.orderBy = 'lastActive';
  }

  // pagination
  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    this.loadUsers();
  }

  // filtering
  resetFilters() {
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.loadUsers();
  }

  // loadUsers() {
  //   this.userService.getUsers().subscribe((users: User[]) => {
  //     this.users = users;
  //   }, error => {
  //     this.alertify.error(error);
  //   });
  // }

  loadUsers() {
  this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, this.userParams)
  .subscribe((res: PaginatedResult< User[]>) => {
  this.users = res.result;
  this.pagination = res.pagination;
  }, error => {
  this.alertify.error(error);
  });
  }
}
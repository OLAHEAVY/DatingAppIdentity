import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { ToastrService } from 'ngx-toastr';
import {
  FormGroup,
  FormControl,
  Validators,
  FormBuilder
} from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // child to parent communication
  @Output() cancelRegister = new EventEmitter();
  user: User;
  // reactive forms
  registerForm: FormGroup;
  // spinner
  public loading = false;
  // date picker
  bsConfig: Partial<BsDatepickerConfig>;

  constructor(
    private authService: AuthService,
    private alertify: AlertifyService,
    private toastr: ToastrService,
    private fb: FormBuilder,
    private router: Router
  ) {}

  ngOnInit() {
    // BsDatePicker
    this.bsConfig = {
      containerClass: 'theme-red'
    },
    // reactive forms
   this.createRegisterForm();
  }
  // reactive forms
  createRegisterForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(8)
      ]],
      confirmPassword: ['', Validators.required]
    }, {validator: this.passwordMatchValidator});
  }

  // custom validator for the password fields
  passwordMatchValidator(g: FormGroup) {
    return g.get('password').value === g.get('confirmPassword').value
      ? null
      : { mismatch: true };
  }

  register() {
    if (this.registerForm.valid) {
      this.loading = true;
      // assigning values in the register form to the user object
      this.user = Object.assign({}, this.registerForm.value);
      this.authService.register(this.user).subscribe(() => {
      this.alertify.success('Registration Successful');
      this.toastr.info('Registration Successful', 'Information');
      }, error => {
        this.alertify.error(error);
      }, () => {
        // the user is logged in
        this.authService.login(this.user).subscribe(() => {
          this.router.navigate(['/members']);
        });
        this.loading = false;
      });

    }
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}

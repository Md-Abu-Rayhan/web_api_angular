import { NgClass, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { AuthService } from '../../../../core/services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-sign-in',
  templateUrl: './sign-in.component.html',
  styleUrls: ['./sign-in.component.css'],
  standalone: true,
  imports: [
    FormsModule, 
    ReactiveFormsModule, 
    RouterLink, 
    AngularSvgIconModule, 
    NgIf, 
    ButtonComponent, 
    NgClass
  ]
})
export class SignInComponent implements OnInit {
  form!: FormGroup;
  submitted = false;
  passwordTextType = false;
  isLoading = false;
  returnUrl = '/dashboard';

  constructor(
    private readonly _formBuilder: FormBuilder,
    private readonly _router: Router,
    private authService: AuthService,
    private toastr: ToastrService
  ) {}

  onClick() {
    console.log('Button clicked');
  }

  ngOnInit(): void {
    this.form = this._formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  get f() {
    return this.form.controls;
  }

  togglePasswordTextType() {
    this.passwordTextType = !this.passwordTextType;
  }

  onSubmit() {
    this.submitted = true;
    
    if (this.form.invalid) return;

    this.isLoading = true;
    
    this.authService.login(this.form.value).subscribe({
      next: () => {
        this._router.navigateByUrl(this.returnUrl);
        this.isLoading = false;
      },
      error: (error) => {
        this.isLoading = false;
        this.toastr.error(error.message || 'Authentication failed', 'Error', {
          timeOut: 5000,
        });
        this.form.reset();
      }
    });
  }
}

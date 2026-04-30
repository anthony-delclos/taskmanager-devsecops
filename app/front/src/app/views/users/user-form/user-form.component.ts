import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserService } from '../../../core/services/user.service';
import { UpdateUserRequest } from '../../../core/models/user.model';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.css'
})
export class UserFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  protected vm = inject(UserService);

  isEdit = signal(false);
  userId = signal<string | null>(null);
  submitting = signal(false);
  errorMsg = signal<string | null>(null);

  form = this.fb.group({
    username: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.minLength(8), Validators.maxLength(100)]],
    isAdmin: [false]
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      this.userId.set(id);
      // En mode édition le mot de passe est optionnel
      this.form.get('password')?.clearValidators();
      this.form.get('password')?.updateValueAndValidity();

      this.vm.getById(id).subscribe({
        next: (user) => this.form.patchValue({
          username: user.username,
          email: user.email,
          isAdmin: user.isAdmin
        }),
        error: () => this.errorMsg.set('Utilisateur introuvable')
      });
    } else {
      // En mode création le mot de passe est obligatoire
      this.form.get('password')?.setValidators([
        Validators.required,
        Validators.minLength(8),
        Validators.maxLength(100)
      ]);
      this.form.get('password')?.updateValueAndValidity();
    }
  }

  submit(): void {
    if (this.form.invalid) return;
    this.submitting.set(true);
    this.errorMsg.set(null);

    const { username, email, password, isAdmin } = this.form.value;

    if (this.isEdit()) {
      const req: UpdateUserRequest = { username: username!, email: email!, isAdmin: isAdmin ?? false };
      if (password) req.password = password;
      this.vm.update(this.userId()!, req).subscribe({
        next: () => this.router.navigate(['/users']),
        error: () => {
          this.errorMsg.set('Erreur lors de la mise à jour');
          this.submitting.set(false);
        }
      });
    } else {
      this.vm.create({ username: username!, email: email!, password: password!, isAdmin: isAdmin ?? false }).subscribe({
        next: () => this.router.navigate(['/users']),
        error: () => {
          this.errorMsg.set('Erreur lors de la création');
          this.submitting.set(false);
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/users']);
  }
}

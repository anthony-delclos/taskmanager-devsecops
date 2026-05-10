import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { User, CreateUserRequest, UpdateUserRequest } from '../models/user.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/users`;

  // ViewModel state (M → VM binding via signals)
  users = signal<User[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  // Derived state
  adminCount = computed(() => this.users().filter(u => u.isAdmin).length);

  loadAll(): void {
    this.loading.set(true);
    this.error.set(null);
    this.http.get<User[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.users.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Erreur lors du chargement des utilisateurs');
        this.loading.set(false);
      }
    });
  }

  getById(id: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateUserRequest): Observable<User> {
    return this.http.post<User>(this.apiUrl, request).pipe(
      tap(user => this.users.update(users => [user, ...users]))
    );
  }

  update(id: string, request: UpdateUserRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request).pipe(
      tap(() => this.loadAll())
    );
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.users.update(users => users.filter(u => u.id !== id)))
    );
  }
}

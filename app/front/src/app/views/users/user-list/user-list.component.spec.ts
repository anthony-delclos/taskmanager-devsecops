import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal, computed } from '@angular/core';
import { of } from 'rxjs';
import { UserListComponent } from './user-list.component';
import { UserService } from '../../../core/services/user.service';
import { User } from '../../../core/models/user.model';

const makeUser = (overrides: Partial<User> = {}): User => ({
  id: 'aaa-111',
  username: 'alice',
  email: 'alice@test.com',
  isAdmin: false,
  creationDate: '2026-01-01T00:00:00Z',
  status: null,
  ...overrides
});

describe('UserListComponent', () => {
  let fixture: ComponentFixture<UserListComponent>;
  let compiled: HTMLElement;
  let mockVm: Partial<UserService>;

  const buildMock = (users: User[] = [], loading = false, error: string | null = null) => ({
    users: signal(users),
    loading: signal(loading),
    error: signal(error),
    adminCount: computed(() => users.filter(u => u.isAdmin).length),
    loadAll: vi.fn(),
    remove: vi.fn().mockReturnValue(of(undefined))
  });

  async function setup(users: User[] = [], loading = false, error: string | null = null) {
    mockVm = buildMock(users, loading, error);

    await TestBed.configureTestingModule({
      imports: [UserListComponent],
      providers: [
        { provide: UserService, useValue: mockVm },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserListComponent);
    compiled = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
  }

  it('should create', async () => {
    await setup();
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should call loadAll on init', async () => {
    await setup();
    expect(mockVm.loadAll).toHaveBeenCalledOnce();
  });

  it('should show loading indicator when loading=true', async () => {
    await setup([], true);
    expect(compiled.textContent).toContain('Chargement');
  });

  it('should show error message when error is set', async () => {
    await setup([], false, 'Erreur réseau');
    expect(compiled.textContent).toContain('Erreur réseau');
  });

  it('should render a row for each user', async () => {
    const users = [makeUser({ id: '1' }), makeUser({ id: '2', username: 'bob' })];
    await setup(users);
    const rows = compiled.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
  });

  it('should display username and email in each row', async () => {
    await setup([makeUser()]);
    expect(compiled.textContent).toContain('alice');
    expect(compiled.textContent).toContain('alice@test.com');
  });

  it('should show "Admin" badge for admin users', async () => {
    await setup([makeUser({ isAdmin: true })]);
    expect(compiled.textContent).toContain('Admin');
  });

  it('should call vm.remove when delete is confirmed', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    await setup([makeUser({ id: 'del-123' })]);

    const btn = compiled.querySelector<HTMLButtonElement>('.btn-danger');
    btn?.click();

    expect(mockVm.remove).toHaveBeenCalledWith('del-123');
  });

  it('should NOT call vm.remove when delete is cancelled', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(false);
    await setup([makeUser({ id: 'del-123' })]);

    compiled.querySelector<HTMLButtonElement>('.btn-danger')?.click();

    expect(mockVm.remove).not.toHaveBeenCalled();
  });
});

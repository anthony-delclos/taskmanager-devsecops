import { TestBed } from '@angular/core/testing';
import { ComponentFixture } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Router } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { signal, computed } from '@angular/core';
import { of } from 'rxjs';
import { UserFormComponent } from './user-form.component';
import { UserService } from '../../../core/services/user.service';
import { User } from '../../../core/models/user.model';

const ALICE: User = {
  id: 'aaa-111',
  username: 'alice',
  email: 'alice@test.com',
  isAdmin: true,
  creationDate: '2026-01-01T00:00:00Z',
  status: null
};

function makeMockVm(overrides: Record<string, unknown> = {}) {
  return {
    users: signal<User[]>([]),
    loading: signal(false),
    error: signal<string | null>(null),
    adminCount: computed(() => 0),
    loadAll: vi.fn(),
    getById: vi.fn().mockReturnValue(of(ALICE)),
    create: vi.fn().mockReturnValue(of(ALICE)),
    update: vi.fn().mockReturnValue(of(undefined)),
    remove: vi.fn().mockReturnValue(of(undefined)),
    ...overrides
  };
}

function makeActivatedRoute(id: string | null) {
  return {
    snapshot: { paramMap: { get: vi.fn().mockReturnValue(id) } }
  };
}

describe('UserFormComponent – create mode', () => {
  let fixture: ComponentFixture<UserFormComponent>;
  let component: UserFormComponent;
  let compiled: HTMLElement;
  let mockVm: ReturnType<typeof makeMockVm>;
  let router: Router;

  beforeEach(async () => {
    mockVm = makeMockVm();

    await TestBed.configureTestingModule({
      imports: [UserFormComponent],
      providers: [
        { provide: UserService, useValue: mockVm },
        { provide: ActivatedRoute, useValue: makeActivatedRoute(null) },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserFormComponent);
    component = fixture.componentInstance;
    compiled = fixture.nativeElement as HTMLElement;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => expect(component).toBeTruthy());

  it('isEdit should be false in create mode', () => {
    expect(component.isEdit()).toBe(false);
  });

  it('should make password required in create mode', () => {
    const ctrl = component.form.get('password')!;
    ctrl.setValue('');
    expect(ctrl.invalid).toBe(true);
  });

  it('form should be invalid when required fields are empty', () => {
    component.form.reset();
    expect(component.form.invalid).toBe(true);
  });

  it('should call vm.create with form values on submit', () => {
    component.form.setValue({ username: 'bob', email: 'bob@test.com', password: 'P@ssw0rd1', isAdmin: false });
    vi.spyOn(router, 'navigate').mockResolvedValue(true);

    component.submit();

    expect(mockVm.create).toHaveBeenCalledWith({
      username: 'bob',
      email: 'bob@test.com',
      password: 'P@ssw0rd1',
      isAdmin: false
    });
  });

  it('should not call vm.create when form is invalid', () => {
    component.form.reset();
    component.submit();
    expect(mockVm.create).not.toHaveBeenCalled();
  });

  it('cancel() should navigate to /users', () => {
    const nav = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    component.cancel();
    expect(nav).toHaveBeenCalledWith(['/users']);
  });
});

describe('UserFormComponent – edit mode', () => {
  let fixture: ComponentFixture<UserFormComponent>;
  let component: UserFormComponent;
  let mockVm: ReturnType<typeof makeMockVm>;
  let router: Router;

  beforeEach(async () => {
    mockVm = makeMockVm();

    await TestBed.configureTestingModule({
      imports: [UserFormComponent],
      providers: [
        { provide: UserService, useValue: mockVm },
        { provide: ActivatedRoute, useValue: makeActivatedRoute('aaa-111') },
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserFormComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('isEdit should be true when id param is present', () => {
    expect(component.isEdit()).toBe(true);
  });

  it('should pre-fill form with existing user data', () => {
    expect(component.form.value.username).toBe('alice');
    expect(component.form.value.email).toBe('alice@test.com');
    expect(component.form.value.isAdmin).toBe(true);
  });

  it('password should be optional in edit mode', () => {
    const ctrl = component.form.get('password')!;
    ctrl.setValue('');
    expect(ctrl.valid).toBe(true);
  });

  it('should call vm.update with userId on submit', () => {
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    component.form.patchValue({ username: 'alice2', email: 'alice2@test.com' });

    component.submit();

    expect(mockVm.update).toHaveBeenCalledWith('aaa-111', expect.objectContaining({ username: 'alice2' }));
  });

  it('should include password in update only when filled', () => {
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    component.form.patchValue({ password: 'NewP@ss99' });

    component.submit();

    const call = (mockVm.update as ReturnType<typeof vi.fn>).mock.calls[0][1];
    expect(call.password).toBe('NewP@ss99');
  });

  it('should NOT include password when left blank in edit mode', () => {
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    component.form.patchValue({ password: '' });

    component.submit();

    const call = (mockVm.update as ReturnType<typeof vi.fn>).mock.calls[0][1];
    expect(call.password).toBeUndefined();
  });
});

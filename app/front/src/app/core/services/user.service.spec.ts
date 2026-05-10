import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { UserService } from './user.service';
import { User } from '../models/user.model';
import { environment } from '../../../../environments/environment';

const BASE = `${environment.apiUrl}/users`;

const makeUser = (overrides: Partial<User> = {}): User => ({
  id: '00000000-0000-0000-0000-000000000001',
  username: 'alice',
  email: 'alice@test.com',
  isAdmin: false,
  creationDate: '2026-01-01T00:00:00Z',
  status: null,
  ...overrides
});

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [UserService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('loadAll()', () => {
    it('should populate users signal on success', () => {
      const users = [makeUser({ id: '1' }), makeUser({ id: '2', username: 'bob' })];

      service.loadAll();
      httpMock.expectOne(BASE).flush(users);

      expect(service.users()).toEqual(users);
      expect(service.loading()).toBe(false);
      expect(service.error()).toBeNull();
    });

    it('should set loading=true while request is in flight', () => {
      service.loadAll();
      expect(service.loading()).toBe(true);
      httpMock.expectOne(BASE).flush([]);
    });

    it('should set error signal on HTTP failure', () => {
      service.loadAll();
      httpMock.expectOne(BASE).flush('err', { status: 500, statusText: 'Server Error' });

      expect(service.error()).toBeTruthy();
      expect(service.loading()).toBe(false);
    });
  });

  describe('adminCount computed', () => {
    it('should count admin users reactively', () => {
      service.users.set([
        makeUser({ isAdmin: false }),
        makeUser({ id: '2', isAdmin: true }),
        makeUser({ id: '3', isAdmin: true })
      ]);
      expect(service.adminCount()).toBe(2);
    });

    it('should return 0 for empty list', () => {
      service.users.set([]);
      expect(service.adminCount()).toBe(0);
    });
  });

  describe('getById()', () => {
    it('should GET user by id', () => {
      const user = makeUser();
      let result: User | undefined;

      service.getById(user.id).subscribe(u => (result = u));
      httpMock.expectOne(`${BASE}/${user.id}`).flush(user);

      expect(result).toEqual(user);
    });
  });

  describe('create()', () => {
    it('should POST and prepend new user to list', () => {
      const existing = makeUser({ id: '1', username: 'existing' });
      service.users.set([existing]);

      const newUser = makeUser({ id: '2', username: 'new' });
      let result: User | undefined;

      service.create({ username: 'new', email: 'new@test.com', password: 'P@ss1234', isAdmin: false })
        .subscribe(u => (result = u));

      const req = httpMock.expectOne(BASE);
      expect(req.request.method).toBe('POST');
      req.flush(newUser);

      expect(result).toEqual(newUser);
      expect(service.users()[0]).toEqual(newUser);
      expect(service.users()).toHaveLength(2);
    });
  });

  describe('remove()', () => {
    it('should DELETE and remove user from list by id', () => {
      service.users.set([makeUser({ id: '1' }), makeUser({ id: '2', username: 'bob' })]);

      service.remove('1').subscribe();
      httpMock.expectOne(`${BASE}/1`).flush(null);

      expect(service.users()).toHaveLength(1);
      expect(service.users()[0].id).toBe('2');
    });
  });

  describe('update()', () => {
    it('should PUT then reload the full list', () => {
      service.update('1', { username: 'renamed' }).subscribe();

      const putReq = httpMock.expectOne(`${BASE}/1`);
      expect(putReq.request.method).toBe('PUT');
      putReq.flush(null);

      // update() calls loadAll() in its tap
      httpMock.expectOne(BASE).flush([]);
    });
  });
});

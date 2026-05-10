export interface User {
  id: string;
  username: string;
  email: string;
  isAdmin: boolean;
  creationDate: string;
  status: string | null;
}

export interface CreateUserRequest {
  username: string;
  email: string;
  password: string;
  isAdmin: boolean;
}

export interface UpdateUserRequest {
  username?: string;
  email?: string;
  password?: string;
  isAdmin?: boolean;
  status?: string;
}

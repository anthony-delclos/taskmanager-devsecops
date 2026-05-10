export interface Subject {
  id: string;
  name: string;
  description: string | null;
  status: string | null;
  priority: string | null;
  deadline: string | null;
  estimatedLoadHours: number | null;
  actualLoadHours: number | null;
  creationDate: string;
  editionDate: string | null;
  categoryId: string | null;
  categoryName: string | null;
  assignedUserId: string | null;
  assignedUserUsername: string | null;
  createdByUserId: string;
  createdByUsername: string | null;
}

export interface CreateSubjectRequest {
  name: string;
  description?: string;
  deadline?: string;
  priority?: string;
  estimatedLoadHours?: number;
  categoryId?: string;
  assignedUserId?: string;
  createdByUserId: string;
}

export interface UpdateSubjectRequest {
  name?: string;
  description?: string;
  deadline?: string;
  priority?: string;
  status?: string;
  estimatedLoadHours?: number;
  actualLoadHours?: number;
  categoryId?: string;
  assignedUserId?: string;
}

import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [RouterLink, DatePipe],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css'
})
export class UserListComponent implements OnInit {
  protected vm = inject(UserService);

  ngOnInit(): void {
    this.vm.loadAll();
  }

  delete(id: string): void {
    if (!confirm('Supprimer cet utilisateur ?')) return;
    this.vm.remove(id).subscribe();
  }
}

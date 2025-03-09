import { HttpClient } from '@angular/common/http';
import { Component, computed, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { dummyData } from 'src/app/shared/dummy/user.dummy';
import { TableActionComponent } from './components/table-action/table-action.component';
import { TableFooterComponent } from './components/table-footer/table-footer.component';
import { TableHeaderComponent } from './components/table-header/table-header.component';
import { TableRowComponent } from './components/table-row/table-row.component';
import { User } from './model/user.model';
import { TableFilterService } from './services/table-filter.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-table',
  imports: [
    AngularSvgIconModule,
    FormsModule,
    TableHeaderComponent,
    TableFooterComponent,
    TableRowComponent,
    TableActionComponent,
  ],
  templateUrl: './table.component.html',
  styleUrl: './table.component.css',
})
export class TableComponent implements OnInit {
  users = signal<User[]>([]);

  constructor(
    private http: HttpClient,
    private tableFilterService: TableFilterService,
    private toastr: ToastrService,
  ) {
    this.initializeData();
  }

  private initializeData(): void {
    this.http.get<User[]>('https://freetestapi.com/api/v1/users?limit=8').subscribe({
      next: (data) => this.users.set(data),
      error: (error) => {
        this.users.set(dummyData);
        this.handleRequestError(error);
      },
    });
  }

  public toggleUsers(checked: boolean) {
    this.users.update((users) => {
      return users.map((user) => {
        return { ...user, selected: checked };
      });
    });
  }

  private handleRequestError(error: any) {
    const msg = 'Error fetching users. Loading dummy data...';
    this.toastr.error(msg, 'Error', {
      timeOut: 5000,
      closeButton: true,
    });
  }

  filteredUsers = computed(() => {
    const search = this.tableFilterService.searchField().toLowerCase();
    const status = this.tableFilterService.statusField();
    const order = this.tableFilterService.orderField();

    return this.users()
      .filter(
        (user) =>
          user.name.toLowerCase().includes(search) ||
          user.username.toLowerCase().includes(search) ||
          user.email.toLowerCase().includes(search) ||
          user.phone.includes(search),
      )
      .filter((user) => {
        if (!status) return true;
        switch (status) {
          case '1':
            return user.status === 1;
          case '2':
            return user.status === 2;
          case '3':
            return user.status === 3;
          default:
            return true;
        }
      })
      .sort((a, b) => {
        const defaultNewest = !order || order === '1';
        if (defaultNewest) {
          return new Date(b.created_at).getTime() - new Date(a.created_at).getTime();
        } else if (order === '2') {
          return new Date(a.created_at).getTime() - new Date(b.created_at).getTime();
        }
        return 0;
      });
  });

  ngOnInit() {}

  retry() {
    console.log('Retrying operation...');
    this.loadData();
  }

  private loadData(): void {
    this.initializeData();
  }
}

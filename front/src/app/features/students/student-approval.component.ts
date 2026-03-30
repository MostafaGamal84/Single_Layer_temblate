import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { StudentGroupService } from '../../core/services/student-group.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ToastService } from '../../core/services/toast.service';
import { PagedResult } from '../../core/models';

@Component({
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="students-page">
      <div class="page-header">
        <div>
          <p class="eyebrow">Student Management</p>
          <h2>Students</h2>
          <p class="header-desc">Approve and manage student registrations</p>
        </div>
      </div>

      <div class="filters-bar">
        <div class="field search-field">
          <input type="text" [(ngModel)]="searchTerm" placeholder="Search students..." (keyup.enter)="loadStudents()" />
        </div>
        <div class="field">
          <select [(ngModel)]="groupFilter" (change)="loadStudents()">
            <option [ngValue]="null">All Groups</option>
            @for (group of groups; track group.id) {
              <option [ngValue]="group.id">{{ group.name }}</option>
            }
          </select>
        </div>
        <div class="field">
          <select [(ngModel)]="statusFilter" (change)="loadStudents()">
            <option [ngValue]="null">All Status</option>
            <option [ngValue]="0">Pending</option>
            <option [ngValue]="1">Active</option>
            <option [ngValue]="2">Rejected</option>
          </select>
        </div>
      </div>

      @if (loading) {
        <p class="loading-text">Loading students...</p>
      }

      @if (!loading && students.length === 0) {
        <div class="empty-state">
          <h4>No students found</h4>
          <p>Students will appear here when they register</p>
        </div>
      }

      @if (!loading && students.length > 0) {
        <div class="students-table">
          <table>
            <thead>
              <tr>
                <th>Student</th>
                <th>Email</th>
                <th>Groups</th>
                <th>Status</th>
                <th>Registered</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (student of students; track student.id) {
                <tr>
                  <td>
                    <div class="student-name">
                      <strong>{{ student.userName }}</strong>
                      <span class="student-id">#{{ student.id }}</span>
                    </div>
                  </td>
                  <td>{{ student.email }}</td>
                  <td>
                    <div class="groups-list">
                      @for (group of student.groups; track group) {
                        <span class="group-chip">{{ group }}</span>
                      }
                      @if (student.groups.length === 0) {
                        <span class="no-groups">No groups</span>
                      }
                    </div>
                  </td>
                  <td>
                    <span class="status-badge" [class]="'status-' + student.status">
                      {{ getStatusName(student.status) }}
                    </span>
                  </td>
                  <td>{{ student.registerTime | date:'short' }}</td>
                  <td>
                    <div class="action-buttons">
                      @if (student.status === 0) {
                        <button type="button" class="success-btn" (click)="approveStudent(student.id)">Approve</button>
                        <button type="button" class="danger" (click)="rejectStudent(student.id)">Reject</button>
                      }
                      @if (student.status === 1) {
                        <button type="button" class="secondary" (click)="suspendStudent(student.id)">Suspend</button>
                      }
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        @if (totalPages > 1) {
          <div class="pagination">
            <button type="button" class="secondary" [disabled]="page === 1" (click)="goToPage(page - 1)">Previous</button>
            <span>Page {{ page }} of {{ totalPages }}</span>
            <button type="button" class="secondary" [disabled]="page === totalPages" (click)="goToPage(page + 1)">Next</button>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .students-page { display: grid; gap: 16px; }

    .page-header {
      display: flex;
      justify-content: space-between;
      gap: 16px;
      align-items: flex-start;
      flex-wrap: wrap;
    }

    .page-header h2 { margin: 0; }
    .header-desc { margin: 4px 0 0; color: var(--muted); }

    .filters-bar { display: flex; gap: 12px; flex-wrap: wrap; }
    .search-field { flex: 1; min-width: 200px; }
    .field { display: grid; gap: 4px; }
    .field select, .field input {
      padding: 10px 12px;
      border: 1px solid var(--border);
      border-radius: 10px;
      background: var(--surface);
      min-height: 42px;
    }

    .loading-text { color: var(--muted); text-align: center; padding: 40px; }

    .empty-state {
      text-align: center;
      padding: 40px;
      border: 1px dashed var(--border-strong);
      border-radius: 18px;
      background: var(--surface-soft);
    }

    .students-table { overflow-x: auto; }

    table {
      width: 100%;
      border-collapse: collapse;
      background: var(--surface);
      border-radius: 12px;
      overflow: hidden;
    }

    th, td {
      padding: 12px 16px;
      text-align: left;
      border-bottom: 1px solid var(--border);
    }

    th {
      background: var(--surface-soft);
      font-size: 0.8rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--muted);
    }

    tr:last-child td { border-bottom: none; }
    tr:hover td { background: var(--surface-soft); }

    .student-name { display: flex; flex-direction: column; gap: 2px; }
    .student-name strong { display: flex; align-items: center; gap: 6px; }
    .student-id { 
      font-size: 0.75rem; 
      color: var(--muted); 
      font-family: monospace;
      background: var(--surface-soft);
      padding: 1px 5px;
      border-radius: 3px;
    }

    .groups-list { display: flex; flex-wrap: wrap; gap: 4px; }
    .group-chip {
      font-size: 0.75rem;
      padding: 2px 8px;
      border-radius: 4px;
      background: var(--info-tint);
      color: var(--info);
      border: 1px solid var(--info-border);
    }
    .no-groups { font-size: 0.8rem; color: var(--muted); }

    .status-badge {
      font-size: 0.75rem;
      padding: 4px 10px;
      border-radius: 6px;
      font-weight: 600;
    }

    .status-0 { background: var(--warning-tint); color: var(--warning); }
    .status-1 { background: var(--success-tint); color: var(--success); }
    .status-2 { background: var(--error-tint); color: var(--error); }

    .action-buttons { display: flex; gap: 6px; flex-wrap: wrap; }
    .success-btn {
      padding: 6px 12px;
      border: none;
      border-radius: 8px;
      background: var(--success);
      color: white;
      font-size: 0.8rem;
      cursor: pointer;
    }

    .pagination { display: flex; justify-content: center; align-items: center; gap: 12px; padding: 16px; }

    @media (max-width: 900px) {
      table { display: block; overflow-x: auto; }
    }
  `]
})
export class StudentApprovalComponent implements OnInit {
  students: any[] = [];
  groups: any[] = [];
  loading = false;
  searchTerm = '';
  groupFilter: number | null = null;
  statusFilter: number | null = null;
  page = 1;
  pageSize = 20;
  totalPages = 1;

  constructor(
    private service: StudentGroupService,
    private confirmDialog: ConfirmDialogService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadGroups();
    this.loadStudents();
  }

  loadGroups(): void {
    this.service.getAll({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (res: any) => {
        this.groups = res.items || [];
      }
    });
  }

  loadStudents(): void {
    this.loading = true;
    const params: any = {
      pageNumber: this.page,
      pageSize: this.pageSize,
      search: this.searchTerm
    };
    if (this.statusFilter !== null) params.status = this.statusFilter;
    if (this.groupFilter !== null) params.groupId = this.groupFilter;

    this.service.getStudents(params).subscribe({
      next: (res: PagedResult<any>) => {
        this.loading = false;
        this.students = res.items;
        this.totalPages = Math.ceil(res.totalCount / this.pageSize);
      },
      error: () => this.loading = false
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.page = page;
    this.loadStudents();
  }

  approveStudent(userId: number): void {
    this.service.approveStudent(userId, 1).subscribe({
      next: () => {
        this.updateStudentInList(userId, 1);
        this.toast.success('Student approved successfully');
      },
      error: () => this.loadStudents()
    });
  }

  async rejectStudent(userId: number): Promise<void> {
    const confirmed = await this.confirmDialog.open({
      title: 'Reject Student',
      message: 'Are you sure you want to reject this student?',
      confirmText: 'Reject',
      tone: 'danger'
    });
    if (confirmed) {
      this.service.approveStudent(userId, 2).subscribe({
        next: () => {
          this.updateStudentInList(userId, 2);
          this.toast.success('Student rejected');
        },
        error: () => this.loadStudents()
      });
    }
  }

  async suspendStudent(userId: number): Promise<void> {
    const confirmed = await this.confirmDialog.open({
      title: 'Suspend Student',
      message: 'Are you sure you want to suspend this student?',
      confirmText: 'Suspend',
      tone: 'danger'
    });
    if (confirmed) {
      this.service.approveStudent(userId, 0).subscribe({
        next: () => {
          this.updateStudentInList(userId, 0);
          this.toast.success('Student suspended');
        },
        error: () => this.loadStudents()
      });
    }
  }

  private updateStudentInList(userId: number, newStatus: number): void {
    const student = this.students.find(s => s.id === userId);
    if (student) {
      student.status = newStatus;
    }
  }

  getStatusName(status: number): string {
    const names: Record<number, string> = { 0: 'Pending', 1: 'Active', 2: 'Rejected' };
    return names[status] || 'Unknown';
  }
}
